using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TopDownShooter.Core;

namespace TopDownShooter.UI
{
    /// <summary>
    /// Individual player HUD component.
    /// Displays health, score, and lives using event-based updates.
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI livesText;

        [Header("Health Bar Colors")]
        [SerializeField] private Color healthyColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color warnColor = new Color(0.9f, 0.7f, 0.1f);
        [SerializeField] private Color criticalColor = new Color(0.9f, 0.2f, 0.2f);

        private int _playerIndex;
        private int _currentHealth;
        private int _maxHealth;
        private int _score;
        private int _lives;

        /// <summary>
        /// Initializes the HUD for a specific player.
        /// </summary>
        public void Initialize(int playerIndex, int maxHealth, int startingLives, Color playerColor)
        {
            _playerIndex = playerIndex;
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _lives = startingLives;
            _score = 0;

            // Set player name with color
            if (playerNameText != null)
            {
                playerNameText.text = $"P{playerIndex + 1}";
                playerNameText.color = playerColor;
            }

            // Subscribe to events
            SubscribeToEvents();

            // Update display
            UpdateHealthDisplay();
            UpdateScoreDisplay();
            UpdateLivesDisplay();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnPlayerHealthChanged += HandleHealthChanged;
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnPlayerDeath += HandlePlayerDeath;
            GameEvents.OnPlayerRespawn += HandlePlayerRespawn;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnPlayerHealthChanged -= HandleHealthChanged;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnPlayerDeath -= HandlePlayerDeath;
            GameEvents.OnPlayerRespawn -= HandlePlayerRespawn;
        }

        private void HandleHealthChanged(int playerIndex, int currentHealth, int maxHealth)
        {
            if (playerIndex != _playerIndex) return;

            _currentHealth = currentHealth;
            _maxHealth = maxHealth;
            UpdateHealthDisplay();
        }

        private void HandleScoreChanged(int playerIndex, int newScore)
        {
            if (playerIndex != _playerIndex) return;

            _score = newScore;
            UpdateScoreDisplay();
        }

        private void HandlePlayerDeath(int playerIndex, int remainingLives)
        {
            if (playerIndex != _playerIndex) return;

            _lives = remainingLives;
            UpdateLivesDisplay();
        }

        private void HandlePlayerRespawn(int playerIndex, Vector2 position)
        {
            if (playerIndex != _playerIndex) return;

            _currentHealth = _maxHealth;
            UpdateHealthDisplay();
        }

        private void UpdateHealthDisplay()
        {
            float healthPercent = _maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0;

            // Update health bar fill
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = healthPercent;

                // Update color based on health
                if (healthPercent > 0.6f)
                {
                    healthBarFill.color = healthyColor;
                }
                else if (healthPercent > 0.3f)
                {
                    healthBarFill.color = warnColor;
                }
                else
                {
                    healthBarFill.color = criticalColor;
                }
            }

            // Update health text
            if (healthText != null)
            {
                healthText.text = $"{_currentHealth}/{_maxHealth}";
            }
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {_score}";
            }
        }

        private void UpdateLivesDisplay()
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {_lives}";
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }
}
