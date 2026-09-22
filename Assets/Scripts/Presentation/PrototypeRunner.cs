using System.Collections.Generic;
using TowerDefense.Presentation.Arena;
using TowerDefense.Presentation.Content;
using TowerDefense.Presentation.Interaction;
using TowerDefense.Presentation.Settings;
using TowerDefense.Presentation.UI;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation
{
    /// <summary>
    /// Prototype driver (GDD §19): owns the deterministic simulation and the run lifecycle, advances ticks, routes
    /// simulation events to the views, and wires the HUD. Drawing lives in <c>Arena/*</c>, pointer input in
    /// <see cref="ShopInput"/>, strings in <see cref="UiText"/>. Visuals only read the simulation; game state changes
    /// exclusively through commands. Visual rule (docs/03 §A1): no flashing, eased motion.
    /// </summary>
    public sealed class PrototypeRunner : MonoBehaviour
    {
        private const int MaxStepsPerFrame = 240;

        /// <summary>Clutter control (docs/03 A8): cap on live line effects, attenuation above this many enemies.</summary>
        private const int MaxLineEffects = 32;
        private const int AttenuateAboveEnemies = 20;

        [SerializeField] private int _seed = 1;

        private GameSimulation _sim;
        private Material _unlit;
        private Material _lineMaterial;
        private ArenaKit _kit;
        private CameraRig _cameraRig;
        private readonly CoreView _core = new CoreView();
        private readonly RingView _ring = new RingView();
        private readonly EnemyViews _enemies = new EnemyViews();
        private readonly EffectsView _effects = new EffectsView();
        private ShopInput _input;
        private HudView _hud;

        private readonly List<SimEvent> _events = new List<SimEvent>();
        private readonly List<FloatingLabel> _floatingLabels = new List<FloatingLabel>();

        private double _tickAccumulator;
        private int _speed = 1;
        private bool _paused;
        private long _damageAtWaveStart;
        private int _creditsAtWaveStart;
        private string _replayStatus;

        /// <summary>The first run of the session resumes a saved run if there is one; "Play again" starts fresh.</summary>
        private bool _resumeOnStart = true;

        // ------------------------------------------------------------------ lifecycle

        private void Awake() => EnsureInitialized();

        /// <summary>Also rebuilds after a script recompile in Play mode, when non-serialized state is lost.</summary>
        private void EnsureInitialized()
        {
            if (_sim != null && _cameraRig != null && _unlit != null)
            {
                return;
            }

            Application.targetFrameRate = 60;
#if UNITY_EDITOR
            Application.runInBackground = true;
#endif
            _unlit = LoadMaterial("Materials/PrototypeUnlit", "Universal Render Pipeline/Unlit");
            _lineMaterial = LoadMaterial("Materials/PrototypeLines", "Sprites/Default");
            _kit = new ArenaKit(_unlit, _lineMaterial);
            _cameraRig = new CameraRig();
            _input = new ShopInput(_kit, _cameraRig, () => _hud, ShowMessage, TryPulse);
            SetupHud();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            StartRun();
        }

        /// <summary>
        /// Material assets in Resources keep their shaders in player builds (Shader.Find returns null on the device for
        /// shaders no asset references); the editor fallback covers a missing asset.
        /// </summary>
        private static Material LoadMaterial(string resourcePath, string fallbackShader)
        {
            var template = Resources.Load<Material>(resourcePath);
            if (template != null)
            {
                return new Material(template);
            }

            Debug.LogWarning($"Material {resourcePath} missing in Resources: falling back to Shader.Find({fallbackShader}).");
            Shader shader = Shader.Find(fallbackShader) ?? Shader.Find("Unlit/Color");
            return new Material(shader);
        }

        private void OnDestroy()
        {
            Destroy(_unlit);
            Destroy(_lineMaterial);
        }

        private void StartRun()
        {
            _enemies.Clear();
            _effects.Clear();
            _events.Clear();
            _tickAccumulator = 0;
            _speed = PlayerOptions.DefaultSpeed;
            _paused = false;
            _input.Reset();
            _replayStatus = null;
            _cameraRig.ResetView();

            ContentDatabase content = ContentLoader.LoadPrototypeOrDefaults();
            _sim = _resumeOnStart ? RunSave.TryResume(content) : null;
            _resumeOnStart = false;
            if (_sim != null)
            {
                _seed = (int)_sim.Config.Seed;
                ShowMessage($"Resumed at wave {_sim.CurrentWave}");
            }
            else
            {
                RunMode mode = PlayerOptions.Endless ? RunMode.Endless : RunMode.Run;
                _sim = new GameSimulation(RunSetup.Create((ulong)_seed, PlayerOptions.Core, PlayerOptions.Grade, mode), content);
            }

            _kit.Sim = _sim;
            _kit.ResetRoot(transform);
            _ring.Build(_kit);
            _core.Build(_kit);
        }

        private void Update()
        {
            EnsureInitialized();
            float deltaTime = Time.deltaTime;
            _cameraRig.UpdateLayout(_sim.Phase, deltaTime);
            _input.HandlePointer();
            _sim.ApplyPendingCommandsNow();
            AdvanceSimulation(deltaTime);
            ProcessEvents();
            _ring.Sync(_kit, _input.IsHighlighted, deltaTime);
            _enemies.Sync(_kit);
            _effects.Update(deltaTime);
            _core.Update(_kit, deltaTime);
            CheckGameOver();
            SyncHud();
        }

        // ------------------------------------------------------------------ HUD (UI Toolkit, docs/09)

        private void SetupHud()
        {
            var document = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            document.panelSettings = Resources.Load<PanelSettings>("PanelSettings");
            document.visualTreeAsset = Resources.Load<VisualTreeAsset>("Hud");
            if (document.panelSettings == null || document.visualTreeAsset == null)
            {
                Debug.LogError("HUD assets missing in Assets/UI/Resources (PanelSettings, Hud.uxml).");
                return;
            }

            _hud = new HudView(document.rootVisualElement);
            _hud.RerollTapped += () => { _sim.Enqueue(Command.Reroll()); _input.DeselectOffer(); };
            _hud.NextWaveTapped += () => { _sim.Enqueue(Command.StartWave()); _input.ClearSelection(); Haptics.Medium(); };
            _hud.UndoTapped += () => { _sim.Enqueue(Command.Undo()); _input.ClearSelection(); };
            _hud.BuySlotTapped += () => _sim.Enqueue(Command.BuySlot(_sim.Ring.SlotCount));
            _hud.SellTapped += () =>
            {
                ModuleInstance module = _sim.Ring.At(_input.SelectedSlot);
                if (module != null)
                {
                    _sim.Enqueue(Command.Sell(module.Slot));
                }

                _input.ClearSelection();
            };
            _hud.MoveTapped += _input.ToggleMoveMode;
            _hud.CloseTapped += _input.ClearSelection;
            _hud.PulseTapped += TryPulse;
            _hud.SpeedTapped += () => _speed = _speed % 3 + 1;
            _hud.PauseTapped += () => _paused = !_paused;
            _hud.PlayAgainTapped += () => { _seed++; RunSave.Clear(); StartRun(); };
        }

        private void SyncHud()
        {
            if (_hud == null)
            {
                return;
            }

            _effects.FillLabels(_hud, _cameraRig.Camera, _floatingLabels);
            _hud.SetNumbers(_floatingLabels);
            _hud.Refresh(_sim, new HudState
            {
                SelectedOffer = _input.SelectedOffer,
                SelectedSlot = _input.SelectedSlot,
                MoveMode = _input.MoveMode,
                Speed = _speed,
                Paused = _paused,
                ReplayStatus = _replayStatus,
                Seed = _seed,
                PreviewText = _input.PreviewText,
                DragSourceOffer = _input.DragSourceOffer,
                ShowSellZone = _input.IsDraggingModule,
                SellZoneHot = _input.SellZoneHot,
            }, UiText.Describe);
        }

        // ------------------------------------------------------------------ simulation

        private void AdvanceSimulation(float deltaTime)
        {
            if (_paused || _sim.Phase != GamePhase.Wave)
            {
                _tickAccumulator = 0;
                return;
            }

            _tickAccumulator += deltaTime * SimConstants.TicksPerSecond * _speed;
            int steps = 0;
            while (_tickAccumulator >= 1.0 && steps < MaxStepsPerFrame && _sim.Phase == GamePhase.Wave)
            {
                _sim.Step();
                _tickAccumulator -= 1.0;
                steps++;
            }
        }

        private void ProcessEvents()
        {
            _events.Clear();
            _sim.DrainEvents(_events);
            foreach (SimEvent e in _events)
            {
                switch (e.Type)
                {
                    case SimEventType.EnemyHit:
                        OnEnemyHit(e);
                        break;
                    case SimEventType.EnemyKilled:
                        if (ShowsNumber(e) && _enemies.TryGetPosition(e.EntityId, out Vector3 killedAt))
                        {
                            _effects.AddNumber(killedAt, UiText.FormatDamage(e.Value), e.Extra == 0);
                        }

                        break;
                    case SimEventType.CoreHit:
                        _core.OnHit();
                        break;
                    case SimEventType.PulseUsed:
                        _effects.SpawnRing(_kit, _core.Position, 0.8f, _sim.Config.PulseRadius / (float)SimConstants.Micro,
                            Palette.WithAlpha(Palette.Core, 0.5f), 0.6f, 0.05f);
                        break;
                    case SimEventType.ModuleMerged:
                        OnModuleMerged(e);
                        break;
                    case SimEventType.WaveStarted:
                        _damageAtWaveStart = _sim.TotalDamage;
                        _creditsAtWaveStart = _sim.Credits;
                        break;
                    case SimEventType.WaveCleared:
                        // Credits were just paid: base + interest (+ Guardian bonus); the summary counts them up.
                        if (!_sim.IsOver)
                        {
                            _hud?.ShowWaveSummary((int)e.Value, _sim.TotalDamage - _damageAtWaveStart, _sim.Credits, _sim.Credits - _creditsAtWaveStart, e.Extra);
                        }

                        break;
                    case SimEventType.CommandRejected:
                        ShowMessage(UiText.DescribeRejection((CommandResult)e.Extra));
                        break;
                    case SimEventType.ShopOpened:
                        RunSave.Save(_sim); // autosave at every shop (GDD §2)
                        break;
                }
            }
        }

        /// <summary>Merge: the module swells softly and a faint ring expands from it (the "satisfying" moment, docs/03 B4).</summary>
        private void OnModuleMerged(SimEvent e)
        {
            if (_ring.TryMarkMerged(e.EntityId, out Vector3 position))
            {
                _effects.SpawnRing(_kit, position, 0.3f, 1.1f, Palette.WithAlpha(Palette.Booster, 0.45f), 0.45f, 0.04f);
            }

            Haptics.Success();
            ShowMessage($"Level {e.Value}");
        }

        private static bool ShowsNumber(SimEvent kill)
        {
            switch (PlayerOptions.DamageNumbers)
            {
                case DamageNumbersMode.None:
                    return false;
                case DamageNumbersMode.BigOnly:
                    return kill.Extra == 0 || kill.Value >= 20 * SimConstants.HpScale; // Pulse kills and heavy hits
                default:
                    return true;
            }
        }

        private void OnEnemyHit(SimEvent e)
        {
            if (e.Extra == 0 || !_enemies.TryGetPosition(e.EntityId, out Vector3 enemyAt))
            {
                return;
            }

            // Clutter control: a cap on simultaneous tracers, and fainter tracers when the arena is crowded.
            if (_effects.ActiveLines >= Mathf.RoundToInt(MaxLineEffects * PlayerOptions.EffectScale))
            {
                return;
            }

            ModuleInstance module = _kit.FindModule(e.Extra);
            if (module == null)
            {
                return;
            }

            // Soft, low-alpha tracer that fades over 0.25 s (calm; at most ~2 per second per module).
            Vector3 from = _kit.SlotWorld(module.Slot) + Vector3.up * 0.3f;
            float crowd = Mathf.Min(1f, AttenuateAboveEnemies / (float)Mathf.Max(1, _sim.Enemies.Count));
            _effects.SpawnBeam(_kit, from, enemyAt, Palette.WithAlpha(Palette.Weapon, 0.35f * crowd * PlayerOptions.EffectScale), 0.04f);
        }

        private void CheckGameOver()
        {
            if (!_sim.IsOver || _replayStatus != null)
            {
                return;
            }

            RunSave.Clear();

            // Every finished run is recorded and re-simulated: the same check the server will do (D19).
            string text = Replay.Record(_sim).Serialize();
            ReplayCheck check = ReplayVerifier.Verify(Replay.Deserialize(text), ContentLoader.LoadPrototypeOrDefaults(),
                () => new RunConfig());
            _replayStatus = check.IsValid
                ? $"Replay verified ({_sim.CommandLog.Count} commands, {text.Length} bytes)"
                : $"Replay MISMATCH: {check.Reason}";
        }

        private void TryPulse()
        {
            CommandResult result = _sim.Validate(Command.Pulse());
            if (result == CommandResult.Ok)
            {
                _sim.Enqueue(Command.Pulse());
                Haptics.Medium();
            }
            else
            {
                ShowMessage(UiText.DescribeRejection(result));
            }
        }

        private void ShowMessage(string text) => _hud?.ShowToast(text);
    }
}
