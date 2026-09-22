using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Android builds for testing. ARM64 only: Unity 6.6 removed x86_64 for Android, so the x86_64 emulator images
    /// run it through their built-in ARM translation.
    /// <list type="bullet">
    /// <item>Benchmark APK: a development build for Tools/bench/run_emulators.py (profiler counters, logs).</item>
    /// <item>Tester APK: a release build for the playtests of docs/13 — no "Development Build" mark, real performance.</item>
    /// </list>
    /// </summary>
    public static class BenchmarkBuildMenu
    {
        public const string ApkPath = "Builds/Android/TowerDefense-bench.apk";
        public const string TesterApkPath = "Builds/Android/TowerDefense-tester.apk";

        [MenuItem("TowerDefense/Build/Android benchmark APK")]
        public static void BuildMenu() => Build();

        [MenuItem("TowerDefense/Build/Android tester APK")]
        public static void BuildTesterMenu() => BuildTester();

        public static bool Build() => BuildApk(ApkPath, BuildOptions.Development);

        public static bool BuildTester() => BuildApk(TesterApkPath, BuildOptions.None);

        private static bool BuildApk(string path, BuildOptions buildOptions)
        {
            bool appBundle = EditorUserBuildSettings.buildAppBundle;
            try
            {
                EditorUserBuildSettings.buildAppBundle = false;
                var options = new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    locationPathName = path,
                    target = BuildTarget.Android,
                    options = buildOptions,
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;
                Debug.Log($"APK: {summary.result}, {summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:F0} s -> {path}");
                return summary.result == BuildResult.Succeeded;
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = appBundle;
            }
        }
    }
}
