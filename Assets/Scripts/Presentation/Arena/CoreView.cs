using TowerDefense.Presentation.Settings;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>The Core: slow breathing and a soft coral tint when hit, fading out. No flashing (docs/03 A7).</summary>
    internal sealed class CoreView
    {
        private const float BaseScale = 1.0f;

        private Transform _core;
        private Material _material;
        private float _warning;
        private float _appliedWarning = -1f;

        public Vector3 Position => _core.position;

        public void Build(ArenaKit kit)
        {
            _material = kit.NewSurface(Palette.CoreSurface);
            GameObject core = kit.CreateSurface(PrimitiveType.Sphere, "Core", new Vector3(0f, 0.4f, 0f), Vector3.one * BaseScale, _material);
            _core = core.transform;
            _warning = 0f;
            _appliedWarning = -1f;
        }

        public void OnHit() => _warning = 1f;

        public void Update(float deltaTime)
        {
            _warning *= Mathf.Exp(-deltaTime * 1.5f);
            float breath = PlayerOptions.ReduceMotion ? BaseScale : BaseScale + 0.03f * Mathf.Sin(Time.time * 1.2f);
            _core.localScale = Vector3.one * breath;

            // Update the material only while the tint is visibly changing.
            if (Mathf.Abs(_warning - _appliedWarning) > 0.004f)
            {
                SurfaceStyle style = Palette.CoreSurface;
                style.Body = Color.Lerp(Palette.Core, Palette.Enemy, 0.55f * _warning);
                ArenaKit.ApplyStyle(_material, style);
                _appliedWarning = _warning;
            }
        }
    }
}
