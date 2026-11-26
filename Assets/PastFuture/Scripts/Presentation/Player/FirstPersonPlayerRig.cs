using PastFuture.UnityAdapters.Player;

namespace PastFuture.Presentation.Player
{
    /// <summary>
    /// First-person rig view (Handover 16-8, 16-17 Player Rig: Input, CameraPivot).
    /// Reads the Gameplay input map in Update, turns it into a world-space move intent for
    /// <see cref="PlayerMotor"/>, and applies yaw to the body and pitch to the camera pivot in LateUpdate.
    /// Continuous movement is merged input, not a Command (16-8). Holds only its own scene
    /// references; it uses no services and writes no WorldState.
    /// </summary>
    [UnityEngine.DisallowMultipleComponent]
    [UnityEngine.DefaultExecutionOrder(-100)] // Read input before PlayerMotor.Update moves the body.
    public sealed class FirstPersonPlayerRig : UnityEngine.MonoBehaviour
    {
        public const string GameplayMap = "Gameplay";
        public const string UiMap = "UI";

        [UnityEngine.SerializeField] private UnityEngine.InputSystem.InputActionAsset controls;
        [UnityEngine.SerializeField] private PlayerMotor motor;
        [UnityEngine.SerializeField] private UnityEngine.Transform cameraPivot;
        [UnityEngine.SerializeField] private LookSettings look = new LookSettings();

        private UnityEngine.InputSystem.InputActionMap _gameplay;
        private UnityEngine.InputSystem.InputActionMap _ui;
        private UnityEngine.InputSystem.InputAction _move;
        private UnityEngine.InputSystem.InputAction _look;
        private UnityEngine.InputSystem.InputAction _sprint;
        private UnityEngine.InputSystem.InputAction _releaseCursor;
        private UnityEngine.InputSystem.InputAction _captureCursor;
        private FirstPersonLookState _lookState;
        private bool _ready;

        public FirstPersonLookState LookState => _lookState;

        private void Awake()
        {
            if (controls == null || motor == null || cameraPivot == null)
            {
                UnityEngine.Debug.LogError("[PastFuture.Presentation] FirstPersonPlayerRig is missing controls, motor or camera pivot.", this);
                enabled = false;
                return;
            }

            _gameplay = controls.FindActionMap(GameplayMap, throwIfNotFound: true);
            _ui = controls.FindActionMap(UiMap, throwIfNotFound: true);
            _move = _gameplay.FindAction("Move", throwIfNotFound: true);
            _look = _gameplay.FindAction("Look", throwIfNotFound: true);
            _sprint = _gameplay.FindAction("Sprint", throwIfNotFound: true);
            _releaseCursor = _ui.FindAction("ReleaseCursor", throwIfNotFound: true);
            _captureCursor = _ui.FindAction("CaptureCursor", throwIfNotFound: true);
            _lookState = new FirstPersonLookState(motor.transform.eulerAngles.y, cameraPivot.localEulerAngles.x, look);
            _ready = true;
        }

        private void OnEnable()
        {
            if (!_ready)
            {
                return;
            }

            _gameplay.Enable();
            _ui.Enable();
            SetCursorCaptured(true);
        }

        private void OnDisable()
        {
            if (!_ready)
            {
                return;
            }

            _gameplay.Disable();
            _ui.Disable();
            motor.SetMoveIntent(UnityEngine.Vector3.zero, false);
            SetCursorCaptured(false);
        }

        private void Update()
        {
            if (_releaseCursor.WasPressedThisFrame())
            {
                SetCursorCaptured(false);
            }
            else if (_captureCursor.WasPressedThisFrame())
            {
                SetCursorCaptured(true);
            }

            var captured = UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked;
            if (captured)
            {
                _lookState.Apply(_look.ReadValue<UnityEngine.Vector2>(), look);
            }

            var input = captured ? _move.ReadValue<UnityEngine.Vector2>() : UnityEngine.Vector2.zero;
            var yaw = UnityEngine.Quaternion.Euler(0f, _lookState.Yaw, 0f);
            motor.SetMoveIntent(yaw * new UnityEngine.Vector3(input.x, 0f, input.y), captured && _sprint.IsPressed());
        }

        private void LateUpdate()
        {
            motor.transform.rotation = UnityEngine.Quaternion.Euler(0f, _lookState.Yaw, 0f);
            cameraPivot.localRotation = UnityEngine.Quaternion.Euler(_lookState.Pitch, 0f, 0f);
        }

        private static void SetCursorCaptured(bool captured)
        {
            UnityEngine.Cursor.lockState = captured ? UnityEngine.CursorLockMode.Locked : UnityEngine.CursorLockMode.None;
            UnityEngine.Cursor.visible = !captured;
        }
    }
}
