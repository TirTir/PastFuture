using System;

namespace PastFuture.Presentation.Player
{
    /// <summary>Mouse look tuning. Presentation only (Handover 16-8: camera direction and sensitivity).</summary>
    [Serializable]
    public sealed class LookSettings
    {
        /// <summary>Degrees of rotation per pixel of pointer movement.</summary>
        public float sensitivity = 0.1f;

        public bool invertY;

        /// <summary>Lowest pitch (looking up), degrees. Unity pitch is positive when looking down.</summary>
        public float minPitch = -85f;

        /// <summary>Highest pitch (looking down), degrees.</summary>
        public float maxPitch = 85f;
    }
}
