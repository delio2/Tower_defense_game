using System;
using System.Globalization;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// The shop screen (docs/09 §2.3): wave preview, drag preview line, offer cards or the selected module's panel,
    /// the Sell zone, the drag ghost and the action row (Undo, Reroll, Slot +1, Next wave).
    /// </summary>
    internal sealed class ShopPanel
    {
        /// <summary>The dragged object sits above the finger so it is never covered (docs/03 B3), in panel pixels.</summary>
        private const float GhostLift = 150f;

        private readonly VisualElement _shop;
        private readonly VisualElement _modulePanel;
        private readonly Label _moduleInfo;
        private readonly Button _sell;
        private readonly Button _move;
        private readonly Button _undo;
        private readonly Button _reroll;
        private readonly Button _buySlot;
        private readonly Label _rerollCost;
        private readonly Label _buySlotCost;
        private readonly Label _previewEffect;
        private readonly VisualElement _sellZone;
        private readonly VisualElement _ghost;
        private readonly Label _ghostName;
        private readonly VisualElement _ghostArt;
        private IVisualElementScheduledItem _ghostHide;

        public ShopPanel(VisualElement root, Action sell, Action move, Action close, Action undo, Action reroll,
            Action buySlot, Action next)
        {
            Cards = new OfferCards(root);
            Preview = new WavePreview(root);
            _shop = root.Q<VisualElement>("shop");
            _modulePanel = root.Q<VisualElement>("module-panel");
            _moduleInfo = root.Q<Label>("module-info");
            _sell = root.Q<Button>("sell");
            _move = root.Q<Button>("move");
            _undo = root.Q<Button>("undo");
            _reroll = root.Q<Button>("reroll");
            _buySlot = root.Q<Button>("buy-slot");
            _rerollCost = root.Q<Label>("reroll-cost");
            _buySlotCost = root.Q<Label>("buy-slot-cost");
            _previewEffect = root.Q<Label>("preview-effect");
            _sellZone = root.Q<VisualElement>("sell-zone");
            _ghost = root.Q<VisualElement>("drag-ghost");
            _ghostName = root.Q<Label>("ghost-name");
            _ghostArt = root.Q<VisualElement>("ghost-art");

            _sell.clicked += sell;
            _move.clicked += move;
            root.Q<Button>("close").clicked += close;
            _undo.clicked += undo;
            _reroll.clicked += reroll;
            _buySlot.clicked += buySlot;
            root.Q<Button>("next").clicked += next;
        }

        public OfferCards Cards { get; }

        public WavePreview Preview { get; }

        /// <summary>True when the shop and its cards are on screen (so a card can be picked).</summary>
        public bool CardsVisible => !_shop.ClassListContains("hidden") && !Cards.Container.ClassListContains("hidden");

        public void SetVisible(bool visible) => Ui.Show(_shop, visible);

        public bool IsOverSellZone(Vector2 panelPosition) =>
            !_sellZone.ClassListContains("hidden") && _sellZone.worldBound.Contains(panelPosition);

        /// <summary>Shows the drag ghost above the finger.</summary>
        public void ShowGhost(string title, ModuleKind kind, Vector2 panelPosition)
        {
            _ghostHide?.Pause();
            _ghostHide = null;
            _ghost.RemoveFromClassList("drag-ghost--returning");
            Ui.Show(_ghost, true);
            Ui.SetText(_ghostName, title);
            Texture2D art = OfferCards.CardArt(kind);
            _ghostArt.style.backgroundImage = art != null ? new StyleBackground(art) : new StyleBackground(StyleKeyword.None);
            PlaceGhost(panelPosition + new Vector2(0f, -GhostLift));
        }

        /// <summary>Hides the ghost; with a return point it floats back first (soft return, no error flash).</summary>
        public void HideGhost(Vector2? returnToPanel)
        {
            if (returnToPanel.HasValue && !_ghost.ClassListContains("hidden"))
            {
                _ghost.AddToClassList("drag-ghost--returning");
                PlaceGhost(returnToPanel.Value);
                _ghostHide?.Pause();
                _ghostHide = _ghost.schedule.Execute(() => Ui.Show(_ghost, false));
                _ghostHide.ExecuteLater(260);
                return;
            }

            Ui.Show(_ghost, false);
        }

        private void PlaceGhost(Vector2 panelCentre)
        {
            _ghost.style.left = panelCentre.x - 120f;
            _ghost.style.top = panelCentre.y - 100f;
        }

        public void Refresh(GameSimulation sim, HudState state, Func<ModuleKind, string> describe)
        {
            Preview.Refresh(sim);

            ModuleInstance selected = sim.Ring.At(state.SelectedSlot);
            Ui.Show(_modulePanel, selected != null);
            Ui.Show(Cards.Container, selected == null);
            if (selected != null)
            {
                CultureInfo invariant = CultureInfo.InvariantCulture;
                string stats = selected.Category == ModuleCategory.Weapon
                    ? Loc.T("shop.module_weapon", NumberFormat.CompactHundredths(selected.EffectiveDamage),
                        (selected.DamageMultiplierPermille / 1000f).ToString("0.##", invariant),
                        (selected.EffectiveRange / (float)SimConstants.Micro).ToString("0.#", invariant),
                        (selected.EffectiveCooldown / (float)SimConstants.TicksPerSecond).ToString("0.##", invariant))
                    : describe(selected.Kind);
                Ui.SetText(_moduleInfo, Loc.T("shop.module_head", Loc.ModuleName(selected.Kind), selected.Level, stats));
                Ui.SetText(_sell, Loc.T("shop.sell_value", ModuleRules.SellValue(selected.Invested)));
                Ui.SetText(_move, Loc.T(state.MoveMode ? "shop.move_pick" : "shop.move"));
            }
            else
            {
                Cards.Refresh(sim, state);
            }

            Ui.Show(_previewEffect, !string.IsNullOrEmpty(state.PreviewText));
            Ui.SetText(_previewEffect, state.PreviewText ?? string.Empty);
            Ui.Show(_sellZone, state.ShowSellZone);
            _sellZone.EnableInClassList("sell-zone--hot", state.SellZoneHot);
            _undo.SetEnabled(sim.CanUndo);
            Ui.SetText(_rerollCost, sim.RerollCost.ToString());
            _reroll.SetEnabled(sim.Credits >= sim.RerollCost);
            Ui.Show(_buySlot, sim.CanBuyExtraSlot);
            Ui.SetText(_buySlotCost, sim.ExtraSlotCost.ToString());
            _buySlot.SetEnabled(sim.Credits >= sim.ExtraSlotCost);
        }
    }
}
