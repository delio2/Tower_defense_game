using System.Collections.Generic;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// The threat chips and the spawn compass at the top of the shop (docs/09 section 2.3). "New" marks a kind never
    /// met before, the one piece of information worth interrupting the calm for.
    /// </summary>
    internal sealed class WavePreview
    {
        /// <summary>Sides the spawn compass splits the arena into.</summary>
        private const int CompassSectors = 8;

        /// <summary>Every icon class a threat chip can wear; the stylesheet holds the drawings (docs/art/icons).</summary>
        public static readonly string[] ThreatIconClasses =
        {
            "threat__icon--enemy", "threat__icon--swarm", "threat__icon--dash",
            "threat__icon--split", "threat__icon--shield", "threat__icon--guardian",
        };

        private readonly VisualElement _threats;
        private readonly VisualElement _compass;
        private readonly List<VisualElement> _threatPool = new List<VisualElement>();

        /// <summary>Share of the next wave entering from each of eight sides, for the compass.</summary>
        private readonly float[] _compassShare = new float[CompassSectors];
        private readonly SortedDictionary<EnemyKind, ThreatCount> _threatCounts = new SortedDictionary<EnemyKind, ThreatCount>();
        private readonly HashSet<EnemyKind> _seenEnemies = new HashSet<EnemyKind>();

        public WavePreview(VisualElement root)
        {
            _threats = root.Q<VisualElement>("threats");
            _compass = root.Q<VisualElement>("compass");
            _compass.generateVisualContent += PaintCompass;
        }

        public static string ThreatIconClass(EnemyKind kind)
        {
            return kind switch
            {
                EnemyKind.Swarmlet => "threat__icon--swarm",
                EnemyKind.Dasher => "threat__icon--dash",
                EnemyKind.Splitter => "threat__icon--split",
                EnemyKind.Warden => "threat__icon--shield",
                EnemyKind.Guardian => "threat__icon--guardian",
                _ => "threat__icon--enemy",
            };
        }

        /// <summary>Wears exactly one threat icon class on an element.</summary>
        public static void SetThreatIcon(VisualElement icon, EnemyKind kind)
        {
            string wanted = ThreatIconClass(kind);
            foreach (string style in ThreatIconClasses)
            {
                icon.EnableInClassList(style, style == wanted);
            }
        }

        /// <summary>Marks a kind as met, so the preview chip stops calling it new (called when one spawns).</summary>
        public void MarkEnemySeen(EnemyKind kind) => _seenEnemies.Add(kind);

        public void Refresh(GameSimulation sim)
        {
            IReadOnlyList<SpawnEntry> spawns = sim.NextWavePreview;
            _threatCounts.Clear();
            for (int i = 0; i < CompassSectors; i++)
            {
                _compassShare[i] = 0f;
            }

            foreach (SpawnEntry entry in spawns)
            {
                _threatCounts.TryGetValue(entry.Kind, out ThreatCount count);
                _threatCounts[entry.Kind] = new ThreatCount(count.Total + 1, count.Elites + (entry.IsElite ? 1 : 0));
                int sector = Mathf.Clamp(entry.Direction * CompassSectors / Directions.Count, 0, CompassSectors - 1);
                _compassShare[sector] += 1f;
            }

            float peak = 0f;
            foreach (float share in _compassShare)
            {
                peak = Mathf.Max(peak, share);
            }

            for (int i = 0; i < CompassSectors; i++)
            {
                _compassShare[i] = peak > 0f ? _compassShare[i] / peak : 0f;
            }

            _compass.MarkDirtyRepaint();
            Ui.Show(_compass, spawns.Count > 0);

            int index = 0;
            foreach (KeyValuePair<EnemyKind, ThreatCount> pair in _threatCounts)
            {
                while (_threatPool.Count <= index)
                {
                    VisualElement built = BuildThreatChip();
                    _threats.Add(built);
                    _threatPool.Add(built);
                }

                VisualElement chip = _threatPool[index++];
                chip.style.display = DisplayStyle.Flex;
                chip.EnableInClassList("threat--new", !_seenEnemies.Contains(pair.Key));
                VisualElement icon = chip[0];
                SetThreatIcon(icon, pair.Key);
                icon.EnableInClassList("threat__icon--elite", pair.Value.Elites > 0);
                Ui.SetText((Label)chip[1], pair.Value.Total.ToString());
            }

            for (int i = index; i < _threatPool.Count; i++)
            {
                _threatPool[i].style.display = DisplayStyle.None;
            }
        }

        private static VisualElement BuildThreatChip()
        {
            var chip = new VisualElement { pickingMode = PickingMode.Ignore };
            chip.AddToClassList("threat");
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("threat__icon");
            var count = new Label { pickingMode = PickingMode.Ignore };
            count.AddToClassList("threat__count");
            chip.Add(icon);
            chip.Add(count);
            return chip;
        }

        /// <summary>
        /// The compass: eight wedges around a ring, the fuller the wedge the more of the wave comes from that side.
        /// It answers which way needs covering without a single word.
        /// </summary>
        private void PaintCompass(MeshGenerationContext context)
        {
            Rect rect = context.visualElement.contentRect;
            if (rect.width <= 1f)
            {
                return;
            }

            var centre = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float radius = rect.width * 0.5f - 6f;
            Painter2D painter = context.painter2D;
            painter.lineWidth = 9f;
            painter.lineCap = LineCap.Butt;
            const float sector = 360f / CompassSectors;
            for (int i = 0; i < CompassSectors; i++)
            {
                float share = _compassShare[i];
                painter.strokeColor = share > 0.01f
                    ? new Color(0.94f, 0.48f, 0.42f, 0.35f + 0.6f * share) // coral, by weight
                    : new Color(0.29f, 0.31f, 0.49f, 0.5f);                // line
                painter.BeginPath();
                float start = -90f + i * sector + 3f;
                painter.Arc(centre, radius, start, start + sector - 6f);
                painter.Stroke();
            }
        }
    }
}
