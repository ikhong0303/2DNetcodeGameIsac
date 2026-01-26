using UnityEngine;
using Unity.Cinemachine;
using TopDownShooter.Data;

namespace TopDownShooter.Camera
{
    /// <summary>
    /// Manages split-screen camera setup using Cinemachine.
    /// Creates and configures cameras for each player.
    /// </summary>
    public class SplitScreenManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private bool verticalSplit = true;

        [Header("Visual")]
        [SerializeField] private Color splitLineColor = Color.black;
        [SerializeField] private float splitLineWidth = 4f;

        private UnityEngine.Camera[] _cameras;
        private CinemachineCamera[] _virtualCameras;
        private Transform[] _targets;

        public static SplitScreenManager Instance { get; private set; }

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
        /// Initializes the split-screen system for the given number of players.
        /// </summary>
        public void Initialize(int playerCount, Transform[] playerTransforms)
        {
            if (playerCount <= 0 || playerTransforms == null)
            {
                Debug.LogError("Invalid player count or transforms");
                return;
            }

            _targets = playerTransforms;
            _cameras = new UnityEngine.Camera[playerCount];
            _virtualCameras = new CinemachineCamera[playerCount];

            // Destroy existing cameras except main
            var existingCameras = FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None);
            foreach (var cam in existingCameras)
            {
                if (cam.CompareTag("MainCamera"))
                {
                    // Keep main camera but disable it for split screen
                    cam.enabled = false;
                }
            }

            // Create cameras for each player
            for (int i = 0; i < playerCount; i++)
            {
                CreatePlayerCamera(i, playerCount);
            }

            Debug.Log($"Split-screen initialized for {playerCount} players");
        }

        private void CreatePlayerCamera(int playerIndex, int totalPlayers)
        {
            // Create camera container
            var cameraContainer = new GameObject($"Player{playerIndex + 1}_Camera");
            cameraContainer.transform.SetParent(transform);

            // Create and configure Unity Camera
            var cam = cameraContainer.AddComponent<UnityEngine.Camera>();
            cam.orthographic = true;
            cam.orthographicSize = gameSettings != null ? gameSettings.CameraSize : 8f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            cam.depth = playerIndex;

            // Set viewport based on split configuration
            SetCameraViewport(cam, playerIndex, totalPlayers);

            _cameras[playerIndex] = cam;

            // Create Cinemachine brain
            var brain = cameraContainer.AddComponent<CinemachineBrain>();

            // Create virtual camera
            var vcamObj = new GameObject($"Player{playerIndex + 1}_VirtualCamera");
            vcamObj.transform.SetParent(cameraContainer.transform);
            vcamObj.transform.position = new Vector3(0, 0, -10);

            var vcam = vcamObj.AddComponent<CinemachineCamera>();
            vcam.Lens.OrthographicSize = gameSettings != null ? gameSettings.CameraSize : 8f;
            vcam.Lens.NearClipPlane = 0.1f;
            vcam.Lens.FarClipPlane = 100f;

            // Add position composer for following
            var composer = vcamObj.AddComponent<CinemachinePositionComposer>();
            composer.Damping = new Vector3(
                gameSettings != null ? gameSettings.CameraSmoothTime : 0.1f,
                gameSettings != null ? gameSettings.CameraSmoothTime : 0.1f,
                0
            );

            // Set follow target
            if (_targets != null && playerIndex < _targets.Length && _targets[playerIndex] != null)
            {
                vcam.Follow = _targets[playerIndex];
            }

            _virtualCameras[playerIndex] = vcam;

            // Assign channel to camera brain
            brain.ChannelMask = (OutputChannels)(1 << playerIndex);
            vcam.OutputChannel = (OutputChannels)(1 << playerIndex);
        }

        private void SetCameraViewport(UnityEngine.Camera cam, int playerIndex, int totalPlayers)
        {
            if (totalPlayers == 1)
            {
                cam.rect = new Rect(0, 0, 1, 1);
            }
            else if (totalPlayers == 2)
            {
                if (verticalSplit)
                {
                    // Left/Right split
                    float width = 0.5f - (splitLineWidth / Screen.width / 2);
                    cam.rect = playerIndex == 0
                        ? new Rect(0, 0, width, 1)
                        : new Rect(0.5f + (splitLineWidth / Screen.width / 2), 0, width, 1);
                }
                else
                {
                    // Top/Bottom split
                    float height = 0.5f - (splitLineWidth / Screen.height / 2);
                    cam.rect = playerIndex == 0
                        ? new Rect(0, 0.5f + (splitLineWidth / Screen.height / 2), 1, height)
                        : new Rect(0, 0, 1, height);
                }
            }
        }

        /// <summary>
        /// Updates the follow target for a specific player camera.
        /// </summary>
        public void SetPlayerTarget(int playerIndex, Transform target)
        {
            if (_virtualCameras == null || playerIndex < 0 || playerIndex >= _virtualCameras.Length)
            {
                return;
            }

            _targets[playerIndex] = target;

            if (_virtualCameras[playerIndex] != null)
            {
                _virtualCameras[playerIndex].Follow = target;
            }
        }

        /// <summary>
        /// Gets the camera for a specific player.
        /// </summary>
        public UnityEngine.Camera GetPlayerCamera(int playerIndex)
        {
            if (_cameras == null || playerIndex < 0 || playerIndex >= _cameras.Length)
            {
                return null;
            }
            return _cameras[playerIndex];
        }

        /// <summary>
        /// Toggles split orientation between vertical and horizontal.
        /// </summary>
        public void ToggleSplitOrientation()
        {
            verticalSplit = !verticalSplit;

            if (_cameras != null)
            {
                for (int i = 0; i < _cameras.Length; i++)
                {
                    SetCameraViewport(_cameras[i], i, _cameras.Length);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnGUI()
        {
            // Draw split line
            if (_cameras != null && _cameras.Length == 2)
            {
                GUI.color = splitLineColor;

                if (verticalSplit)
                {
                    float x = Screen.width / 2f - splitLineWidth / 2f;
                    GUI.DrawTexture(new Rect(x, 0, splitLineWidth, Screen.height), Texture2D.whiteTexture);
                }
                else
                {
                    float y = Screen.height / 2f - splitLineWidth / 2f;
                    GUI.DrawTexture(new Rect(0, y, Screen.width, splitLineWidth), Texture2D.whiteTexture);
                }

                GUI.color = Color.white;
            }
        }
    }
}
