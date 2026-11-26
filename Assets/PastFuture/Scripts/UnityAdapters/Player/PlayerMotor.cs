namespace PastFuture.UnityAdapters.Player
{
    /// <summary>
    /// CharacterController-based first-person body (Handover 16-8, 16-17 Player Rig; ADR-499 title).
    /// Adapter only: it moves the Unity body from a move intent and reports what happened.
    /// It owns no game rules and writes no WorldState. Moves in Update with real delta time;
    /// the 20x game clock never scales it (16-3: Time.timeScale is not used for game time).
    /// </summary>
    [UnityEngine.DisallowMultipleComponent]
    [UnityEngine.RequireComponent(typeof(UnityEngine.CharacterController))]
    public sealed class PlayerMotor : UnityEngine.MonoBehaviour
    {
        [UnityEngine.SerializeField] private MovementSettings settings = new MovementSettings();

        private UnityEngine.CharacterController _controller;
        private UnityEngine.Vector3 _intent;
        private bool _sprint;
        private UnityEngine.Vector3 _planarVelocity;
        private float _verticalVelocity;

        /// <summary>Planar velocity actually achieved in the last move (after collisions), m/s.</summary>
        public UnityEngine.Vector3 PlanarVelocity => _planarVelocity;

        /// <summary>Vertical speed used for the last move, m/s.</summary>
        public float VerticalVelocity => _verticalVelocity;

        /// <summary>Whether the controller touched ground during the last move.</summary>
        public bool IsGrounded => _controller != null && _controller.isGrounded;

        public MovementSettings Settings => settings;

        /// <summary>
        /// Sets the world-space move direction (y ignored, length clamped to 1) and sprint state.
        /// The intent stays until it is set again.
        /// </summary>
        public void SetMoveIntent(UnityEngine.Vector3 worldDirection, bool sprint)
        {
            _intent = worldDirection;
            _sprint = sprint;
        }

        private void Awake()
        {
            _controller = GetComponent<UnityEngine.CharacterController>();

            // CharacterController skips any move shorter than minMoveDistance (default 0.001 m).
            // At high frame rates a whole frame of walking is shorter than that, so the body would not move.
            _controller.minMoveDistance = 0f;
        }

        private void OnDisable()
        {
            _intent = UnityEngine.Vector3.zero;
            _sprint = false;
            _planarVelocity = UnityEngine.Vector3.zero;
            _verticalVelocity = 0f;
        }

        private void Update()
        {
            var deltaTime = UnityEngine.Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            _planarVelocity = PlayerMotorMath.StepPlanarVelocity(_planarVelocity, _intent, _sprint, settings, deltaTime);
            _verticalVelocity = PlayerMotorMath.StepVerticalVelocity(_verticalVelocity, _controller.isGrounded, settings, deltaTime);

            var flags = _controller.Move((_planarVelocity + UnityEngine.Vector3.up * _verticalVelocity) * deltaTime);

            // Keep only the velocity the controller achieved, so pushing into a wall does not build up speed.
            var achieved = _controller.velocity;
            _planarVelocity = new UnityEngine.Vector3(achieved.x, 0f, achieved.z);
            if ((flags & UnityEngine.CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
            {
                _verticalVelocity = 0f;
            }
        }
    }
}
