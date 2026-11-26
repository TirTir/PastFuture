namespace PastFuture.Presentation.Player
{
    /// <summary>
    /// Yaw/pitch accumulated from pointer deltas. Pure state so EditMode tests can check it.
    /// Yaw is kept in [0, 360), pitch is clamped to the settings range.
    /// </summary>
    public sealed class FirstPersonLookState
    {
        public FirstPersonLookState(float yaw, float pitch, LookSettings settings)
        {
            Yaw = WrapYaw(yaw);
            Pitch = ClampPitch(NormalizeAngle(pitch), settings);
        }

        public float Yaw { get; private set; }

        public float Pitch { get; private set; }

        /// <summary>Applies one pointer delta (pixels). Moving the pointer up looks up.</summary>
        public void Apply(UnityEngine.Vector2 pointerDelta, LookSettings settings)
        {
            var vertical = settings.invertY ? -pointerDelta.y : pointerDelta.y;
            Yaw = WrapYaw(Yaw + pointerDelta.x * settings.sensitivity);
            Pitch = ClampPitch(Pitch - vertical * settings.sensitivity, settings);
        }

        private static float WrapYaw(float yaw)
        {
            var wrapped = yaw % 360f;
            return wrapped < 0f ? wrapped + 360f : wrapped;
        }

        private static float NormalizeAngle(float angle)
        {
            var wrapped = WrapYaw(angle);
            return wrapped > 180f ? wrapped - 360f : wrapped;
        }

        private static float ClampPitch(float pitch, LookSettings settings)
        {
            return UnityEngine.Mathf.Clamp(pitch, settings.minPitch, settings.maxPitch);
        }
    }
}
