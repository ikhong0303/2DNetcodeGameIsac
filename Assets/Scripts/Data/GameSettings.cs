using UnityEngine;

namespace TopDownShooter.Data
{
    /// <summary>
    /// ScriptableObject containing global game settings.
    /// Centralized configuration for game-wide parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "TopDownShooter/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Game Rules")]
        [Tooltip("Number of lives each player starts with")]
        [SerializeField, Range(1, 10)] private int startingLives = 3;

        [Tooltip("Time in seconds before respawning")]
        [SerializeField, Range(0f, 10f)] private float respawnDelay = 2f;

        [Tooltip("Duration of invincibility after respawn")]
        [SerializeField, Range(0f, 5f)] private float respawnInvincibility = 2f;

        [Header("Arena")]
        [Tooltip("Half-width of the play area")]
        [SerializeField, Range(5f, 50f)] private float arenaBoundsX = 20f;

        [Tooltip("Half-height of the play area")]
        [SerializeField, Range(5f, 50f)] private float arenaBoundsY = 12f;

        [Header("Camera")]
        [Tooltip("Size of each player's camera view")]
        [SerializeField, Range(3f, 20f)] private float cameraSize = 8f;

        [Tooltip("Camera follow smoothing")]
        [SerializeField, Range(0.01f, 1f)] private float cameraSmoothTime = 0.1f;

        [Header("Object Pooling")]
        [Tooltip("Initial pool size for projectiles")]
        [SerializeField, Range(10, 200)] private int projectilePoolSize = 50;

        [Header("Audio")]
        [Tooltip("Master volume")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

        [Tooltip("Music volume")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

        [Tooltip("SFX volume")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;

        // Public accessors (read-only)
        public int StartingLives => startingLives;
        public float RespawnDelay => respawnDelay;
        public float RespawnInvincibility => respawnInvincibility;
        public float ArenaBoundsX => arenaBoundsX;
        public float ArenaBoundsY => arenaBoundsY;
        public float CameraSize => cameraSize;
        public float CameraSmoothTime => cameraSmoothTime;
        public int ProjectilePoolSize => projectilePoolSize;
        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;

        /// <summary>
        /// Returns the arena bounds as a Rect.
        /// </summary>
        public Rect GetArenaBounds()
        {
            return new Rect(-arenaBoundsX, -arenaBoundsY, arenaBoundsX * 2, arenaBoundsY * 2);
        }

        /// <summary>
        /// Checks if a position is within the arena bounds.
        /// </summary>
        public bool IsWithinBounds(Vector2 position)
        {
            return Mathf.Abs(position.x) <= arenaBoundsX &&
                   Mathf.Abs(position.y) <= arenaBoundsY;
        }

        /// <summary>
        /// Clamps a position to stay within arena bounds.
        /// </summary>
        public Vector2 ClampToBounds(Vector2 position)
        {
            return new Vector2(
                Mathf.Clamp(position.x, -arenaBoundsX, arenaBoundsX),
                Mathf.Clamp(position.y, -arenaBoundsY, arenaBoundsY)
            );
        }
    }
}
