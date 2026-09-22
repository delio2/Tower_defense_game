using System;
using System.Collections.Generic;
using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// The ring: guide circles, petals (slots), module views and the booster links shown in the shop. Module views
    /// follow module ids, so they glide to a new slot after a Move instead of popping.
    /// </summary>
    internal sealed class RingView
    {
        private readonly List<Renderer> _petals = new List<Renderer>();
        private readonly List<LineRenderer> _links = new List<LineRenderer>();
        private readonly Dictionary<int, ModuleView> _modules = new Dictionary<int, ModuleView>();
        private readonly List<int> _staleIds = new List<int>();

        public void Build(ArenaKit kit)
        {
            _petals.Clear();
            _links.Clear();
            _modules.Clear();

            foreach (float radius in new[] { 3f, 6f, 9f })
            {
                LineRenderer guide = kit.CreateLine("Guide", 0.02f, Palette.WithAlpha(Palette.ArenaLine, 0.7f), true);
                ArenaKit.SetCircle(guide, Vector3.zero, radius);
            }

            for (int slot = 0; slot < Ring.MaxSlots; slot++)
            {
                GameObject petal = kit.CreatePrimitive(PrimitiveType.Cylinder, $"Slot {slot}", Vector3.zero,
                    new Vector3(0.9f, 0.02f, 0.9f), Palette.Petal);
                _petals.Add(petal.GetComponent<Renderer>());
                _links.Add(kit.CreateLine("Link", 0.05f, Palette.WithAlpha(Palette.Booster, 0.5f), false));
            }
        }

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

                _petals[slot].transform.localPosition = kit.SlotWorld(slot);
                kit.SetColor(_petals[slot], isHighlighted(slot) ? Palette.PetalSelected : Palette.Petal);
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
                view.Root.localScale = view.BaseScale * levelScale * swell;
                view.Root.localPosition = Vector3.Lerp(view.Root.localPosition, kit.SlotWorld(slot) + Vector3.up * 0.3f, 1f - Mathf.Exp(-deltaTime * 12f));

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
            PrimitiveType shape;
            Vector3 scale;
            Color color;
            switch (module.Category)
            {
                case ModuleCategory.Weapon:
                    shape = PrimitiveType.Sphere;
                    scale = module.Kind == ModuleKind.Scatter ? new Vector3(0.55f, 0.35f, 0.55f) : Vector3.one * 0.5f;
                    color = Palette.Weapon;
                    break;
                case ModuleCategory.Booster:
                    shape = PrimitiveType.Cylinder;
                    scale = new Vector3(0.55f, 0.08f, 0.55f);
                    color = Palette.Booster;
                    break;
                default:
                    shape = PrimitiveType.Cube;
                    scale = Vector3.one * 0.38f;
                    color = Palette.Economy;
                    break;
            }

            GameObject go = kit.CreatePrimitive(shape, $"{module.Kind} #{module.Id}", kit.SlotWorld(module.Slot) + Vector3.up * 0.3f, scale, color);
            if (module.Category == ModuleCategory.Economy)
            {
                go.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            }

            return new ModuleView(go.transform, scale);
        }

        private sealed class ModuleView
        {
            public readonly Transform Root;
            public readonly Vector3 BaseScale;

            /// <summary>Time.time when a merge swelled this module (scale 1.15 → 1 over 0.15 s, docs/03 A7).</summary>
            public float MergeAt = -1f;

            public ModuleView(Transform root, Vector3 baseScale)
            {
                Root = root;
                BaseScale = baseScale;
            }
        }
    }
}
