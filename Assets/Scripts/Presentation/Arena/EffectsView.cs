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

        /// <summary>A death scatters this many flakes, and no more than <see cref="MaxFlakes"/> fly at once (docs/03 A8).</summary>
        private const int FlakesPerDeath = 4;
        private const int MaxFlakes = 12;
        private const float FlakeLifetime = 0.55f;

        private readonly List<LineEffect> _effects = new List<LineEffect>();
        private readonly Stack<LineRenderer> _linePool = new Stack<LineRenderer>();
        private readonly List<FloatingNumber> _numbers = new List<FloatingNumber>();
        private readonly List<Flake> _flakes = new List<Flake>();
        private readonly Stack<Transform> _flakePool = new Stack<Transform>();

        public int ActiveLines => _effects.Count;

        public void Clear()
        {
            _effects.Clear();
            _linePool.Clear();
            _numbers.Clear();
            _flakes.Clear();
            _flakePool.Clear();
        }

        /// <summary>
        /// A death: a few flakes drift outward and sink while they shrink away (docs/03 A7 — a dissolve, not a burst).
        /// Silent when "reduce motion" is on or the cap is reached; the cap is what keeps a swarm wipe readable.
        /// </summary>
        public void SpawnFlakes(ArenaKit kit, Vector3 centre, Color color)
        {
            if (PlayerOptions.ReduceMotion)
            {
                return;
            }

            int wanted = Mathf.Min(FlakesPerDeath, MaxFlakes - _flakes.Count);
            for (int i = 0; i < wanted; i++)
            {
                Transform flake = RentFlake(kit, color);
                float angle = Random.value * Mathf.PI * 2f;
                var drift = new Vector3(Mathf.Cos(angle), 1.6f, Mathf.Sin(angle)) * Random.Range(0.5f, 0.9f);
                flake.position = centre;
                flake.localScale = Vector3.one * 0.09f;
                flake.localRotation = Random.rotation;
                _flakes.Add(new Flake(flake, centre, drift));
            }
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

            for (int i = _flakes.Count - 1; i >= 0; i--)
            {
                Flake flake = _flakes[i];
                flake.Age += deltaTime;
                float t = Mathf.Clamp01(flake.Age / FlakeLifetime);
                // Thrown out and up, then pulled down: the arc reads as "it came apart", not "it exploded".
                flake.Root.position = flake.Origin + flake.Drift * t + Vector3.up * (-1.8f * t * t);
                flake.Root.localScale = Vector3.one * (0.09f * (1f - t));
                if (t >= 1f)
                {
                    flake.Root.gameObject.SetActive(false);
                    _flakePool.Push(flake.Root);
                    _flakes.RemoveAt(i);
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

        private Transform RentFlake(ArenaKit kit, Color color)
        {
            if (_flakePool.Count > 0)
            {
                Transform pooled = _flakePool.Pop();
                pooled.gameObject.SetActive(true);
                kit.SetColor(pooled.GetComponent<Renderer>(), color);
                return pooled;
            }

            return kit.CreatePrimitive(PrimitiveType.Cube, "Flake", Vector3.zero, Vector3.one * 0.09f, color).transform;
        }

        private sealed class Flake
        {
            public readonly Transform Root;
            public readonly Vector3 Origin;
            public readonly Vector3 Drift;
            public float Age;

            public Flake(Transform root, Vector3 origin, Vector3 drift)
            {
                Root = root;
                Origin = origin;
                Drift = drift;
            }
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
