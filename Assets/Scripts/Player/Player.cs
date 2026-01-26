using UnityEngine;
using UnityEngine.InputSystem;
using TopDownShooter.Core;
using TopDownShooter.Data;
using TopDownShooter.Combat;

namespace TopDownShooter.Player
{
    /// <summary>
    /// Main player component that coordinates all player-related subsystems.
    /// Acts as a facade for the player entity.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(Health))]
    public class Player : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int playerIndex;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private WeaponData weaponData;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform firePoint;

        private PlayerController _controller;
        private Health _health;
        private Weapon _weapon;
        private PlayerInput _playerInput;

        private int _score;
        private int _lives;
        private bool _isActive;

        public int PlayerIndex => playerIndex;
        public int Score => _score;
        public int Lives => _lives;
        public bool IsActive => _isActive;
        public bool IsAlive => _health != null && _health.IsAlive;
        public Health Health => _health;
        public PlayerController Controller => _controller;
        public PlayerData Data => playerData;

        /// <summary>
        /// Initializes the player with all required data.
        /// </summary>
        public void Initialize(int index, PlayerData data, WeaponData weapon, PlayerInput input, int startingLives)
        {
            playerIndex = index;
            playerData = data;
            weaponData = weapon;
            _playerInput = input;
            _lives = startingLives;
            _isActive = true;

            // Get components
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<Health>();

            // Setup components
            SetupComponents();

            // Apply visual customization
            ApplyVisuals();

            // Subscribe to events
            SubscribeToEvents();

            Debug.Log($"Player {playerIndex} initialized with {_lives} lives");
        }

        private void SetupComponents()
        {
            // Initialize controller
            _controller.Initialize(playerData, _playerInput);

            // Initialize health
            _health.Initialize(playerData.MaxHealth, playerData.InvincibilityDuration, playerIndex);

            // Create weapon
            _weapon = gameObject.AddComponent<Weapon>();
            _weapon.Initialize(weaponData, playerIndex, firePoint);
        }

        private void ApplyVisuals()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = playerData.PlayerColor;

                if (playerData.PlayerSprite != null)
                {
                    spriteRenderer.sprite = playerData.PlayerSprite;
                }
            }
        }

        private void SubscribeToEvents()
        {
            _health.OnDeath += HandleDeath;
            _health.OnDamageTaken += HandleDamageTaken;
        }

        private void Update()
        {
            if (!_isActive || !IsAlive) return;

            // Handle weapon firing
            if (_controller.IsFireHeld && _weapon != null)
            {
                _weapon.TryFire(_controller.AimDirection);
            }

            // Update visual rotation based on aim direction
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            Vector2 aim = _controller.AimDirection;
            if (aim.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void HandleDeath()
        {
            _lives--;
            _isActive = false;
            _controller.StopMovement();

            GameEvents.TriggerPlayerDeath(playerIndex, _lives);

            Debug.Log($"Player {playerIndex} died. Lives remaining: {_lives}");

            if (_lives > 0)
            {
                // Will be respawned by GameManager
            }
            else
            {
                // Permanent death - game over check handled by GameManager
                gameObject.SetActive(false);
            }
        }

        private void HandleDamageTaken(int damage, int attackerIndex)
        {
            // Visual feedback (sprite flash)
            StartCoroutine(DamageFlash());

            Debug.Log($"Player {playerIndex} took {damage} damage from Player {attackerIndex}");
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            if (spriteRenderer == null) yield break;

            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        /// <summary>
        /// Respawns the player at the given position.
        /// </summary>
        public void Respawn(Vector2 position, float invincibilityDuration)
        {
            _controller.Teleport(position);
            _health.ResetHealth();
            _health.GrantInvincibility(invincibilityDuration);
            _isActive = true;
            gameObject.SetActive(true);

            GameEvents.TriggerPlayerRespawn(playerIndex, position);

            Debug.Log($"Player {playerIndex} respawned at {position}");
        }

        /// <summary>
        /// Adds score to this player.
        /// </summary>
        public void AddScore(int amount)
        {
            _score += amount;
            GameEvents.TriggerScoreChanged(playerIndex, _score);
        }

        /// <summary>
        /// Clamps the player position to arena bounds.
        /// </summary>
        public void ClampToArenaBounds(float boundsX, float boundsY)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -boundsX, boundsX);
            pos.y = Mathf.Clamp(pos.y, -boundsY, boundsY);
            transform.position = pos;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDeath -= HandleDeath;
                _health.OnDamageTaken -= HandleDamageTaken;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw fire point
            if (firePoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(firePoint.position, 0.1f);
                Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 0.5f);
            }
        }
#endif
    }
}
