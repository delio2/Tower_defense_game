using System.Collections.Generic;
using TowerDefense.Presentation.Settings;
using TowerDefense.Presentation.UI;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Presentation.Services
{
    /// <summary>
    /// Turns what happens in a run into the numbers the Gates ask for (docs/06 Gates 1, 2, 2.5; docs/13): real
    /// seconds per wave and per shop, the longest stretch without a touch in each shop ("nobody stuck more than
    /// 10 s"), run length, what ended it, and the first-run funnel of docs/07 §3 (first tap, kill, shop, combo,
    /// wave 2, act 1). It only reads the simulation's events and the pointer; it never changes anything.
    /// </summary>
    internal sealed class SessionRecorder
    {
        private readonly IAnalytics _analytics;
        private readonly bool _bot;

        private float _phaseStart;
        private float _runStart;
        private float _lastTouch;
        private float _longestIdle;
        private float _pausedSeconds;
        private long _waveStartIntegrity;
        private int _waveKills;
        private int _wavePulses;
        private int _buys, _merges, _sells, _moves, _rerolls, _undos, _rejected;
        private bool _inShop;
        private bool _inWave;
        private bool _runOpen;
        private bool _runEnded;
        private int _runsThisSession;

        public SessionRecorder(IAnalytics analytics, bool bot)
        {
            _analytics = analytics;
            _bot = bot;
            _analytics.Track("session_start",
                ("version", Application.version),
                ("balance", RunConfig.BalanceVersion),
                ("device", SystemInfo.deviceModel),
                ("ram_mb", SystemInfo.systemMemorySize),
                ("screen", $"{Screen.width}x{Screen.height}"),
                ("language", Loc.Language.ToString()),
                ("quality", GraphicsQuality.LevelName(GraphicsQuality.Level)),
                ("bot", bot));
        }

        public void RunStarted(GameSimulation sim, bool resumed)
        {
            float now = Time.unscaledTime;
            _runStart = now;
            _runOpen = true;
            _runEnded = false;
            _inShop = _inWave = false;
            _runsThisSession++;
            _analytics.Track("run_start", ("seed", (long)sim.Config.Seed), ("core", sim.Config.Core.ToString()),
                ("grade", sim.Config.Grade), ("mode", sim.Config.Mode.ToString()), ("resumed", resumed),
                ("run_in_session", _runsThisSession), ("wave", sim.CurrentWave));
            if (sim.Phase == GamePhase.Shop)
            {
                OpenShop(sim, now);
            }
        }

        /// <summary>Called once per frame after the simulation's events were drained.</summary>
        public void Observe(GameSimulation sim, List<SimEvent> events, bool paused)
        {
            float now = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;
            if (paused)
            {
                _pausedSeconds += dt;
            }

            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                Touched(now);
            }
            else if (_inShop && !paused)
            {
                _longestIdle = Mathf.Max(_longestIdle, now - _lastTouch);
            }

            foreach (SimEvent e in events)
            {
                switch (e.Type)
                {
                    case SimEventType.WaveStarted:
                        CloseShop(sim, now);
                        _inWave = true;
                        _phaseStart = now;
                        _pausedSeconds = 0f;
                        _waveStartIntegrity = sim.Integrity;
                        _waveKills = _wavePulses = 0;
                        _analytics.Track("wave_start", ("wave", sim.CurrentWave));
                        break;
                    case SimEventType.EnemyKilled:
                        _waveKills++;
                        Funnel("first_kill");
                        break;
                    case SimEventType.PulseUsed:
                        _wavePulses++;
                        break;
                    case SimEventType.WaveCleared:
                        EndWave(sim, now, (int)e.Value);
                        if (e.Value == 1)
                        {
                            Funnel("wave_2");
                        }

                        if (e.Value == 6)
                        {
                            Funnel("act_1_clear");
                        }

                        break;
                    case SimEventType.ShopOpened:
                        OpenShop(sim, now);
                        break;
                    case SimEventType.ModuleBought:
                        _buys++;
                        CheckCombo(sim);
                        break;
                    case SimEventType.ModuleMerged:
                        _merges++;
                        break;
                    case SimEventType.ModuleSold:
                        _sells++;
                        break;
                    case SimEventType.ModulesSwapped:
                        _moves++;
                        CheckCombo(sim);
                        break;
                    case SimEventType.Rerolled:
                        _rerolls++;
                        break;
                    case SimEventType.Undone:
                        _undos++;
                        break;
                    case SimEventType.CommandRejected:
                        _rejected++;
                        break;
                }
            }

            if (sim.IsOver && _runOpen && !_runEnded)
            {
                if (_inWave)
                {
                    EndWave(sim, now, sim.WavesCleared + 1);
                }

                _runEnded = true;
                _analytics.Track("run_end", ("result", sim.HasWon ? "victory" : "defeat"), ("waves", sim.WavesCleared),
                    ("minutes", (now - _runStart) / 60f), ("sim_minutes", sim.Tick / (float)SimConstants.TicksPerSecond / 60f),
                    ("defeated_by", sim.DefeatedBy?.ToString()), ("kills", sim.Kills));
            }
        }

        /// <summary>The player abandoned the run or chose "Play again" before the end: close it honestly.</summary>
        public void RunAbandoned(GameSimulation sim)
        {
            if (_runOpen && !_runEnded)
            {
                _analytics.Track("run_abandon", ("wave", sim.CurrentWave), ("minutes", (Time.unscaledTime - _runStart) / 60f));
            }

            _runOpen = false;
        }

        private void Touched(float now)
        {
            if (_inShop)
            {
                _longestIdle = Mathf.Max(_longestIdle, now - _lastTouch);
            }

            _lastTouch = now;
            Funnel("first_tap");
        }

        private void OpenShop(GameSimulation sim, float now)
        {
            if (_inShop)
            {
                return;
            }

            _inShop = true;
            _phaseStart = now;
            _lastTouch = now;
            _longestIdle = 0f;
            _pausedSeconds = 0f;
            _buys = _merges = _sells = _moves = _rerolls = _undos = _rejected = 0;
            _analytics.Track("shop_open", ("wave", sim.CurrentWave), ("credits", sim.Credits));
            Funnel("first_shop");
        }

        private void CloseShop(GameSimulation sim, float now)
        {
            if (!_inShop)
            {
                return;
            }

            _inShop = false;
            _analytics.Track("shop_close", ("wave", sim.CurrentWave), ("seconds", now - _phaseStart - _pausedSeconds),
                ("idle_max", _longestIdle), ("buys", _buys), ("merges", _merges), ("sells", _sells), ("moves", _moves),
                ("rerolls", _rerolls), ("undos", _undos), ("rejected", _rejected), ("credits_left", sim.Credits));
        }

        private void EndWave(GameSimulation sim, float now, int wave)
        {
            if (!_inWave)
            {
                return;
            }

            _inWave = false;
            _analytics.Track("wave_end", ("wave", wave), ("seconds", now - _phaseStart - _pausedSeconds),
                ("integrity_lost", (_waveStartIntegrity - sim.Integrity) / SimConstants.HpScale), ("kills", _waveKills),
                ("pulses", _wavePulses), ("guardian", sim.IsGuardianWave));
        }

        /// <summary>First-run funnel (docs/07 §3): each step is reported once per install.</summary>
        private void Funnel(string step)
        {
            if (_bot)
            {
                return;
            }

            string key = "funnel." + step;
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                return;
            }

            PlayerPrefs.SetInt(key, 1);
            _analytics.Track("funnel", ("step", step), ("seconds_since_install_session", Time.realtimeSinceStartup));
        }

        /// <summary>The first combo: a booster sitting next to a weapon (docs/07 §1.5, the first "aha").</summary>
        private void CheckCombo(GameSimulation sim)
        {
            Ring ring = sim.Ring;
            for (int slot = 0; slot < ring.SlotCount; slot++)
            {
                ModuleInstance module = ring.At(slot);
                if (module == null || module.Category != ModuleCategory.Booster)
                {
                    continue;
                }

                ModuleInstance left = ring.At(ring.LeftOf(slot));
                ModuleInstance right = ring.At(ring.RightOf(slot));
                if ((left != null && left.Category == ModuleCategory.Weapon) || (right != null && right.Category == ModuleCategory.Weapon))
                {
                    Funnel("first_combo");
                    return;
                }
            }
        }
    }
}
