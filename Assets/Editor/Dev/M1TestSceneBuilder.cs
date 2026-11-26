using System;
using PastFuture.Presentation.Player;
using PastFuture.UnityAdapters.Player;

namespace PastFuture.Editor.Dev
{
    /// <summary>
    /// Generates the M1 greybox test scene: a 6 m × 6 m room, a 1 m doorway and a 2 m × 10 m corridor,
    /// a low step (0.2 m, climbable), a tall block (0.5 m, blocks) and a table, plus the first-person
    /// player rig (Handover 4절 M1 "방/복도", ADR-572). The scene is generated: re-running overwrites it,
    /// so change the layout here rather than by hand. Not in the build scene list.
    /// <para>Batchmode (Editor closed):
    /// <c>Unity -batchmode -nographics -quit -projectPath . -executeMethod PastFuture.Editor.Dev.M1TestSceneBuilder.Build -logFile TestResults/m1scene.log</c></para>
    /// </summary>
    public static class M1TestSceneBuilder
    {
        public const string ScenePath = "Assets/PastFuture/Scenes/Test/M1_TestRoom.unity";
        public const string ControlsPath = "Assets/PastFuture/Settings/Input/PastFutureControls.inputactions";

        /// <summary>Player spawn: room centre-south, facing the doorway (+Z).</summary>
        public static readonly UnityEngine.Vector3 PlayerSpawn = new UnityEngine.Vector3(0f, 0.05f, -1.5f);

        public const float WallHeight = 2.8f;
        public const float WallThickness = 0.2f;

        private const string LogPrefix = "[PastFuture.Editor.Dev]";

        [UnityEditor.MenuItem("PastFuture/Dev/Generate M1 Test Room Scene")]
        public static void Build()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // Load after NewScene: opening a scene unloads assets loaded before it, and a reference
            // to such an unloaded (non-persistent) object serializes as null.
            var controls = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(ControlsPath);
            if (controls == null || !UnityEditor.EditorUtility.IsPersistent(controls))
            {
                throw new InvalidOperationException($"{LogPrefix} Input actions not found at {ControlsPath}.");
            }

            CreateLight();
            CreateEnvironment();
            CreatePlayer(controls);

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ScenePath));
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"{LogPrefix} Failed to save {ScenePath}.");
            }

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log($"{LogPrefix} Generated {ScenePath}.");
        }

        private static void CreateLight()
        {
            var light = new UnityEngine.GameObject("Directional Light").AddComponent<UnityEngine.Light>();
            light.type = UnityEngine.LightType.Directional;
            light.shadows = UnityEngine.LightShadows.Soft;
            light.transform.rotation = UnityEngine.Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateEnvironment()
        {
            var root = new UnityEngine.GameObject("Environment").transform;
            const float h = WallHeight;
            const float t = WallThickness;
            const float half = h / 2f;

            // Floors (top surface at y = 0).
            Box(root, "Room_Floor", new UnityEngine.Vector3(0f, -0.1f, 0f), new UnityEngine.Vector3(6f, 0.2f, 6f));
            Box(root, "Corridor_Floor", new UnityEngine.Vector3(0f, -0.1f, 8.1f), new UnityEngine.Vector3(2.2f, 0.2f, 10.2f));

            // Room walls (inner faces at x = ±3, z = ±3).
            Box(root, "Room_Wall_South", new UnityEngine.Vector3(0f, half, -3f - t / 2f), new UnityEngine.Vector3(6f + 2f * t, h, t));
            Box(root, "Room_Wall_East", new UnityEngine.Vector3(3f + t / 2f, half, 0f), new UnityEngine.Vector3(t, h, 6f));
            Box(root, "Room_Wall_West", new UnityEngine.Vector3(-3f - t / 2f, half, 0f), new UnityEngine.Vector3(t, h, 6f));

            // North wall with a 1.0 m × 2.1 m doorway centred on x = 0.
            const float door = 1.0f;
            const float doorHeight = 2.1f;
            var segment = 3f + t - door / 2f;
            var northZ = 3f + t / 2f;
            Box(root, "Room_Wall_North_Left", new UnityEngine.Vector3(-(door / 2f + segment / 2f), half, northZ), new UnityEngine.Vector3(segment, h, t));
            Box(root, "Room_Wall_North_Right", new UnityEngine.Vector3(door / 2f + segment / 2f, half, northZ), new UnityEngine.Vector3(segment, h, t));
            Box(root, "Room_Doorway_Lintel", new UnityEngine.Vector3(0f, (doorHeight + h) / 2f, northZ), new UnityEngine.Vector3(door, h - doorHeight, t));

            // Corridor (inner width 2 m, from z = 3.2 to 13.2).
            Box(root, "Corridor_Wall_East", new UnityEngine.Vector3(1f + t / 2f, half, 8.2f), new UnityEngine.Vector3(t, h, 10f));
            Box(root, "Corridor_Wall_West", new UnityEngine.Vector3(-1f - t / 2f, half, 8.2f), new UnityEngine.Vector3(t, h, 10f));
            Box(root, "Corridor_Wall_End", new UnityEngine.Vector3(0f, half, 13.2f + t / 2f), new UnityEngine.Vector3(2f + 2f * t, h, t));

            // Movement checks.
            Box(root, "Obstacle_LowStep_0.2m", new UnityEngine.Vector3(1.5f, 0.1f, -1.5f), new UnityEngine.Vector3(1f, 0.2f, 1f));
            Box(root, "Obstacle_TallBlock_0.5m", new UnityEngine.Vector3(1.5f, 0.25f, 1f), new UnityEngine.Vector3(1f, 0.5f, 1f));
            Box(root, "Prop_Table", new UnityEngine.Vector3(-1.5f, 0.375f, -1.5f), new UnityEngine.Vector3(1.2f, 0.75f, 0.6f));
        }

        private static void CreatePlayer(UnityEngine.InputSystem.InputActionAsset controls)
        {
            var player = new UnityEngine.GameObject("Player");
            player.transform.position = PlayerSpawn;

            var controller = player.AddComponent<UnityEngine.CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new UnityEngine.Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 45f;
            controller.skinWidth = 0.03f;
            controller.minMoveDistance = 0f; // PlayerMotor also enforces this.

            var motor = player.AddComponent<PlayerMotor>();

            var pivot = new UnityEngine.GameObject("CameraPivot").transform;
            pivot.SetParent(player.transform, false);
            pivot.localPosition = new UnityEngine.Vector3(0f, 1.6f, 0f);

            var cameraObject = new UnityEngine.GameObject("Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(pivot, false);
            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.nearClipPlane = 0.05f;
            camera.fieldOfView = 70f;
            cameraObject.AddComponent<UnityEngine.AudioListener>();

            var rig = player.AddComponent<FirstPersonPlayerRig>();
            var serialized = new UnityEditor.SerializedObject(rig);
            serialized.FindProperty("controls").objectReferenceValue = controls;
            serialized.FindProperty("motor").objectReferenceValue = motor;
            serialized.FindProperty("cameraPivot").objectReferenceValue = pivot;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (serialized.FindProperty("controls").objectReferenceValue == null
                || serialized.FindProperty("motor").objectReferenceValue == null
                || serialized.FindProperty("cameraPivot").objectReferenceValue == null)
            {
                throw new InvalidOperationException($"{LogPrefix} FirstPersonPlayerRig references were not assigned.");
            }
        }

        private static void Box(UnityEngine.Transform parent, string name, UnityEngine.Vector3 position, UnityEngine.Vector3 size)
        {
            var box = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = size;
            box.isStatic = true;
        }
    }
}
