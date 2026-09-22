using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TowerDefense.Presentation.Settings;
using TowerDefense.Simulation;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace TowerDefense.Presentation.Diagnostics
{
    /// <summary>
    /// Autoplay benchmark (docs/06 §4): a balance bot plays a whole run through the real game — models, effects, HUD —
    /// while this class measures every wave: frame times (average, 95th and 99th percentile, worst), enemies alive,
    /// draw calls, SetPass calls, triangles and memory. One log line per wave, tagged <c>[BENCH]</c>, so a host script
    /// reads them from logcat (Tools/bench/run_emulators.py). Nothing is saved and the player's options are untouched.
    ///
    /// Started by an Android intent extra (<c>am start … -e benchmark "strategy=MaxDps;speed=2;quality=Low"</c>), a
    /// desktop command-line argument (<c>-benchmark strategy=…</c>) or, in the editor, <see cref="RequestInEditor"/>.
    /// Settings: strategy (a <see cref="BotStrategy"/>), speed 1–3, quality Auto/Low/Medium/High, seed, quit (1 = close
    /// the app at the end).
    /// </summary>
    internal sealed class AutoplayBenchmark
    {
        public const string Tag = "[BENCH]";
        private const string EditorRequestKey = "bench.request";

        /// <summary>Real seconds the shop stays on screen before the bot plays it (so a host can take a screenshot).</summary>
        private const float ShopDwellSeconds = 4f;

        private readonly BotStrategy _strategy;
        private readonly bool _quitAtEnd;
        private readonly List<float> _frameMs = new List<float>(8192);
        private readonly List<float> _runFrameMs = new List<float>(65536);
        private ProfilerRecorder _drawCalls;
        private ProfilerRecorder _setPassCalls;
        private ProfilerRecorder _triangles;
        private float _shopTimer;

        /// <summary>
        /// Screenshots the game takes itself, at the exact moment (a host screenshot over adb arrives seconds late):
        /// the shop at these waves once the summary has gone, and these Guardian waves a few seconds in.
        /// Saved to persistentDataPath/bench; Tools/bench/run_emulators.py pulls them.
        /// </summary>
        private static readonly int[] ShopShots = { 1, 7, 13 };
        private static readonly int[] WaveShots = { 6, 12, 18 };
        private const float ShopShotAfter = 2.8f;
        private const float WaveShotAfter = 6f;
        private float _waveClock;
        private int _lastShot = -1;
        private int _shopPlayedForWave = -1;
        private int _enemiesMax;
        private long _drawCallsMax, _setPassMax, _trianglesMax;
        private bool _inWave;
        private bool _finished;
        private float _quitTimer = -1f;

        private AutoplayBenchmark(BotStrategy strategy, int speed, int quality, ulong seed, bool quitAtEnd)
        {
            _strategy = strategy;
            Speed = speed;
            QualityLevel = quality;
            Seed = seed;
            _quitAtEnd = quitAtEnd;
        }

        public int Speed { get; }
        public int QualityLevel { get; }
        public ulong Seed { get; }

        /// <summary>Editor only: the next Play session runs the benchmark with these settings (one-shot).</summary>
        public static void RequestInEditor(string settings)
        {
            PlayerPrefs.SetString(EditorRequestKey, settings);
            PlayerPrefs.Save();
        }

        /// <summary>Reads the request from the intent, the command line or the editor, whichever is present.</summary>
        public static AutoplayBenchmark TryCreate()
        {
            string settings = ReadRequest();
            if (settings == null)
            {
                return null;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string pair in settings.Split(';', ','))
            {
                string[] kv = pair.Split('=');
                if (kv.Length == 2)
                {
                    values[kv[0].Trim()] = kv[1].Trim();
                }
            }

            BotStrategy strategy = values.TryGetValue("strategy", out string s) && Enum.TryParse(s, true, out BotStrategy parsed)
                ? parsed
                : BotStrategy.MaxDps;
            int speed = values.TryGetValue("speed", out string sp) && int.TryParse(sp, out int v) ? Mathf.Clamp(v, 1, 3) : 2;
            QualityChoice choice = values.TryGetValue("quality", out string q) && Enum.TryParse(q, true, out QualityChoice c)
                ? c
                : QualityChoice.Auto;
            ulong seed = values.TryGetValue("seed", out string sd) && ulong.TryParse(sd, out ulong n) ? n : 1UL;
            bool quit = !values.TryGetValue("quit", out string qt) || qt != "0";
            return new AutoplayBenchmark(strategy, speed, GraphicsQuality.Resolve(choice), seed, quit);
        }

        private static string ReadRequest()
        {
#if UNITY_EDITOR
            if (PlayerPrefs.HasKey(EditorRequestKey))
            {
                string request = PlayerPrefs.GetString(EditorRequestKey);
                PlayerPrefs.DeleteKey(EditorRequestKey);
                return request;
            }
#elif UNITY_ANDROID
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
                string extra = intent.Call<string>("getStringExtra", "benchmark");
                if (!string.IsNullOrEmpty(extra))
                {
                    return extra;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{Tag} could not read the intent: {e.Message}");
            }
#endif
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-benchmark")
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        public void Begin(GameSimulation sim)
        {
            _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            _setPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            _triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            try
            {
                string folder = ShotFolder;
                if (Directory.Exists(folder))
                {
                    foreach (string old in Directory.GetFiles(folder, "*.png"))
                    {
                        File.Delete(old);
                    }
                }

                Directory.CreateDirectory(folder);
            }
            catch (IOException e)
            {
                Debug.LogWarning($"{Tag} screenshots disabled: {e.Message}");
            }

            Log(string.Format(CultureInfo.InvariantCulture,
                "start device=\"{0}\" gpu=\"{1}\" ram_mb={2} screen={3}x{4} dpi={5:F0} quality={6} strategy={7} speed={8} seed={9} balance={10}",
                SystemInfo.deviceModel, SystemInfo.graphicsDeviceName, SystemInfo.systemMemorySize, Screen.width, Screen.height,
                Screen.dpi, GraphicsQuality.LevelName(QualityLevel), _strategy, Speed, Seed, RunConfig.BalanceVersion));
        }

        /// <summary>Called once per frame by the runner, before the simulation advances.</summary>
        public void Tick(GameSimulation sim, float unscaledDeltaTime)
        {
            if (_finished)
            {
                if (_quitAtEnd && _quitTimer > 0f && (_quitTimer -= unscaledDeltaTime) <= 0f)
                {
                    Application.Quit();
                }

                return;
            }

            if (sim.Phase == GamePhase.Wave)
            {
                if (!_inWave)
                {
                    _inWave = true;
                    _frameMs.Clear();
                    _enemiesMax = 0;
                    _drawCallsMax = _setPassMax = _trianglesMax = 0;
                    Log($"wave-start wave={sim.CurrentWave}");
                    _waveClock = 0f;
                }

                _waveClock += unscaledDeltaTime;
                if (_waveClock >= WaveShotAfter && Array.IndexOf(WaveShots, sim.CurrentWave) >= 0)
                {
                    Shoot($"wave-{sim.CurrentWave}", 100 + sim.CurrentWave);
                }

                Sample(sim, unscaledDeltaTime);
                return;
            }

            if (_inWave)
            {
                _inWave = false;
                LogWave(sim);
            }

            if (sim.IsOver)
            {
                Finish(sim);
                return;
            }

            if (sim.Phase == GamePhase.Shop && _shopPlayedForWave != sim.CurrentWave)
            {
                if (_shopTimer <= 0f)
                {
                    _shopTimer = ShopDwellSeconds;
                    Log($"shop wave={sim.CurrentWave} credits={sim.Credits}");
                }

                _shopTimer -= unscaledDeltaTime;
                if (ShopDwellSeconds - _shopTimer >= ShopShotAfter && Array.IndexOf(ShopShots, sim.CurrentWave) >= 0)
                {
                    Shoot($"shop-{sim.CurrentWave}", sim.CurrentWave);
                }

                if (_shopTimer <= 0f)
                {
                    _shopPlayedForWave = sim.CurrentWave;
                    BalanceBot.PlayShop(sim, _strategy);
                    sim.Enqueue(Command.StartWave());
                }
            }
        }

        /// <summary>
        /// Called by the runner before every simulation tick, exactly where the farm's bot decides (BalanceBot.Play):
        /// the Pulse lands on the same tick whatever the frame rate, so a benchmark run ends with the same state hash
        /// on every device, emulator and in Tools/BotFarm — a cross-platform determinism check for free.
        /// </summary>
        public void BeforeStep(GameSimulation sim)
        {
            if (!_finished && BalanceBot.WantsPulse(sim, _strategy))
            {
                sim.Enqueue(Command.Pulse());
            }
        }

        private void Sample(GameSimulation sim, float unscaledDeltaTime)
        {
            float ms = unscaledDeltaTime * 1000f;
            _frameMs.Add(ms);
            _runFrameMs.Add(ms);
            int alive = 0;
            IReadOnlyList<Enemy> enemies = sim.Enemies;
            for (int i = 0; i < enemies.Count; i++) // indexed: no enumerator allocation per frame
            {
                if (enemies[i].IsAlive)
                {
                    alive++;
                }
            }

            _enemiesMax = Math.Max(_enemiesMax, alive);
            _drawCallsMax = Math.Max(_drawCallsMax, _drawCalls.LastValue);
            _setPassMax = Math.Max(_setPassMax, _setPassCalls.LastValue);
            _trianglesMax = Math.Max(_trianglesMax, _triangles.LastValue);
        }

        private void LogWave(GameSimulation sim)
        {
            var sb = new StringBuilder();
            sb.Append("wave=").Append(sim.WavesCleared + (sim.Phase == GamePhase.Defeat ? 1 : 0));
            AppendFrameStats(sb, _frameMs);
            sb.Append(" enemies_max=").Append(_enemiesMax)
              .Append(" draws_max=").Append(_drawCallsMax)
              .Append(" setpass_max=").Append(_setPassMax)
              .Append(" tris_max=").Append(_trianglesMax)
              .Append(" mem_mb=").Append(Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024))
              .Append(" integrity=").Append(sim.Integrity / SimConstants.HpScale);
            Log(sb.ToString());
        }

        private void Finish(GameSimulation sim)
        {
            _finished = true;
            var sb = new StringBuilder("end result=").Append(sim.HasWon ? "victory" : "defeat")
                .Append(" waves=").Append(sim.WavesCleared)
                .Append(" hash=").Append(sim.ComputeStateHash());
            AppendFrameStats(sb, _runFrameMs);
            sb.Append(" mem_mb=").Append(Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024));
            Log(sb.ToString());
            _drawCalls.Dispose();
            _setPassCalls.Dispose();
            _triangles.Dispose();
            _quitTimer = 3f;
        }

        /// <summary>Frame count, average fps and frame-time percentiles; "slow" = frames over 33.4 ms (below 30 fps).</summary>
        private static void AppendFrameStats(StringBuilder sb, List<float> frames)
        {
            if (frames.Count == 0)
            {
                sb.Append(" frames=0");
                return;
            }

            var sorted = new List<float>(frames);
            sorted.Sort();
            double total = 0;
            int slow = 0;
            foreach (float ms in sorted)
            {
                total += ms;
                if (ms > 33.4f)
                {
                    slow++;
                }
            }

            float Percentile(float p) => sorted[Math.Min(sorted.Count - 1, (int)(p * sorted.Count))];
            sb.Append(string.Format(CultureInfo.InvariantCulture,
                " frames={0} fps_avg={1:F1} ms_p50={2:F1} ms_p95={3:F1} ms_p99={4:F1} ms_max={5:F1} slow_pct={6:F1}",
                sorted.Count, 1000.0 * sorted.Count / total, Percentile(0.5f), Percentile(0.95f), Percentile(0.99f),
                sorted[sorted.Count - 1], 100.0 * slow / sorted.Count));
        }

        private static string ShotFolder => Path.Combine(Application.persistentDataPath, "bench");

        /// <summary>One screenshot per moment (<paramref name="id"/> keeps it from repeating every frame).</summary>
        private void Shoot(string name, int id)
        {
            if (_lastShot == id)
            {
                return;
            }

            _lastShot = id;
            ScreenCapture.CaptureScreenshot(Path.Combine(ShotFolder, name + ".png"));
            Log($"shot {name}");
        }

        private static void Log(string message) => Debug.Log($"{Tag} {message}");
    }
}
