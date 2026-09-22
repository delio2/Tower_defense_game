using UnityEngine;

namespace TowerDefense.Presentation.Settings
{
    /// <summary>
    /// Three short haptic levels (docs/03 B5): light (pick up, magnet, button), medium (place, Pulse, Next wave),
    /// success (two taps: merge, victory). Android uses VibrationEffect with amplitude (API 26+, our minimum);
    /// other platforms are silent. Never on errors, never long or continuous.
    /// </summary>
    public static class Haptics
    {
        private const int LightMs = 15;
        private const int MediumMs = 25;
        private const int LightAmplitude = 60;
        private const int MediumAmplitude = 140;
        private const int SuccessAmplitude = 180;

        public static void Light() => OneShot(LightMs, LightAmplitude);

        public static void Medium() => OneShot(MediumMs, MediumAmplitude);

        /// <summary>Two taps: 25 ms, 60 ms pause, 25 ms.</summary>
        public static void Success() => Waveform(new long[] { 0, MediumMs, 60, MediumMs }, new int[] { 0, SuccessAmplitude, 0, SuccessAmplitude });

        private static void OneShot(int milliseconds, int amplitude)
        {
            if (!PlayerOptions.Haptics)
            {
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                using AndroidJavaObject effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", (long)milliseconds, amplitude);
                Vibrate(effect);
            }
            catch (System.Exception)
            {
                // No vibrator or no amplitude control: stay silent rather than buzz wrongly.
            }
#endif
        }

        private static void Waveform(long[] timings, int[] amplitudes)
        {
            if (!PlayerOptions.Haptics)
            {
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                using AndroidJavaObject effect = effectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, -1);
                Vibrate(effect);
            }
            catch (System.Exception)
            {
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void Vibrate(AndroidJavaObject effect)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            if (vibrator != null && vibrator.Call<bool>("hasVibrator"))
            {
                vibrator.Call("vibrate", effect);
            }
        }
#endif
    }
}
