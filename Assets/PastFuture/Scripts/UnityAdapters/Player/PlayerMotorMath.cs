namespace PastFuture.UnityAdapters.Player
{
    /// <summary>
    /// Pure velocity steps used by <see cref="PlayerMotor"/>. No scene access, so EditMode tests
    /// can check them directly. <c>deltaTime</c> is real (physics/presentation) time, never game time.
    /// </summary>
    public static class PlayerMotorMath
    {
        private const float InputDeadZoneSqr = 0.0001f;

        /// <summary>
        /// Moves the planar velocity toward <paramref name="intent"/> × speed. The intent is flattened
        /// and clamped to length 1, so diagonal input is not faster than straight input.
        /// </summary>
        public static UnityEngine.Vector3 StepPlanarVelocity(
            UnityEngine.Vector3 current, UnityEngine.Vector3 intent, bool sprint, MovementSettings settings, float deltaTime)
        {
            current.y = 0f;
            intent.y = 0f;
            if (intent.sqrMagnitude > 1f)
            {
                intent.Normalize();
            }

            var hasInput = intent.sqrMagnitude > InputDeadZoneSqr;
            var speed = sprint ? settings.sprintSpeed : settings.walkSpeed;
            var target = hasInput ? intent * speed : UnityEngine.Vector3.zero;
            var rate = hasInput ? settings.acceleration : settings.deceleration;
            return UnityEngine.Vector3.MoveTowards(current, target, rate * deltaTime);
        }

        /// <summary>
        /// Applies gravity. While grounded and not moving up, the vertical speed is held at
        /// -<see cref="MovementSettings.groundedStickSpeed"/> instead of growing.
        /// </summary>
        public static float StepVerticalVelocity(float current, bool grounded, MovementSettings settings, float deltaTime)
        {
            if (grounded && current <= 0f)
            {
                return -settings.groundedStickSpeed;
            }

            var next = current - settings.gravity * deltaTime;
            return next < -settings.maxFallSpeed ? -settings.maxFallSpeed : next;
        }
    }
}
