using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// Everything the HUD draws over the arena, in panel space (docs/09 §2.2b): the Integrity ring around the Core,
    /// edge markers for off-screen threats, the Guardian strip, floating damage numbers, the inspector bubble and the
    /// toast line. None of it pauses the game; the pools mean a wave never allocates.
    /// </summary>
    internal sealed class ArenaOverlay
    {
        private const float ToastSeconds = 2f;

        /// <summary>How long a tapped module or enemy keeps its bubble on screen (docs/09 §2.2).</summary>
        private const float InspectorSeconds = 2.5f;

        /// <summary>Width and height of the Integrity ring element, matching .core-arc in the stylesheet.</summary>
        private const float CoreArcSize = 300f;

        /// <summary>How long the Guardian's name stays before the strip becomes only its health bar.</summary>
        private const float GuardianNameSeconds = 2f;

        private readonly VisualElement _numbers;
        private readonly List<Label> _numberPool = new List<Label>();
        private readonly Label _toast;
        private float _toastUntil;
        private readonly VisualElement _coreArc;
        private float _coreArcFraction = 1f;
        private readonly VisualElement _edgeMarkers;
        private readonly List<VisualElement> _edgeMarkerPool = new List<VisualElement>();
        private readonly VisualElement _guardian;
        private readonly Label _guardianName;
        private readonly VisualElement _guardianFill;
        private float _guardianNameUntil;
        private readonly VisualElement _inspector;
        private readonly Label _inspectorTitle;
        private readonly Label _inspectorBody;
        private float _inspectorUntil;

        public ArenaOverlay(VisualElement root)
        {
            _numbers = root.Q<VisualElement>("numbers");
            _toast = root.Q<Label>("toast");
            _coreArc = root.Q<VisualElement>("core-arc");
            _coreArc.generateVisualContent += PaintCoreArc;
            _edgeMarkers = root.Q<VisualElement>("edge-markers");
            _guardian = root.Q<VisualElement>("guardian");
            _guardianName = root.Q<Label>("guardian-name");
            _guardianFill = root.Q<VisualElement>("guardian-fill");
            _inspector = root.Q<VisualElement>("inspector");
            _inspectorTitle = root.Q<Label>("inspector-title");
            _inspectorBody = root.Q<Label>("inspector-body");
        }

        /// <summary>Called every frame: retires the toast and the inspector bubble when their time is up.</summary>
        public void Tick()
        {
            if (Time.time >= _toastUntil)
            {
                _toast.RemoveFromClassList("toast--visible");
            }

            if (_inspectorUntil > 0f && Time.time >= _inspectorUntil)
            {
                HideInspector();
            }
        }

        public void ShowToast(string text)
        {
            _toast.text = text;
            _toastUntil = Time.time + ToastSeconds;
            _toast.AddToClassList("toast--visible");
        }

        /// <summary>
        /// Shows the read-only bubble above a point of the arena for a couple of seconds (docs/09 §2.2). The caller
        /// passes panel coordinates, so it works for a module on the ring and for a moving enemy alike.
        /// </summary>
        public void ShowInspector(Vector2 panelPosition, string title, string body)
        {
            Ui.SetText(_inspectorTitle, title);
            Ui.SetText(_inspectorBody, body);
            _inspector.style.left = panelPosition.x - 240f;
            _inspector.style.top = Mathf.Max(200f, panelPosition.y - 210f);
            Ui.Show(_inspector, true);
            _inspectorUntil = Time.time + InspectorSeconds;
        }

        public void HideInspector()
        {
            Ui.Show(_inspector, false);
            _inspectorUntil = 0f;
        }

        /// <summary>
        /// Puts the Integrity ring around the Core's projection: <paramref name="panelPosition"/> is where the Core
        /// is on screen, <paramref name="fraction"/> how much health is left. A null position hides it.
        /// </summary>
        public void SetCoreArc(Vector2? panelPosition, float fraction)
        {
            Ui.Show(_coreArc, panelPosition.HasValue);
            if (!panelPosition.HasValue)
            {
                return;
            }

            _coreArc.style.left = panelPosition.Value.x - CoreArcSize * 0.5f;
            _coreArc.style.top = panelPosition.Value.y - CoreArcSize * 0.5f;
            if (Mathf.Abs(fraction - _coreArcFraction) > 0.002f)
            {
                _coreArcFraction = fraction;
                _coreArc.MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The ring: a faint full circle for what the Core could take, a teal arc for what is left. Teal because an
        /// ivory ring would vanish against the ivory Core, and coral is reserved for the enemies (docs/03 A4).
        /// </summary>
        private void PaintCoreArc(MeshGenerationContext context)
        {
            Rect rect = context.visualElement.contentRect;
            var centre = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float radius = rect.width * 0.5f - 8f;
            Painter2D painter = context.painter2D;
            painter.lineWidth = 8f;

            painter.strokeColor = new Color(0.29f, 0.31f, 0.49f, 0.7f); // --line
            painter.BeginPath();
            painter.Arc(centre, radius, 0f, 360f);
            painter.Stroke();

            if (_coreArcFraction <= 0.001f)
            {
                return;
            }

            painter.strokeColor = new Color(0.36f, 0.78f, 0.75f, 0.95f); // --teal
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.Arc(centre, radius, -90f, -90f + 360f * Mathf.Clamp01(_coreArcFraction));
            painter.Stroke();
        }

        /// <summary>
        /// Draws one dot per off-screen threat, already clamped to the panel edge by the caller (docs/06 2.5-B6).
        /// </summary>
        public void SetEdgeMarkers(List<Vector2> positions)
        {
            while (_edgeMarkerPool.Count < positions.Count)
            {
                var marker = new VisualElement { pickingMode = PickingMode.Ignore };
                marker.AddToClassList("edge-marker");
                _edgeMarkers.Add(marker);
                _edgeMarkerPool.Add(marker);
            }

            for (int i = 0; i < _edgeMarkerPool.Count; i++)
            {
                VisualElement marker = _edgeMarkerPool[i];
                if (i >= positions.Count)
                {
                    marker.style.display = DisplayStyle.None;
                    continue;
                }

                marker.style.display = DisplayStyle.Flex;
                marker.style.left = positions[i].x - 21f;
                marker.style.top = positions[i].y - 21f;
            }
        }

        /// <summary>
        /// The Guardian announces itself by name, then the same strip stays on as its health bar (docs/06 2.5-B6).
        /// Passing a null name puts it away.
        /// </summary>
        public void SetGuardian(string guardianName, float healthFraction)
        {
            bool present = guardianName != null;
            Ui.Show(_guardian, present);
            if (!present)
            {
                _guardianNameUntil = 0f;
                return;
            }

            if (_guardianName.text != guardianName)
            {
                Ui.SetText(_guardianName, guardianName);
                _guardianNameUntil = Time.time + GuardianNameSeconds;
            }

            Ui.Show(_guardianName, Time.time < _guardianNameUntil);
            _guardianFill.style.width = Length.Percent(Mathf.Clamp01(healthFraction) * 100f);
        }

        public void SetNumbers(List<FloatingLabel> labels)
        {
            while (_numberPool.Count < labels.Count)
            {
                var label = new Label { pickingMode = PickingMode.Ignore };
                label.AddToClassList("floating-number");
                _numbers.Add(label);
                _numberPool.Add(label);
            }

            for (int i = 0; i < _numberPool.Count; i++)
            {
                Label label = _numberPool[i];
                if (i >= labels.Count)
                {
                    label.style.display = DisplayStyle.None;
                    continue;
                }

                FloatingLabel item = labels[i];
                label.style.display = DisplayStyle.Flex;
                label.text = item.Text;
                label.style.left = item.PanelPosition.x - 150f;
                label.style.top = item.PanelPosition.y - 30f;
                label.style.opacity = item.Alpha;
                label.EnableInClassList("floating-number--big", item.Big);
            }
        }
    }
}
