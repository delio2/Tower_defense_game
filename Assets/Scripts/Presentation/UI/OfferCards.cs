using System.Collections.Generic;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// The shop's offer cards v2 (docs/09 §2.3): the model render, the name, a category glyph in the corner, reach dots
    /// for the range bands it covers (D32), the cost beside the Credits icon, and the merge badge. The effect sentence
    /// is deliberately gone: at card width it wrapped to four lines. It lives in the drag preview and the module sheet.
    /// </summary>
    internal sealed class OfferCards
    {
        /// <summary>Gap between one card arriving and the next (docs/09 section 4.5).</summary>
        private const long CardStaggerMilliseconds = 40;

        /// <summary>Range bands a weapon can cover from the ring: short, mid, far (docs/12 §1, D32).</summary>
        private const int ReachBands = 3;
        private static readonly float[] BandEdges = { 3.0f, 5.5f, 7.5f };

        /// <summary>Sizes a card name may take, largest first (the design system's caption size and below).</summary>
        private static readonly float[] NameSizes = { 36f, 34f, 30f, 27f };

        private static readonly Dictionary<ModuleKind, Texture2D> CardArtCache = new Dictionary<ModuleKind, Texture2D>();

        private readonly VisualElement _cards;
        private readonly List<VisualElement> _cardViews = new List<VisualElement>();

        public OfferCards(VisualElement root)
        {
            _cards = root.Q<VisualElement>("cards");
        }

        public VisualElement Container => _cards;

        /// <summary>Card art rendered from the Blender models with the game's light (Assets/UI/Resources/Cards, docs/03 A10).</summary>
        public static Texture2D CardArt(ModuleKind kind)
        {
            if (!CardArtCache.TryGetValue(kind, out Texture2D art))
            {
                art = Resources.Load<Texture2D>($"Cards/{kind}");
                CardArtCache[kind] = art;
            }

            return art;
        }

        /// <summary>Card under a panel point, or -1. Sold cards do not count.</summary>
        public int CardAt(Vector2 panelPosition)
        {
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

        public Vector2 CardCentre(int index)
        {
            return index >= 0 && index < _cardViews.Count ? _cardViews[index].worldBound.center : Vector2.zero;
        }

        /// <summary>Drops the offer cards in 40 ms apart when the shop opens (docs/09 section 4.5).</summary>
        public void StaggerIn()
        {
            for (int i = 0; i < _cardViews.Count; i++)
            {
                VisualElement card = _cardViews[i];
                card.AddToClassList("card--entering");
                long delay = CardStaggerMilliseconds * i;
                card.schedule.Execute(() => card.RemoveFromClassList("card--entering")).ExecuteLater(delay + 1);
            }
        }

        public void Refresh(GameSimulation sim, HudState state)
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
                FitName(card.Q<Label>("name"), Loc.ModuleName(offer.Value));

                int reach = ReachOf(definition);
                VisualElement reachRow = card.Q<VisualElement>("reach");
                Ui.Show(reachRow, reach > 0);
                for (int band = 0; band < ReachBands; band++)
                {
                    reachRow[band].EnableInClassList("card__dot--on", band < reach);
                }

                VisualElement glyph = card.Q<VisualElement>("glyph");
                glyph.EnableInClassList("card__glyph--weapon", definition.Category == ModuleCategory.Weapon);
                glyph.EnableInClassList("card__glyph--booster", definition.Category == ModuleCategory.Booster);
                glyph.EnableInClassList("card__glyph--economy", definition.Category == ModuleCategory.Economy);

                var cost = card.Q<Label>("cost");
                Ui.SetText(cost, definition.Cost.ToString());
                cost.EnableInClassList("card__cost--unaffordable", !affordable);
                VisualElement badge = card.Q<VisualElement>("badge");
                Ui.Show(badge, merges);
                if (merges)
                {
                    Ui.SetText(card.Q<Label>("badge-level"), $"L{sim.Ring.FindMergeTarget(offer.Value).Level + 1}");
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
            Ui.SetText(label, text);
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
    }
}
