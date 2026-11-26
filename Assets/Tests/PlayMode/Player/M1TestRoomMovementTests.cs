using System.Collections;
using NUnit.Framework;
using PastFuture.Presentation.Player;
using PastFuture.UnityAdapters.Player;
using UnityEngine.TestTools;

namespace PastFuture.Tests.PlayMode.Player
{
    /// <summary>
    /// Movement and collision in the generated M1 test room (ADR-572). Handover ID 없음.
    /// The input rig is disabled and <see cref="PlayerMotor"/> is driven directly, because
    /// batchmode has no keyboard or mouse. Input wiring is checked by playing in the Editor.
    /// Geometry values match PastFuture.Editor.Dev.M1TestSceneBuilder.
    /// </summary>
    [Category("Player")]
    public sealed class M1TestRoomMovementTests
    {
        private const string ScenePath = "Assets/PastFuture/Scenes/Test/M1_TestRoom.unity";
        private const float Radius = 0.3f;

        private PlayerMotor _motor;
        private UnityEngine.CharacterController _controller;

        [UnitySetUp]
        public IEnumerator LoadTestRoom()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
#else
            Assert.Ignore("The M1 test room is not in the build scene list; Editor only.");
            yield break;
#endif
            var rigs = UnityEngine.Object.FindObjectsByType<FirstPersonPlayerRig>(UnityEngine.FindObjectsSortMode.None);
            Assert.That(rigs, Has.Length.EqualTo(1), "Expected one FirstPersonPlayerRig in the test room.");
            Assert.That(rigs[0].enabled, Is.True, "Rig disabled itself: a serialized reference is missing.");
            rigs[0].enabled = false;

            _motor = rigs[0].GetComponent<PlayerMotor>();
            _controller = rigs[0].GetComponent<UnityEngine.CharacterController>();
            Assert.That(_motor, Is.Not.Null);
            Assert.That(_controller, Is.Not.Null);
        }

        [TearDown]
        public void ResetFrameTime()
        {
            UnityEngine.Time.captureDeltaTime = 0f;
        }

        [UnityTest]
        public IEnumerator M1TestRoom_HasOneCameraAndOneAudioListener()
        {
            yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(UnityEngine.FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<UnityEngine.AudioListener>(UnityEngine.FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        [Test]
        public void PlayerMotor_Controller_HasZeroMinMoveDistance()
        {
            Assert.That(_controller.minMoveDistance, Is.EqualTo(0f), "Small per-frame moves would be dropped at high frame rates.");
        }

        [UnityTest]
        public IEnumerator PlayerMotor_AtSpawn_SettlesGroundedOnFloor()
        {
            yield return new UnityEngine.WaitForSeconds(0.5f);

            Assert.That(_motor.IsGrounded, Is.True);
            Assert.That(_motor.transform.position.y, Is.EqualTo(0f).Within(0.1f));
        }

        [UnityTest]
        public IEnumerator PlayerMotor_WalkIntent_ReachesWalkSpeed()
        {
            _motor.SetMoveIntent(new UnityEngine.Vector3(0f, 0f, 1f), false);
            yield return new UnityEngine.WaitForSeconds(0.5f);

            Assert.That(_motor.PlanarVelocity.magnitude, Is.EqualTo(_motor.Settings.walkSpeed).Within(0.2f));
        }

        [UnityTest]
        public IEnumerator PlayerMotor_SprintIntent_ReachesSprintSpeed()
        {
            _motor.SetMoveIntent(new UnityEngine.Vector3(0f, 0f, 1f), true);
            yield return new UnityEngine.WaitForSeconds(0.5f);

            Assert.That(_motor.PlanarVelocity.magnitude, Is.EqualTo(_motor.Settings.sprintSpeed).Within(0.3f));
        }

        [UnityTest]
        public IEnumerator PlayerMotor_WalkIntoWall_StopsAtWall()
        {
            // South wall inner face is z = -3; spawn is z = -1.5.
            _motor.SetMoveIntent(new UnityEngine.Vector3(0f, 0f, -1f), false);
            yield return new UnityEngine.WaitForSeconds(2f);

            Assert.That(_motor.transform.position.z, Is.LessThan(-3f + Radius + 0.05f), "The player did not reach the wall.");
            Assert.That(_motor.transform.position.z, Is.GreaterThanOrEqualTo(-3f + Radius - 0.05f), "The player went into the wall.");
            Assert.That(_motor.PlanarVelocity.magnitude, Is.LessThan(0.1f), "Speed must not build up against a wall.");
        }

        [UnityTest]
        public IEnumerator PlayerMotor_LowStep_IsClimbed()
        {
            // 0.2 m step spans x 1..2, z -2..-1; stepOffset is 0.3 m.
            yield return Teleport(new UnityEngine.Vector3(1.5f, 0.05f, -2.6f));
            _motor.SetMoveIntent(new UnityEngine.Vector3(0f, 0f, 1f), false);

            var maxY = float.MinValue;
            var elapsed = 0f;
            while (elapsed < 0.6f)
            {
                yield return null;
                elapsed += UnityEngine.Time.deltaTime;
                maxY = UnityEngine.Mathf.Max(maxY, _motor.transform.position.y);
            }

            Assert.That(maxY, Is.GreaterThan(0.15f), "The player did not step up onto the 0.2 m step.");
            Assert.That(_motor.transform.position.z, Is.GreaterThan(-1.6f), "The player stopped at the low step.");
        }

        [UnityTest]
        public IEnumerator PlayerMotor_TallBlock_BlocksMovement()
        {
            // 0.5 m block spans x 1..2, z 0.5..1.5; higher than stepOffset.
            yield return Teleport(new UnityEngine.Vector3(1.5f, 0.05f, -0.4f));
            _motor.SetMoveIntent(new UnityEngine.Vector3(0f, 0f, 1f), false);
            yield return new UnityEngine.WaitForSeconds(1.5f);

            Assert.That(_motor.transform.position.z, Is.GreaterThan(0.5f - Radius - 0.05f), "The player did not reach the block.");
            Assert.That(_motor.transform.position.z, Is.LessThanOrEqualTo(0.5f - Radius + 0.05f), "The player went into the block.");
            Assert.That(_motor.transform.position.y, Is.LessThan(0.1f), "The player climbed the 0.5 m block.");
        }

        [UnityTest]
        public IEnumerator PlayerMotor_ThroughDoorway_EntersCorridor()
        {
            yield return Teleport(new UnityEngine.Vector3(0f, 0.05f, 2f));
            _motor.SetMoveIntent(new UnityEngine.Vector3(0f, 0f, 1f), false);
            yield return new UnityEngine.WaitForSeconds(1.5f);

            var position = _motor.transform.position;
            Assert.That(position.z, Is.GreaterThan(4f), "The player did not pass the 1 m doorway.");
            Assert.That(UnityEngine.Mathf.Abs(position.x), Is.LessThanOrEqualTo(1f - Radius + 0.05f));
        }

        [UnityTest]
        public IEnumerator PlayerMotor_SameWalkAt30And60Fps_CoversSameDistance()
        {
            // Fixed frame times stand in for 30 and 60 FPS (batchmode otherwise runs at thousands of FPS).
            var distances = new float[2];
            var frameRates = new[] { 30, 60 };
            for (var i = 0; i < frameRates.Length; i++)
            {
                UnityEngine.Time.captureDeltaTime = 1f / frameRates[i];
                yield return Teleport(new UnityEngine.Vector3(-0.5f, 0.05f, -2.5f));
                var startZ = _motor.transform.position.z;
                _motor.SetMoveIntent(new UnityEngine.Vector3(0f, 0f, 1f), false);
                for (var frame = 0; frame < frameRates[i]; frame++) // one second of game frames
                {
                    yield return null;
                }

                _motor.SetMoveIntent(UnityEngine.Vector3.zero, false);
                distances[i] = _motor.transform.position.z - startZ;
            }

            UnityEngine.Time.captureDeltaTime = 0f;
            TestContext.Out.WriteLine($"1 s walk: 30 FPS {distances[0]:F3} m, 60 FPS {distances[1]:F3} m");
            Assert.That(distances[0], Is.GreaterThan(2f), "The player barely moved at 30 FPS.");
            Assert.That(distances[0], Is.EqualTo(distances[1]).Within(0.1f),
                $"30 FPS moved {distances[0]:F3} m, 60 FPS moved {distances[1]:F3} m.");
        }

        private IEnumerator Teleport(UnityEngine.Vector3 position)
        {
            // A CharacterController overrides transform writes while enabled.
            _controller.enabled = false;
            _motor.transform.position = position;
            _controller.enabled = true;
            yield return new UnityEngine.WaitForSeconds(0.2f); // Settle onto the floor.
        }
    }
}
