using NUnit.Framework;
using PastFuture.UnityAdapters.Player;

namespace PastFuture.Tests.EditMode.Player
{
    // Handover ID 없음: M1 movement math (ADR-572).
    [Category("Player")]
    public sealed class PlayerMotorMathTests
    {
        private const float Tolerance = 1e-4f;
        private static readonly UnityEngine.Vector3 Forward = new UnityEngine.Vector3(0f, 0f, 1f);

        [Test]
        public void PlayerMotorMath_WalkInputHeld_ReachesWalkSpeedWithoutOvershoot()
        {
            var settings = new MovementSettings();
            var velocity = UnityEngine.Vector3.zero;
            for (var i = 0; i < 120; i++)
            {
                velocity = PlayerMotorMath.StepPlanarVelocity(velocity, Forward, false, settings, 1f / 60f);
                Assert.That(velocity.magnitude, Is.LessThanOrEqualTo(settings.walkSpeed + Tolerance));
            }

            Assert.That(velocity.z, Is.EqualTo(settings.walkSpeed).Within(Tolerance));
        }

        [Test]
        public void PlayerMotorMath_SprintHeld_UsesSprintSpeed()
        {
            var settings = new MovementSettings();
            var velocity = UnityEngine.Vector3.zero;
            for (var i = 0; i < 120; i++)
            {
                velocity = PlayerMotorMath.StepPlanarVelocity(velocity, Forward, true, settings, 1f / 60f);
            }

            Assert.That(velocity.magnitude, Is.EqualTo(settings.sprintSpeed).Within(Tolerance));
        }

        [Test]
        public void PlayerMotorMath_DiagonalInput_IsNotFasterThanStraight()
        {
            var settings = new MovementSettings();
            var diagonal = new UnityEngine.Vector3(1f, 0f, 1f);
            var velocity = UnityEngine.Vector3.zero;
            for (var i = 0; i < 120; i++)
            {
                velocity = PlayerMotorMath.StepPlanarVelocity(velocity, diagonal, false, settings, 1f / 60f);
            }

            Assert.That(velocity.magnitude, Is.EqualTo(settings.walkSpeed).Within(Tolerance));
        }

        [Test]
        public void PlayerMotorMath_InputReleased_DeceleratesToZero()
        {
            var settings = new MovementSettings();
            var velocity = Forward * settings.walkSpeed;
            for (var i = 0; i < 60; i++)
            {
                velocity = PlayerMotorMath.StepPlanarVelocity(velocity, UnityEngine.Vector3.zero, false, settings, 1f / 60f);
            }

            Assert.That(velocity, Is.EqualTo(UnityEngine.Vector3.zero));
        }

        [Test]
        public void PlayerMotorMath_VerticalIntent_IsIgnored()
        {
            var settings = new MovementSettings();
            var velocity = PlayerMotorMath.StepPlanarVelocity(
                new UnityEngine.Vector3(0f, 5f, 0f), UnityEngine.Vector3.up, false, settings, 1f);

            Assert.That(velocity, Is.EqualTo(UnityEngine.Vector3.zero));
        }

        [Test]
        public void PlayerMotorMath_SameDurationAt30And60Fps_ReachesSameSpeed()
        {
            var settings = new MovementSettings();
            var at30 = UnityEngine.Vector3.zero;
            var at60 = UnityEngine.Vector3.zero;
            for (var i = 0; i < 6; i++)
            {
                at30 = PlayerMotorMath.StepPlanarVelocity(at30, Forward, false, settings, 1f / 30f);
            }

            for (var i = 0; i < 12; i++)
            {
                at60 = PlayerMotorMath.StepPlanarVelocity(at60, Forward, false, settings, 1f / 60f);
            }

            Assert.That(at30.z, Is.EqualTo(at60.z).Within(Tolerance));
        }

        [Test]
        public void PlayerMotorMath_Grounded_HoldsStickSpeed()
        {
            var settings = new MovementSettings();
            Assert.That(PlayerMotorMath.StepVerticalVelocity(-10f, true, settings, 1f / 60f),
                Is.EqualTo(-settings.groundedStickSpeed));
        }

        [Test]
        public void PlayerMotorMath_Falling_AcceleratesUntilMaxFallSpeed()
        {
            var settings = new MovementSettings();
            Assert.That(PlayerMotorMath.StepVerticalVelocity(0f, false, settings, 0.5f),
                Is.EqualTo(-settings.gravity * 0.5f).Within(Tolerance));
            Assert.That(PlayerMotorMath.StepVerticalVelocity(-49f, false, settings, 1f),
                Is.EqualTo(-settings.maxFallSpeed));
        }
    }
}
