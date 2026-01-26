#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TopDownShooter.Core;
using TopDownShooter.Data;

namespace TopDownShooter.Editor
{
    /// <summary>
    /// Editor utilities for setting up the TopDown Shooter game.
    /// </summary>
    public static class GameSetupEditor
    {
        private const string DataPath = "Assets/Data/";
        private const string PrefabPath = "Assets/Prefabs/";

        [MenuItem("TopDown Shooter/Setup/Create All Assets", false, 0)]
        public static void CreateAllAssets()
        {
            CreateFolders();
            CreateGameSettings();
            CreatePlayerData();
            CreateWeaponData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("All TopDown Shooter assets created!");
        }

        [MenuItem("TopDown Shooter/Setup/Setup New Scene", false, 20)]
        public static void SetupNewScene()
        {
            // Create GameBootstrap
            var bootstrap = new GameObject("GameBootstrap");
            var bs = bootstrap.AddComponent<GameBootstrap>();

            // Try to assign assets
            var gameSettings = AssetDatabase.LoadAssetAtPath<GameSettings>($"{DataPath}GameSettings.asset");
            var player1Data = AssetDatabase.LoadAssetAtPath<PlayerData>($"{DataPath}Player1Data.asset");
            var player2Data = AssetDatabase.LoadAssetAtPath<PlayerData>($"{DataPath}Player2Data.asset");
            var weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>($"{DataPath}DefaultWeapon.asset");
            var inputActions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                "Assets/Scripts/Input/GameInput.inputactions");

            var so = new SerializedObject(bs);
            so.FindProperty("gameSettings").objectReferenceValue = gameSettings;
            so.FindProperty("player1Data").objectReferenceValue = player1Data;
            so.FindProperty("player2Data").objectReferenceValue = player2Data;
            so.FindProperty("defaultWeapon").objectReferenceValue = weaponData;
            so.FindProperty("inputActions").objectReferenceValue = inputActions;
            so.ApplyModifiedProperties();

            Selection.activeGameObject = bootstrap;
            Debug.Log("Scene setup complete! Press Play to start the game.");
        }

        private static void CreateFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
        }

        [MenuItem("TopDown Shooter/Setup/Create Game Settings", false, 100)]
        public static void CreateGameSettings()
        {
            CreateFolders();
            var settings = ScriptableObject.CreateInstance<GameSettings>();
            AssetDatabase.CreateAsset(settings, $"{DataPath}GameSettings.asset");
            Debug.Log("GameSettings created at " + DataPath);
        }

        [MenuItem("TopDown Shooter/Setup/Create Player Data", false, 101)]
        public static void CreatePlayerData()
        {
            CreateFolders();

            // Player 1 - Blue
            var p1 = ScriptableObject.CreateInstance<PlayerData>();
            SetPrivateField(p1, "moveSpeed", 8f);
            SetPrivateField(p1, "playerColor", new Color(0.3f, 0.6f, 1f));
            AssetDatabase.CreateAsset(p1, $"{DataPath}Player1Data.asset");

            // Player 2 - Red
            var p2 = ScriptableObject.CreateInstance<PlayerData>();
            SetPrivateField(p2, "moveSpeed", 8f);
            SetPrivateField(p2, "playerColor", new Color(1f, 0.4f, 0.4f));
            AssetDatabase.CreateAsset(p2, $"{DataPath}Player2Data.asset");

            Debug.Log("PlayerData assets created at " + DataPath);
        }

        [MenuItem("TopDown Shooter/Setup/Create Weapon Data", false, 102)]
        public static void CreateWeaponData()
        {
            CreateFolders();
            var weapon = ScriptableObject.CreateInstance<WeaponData>();
            SetPrivateField(weapon, "weaponName", "Default Blaster");
            SetPrivateField(weapon, "damage", 25);
            SetPrivateField(weapon, "fireRate", 0.15f);
            SetPrivateField(weapon, "projectileSpeed", 20f);
            SetPrivateField(weapon, "projectileColor", Color.yellow);
            AssetDatabase.CreateAsset(weapon, $"{DataPath}DefaultWeapon.asset");
            Debug.Log("WeaponData created at " + DataPath);
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
#endif
