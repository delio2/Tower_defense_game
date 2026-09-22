using UnityEngine;

namespace TowerDefense.Presentation.Settings
{
    public enum QualityChoice : byte
    {
        Auto = 0,
        Low = 1,
        Medium = 2,
        High = 3,
    }

    /// <summary>
    /// The three graphics quality levels (docs/03 A6, docs/06 2.5-A3), one URP asset each in <c>Assets/Settings</c>:
    /// Low — no shadows, no HDR, no post-processing, render scale 0.75 (30 fps on a 4 GB phone);
    /// Medium — hard shadows, bloom and vignette, render scale 0.8 (the level measured at 60 fps on the Pixel 10);
    /// High — soft shadows at 2048, 4× MSAA, full resolution. Auto picks one from the device memory.
    /// Presentation only: the simulation never sees it, so replays are unaffected.
    /// </summary>
    public static class GraphicsQuality
    {
        // Indices of ProjectSettings/QualitySettings.asset
        public const int Low = 0;
        public const int Medium = 1;
        public const int High = 2;

        public static int Level => QualitySettings.GetQualityLevel();

        public static bool PostProcessing => Level != Low;

        public static bool Shadows => Level != Low;

        public static int Resolve(QualityChoice choice) => choice switch
        {
            QualityChoice.Low => Low,
            QualityChoice.Medium => Medium,
            QualityChoice.High => High,
            _ => AutoLevel(),
        };

        /// <summary>
        /// Memory is the one signal every Android device reports honestly, and D4 names the 4 GB phone as the reference
        /// low end. A phone sold as 4 GB reports about 3.6 GB, 6 GB about 5.6 GB (estimate): the thresholds sit between.
        /// </summary>
        public static int AutoLevel()
        {
            int megabytes = SystemInfo.systemMemorySize;
            if (megabytes <= 4608)
            {
                return Low;
            }

            return megabytes <= 6656 ? Medium : High;
        }

        /// <summary>Switches to the level the options ask for; a no-op when it is already active.</summary>
        public static void Apply()
        {
            int level = Resolve(PlayerOptions.Quality);
            if (Level != level)
            {
                QualitySettings.SetQualityLevel(level, true);
            }
        }

        public static string LevelName(int level) => level switch
        {
            Low => "Low",
            High => "High",
            _ => "Medium",
        };
    }
}
