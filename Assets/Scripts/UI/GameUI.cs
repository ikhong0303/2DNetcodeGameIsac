using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TopDownShooter.Core;

namespace TopDownShooter.UI
{
    /// <summary>
    /// Main game UI controller.
    /// Manages game state UI and player HUDs using Observer Pattern.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Player HUDs")]
        [SerializeField] private PlayerHUD[] playerHUDs;

        [Header("Game State UI")]
        [SerializeField] private GameObject startScreen;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private GameObject gameOverScreen;

        [Header("Game Over")]
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        [Header("Pause")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseQuitButton;

        public static GameUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupButtons();
            SubscribeToEvents();
        }

        private void SetupButtons()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (pauseQuitButton != null)
            {
                pauseQuitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnGameStart += HandleGameStart;
            GameEvents.OnGamePaused += HandleGamePaused;
            GameEvents.OnGameResumed += HandleGameResumed;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameStart -= HandleGameStart;
            GameEvents.OnGamePaused -= HandleGamePaused;
            GameEvents.OnGameResumed -= HandleGameResumed;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        /// <summary>
        /// Initializes player HUDs.
        /// </summary>
        public void InitializePlayerHUDs(int playerCount, int maxHealth, int startingLives, Color[] playerColors)
        {
            for (int i = 0; i < playerCount && i < playerHUDs.Length; i++)
            {
                if (playerHUDs[i] != null)
                {
                    Color color = i < playerColors.Length ? playerColors[i] : Color.white;
                    playerHUDs[i].Initialize(i, maxHealth, startingLives, color);
                    playerHUDs[i].gameObject.SetActive(true);
                }
            }

            // Hide unused HUDs
            for (int i = playerCount; i < playerHUDs.Length; i++)
            {
                if (playerHUDs[i] != null)
                {
                    playerHUDs[i].gameObject.SetActive(false);
                }
            }
        }

        private void HandleGameStart()
        {
            HideAllScreens();
        }

        private void HandleGamePaused()
        {
            ShowPauseScreen();
        }

        private void HandleGameResumed()
        {
            HidePauseScreen();
        }

        private void HandleGameOver(int winnerIndex)
        {
            ShowGameOverScreen(winnerIndex);
        }

        /// <summary>
        /// Shows the start screen.
        /// </summary>
        public void ShowStartScreen()
        {
            if (startScreen != null) startScreen.SetActive(true);
            if (pauseScreen != null) pauseScreen.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
        }

        /// <summary>
        /// Shows the pause screen.
        /// </summary>
        public void ShowPauseScreen()
        {
            if (pauseScreen != null) pauseScreen.SetActive(true);
        }

        /// <summary>
        /// Hides the pause screen.
        /// </summary>
        public void HidePauseScreen()
        {
            if (pauseScreen != null) pauseScreen.SetActive(false);
        }

        /// <summary>
        /// Shows the game over screen with winner information.
        /// </summary>
        public void ShowGameOverScreen(int winnerIndex)
        {
            if (gameOverScreen != null) gameOverScreen.SetActive(true);

            if (winnerText != null)
            {
                winnerText.text = winnerIndex >= 0 ? $"Player {winnerIndex + 1} Wins!" : "Draw!";
            }
        }

        /// <summary>
        /// Hides all overlay screens.
        /// </summary>
        public void HideAllScreens()
        {
            if (startScreen != null) startScreen.SetActive(false);
            if (pauseScreen != null) pauseScreen.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
        }

        private void OnRestartClicked()
        {
            HideAllScreens();
            // GameManager will handle actual restart
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }

        private void OnResumeClicked()
        {
            GameEvents.TriggerGameResumed();
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Update()
        {
            // Handle pause input
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (pauseScreen != null && pauseScreen.activeSelf)
                {
                    GameEvents.TriggerGameResumed();
                }
                else if (gameOverScreen == null || !gameOverScreen.activeSelf)
                {
                    GameEvents.TriggerGamePaused();
                }
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
