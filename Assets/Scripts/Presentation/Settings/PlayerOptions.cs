using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Settings
{
    public enum LanguageChoice : byte
    {
        Auto = 0,
        English = 1,
        Italian = 2,
    }

    public enum DamageNumbersMode : byte
    {
        All = 0,
        BigOnly = 1,
        None = 2,
    }

    /// <summary>
    /// Player-facing options (docs/03 B7, docs/09 §2.7). Stored in PlayerPrefs for the prototype; they only affect
    /// presentation, never the simulation, so replays are unaffected.
    /// </summary>
    public static class PlayerOptions
    {
        private const string ReduceMotionKey = "options.reduceMotion";
        private const string EffectIntensityKey = "options.effectIntensity";
        private const string DamageNumbersKey = "options.damageNumbers";
        private const string HapticsKey = "options.haptics";
        private const string DefaultSpeedKey = "options.defaultSpeed";
        private const string CoreKey = "run.core";
        private const string GradeKey = "run.grade";
        private const string EndlessKey = "run.endless";

        public static bool ReduceMotion
        {
            get => PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1;
            set => PlayerPrefs.SetInt(ReduceMotionKey, value ? 1 : 0);
        }

        /// <summary>0–100: scales effect alpha, flake counts and effect caps.</summary>
        public static int EffectIntensity
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(EffectIntensityKey, 100), 0, 100);
            set => PlayerPrefs.SetInt(EffectIntensityKey, Mathf.Clamp(value, 0, 100));
        }

        public static float EffectScale => EffectIntensity / 100f;

        public static DamageNumbersMode DamageNumbers
        {
            get => (DamageNumbersMode)Mathf.Clamp(PlayerPrefs.GetInt(DamageNumbersKey, 0), 0, 2);
            set => PlayerPrefs.SetInt(DamageNumbersKey, (int)value);
        }

        public static bool Haptics
        {
            get => PlayerPrefs.GetInt(HapticsKey, 1) == 1;
            set => PlayerPrefs.SetInt(HapticsKey, value ? 1 : 0);
        }

        /// <summary>1x / 2x / 3x used when a run starts (docs/09 §2.7 "default speed").</summary>
        public static int DefaultSpeed
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(DefaultSpeedKey, 1), 1, 3);
            set => PlayerPrefs.SetInt(DefaultSpeedKey, Mathf.Clamp(value, 1, 3));
        }

        // Choices for the next run (a Home screen replaces these in Phase 3/4, docs/09 §2.1)
        public static CoreType Core
        {
            get => (CoreType)Mathf.Clamp(PlayerPrefs.GetInt(CoreKey, 0), 0, 3);
            set => PlayerPrefs.SetInt(CoreKey, (int)value);
        }

        public static int Grade
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(GradeKey, 0), 0, 3);
            set => PlayerPrefs.SetInt(GradeKey, Mathf.Clamp(value, 0, 3));
        }

        public static bool Endless
        {
            get => PlayerPrefs.GetInt(EndlessKey, 0) == 1;
            set => PlayerPrefs.SetInt(EndlessKey, value ? 1 : 0);
        }

        public static void Save() => PlayerPrefs.Save();

        /// <summary>
        /// Larger text for everyone who needs it (docs/09 §6): the stylesheet overrides the five font variables,
        /// so every rule that uses them follows without a second layout.
        /// </summary>
        public static bool LargeText
        {
            get => PlayerPrefs.GetInt(LargeTextKey, 0) == 1;
            set => PlayerPrefs.SetInt(LargeTextKey, value ? 1 : 0);
        }

        /// <summary>The design system's fourth theme: the same layout with contrast pushed up (docs/09 §6).</summary>
        public static bool HighContrast
        {
            get => PlayerPrefs.GetInt(HighContrastKey, 0) == 1;
            set => PlayerPrefs.SetInt(HighContrastKey, value ? 1 : 0);
        }

        /// <summary>Graphics quality level (<see cref="GraphicsQuality"/>); Auto chooses from the device memory.</summary>
        public static QualityChoice Quality
        {
            get => (QualityChoice)Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, 0), 0, 3);
            set => PlayerPrefs.SetInt(QualityKey, (int)value);
        }

        /// <summary>Interface language; Auto follows the phone (English when the phone speaks neither).</summary>
        public static LanguageChoice Language
        {
            get => (LanguageChoice)Mathf.Clamp(PlayerPrefs.GetInt(LanguageKey, 0), 0, 2);
            set => PlayerPrefs.SetInt(LanguageKey, (int)value);
        }

        private const string LanguageKey = "opt.language";
        private const string QualityKey = "opt.quality";
        private const string LargeTextKey = "opt.largeText";
        private const string HighContrastKey = "opt.highContrast";
    }
}
