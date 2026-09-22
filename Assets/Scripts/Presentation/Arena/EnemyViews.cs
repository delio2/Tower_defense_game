using System.Collections.Generic;
using TowerDefense.Presentation.Settings;
using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// Enemy views keyed by enemy id: body, HP bar and the aura/elite halos. Simple-shape silhouettes per kind
    /// (docs/03 A5 with primitives) until the Blender models of Phase 2.5-A6.
    /// </summary>
    internal sealed class EnemyViews
    {
        private readonly Dictionary<int, EnemyView> _views = new Dictionary<int, EnemyView>();
        private readonly List<int> _staleIds = new List<int>();

        public void Clear() => _views.Clear();

        public bool TryGetPosition(int enemyId, out Vector3 position)
        {
            if (_views.TryGetValue(enemyId, out EnemyView view))
            {
                position = view.Body.transform.position;
                return true;
            }

            position = default;
            return false;
        }

        public void Sync(ArenaKit kit)
        {
            foreach (Enemy enemy in kit.Sim.Enemies)
            {
                if (!_views.TryGetValue(enemy.Id, out EnemyView view))
                {
                    view = Create(kit, enemy);
                    _views.Add(enemy.Id, view);
                }

                enemy.GetPosition(out long x, out long y);
                Vector3 position = new Vector3(x / (float)SimConstants.Micro, 0.45f, y / (float)SimConstants.Micro);
                view.Body.transform.position = position;
                view.Body.transform.rotation = Orientation(view, position, enemy.Kind);

                if (view.Halo != null)
                {
                    ArenaKit.SetCircle(view.Halo, position, view.HaloRadius);
                    if (view.Halo2 != null)
                    {
                        ArenaKit.SetCircle(view.Halo2, position, view.HaloRadius * 1.25f);
                    }
                }

                float fraction = enemy.MaxHp > 0 ? (float)enemy.Hp / enemy.MaxHp : 0f;
                float width = view.Size * 1.2f;
                view.HpBar.position = position + new Vector3(-(1f - fraction) * width * 0.5f, 0.5f, view.Size * 0.9f);
                view.HpBar.localScale = new Vector3(width * fraction, 0.03f, 0.06f);
            }

            _staleIds.Clear();
            foreach (KeyValuePair<int, EnemyView> pair in _views)
            {
                if (kit.FindEnemy(pair.Key) == null)
                {
                    _staleIds.Add(pair.Key);
                }
            }

            foreach (int id in _staleIds)
            {
                EnemyView stale = _views[id];
                Object.Destroy(stale.Body);
                Object.Destroy(stale.HpBar.gameObject);
                if (stale.Halo != null)
                {
                    Object.Destroy(stale.Halo.gameObject);
                }

                if (stale.Halo2 != null)
                {
                    Object.Destroy(stale.Halo2.gameObject);
                }

                _views.Remove(id);
            }
        }

        /// <summary>
        /// Models look along local −Z: point that at the Core, with a slow roll around the travel axis (docs/03 A6).
        /// The Guardian turns slowly on itself instead. Primitive fallbacks keep the old slow spin.
        /// </summary>
        private static Quaternion Orientation(EnemyView view, Vector3 position, EnemyKind kind)
        {
            float t = PlayerOptions.ReduceMotion ? 0f : Time.time;
            if (!view.IsModel)
            {
                return Quaternion.Euler(0f, 45f + t * 12f, 0f);
            }

            if (kind == EnemyKind.Guardian)
            {
                return Quaternion.Euler(0f, t * 8f, 0f);
            }

            Vector3 outward = new Vector3(position.x, 0f, position.z);
            Quaternion facing = outward.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(outward.normalized) : Quaternion.identity;
            return facing * Quaternion.Euler(0f, 0f, t * 20f + view.Phase);
        }

        /// <summary>
        /// Blender models (Tools/blender/build_models_v1.py) when present, otherwise primitives: Drifter cube, Swarmlet small cube, Brute squat wide cube, Dasher long thin cube (arrow-like), Splitter cube
        /// with a glowing core, Warden cube with a halo showing its aura, all coral; Guardian large rose capsule with a
        /// halo. Elites: double halo and the rose tint (rose is reserved for elites and Guardians, docs/03 A4).
        /// </summary>
        private static EnemyView Create(ArenaKit kit, Enemy enemy)
        {
            float size = 0.35f;
            Color color = Palette.Enemy;
            SurfaceStyle style = Palette.EnemySurface;
            Vector3 scale;
            PrimitiveType shape = PrimitiveType.Cube;
            float haloRadius = 0f;
            switch (enemy.Kind)
            {
                case EnemyKind.Swarmlet:
                    size = 0.22f;
                    scale = new Vector3(size, size * 0.6f, size);
                    break;
                case EnemyKind.Brute:
                    size = 0.55f;
                    scale = new Vector3(size, size * 0.45f, size);
                    break;
                case EnemyKind.Dasher:
                    size = 0.35f;
                    scale = new Vector3(size * 0.45f, size * 0.5f, size * 1.4f);
                    break;
                case EnemyKind.Splitter:
                    size = 0.4f;
                    color = Color.Lerp(Palette.Enemy, Palette.Core, 0.25f);
                    style.Inside = Palette.Glow;
                    style.InsideStrength = 0.6f;
                    scale = new Vector3(size, size * 0.6f, size);
                    break;
                case EnemyKind.Warden:
                    size = 0.45f;
                    scale = new Vector3(size, size * 0.8f, size);
                    haloRadius = enemy.Definition.ShieldRadiusMilli / 1000f;
                    break;
                case EnemyKind.Guardian:
                    size = 1.0f;
                    color = Palette.EnemyHeavy;
                    style = Palette.EnemyHeavySurface;
                    shape = PrimitiveType.Capsule;
                    scale = new Vector3(size, size * 0.5f, size);
                    haloRadius = size * 0.9f;
                    break;
                default:
                    scale = new Vector3(size, size * 0.6f, size);
                    break;
            }

            if (enemy.IsElite)
            {
                size *= 1.1f;
                scale *= 1.1f;
                color = Palette.EnemyHeavy;
                style = Palette.EnemyHeavySurface;
                haloRadius = Mathf.Max(haloRadius, size * 1.1f);
            }

            float modelScale = enemy.IsElite ? 1.1f : 1f;
            GameObject body = kit.CreateModel($"Models/Enemies/{enemy.Kind}", $"{enemy.Kind} #{enemy.Id}", Vector3.zero, modelScale, kit.Surface(style));
            bool isModel = body != null;
            if (!isModel)
            {
                body = kit.CreateSurface(shape, $"{enemy.Kind} #{enemy.Id}", Vector3.zero, scale, kit.Surface(style));
            }

            GameObject bar = kit.CreatePrimitive(PrimitiveType.Cube, "HP", Vector3.zero, Vector3.one * 0.05f,
                Color.Lerp(Palette.Core, Palette.Background, 0.3f));
            var view = new EnemyView(body, bar.transform, size) { IsModel = isModel, Phase = enemy.Id * 47f % 360f };
            if (haloRadius > 0f)
            {
                view.Halo = kit.CreateLine("Halo", 0.025f, Palette.WithAlpha(color, enemy.IsElite ? 0.55f : 0.35f), true);
                view.HaloRadius = haloRadius;
                if (enemy.IsElite)
                {
                    view.Halo2 = kit.CreateLine("Halo2", 0.02f, Palette.WithAlpha(color, 0.3f), true);
                }
            }

            return view;
        }

        private sealed class EnemyView
        {
            public readonly GameObject Body;
            public readonly Transform HpBar;
            public readonly float Size;

            /// <summary>Aura ring (Warden, Guardian) or elite halo; a second ring marks elites.</summary>
            public LineRenderer Halo;
            public LineRenderer Halo2;
            public float HaloRadius;
            public bool IsModel;

            /// <summary>Per-enemy roll offset so a group does not turn in lockstep.</summary>
            public float Phase;

            public EnemyView(GameObject body, Transform hpBar, float size)
            {
                Body = body;
                HpBar = hpBar;
                Size = size;
            }
        }
    }
}
