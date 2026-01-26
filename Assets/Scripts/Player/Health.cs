using System;
using UnityEngine;
using TopDownShooter.Core;

namespace TopDownShooter.Player
{
    /// <summary>
    /// Reusable health component for any damageable entity.
    /// Implements damage, healing, and death logic.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invincibilityDuration = 0.5f;

        private int _currentHealth;
        private float _invincibilityTimer;
        private int _ownerIndex = -1;

        /// <summary>
        /// Event fired when health changes. Parameters: currentHealth, maxHealth
        /// </summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>
        /// Event fired when damage is taken. Parameters: damageAmount, attackerIndex
        /// </summary>
        public event Action<int, int> OnDamageTaken;

        /// <summary>
        /// Event fired when entity dies.
        /// </summary>
        public event Action OnDeath;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => _currentHealth > 0;
        public bool IsInvincible => _invincibilityTimer > 0;
        public float HealthPercent => maxHealth > 0 ? (float)_currentHealth / maxHealth : 0;

        /// <summary>
        /// Initializes the health component with configuration.
        /// </summary>
        public void Initialize(int maxHp, float invincibility, int ownerIndex)
        {
            maxHealth = maxHp;
            invincibilityDuration = invincibility;
            _ownerIndex = ownerIndex;
            _currentHealth = maxHealth;
            _invincibilityTimer = 0;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        /// <summary>
        /// Resets health to maximum.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            _invincibilityTimer = 0;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        /// <summary>
        /// Applies damage to the entity.
        /// </summary>
        /// <param name="amount">Amount of damage to apply</param>
        /// <param name="attackerIndex">Index of the attacking player (-1 for environment)</param>
        /// <returns>True if damage was applied</returns>
        public bool TakeDamage(int amount, int attackerIndex = -1)
        {
            if (!IsAlive || IsInvincible || amount <= 0)
            {
                return false;
            }

            // Prevent self-damage
            if (attackerIndex >= 0 && attackerIndex == _ownerIndex)
            {
                return false;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _invincibilityTimer = invincibilityDuration;

            OnDamageTaken?.Invoke(amount, attackerIndex);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            // Trigger global event
            if (_ownerIndex >= 0)
            {
                GameEvents.TriggerPlayerDamaged(_ownerIndex, _currentHealth, maxHealth, amount);
            }

            if (_currentHealth <= 0)
            {
                Die();
            }

            return true;
        }

        /// <summary>
        /// Heals the entity.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        /// <returns>Actual amount healed</returns>
        public int Heal(int amount)
        {
            if (!IsAlive || amount <= 0)
            {
                return 0;
            }

            int previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            int actualHeal = _currentHealth - previousHealth;

            if (actualHeal > 0)
            {
                OnHealthChanged?.Invoke(_currentHealth, maxHealth);

                if (_ownerIndex >= 0)
                {
                    GameEvents.TriggerPlayerHealthChanged(_ownerIndex, _currentHealth, maxHealth);
                }
            }

            return actualHeal;
        }

        /// <summary>
        /// Grants temporary invincibility.
        /// </summary>
        public void GrantInvincibility(float duration)
        {
            _invincibilityTimer = Mathf.Max(_invincibilityTimer, duration);
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }

        private void Update()
        {
            if (_invincibilityTimer > 0)
            {
                _invincibilityTimer -= Time.deltaTime;
            }
        }
    }
}
