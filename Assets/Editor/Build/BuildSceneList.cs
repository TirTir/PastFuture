using System;
using System.IO;

namespace PastFuture.Editor.Build
{
    /// <summary>
    /// Sets the Build Profiles scene list so the Bootstrap scene is scene 0 (ADR-569).
    /// Only the list changes: scene files that drop out of it (e.g. the template
    /// SampleScene) stay on disk.
    /// <para>Batchmode (Editor closed):
    /// <c>Unity -batchmode -nographics -projectPath . -executeMethod PastFuture.Editor.Build.BuildSceneList.Apply -quit -logFile -</c></para>
    /// <para>Editor: menu PastFuture/Build/Apply Build Scene List.</para>
    /// </summary>
    public static class BuildSceneList
    {
        public const string BootstrapScenePath = "Assets/PastFuture/Scenes/Bootstrap.unity";

        private const string LogPrefix = "[PastFuture.Editor.Build]";

        /// <summary>Build scene order. Index 0 is loaded first in a player build.</summary>
        private static readonly string[] ScenePaths = { BootstrapScenePath };

        [UnityEditor.MenuItem("PastFuture/Build/Apply Build Scene List")]
        public static void Apply()
        {
            var scenes = new UnityEditor.EditorBuildSettingsScene[ScenePaths.Length];
            for (var i = 0; i < ScenePaths.Length; i++)
            {
                var path = ScenePaths[i];
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid) || !File.Exists(path))
                {
                    // Throwing makes a batchmode -executeMethod run exit with code 1.
                    throw new InvalidOperationException($"{LogPrefix} Scene not found: {path}");
                }

                scenes[i] = new UnityEditor.EditorBuildSettingsScene(new UnityEditor.GUID(guid), enabled: true);
            }

            var previous = UnityEditor.EditorBuildSettings.scenes;
            UnityEditor.EditorBuildSettings.scenes = scenes;
            UnityEditor.AssetDatabase.SaveAssets();

            UnityEngine.Debug.Log(
                $"{LogPrefix} Build scene list: [{Describe(previous)}] -> [{Describe(UnityEditor.EditorBuildSettings.scenes)}]");
        }

        private static string Describe(UnityEditor.EditorBuildSettingsScene[] scenes)
        {
            var parts = new string[scenes.Length];
            for (var i = 0; i < scenes.Length; i++)
            {
                parts[i] = $"{i}:{scenes[i].path}{(scenes[i].enabled ? string.Empty : " (disabled)")}";
            }

            return string.Join(", ", parts);
        }
    }
}
