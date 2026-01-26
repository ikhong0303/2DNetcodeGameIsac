using System;
using UnityEngine;

namespace TopDownShooter.Core
{
    /// <summary>
    /// Static event hub implementing the Observer Pattern.
    /// Provides decoupled communication between game systems.
    /// </summary>
    public static class GameEvents
    {
        #region Player Events

        /// <summary>
        /// Invoked when a player takes damage.
        /// Parameters: playerIndex, currentHealth, maxHealth, damageAmount
        /// </summary>
        public static event Action<int, int, int, int> OnPlayerDamaged;

        /// <summary>
        /// Invoked when a player's health changes.
        /// Parameters: playerIndex, currentHealth, maxHealth
        /// </summary>
        public static event Action<int, int, int> OnPlayerHealthChanged;

        /// <summary>
        /// Invoked when a player dies.
        /// Parameters: playerIndex, remainingLives
        /// </summary>
        public static event Action<int, int> OnPlayerDeath;

        /// <summary>
        /// Invoked when a player respawns.
        /// Parameters: playerIndex, spawnPosition
        /// </summary>
        public static event Action<int, Vector2> OnPlayerRespawn;

        /// <summary>
        /// Invoked when a player's score changes.
        /// Parameters: playerIndex, newScore
        /// </summary>
        public static event Action<int, int> OnScoreChanged;

        #endregion

        #region Game State Events

        /// <summary>
        /// Invoked when the game starts.
        /// </summary>
        public static event Action OnGameStart;

        /// <summary>
        /// Invoked when the game is paused.
        /// </summary>
        public static event Action OnGamePaused;

        /// <summary>
        /// Invoked when the game is resumed.
        /// </summary>
        public static event Action OnGameResumed;

        /// <summary>
        /// Invoked when the game ends.
        /// Parameters: winnerPlayerIndex (-1 if draw)
        /// </summary>
        public static event Action<int> OnGameOver;

        #endregion

        #region Combat Events

        /// <summary>
        /// Invoked when a projectile is fired.
        /// Parameters: position, direction, ownerPlayerIndex
        /// </summary>
        public static event Action<Vector2, Vector2, int> OnProjectileFired;

        /// <summary>
        /// Invoked when a projectile hits something.
        /// Parameters: position, wasPlayer
        /// </summary>
        public static event Action<Vector2, bool> OnProjectileHit;

        #endregion

        #region Trigger Methods

        public static void TriggerPlayerDamaged(int playerIndex, int currentHealth, int maxHealth, int damage)
        {
            OnPlayerDamaged?.Invoke(playerIndex, currentHealth, maxHealth, damage);
        }

        public static void TriggerPlayerHealthChanged(int playerIndex, int currentHealth, int maxHealth)
        {
            OnPlayerHealthChanged?.Invoke(playerIndex, currentHealth, maxHealth);
        }

        public static void TriggerPlayerDeath(int playerIndex, int remainingLives)
        {
            OnPlayerDeath?.Invoke(playerIndex, remainingLives);
        }

        public static void TriggerPlayerRespawn(int playerIndex, Vector2 spawnPosition)
        {
            OnPlayerRespawn?.Invoke(playerIndex, spawnPosition);
        }

        public static void TriggerScoreChanged(int playerIndex, int newScore)
        {
            OnScoreChanged?.Invoke(playerIndex, newScore);
        }

        public static void TriggerGameStart()
        {
            OnGameStart?.Invoke();
        }

        public static void TriggerGamePaused()
        {
            OnGamePaused?.Invoke();
        }

        public static void TriggerGameResumed()
        {
            OnGameResumed?.Invoke();
        }

        public static void TriggerGameOver(int winnerPlayerIndex)
        {
            OnGameOver?.Invoke(winnerPlayerIndex);
        }

        public static void TriggerProjectileFired(Vector2 position, Vector2 direction, int ownerIndex)
        {
            OnProjectileFired?.Invoke(position, direction, ownerIndex);
        }

        public static void TriggerProjectileHit(Vector2 position, bool wasPlayer)
        {
            OnProjectileHit?.Invoke(position, wasPlayer);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clears all event subscriptions. Call when loading a new scene.
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPlayerDamaged = null;
            OnPlayerHealthChanged = null;
            OnPlayerDeath = null;
            OnPlayerRespawn = null;
            OnScoreChanged = null;
            OnGameStart = null;
            OnGamePaused = null;
            OnGameResumed = null;
            OnGameOver = null;
            OnProjectileFired = null;
            OnProjectileHit = null;
        }

        #endregion
    }
}
