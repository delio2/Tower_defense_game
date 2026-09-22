using System.Collections.Generic;
using TowerDefense.Presentation.Arena;
using TowerDefense.Presentation.Content;
using TowerDefense.Presentation.Interaction;
using TowerDefense.Presentation.Settings;
using TowerDefense.Presentation.UI;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation
{
    /// <summary>
    /// Prototype driver (GDD §19): owns the deterministic simulation and the run lifecycle, advances ticks, routes
    /// simulation events to the views, and wires the HUD. Drawing lives in <c>Arena/*</c>, pointer input in
    /// <see cref="ShopInput"/>, strings in <see cref="UiText"/>. Visuals only read the simulation; game state changes
    /// exclusively through commands. Visual rule (docs/03 §A1): no flashing, eased motion.
    /// </summary>
    public sealed class PrototypeRunner : MonoBehaviour
    {
        private const int MaxStepsPerFrame = 240;

        /// <summary>Clutter control (docs/03 A8): cap on live line effects, attenuation above this many enemies.</summary>
        private const int MaxLineEffects = 32;
        private const int AttenuateAboveEnemies = 20;

        /// <summary>Calm rule (docs/03 A1): no effect repeats more than twice a second in one spot.</summary>
        private const float MinSecondsBetweenTracers = 0.5f;

        /// <summary>How far inside the panel edge an off-screen marker sits, in panel pixels.</summary>
        private const float EdgeMargin = 48f;

        [SerializeField] private int _seed = 1;

        private GameSimulation _sim;
        private Material _unlit;
        private Material _lineMaterial;
        private Material _surfaceTemplate;
        private ArenaKit _kit;
        private ArenaLighting _lighting;
        private ArenaBackdrop _backdrop;
        private CameraRig _cameraRig;
        private readonly CoreView _core = new CoreView();
        private readonly RingView _ring = new RingView();
        private readonly EnemyViews _enemies = new EnemyViews();
        private readonly EffectsView _effects = new EffectsView();
        private ShopInput _input;
        private HudView _hud;

        private readonly List<SimEvent> _events = new List<SimEvent>();
        private readonly List<FloatingLabel> _floatingLabels = new List<FloatingLabel>();
        private readonly List<Vector2> _edgeMarkers = new List<Vector2>();

        private double _tickAccumulator;
        private int _speed = 1;
        private bool _paused;
        private long _damageAtWaveStart;
        private int _creditsAtWaveStart;
        private string _replayStatus;

        /// <summary>The first run of the session resumes a saved run if there is one; "Play again" starts fresh.</summary>
        private bool _resumeOnStart = true;

        // ------------------------------------------------------------------ lifecycle

        private void Awake() => EnsureInitialized();

        /// <summary>Also rebuilds after a script recompile in Play mode, when non-serialized state is lost.</summary>
        private void EnsureInitialized()
        {
            if (_sim != null && _cameraRig != null && _unlit != null)
            {
                return;
            }

            Application.targetFrameRate = 60;
#if UNITY_EDITOR
            Application.runInBackground = true;
#endif
            _unlit = LoadMaterial("Materials/PrototypeUnlit", "Universal Render Pipeline/Unlit");
            _lineMaterial = LoadMaterial("Materials/PrototypeLines", "Sprites/Default");
            _surfaceTemplate = LoadMaterial("Materials/ThreeSurface", "TowerDefense/ThreeSurface");
            _kit = new ArenaKit(_unlit, _lineMaterial, _surfaceTemplate);
            _cameraRig = new CameraRig();
            _backdrop = new ArenaBackdrop(LoadMaterial("Materials/SkyGround", "TowerDefense/SkyGround"));
            _input = new ShopInput(_kit, _cameraRig, () => _hud, ShowMessage, TryPulse, Inspect);
            SetupHud();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            _lighting = new ArenaLighting(transform, _cameraRig.Camera); // after the cleanup: children are destroyed at frame end
            StartRun();
        }

        /// <summary>
        /// Material assets in Resources keep their shaders in player builds (Shader.Find returns null on the device for
        /// shaders no asset references); the editor fallback covers a missing asset.
        /// </summary>
        private static Material LoadMaterial(string resourcePath, string fallbackShader)
        {
            var template = Resources.Load<Material>(resourcePath);
            if (template != null)
            {
                return new Material(template);
            }

            Debug.LogWarning($"Material {resourcePath} missing in Resources: falling back to Shader.Find({fallbackShader}).");
            Shader shader = Shader.Find(fallbackShader) ?? Shader.Find("Unlit/Color");
            return new Material(shader);
        }

        private void OnDestroy()
        {
            Destroy(_unlit);
            Destroy(_lineMaterial);
            Destroy(_surfaceTemplate);
            _kit?.DestroyOwnedMaterials();
            _lighting?.Dispose();
            _backdrop?.Dispose();
        }

        private void StartRun()
        {
            _enemies.Clear();
            _effects.Clear();
            _events.Clear();
            _tickAccumulator = 0;
            _speed = PlayerOptions.DefaultSpeed;
            _paused = false;
            _input.Reset();
            _replayStatus = null;
            _cameraRig.ResetView();

            ContentDatabase content = ContentLoader.LoadPrototypeOrDefaults();
            _sim = _resumeOnStart ? RunSave.TryResume(content) : null;
            _resumeOnStart = false;
            if (_sim != null)
            {
                _seed = (int)_sim.Config.Seed;
                ShowMessage($"Resumed at wave {_sim.CurrentWave}");
            }
            else
            {
                RunMode mode = PlayerOptions.Endless ? RunMode.Endless : RunMode.Run;
                _sim = new GameSimulation(RunSetup.Create((ulong)_seed, PlayerOptions.Core, PlayerOptions.Grade, mode), content);
            }

            _kit.Sim = _sim;
            _kit.ResetRoot(transform);
            _backdrop.Build(_kit, _cameraRig.Camera);
            _ring.Build(_kit);
            _core.Build(_kit);
            ApplyActTheme();
        }

        private void Update()
        {
            EnsureInitialized();
            float deltaTime = Time.deltaTime;
            _cameraRig.UpdateLayout(_sim.Phase, deltaTime, FurthestEnemyRadius());
            _input.HandlePointer();
            _sim.ApplyPendingCommandsNow();
            AdvanceSimulation(deltaTime);
            ProcessEvents();
            _ring.Sync(_kit, _input.IsHighlighted, deltaTime);
            _ring.ShowReach(_kit, _input.DragReach);
            _enemies.Sync(_kit, deltaTime);
            _effects.Update(deltaTime);
            _core.SetIntegrity(_sim.MaxIntegrity > 0 ? _sim.Integrity / (float)_sim.MaxIntegrity : 0f);
            _core.Update(deltaTime);
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
            _hud.RerollTapped += () => { _sim.Enqueue(Command.Reroll()); _input.DeselectOffer(); };
            _hud.NextWaveTapped += () => { _sim.Enqueue(Command.StartWave()); _input.ClearSelection(); Haptics.Medium(); };
            _hud.UndoTapped += () => { _sim.Enqueue(Command.Undo()); _input.ClearSelection(); };
            _hud.BuySlotTapped += () => _sim.Enqueue(Command.BuySlot(_sim.Ring.SlotCount));
            _hud.SellTapped += () =>
            {
                ModuleInstance module = _sim.Ring.At(_input.SelectedSlot);
                if (module != null)
                {
                    _sim.Enqueue(Command.Sell(module.Slot));
                }

                _input.ClearSelection();
            };
            _hud.MoveTapped += _input.ToggleMoveMode;
            _hud.CloseTapped += _input.ClearSelection;
            _hud.PulseTapped += TryPulse;
            _hud.SpeedTapped += () => _speed = _speed % 3 + 1;
            _hud.PauseTapped += () => _paused = !_paused;
            _hud.PlayAgainTapped += () => { _seed++; RunSave.Clear(); StartRun(); };
        }

        private void SyncHud()
        {
            if (_hud == null)
            {
                return;
            }

            _hud.SetPaused(_paused, _sim, _seed, _speed);
            _effects.FillLabels(_hud, _cameraRig.Camera, _floatingLabels);
            _hud.SetNumbers(_floatingLabels);
            SyncThreats();
            _hud.Refresh(_sim, new HudState
            {
                SelectedOffer = _input.SelectedOffer,
                SelectedSlot = _input.SelectedSlot,
                MoveMode = _input.MoveMode,
                Speed = _speed,
                Paused = _paused,
                ReplayStatus = _replayStatus,
                Seed = _seed,
                PreviewText = _input.PreviewText,
                DragSourceOffer = _input.DragSourceOffer,
                ShowSellZone = _input.IsDraggingModule,
                SellZoneHot = _input.SellZoneHot,
            }, UiText.Describe);
        }

        // ------------------------------------------------------------------ simulation

        private void AdvanceSimulation(float deltaTime)
        {
            if (_paused || _sim.Phase != GamePhase.Wave)
            {
                _tickAccumulator = 0;
                return;
            }

            _tickAccumulator += deltaTime * SimConstants.TicksPerSecond * _speed;
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
                    case SimEventType.EnemySpawned:
                        _hud?.MarkEnemySeen((EnemyKind)e.Value); // met in person: its chip stops saying "new"
                        break;
                    case SimEventType.EnemyHit:
                        OnEnemyHit(e);
                        break;
                    case SimEventType.EnemyKilled:
                        if (_enemies.TryGetPosition(e.EntityId, out Vector3 killedAt))
                        {
                            _effects.SpawnFlakes(_kit, killedAt, Palette.WithAlpha(Palette.Enemy, 0.85f));
                            if (ShowsNumber(e))
                            {
                                _effects.AddNumber(killedAt, UiText.FormatDamage(e.Value), e.Extra == 0);
                            }
                        }

                        break;
                    case SimEventType.CoreHit:
                        _core.OnHit();
                        break;
                    case SimEventType.PulseUsed:
                        _effects.SpawnRing(_kit, _core.Position, 0.8f, _sim.Config.PulseRadius / (float)SimConstants.Micro,
                            Palette.WithAlpha(Palette.Core, 0.5f), 0.6f, 0.05f);
                        _core.OnPulse();
                        break;
                    case SimEventType.ModuleMerged:
                        OnModuleMerged(e);
                        break;
                    case SimEventType.WaveStarted:
                        ApplyActTheme(); // temporary: the act card of Phase 3 will own this switch (docs/06 2.5-B7)
                        _damageAtWaveStart = _sim.TotalDamage;
                        _creditsAtWaveStart = _sim.Credits;
                        break;
                    case SimEventType.WaveCleared:
                        // Credits were just paid: base + interest (+ Guardian bonus); the summary counts them up.
                        if (!_sim.IsOver)
                        {
                            _hud?.ShowWaveSummary((int)e.Value, _sim.TotalDamage - _damageAtWaveStart, _sim.Credits,
                                _sim.Credits - _creditsAtWaveStart, e.Extra, _sim.Ring);
                        }

                        break;
                    case SimEventType.CommandRejected:
                        ShowMessage(UiText.DescribeRejection((CommandResult)e.Extra));
                        break;
                    case SimEventType.ShopOpened:
                        RunSave.Save(_sim); // autosave at every shop (GDD §2)
                        break;
                }
            }
        }

        /// <summary>Merge: the module swells softly and a faint ring expands from it (the "satisfying" moment, docs/03 B4).</summary>
        private void OnModuleMerged(SimEvent e)
        {
            if (_ring.TryMarkMerged(e.EntityId, out Vector3 position))
            {
                _effects.SpawnRing(_kit, position, 0.3f, 1.1f, Palette.WithAlpha(Palette.Booster, 0.45f), 0.45f, 0.04f);
            }

            Haptics.Success();
            ShowMessage($"Level {e.Value}");
        }

        /// <summary>
        /// Numbers are for kills and for hits that take a real bite out of something (docs/09 §2.2): a quarter of the
        /// enemy's maximum health. Chip damage stays silent, so the arena keeps its numbers few and meaningful.
        /// </summary>
        private void ShowHeavyHitNumber(SimEvent hit, Vector3 at)
        {
            if (PlayerOptions.DamageNumbers == DamageNumbersMode.None)
            {
                return;
            }

            Enemy enemy = _kit.FindEnemy(hit.EntityId);
            if (enemy != null && enemy.MaxHp > 0 && hit.Value * 4 >= enemy.MaxHp)
            {
                _effects.AddNumber(at, UiText.FormatDamage(hit.Value), false);
            }
        }

        private static bool ShowsNumber(SimEvent kill)
        {
            switch (PlayerOptions.DamageNumbers)
            {
                case DamageNumbersMode.None:
                    return false;
                case DamageNumbersMode.BigOnly:
                    return kill.Extra == 0 || kill.Value >= 20 * SimConstants.HpScale; // Pulse kills and heavy hits
                default:
                    return true;
            }
        }

        private void OnEnemyHit(SimEvent e)
        {
            if (e.Extra == 0 || !_enemies.TryGetPosition(e.EntityId, out Vector3 enemyAt))
            {
                return;
            }

            ShowHeavyHitNumber(e, enemyAt);

            // Clutter control: a cap on simultaneous tracers, and fainter tracers when the arena is crowded.
            if (_effects.ActiveLines >= Mathf.RoundToInt(MaxLineEffects * PlayerOptions.EffectScale))
            {
                return;
            }

            ModuleInstance module = _kit.FindModule(e.Extra);
            if (module == null)
            {
                return;
            }

            // "No effect repeated more than twice a second in one spot" (docs/03 A1): a fast module still fires every
            // shot, it just does not draw a line for every one of them.
            if (_lastTracer.TryGetValue(module.Id, out float last) && Time.time - last < MinSecondsBetweenTracers)
            {
                return;
            }

            _lastTracer[module.Id] = Time.time;

            // A thin ivory tracer that fades over 0.25 s, as in the mood shot: at the wave framing a teal line at
            // 0.35 alpha disappeared against the floor. Still calm - it is one line, twice a second at most.
            Vector3 from = _kit.SlotWorld(module.Slot) + Vector3.up * 0.3f;
            float crowd = Mathf.Min(1f, AttenuateAboveEnemies / (float)Mathf.Max(1, _sim.Enemies.Count));
            _effects.SpawnBeam(_kit, from, enemyAt, Palette.WithAlpha(Palette.Core, 0.6f * crowd * PlayerOptions.EffectScale), 0.05f);
        }

        /// <summary>Last time each module drew a tracer, for the "at most 2 per second in one spot" rule.</summary>
        private readonly Dictionary<int, float> _lastTracer = new Dictionary<int, float>();

        private void CheckGameOver()
        {
            if (!_sim.IsOver || _replayStatus != null)
            {
                return;
            }

            RunSave.Clear();

            // Every finished run is recorded and re-simulated: the same check the server will do (D19).
            string text = Replay.Record(_sim).Serialize();
            ReplayCheck check = ReplayVerifier.Verify(Replay.Deserialize(text), ContentLoader.LoadPrototypeOrDefaults(),
                () => new RunConfig());
            _replayStatus = check.IsValid
                ? $"Replay verified ({_sim.CommandLog.Count} commands, {text.Length} bytes)"
                : $"Replay MISMATCH: {check.Reason}";
        }

        /// <summary>Sky of the current act (Endless stays in Night): floor gradient, rings, key light, ambient.</summary>
        private void ApplyActTheme()
        {
            int act = Mathf.Clamp((_sim.CurrentWave - 1) / 6, 0, Palette.Acts.Length - 1);
            _lighting.ApplyTheme(Palette.Acts[act]);
            _backdrop.ApplyTheme(Palette.Acts[act]);
        }

        private void TryPulse()
        {
            CommandResult result = _sim.Validate(Command.Pulse());
            if (result == CommandResult.Ok)
            {
                _sim.Enqueue(Command.Pulse());
                Haptics.Medium();
            }
            else
            {
                ShowMessage(UiText.DescribeRejection(result));
            }
        }

        /// <summary>
        /// Threats the player cannot see: an elite or Guardian outside the panel leaves a marker on the edge it is
        /// behind, and a Guardian also owns the strip under the top bar (docs/06 2.5-B6).
        /// </summary>
        private void SyncThreats()
        {
            _edgeMarkers.Clear();
            Enemy guardian = null;
            if (_sim.Phase == GamePhase.Wave)
            {
                Vector2 panelSize = _hud.PanelSize;
                foreach (Enemy enemy in _sim.Enemies)
                {
                    bool worthMarking = enemy.IsElite || enemy.Kind == EnemyKind.Guardian;
                    if (enemy.Kind == EnemyKind.Guardian && (guardian == null || enemy.Hp > guardian.Hp))
                    {
                        guardian = enemy;
                    }

                    if (!worthMarking || !_enemies.TryGetPosition(enemy.Id, out Vector3 at))
                    {
                        continue;
                    }

                    Vector2 panel = _hud.WorldToPanel(_cameraRig.Camera, at);
                    if (panel.x >= EdgeMargin && panel.x <= panelSize.x - EdgeMargin &&
                        panel.y >= EdgeMargin && panel.y <= panelSize.y - EdgeMargin)
                    {
                        continue; // on screen: the enemy speaks for itself
                    }

                    _edgeMarkers.Add(new Vector2(
                        Mathf.Clamp(panel.x, EdgeMargin, panelSize.x - EdgeMargin),
                        Mathf.Clamp(panel.y, EdgeMargin, panelSize.y - EdgeMargin)));
                }
            }

            _hud.SetEdgeMarkers(_edgeMarkers);
            _hud.SetCoreArc(_sim.Phase == GamePhase.Wave ? _hud.WorldToPanel(_cameraRig.Camera, _core.Position) : (Vector2?)null,
                _core.Integrity);
            _hud.SetGuardian(guardian == null ? null : guardian.Kind.ToString(),
                guardian == null || guardian.MaxHp <= 0 ? 0f : guardian.Hp / (float)guardian.MaxHp);
        }

        /// <summary>The offer card under a long press: what the module does and what it costs (docs/06 2.5-C4).</summary>
        private void InspectOffer(int index)
        {
            ModuleKind? offer = _sim.OfferAt(index);
            if (!offer.HasValue)
            {
                _hud.HideInspector();
                return;
            }

            ModuleDefinition definition = _sim.Content.Module(offer.Value);
            _hud.ShowInspector(_hud.CardCentre(index), offer.Value.ToString(), UiText.OfferSheet(definition));
        }

        /// <summary>How far out the furthest living enemy is, in world units, for the wave framing.</summary>
        private float FurthestEnemyRadius()
        {
            long furthest = 0;
            foreach (Enemy enemy in _sim.Enemies)
            {
                furthest = System.Math.Max(furthest, enemy.Radius);
            }

            return furthest / (float)SimConstants.Micro;
        }

        private void ShowMessage(string text) => _hud?.ShowToast(text);

        /// <summary>
        /// A tap during a wave, or a long press in the shop: whatever is under the finger explains itself in a
        /// bubble (docs/06 2.5-B5, 2.5-C4). <paramref name="slot"/> is a ring slot, or -2 - index for an offer card.
        /// Read-only — inspecting never touches the simulation.
        /// </summary>
        private void Inspect(int slot, int enemyId)
        {
            if (_hud == null)
            {
                return;
            }

            if (slot <= -2)
            {
                InspectOffer(-2 - slot);
                return;
            }

            ModuleInstance module = slot >= 0 ? _sim.Ring.At(slot) : null;
            if (module != null)
            {
                Vector2 at = _hud.WorldToPanel(_cameraRig.Camera, _kit.SlotWorld(slot) + Vector3.up * 0.6f);
                string body = _sim.Phase == GamePhase.Shop
                    ? UiText.ModuleSheet(module, _sim.Ring, ModuleRules.SellValue(module.Invested))
                    : UiText.ModuleTooltip(module);
                _hud.ShowInspector(at, UiText.ModuleTitle(module), body);
                return;
            }

            Enemy enemy = enemyId >= 0 ? _kit.FindEnemy(enemyId) : null;
            if (enemy != null && _enemies.TryGetPosition(enemyId, out Vector3 enemyAt))
            {
                Vector2 at = _hud.WorldToPanel(_cameraRig.Camera, enemyAt + Vector3.up * 0.6f);
                _hud.ShowInspector(at, UiText.EnemyTitle(enemy), UiText.EnemyCard(enemy));
                return;
            }

            _hud.HideInspector();
        }
    }
}
