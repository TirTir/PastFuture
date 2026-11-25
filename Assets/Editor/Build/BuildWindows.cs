using System;
using System.Linq;

namespace PastFuture.Editor.Build
{
    /// <summary>
    /// Windows 64-bit Development Player build from the Mac Editor (Handover 16-18 build
    /// candidate, ADR-571). Mono backend only: a Mac Editor cannot produce Windows IL2CPP.
    /// Builds the enabled scenes from the Build Profiles scene list (see <see cref="BuildSceneList"/>).
    /// A failed BuildReport throws, so a batchmode run exits with code 1 (CI failure).
    /// <para>Batchmode (Editor closed):
    /// <c>Unity -batchmode -nographics -quit -projectPath . -buildTarget StandaloneWindows64 -executeMethod PastFuture.Editor.Build.BuildWindows.Run -logFile TestResults/build-windows.log</c></para>
    /// <para>Editor: menu PastFuture/Build/Windows Development Build.</para>
    /// </summary>
    public static class BuildWindows
    {
        /// <summary>Output under the git-ignored Builds/ folder.</summary>
        public const string OutputPath = "Builds/Windows/PastFuture.exe";

        private const string LogPrefix = "[PastFuture.Editor.Build]";

        [UnityEditor.MenuItem("PastFuture/Build/Windows Development Build")]
        public static void Run()
        {
            var scenes = UnityEditor.EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new InvalidOperationException($"{LogPrefix} No enabled scenes in the build list.");
            }

            var options = new UnityEditor.BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = UnityEditor.BuildTarget.StandaloneWindows64,
                targetGroup = UnityEditor.BuildTargetGroup.Standalone,
                options = UnityEditor.BuildOptions.Development,
            };

            var backend = UnityEditor.PlayerSettings.GetScriptingBackend(
                UnityEditor.Build.NamedBuildTarget.Standalone);
            var report = UnityEditor.BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            UnityEngine.Debug.Log(
                $"{LogPrefix} Windows build {summary.result}: output={summary.outputPath}, " +
                $"backend={backend}, scenes=[{string.Join(", ", scenes)}], " +
                $"size={summary.totalSize} bytes, errors={summary.totalErrors}, " +
                $"warnings={summary.totalWarnings}, time={summary.totalTime}");

            if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"{LogPrefix} Windows build failed: {summary.result} ({summary.totalErrors} errors).");
            }
        }
    }
}
