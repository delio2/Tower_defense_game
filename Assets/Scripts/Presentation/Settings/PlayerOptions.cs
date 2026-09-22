using UnityEngine;

namespace TowerDefense.Presentation.Settings
{
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

        public static void Save() => PlayerPrefs.Save();
    }
}
