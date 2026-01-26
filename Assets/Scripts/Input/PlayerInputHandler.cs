using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownShooter.Input
{
    /// <summary>
    /// Handles local multiplayer input setup.
    /// Manages PlayerInput components for split-screen play.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset inputActions;

        private PlayerInput[] _playerInputs;

        public static PlayerInputHandler Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Sets up PlayerInput for a player GameObject.
        /// </summary>
        public PlayerInput SetupPlayerInput(GameObject playerObject, int playerIndex)
        {
            var playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = playerObject.AddComponent<PlayerInput>();
            }

            // Configure input based on player index
            playerInput.actions = inputActions;
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

            // Set control scheme based on player index
            // Player 0: WASD + Space (Keyboard1)
            // Player 1: Arrow keys + RCtrl/Numpad0 (Keyboard2)
            // Both can also use gamepads
            if (playerIndex == 0)
            {
                playerInput.defaultControlScheme = "Keyboard1";
            }
            else
            {
                playerInput.defaultControlScheme = "Keyboard2";
            }

            return playerInput;
        }

        /// <summary>
        /// Gets the input actions asset.
        /// </summary>
        public InputActionAsset GetInputActions() => inputActions;

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
