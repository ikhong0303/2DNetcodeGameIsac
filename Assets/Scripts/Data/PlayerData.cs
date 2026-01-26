using UnityEngine;

namespace TopDownShooter.Data
{
    /// <summary>
    /// ScriptableObject containing player configuration data.
    /// Allows designers to tweak player stats without modifying code.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "TopDownShooter/Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Base movement speed in units per second")]
        [SerializeField, Range(1f, 20f)] private float moveSpeed = 8f;

        [Tooltip("How quickly the player reaches max speed")]
        [SerializeField, Range(1f, 50f)] private float acceleration = 25f;

        [Tooltip("How quickly the player stops")]
        [SerializeField, Range(1f, 50f)] private float deceleration = 20f;

        [Header("Health")]
        [Tooltip("Maximum health points")]
        [SerializeField, Range(1, 1000)] private int maxHealth = 100;

        [Tooltip("Time in seconds of invincibility after taking damage")]
        [SerializeField, Range(0f, 3f)] private float invincibilityDuration = 0.5f;

        [Header("Visual")]
        [Tooltip("Color tint for this player")]
        [SerializeField] private Color playerColor = Color.white;

        [Tooltip("Sprite to use for the player")]
        [SerializeField] private Sprite playerSprite;

        // Public accessors (read-only)
        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public int MaxHealth => maxHealth;
        public float InvincibilityDuration => invincibilityDuration;
        public Color PlayerColor => playerColor;
        public Sprite PlayerSprite => playerSprite;
    }
}
