using System;

namespace PastFuture.UnityAdapters.Player
{
    /// <summary>
    /// First-person movement tuning for <see cref="PlayerMotor"/> (Handover 16-8 MovementProfile candidate).
    /// M1 values are provisional and measured against the 20x game clock later (M1 step 6).
    /// All speeds are real-time metres per second: Unity physics time is independent of the game clock (16-3, 16-17).
    /// </summary>
    [Serializable]
    public sealed class MovementSettings
    {
        /// <summary>Walking speed in m/s.</summary>
        public float walkSpeed = 3.0f;

        /// <summary>Sprint speed in m/s while the sprint input is held.</summary>
        public float sprintSpeed = 5.5f;

        /// <summary>Planar speed change per second while there is move input (m/s²).</summary>
        public float acceleration = 20f;

        /// <summary>Planar speed change per second with no move input (m/s²).</summary>
        public float deceleration = 25f;

        /// <summary>Downward acceleration (m/s², positive value).</summary>
        public float gravity = 9.81f;

        /// <summary>Small downward speed kept while grounded so the controller stays on slopes and steps.</summary>
        public float groundedStickSpeed = 2f;

        /// <summary>Fastest allowed fall speed (m/s, positive value).</summary>
        public float maxFallSpeed = 50f;
    }
}
