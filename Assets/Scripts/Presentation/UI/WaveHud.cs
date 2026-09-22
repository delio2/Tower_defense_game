using System;
using TowerDefense.Presentation.Settings;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// The top bar (Integrity, wave, Credits, wave progress hairline) and the wave controls: the Pulse with its
    /// painted cooldown ring, speed and pause (docs/09 §2.2).
    /// </summary>
    internal sealed class WaveHud
    {
        private readonly Label _integrity;
        private readonly Label _wave;
        private readonly Label _credits;
        private readonly VisualElement _waveProgress;
        private readonly VisualElement _waveProgressFill;
        private readonly VisualElement _waveControls;
        private readonly VisualElement _pulse;
        private readonly VisualElement _pulseHalo;
        private readonly VisualElement _pulseRing;
        private readonly Label _pulseLabel;
        private readonly Button _speed;
        private readonly Button _pause;

        /// <summary>Share of the cooldown already served, 0..1; the ring paints it.</summary>
        private float _pulseCharge;

        public WaveHud(VisualElement root, Action pulse, Action speed, Action pause)
        {
            _integrity = root.Q<Label>("integrity");
            _wave = root.Q<Label>("wave");
            _credits = root.Q<Label>("credits");
            _waveProgress = root.Q<VisualElement>("wave-progress");
            _waveProgressFill = root.Q<VisualElement>("wave-progress-fill");
            _waveControls = root.Q<VisualElement>("wave-controls");
            _pulse = root.Q<VisualElement>("pulse");
            _pulseHalo = root.Q<VisualElement>("pulse-halo");
            _pulseRing = root.Q<VisualElement>("pulse-ring");
            _pulseRing.generateVisualContent += PaintPulseRing;
            _pulseLabel = root.Q<Label>("pulse-label");
            _speed = root.Q<Button>("speed");
            _pause = root.Q<Button>("pause");

            _pulse.RegisterCallback<ClickEvent>(_ => pulse());
            _speed.clicked += speed;
            _pause.clicked += pause;
        }

        public void RefreshTopBar(GameSimulation sim)
        {
            RefreshWaveProgress(sim);
            long integrity = sim.Integrity / SimConstants.HpScale;
            Ui.SetText(_integrity, integrity.ToString()); // the heart is the drawn icon next to it, not a glyph
            _integrity.EnableInClassList("top-number--warning", sim.Integrity * 4 <= sim.MaxIntegrity);
            bool endless = sim.Config.Endless && sim.HasWon;
            string key = sim.IsGuardianWave
                ? endless ? "hud.guardian_endless" : "hud.guardian"
                : endless ? "hud.wave_endless" : "hud.wave";
            Ui.SetText(_wave, Loc.T(key, sim.CurrentWave, sim.TotalWaves));
            Ui.SetText(_credits, sim.Credits.ToString());
        }

        /// <summary>
        /// The hairline under the top bar: how much of the wave has been dealt with, spawns and survivors together
        /// (docs/09 §2.2). Hidden outside a wave, so the shop keeps its calm top edge.
        /// </summary>
        private void RefreshWaveProgress(GameSimulation sim)
        {
            bool inWave = sim.Phase == GamePhase.Wave && sim.WaveEnemyCount > 0;
            Ui.Show(_waveProgress, inWave);
            if (!inWave)
            {
                return;
            }

            float done = 1f - sim.WaveEnemiesLeft / (float)sim.WaveEnemyCount;
            _waveProgressFill.style.width = Length.Percent(Mathf.Clamp01(done) * 100f);
        }

        public void SetControlsVisible(bool visible) => Ui.Show(_waveControls, visible);

        public void RefreshControls(GameSimulation sim, HudState state)
        {
            float cooldown = sim.Config.PulseCooldownTicks == 0 ? 0f : sim.PulseCooldownRemaining / (float)sim.Config.PulseCooldownTicks;
            bool ready = sim.IsPulseReady;
            _pulse.EnableInClassList("pulse--ready", ready);

            float charge = 1f - cooldown;
            if (Mathf.Abs(charge - _pulseCharge) > 0.002f)
            {
                _pulseCharge = charge;
                _pulseRing.MarkDirtyRepaint();
            }

            // Ready: the edge breathes between 0.3 and 0.6 over two seconds (docs/09 §2.2). Still when motion is reduced.
            float halo = 0f;
            if (ready)
            {
                halo = PlayerOptions.ReduceMotion ? 0.45f : 0.45f + 0.15f * Mathf.Sin(Time.time * Mathf.PI);
            }

            _pulseHalo.style.opacity = halo;

            int seconds = Mathf.CeilToInt(sim.PulseCooldownRemaining / (float)SimConstants.TicksPerSecond);
            Ui.SetText(_pulseLabel, ready ? Loc.T("hud.pulse") : seconds <= 5 ? seconds.ToString() : Loc.T("hud.pulse_wait"));
            Ui.SetText(_speed, $"{state.Speed}x");
            Ui.SetText(_pause, string.Empty);
            _pause.EnableInClassList("btn--icon", true);
            _pause.EnableInClassList("btn--icon-resume", state.Paused);
        }

        /// <summary>
        /// The cooldown ring: a full circle of track with the charged part drawn over it, clockwise from the top.
        /// Painted rather than styled because USS has no arcs.
        /// </summary>
        private void PaintPulseRing(MeshGenerationContext context)
        {
            Rect rect = context.visualElement.contentRect;
            if (rect.width <= 1f || rect.height <= 1f)
            {
                return;
            }

            var centre = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f - 6f;
            Painter2D painter = context.painter2D;
            painter.lineWidth = 9f;
            painter.lineCap = LineCap.Butt;

            painter.strokeColor = new Color(0.56f, 0.54f, 0.62f, 0.55f); // --ivory-40, faint: the empty track
            painter.BeginPath();
            painter.Arc(centre, radius, 0f, 360f);
            painter.Stroke();

            if (_pulseCharge <= 0.001f)
            {
                return;
            }

            painter.strokeColor = _pulse.ClassListContains("pulse--ready") ? new Color(0.08f, 0.09f, 0.19f) : new Color(0.95f, 0.91f, 0.84f);
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.Arc(centre, radius, -90f, -90f + 360f * Mathf.Clamp01(_pulseCharge));
            painter.Stroke();
        }
    }
}
