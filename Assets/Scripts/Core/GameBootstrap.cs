using UnityEngine;
using UnityEngine.InputSystem;
using TopDownShooter.Data;
using TopDownShooter.Camera;
using TopDownShooter.Pooling;
using TopDownShooter.UI;

namespace TopDownShooter.Core
{
    /// <summary>
    /// Bootstrap component that sets up the entire game scene at runtime.
    /// Add this to an empty scene to auto-create all required objects.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Game Configuration")]
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private PlayerData player1Data;
        [SerializeField] private PlayerData player2Data;
        [SerializeField] private WeaponData defaultWeapon;
        [SerializeField] private InputActionAsset inputActions;

        private void Awake()
        {
            SetupGame();
        }

        private void SetupGame()
        {
            // Create default configurations if not assigned
            CreateDefaultConfigsIfNeeded();

            // Create Pool Manager
            CreatePoolManager();

            // Create Arena
            CreateArena();

            // Create Game Manager
            CreateGameManager();

            // Destroy bootstrap after setup
            Destroy(this);
        }

        private void CreateDefaultConfigsIfNeeded()
        {
            if (gameSettings == null)
            {
                gameSettings = ScriptableObject.CreateInstance<GameSettings>();
            }

            if (player1Data == null)
            {
                player1Data = CreatePlayerData(0, new Color(0.3f, 0.6f, 1f));
            }

            if (player2Data == null)
            {
                player2Data = CreatePlayerData(1, new Color(1f, 0.4f, 0.4f));
            }

            if (defaultWeapon == null)
            {
                defaultWeapon = ScriptableObject.CreateInstance<WeaponData>();
            }
        }

        private PlayerData CreatePlayerData(int index, Color color)
        {
            var data = ScriptableObject.CreateInstance<PlayerData>();
            SetPrivateField(data, "moveSpeed", 8f);
            SetPrivateField(data, "acceleration", 25f);
            SetPrivateField(data, "deceleration", 20f);
            SetPrivateField(data, "maxHealth", 100);
            SetPrivateField(data, "invincibilityDuration", 0.5f);
            SetPrivateField(data, "playerColor", color);
            return data;
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        private void CreatePoolManager()
        {
            var poolObj = new GameObject("PoolManager");
            poolObj.AddComponent<PoolManager>();
        }

        private void CreateArena()
        {
            var arenaObj = new GameObject("Arena");
            var arena = arenaObj.AddComponent<ArenaManager>();
            arena.Initialize(gameSettings);
        }

        private void CreateGameManager()
        {
            var gmObj = new GameObject("GameManager");
            var gm = gmObj.AddComponent<GameManager>();

            // Set configuration via reflection
            SetPrivateField(gm, "gameSettings", gameSettings);
            SetPrivateField(gm, "playerDataAssets", new PlayerData[] { player1Data, player2Data });
            SetPrivateField(gm, "defaultWeaponData", defaultWeapon);
            SetPrivateField(gm, "inputActions", inputActions);

            // Create spawn points
            var spawnPoints = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                var sp = new GameObject($"SpawnPoint_{i + 1}");
                sp.transform.position = i == 0 ? new Vector3(-5, 0, 0) : new Vector3(5, 0, 0);
                spawnPoints[i] = sp.transform;
            }
            SetPrivateField(gm, "spawnPoints", spawnPoints);
        }
    }
}
