using TowerDefense.Presentation.Settings;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// The Core: slow breathing and a soft coral tint when hit. No flashing (docs/03 A7). The Integrity arc that
    /// rings it lives in the HUD: from a 35-degree camera a horizontal ring flattens against the Core and the petals
    /// swallow it, so it is drawn in panel space around the Core's projection instead (HudView.SetCoreArc).
    /// </summary>
    internal sealed class CoreView
    {
        private const float BaseScale = 1.0f;

        private Transform _core;
        private Material _material;
        private float _integrity = 1f;
        private float _warning;
        private float _appliedWarning = -1f;
        private float _pulse;
        private float _appliedPulse = -1f;

        public Vector3 Position => _core.position;

        public void Build(ArenaKit kit)
        {
            _material = kit.NewSurface(Palette.CoreSurface);
            GameObject core = kit.CreateSurface(PrimitiveType.Sphere, "Core", new Vector3(0f, 0.4f, 0f), Vector3.one * BaseScale, _material);
            _core = core.transform;
            _integrity = 1f;
            _warning = 0f;
            _appliedWarning = -1f;
            _pulse = 0f;
            _appliedPulse = -1f;
        }

        public void OnHit() => _warning = 1f;

        /// <summary>
        /// Integrity as a fraction of the maximum, for whoever draws the arc. It never turns red at any level:
        /// coral belongs to the enemies (docs/03 A4), and the shrinking arc is the warning.
        /// </summary>
        public void SetIntegrity(float fraction) => _integrity = Mathf.Clamp01(fraction);

        public float Integrity => _integrity;

        /// <summary>The Core lights from the inside while the Pulse ring travels out, then settles (docs/03 A7).</summary>
        public void OnPulse() => _pulse = 1f;

        public void Update(float deltaTime)
        {
            _warning *= Mathf.Exp(-deltaTime * 1.5f);
            _pulse *= Mathf.Exp(-deltaTime * 2.2f);
            float breath = PlayerOptions.ReduceMotion ? BaseScale : BaseScale + 0.03f * Mathf.Sin(Time.time * 1.2f);
            _core.localScale = Vector3.one * breath;

            // Update the material only while the tint or the glow is visibly changing.
            if (Mathf.Abs(_warning - _appliedWarning) > 0.004f || Mathf.Abs(_pulse - _appliedPulse) > 0.004f)
            {
                SurfaceStyle style = Palette.CoreSurface;
                style.Body = Color.Lerp(Palette.Core, Palette.Enemy, 0.55f * _warning);
                style.InsideStrength += 0.8f * _pulse;
                ArenaKit.ApplyStyle(_material, style);
                _appliedWarning = _warning;
                _appliedPulse = _pulse;
            }
        }
    }
}
