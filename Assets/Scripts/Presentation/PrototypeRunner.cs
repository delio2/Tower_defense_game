using System.Collections.Generic;
using TowerDefense.Presentation.Content;
using TowerDefense.Presentation.UI;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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
        /// <summary>Wave view: the whole arena. Shop view: close-up on the ring so slots are comfortable to tap (48dp+).</summary>
        private const float ArenaViewRadius = 9.4f;
        private const float ShopViewRadius = 3.4f;
        private const float SlotPickRadius = 0.6f;
        private const float CorePickRadius = 0.9f;

        /// <summary>Magnet radius around a slot centre, world units (docs/07 §1.2).</summary>
        private const float MagnetRadius = 0.45f;

        /// <summary>Drag threshold: 8 dp (docs/07 §1.2), converted to screen pixels from the panel scale.</summary>
        private const float DragThresholdDp = 8f;

        private enum DragKind : byte { None, Offer, Module }
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
        private DragKind _pendingKind;
        private DragKind _dragKind;
        private int _dragIndex = -1;
        private Vector2 _pressScreen;
        private int _dragTargetSlot = -1;
        private bool _dragOverSell;
        private string _previewText;
        private long _damageAtWaveStart;
        private int _creditsAtWaveStart;
        private float _coreWarning;
        private float _viewRadius = ShopViewRadius;
        private string _replayStatus;

        private HudView _hud;
        private readonly List<FloatingLabel> _floatingLabels = new List<FloatingLabel>();

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
            SetupHud();
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
            ResetDrag();
            _replayStatus = null;
            _viewRadius = ShopViewRadius;

            _sim = new GameSimulation(new RunConfig { Seed = (ulong)_seed }, ContentLoader.LoadPrototypeOrDefaults());
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
            SyncHud();
        }

        // ------------------------------------------------------------------ HUD (UI Toolkit, docs/09)

        private void SetupHud()
        {
            var document = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            document.panelSettings = Resources.Load<PanelSettings>("PanelSettings");
            document.visualTreeAsset = Resources.Load<VisualTreeAsset>("Hud");
            if (document.panelSettings == null || document.visualTreeAsset == null)
            {
                Debug.LogError("HUD assets missing in Assets/UI/Resources (PanelSettings, Hud.uxml).");
                return;
            }

            _hud = new HudView(document.rootVisualElement);
            _hud.RerollTapped += () => { _sim.Enqueue(Command.Reroll()); _selectedOffer = -1; };
            _hud.NextWaveTapped += () => { _sim.Enqueue(Command.StartWave()); ClearSelection(); };
            _hud.UndoTapped += () => { _sim.Enqueue(Command.Undo()); ClearSelection(); };
            _hud.BuySlotTapped += () => _sim.Enqueue(Command.BuySlot(_sim.Ring.SlotCount));
            _hud.SellTapped += () =>
            {
                ModuleInstance module = _sim.Ring.At(_selectedSlot);
                if (module != null)
                {
                    _sim.Enqueue(Command.Sell(module.Slot));
                }

                ClearSelection();
            };
            _hud.MoveTapped += () => _moveMode = !_moveMode;
            _hud.CloseTapped += ClearSelection;
            _hud.PulseTapped += TryPulse;
            _hud.SpeedTapped += () => _speed = _speed % 3 + 1;
            _hud.PauseTapped += () => _paused = !_paused;
            _hud.PlayAgainTapped += () => { _seed++; StartRun(); };
        }

        private void ClearSelection()
        {
            _selectedOffer = -1;
            _selectedSlot = -1;
            _moveMode = false;
        }

        private void SyncHud()
        {
            if (_hud == null)
            {
                return;
            }

            _floatingLabels.Clear();
            foreach (FloatingNumber number in _numbers)
            {
                _floatingLabels.Add(new FloatingLabel
                {
                    PanelPosition = _hud.WorldToPanel(_camera, number.Position + Vector3.forward * (number.Age * 0.6f)),
                    Text = number.Text,
                    Alpha = (1f - number.Age / 0.9f) * 0.8f,
                    Big = number.Big,
                });
            }

            _hud.SetNumbers(_floatingLabels);
            _hud.Refresh(_sim, new HudState
            {
                SelectedOffer = _selectedOffer,
                SelectedSlot = _selectedSlot,
                MoveMode = _moveMode,
                Speed = _speed,
                Paused = _paused,
                ReplayStatus = _replayStatus,
                Seed = _seed,
                PreviewText = _previewText,
                DragSourceOffer = _dragKind == DragKind.Offer ? _dragIndex : -1,
                ShowSellZone = _dragKind == DragKind.Module,
                SellZoneHot = _dragOverSell,
            }, Describe);
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
                            _numbers.Add(new FloatingNumber(killed.Body.transform.position, FormatDamage(e.Value), e.Extra == 0));
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
                        OnModuleMerged(e);
                        break;
                    case SimEventType.WaveStarted:
                        _damageAtWaveStart = _sim.TotalDamage;
                        _creditsAtWaveStart = _sim.Credits;
                        break;
                    case SimEventType.WaveCleared:
                        // Credits were just paid: base + interest (+ Guardian bonus); the summary counts them up.
                        if (!_sim.IsOver)
                        {
                            _hud?.ShowWaveSummary((int)e.Value, _sim.TotalDamage - _damageAtWaveStart, _sim.Credits, _sim.Credits - _creditsAtWaveStart, e.Extra);
                        }

                        break;
                    case SimEventType.CommandRejected:
                        ShowMessage(DescribeRejection((CommandResult)e.Extra));
                        break;
                }
            }
        }

        /// <summary>Merge: the module swells softly and a faint ring expands from it (the "satisfying" moment, docs/03 B4).</summary>
        private void OnModuleMerged(SimEvent e)
        {
            if (_moduleViews.TryGetValue(e.EntityId, out ModuleView view))
            {
                view.MergeAt = Time.time;
                SpawnRing(view.Root.position, 0.3f, 1.1f, Palette.WithAlpha(Palette.Booster, 0.45f), 0.45f, 0.04f);
            }

            ShowMessage($"Level {e.Value}");
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
            ReplayCheck check = ReplayVerifier.Verify(Replay.Deserialize(text), ContentLoader.LoadPrototypeOrDefaults(),
                () => new RunConfig());
            _replayStatus = check.IsValid
                ? $"Replay verified ({_sim.CommandLog.Count} commands, {text.Length} bytes)"
                : $"Replay MISMATCH: {check.Reason}";
        }

        // ------------------------------------------------------------------ input

        private void HandlePointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            Vector2 screen = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame)
            {
                OnPress(screen);
            }
            else if (pointer.press.isPressed && _pendingKind != DragKind.None)
            {
                OnHold(screen);
            }
            else if (pointer.press.wasReleasedThisFrame && _pendingKind != DragKind.None)
            {
                OnRelease(screen);
            }
        }

        private void OnPress(Vector2 screen)
        {
            if (_sim.IsOver)
            {
                return;
            }

            _pressScreen = screen;
            if (_sim.Phase == GamePhase.Shop)
            {
                int card = _hud != null ? _hud.CardAt(screen) : -1;
                if (card >= 0)
                {
                    _pendingKind = DragKind.Offer;
                    _dragIndex = card;
                    return;
                }
            }

            if (IsOverUi(screen))
            {
                return; // buttons are handled by UI Toolkit
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
            if (slot >= 0 && _sim.Ring.At(slot) != null && !_moveMode && _selectedOffer < 0)
            {
                // A press on a module may become a drag; a plain tap selects it on release.
                _pendingKind = DragKind.Module;
                _dragIndex = slot;
                return;
            }

            TapArena(slot);
        }

        /// <summary>The pre-drag shop interaction: tap a card, then a slot; or Move mode.</summary>
        private void TapArena(int slot)
        {
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

        private void OnHold(Vector2 screen)
        {
            if (_dragKind == DragKind.None)
            {
                float thresholdPixels = DragThresholdDp * Screen.height / 640f; // 1920 px reference = 640 dp
                if ((screen - _pressScreen).sqrMagnitude < thresholdPixels * thresholdPixels)
                {
                    return;
                }

                _dragKind = _pendingKind;
                ClearSelection();
            }

            Vector3 world = ScreenToWorld(screen);
            _dragTargetSlot = NearestSlot(world, MagnetRadius);
            _dragOverSell = _dragKind == DragKind.Module && _hud != null && _hud.IsOverSellZone(screen);
            _previewText = BuildPreview();
            if (_hud == null)
            {
                return;
            }

            if (_dragKind == DragKind.Offer)
            {
                ModuleKind? offer = _sim.OfferAt(_dragIndex);
                if (offer == null)
                {
                    CancelDrag();
                    return;
                }

                _hud.ShowGhost(offer.Value.ToString(), _sim.Content.Module(offer.Value).Cost.ToString(), screen);
            }
            else
            {
                ModuleInstance module = _sim.Ring.At(_dragIndex);
                if (module == null)
                {
                    CancelDrag();
                    return;
                }

                _hud.ShowGhost($"{module.Kind} L{module.Level}", string.Empty, screen);
            }
        }

        private void OnRelease(Vector2 screen)
        {
            if (_dragKind == DragKind.None)
            {
                // A tap: cards select an offer (or merge at once); modules open their panel.
                if (_pendingKind == DragKind.Offer)
                {
                    OnOfferTapped(_dragIndex);
                }
                else
                {
                    TapArena(_dragIndex);
                }

                ResetDrag();
                return;
            }

            bool dropped = false;
            if (_dragKind == DragKind.Offer)
            {
                int slot = _dragTargetSlot >= 0 ? _dragTargetSlot : MergeTargetSlot(_dragIndex);
                if (slot >= 0)
                {
                    CommandResult result = _sim.Validate(Command.Buy(_dragIndex, slot));
                    if (result == CommandResult.Ok)
                    {
                        _sim.Enqueue(Command.Buy(_dragIndex, slot));
                        dropped = true;
                    }
                    else
                    {
                        ShowMessage(DescribeRejection(result));
                    }
                }
            }
            else if (_dragOverSell)
            {
                _sim.Enqueue(Command.Sell(_dragIndex));
                dropped = true;
            }
            else if (_dragTargetSlot >= 0 && _dragTargetSlot != _dragIndex)
            {
                _sim.Enqueue(Command.Move(_dragIndex, _dragTargetSlot));
                dropped = true;
            }

            if (dropped)
            {
                _hud?.HideGhost();
            }
            else
            {
                CancelDrag();
                return;
            }

            ResetDrag();
        }

        /// <summary>Invalid release: the ghost floats back to where it came from (docs/03 B3 rule 5).</summary>
        private void CancelDrag()
        {
            if (_hud != null)
            {
                Vector2 origin = _dragKind == DragKind.Offer
                    ? _hud.CardCentre(_dragIndex)
                    : _hud.WorldToPanel(_camera, SlotWorld(_dragIndex));
                _hud.HideGhost(origin);
            }

            ResetDrag();
        }

        private void ResetDrag()
        {
            _pendingKind = DragKind.None;
            _dragKind = DragKind.None;
            _dragIndex = -1;
            _dragTargetSlot = -1;
            _dragOverSell = false;
            _previewText = null;
        }

        /// <summary>Slot of the module a card would merge into, or -1.</summary>
        private int MergeTargetSlot(int offer)
        {
            ModuleKind? kind = _sim.OfferAt(offer);
            return kind.HasValue ? _sim.Ring.FindMergeTarget(kind.Value)?.Slot ?? -1 : -1;
        }

        /// <summary>The effect of the pending drop, computed by the simulation previews (what you see is what happens).</summary>
        private string BuildPreview()
        {
            long before = _sim.RingDps();
            if (_dragKind == DragKind.Offer)
            {
                int slot = _dragTargetSlot >= 0 ? _dragTargetSlot : MergeTargetSlot(_dragIndex);
                if (slot < 0)
                {
                    return null;
                }

                if (!_sim.TryPreviewBuy(_dragIndex, slot, out long after, out bool merges))
                {
                    return DescribeRejection(_sim.Validate(Command.Buy(_dragIndex, slot)));
                }

                ModuleInstance target = merges ? _sim.Ring.At(slot) : null;
                string level = target != null ? $"Level {target.Level + 1} · " : string.Empty;
                return level + DpsChange(before, after);
            }

            if (_dragOverSell && _sim.TryPreviewSell(_dragIndex, out long afterSell, out int refund))
            {
                return $"Sell: +{refund} · " + DpsChange(before, afterSell);
            }

            if (_dragTargetSlot >= 0 && _dragTargetSlot != _dragIndex && _sim.TryPreviewMove(_dragIndex, _dragTargetSlot, out long afterMove))
            {
                return DpsChange(before, afterMove);
            }

            return null;
        }

        private static string DpsChange(long before, long after)
        {
            string from = NumberFormat.CompactHundredths(before);
            string to = NumberFormat.CompactHundredths(after);
            if (before <= 0)
            {
                return $"DPS {from} → {to}";
            }

            long percent = (after - before) * 100 / before;
            string sign = percent >= 0 ? "+" : string.Empty;
            return $"DPS {from} → {to} ({sign}{percent}%)";
        }

        private int NearestSlot(Vector3 world, float radius)
        {
            int best = -1;
            float bestDistance = radius;
            for (int slot = 0; slot < _sim.Ring.SlotCount; slot++)
            {
                float distance = Vector3.Distance(new Vector3(world.x, 0f, world.z), SlotWorld(slot));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = slot;
                }
            }

            return best;
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

        private bool IsOverUi(Vector2 screen) => _hud != null && _hud.IsPointerOver(screen);

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
                bool validTarget = _dragKind == DragKind.Offer
                    ? (ring.At(slot) == null && MergeTargetSlot(_dragIndex) < 0) || slot == MergeTargetSlot(_dragIndex)
                    : _dragKind == DragKind.Module && slot != _dragIndex;
                bool highlighted = slot == _selectedSlot || slot == _dragTargetSlot || validTarget
                    || (_selectedOffer >= 0 && ring.At(slot) == null);
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
                float merge = view.MergeAt >= 0f ? Mathf.Clamp01((Time.time - view.MergeAt) / 0.15f) : 1f;
                float swell = 1f + 0.15f * (1f - merge) * (1f - merge); // ease-out back to 1
                view.Root.localScale = view.BaseScale * levelScale * swell;
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

        // ------------------------------------------------------------------ shop intents

        private void OnOfferTapped(int index)
        {
            ModuleKind? offer = _sim.OfferAt(index);
            if (offer == null)
            {
                return;
            }

            bool merges = _sim.Ring.FindMergeTarget(offer.Value) != null;
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

        private void ShowMessage(string text) => _hud?.ShowToast(text);

        private static string FormatDamage(long hundredths) => NumberFormat.CompactHundredths(hundredths);

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

            /// <summary>Time.time when a merge swelled this module (scale 1.15 → 1 over 0.15 s, docs/03 A7).</summary>
            public float MergeAt = -1f;

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
