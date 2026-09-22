using System;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// Run end v2 (docs/06 2.5-D2, docs/09 §2.4): what stopped the run with its drawn icon, totals rolling up, seed and
    /// replay status, Play again, and the "Same seed" / "Share" buttons that say which phase brings them.
    /// </summary>
    internal sealed class RunEndPanel
    {
        /// <summary>How long the totals take to roll up, the same beat as the wave summary.</summary>
        private const float RollSeconds = 1.2f;

        private readonly VisualElement _panel;
        private readonly Label _title;
        private readonly Label _stats;
        private readonly VisualElement _stoppedBy;
        private readonly VisualElement _stoppedIcon;
        private readonly Label _stoppedLabel;
        private readonly Label _totalDamage;
        private readonly Label _totalKills;

        /// <summary>When the panel opened, so its totals can roll up; -1 while a run is going.</summary>
        private float _start = -1f;

        public RunEndPanel(VisualElement root, Action playAgain, Action<string> toast)
        {
            _panel = root.Q<VisualElement>("game-over");
            _title = root.Q<Label>("game-over-title");
            _stats = root.Q<Label>("game-over-stats");
            _stoppedBy = root.Q<VisualElement>("stopped-by");
            _stoppedIcon = root.Q<VisualElement>("stopped-icon");
            _stoppedLabel = root.Q<Label>("stopped-label");
            _totalDamage = root.Q<Label>("total-damage");
            _totalKills = root.Q<Label>("total-kills");
            root.Q<Button>("same-seed").clicked += () => toast(Loc.T("msg.same_seed_later"));
            root.Q<Button>("share-run").clicked += () => toast(Loc.T("msg.share_later"));
            root.Q<Button>("play-again").clicked += playAgain;
        }

        public void Refresh(GameSimulation sim, HudState state)
        {
            Ui.Show(_panel, sim.IsOver);
            if (!sim.IsOver)
            {
                _start = -1f; // a new run: the totals roll again when it ends
                return;
            }

            string title = sim.Phase == GamePhase.Victory ? Loc.T("runend.victory")
                : sim.HasWon ? Loc.T("runend.endless", sim.WavesCleared)
                : Loc.T("runend.wave", sim.WavesCleared + 1, sim.TotalWaves);
            Ui.SetText(_title, title);
            if (_start < 0f)
            {
                _start = Time.time;
            }

            bool stopped = sim.DefeatedBy.HasValue;
            Ui.Show(_stoppedBy, stopped);
            if (stopped)
            {
                Ui.SetText(_stoppedLabel, Loc.T("runend.stopped", Loc.EnemyName(sim.DefeatedBy.Value)));
                WavePreview.SetThreatIcon(_stoppedIcon, sim.DefeatedBy.Value);
            }

            float roll = Ui.EaseOut(Mathf.Clamp01((Time.time - _start) / RollSeconds));
            Ui.SetText(_totalDamage, Loc.T("runend.damage", NumberFormat.CompactHundredths((long)(sim.TotalDamage * roll))));
            Ui.SetText(_totalKills, Loc.T("runend.kills", Mathf.RoundToInt(sim.Kills * roll)));
            Ui.SetText(_stats, Loc.T("runend.stats", state.Seed, state.ReplayStatus));
        }
    }
}
