using System.Collections;
using NUnit.Framework;
using PastFuture.Presentation.Player;
using PastFuture.UnityAdapters.Player;
using UnityEngine.TestTools;

namespace PastFuture.Tests.PlayMode.Player
{
    /// <summary>
    /// Input wiring of the M1 test room rig (ADR-572). Handover ID 없음.
    /// <see cref="UnityEngine.InputSystem.InputTestFixture"/> replaces the real devices with a
    /// virtual keyboard and mouse, so key presses and mouse movement go through
    /// PastFutureControls → <see cref="FirstPersonPlayerRig"/> → <see cref="PlayerMotor"/>
    /// exactly as in play. Whether the controls feel natural still needs a person.
    /// </summary>
    [Category("Player")]
    public sealed class FirstPersonRigInputTests : UnityEngine.InputSystem.InputTestFixture
    {
        private const string ScenePath = "Assets/PastFuture/Scenes/Test/M1_TestRoom.unity";

        private UnityEngine.InputSystem.Keyboard _keyboard;
        private UnityEngine.InputSystem.Mouse _mouse;
        private FirstPersonPlayerRig _rig;
        private PlayerMotor _motor;

        [UnityTest]
        public IEnumerator Rig_HoldW_WalksForward()
        {
            yield return LoadTestRoom();
            var start = _motor.transform.position;

            Press(_keyboard.wKey);
            yield return new UnityEngine.WaitForSeconds(0.5f);

            Assert.That(_motor.PlanarVelocity.magnitude, Is.EqualTo(_motor.Settings.walkSpeed).Within(0.2f));
            Assert.That(_motor.transform.position.z - start.z, Is.GreaterThan(0.8f), "W did not move the player forward.");
            Assert.That(UnityEngine.Mathf.Abs(_motor.transform.position.x - start.x), Is.LessThan(0.05f));
        }

        [UnityTest]
        public IEnumerator Rig_HoldShiftW_Sprints()
        {
            yield return LoadTestRoom();

            Press(_keyboard.leftShiftKey);
            Press(_keyboard.wKey);
            yield return new UnityEngine.WaitForSeconds(0.5f);

            Assert.That(_motor.PlanarVelocity.magnitude, Is.EqualTo(_motor.Settings.sprintSpeed).Within(0.3f));
        }

        [UnityTest]
        public IEnumerator Rig_ReleaseW_Stops()
        {
            yield return LoadTestRoom();

            Press(_keyboard.wKey);
            yield return new UnityEngine.WaitForSeconds(0.3f);
            Release(_keyboard.wKey);
            yield return new UnityEngine.WaitForSeconds(0.5f);

            Assert.That(_motor.PlanarVelocity.magnitude, Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator Rig_HoldD_StrafesRight()
        {
            yield return LoadTestRoom();
            var start = _motor.transform.position;

            Press(_keyboard.dKey);
            yield return new UnityEngine.WaitForSeconds(0.3f);

            Assert.That(_motor.transform.position.x - start.x, Is.GreaterThan(0.3f), "D did not move the player right.");
            Assert.That(UnityEngine.Mathf.Abs(_motor.transform.position.z - start.z), Is.LessThan(0.05f));
        }

        [UnityTest]
        public IEnumerator Rig_MouseRight_TurnsBody_AndWFollowsView()
        {
            yield return LoadTestRoom();

            // 900 px × 0.1 °/px = 90°: facing +X instead of +Z.
            Set(_mouse.delta, new UnityEngine.Vector2(900f, 0f));
            yield return null;
            yield return null;

            Assert.That(_rig.LookState.Yaw, Is.EqualTo(90f).Within(0.5f));
            Assert.That(_motor.transform.eulerAngles.y, Is.EqualTo(90f).Within(0.5f), "Yaw was not applied to the body.");

            var start = _motor.transform.position;
            Press(_keyboard.wKey);
            yield return new UnityEngine.WaitForSeconds(0.3f);

            Assert.That(_motor.transform.position.x - start.x, Is.GreaterThan(0.3f), "W must follow the view direction.");
            Assert.That(UnityEngine.Mathf.Abs(_motor.transform.position.z - start.z), Is.LessThan(0.05f));
        }

        [UnityTest]
        public IEnumerator Rig_MouseUp_PitchesCameraUp_AndClamps()
        {
            yield return LoadTestRoom();
            var pivot = _rig.transform.Find("CameraPivot");
            Assert.That(pivot, Is.Not.Null);

            Set(_mouse.delta, new UnityEngine.Vector2(0f, 200f));
            yield return null;
            yield return null;

            Assert.That(_rig.LookState.Pitch, Is.EqualTo(-20f).Within(0.5f), "Mouse up must look up.");
            Assert.That(UnityEngine.Mathf.DeltaAngle(0f, pivot.localEulerAngles.x), Is.EqualTo(-20f).Within(0.5f));
            Assert.That(_motor.transform.eulerAngles.x, Is.EqualTo(0f).Within(0.01f), "Pitch must not tilt the body.");

            Set(_mouse.delta, new UnityEngine.Vector2(0f, 5000f));
            yield return null;
            yield return null;

            Assert.That(_rig.LookState.Pitch, Is.EqualTo(-85f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator Rig_Escape_ReleasesCursor_AndIgnoresInputUntilClick()
        {
            yield return LoadTestRoom();
            Assert.That(_rig.IsInputCaptured, Is.True, "Input must be captured when play starts.");

            PressAndRelease(_keyboard.escapeKey);
            yield return null;
            yield return null;
            Assert.That(_rig.IsInputCaptured, Is.False);
            Assert.That(UnityEngine.Cursor.visible, Is.True, "The cursor must be shown after Esc.");

            var start = _motor.transform.position;
            Press(_keyboard.wKey);
            yield return new UnityEngine.WaitForSeconds(0.3f);
            Assert.That(UnityEngine.Vector3.Distance(_motor.transform.position, start), Is.LessThan(0.05f),
                "Movement must be ignored while the cursor is released.");

            PressAndRelease(_mouse.leftButton);
            yield return null;
            yield return null;
            Assert.That(_rig.IsInputCaptured, Is.True);

            yield return new UnityEngine.WaitForSeconds(0.3f);
            Assert.That(_motor.transform.position.z - start.z, Is.GreaterThan(0.3f), "W must work again after the click.");
        }

        private IEnumerator LoadTestRoom()
        {
            // Devices must exist in the test input system before the rig enables its actions.
            _keyboard = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            _mouse = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>();

#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
#else
            Assert.Ignore("The M1 test room is not in the build scene list; Editor only.");
            yield break;
#endif
            var rigs = UnityEngine.Object.FindObjectsByType<FirstPersonPlayerRig>(UnityEngine.FindObjectsSortMode.None);
            Assert.That(rigs, Has.Length.EqualTo(1));
            _rig = rigs[0];
            Assert.That(_rig.enabled, Is.True, "Rig disabled itself: a serialized reference is missing.");
            _motor = _rig.GetComponent<PlayerMotor>();

            // Let the player land before measuring.
            yield return new UnityEngine.WaitForSeconds(0.3f);
            Assert.That(_motor.IsGrounded, Is.True);
        }
    }
}
