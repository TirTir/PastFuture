using NUnit.Framework;
using PastFuture.Presentation.Player;

namespace PastFuture.Tests.EditMode.Player
{
    // Handover ID 없음: M1 first-person look (ADR-572).
    [Category("Player")]
    public sealed class FirstPersonLookStateTests
    {
        private const float Tolerance = 1e-3f;

        [Test]
        public void FirstPersonLookState_PointerUp_LooksUp_AndPitchIsClamped()
        {
            var settings = new LookSettings();
            var state = new FirstPersonLookState(0f, 0f, settings);

            state.Apply(new UnityEngine.Vector2(0f, 100f), settings);
            Assert.That(state.Pitch, Is.EqualTo(-100f * settings.sensitivity).Within(Tolerance));

            state.Apply(new UnityEngine.Vector2(0f, 100000f), settings);
            Assert.That(state.Pitch, Is.EqualTo(settings.minPitch));

            state.Apply(new UnityEngine.Vector2(0f, -100000f), settings);
            Assert.That(state.Pitch, Is.EqualTo(settings.maxPitch));
        }

        [Test]
        public void FirstPersonLookState_InvertY_ReversesPitch()
        {
            var settings = new LookSettings { invertY = true };
            var state = new FirstPersonLookState(0f, 0f, settings);

            state.Apply(new UnityEngine.Vector2(0f, 100f), settings);

            Assert.That(state.Pitch, Is.EqualTo(100f * settings.sensitivity).Within(Tolerance));
        }

        [Test]
        public void FirstPersonLookState_Yaw_WrapsIntoZeroTo360()
        {
            var settings = new LookSettings { sensitivity = 1f };
            var state = new FirstPersonLookState(350f, 0f, settings);

            state.Apply(new UnityEngine.Vector2(20f, 0f), settings);
            Assert.That(state.Yaw, Is.EqualTo(10f).Within(Tolerance));

            state.Apply(new UnityEngine.Vector2(-30f, 0f), settings);
            Assert.That(state.Yaw, Is.EqualTo(340f).Within(Tolerance));
        }

        [Test]
        public void FirstPersonLookState_InitialPitchFromEuler_IsReadAsSignedAngle()
        {
            // Transform.localEulerAngles reports -10 degrees as 350.
            var state = new FirstPersonLookState(0f, 350f, new LookSettings());

            Assert.That(state.Pitch, Is.EqualTo(-10f).Within(Tolerance));
        }
    }
}
