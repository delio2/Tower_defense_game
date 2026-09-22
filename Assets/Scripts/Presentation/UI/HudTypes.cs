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

    /// <summary>Small helpers every HUD panel shares.</summary>
    internal static class Ui
    {
        public static void Show(VisualElement element, bool visible) => element.EnableInClassList("hidden", !visible);

        /// <summary>Writes a label only when the text changes, so an unchanged label never dirties the layout.</summary>
        public static void SetText(TextElement element, string text)
        {
            if (element.text != text)
            {
                element.text = text;
            }
        }

        public static float EaseOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);

        /// <summary>Screen point (bottom-left origin) to panel coordinates.</summary>
        public static Vector2 ToPanel(VisualElement root, Vector2 screenPosition) =>
            RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));

        public static string DamageNumbersLabel() => Settings.PlayerOptions.DamageNumbers switch
        {
            Settings.DamageNumbersMode.BigOnly => Loc.T("numbers.big"),
            Settings.DamageNumbersMode.None => Loc.T("numbers.none"),
            _ => Loc.T("numbers.all"),
        };

        /// <summary>The language the options ask for, resolved against the phone's.</summary>
        public static GameLanguage ResolveLanguage(Settings.LanguageChoice choice) => choice switch
        {
            Settings.LanguageChoice.English => GameLanguage.English,
            Settings.LanguageChoice.Italian => GameLanguage.Italian,
            _ => Loc.SystemLanguage,
        };
    }
}
