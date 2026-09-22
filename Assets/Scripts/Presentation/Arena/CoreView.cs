using TowerDefense.Presentation.Settings;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>The Core: slow breathing and a soft coral tint when hit, fading out. No flashing (docs/03 A7).</summary>
    internal sealed class CoreView
    {
        private Transform _core;
        private Renderer _renderer;
        private float _warning;

        public Vector3 Position => _core.position;

        public void Build(ArenaKit kit)
        {
            GameObject core = kit.CreatePrimitive(PrimitiveType.Sphere, "Core", new Vector3(0f, 0.35f, 0f), Vector3.one * 1.3f, Palette.Core);
            _core = core.transform;
            _renderer = core.GetComponent<Renderer>();
            _warning = 0f;
        }

        public void OnHit() => _warning = 1f;

        public void Update(ArenaKit kit, float deltaTime)
        {
            _warning *= Mathf.Exp(-deltaTime * 1.5f);
            float breath = PlayerOptions.ReduceMotion ? 1.3f : 1.3f + 0.03f * Mathf.Sin(Time.time * 1.2f);
            _core.localScale = Vector3.one * breath;
            kit.SetColor(_renderer, Color.Lerp(Palette.Core, Palette.Enemy, 0.55f * _warning));
        }
    }
}
