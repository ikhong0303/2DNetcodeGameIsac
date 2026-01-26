using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TopDownShooter.Data;
using TopDownShooter.Player;
using TopDownShooter.Camera;
using TopDownShooter.Pooling;
using TopDownShooter.UI;

namespace TopDownShooter.Core
{
    /// <summary>
    /// Main game coordinator. Manages game state, player spawning,
    /// and coordinates all game systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private PlayerData[] playerDataAssets;
        [SerializeField] private WeaponData defaultWeaponData;
        [SerializeField] private InputActionAsset inputActions;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("References")]
        [SerializeField] private SplitScreenManager splitScreenManager;
        [SerializeField] private GameUI gameUI;

        private Player.Player[] _players;
        private GameState _currentState;
        private int _playerCount = 2;
        private bool _isPaused;

        public static GameManager Instance { get; private set; }
        public GameState CurrentState => _currentState;
        public GameSettings Settings => gameSettings;

        public enum GameState
        {
            Initializing,
            WaitingToStart,
            Playing,
            Paused,
            GameOver
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _currentState = GameState.Initializing;
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Clear any old event subscriptions
            GameEvents.ClearAllEvents();

            // Subscribe to events
            SubscribeToEvents();

            // Setup pool manager
            SetupPoolManager();

            // Spawn players
            SpawnPlayers();

            // Initialize split-screen cameras
            InitializeCameras();

            // Initialize UI
            InitializeUI();

            // Start game
            StartGame();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnPlayerDeath += HandlePlayerDeath;
            GameEvents.OnGamePaused += HandleGamePaused;
            GameEvents.OnGameResumed += HandleGameResumed;
        }

        private void SetupPoolManager()
        {
            if (FindFirstObjectByType<PoolManager>() == null)
            {
                var poolObj = new GameObject("PoolManager");
                poolObj.AddComponent<PoolManager>();
            }
        }

        private void SpawnPlayers()
        {
            _players = new Player.Player[_playerCount];

            for (int i = 0; i < _playerCount; i++)
            {
                // Get spawn position
                Vector2 spawnPos = GetSpawnPosition(i);

                // Create player object
                var playerObj = CreatePlayerObject(i);
                playerObj.transform.position = spawnPos;

                // Get components
                var player = playerObj.GetComponent<Player.Player>();

                // Setup PlayerInput
                var playerInput = playerObj.AddComponent<PlayerInput>();
                playerInput.actions = inputActions;
                playerInput.defaultControlScheme = i == 0 ? "Keyboard1" : "Keyboard2";
                playerInput.neverAutoSwitchControlSchemes = true;

                // Get player data
                PlayerData data = i < playerDataAssets.Length && playerDataAssets[i] != null
                    ? playerDataAssets[i]
                    : CreateDefaultPlayerData(i);

                // Initialize player
                player.Initialize(
                    i,
                    data,
                    defaultWeaponData != null ? defaultWeaponData : CreateDefaultWeaponData(),
                    playerInput,
                    gameSettings != null ? gameSettings.StartingLives : 3
                );

                _players[i] = player;

                Debug.Log($"Spawned Player {i} at {spawnPos}");
            }
        }

        private GameObject CreatePlayerObject(int playerIndex)
        {
            var obj = new GameObject($"Player_{playerIndex + 1}");

            // Add Rigidbody2D
            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Add Collider
            var collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;

            // Add SpriteRenderer
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlayerSprite();
            sr.sortingOrder = 10;

            // Create fire point
            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(obj.transform);
            firePoint.transform.localPosition = new Vector3(0.5f, 0, 0);

            // Add player components
            obj.AddComponent<PlayerController>();
            obj.AddComponent<Health>();
            var player = obj.AddComponent<Player.Player>();

            // Set sprite renderer reference via reflection or serialized field
            var spriteField = typeof(Player.Player).GetField("spriteRenderer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            spriteField?.SetValue(player, sr);

            var firePointField = typeof(Player.Player).GetField("firePoint",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            firePointField?.SetValue(player, firePoint.transform);

            // Add player layer
            obj.layer = LayerMask.NameToLayer("Default");
            obj.tag = "Player";

            return obj;
        }

        private Sprite CreatePlayerSprite()
        {
            int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            Vector2 center = new Vector2(size / 2f, size / 2f);

            // Create a triangle-ish shape pointing right
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - center.x) / (size / 2f);
                    float ny = (y - center.y) / (size / 2f);

                    // Triangle shape
                    bool inShape = nx > -0.7f && Mathf.Abs(ny) < (0.7f - nx * 0.5f);

                    if (inShape)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
        }

        private PlayerData CreateDefaultPlayerData(int playerIndex)
        {
            var data = ScriptableObject.CreateInstance<PlayerData>();

            // Use reflection to set values
            var moveSpeedField = typeof(PlayerData).GetField("moveSpeed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            moveSpeedField?.SetValue(data, 8f);

            var colorField = typeof(PlayerData).GetField("playerColor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            colorField?.SetValue(data, playerIndex == 0 ? new Color(0.3f, 0.6f, 1f) : new Color(1f, 0.4f, 0.4f));

            return data;
        }

        private WeaponData CreateDefaultWeaponData()
        {
            var data = ScriptableObject.CreateInstance<WeaponData>();
            return data;
        }

        private Vector2 GetSpawnPosition(int playerIndex)
        {
            if (spawnPoints != null && playerIndex < spawnPoints.Length && spawnPoints[playerIndex] != null)
            {
                return spawnPoints[playerIndex].position;
            }

            // Default spawn positions
            float offset = 5f;
            return playerIndex == 0
                ? new Vector2(-offset, 0)
                : new Vector2(offset, 0);
        }

        private void InitializeCameras()
        {
            if (splitScreenManager == null)
            {
                var camManagerObj = new GameObject("SplitScreenManager");
                splitScreenManager = camManagerObj.AddComponent<SplitScreenManager>();

                // Set game settings reference via reflection
                var settingsField = typeof(SplitScreenManager).GetField("gameSettings",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                settingsField?.SetValue(splitScreenManager, gameSettings);
            }

            Transform[] playerTransforms = new Transform[_players.Length];
            for (int i = 0; i < _players.Length; i++)
            {
                playerTransforms[i] = _players[i].transform;
            }

            splitScreenManager.Initialize(_playerCount, playerTransforms);
        }

        private void InitializeUI()
        {
            if (gameUI != null)
            {
                Color[] playerColors = new Color[_players.Length];
                int maxHealth = gameSettings != null ? gameSettings.StartingLives * 100 : 100;
                int startingLives = gameSettings != null ? gameSettings.StartingLives : 3;

                for (int i = 0; i < _players.Length; i++)
                {
                    playerColors[i] = _players[i].Data != null ? _players[i].Data.PlayerColor : Color.white;
                }

                gameUI.InitializePlayerHUDs(_playerCount, maxHealth, startingLives, playerColors);
            }
        }

        private void StartGame()
        {
            _currentState = GameState.Playing;
            _isPaused = false;
            Time.timeScale = 1f;

            GameEvents.TriggerGameStart();

            Debug.Log("Game Started!");
        }

        private void HandlePlayerDeath(int playerIndex, int remainingLives)
        {
            if (_currentState != GameState.Playing) return;

            if (remainingLives > 0)
            {
                // Schedule respawn
                StartCoroutine(RespawnPlayer(playerIndex));
            }
            else
            {
                // Check for game over
                CheckGameOver();
            }
        }

        private IEnumerator RespawnPlayer(int playerIndex)
        {
            float respawnDelay = gameSettings != null ? gameSettings.RespawnDelay : 2f;
            float respawnInvincibility = gameSettings != null ? gameSettings.RespawnInvincibility : 2f;

            yield return new WaitForSeconds(respawnDelay);

            if (_currentState != GameState.Playing) yield break;

            Vector2 spawnPos = GetSpawnPosition(playerIndex);
            _players[playerIndex].Respawn(spawnPos, respawnInvincibility);
        }

        private void CheckGameOver()
        {
            int aliveCount = 0;
            int winnerIndex = -1;

            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].Lives > 0)
                {
                    aliveCount++;
                    winnerIndex = i;
                }
            }

            if (aliveCount <= 1)
            {
                EndGame(winnerIndex);
            }
        }

        private void EndGame(int winnerIndex)
        {
            _currentState = GameState.GameOver;

            // Award score to winner
            if (winnerIndex >= 0)
            {
                _players[winnerIndex].AddScore(100);
            }

            GameEvents.TriggerGameOver(winnerIndex);

            Debug.Log($"Game Over! Winner: Player {winnerIndex + 1}");
        }

        private void HandleGamePaused()
        {
            if (_currentState != GameState.Playing) return;

            _currentState = GameState.Paused;
            _isPaused = true;
            Time.timeScale = 0f;
        }

        private void HandleGameResumed()
        {
            if (_currentState != GameState.Paused) return;

            _currentState = GameState.Playing;
            _isPaused = false;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (_currentState != GameState.Playing) return;

            // Clamp players to arena bounds
            if (gameSettings != null)
            {
                foreach (var player in _players)
                {
                    if (player != null && player.IsActive)
                    {
                        player.ClampToArenaBounds(gameSettings.ArenaBoundsX, gameSettings.ArenaBoundsY);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnPlayerDeath -= HandlePlayerDeath;
            GameEvents.OnGamePaused -= HandleGamePaused;
            GameEvents.OnGameResumed -= HandleGameResumed;

            if (Instance == this)
            {
                Instance = null;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw arena bounds
            if (gameSettings != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = Vector3.zero;
                Vector3 size = new Vector3(gameSettings.ArenaBoundsX * 2, gameSettings.ArenaBoundsY * 2, 0);
                Gizmos.DrawWireCube(center, size);
            }

            // Draw spawn points
            Gizmos.color = Color.green;
            if (spawnPoints != null)
            {
                foreach (var sp in spawnPoints)
                {
                    if (sp != null)
                    {
                        Gizmos.DrawWireSphere(sp.position, 0.5f);
                    }
                }
            }
        }
#endif
    }
}
