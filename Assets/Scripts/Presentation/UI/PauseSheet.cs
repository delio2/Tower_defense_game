using System;
using TowerDefense.Presentation.Settings;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// Pause v2 as a sheet (docs/06 2.5-D3, docs/09 §2.8): what this run is, the four options worth changing mid-run,
    /// Resume, and hold-to-abandon — a tap can never lose a run.
    /// </summary>
    internal sealed class PauseSheet
    {
        /// <summary>How long "abandon" has to be held before it counts (docs/07 §1.2).</summary>
        private const float AbandonHoldSeconds = 1f;

        private readonly VisualElement _sheet;
        private readonly Label _info;
        private readonly Toggle _reduceMotion;
        private readonly SliderInt _effects;
        private readonly Button _numbers;
        private readonly Button _speed;
        private readonly Button _abandon;
        private readonly Action _onAbandon;

        /// <summary>When the abandon button went down, or -1 when nothing is being held.</summary>
        private float _abandonStart = -1f;
        private int _speedValue = 1;

        public PauseSheet(VisualElement root, Action resume, Action speed, Action abandon, Action optionsChanged)
        {
            _onAbandon = abandon;
            _sheet = root.Q<VisualElement>("pause-sheet");
            _info = root.Q<Label>("pause-info");
            _reduceMotion = root.Q<Toggle>("pause-reduce-motion");
            _effects = root.Q<SliderInt>("pause-effects");
            _numbers = root.Q<Button>("pause-numbers");
            _speed = root.Q<Button>("pause-speed");
            _abandon = root.Q<Button>("pause-abandon");
            _reduceMotion.RegisterValueChangedCallback(e => { PlayerOptions.ReduceMotion = e.newValue; optionsChanged(); });
            _effects.RegisterValueChangedCallback(e => { PlayerOptions.EffectIntensity = e.newValue; optionsChanged(); });
            _numbers.clicked += () =>
            {
                PlayerOptions.DamageNumbers = (DamageNumbersMode)(((int)PlayerOptions.DamageNumbers + 1) % 3);
                RefreshLabels();
                optionsChanged();
            };

            _speed.clicked += speed;
            root.Q<Button>("pause-resume").clicked += resume;
            SetUpHoldToAbandon();
        }

        /// <summary>Shows the sheet with what this run is (docs/09 §2.8), or puts it away.</summary>
        public void SetPaused(bool paused, GameSimulation sim, int seed, int speed)
        {
            Ui.Show(_sheet, paused);
            if (!paused)
            {
                CancelAbandon();
                return;
            }

            _reduceMotion.SetValueWithoutNotify(PlayerOptions.ReduceMotion);
            _effects.SetValueWithoutNotify(PlayerOptions.EffectIntensity);
            Ui.SetText(_info, Loc.T("pause.info", sim.CurrentWave, sim.TotalWaves, Loc.CoreName(PlayerOptions.Core), PlayerOptions.Grade, seed));
            _speedValue = speed;
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            Ui.SetText(_numbers, Ui.DamageNumbersLabel());
            Ui.SetText(_speed, $"{_speedValue}x");
        }

        /// <summary>
        /// Abandoning a run is irreversible, so it is held rather than tapped (docs/09 §0 rule 3). The button fills
        /// while the finger stays down and only fires at the end.
        /// </summary>
        private void SetUpHoldToAbandon()
        {
            _abandon.RegisterCallback<PointerDownEvent>(_ =>
            {
                _abandonStart = Time.unscaledTime;
                _abandon.schedule.Execute(TickAbandon).Every(50).Until(() => _abandonStart < 0f);
            });
            _abandon.RegisterCallback<PointerUpEvent>(_ => CancelAbandon());
            _abandon.RegisterCallback<PointerLeaveEvent>(_ => CancelAbandon());
        }

        private void TickAbandon()
        {
            if (_abandonStart < 0f)
            {
                return;
            }

            float held = (Time.unscaledTime - _abandonStart) / AbandonHoldSeconds;
            Ui.SetText(_abandon, held >= 1f ? Loc.T("pause.abandoned") : Loc.T("pause.abandon_progress", Mathf.RoundToInt(held * 100f)));
            if (held >= 1f)
            {
                CancelAbandon();
                _onAbandon();
            }
        }

        private void CancelAbandon()
        {
            _abandonStart = -1f;
            Ui.SetText(_abandon, Loc.T("pause.abandon"));
        }
    }
}
