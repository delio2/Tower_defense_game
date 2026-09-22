using System;
using System.Collections.Generic;
using TowerDefense.Presentation.Settings;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>What the HUD needs from the runner besides the simulation (selection, speed, pause, run status).</summary>
    public struct HudState
    {
        public int SelectedOffer;
        public int SelectedSlot;
        public bool MoveMode;
        public int Speed;
        public bool Paused;
        public string ReplayStatus;
        public int Seed;

        /// <summary>Text shown above the cards while dragging ("DPS 16 → 24 (+50%)"); null hides it.</summary>
        public string PreviewText;

        /// <summary>Offer index being dragged (dimmed), or -1.</summary>
        public int DragSourceOffer;

        /// <summary>True while a ring module is dragged: shows the Sell zone.</summary>
        public bool ShowSellZone;
        public bool SellZoneHot;
    }

    /// <summary>One module's contribution to a wave, for the summary bars.</summary>
    internal readonly struct ModuleShare
    {
        public ModuleShare(ModuleKind kind, long damage, float share)
        {
            Kind = kind;
            Damage = damage;
            Share = share;
        }

        public ModuleKind Kind { get; }
        public long Damage { get; }
        public float Share { get; }
    }

    /// <summary>How many of an enemy kind a wave sends, and how many of those are elites.</summary>
    internal readonly struct ThreatCount
    {
        public ThreatCount(int total, int elites)
        {
            Total = total;
            Elites = elites;
        }

        public int Total { get; }
        public int Elites { get; }
    }

    /// <summary>A damage number to draw this frame, in panel coordinates.</summary>
    public struct FloatingLabel
    {
        public Vector2 PanelPosition;
        public string Text;
        public float Alpha;
        public bool Big;
    }

    /// <summary>
    /// UI Toolkit HUD (docs/09): reads the simulation and raises intents; never changes game state itself.
    /// Structure comes from Hud.uxml, tokens from Theme.uss. Only user intents are exposed, as events.
    /// </summary>
    public sealed class HudView
    {
        public event Action RerollTapped;
        public event Action NextWaveTapped;
        public event Action UndoTapped;
        public event Action BuySlotTapped;
        public event Action SellTapped;
        public event Action MoveTapped;
        public event Action CloseTapped;
        public event Action PulseTapped;
        public event Action SpeedTapped;
        public event Action PauseTapped;
        public event Action PlayAgainTapped;

        /// <summary>Raised after any option changes (values are already saved in <see cref="PlayerOptions"/>).</summary>
        public event Action OptionsChanged;

        private const float ToastSeconds = 2f;

        /// <summary>How long a tapped module or enemy keeps its bubble on screen (docs/09 §2.2).</summary>
        private const float InspectorSeconds = 2.5f;

        /// <summary>How long "abandon" has to be held before it counts (docs/07 §1.2).</summary>
        private const float AbandonHoldSeconds = 1f;

        /// <summary>How long the run-end totals take to roll up.</summary>
        private const float RunEndRoll = 1.2f;

        /// <summary>How many modules the wave summary lists.</summary>
        private const int MaxShares = 4;

        /// <summary>Gap between one card arriving and the next (docs/09 section 4.5).</summary>
        private const long CardStaggerMilliseconds = 40;

        /// <summary>Sides the spawn compass splits the arena into.</summary>
        private const int CompassSectors = 8;

        /// <summary>Width and height of the Integrity ring element, matching .core-arc in the stylesheet.</summary>
        private const float CoreArcSize = 300f;

        /// <summary>How long the Guardian's name stays before the strip becomes only its health bar.</summary>
        private const float GuardianNameSeconds = 2f;

        private readonly VisualElement _root;
        private readonly VisualElement _numbers;
        private readonly Label _integrity;
        private readonly Label _wave;
        private readonly Label _credits;
        private readonly Label _toast;
        private readonly VisualElement _shop;
        private readonly VisualElement _threats;
        private readonly VisualElement _compass;
        private readonly List<VisualElement> _threatPool = new List<VisualElement>();

        /// <summary>Share of the next wave entering from each of eight sides, for the compass.</summary>
        private readonly float[] _compassShare = new float[CompassSectors];
        private readonly VisualElement _modulePanel;
        private readonly Label _moduleInfo;
        private readonly Button _sell;
        private readonly Button _move;
        private readonly VisualElement _cards;
        private readonly Button _undo;
        private readonly Button _reroll;
        private readonly Button _buySlot;
        private readonly Label _rerollCost;
        private readonly Label _buySlotCost;
        private readonly Button _next;
        private readonly VisualElement _waveControls;
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
        private readonly VisualElement _waveProgress;
        private readonly VisualElement _waveProgressFill;
        private readonly VisualElement _pulse;
        private readonly VisualElement _pulseHalo;
        private readonly VisualElement _pulseRing;
        private readonly Label _pulseLabel;

        /// <summary>Share of the cooldown already served, 0..1; the ring paints it.</summary>
        private float _pulseCharge;
        private readonly Button _speed;
        private readonly Button _pause;
        private readonly VisualElement _gameOver;
        private readonly Label _gameOverTitle;
        private readonly Label _gameOverStats;
        private readonly VisualElement _stoppedBy;
        private readonly VisualElement _stoppedIcon;
        private readonly Label _stoppedLabel;
        private readonly Label _totalDamage;
        private readonly Label _totalKills;

        /// <summary>When the run-end panel opened, so its totals can roll up like the wave summary does.</summary>
        private float _runEndStart = -1f;
        private readonly VisualElement _options;
        private readonly Toggle _optReduceMotion;
        private readonly SliderInt _optEffects;
        private readonly Button _optNumbers;
        private readonly Toggle _optHaptics;
        private readonly Button _optSpeed;
        private readonly Button _optCore;
        private readonly Button _optGrade;
        private readonly Toggle _optEndless;
        private readonly Toggle _optLargeText;
        private readonly Toggle _optContrast;
        private readonly VisualElement _pauseSheet;
        private readonly Label _pauseInfo;
        private readonly Toggle _pauseReduceMotion;
        private readonly SliderInt _pauseEffects;
        private readonly Button _pauseNumbers;
        private readonly Button _pauseSpeed;
        private readonly Button _pauseAbandon;
        private readonly VisualElement _summary;
        private readonly Label _summaryTitle;
        private readonly Label _summaryDamage;
        private readonly Label _summaryCredits;
        private readonly VisualElement _summaryShares;
        private readonly List<VisualElement> _sharePool = new List<VisualElement>();
        private readonly List<ModuleShare> _shares = new List<ModuleShare>();
        private float _summaryStart = -1f;
        private int _summaryWave;
        private long _summaryDamageValue;
        private int _summaryCreditsAfter;
        private int _summaryCreditsGained;
        private int _summaryInterest;

        /// <summary>Summary timeline (docs/03 B4): damage rolls 0–0.8 s, coins add 0.8–1.6 s, closes at 2.2 s; a tap skips.</summary>
        private const float SummaryDamageEnd = 0.8f;
        private const float SummaryCoinsEnd = 1.6f;
        private const float SummaryEnd = 2.2f;
        private readonly Label _previewEffect;
        private readonly VisualElement _sellZone;
        private readonly VisualElement _ghost;
        private readonly Label _ghostName;
        private readonly VisualElement _ghostArt;

        /// <summary>The dragged object sits above the finger so it is never covered (docs/03 B3), in panel pixels.</summary>
        private const float GhostLift = 150f;

        private readonly List<VisualElement> _cardViews = new List<VisualElement>();
        private readonly List<Label> _numberPool = new List<Label>();
        private readonly SortedDictionary<EnemyKind, ThreatCount> _threatCounts = new SortedDictionary<EnemyKind, ThreatCount>();
        private readonly HashSet<EnemyKind> _seenEnemies = new HashSet<EnemyKind>();
        private float _toastUntil;

        /// <summary>When the abandon button went down, or -1 when nothing is being held.</summary>
        private float _abandonStart = -1f;
        private int _pauseSpeedValue = 1;

        /// <summary>Phase the HUD last drew, to catch the moment the shop opens and stagger the cards in.</summary>
        private GamePhase _lastPhase = GamePhase.Wave;
        private IVisualElementScheduledItem _ghostHide;

        public HudView(VisualElement root)
        {
            _root = root;
            _numbers = root.Q<VisualElement>("numbers");
            _integrity = root.Q<Label>("integrity");
            _wave = root.Q<Label>("wave");
            _credits = root.Q<Label>("credits");
            _toast = root.Q<Label>("toast");
            _shop = root.Q<VisualElement>("shop");
            _threats = root.Q<VisualElement>("threats");
            _compass = root.Q<VisualElement>("compass");
            _compass.generateVisualContent += PaintCompass;
            _modulePanel = root.Q<VisualElement>("module-panel");
            _moduleInfo = root.Q<Label>("module-info");
            _sell = root.Q<Button>("sell");
            _move = root.Q<Button>("move");
            _cards = root.Q<VisualElement>("cards");
            _undo = root.Q<Button>("undo");
            _reroll = root.Q<Button>("reroll");
            _buySlot = root.Q<Button>("buy-slot");
            _rerollCost = root.Q<Label>("reroll-cost");
            _buySlotCost = root.Q<Label>("buy-slot-cost");
            _next = root.Q<Button>("next");
            _waveControls = root.Q<VisualElement>("wave-controls");
            _coreArc = root.Q<VisualElement>("core-arc");
            _coreArc.generateVisualContent += PaintCoreArc;
            _edgeMarkers = root.Q<VisualElement>("edge-markers");
            _guardian = root.Q<VisualElement>("guardian");
            _guardianName = root.Q<Label>("guardian-name");
            _guardianFill = root.Q<VisualElement>("guardian-fill");
            _inspector = root.Q<VisualElement>("inspector");
            _inspectorTitle = root.Q<Label>("inspector-title");
            _inspectorBody = root.Q<Label>("inspector-body");
            _waveProgress = root.Q<VisualElement>("wave-progress");
            _waveProgressFill = root.Q<VisualElement>("wave-progress-fill");
            _pulse = root.Q<VisualElement>("pulse");
            _pulseHalo = root.Q<VisualElement>("pulse-halo");
            _pulseRing = root.Q<VisualElement>("pulse-ring");
            _pulseRing.generateVisualContent += PaintPulseRing;
            _pulseLabel = root.Q<Label>("pulse-label");
            _speed = root.Q<Button>("speed");
            _pause = root.Q<Button>("pause");
            _gameOver = root.Q<VisualElement>("game-over");
            _gameOverTitle = root.Q<Label>("game-over-title");
            _gameOverStats = root.Q<Label>("game-over-stats");
            _stoppedBy = root.Q<VisualElement>("stopped-by");
            _stoppedIcon = root.Q<VisualElement>("stopped-icon");
            _stoppedLabel = root.Q<Label>("stopped-label");
            _totalDamage = root.Q<Label>("total-damage");
            _totalKills = root.Q<Label>("total-kills");
            root.Q<Button>("same-seed").clicked += () => ShowToast("Same seed arrives with the Daily (Phase 4)");
            root.Q<Button>("share-run").clicked += () => ShowToast("Replay codes arrive in Phase 4");
            _previewEffect = root.Q<Label>("preview-effect");
            _options = root.Q<VisualElement>("options");
            _optReduceMotion = root.Q<Toggle>("opt-reduce-motion");
            _optEffects = root.Q<SliderInt>("opt-effects");
            _optNumbers = root.Q<Button>("opt-numbers");
            _optHaptics = root.Q<Toggle>("opt-haptics");
            _optSpeed = root.Q<Button>("opt-speed");
            _optCore = root.Q<Button>("opt-core");
            _optGrade = root.Q<Button>("opt-grade");
            _optEndless = root.Q<Toggle>("opt-endless");
            _optCore.clicked += () => { PlayerOptions.Core = (CoreType)(((int)PlayerOptions.Core + 1) % 4); RefreshOptionLabels(); };
            _optGrade.clicked += () => { PlayerOptions.Grade = (PlayerOptions.Grade + 1) % 4; RefreshOptionLabels(); };
            _optEndless.RegisterValueChangedCallback(e => PlayerOptions.Endless = e.newValue);
            _optLargeText = root.Q<Toggle>("opt-large-text");
            _optContrast = root.Q<Toggle>("opt-contrast");
            _optLargeText.RegisterValueChangedCallback(e => { PlayerOptions.LargeText = e.newValue; ApplyAccessibility(); });
            _optContrast.RegisterValueChangedCallback(e => { PlayerOptions.HighContrast = e.newValue; ApplyAccessibility(); });

            _pauseSheet = root.Q<VisualElement>("pause-sheet");
            _pauseInfo = root.Q<Label>("pause-info");
            _pauseReduceMotion = root.Q<Toggle>("pause-reduce-motion");
            _pauseEffects = root.Q<SliderInt>("pause-effects");
            _pauseNumbers = root.Q<Button>("pause-numbers");
            _pauseSpeed = root.Q<Button>("pause-speed");
            _pauseAbandon = root.Q<Button>("pause-abandon");
            _pauseReduceMotion.RegisterValueChangedCallback(e => { PlayerOptions.ReduceMotion = e.newValue; OptionsChanged?.Invoke(); });
            _pauseEffects.RegisterValueChangedCallback(e => { PlayerOptions.EffectIntensity = e.newValue; OptionsChanged?.Invoke(); });
            _pauseNumbers.clicked += () =>
            {
                PlayerOptions.DamageNumbers = (DamageNumbersMode)(((int)PlayerOptions.DamageNumbers + 1) % 3);
                RefreshPauseLabels();
                OptionsChanged?.Invoke();
            };

            _pauseSpeed.clicked += () => SpeedTapped?.Invoke();
            root.Q<Button>("pause-resume").clicked += () => PauseTapped?.Invoke();
            SetUpHoldToAbandon();
            ApplyAccessibility();
            root.Q<Button>("options-open").clicked += OpenOptions;
            root.Q<Button>("options-close").clicked += CloseOptions;
            _optReduceMotion.RegisterValueChangedCallback(e => { PlayerOptions.ReduceMotion = e.newValue; OptionsChanged?.Invoke(); });
            _optEffects.RegisterValueChangedCallback(e => { PlayerOptions.EffectIntensity = e.newValue; OptionsChanged?.Invoke(); });
            _optHaptics.RegisterValueChangedCallback(e => { PlayerOptions.Haptics = e.newValue; OptionsChanged?.Invoke(); });
            _optNumbers.clicked += () =>
            {
                PlayerOptions.DamageNumbers = (DamageNumbersMode)(((int)PlayerOptions.DamageNumbers + 1) % 3);
                RefreshOptionLabels();
                OptionsChanged?.Invoke();
            };
            _optSpeed.clicked += () =>
            {
                PlayerOptions.DefaultSpeed = PlayerOptions.DefaultSpeed % 3 + 1;
                RefreshOptionLabels();
                OptionsChanged?.Invoke();
            };
            _summary = root.Q<VisualElement>("wave-summary");
            _summaryTitle = root.Q<Label>("summary-title");
            _summaryDamage = root.Q<Label>("summary-damage");
            _summaryCredits = root.Q<Label>("summary-credits");
            _summaryShares = root.Q<VisualElement>("summary-shares");
            _summary.RegisterCallback<PointerDownEvent>(_ => SkipSummary());
            _sellZone = root.Q<VisualElement>("sell-zone");
            _ghost = root.Q<VisualElement>("drag-ghost");
            _ghostName = root.Q<Label>("ghost-name");
            _ghostArt = root.Q<VisualElement>("ghost-art");

            _sell.clicked += () => SellTapped?.Invoke();
            _move.clicked += () => MoveTapped?.Invoke();
            root.Q<Button>("close").clicked += () => CloseTapped?.Invoke();
            _undo.clicked += () => UndoTapped?.Invoke();
            _reroll.clicked += () => RerollTapped?.Invoke();
            _buySlot.clicked += () => BuySlotTapped?.Invoke();
            _next.clicked += () => NextWaveTapped?.Invoke();
            _pulse.RegisterCallback<ClickEvent>(_ => PulseTapped?.Invoke());
            _speed.clicked += () => SpeedTapped?.Invoke();
            _pause.clicked += () => PauseTapped?.Invoke();
            root.Q<Button>("play-again").clicked += () => PlayAgainTapped?.Invoke();
        }

        /// <summary>True when a screen point (bottom-left origin) is over an interactive element of the HUD.</summary>
        public bool IsPointerOver(Vector2 screenPosition)
        {
            IPanel panel = _root.panel;
            if (panel == null)
            {
                return false;
            }

            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            VisualElement picked = panel.Pick(panelPosition);
            return picked != null && picked != _root;
        }

        private Vector2 ToPanel(Vector2 screenPosition)
        {
            return RuntimePanelUtils.ScreenToPanel(_root.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
        }

        /// <summary>Offer card under a screen point, or -1. Sold cards do not count.</summary>
        public int CardAt(Vector2 screenPosition)
        {
            if (_root.panel == null || IsSummaryOpen || IsOptionsOpen || _cards.ClassListContains("hidden") || _shop.ClassListContains("hidden"))
            {
                return -1;
            }

            Vector2 panelPosition = ToPanel(screenPosition);
            for (int i = 0; i < _cardViews.Count; i++)
            {
                VisualElement card = _cardViews[i];
                if (!card.ClassListContains("card--sold") && card.worldBound.Contains(panelPosition))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool IsOverSellZone(Vector2 screenPosition)
        {
            return _root.panel != null && !_sellZone.ClassListContains("hidden") && _sellZone.worldBound.Contains(ToPanel(screenPosition));
        }

        /// <summary>Panel position of a card centre, to float a ghost back to it.</summary>
        public Vector2 CardCentre(int index)
        {
            return index >= 0 && index < _cardViews.Count ? _cardViews[index].worldBound.center : Vector2.zero;
        }

        /// <summary>Shows the drag ghost above the finger.</summary>
        public void ShowGhost(string title, ModuleKind kind, Vector2 screenPosition)
        {
            _ghostHide?.Pause();
            _ghostHide = null;
            _ghost.RemoveFromClassList("drag-ghost--returning");
            Show(_ghost, true);
            SetText(_ghostName, title);
            Texture2D art = CardArt(kind);
            _ghostArt.style.backgroundImage = art != null ? new StyleBackground(art) : new StyleBackground(StyleKeyword.None);
            PlaceGhost(ToPanel(screenPosition) + new Vector2(0f, -GhostLift));
        }

        /// <summary>Hides the ghost; with a return point it floats back first (soft return, no error flash).</summary>
        public void HideGhost(Vector2? returnToPanel = null)
        {
            if (returnToPanel.HasValue && !_ghost.ClassListContains("hidden"))
            {
                _ghost.AddToClassList("drag-ghost--returning");
                PlaceGhost(returnToPanel.Value);
                _ghostHide?.Pause();
                _ghostHide = _ghost.schedule.Execute(() => Show(_ghost, false));
                _ghostHide.ExecuteLater(260);
                return;
            }

            Show(_ghost, false);
        }

        private void PlaceGhost(Vector2 panelCentre)
        {
            _ghost.style.left = panelCentre.x - 120f;
            _ghost.style.top = panelCentre.y - 100f;
        }

        /// <summary>Opens the end-of-wave summary; the shop stays hidden until it closes or is tapped.</summary>
        public void ShowWaveSummary(int wave, long waveDamage, int creditsAfter, int creditsGained, int interest,
            Ring ring)
        {
            CollectShares(ring);
            _summaryStart = Time.time;
            _summaryWave = wave;
            _summaryDamageValue = waveDamage;
            _summaryCreditsAfter = creditsAfter;
            _summaryCreditsGained = creditsGained;
            _summaryInterest = interest;
            Show(_summary, true);
        }

        public bool IsSummaryOpen => _summaryStart >= 0f;

        public bool IsOptionsOpen => !_options.ClassListContains("hidden");

        /// <summary>
        /// Turns the two accessibility settings into classes on the root: the stylesheet does the rest, because
        /// both are only a different set of the same variables (docs/09 §6).
        /// </summary>
        private void ApplyAccessibility()
        {
            _root.EnableInClassList("text-large", PlayerOptions.LargeText);
            _root.EnableInClassList("theme-contrast", PlayerOptions.HighContrast);
            PlayerOptions.Save();
        }

        /// <summary>
        /// Abandoning a run is irreversible, so it is held rather than tapped (docs/09 §0 rule 3). The button fills
        /// while the finger stays down and only fires at the end.
        /// </summary>
        private void SetUpHoldToAbandon()
        {
            _pauseAbandon.RegisterCallback<PointerDownEvent>(_ =>
            {
                _abandonStart = Time.unscaledTime;
                _pauseAbandon.schedule.Execute(TickAbandon).Every(50).Until(() => _abandonStart < 0f);
            });
            _pauseAbandon.RegisterCallback<PointerUpEvent>(_ => CancelAbandon());
            _pauseAbandon.RegisterCallback<PointerLeaveEvent>(_ => CancelAbandon());
        }

        private void TickAbandon()
        {
            if (_abandonStart < 0f)
            {
                return;
            }

            float held = (Time.unscaledTime - _abandonStart) / AbandonHoldSeconds;
            SetText(_pauseAbandon, held >= 1f ? "Abandoned" : $"Hold to abandon  {Mathf.RoundToInt(held * 100f)}%");
            if (held >= 1f)
            {
                CancelAbandon();
                PlayAgainTapped?.Invoke();
            }
        }

        private void CancelAbandon()
        {
            _abandonStart = -1f;
            SetText(_pauseAbandon, "Hold to abandon");
        }

        /// <summary>Shows the pause sheet with what this run is (docs/09 §2.8), or puts it away.</summary>
        public void SetPaused(bool paused, GameSimulation sim, int seed, int speed)
        {
            Show(_pauseSheet, paused);
            if (!paused)
            {
                CancelAbandon();
                return;
            }

            _pauseReduceMotion.SetValueWithoutNotify(PlayerOptions.ReduceMotion);
            _pauseEffects.SetValueWithoutNotify(PlayerOptions.EffectIntensity);
            SetText(_pauseInfo, $"Wave {sim.CurrentWave} of {sim.TotalWaves}\n{PlayerOptions.Core}, grade {PlayerOptions.Grade}\nSeed {seed}");
            _pauseSpeedValue = speed;
            RefreshPauseLabels();
        }

        private void RefreshPauseLabels()
        {
            SetText(_pauseNumbers, PlayerOptions.DamageNumbers switch
            {
                DamageNumbersMode.BigOnly => "Big only",
                DamageNumbersMode.None => "None",
                _ => "All",
            });
            SetText(_pauseSpeed, $"{_pauseSpeedValue}x");
        }

        private void OpenOptions()
        {
            _optLargeText.SetValueWithoutNotify(PlayerOptions.LargeText);
            _optContrast.SetValueWithoutNotify(PlayerOptions.HighContrast);
            _optReduceMotion.SetValueWithoutNotify(PlayerOptions.ReduceMotion);
            _optEffects.SetValueWithoutNotify(PlayerOptions.EffectIntensity);
            _optHaptics.SetValueWithoutNotify(PlayerOptions.Haptics);
            _optEndless.SetValueWithoutNotify(PlayerOptions.Endless);
            RefreshOptionLabels();
            Show(_options, true);
        }

        private void CloseOptions()
        {
            PlayerOptions.Save();
            Show(_options, false);
        }

        private void RefreshOptionLabels()
        {
            SetText(_optNumbers, PlayerOptions.DamageNumbers switch
            {
                DamageNumbersMode.BigOnly => "Big only",
                DamageNumbersMode.None => "None",
                _ => "All",
            });
            SetText(_optSpeed, $"{PlayerOptions.DefaultSpeed}x");
            SetText(_optCore, PlayerOptions.Core.ToString());
            SetText(_optGrade, PlayerOptions.Grade == 0 ? "Grade 0" : $"Grade {PlayerOptions.Grade}");
        }

        private void SkipSummary()
        {
            _summaryStart = -1f;
            Show(_summary, false);
        }

        private void RefreshSummary(GameSimulation sim)
        {
            if (!IsSummaryOpen)
            {
                return;
            }

            float t = Time.time - _summaryStart;
            if (t >= SummaryEnd)
            {
                SkipSummary();
                return;
            }

            int gained = _summaryCreditsGained;
            SetText(_summaryTitle, $"Wave {_summaryWave} cleared");
            float damageT = Mathf.Clamp01(t / SummaryDamageEnd);
            long shownDamage = (long)(_summaryDamageValue * EaseOut(damageT));
            SetText(_summaryDamage, $"Damage {NumberFormat.CompactHundredths(shownDamage)}");
            float coinT = Mathf.Clamp01((t - SummaryDamageEnd) / (SummaryCoinsEnd - SummaryDamageEnd));
            int shownGain = Mathf.RoundToInt(gained * coinT);
            string interest = _summaryInterest > 0 && coinT >= 1f ? $"  (+{_summaryInterest} interest)" : string.Empty;
            SetText(_summaryCredits, $"Credits {_summaryCreditsAfter - gained + shownGain}   +{shownGain}{interest}");

            // The bars fill last, after the numbers have landed.
            float shareT = Mathf.Clamp01((t - SummaryCoinsEnd) / (SummaryEnd - SummaryCoinsEnd));
            for (int i = 0; i < _shares.Count && i < _sharePool.Count; i++)
            {
                VisualElement track = _sharePool[i][1];
                track[0].style.width = Length.Percent(_shares[i].Share * 100f * EaseOut(shareT));
            }
        }

        /// <summary>
        /// The modules that did damage this wave, biggest first, as shares of the total. Only the top few: the
        /// panel answers "what carried this wave", not "here is a table".
        /// </summary>
        private void CollectShares(Ring ring)
        {
            _shares.Clear();
            long total = 0;
            for (int slot = 0; slot < ring.SlotCount; slot++)
            {
                ModuleInstance module = ring.At(slot);
                if (module != null && module.DamageThisWave > 0)
                {
                    total += module.DamageThisWave;
                    _shares.Add(new ModuleShare(module.Kind, module.DamageThisWave, 0f));
                }
            }

            _shares.Sort((a, b) => b.Damage.CompareTo(a.Damage));
            if (_shares.Count > MaxShares)
            {
                _shares.RemoveRange(MaxShares, _shares.Count - MaxShares);
            }

            for (int i = 0; i < _shares.Count; i++)
            {
                _shares[i] = new ModuleShare(_shares[i].Kind, _shares[i].Damage,
                    total > 0 ? _shares[i].Damage / (float)total : 0f);
            }

            for (int i = 0; i < _shares.Count; i++)
            {
                while (_sharePool.Count <= i)
                {
                    VisualElement row = BuildShareRow();
                    _summaryShares.Add(row);
                    _sharePool.Add(row);
                }

                VisualElement built = _sharePool[i];
                built.style.display = DisplayStyle.Flex;
                SetText((Label)built[0], $"{_shares[i].Kind}  {Mathf.RoundToInt(_shares[i].Share * 100f)}%");
                built[1][0].style.width = Length.Percent(0f);
            }

            for (int i = _shares.Count; i < _sharePool.Count; i++)
            {
                _sharePool[i].style.display = DisplayStyle.None;
            }
        }

        private static VisualElement BuildShareRow()
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList("share");
            var name = new Label { pickingMode = PickingMode.Ignore };
            name.AddToClassList("share__name");
            var track = new VisualElement { pickingMode = PickingMode.Ignore };
            track.AddToClassList("share__track");
            var fill = new VisualElement { pickingMode = PickingMode.Ignore };
            fill.AddToClassList("share__fill");
            track.Add(fill);
            row.Add(name);
            row.Add(track);
            return row;
        }

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);

        public void ShowToast(string text)
        {
            _toast.text = text;
            _toastUntil = Time.time + ToastSeconds;
            _toast.AddToClassList("toast--visible");
        }

        /// <summary>Converts a world point to panel coordinates for floating labels.</summary>
        /// <summary>Size of the panel in its own coordinates (1080 x 1920 at the reference resolution).</summary>
        public Vector2 PanelSize => _root.layout.size;

        public Vector2 WorldToPanel(Camera camera, Vector3 world)
        {
            Vector3 screen = camera.WorldToScreenPoint(world);
            return RuntimePanelUtils.ScreenToPanel(_root.panel, new Vector2(screen.x, Screen.height - screen.y));
        }

        /// <summary>
        /// Puts the Integrity ring around the Core's projection: <paramref name="panelPosition"/> is where the Core
        /// is on screen, <paramref name="fraction"/> how much health is left. A null position hides it.
        /// </summary>
        public void SetCoreArc(Vector2? panelPosition, float fraction)
        {
            Show(_coreArc, panelPosition.HasValue);
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
        /// Pooled like the damage numbers: a wave never allocates.
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
            Show(_guardian, present);
            if (!present)
            {
                _guardianNameUntil = 0f;
                return;
            }

            if (_guardianName.text != guardianName)
            {
                SetText(_guardianName, guardianName);
                _guardianNameUntil = Time.time + GuardianNameSeconds;
            }

            Show(_guardianName, Time.time < _guardianNameUntil);
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

        public void Refresh(GameSimulation sim, HudState state, Func<ModuleKind, string> describe)
        {
            RefreshTopBar(sim);
            if (Time.time >= _toastUntil)
            {
                _toast.RemoveFromClassList("toast--visible");
            }

            RefreshSummary(sim);
            bool shop = sim.Phase == GamePhase.Shop;
            Show(_shop, shop && !sim.IsOver && !IsSummaryOpen);
            Show(_waveControls, sim.Phase == GamePhase.Wave);
            Show(_gameOver, sim.IsOver);

            if (shop)
            {
                RefreshShop(sim, state, describe);
            }
            else if (sim.Phase == GamePhase.Wave)
            {
                RefreshWaveControls(sim, state);
            }

            if (!sim.IsOver)
            {
                _runEndStart = -1f; // a new run: the totals roll again when it ends
            }

            if (sim.IsOver)
            {
                string title = sim.Phase == GamePhase.Victory ? "Victory"
                    : sim.HasWon ? $"Endless: wave {sim.WavesCleared}"
                    : $"Wave {sim.WavesCleared + 1} of {sim.TotalWaves}";
                SetText(_gameOverTitle, title);
                if (_runEndStart < 0f)
                {
                    _runEndStart = Time.time;
                }

                bool stopped = sim.DefeatedBy.HasValue;
                Show(_stoppedBy, stopped);
                if (stopped)
                {
                    SetText(_stoppedLabel, $"Stopped by {sim.DefeatedBy.Value}");
                    foreach (string style in ThreatIconClasses)
                    {
                        _stoppedIcon.EnableInClassList(style, style == ThreatIconClass(sim.DefeatedBy.Value));
                    }
                }

                // The totals roll up over RunEndRoll seconds, the same beat as the wave summary.
                float roll = EaseOut(Mathf.Clamp01((Time.time - _runEndStart) / RunEndRoll));
                SetText(_totalDamage, $"Damage {NumberFormat.CompactHundredths((long)(sim.TotalDamage * roll))}");
                SetText(_totalKills, $"Kills {Mathf.RoundToInt(sim.Kills * roll)}");
                SetText(_gameOverStats, $"Seed {state.Seed}\n{state.ReplayStatus}");
            }
        }

        private void RefreshTopBar(GameSimulation sim)
        {
            RefreshWaveProgress(sim);
            RefreshInspector();
            if (sim.Phase != _lastPhase)
            {
                if (sim.Phase == GamePhase.Shop)
                {
                    StaggerCardsIn();
                }

                _lastPhase = sim.Phase;
            }

            long integrity = sim.Integrity / SimConstants.HpScale;
            SetText(_integrity, integrity.ToString()); // the heart is the drawn icon next to it, not a glyph
            _integrity.EnableInClassList("top-number--warning", sim.Integrity * 4 <= sim.MaxIntegrity);
            bool endless = sim.Config.Endless && sim.HasWon;
            string label = sim.IsGuardianWave ? "Guardian" : "Wave";
            SetText(_wave, endless ? $"{label} {sim.CurrentWave} (endless)" : $"{label} {sim.CurrentWave}/{sim.TotalWaves}");
            SetText(_credits, sim.Credits.ToString());
        }

        /// <summary>
        /// The hairline under the top bar: how much of the wave has been dealt with, spawns and survivors together
        /// (docs/09 §2.2). Hidden outside a wave, so the shop keeps its calm top edge.
        /// </summary>
        /// <summary>
        /// Shows the read-only bubble above a point of the arena for a couple of seconds (docs/09 §2.2). The caller
        /// passes panel coordinates, so it works for a module on the ring and for a moving enemy alike.
        /// </summary>
        public void ShowInspector(Vector2 panelPosition, string title, string body)
        {
            SetText(_inspectorTitle, title);
            SetText(_inspectorBody, body);
            _inspector.style.left = panelPosition.x - 240f;
            _inspector.style.top = Mathf.Max(200f, panelPosition.y - 210f);
            Show(_inspector, true);
            _inspectorUntil = Time.time + InspectorSeconds;
        }

        public void HideInspector()
        {
            Show(_inspector, false);
            _inspectorUntil = 0f;
        }

        private void RefreshInspector()
        {
            if (_inspectorUntil > 0f && Time.time >= _inspectorUntil)
            {
                HideInspector();
            }
        }

        private void RefreshWaveProgress(GameSimulation sim)
        {
            bool inWave = sim.Phase == GamePhase.Wave && sim.WaveEnemyCount > 0;
            Show(_waveProgress, inWave);
            if (!inWave)
            {
                return;
            }

            float done = 1f - sim.WaveEnemiesLeft / (float)sim.WaveEnemyCount;
            _waveProgressFill.style.width = Length.Percent(Mathf.Clamp01(done) * 100f);
        }

        private void RefreshShop(GameSimulation sim, HudState state, Func<ModuleKind, string> describe)
        {
            RefreshWavePreview(sim);

            ModuleInstance selected = sim.Ring.At(state.SelectedSlot);
            Show(_modulePanel, selected != null);
            Show(_cards, selected == null);
            if (selected != null)
            {
                string stats = selected.Category == ModuleCategory.Weapon
                    ? $"damage {NumberFormat.CompactHundredths(selected.EffectiveDamage)} ×{selected.DamageMultiplierPermille / 1000f:0.##}\nrange {selected.EffectiveRange / (float)SimConstants.Micro:0.#} · every {selected.EffectiveCooldown / (float)SimConstants.TicksPerSecond:0.##} s"
                    : describe(selected.Kind);
                SetText(_moduleInfo, $"{selected.Kind}  L{selected.Level}\n{stats}");
                SetText(_sell, $"Sell\n+{ModuleRules.SellValue(selected.Invested)}");
                SetText(_move, state.MoveMode ? "Tap a\nslot" : "Move");
            }
            else
            {
                RefreshCards(sim, state, describe);
            }

            Show(_previewEffect, !string.IsNullOrEmpty(state.PreviewText));
            SetText(_previewEffect, state.PreviewText ?? string.Empty);
            Show(_sellZone, state.ShowSellZone);
            _sellZone.EnableInClassList("sell-zone--hot", state.SellZoneHot);
            _undo.SetEnabled(sim.CanUndo);
            SetText(_rerollCost, sim.RerollCost.ToString());
            _reroll.SetEnabled(sim.Credits >= sim.RerollCost);
            Show(_buySlot, sim.CanBuyExtraSlot);
            SetText(_buySlotCost, sim.ExtraSlotCost.ToString());
            _buySlot.SetEnabled(sim.Credits >= sim.ExtraSlotCost);
        }

        private void RefreshCards(GameSimulation sim, HudState state, Func<ModuleKind, string> describe)
        {
            while (_cardViews.Count < sim.OfferCount)
            {
                VisualElement card = BuildCard();
                _cards.Add(card);
                _cardViews.Add(card);
            }

            for (int i = 0; i < _cardViews.Count; i++)
            {
                VisualElement card = _cardViews[i];
                ModuleKind? offer = i < sim.OfferCount ? sim.OfferAt(i) : null;
                card.EnableInClassList("card--sold", offer == null);
                if (offer == null)
                {
                    continue;
                }

                ModuleDefinition definition = sim.Content.Module(offer.Value);
                bool merges = sim.Ring.FindMergeTarget(offer.Value) != null;
                bool affordable = sim.Credits >= definition.Cost;
                FitName(card.Q<Label>("name"), offer.Value.ToString());

                int reach = ReachOf(definition);
                VisualElement reachRow = card.Q<VisualElement>("reach");
                Show(reachRow, reach > 0);
                for (int band = 0; band < ReachBands; band++)
                {
                    reachRow[band].EnableInClassList("card__dot--on", band < reach);
                }

                VisualElement glyph = card.Q<VisualElement>("glyph");
                glyph.EnableInClassList("card__glyph--weapon", definition.Category == ModuleCategory.Weapon);
                glyph.EnableInClassList("card__glyph--booster", definition.Category == ModuleCategory.Booster);
                glyph.EnableInClassList("card__glyph--economy", definition.Category == ModuleCategory.Economy);

                var cost = card.Q<Label>("cost");
                SetText(cost, definition.Cost.ToString());
                cost.EnableInClassList("card__cost--unaffordable", !affordable);
                VisualElement badge = card.Q<VisualElement>("badge");
                Show(badge, merges);
                if (merges)
                {
                    SetText(card.Q<Label>("badge-level"), $"L{sim.Ring.FindMergeTarget(offer.Value).Level + 1}");
                }
                var icon = card.Q<VisualElement>("icon");
                Texture2D art = CardArt(offer.Value);
                icon.style.backgroundImage = art != null ? new StyleBackground(art) : new StyleBackground(StyleKeyword.None);
                icon.EnableInClassList("card__icon--art", art != null);
                icon.EnableInClassList("card__icon--weapon", art == null && definition.Category == ModuleCategory.Weapon);
                icon.EnableInClassList("card__icon--booster", art == null && definition.Category == ModuleCategory.Booster);
                icon.EnableInClassList("card__icon--economy", art == null && definition.Category == ModuleCategory.Economy);
                card.EnableInClassList("card--uncommon", definition.Rarity == Rarity.Uncommon);
                card.EnableInClassList("card--rare", definition.Rarity == Rarity.Rare);
                card.EnableInClassList("card--disabled", !affordable);
                card.EnableInClassList("card--selected", i == state.SelectedOffer);
                card.EnableInClassList("card--source", i == state.DragSourceOffer);
            }
        }

        private static readonly Dictionary<ModuleKind, Texture2D> CardArtCache = new Dictionary<ModuleKind, Texture2D>();

        /// <summary>Card art rendered from the Blender models with the game's light (Assets/UI/Resources/Cards, docs/03 A10).</summary>
        private static Texture2D CardArt(ModuleKind kind)
        {
            if (!CardArtCache.TryGetValue(kind, out Texture2D art))
            {
                art = Resources.Load<Texture2D>($"Cards/{kind}");
                CardArtCache[kind] = art;
            }

            return art;
        }

        /// <summary>
        /// Offer card v2 (docs/09 §2.3): the model render, the name, a category glyph in the corner, reach dots for
        /// the range bands it covers (D32), the cost beside the Credits icon, and the merge badge. The effect
        /// sentence is deliberately gone: at card width it wrapped to four lines. It lives in the drag preview and
        /// in the module panel instead.
        /// </summary>
        private static VisualElement BuildCard()
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            var badge = new VisualElement { name = "badge", pickingMode = PickingMode.Ignore };
            badge.AddToClassList("card__badge");
            var badgeIcon = new VisualElement { pickingMode = PickingMode.Ignore };
            badgeIcon.AddToClassList("card__badge-icon");
            var badgeLevel = new Label { name = "badge-level", pickingMode = PickingMode.Ignore };
            badgeLevel.AddToClassList("card__badge-level");
            badge.Add(badgeIcon);
            badge.Add(badgeLevel);
            var glyph = new VisualElement { name = "glyph", pickingMode = PickingMode.Ignore };
            glyph.AddToClassList("card__glyph");
            var icon = new VisualElement { name = "icon", pickingMode = PickingMode.Ignore };
            icon.AddToClassList("card__icon");
            var name = new Label { name = "name", pickingMode = PickingMode.Ignore };
            name.AddToClassList("card__name");

            var reach = new VisualElement { name = "reach", pickingMode = PickingMode.Ignore };
            reach.AddToClassList("card__reach");
            for (int band = 0; band < ReachBands; band++)
            {
                var dot = new VisualElement { name = "dot" + band, pickingMode = PickingMode.Ignore };
                dot.AddToClassList("card__dot");
                reach.Add(dot);
            }

            var costRow = new VisualElement { name = "cost-row", pickingMode = PickingMode.Ignore };
            costRow.AddToClassList("card__cost-row");
            var costIcon = new VisualElement { pickingMode = PickingMode.Ignore };
            costIcon.AddToClassList("icon");
            costIcon.AddToClassList("icon--credits");
            costIcon.AddToClassList("icon--small");
            var cost = new Label { name = "cost", pickingMode = PickingMode.Ignore };
            cost.AddToClassList("card__cost");
            costRow.Add(costIcon);
            costRow.Add(cost);

            card.Add(badge);
            card.Add(glyph);
            card.Add(icon);
            card.Add(name);
            card.Add(reach);
            card.Add(costRow);
            return card;
        }

        /// <summary>
        /// Writes a module name and shrinks it until it fits its card on one line. A fixed size cannot work: the
        /// card is a share of the screen width, so the same 34 px that fits a 16:9 panel clipped "Overclock" on a
        /// taller phone. Steps down rather than scaling smoothly, so names stay visually consistent.
        /// </summary>
        private static void FitName(Label label, string text)
        {
            SetText(label, text);
            float available = label.resolvedStyle.width;
            if (available <= 1f)
            {
                return; // first frame: no layout yet, the next refresh sizes it
            }

            foreach (float size in NameSizes)
            {
                label.style.fontSize = size;
                if (label.MeasureTextSize(text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x <= available)
                {
                    return;
                }
            }
        }

        /// <summary>Sizes a card name may take, largest first (the design system's caption size and below).</summary>
        private static readonly float[] NameSizes = { 36f, 34f, 30f, 27f };

        /// <summary>Range bands a weapon can cover from the ring: short, mid, far (docs/12 §1, D32).</summary>
        private const int ReachBands = 3;
        private static readonly float[] BandEdges = { 3.0f, 5.5f, 7.5f };

        /// <summary>
        /// How many bands a module reaches, measured outward from the ring it sits on. Boosters and economy modules
        /// have no reach: their dots stay off.
        /// </summary>
        private static int ReachOf(ModuleDefinition definition)
        {
            if (definition.Category != ModuleCategory.Weapon)
            {
                return 0;
            }

            float ring = SimConstants.RingRadius / (float)SimConstants.Micro;
            float outer = ring + definition.RangeMilli / 1000f;
            int bands = 0;
            foreach (float edge in BandEdges)
            {
                if (outer >= edge)
                {
                    bands++;
                }
            }

            return Mathf.Max(1, bands);
        }

        private void RefreshWaveControls(GameSimulation sim, HudState state)
        {
            float cooldown = sim.Config.PulseCooldownTicks == 0 ? 0f : sim.PulseCooldownRemaining / (float)sim.Config.PulseCooldownTicks;
            bool ready = sim.IsPulseReady;
            _pulse.EnableInClassList("pulse--ready", ready);

            float charge = 1f - cooldown;
            if (Mathf.Abs(charge - _pulseCharge) > 0.002f)
            {
                _pulseCharge = charge;
                _pulseRing.MarkDirtyRepaint();
            }

            // Ready: the edge breathes between 0.3 and 0.6 over two seconds (docs/09 §2.2). Still when motion is reduced.
            float halo = 0f;
            if (ready)
            {
                halo = PlayerOptions.ReduceMotion ? 0.45f : 0.45f + 0.15f * Mathf.Sin(Time.time * Mathf.PI);
            }

            _pulseHalo.style.opacity = halo;

            int seconds = Mathf.CeilToInt(sim.PulseCooldownRemaining / (float)SimConstants.TicksPerSecond);
            SetText(_pulseLabel, ready ? "Pulse" : seconds <= 5 ? seconds.ToString() : "…");
            SetText(_speed, $"{state.Speed}x");
            SetText(_pause, string.Empty);
            _pause.EnableInClassList("btn--icon", true);
            _pause.EnableInClassList("btn--icon-resume", state.Paused);
        }

        /// <summary>
        /// The cooldown ring: a full circle of track with the charged part drawn over it, clockwise from the top.
        /// Painted rather than styled because USS has no arcs.
        /// </summary>
        private void PaintPulseRing(MeshGenerationContext context)
        {
            Rect rect = context.visualElement.contentRect;
            if (rect.width <= 1f || rect.height <= 1f)
            {
                return;
            }

            var centre = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f - 6f;
            Painter2D painter = context.painter2D;
            painter.lineWidth = 9f;
            painter.lineCap = LineCap.Butt;

            painter.strokeColor = new Color(0.56f, 0.54f, 0.62f, 0.55f); // --ivory-40, faint: the empty track
            painter.BeginPath();
            painter.Arc(centre, radius, 0f, 360f);
            painter.Stroke();

            if (_pulseCharge <= 0.001f)
            {
                return;
            }

            painter.strokeColor = _pulse.ClassListContains("pulse--ready") ? new Color(0.08f, 0.09f, 0.19f) : new Color(0.95f, 0.91f, 0.84f);
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.Arc(centre, radius, -90f, -90f + 360f * Mathf.Clamp01(_pulseCharge));
            painter.Stroke();
        }

        /// <summary>
        /// The threat chips and the spawn compass (docs/09 section 2.3). "New" marks a kind never met before, the
        /// one piece of information worth interrupting the calm for.
        /// </summary>
        private void RefreshWavePreview(GameSimulation sim)
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
            Show(_compass, spawns.Count > 0);

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
                foreach (string style in ThreatIconClasses)
                {
                    icon.EnableInClassList(style, style == ThreatIconClass(pair.Key));
                }

                icon.EnableInClassList("threat__icon--elite", pair.Value.Elites > 0);
                SetText((Label)chip[1], pair.Value.Total.ToString());
            }

            for (int i = index; i < _threatPool.Count; i++)
            {
                _threatPool[i].style.display = DisplayStyle.None;
            }
        }

        /// <summary>Marks a kind as met, so the preview chip stops calling it new (called when one spawns).</summary>
        public void MarkEnemySeen(EnemyKind kind) => _seenEnemies.Add(kind);

        /// <summary>Drops the offer cards in 40 ms apart when the shop opens (docs/09 section 4.5).</summary>
        private void StaggerCardsIn()
        {
            for (int i = 0; i < _cardViews.Count; i++)
            {
                VisualElement card = _cardViews[i];
                card.AddToClassList("card--entering");
                long delay = CardStaggerMilliseconds * i;
                card.schedule.Execute(() => card.RemoveFromClassList("card--entering")).ExecuteLater(delay + 1);
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

        /// <summary>Every icon class a threat chip can wear; the stylesheet holds the drawings (docs/art/icons).</summary>
        private static readonly string[] ThreatIconClasses =
        {
            "threat__icon--enemy", "threat__icon--swarm", "threat__icon--dash",
            "threat__icon--split", "threat__icon--shield", "threat__icon--guardian",
        };

        private static string ThreatIconClass(EnemyKind kind)
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

        private static string DescribeWave(IReadOnlyList<SpawnEntry> spawns)
        {
            if (spawns.Count == 0)
            {
                return "—";
            }

            var counts = new SortedDictionary<EnemyKind, int>();
            foreach (SpawnEntry entry in spawns)
            {
                counts[entry.Kind] = counts.TryGetValue(entry.Kind, out int n) ? n + 1 : 1;
            }

            var parts = new List<string>();
            foreach (KeyValuePair<EnemyKind, int> pair in counts)
            {
                parts.Add($"{pair.Value} {pair.Key}");
            }

            return string.Join(" · ", parts);
        }

        private static void Show(VisualElement element, bool visible) => element.EnableInClassList("hidden", !visible);

        private static void SetText(TextElement element, string text)
        {
            if (element.text != text)
            {
                element.text = text;
            }
        }
    }
}
