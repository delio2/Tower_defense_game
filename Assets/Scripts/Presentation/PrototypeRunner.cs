using System.Collections.Generic;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace TowerDefense.Presentation
{
    /// <summary>
    /// Prototype v2 driver (GDD v0.2 §19): runs the deterministic core-defense simulation and draws it with simple,
    /// calm shapes. Visuals only read the simulation; game state changes exclusively through commands.
    /// Visual rule (docs/03 §A1): no flashing, nothing repeating more than ~2x/s in one spot, eased motion.
    /// </summary>
    public sealed class PrototypeRunner : MonoBehaviour
    {
        private const float TopBarFraction = 0.1f;
        private const float BottomBarFraction = 0.26f;
        private const float ReferenceHeight = 1600f;
        /// <summary>Wave view: the whole arena. Shop view: close-up on the ring so slots are comfortable to tap (48dp+).</summary>
        private const float ArenaViewRadius = 9.4f;
        private const float ShopViewRadius = 3.4f;
        private const float SlotPickRadius = 0.6f;
        private const float CorePickRadius = 0.9f;
        private const int MaxStepsPerFrame = 240;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private int _seed = 1;

        private GameSimulation _sim;
        private Camera _camera;
        private Transform _root;
        private Material _unlit;
        private Material _lineMaterial;
        private MaterialPropertyBlock _block;

        private Transform _core;
        private Renderer _coreRenderer;
        private readonly List<Renderer> _petals = new List<Renderer>();
        private readonly List<LineRenderer> _links = new List<LineRenderer>();
        private readonly Dictionary<int, ModuleView> _moduleViews = new Dictionary<int, ModuleView>();
        private readonly Dictionary<int, EnemyView> _enemyViews = new Dictionary<int, EnemyView>();
        private readonly List<SimEvent> _events = new List<SimEvent>();
        private readonly List<LineEffect> _effects = new List<LineEffect>();
        private readonly Stack<LineRenderer> _linePool = new Stack<LineRenderer>();
        private readonly List<FloatingNumber> _numbers = new List<FloatingNumber>();
        private readonly List<int> _staleIds = new List<int>();

        private double _tickAccumulator;
        private int _speed = 1;
        private bool _paused;
        private int _selectedOffer = -1;
        private int _selectedSlot = -1;
        private bool _moveMode;
        private float _coreWarning;
        private float _viewRadius = ShopViewRadius;
        private string _message;
        private float _messageUntil;
        private string _replayStatus;

        private GUIStyle _labelStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _bigStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _numberStyle;
        private GUISkin _stylesSkin;

        // ------------------------------------------------------------------ lifecycle

        private void Awake() => EnsureInitialized();

        /// <summary>Also rebuilds after a script recompile in Play mode, when non-serialized state is lost.</summary>
        private void EnsureInitialized()
        {
            if (_sim != null && _camera != null && _unlit != null)
            {
                return;
            }

            Application.targetFrameRate = 60;
#if UNITY_EDITOR
            Application.runInBackground = true;
#endif
            _block = new MaterialPropertyBlock();
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            _unlit = new Material(unlitShader) { name = "PrototypeUnlit" };
            _lineMaterial = new Material(Shader.Find("Sprites/Default")) { name = "PrototypeLines" };
            SetupCamera();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            _root = null;
            StartRun();
        }

        private void OnDestroy()
        {
            Destroy(_unlit);
            Destroy(_lineMaterial);
        }

        private void StartRun()
        {
            if (_root != null)
            {
                Destroy(_root.gameObject);
            }

            _moduleViews.Clear();
            _enemyViews.Clear();
            _effects.Clear();
            _linePool.Clear();
            _numbers.Clear();
            _events.Clear();
            _petals.Clear();
            _links.Clear();
            _tickAccumulator = 0;
            _paused = false;
            _selectedOffer = -1;
            _selectedSlot = -1;
            _moveMode = false;
            _replayStatus = null;
            _viewRadius = ShopViewRadius;

            _sim = new GameSimulation(new RunConfig { Seed = (ulong)_seed }, ContentDatabase.CreatePrototypeDefaults());
            _root = new GameObject("Arena").transform;
            _root.SetParent(transform, false);
            BuildArena();
        }

        private void Update()
        {
            EnsureInitialized();
            UpdateLayout();
            HandlePointer();
            _sim.ApplyPendingCommandsNow();
            AdvanceSimulation();
            ProcessEvents();
            SyncRing();
            SyncEnemies();
            UpdateEffects();
            UpdateCore();
            CheckGameOver();
        }

        // ------------------------------------------------------------------ simulation

        private void AdvanceSimulation()
        {
            if (_paused || _sim.Phase != GamePhase.Wave)
            {
                _tickAccumulator = 0;
                return;
            }

            _tickAccumulator += Time.deltaTime * SimConstants.TicksPerSecond * _speed;
            int steps = 0;
            while (_tickAccumulator >= 1.0 && steps < MaxStepsPerFrame && _sim.Phase == GamePhase.Wave)
            {
                _sim.Step();
                _tickAccumulator -= 1.0;
                steps++;
            }
        }

        private void ProcessEvents()
        {
            _events.Clear();
            _sim.DrainEvents(_events);
            foreach (SimEvent e in _events)
            {
                switch (e.Type)
                {
                    case SimEventType.EnemyHit:
                        OnEnemyHit(e);
                        break;
                    case SimEventType.EnemyKilled:
                        if (_enemyViews.TryGetValue(e.EntityId, out EnemyView killed))
                        {
                            _numbers.Add(new FloatingNumber(killed.Body.transform.position, FormatDamage(e.Value)));
                        }

                        break;
                    case SimEventType.CoreHit:
                        _coreWarning = 1f;
                        break;
                    case SimEventType.PulseUsed:
                        SpawnRing(_core.position, 0.8f, _sim.Config.PulseRadius / (float)SimConstants.Micro,
                            Palette.WithAlpha(Palette.Core, 0.5f), 0.6f, 0.05f);
                        break;
                    case SimEventType.ModuleMerged:
                        ShowMessage($"Merged: level {e.Value}");
                        break;
                    case SimEventType.WaveCleared:
                        ShowMessage(e.Extra > 0 ? $"Wave cleared  +{e.Extra} interest" : "Wave cleared");
                        break;
                    case SimEventType.CommandRejected:
                        ShowMessage(DescribeRejection((CommandResult)e.Extra));
                        break;
                }
            }
        }

        private void OnEnemyHit(SimEvent e)
        {
            if (e.Extra == 0 || !_enemyViews.TryGetValue(e.EntityId, out EnemyView enemy))
            {
                return;
            }

            ModuleInstance module = FindModule(e.Extra);
            if (module == null)
            {
                return;
            }

            // Soft, low-alpha tracer that fades over 0.25 s (calm; at most ~2 per second per module).
            Vector3 from = SlotWorld(module.Slot) + Vector3.up * 0.3f;
            SpawnBeam(from, enemy.Body.transform.position, Palette.WithAlpha(Palette.Weapon, 0.35f), 0.04f);
        }

        private void CheckGameOver()
        {
            if (!_sim.IsOver || _replayStatus != null)
            {
                return;
            }

            // Every finished run is recorded and re-simulated: the same check the server will do (D19).
            string text = Replay.Record(_sim).Serialize();
            ReplayCheck check = ReplayVerifier.Verify(Replay.Deserialize(text), ContentDatabase.CreatePrototypeDefaults(),
                () => new RunConfig());
            _replayStatus = check.IsValid
                ? $"Replay verified ({_sim.CommandLog.Count} commands, {text.Length} bytes)"
                : $"Replay MISMATCH: {check.Reason}";
        }

        // ------------------------------------------------------------------ input

        private void HandlePointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame || _sim.IsOver)
            {
                return;
            }

            Vector2 screen = pointer.position.ReadValue();
            if (IsOverUi(screen))
            {
                return;
            }

            Vector3 world = ScreenToWorld(screen);
            if (_sim.Phase == GamePhase.Wave)
            {
                if (new Vector2(world.x, world.z).magnitude < CorePickRadius)
                {
                    TryPulse();
                }

                return;
            }

            int slot = PickSlot(world);
            if (slot < 0)
            {
                _selectedSlot = -1;
                _moveMode = false;
                return;
            }

            if (_moveMode && _selectedSlot >= 0)
            {
                _sim.Enqueue(Command.Move(_selectedSlot, slot));
                _moveMode = false;
                _selectedSlot = slot;
                return;
            }

            if (_selectedOffer >= 0)
            {
                _sim.Enqueue(Command.Buy(_selectedOffer, slot));
                _selectedOffer = -1;
                return;
            }

            _selectedSlot = _sim.Ring.At(slot) != null && _selectedSlot != slot ? slot : -1;
        }

        private void TryPulse()
        {
            CommandResult result = _sim.Validate(Command.Pulse());
            if (result == CommandResult.Ok)
            {
                _sim.Enqueue(Command.Pulse());
            }
            else
            {
                ShowMessage(DescribeRejection(result));
            }
        }

        private bool IsOverUi(Vector2 screen)
        {
            return screen.y < Screen.height * BottomBarFraction || screen.y > Screen.height * (1f - TopBarFraction);
        }

        private Vector3 ScreenToWorld(Vector2 screen)
        {
            Ray ray = _camera.ScreenPointToRay(screen);
            float distance = Mathf.Abs(ray.direction.y) < 1e-5f ? 0f : -ray.origin.y / ray.direction.y;
            return ray.origin + ray.direction * distance;
        }

        private int PickSlot(Vector3 world)
        {
            for (int slot = 0; slot < _sim.Ring.SlotCount; slot++)
            {
                if (Vector3.Distance(new Vector3(world.x, 0f, world.z), SlotWorld(slot)) < SlotPickRadius)
                {
                    return slot;
                }
            }

            return -1;
        }

        // ------------------------------------------------------------------ arena and views

        private void SetupCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                _camera = cameraObject.AddComponent<Camera>();
            }

            _camera.orthographic = true;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Palette.Background;
            _camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 100f;
        }

        private void UpdateLayout()
        {
            // Smooth zoom between the shop close-up and the full arena (eased, no cuts).
            float target = _sim.Phase == GamePhase.Shop ? ShopViewRadius : ArenaViewRadius;
            _viewRadius = Mathf.Lerp(_viewRadius, target, 1f - Mathf.Exp(-Time.deltaTime * 3f));

            float aspect = Mathf.Max(0.1f, _camera.aspect);
            float usable = 1f - TopBarFraction - BottomBarFraction;
            float size = Mathf.Max(_viewRadius / usable, _viewRadius / aspect);
            _camera.orthographicSize = size;
            float bandCentre = BottomBarFraction + usable * 0.5f;
            float offset = (bandCentre - 0.5f) * size * 2f;
            _camera.transform.position = new Vector3(0f, 20f, -offset);
        }

        private void BuildArena()
        {
            foreach (float radius in new[] { 3f, 6f, 9f })
            {
                LineRenderer guide = CreateLine("Guide", 0.02f, Palette.WithAlpha(Palette.ArenaLine, 0.7f), true);
                SetCircle(guide, Vector3.zero, radius);
            }

            GameObject core = CreatePrimitive(PrimitiveType.Sphere, "Core", _root, new Vector3(0f, 0.35f, 0f), Vector3.one * 1.3f, Palette.Core);
            _core = core.transform;
            _coreRenderer = core.GetComponent<Renderer>();

            for (int slot = 0; slot < Ring.MaxSlots; slot++)
            {
                GameObject petal = CreatePrimitive(PrimitiveType.Cylinder, $"Slot {slot}", _root, Vector3.zero,
                    new Vector3(0.9f, 0.02f, 0.9f), Palette.Petal);
                _petals.Add(petal.GetComponent<Renderer>());
                _links.Add(CreateLine("Link", 0.05f, Palette.WithAlpha(Palette.Booster, 0.5f), false));
            }
        }

        private void SyncRing()
        {
            Ring ring = _sim.Ring;
            for (int slot = 0; slot < _petals.Count; slot++)
            {
                bool active = slot < ring.SlotCount;
                _petals[slot].gameObject.SetActive(active);
                _links[slot].enabled = false;
                if (!active)
                {
                    continue;
                }

                _petals[slot].transform.localPosition = SlotWorld(slot);
                bool highlighted = slot == _selectedSlot || (_selectedOffer >= 0 && ring.At(slot) == null);
                SetColor(_petals[slot], highlighted ? Palette.PetalSelected : Palette.Petal);
            }

            // Module views follow the ring (ids are stable; slots can change with Move).
            _staleIds.Clear();
            foreach (KeyValuePair<int, ModuleView> pair in _moduleViews)
            {
                ModuleInstance module = FindModule(pair.Key);
                if (module == null)
                {
                    _staleIds.Add(pair.Key);
                }
            }

            foreach (int id in _staleIds)
            {
                Destroy(_moduleViews[id].Root.gameObject);
                _moduleViews.Remove(id);
            }

            for (int slot = 0; slot < ring.SlotCount; slot++)
            {
                ModuleInstance module = ring.At(slot);
                if (module == null)
                {
                    continue;
                }

                if (!_moduleViews.TryGetValue(module.Id, out ModuleView view))
                {
                    view = CreateModuleView(module);
                    _moduleViews.Add(module.Id, view);
                }

                float levelScale = 1f + 0.18f * (module.Level - 1);
                view.Root.localScale = view.BaseScale * levelScale;
                view.Root.localPosition = Vector3.Lerp(view.Root.localPosition, SlotWorld(slot) + Vector3.up * 0.3f, 1f - Mathf.Exp(-Time.deltaTime * 12f));

                // In the shop, boosters show soft links to their neighbours: the combos are visible.
                if (_sim.Phase == GamePhase.Shop && module.Category == ModuleCategory.Booster)
                {
                    DrawBoosterLinks(slot);
                }
            }
        }

        private void DrawBoosterLinks(int slot)
        {
            Ring ring = _sim.Ring;
            LineRenderer line = _links[slot];
            line.enabled = true;
            line.positionCount = 3;
            line.SetPosition(0, SlotWorld(ring.LeftOf(slot)) + Vector3.up * 0.05f);
            line.SetPosition(1, SlotWorld(slot) + Vector3.up * 0.05f);
            line.SetPosition(2, SlotWorld(ring.RightOf(slot)) + Vector3.up * 0.05f);
        }

        private ModuleView CreateModuleView(ModuleInstance module)
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

            GameObject go = CreatePrimitive(shape, $"{module.Kind} #{module.Id}", _root, SlotWorld(module.Slot) + Vector3.up * 0.3f, scale, color);
            if (module.Category == ModuleCategory.Economy)
            {
                go.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            }

            return new ModuleView(go.transform, scale);
        }

        private void SyncEnemies()
        {
            foreach (Enemy enemy in _sim.Enemies)
            {
                if (!_enemyViews.TryGetValue(enemy.Id, out EnemyView view))
                {
                    view = CreateEnemyView(enemy);
                    _enemyViews.Add(enemy.Id, view);
                }

                enemy.GetPosition(out long x, out long y);
                Vector3 position = new Vector3(x / (float)SimConstants.Micro, 0.25f, y / (float)SimConstants.Micro);
                view.Body.transform.position = position;
                view.Body.transform.rotation = Quaternion.Euler(0f, 45f + Time.time * 12f, 0f);

                float fraction = enemy.MaxHp > 0 ? (float)enemy.Hp / enemy.MaxHp : 0f;
                float width = view.Size * 1.2f;
                view.HpBar.position = position + new Vector3(-(1f - fraction) * width * 0.5f, 0.5f, view.Size * 0.9f);
                view.HpBar.localScale = new Vector3(width * fraction, 0.03f, 0.06f);
            }

            _staleIds.Clear();
            foreach (KeyValuePair<int, EnemyView> pair in _enemyViews)
            {
                if (FindEnemy(pair.Key) == null)
                {
                    _staleIds.Add(pair.Key);
                }
            }

            foreach (int id in _staleIds)
            {
                Destroy(_enemyViews[id].Body);
                Destroy(_enemyViews[id].HpBar.gameObject);
                _enemyViews.Remove(id);
            }
        }

        private EnemyView CreateEnemyView(Enemy enemy)
        {
            float size;
            Color color = Palette.Enemy;
            switch (enemy.Kind)
            {
                case EnemyKind.Swarmlet:
                    size = 0.22f;
                    break;
                case EnemyKind.Brute:
                    size = 0.55f;
                    color = Palette.EnemyHeavy;
                    break;
                case EnemyKind.Guardian:
                    size = 1.0f;
                    color = Palette.EnemyHeavy;
                    break;
                default:
                    size = 0.35f;
                    break;
            }

            GameObject body = CreatePrimitive(PrimitiveType.Cube, $"{enemy.Kind} #{enemy.Id}", _root, Vector3.zero,
                new Vector3(size, size * 0.6f, size), color);
            GameObject bar = CreatePrimitive(PrimitiveType.Cube, "HP", _root, Vector3.zero, Vector3.one * 0.05f,
                Color.Lerp(Palette.Core, Palette.Background, 0.3f));
            return new EnemyView(body, bar.transform, size);
        }

        private void UpdateCore()
        {
            // Slow breathing and a soft warning tint when hit: no flashing.
            _coreWarning *= Mathf.Exp(-Time.deltaTime * 1.5f);
            float breath = 1.3f + 0.03f * Mathf.Sin(Time.time * 1.2f);
            _core.localScale = Vector3.one * breath;
            SetColor(_coreRenderer, Color.Lerp(Palette.Core, Palette.Enemy, 0.55f * _coreWarning));
        }

        // ------------------------------------------------------------------ effects

        private void SpawnRing(Vector3 centre, float fromRadius, float toRadius, Color color, float duration, float width)
        {
            LineRenderer line = RentLine(width, color, true);
            _effects.Add(new LineEffect(line, centre, fromRadius, toRadius, color, duration, true));
        }

        private void SpawnBeam(Vector3 from, Vector3 to, Color color, float width)
        {
            LineRenderer line = RentLine(width, color, false);
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            _effects.Add(new LineEffect(line, from, 0f, 0f, color, 0.25f, false));
        }

        private void UpdateEffects()
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                LineEffect effect = _effects[i];
                effect.Age += Time.deltaTime;
                float t = Mathf.Clamp01(effect.Age / effect.Duration);
                float eased = 1f - (1f - t) * (1f - t);
                Color color = effect.Color;
                color.a *= 1f - t;
                effect.Line.startColor = color;
                effect.Line.endColor = color;
                if (effect.IsRing)
                {
                    SetCircle(effect.Line, effect.Centre, Mathf.Lerp(effect.FromRadius, effect.ToRadius, eased));
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
                _numbers[i].Age += Time.deltaTime;
                if (_numbers[i].Age > 0.9f)
                {
                    _numbers.RemoveAt(i);
                }
            }
        }

        private LineRenderer RentLine(float width, Color color, bool loop)
        {
            LineRenderer line = _linePool.Count > 0 ? _linePool.Pop() : CreateLine("Effect", width, color, loop);
            line.enabled = true;
            line.loop = loop;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            return line;
        }

        private static void SetCircle(LineRenderer line, Vector3 centre, float radius)
        {
            const int segments = 64;
            line.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                line.SetPosition(i, centre + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        // ------------------------------------------------------------------ HUD (IMGUI, prototype only)

        private void OnGUI()
        {
            if (_sim == null || _camera == null)
            {
                return;
            }

            EnsureStyles();
            float scale = Screen.height / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = ReferenceHeight;

            DrawNumbers(scale);
            DrawTopBar(width);
            if (_sim.Phase == GamePhase.Shop)
            {
                DrawShop(width, height);
            }
            else if (_sim.Phase == GamePhase.Wave)
            {
                DrawWaveControls(width, height);
            }

            if (Time.time < _messageUntil && !string.IsNullOrEmpty(_message))
            {
                GUI.Label(new Rect(0f, height * TopBarFraction + 10f, width, 50f), _message, _smallStyle);
            }

            if (_sim.IsOver)
            {
                DrawGameOver(width, height);
            }
        }

        private void DrawNumbers(float scale)
        {
            foreach (FloatingNumber number in _numbers)
            {
                Vector3 screen = _camera.WorldToScreenPoint(number.Position);
                float alpha = 1f - number.Age / 0.9f;
                var rect = new Rect(screen.x / scale - 100f, (Screen.height - screen.y) / scale - 40f - number.Age * 40f, 200f, 40f);
                Color previous = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha * 0.8f);
                GUI.Label(rect, number.Text, _numberStyle);
                GUI.color = previous;
            }
        }

        private void DrawTopBar(float width)
        {
            string guardian = _sim.IsGuardianWave ? "  GUARDIAN" : string.Empty;
            GUI.Label(new Rect(20f, 14f, width - 40f, 50f),
                $"Core {_sim.Integrity / SimConstants.HpScale}/{_sim.MaxIntegrity / SimConstants.HpScale}    Wave {_sim.CurrentWave}/{_sim.TotalWaves}{guardian}    Credits {_sim.Credits}",
                _labelStyle);
            GUI.Label(new Rect(20f, 70f, width - 40f, 40f),
                $"Damage {FormatDamage(_sim.TotalDamage)}    Kills {_sim.Kills}    seed {_seed}", _smallStyle);
        }

        private void DrawShop(float width, float height)
        {
            float top = height * (1f - BottomBarFraction) + 8f;
            float gap = 12f;
            GUI.Label(new Rect(gap, top, width - 2f * gap, 40f), $"Next: {DescribeWave(_sim.NextWavePreview)}", _smallStyle);
            top += 46f;

            ModuleInstance selected = _sim.Ring.At(_selectedSlot);
            float rowHeight = 190f;
            if (selected != null)
            {
                DrawModulePanel(selected, width, top, rowHeight, gap);
            }
            else
            {
                float cardWidth = (width - gap * (_sim.OfferCount + 1)) / _sim.OfferCount;
                for (int i = 0; i < _sim.OfferCount; i++)
                {
                    var rect = new Rect(gap + i * (cardWidth + gap), top, cardWidth, rowHeight);
                    ModuleKind? offer = _sim.OfferAt(i);
                    if (offer == null)
                    {
                        GUI.Label(rect, "sold", _smallStyle);
                        continue;
                    }

                    ModuleDefinition definition = _sim.Content.Module(offer.Value);
                    bool merges = _sim.Ring.FindMergeTarget(offer.Value) != null;
                    string tag = merges ? "MERGE" : definition.Category.ToString().ToUpperInvariant();
                    string label = $"{offer.Value}\n{definition.Cost} cr\n{Describe(offer.Value)}\n<{tag}>";
                    GUI.enabled = _sim.Credits >= definition.Cost;
                    Color previous = GUI.backgroundColor;
                    GUI.backgroundColor = i == _selectedOffer ? Palette.Weapon : previous;
                    if (GUI.Button(rect, label, _cardStyle))
                    {
                        OnOfferTapped(i, merges);
                    }

                    GUI.backgroundColor = previous;
                    GUI.enabled = true;
                }
            }

            float second = top + rowHeight + gap;
            float buttonWidth = (width - gap * 3f) / 2f;
            GUI.enabled = _sim.Credits >= _sim.RerollCost;
            if (GUI.Button(new Rect(gap, second, buttonWidth, 90f), $"Reroll ({_sim.RerollCost})", _buttonStyle))
            {
                _sim.Enqueue(Command.Reroll());
                _selectedOffer = -1;
            }

            GUI.enabled = true;
            if (GUI.Button(new Rect(gap * 2f + buttonWidth, second, buttonWidth, 90f), "Next wave", _buttonStyle))
            {
                _sim.Enqueue(Command.StartWave());
                _selectedOffer = -1;
                _selectedSlot = -1;
                _moveMode = false;
            }
        }

        private void OnOfferTapped(int index, bool merges)
        {
            if (merges)
            {
                _sim.Enqueue(Command.Buy(index, 0));
                _selectedOffer = -1;
                return;
            }

            if (!_sim.Ring.HasFreeSlot())
            {
                ShowMessage("Ring full: sell a module first");
                return;
            }

            _selectedOffer = _selectedOffer == index ? -1 : index;
            _selectedSlot = -1;
            if (_selectedOffer >= 0)
            {
                ShowMessage("Tap a free slot on the ring");
            }
        }

        private void DrawModulePanel(ModuleInstance module, float width, float top, float rowHeight, float gap)
        {
            string stats = module.Category == ModuleCategory.Weapon
                ? $"dmg {FormatDamage(module.EffectiveDamage)}  x{module.DamageMultiplierPermille / 1000f:0.##}\nrange {module.EffectiveRange / (float)SimConstants.Micro:0.#}  every {module.EffectiveCooldown / (float)SimConstants.TicksPerSecond:0.##}s"
                : Describe(module.Kind);
            float infoWidth = width * 0.5f;
            GUI.Label(new Rect(gap, top, infoWidth, rowHeight), $"{module.Kind}  L{module.Level}\n{stats}", _smallStyle);

            float buttonWidth = (width - infoWidth - gap * 4f) / 3f;
            float x = gap * 2f + infoWidth;
            if (GUI.Button(new Rect(x, top, buttonWidth, rowHeight), $"Sell\n+{ModuleRules.SellValue(module.Invested)}", _buttonStyle))
            {
                _sim.Enqueue(Command.Sell(module.Slot));
                _selectedSlot = -1;
            }

            if (GUI.Button(new Rect(x + buttonWidth + gap, top, buttonWidth, rowHeight), _moveMode ? "Tap\nslot" : "Move", _buttonStyle))
            {
                _moveMode = !_moveMode;
            }

            if (GUI.Button(new Rect(x + (buttonWidth + gap) * 2f, top, buttonWidth, rowHeight), "Close", _buttonStyle))
            {
                _selectedSlot = -1;
                _moveMode = false;
            }
        }

        private void DrawWaveControls(float width, float height)
        {
            float top = height * (1f - BottomBarFraction) + 20f;
            float gap = 12f;
            float pulseWidth = width * 0.5f;
            float cooldown = _sim.PulseCooldownRemaining / (float)_sim.Config.PulseCooldownTicks;
            string pulseLabel = _sim.IsPulseReady ? "PULSE\n(or tap the core)" : $"Pulse\n{Mathf.CeilToInt(cooldown * 100f)}%";
            GUI.enabled = _sim.IsPulseReady;
            if (GUI.Button(new Rect(gap, top, pulseWidth - gap, 200f), pulseLabel, _buttonStyle))
            {
                TryPulse();
            }

            GUI.enabled = true;
            float side = (width - pulseWidth - gap * 2f);
            if (GUI.Button(new Rect(pulseWidth + gap, top, side, 94f), $"Speed {_speed}x", _buttonStyle))
            {
                _speed = _speed % 3 + 1;
            }

            if (GUI.Button(new Rect(pulseWidth + gap, top + 106f, side, 94f), _paused ? "Resume" : "Pause", _buttonStyle))
            {
                _paused = !_paused;
            }
        }

        private void DrawGameOver(float width, float height)
        {
            string title = _sim.Phase == GamePhase.Victory ? "VICTORY" : "DEFEAT";
            GUI.Label(new Rect(0f, height * 0.3f, width, 120f), title, _bigStyle);
            GUI.Label(new Rect(0f, height * 0.3f + 130f, width, 120f),
                $"Waves {_sim.WavesCleared}/{_sim.TotalWaves}   Damage {FormatDamage(_sim.TotalDamage)}   Kills {_sim.Kills}\n{_replayStatus}",
                _smallStyle);
            if (GUI.Button(new Rect(width * 0.5f - 170f, height * 0.3f + 280f, 340f, 100f), "Play again", _buttonStyle))
            {
                _seed++;
                StartRun();
            }
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null && _stylesSkin == GUI.skin)
            {
                return;
            }

            _stylesSkin = GUI.skin;
            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 32, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _labelStyle.normal.textColor = Palette.Text;
            _smallStyle = new GUIStyle(_labelStyle) { fontSize = 26 };
            _smallStyle.normal.textColor = Palette.TextDim;
            _bigStyle = new GUIStyle(_labelStyle) { fontSize = 90, fontStyle = FontStyle.Bold };
            _numberStyle = new GUIStyle(_labelStyle) { fontSize = 26 };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 30, wordWrap = true };
            _cardStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, wordWrap = true, alignment = TextAnchor.MiddleCenter };
        }

        // ------------------------------------------------------------------ helpers

        private Vector3 SlotWorld(int slot)
        {
            Directions.PointAt(Directions.OfSlot(slot, _sim.Ring.SlotCount), SimConstants.RingRadius, out long x, out long y);
            return new Vector3(x / (float)SimConstants.Micro, 0f, y / (float)SimConstants.Micro);
        }

        private GameObject CreatePrimitive(PrimitiveType type, string objectName, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = objectName;
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = _unlit;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            SetColor(renderer, color);
            return go;
        }

        private LineRenderer CreateLine(string objectName, float width, Color color, bool loop)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(_root, false);
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _lineMaterial;
            line.useWorldSpace = true;
            line.loop = loop;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = 0;
            return line;
        }

        private void SetColor(Renderer renderer, Color color)
        {
            _block.Clear();
            _block.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(_block);
        }

        private ModuleInstance FindModule(int id)
        {
            for (int slot = 0; slot < _sim.Ring.SlotCount; slot++)
            {
                ModuleInstance module = _sim.Ring.At(slot);
                if (module != null && module.Id == id)
                {
                    return module;
                }
            }

            return null;
        }

        private Enemy FindEnemy(int id)
        {
            foreach (Enemy enemy in _sim.Enemies)
            {
                if (enemy.Id == id)
                {
                    return enemy;
                }
            }

            return null;
        }

        private void ShowMessage(string text)
        {
            _message = text;
            _messageUntil = Time.time + 2f;
        }

        /// <summary>Compact number notation (GDD v0.2 §7): 950 · 1.2K · 3.4M · 5.6B. Input in hundredths of HP.</summary>
        private static string FormatDamage(long hundredths)
        {
            double value = hundredths / (double)SimConstants.HpScale;
            if (value < 1000)
            {
                return value.ToString("0");
            }

            string[] suffixes = { "K", "M", "B", "T" };
            int index = -1;
            while (value >= 1000 && index < suffixes.Length - 1)
            {
                value /= 1000;
                index++;
            }

            return value.ToString("0.#") + suffixes[index];
        }

        private static string Describe(ModuleKind kind)
        {
            return kind switch
            {
                ModuleKind.Emitter => "Shoots the enemy closest to the core",
                ModuleKind.Scatter => "Hits 3 enemies at once",
                ModuleKind.Amplifier => "Neighbours deal x1.5 damage",
                ModuleKind.Lens => "Neighbours: +1.5 range, +2 damage",
                ModuleKind.Overclock => "Neighbours fire 25% faster",
                ModuleKind.Bank => "+1 interest cap, +1 credit per wave",
                ModuleKind.Bulwark => "+25 integrity, repairs 5 per wave",
                _ => kind.ToString(),
            };
        }

        private static string DescribeWave(IReadOnlyList<SpawnEntry> spawns)
        {
            var counts = new SortedDictionary<EnemyKind, int>();
            foreach (SpawnEntry spawn in spawns)
            {
                counts.TryGetValue(spawn.Kind, out int count);
                counts[spawn.Kind] = count + 1;
            }

            var parts = new List<string>();
            foreach (KeyValuePair<EnemyKind, int> pair in counts)
            {
                parts.Add($"{pair.Key} x{pair.Value}");
            }

            return string.Join("   ", parts);
        }

        private static string DescribeRejection(CommandResult result)
        {
            return result switch
            {
                CommandResult.NotEnoughCredits => "Not enough credits",
                CommandResult.SlotOccupied => "That slot is taken",
                CommandResult.PulseNotReady => "Pulse is recharging",
                CommandResult.OfferAlreadyBought => "Already bought",
                CommandResult.InvalidSlot => "Pick a slot on the ring",
                _ => result.ToString(),
            };
        }

        // ------------------------------------------------------------------ view types

        private sealed class ModuleView
        {
            public readonly Transform Root;
            public readonly Vector3 BaseScale;

            public ModuleView(Transform root, Vector3 baseScale)
            {
                Root = root;
                BaseScale = baseScale;
            }
        }

        private sealed class EnemyView
        {
            public readonly GameObject Body;
            public readonly Transform HpBar;
            public readonly float Size;

            public EnemyView(GameObject body, Transform hpBar, float size)
            {
                Body = body;
                HpBar = hpBar;
                Size = size;
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
            public float Age;

            public FloatingNumber(Vector3 position, string text)
            {
                Position = position;
                Text = text;
            }
        }
    }
}
