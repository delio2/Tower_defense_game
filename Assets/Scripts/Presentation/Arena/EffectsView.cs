using System.Collections.Generic;
using TowerDefense.Presentation.Settings;
using TowerDefense.Presentation.UI;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// Transient effects: expanding rings (Pulse, merge), hit tracers and floating damage numbers. Everything fades;
    /// nothing blinks (docs/03 A1). Lines are pooled; the tracer count is capped (clutter control, docs/03 A8).
    /// </summary>
    internal sealed class EffectsView
    {
        private const float NumberLifetime = 0.9f;

        private readonly List<LineEffect> _effects = new List<LineEffect>();
        private readonly Stack<LineRenderer> _linePool = new Stack<LineRenderer>();
        private readonly List<FloatingNumber> _numbers = new List<FloatingNumber>();

        public int ActiveLines => _effects.Count;

        public void Clear()
        {
            _effects.Clear();
            _linePool.Clear();
            _numbers.Clear();
        }

        public void SpawnRing(ArenaKit kit, Vector3 centre, float fromRadius, float toRadius, Color color, float duration, float width)
        {
            color.a *= PlayerOptions.EffectScale;
            if (PlayerOptions.ReduceMotion)
            {
                duration *= 0.5f;
            }

            LineRenderer line = RentLine(kit, width, color, true);
            _effects.Add(new LineEffect(line, centre, fromRadius, toRadius, color, duration, true));
        }

        public void SpawnBeam(ArenaKit kit, Vector3 from, Vector3 to, Color color, float width)
        {
            LineRenderer line = RentLine(kit, width, color, false);
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            _effects.Add(new LineEffect(line, from, 0f, 0f, color, 0.25f, false));
        }

        public void AddNumber(Vector3 position, string text, bool big) => _numbers.Add(new FloatingNumber(position, text, big));

        public void Update(float deltaTime)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                LineEffect effect = _effects[i];
                effect.Age += deltaTime;
                float t = Mathf.Clamp01(effect.Age / effect.Duration);
                float eased = 1f - (1f - t) * (1f - t);
                Color color = effect.Color;
                color.a *= 1f - t;
                effect.Line.startColor = color;
                effect.Line.endColor = color;
                if (effect.IsRing)
                {
                    ArenaKit.SetCircle(effect.Line, effect.Centre, Mathf.Lerp(effect.FromRadius, effect.ToRadius, eased));
                }

                if (t >= 1f)
                {
                    effect.Line.enabled = false;
                    _linePool.Push(effect.Line);
                    _effects.RemoveAt(i);
                }
            }

            for (int i = _numbers.Count - 1; i >= 0; i--)
            {
                _numbers[i].Age += deltaTime;
                if (_numbers[i].Age > NumberLifetime)
                {
                    _numbers.RemoveAt(i);
                }
            }
        }

        /// <summary>Projects the floating numbers into HUD labels: they rise and fade over 0.9 s.</summary>
        public void FillLabels(HudView hud, Camera camera, List<FloatingLabel> labels)
        {
            labels.Clear();
            foreach (FloatingNumber number in _numbers)
            {
                labels.Add(new FloatingLabel
                {
                    PanelPosition = hud.WorldToPanel(camera, number.Position + Vector3.forward * (number.Age * 0.6f)),
                    Text = number.Text,
                    Alpha = (1f - number.Age / NumberLifetime) * 0.8f,
                    Big = number.Big,
                });
            }
        }

        private LineRenderer RentLine(ArenaKit kit, float width, Color color, bool loop)
        {
            LineRenderer line = _linePool.Count > 0 ? _linePool.Pop() : kit.CreateLine("Effect", width, color, loop);
            line.enabled = true;
            line.loop = loop;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            return line;
        }

        private sealed class LineEffect
        {
            public readonly LineRenderer Line;
            public readonly Vector3 Centre;
            public readonly float FromRadius;
            public readonly float ToRadius;
            public readonly Color Color;
            public readonly float Duration;
            public readonly bool IsRing;
            public float Age;

            public LineEffect(LineRenderer line, Vector3 centre, float fromRadius, float toRadius, Color color, float duration, bool isRing)
            {
                Line = line;
                Centre = centre;
                FromRadius = fromRadius;
                ToRadius = toRadius;
                Color = color;
                Duration = duration;
                IsRing = isRing;
            }
        }

        private sealed class FloatingNumber
        {
            public readonly Vector3 Position;
            public readonly string Text;
            public readonly bool Big;
            public float Age;

            public FloatingNumber(Vector3 position, string text, bool big)
            {
                Position = position;
                Text = text;
                Big = big;
            }
        }
    }
}
