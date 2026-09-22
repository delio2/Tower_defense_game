using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// The still background (docs/03 A6): the dusk floor gradient that receives soft shadows, and the concentric rings
    /// that are the range bands (D32): I 3.0 Pulse reach, II 5.5 short, III 7.5 mid, edge 9.0 where enemies enter.
    /// Nothing here moves.
    /// </summary>
    internal sealed class ArenaBackdrop
    {
        public static readonly float[] BandRadii = { 3.0f, 5.5f, 7.5f, 9.0f };

        /// <summary>Floor size in world units: wider than any camera view, so its edges are never seen.</summary>
        private const float GroundSize = 80f;

        private static readonly int FarId = Shader.PropertyToID("_FarColor");
        private static readonly int MidId = Shader.PropertyToID("_MidColor");
        private static readonly int NearId = Shader.PropertyToID("_NearColor");
        private static readonly int GlowId = Shader.PropertyToID("_GlowColor");

        private readonly Material _groundMaterial;
        private readonly LineRenderer[] _rings = new LineRenderer[4];
        private Camera _camera;

        public ArenaBackdrop(Material groundTemplate)
        {
            _groundMaterial = new Material(groundTemplate);
        }

        public void Build(ArenaKit kit, Camera camera)
        {
            _camera = camera;
            var ground = new GameObject("Ground");
            ground.transform.SetParent(kit.Root, false);
            ground.transform.localPosition = new Vector3(0f, -0.03f, 0f);
            Mesh plane = Resources.GetBuiltinResource<Mesh>("Plane.fbx");
            float meshSize = Mathf.Max(0.01f, plane.bounds.size.x);
            ground.transform.localScale = Vector3.one * (GroundSize / meshSize); // scale from the real mesh size
            ground.AddComponent<MeshFilter>().sharedMesh = plane;
            MeshRenderer renderer = ground.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _groundMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;

            for (int i = 0; i < BandRadii.Length; i++)
            {
                _rings[i] = kit.CreateLine($"Band {i + 1}", i == BandRadii.Length - 1 ? 0.03f : 0.025f, Color.white, true);
                ArenaKit.SetCircle(_rings[i], new Vector3(0f, -0.01f, 0f), BandRadii[i]);
            }
        }

        public void ApplyTheme(in SkyTheme theme)
        {
            _groundMaterial.SetColor(FarId, theme.Far);
            _groundMaterial.SetColor(MidId, theme.Mid);
            _groundMaterial.SetColor(NearId, theme.Near);
            _groundMaterial.SetColor(GlowId, theme.Glow);
            if (_camera != null)
            {
                _camera.backgroundColor = theme.Near;
            }

            foreach (LineRenderer ring in _rings)
            {
                if (ring != null)
                {
                    Color colour = Palette.WithAlpha(theme.Ring, 0.18f);
                    ring.startColor = colour;
                    ring.endColor = colour;
                }
            }
        }

        public void Dispose() => Object.Destroy(_groundMaterial);
    }
}
