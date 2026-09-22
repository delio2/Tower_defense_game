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

        private readonly VisualElement _root;
        private readonly VisualElement _numbers;
        private readonly Label _integrity;
        private readonly Label _wave;
        private readonly Label _credits;
        private readonly Label _toast;
        private readonly VisualElement _shop;
        private readonly Label _preview;
        private readonly VisualElement _modulePanel;
        private readonly Label _moduleInfo;
        private readonly Button _sell;
        private readonly Button _move;
        private readonly VisualElement _cards;
        private readonly Button _undo;
        private readonly Button _reroll;
        private readonly Button _buySlot;
        private readonly Button _next;
        private readonly VisualElement _waveControls;
        private readonly VisualElement _pulse;
        private readonly VisualElement _pulseFill;
        private readonly Label _pulseLabel;
        private readonly Button _speed;
        private readonly Button _pause;
        private readonly VisualElement _gameOver;
        private readonly Label _gameOverTitle;
        private readonly Label _gameOverStats;
        private readonly VisualElement _options;
        private readonly Toggle _optReduceMotion;
        private readonly SliderInt _optEffects;
        private readonly Button _optNumbers;
        private readonly Toggle _optHaptics;
        private readonly Button _optSpeed;
        private readonly Button _optCore;
        private readonly Button _optGrade;
        private readonly Toggle _optEndless;
        private readonly VisualElement _summary;
        private readonly Label _summaryTitle;
        private readonly Label _summaryDamage;
        private readonly Label _summaryCredits;
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
        private readonly Label _ghostCost;

        /// <summary>The dragged object sits above the finger so it is never covered (docs/03 B3), in panel pixels.</summary>
        private const float GhostLift = 150f;

        private readonly List<VisualElement> _cardViews = new List<VisualElement>();
        private readonly List<Label> _numberPool = new List<Label>();
        private float _toastUntil;
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
            _preview = root.Q<Label>("preview");
            _modulePanel = root.Q<VisualElement>("module-panel");
            _moduleInfo = root.Q<Label>("module-info");
            _sell = root.Q<Button>("sell");
            _move = root.Q<Button>("move");
            _cards = root.Q<VisualElement>("cards");
            _undo = root.Q<Button>("undo");
            _reroll = root.Q<Button>("reroll");
            _buySlot = root.Q<Button>("buy-slot");
            _next = root.Q<Button>("next");
            _waveControls = root.Q<VisualElement>("wave-controls");
            _pulse = root.Q<VisualElement>("pulse");
            _pulseFill = root.Q<VisualElement>("pulse-fill");
            _pulseLabel = root.Q<Label>("pulse-label");
            _speed = root.Q<Button>("speed");
            _pause = root.Q<Button>("pause");
            _gameOver = root.Q<VisualElement>("game-over");
            _gameOverTitle = root.Q<Label>("game-over-title");
            _gameOverStats = root.Q<Label>("game-over-stats");
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
            _summary.RegisterCallback<PointerDownEvent>(_ => SkipSummary());
            _sellZone = root.Q<VisualElement>("sell-zone");
            _ghost = root.Q<VisualElement>("drag-ghost");
            _ghostName = root.Q<Label>("ghost-name");
            _ghostCost = root.Q<Label>("ghost-cost");

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
        public void ShowGhost(string title, string cost, Vector2 screenPosition)
        {
            _ghostHide?.Pause();
            _ghostHide = null;
            _ghost.RemoveFromClassList("drag-ghost--returning");
            Show(_ghost, true);
            SetText(_ghostName, title);
            SetText(_ghostCost, cost);
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
        public void ShowWaveSummary(int wave, long waveDamage, int creditsAfter, int creditsGained, int interest)
        {
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

        private void OpenOptions()
        {
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
        }

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);

        public void ShowToast(string text)
        {
            _toast.text = text;
            _toastUntil = Time.time + ToastSeconds;
            _toast.AddToClassList("toast--visible");
        }

        /// <summary>Converts a world point to panel coordinates for floating labels.</summary>
        public Vector2 WorldToPanel(Camera camera, Vector3 world)
        {
            Vector3 screen = camera.WorldToScreenPoint(world);
            return RuntimePanelUtils.ScreenToPanel(_root.panel, new Vector2(screen.x, Screen.height - screen.y));
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

            if (sim.IsOver)
            {
                string title = sim.Phase == GamePhase.Victory ? "Victory"
                    : sim.HasWon ? $"Endless: wave {sim.WavesCleared}"
                    : $"Wave {sim.WavesCleared + 1} of {sim.TotalWaves}";
                SetText(_gameOverTitle, title);
                string stopped = sim.DefeatedBy.HasValue ? $"Stopped by: {sim.DefeatedBy.Value}\n" : string.Empty;
                SetText(_gameOverStats, $"{stopped}Total damage {NumberFormat.CompactHundredths(sim.TotalDamage)}   Kills {sim.Kills}\nSeed {state.Seed}\n{state.ReplayStatus}");
            }
        }

        private void RefreshTopBar(GameSimulation sim)
        {
            long integrity = sim.Integrity / SimConstants.HpScale;
            SetText(_integrity, $"♥ {integrity}");
            _integrity.EnableInClassList("top-number--warning", sim.Integrity * 4 <= sim.MaxIntegrity);
            bool endless = sim.Config.Endless && sim.HasWon;
            string label = sim.IsGuardianWave ? "Guardian" : "Wave";
            SetText(_wave, endless ? $"{label} {sim.CurrentWave} (endless)" : $"{label} {sim.CurrentWave}/{sim.TotalWaves}");
            SetText(_credits, sim.Credits.ToString());
        }

        private void RefreshShop(GameSimulation sim, HudState state, Func<ModuleKind, string> describe)
        {
            SetText(_preview, "Next: " + DescribeWave(sim.NextWavePreview));

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
            SetText(_reroll, $"Reroll {sim.RerollCost}");
            _reroll.SetEnabled(sim.Credits >= sim.RerollCost);
            Show(_buySlot, sim.CanBuyExtraSlot);
            SetText(_buySlot, $"Slot +1 ({sim.ExtraSlotCost})");
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
                SetText(card.Q<Label>("name"), offer.Value.ToString());
                SetText(card.Q<Label>("effect"), describe(offer.Value));
                var cost = card.Q<Label>("cost");
                SetText(cost, definition.Cost.ToString());
                cost.EnableInClassList("card__cost--unaffordable", !affordable);
                var badge = card.Q<Label>("badge");
                SetText(badge, merges ? $"Merge L{sim.Ring.FindMergeTarget(offer.Value).Level + 1}" : string.Empty);
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

        private static VisualElement BuildCard()
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            var badge = new Label { name = "badge", pickingMode = PickingMode.Ignore };
            badge.AddToClassList("card__badge");
            var icon = new VisualElement { name = "icon", pickingMode = PickingMode.Ignore };
            icon.AddToClassList("card__icon");
            var name = new Label { name = "name", pickingMode = PickingMode.Ignore };
            name.AddToClassList("card__name");
            var effect = new Label { name = "effect", pickingMode = PickingMode.Ignore };
            effect.AddToClassList("card__effect");
            var cost = new Label { name = "cost", pickingMode = PickingMode.Ignore };
            cost.AddToClassList("card__cost");
            card.Add(badge);
            card.Add(icon);
            card.Add(name);
            card.Add(effect);
            card.Add(cost);
            return card;
        }

        private void RefreshWaveControls(GameSimulation sim, HudState state)
        {
            float cooldown = sim.Config.PulseCooldownTicks == 0 ? 0f : sim.PulseCooldownRemaining / (float)sim.Config.PulseCooldownTicks;
            _pulse.EnableInClassList("pulse--ready", sim.IsPulseReady);
            _pulseFill.style.height = Length.Percent((1f - cooldown) * 100f);
            int seconds = Mathf.CeilToInt(sim.PulseCooldownRemaining / (float)SimConstants.TicksPerSecond);
            SetText(_pulseLabel, sim.IsPulseReady ? "Pulse" : seconds <= 5 ? seconds.ToString() : "…");
            SetText(_speed, $"{state.Speed}x");
            SetText(_pause, state.Paused ? "Resume" : "Pause");
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
