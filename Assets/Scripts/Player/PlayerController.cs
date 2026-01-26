using UnityEngine;
using UnityEngine.InputSystem;
using TopDownShooter.Data;

namespace TopDownShooter.Player
{
    /// <summary>
    /// Handles player input and movement using the new Input System.
    /// Supports both keyboard and gamepad controls.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerData playerData;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;
        private Vector2 _aimDirection;
        private Vector2 _currentVelocity;
        private bool _fireHeld;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _aimAction;
        private InputAction _fireAction;

        public Vector2 MoveInput => _moveInput;
        public Vector2 AimDirection => _aimDirection.sqrMagnitude > 0.01f ? _aimDirection.normalized : GetMoveBasedAim();
        public bool IsFireHeld => _fireHeld;
        public PlayerData Data => playerData;

        /// <summary>
        /// Initializes the controller with player data and input.
        /// </summary>
        public void Initialize(PlayerData data, PlayerInput input)
        {
            playerData = data;
            _playerInput = input;
            SetupInputActions();
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Try to get PlayerInput if not set via Initialize
            if (_playerInput == null)
            {
                _playerInput = GetComponent<PlayerInput>();
            }

            if (_playerInput != null)
            {
                SetupInputActions();
            }
        }

        private void SetupInputActions()
        {
            if (_playerInput == null) return;

            _moveAction = _playerInput.actions["Move"];
            _aimAction = _playerInput.actions["Aim"];
            _fireAction = _playerInput.actions["Fire"];

            // Subscribe to fire action
            if (_fireAction != null)
            {
                _fireAction.started += OnFireStarted;
                _fireAction.canceled += OnFireCanceled;
            }
        }

        private void OnFireStarted(InputAction.CallbackContext ctx) => _fireHeld = true;
        private void OnFireCanceled(InputAction.CallbackContext ctx) => _fireHeld = false;

        private void Update()
        {
            // Read input values
            if (_moveAction != null)
            {
                _moveInput = _moveAction.ReadValue<Vector2>();
            }

            if (_aimAction != null)
            {
                _aimDirection = _aimAction.ReadValue<Vector2>();
            }
        }

        private void FixedUpdate()
        {
            if (playerData == null) return;

            ApplyMovement();
        }

        private void ApplyMovement()
        {
            Vector2 targetVelocity = _moveInput * playerData.MoveSpeed;

            // Smooth acceleration/deceleration
            float accel = _moveInput.sqrMagnitude > 0.01f ? playerData.Acceleration : playerData.Deceleration;
            _currentVelocity = Vector2.MoveTowards(_currentVelocity, targetVelocity, accel * Time.fixedDeltaTime);

            _rigidbody.linearVelocity = _currentVelocity;
        }

        private Vector2 GetMoveBasedAim()
        {
            // If no explicit aim, use movement direction
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                return _moveInput.normalized;
            }
            // Default to facing right
            return Vector2.right;
        }

        /// <summary>
        /// Stops all movement immediately.
        /// </summary>
        public void StopMovement()
        {
            _moveInput = Vector2.zero;
            _currentVelocity = Vector2.zero;
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Teleports the player to a position.
        /// </summary>
        public void Teleport(Vector2 position)
        {
            StopMovement();
            transform.position = position;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_fireAction != null)
            {
                _fireAction.started -= OnFireStarted;
                _fireAction.canceled -= OnFireCanceled;
            }
        }
    }
}
