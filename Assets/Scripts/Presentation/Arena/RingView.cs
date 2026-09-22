using System;
using System.Collections.Generic;
using TowerDefense.Presentation.Settings;
using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// The ring: petals (slots), module views (Blender models, primitive fallback) and the booster links shown in the shop. Module views
    /// follow module ids, so they glide to a new slot after a Move instead of popping.
    /// </summary>
    internal sealed class RingView
    {
        /// <summary>Primitive placeholders are centred; Blender models sit on their base, on the petal's top.</summary>
        private const float PrimitiveHeight = 0.55f;
        private const float ModelHeight = 0.18f;
        private const float ModelScale = 1.25f;

        /// <summary>How long a newly placed module takes to settle onto its petal.</summary>
        private const float SettleSeconds = 0.22f;

        /// <summary>Petal shape: a flattened ellipsoid, long axis pointing away from the Core (mood shot v1).</summary>
        private static readonly Vector3 PetalScale = new Vector3(0.84f, 0.2f, 1.24f);

        /// <summary>Modules sit a little further out than the slot centre so the Core never hides them (visual only).</summary>
        private const float ModuleRadialOffset = 1.12f;

        private LineRenderer _reach;
        private readonly List<Renderer> _petals = new List<Renderer>();
        private readonly List<LineRenderer> _links = new List<LineRenderer>();
        private readonly Dictionary<int, ModuleView> _modules = new Dictionary<int, ModuleView>();
        private readonly List<int> _staleIds = new List<int>();

        public void Build(ArenaKit kit)
        {
            _petals.Clear();
            _links.Clear();
            _modules.Clear();
            _petalMaterial = kit.Surface(Palette.PetalSurface);
            _petalLitMaterial = kit.Surface(Palette.PetalLitSurface);

            _reach = kit.CreateLine("Reach", 0.04f, Palette.WithAlpha(Palette.Weapon, 0.5f), true);
            _reach.enabled = false;

            // Petals: flattened ellipsoids, long axis pointing away from the Core (mood shot v1).
            for (int slot = 0; slot < Ring.MaxSlots; slot++)
            {
                GameObject petal = kit.CreateSurface(PrimitiveType.Sphere, $"Slot {slot}", Vector3.zero,
                    PetalScale, _petalMaterial);
                _petals.Add(petal.GetComponent<Renderer>());
                _links.Add(kit.CreateLine("Link", 0.05f, Palette.WithAlpha(Palette.Booster, 0.5f), false));
            }
        }

        private Material _petalMaterial;
        private Material _petalLitMaterial;

        /// <summary>Marks a merge: the module swells 1.15 → 1 over 0.15 s (docs/03 A7). False if the view is not known.</summary>
        public bool TryMarkMerged(int moduleId, out Vector3 position)
        {
            if (_modules.TryGetValue(moduleId, out ModuleView view))
            {
                view.MergeAt = Time.time;
                position = view.Root.position;
                return true;
            }

            position = default;
            return false;
        }

        /// <summary>
        /// Shows how far a dragged weapon would shoot from the slot under the finger (docs/06 2.5-C2): a soft circle
        /// at its range, so "will it cover that side?" is answered before dropping. Null clears it.
        /// </summary>
        public void ShowReach(ArenaKit kit, (int Slot, float Range)? reach)
        {
            if (!reach.HasValue)
            {
                _reach.enabled = false;
                return;
            }

            _reach.enabled = true;
            ArenaKit.SetCircle(_reach, kit.SlotWorld(reach.Value.Slot) + Vector3.up * 0.02f, reach.Value.Range);
        }

        /// <param name="isHighlighted">Whether a slot is lit (selected, drag target, valid drop, free for a selected card).</param>
        public void Sync(ArenaKit kit, Func<int, bool> isHighlighted, float deltaTime)
        {
            Ring ring = kit.Sim.Ring;
            for (int slot = 0; slot < _petals.Count; slot++)
            {
                bool active = slot < ring.SlotCount;
                _petals[slot].gameObject.SetActive(active);
                _links[slot].enabled = false;
                if (!active)
                {
                    continue;
                }

                Vector3 slotPosition = kit.SlotWorld(slot);
                Transform petal = _petals[slot].transform;
                petal.localPosition = slotPosition * 1.05f + Vector3.up * 0.1f;
                petal.localRotation = Quaternion.LookRotation(new Vector3(slotPosition.x, 0f, slotPosition.z));

                // A lit petal breathes rather than blinking: 4% over 1.4 s (docs/09 §4.2).
                bool lit = isHighlighted(slot);
                _petals[slot].sharedMaterial = lit ? _petalLitMaterial : _petalMaterial;
                float breath = lit && !PlayerOptions.ReduceMotion ? 1f + 0.04f * Mathf.Sin(Time.time * 4.5f) : 1f;
                petal.localScale = PetalScale * breath;
            }

            _staleIds.Clear();
            foreach (KeyValuePair<int, ModuleView> pair in _modules)
            {
                if (kit.FindModule(pair.Key) == null)
                {
                    _staleIds.Add(pair.Key);
                }
            }

            foreach (int id in _staleIds)
            {
                UnityEngine.Object.Destroy(_modules[id].Root.gameObject);
                _modules.Remove(id);
            }

            for (int slot = 0; slot < ring.SlotCount; slot++)
            {
                ModuleInstance module = ring.At(slot);
                if (module == null)
                {
                    continue;
                }

                if (!_modules.TryGetValue(module.Id, out ModuleView view))
                {
                    view = CreateModuleView(kit, module);
                    _modules.Add(module.Id, view);
                }

                float levelScale = 1f + 0.18f * (module.Level - 1);
                float merge = view.MergeAt >= 0f ? Mathf.Clamp01((Time.time - view.MergeAt) / 0.15f) : 1f;
                float swell = 1f + 0.15f * (1f - merge) * (1f - merge); // ease-out back to 1

                // Settling in: a placed module grows into its petal instead of appearing (docs/03 A7).
                float settle = Mathf.Clamp01((Time.time - view.PlacedAt) / SettleSeconds);
                float eased = 1f - (1f - settle) * (1f - settle);
                view.Root.localScale = view.BaseScale * levelScale * swell * Mathf.Lerp(0.55f, 1f, eased);
                Vector3 target = kit.SlotWorld(slot) * ModuleRadialOffset;
                float height = view.Height + IdleFloat(view.Phase);
                view.Root.localPosition = Vector3.Lerp(view.Root.localPosition, target + Vector3.up * height, 1f - Mathf.Exp(-deltaTime * 12f));
                if (view.IsModel)
                {
                    view.Root.localRotation = FacingOutward(target);
                }

                // In the shop, boosters show soft links to their neighbours: the combos are visible.
                if (kit.Sim.Phase == GamePhase.Shop && module.Category == ModuleCategory.Booster)
                {
                    DrawBoosterLinks(kit, slot);
                }
            }
        }

        private void DrawBoosterLinks(ArenaKit kit, int slot)
        {
            Ring ring = kit.Sim.Ring;
            LineRenderer line = _links[slot];
            line.enabled = true;
            line.positionCount = 3;
            line.SetPosition(0, kit.SlotWorld(ring.LeftOf(slot)) + Vector3.up * 0.05f);
            line.SetPosition(1, kit.SlotWorld(slot) + Vector3.up * 0.05f);
            line.SetPosition(2, kit.SlotWorld(ring.RightOf(slot)) + Vector3.up * 0.05f);
        }

        private static ModuleView CreateModuleView(ArenaKit kit, ModuleInstance module)
        {
            SurfaceStyle style = module.Category switch
            {
                ModuleCategory.Weapon => Palette.WeaponSurface,
                ModuleCategory.Booster => Palette.BoosterSurface,
                _ => Palette.EconomySurface,
            };
            Material material = kit.Surface(style);
            Vector3 slot = kit.SlotWorld(module.Slot) * ModuleRadialOffset;
            string objectName = $"{module.Kind} #{module.Id}";

            GameObject model = kit.CreateModel($"Models/Modules/{module.Kind}", objectName, slot + Vector3.up * ModelHeight, ModelScale, material);
            if (model != null)
            {
                model.transform.localRotation = FacingOutward(slot);
                return new ModuleView(model.transform, Vector3.one * ModelScale, ModelHeight, true) { Phase = Phase(module.Id) };
            }

            // Fallback placeholder shapes (a model is missing); sized ~1.7x the v1 guess (mood shot lesson).
            PrimitiveType shape;
            Vector3 scale;
            switch (module.Category)
            {
                case ModuleCategory.Weapon:
                    shape = PrimitiveType.Sphere;
                    scale = module.Kind == ModuleKind.Scatter ? new Vector3(0.85f, 0.55f, 0.85f) : Vector3.one * 0.75f;
                    break;
                case ModuleCategory.Booster:
                    shape = PrimitiveType.Cylinder;
                    scale = new Vector3(0.85f, 0.12f, 0.85f);
                    break;
                default:
                    shape = PrimitiveType.Cube;
                    scale = Vector3.one * 0.58f;
                    break;
            }

            GameObject go = kit.CreateSurface(shape, objectName, slot + Vector3.up * PrimitiveHeight, scale, material);
            return new ModuleView(go.transform, scale, PrimitiveHeight, false) { Phase = Phase(module.Id) };
        }

        /// <summary>A stable per-module offset in radians, spread over the circle by the module id.</summary>
        private static float Phase(int moduleId) => moduleId * 1.7f % 6.2831853f;

        /// <summary>
        /// The idle float of docs/03 A7: ± 3.5 cm over 2.6 s, each module on its own phase so the ring never breathes
        /// in lockstep. Still under "reduce motion".
        /// </summary>
        private static float IdleFloat(float phase)
        {
            return PlayerOptions.ReduceMotion ? 0f : 0.035f * Mathf.Sin(Time.time * 2.4f + phase);
        }

        /// <summary>Models look along local −Z: point that away from the Core (Lance aims outward).</summary>
        private static Quaternion FacingOutward(Vector3 slot)
        {
            Vector3 outward = new Vector3(slot.x, 0f, slot.z);
            return outward.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(-outward.normalized) : Quaternion.identity;
        }

        private sealed class ModuleView
        {
            public readonly Transform Root;
            public readonly Vector3 BaseScale;
            public readonly float Height;
            public readonly bool IsModel;

            /// <summary>Offset into the idle float so neighbouring modules do not rise and fall together.</summary>
            public float Phase;

            /// <summary>Time.time when this view was created, for the settle-in growth.</summary>
            public float PlacedAt = Time.time;

            /// <summary>Time.time when a merge swelled this module (scale 1.15 → 1 over 0.15 s, docs/03 A7).</summary>
            public float MergeAt = -1f;

            public ModuleView(Transform root, Vector3 baseScale, float height, bool isModel)
            {
                Root = root;
                BaseScale = baseScale;
                Height = height;
                IsModel = isModel;
            }
        }
    }
}
