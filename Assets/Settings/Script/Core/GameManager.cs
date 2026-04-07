using UnityEngine;
using System;
using UnityTV.Player;

namespace UnityTV.Core
{
    /// <summary>
    /// Central game manager handling game state, scene transitions, and global game logic
    /// Singleton pattern - accessible from anywhere via GameManager.Instance
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        [Header("Player Reference")]
        public PlayerData PlayerData { get; private set; }

        [Header("Managers")]
        public TimeManager TimeManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public SaveLoadManager SaveManager { get; private set; }
        public UIManager UIManager { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool enableDebugMode = true;

        // Events
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnDayChanged;
        public event Action OnGameOver;
        public event Action OnGameWon;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeManagers();
        }

        private void Start()
        {
            // Initialize game
            ChangeGameState(GameState.MainMenu);
        }

        private void InitializeManagers()
        {
            // Get or create manager components
            TimeManager = GetComponent<TimeManager>() ?? gameObject.AddComponent<TimeManager>();
            AudioManager = GetComponent<AudioManager>() ?? gameObject.AddComponent<AudioManager>();
            SaveManager = GetComponent<SaveLoadManager>() ?? gameObject.AddComponent<SaveLoadManager>();
            UIManager = FindFirstObjectByType<UIManager>();

            // Subscribe to time events
            if (TimeManager != null)
            {
                TimeManager.OnDayEnded += HandleDayEnded;
                TimeManager.OnDeadlineReached += HandleDeadline;
            }
        }

        /// <summary>
        /// Change the current game state
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (currentState == newState) return;

            GameState previousState = currentState;
            currentState = newState;

            Log($"Game State Changed: {previousState} → {newState}");

            // Handle state transitions
            OnExitState(previousState);
            OnEnterState(newState);

            OnGameStateChanged?.Invoke(newState);
        }

        private void OnExitState(GameState state)
        {
            switch (state)
            {
                case GameState.WatchingTV:
                    // Pause time when leaving TV
                    TimeManager?.PauseTime();
                    break;
                case GameState.Combat:
                    // Clean up combat scene
                    break;
            }
        }

        private void OnEnterState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    SceneController.LoadScene("00_MainMenu");
                    AudioManager?.PlayMusic("MainMenuTheme");
                    break;

                case GameState.CharacterCreation:
                    SceneController.LoadScene("01_CharacterCreation");
                    AudioManager?.PlayMusic("CharacterCreationTheme");
                    break;

                case GameState.LivingRoom:
                    SceneController.LoadScene("02_LivingRoom");
                    AudioManager?.PlayMusic("LivingRoomAmbience");
                    TimeManager?.ResumeTime();
                    break;

                case GameState.WatchingTV:
                    SceneController.LoadScene("03_TVInterface");
                    AudioManager?.PlayMusic("TVStaticNoise");
                    break;

                case GameState.Combat:
                    SceneController.LoadScene("04_ChannelVI_Combat");
                    AudioManager?.PlayMusic("CombatTheme");
                    break;

                case GameState.GameOver:
                    HandleGameOver();
                    break;
            }
        }

        /// <summary>
        /// Start a new game with character creation
        /// </summary>
        public void StartNewGame()
        {
            Log("Starting New Game");

            PlayerData = new PlayerData();
            ChangeGameState(GameState.CharacterCreation);
        }

        /// <summary>
        /// Continue from saved game
        /// </summary>
        public void ContinueGame()
        {
            Log("Continuing Game");

            if (SaveManager.LoadGame())
            {
                PlayerData = SaveManager.GetPlayerData();
                ChangeGameState(GameState.LivingRoom);
            }
            else
            {
                Debug.LogError("Failed to load save file!");
                UIManager?.ShowNotification("No save file found!");
            }
        }

        /// <summary>
        /// Complete character creation and move to living room
        /// </summary>
        public void CompleteCharacterCreation(PlayerData playerData)
        {
            this.PlayerData = playerData;

            // Initialize time system
            TimeManager?.StartGame();

            Log($"Character Creation Complete - Player: {playerData.PlayerName}");
            ChangeGameState(GameState.LivingRoom);
        }

        /// <summary>
        /// Enter TV watching mode
        /// </summary>
        public void EnterTVMode()
        {
            if (currentState != GameState.LivingRoom)
            {
                Debug.LogWarning("Can only watch TV from living room!");
                return;
            }

            ChangeGameState(GameState.WatchingTV);
        }

        /// <summary>
        /// Exit TV and return to living room
        /// </summary>
        public void ExitTVMode()
        {
            if (currentState != GameState.WatchingTV)
            {
                Debug.LogWarning("Not currently watching TV!");
                return;
            }

            ChangeGameState(GameState.LivingRoom);
        }

        /// <summary>
        /// Enter combat mode (Channel VI)
        /// </summary>
        public void EnterCombat()
        {
            ChangeGameState(GameState.Combat);
        }

        /// <summary>
        /// Exit combat and return to TV
        /// </summary>
        public void ExitCombat(bool victory, int statsGained)
        {
            if (victory)
            {
                Log($"Combat Victory! Stats gained: {statsGained}");
                UIManager?.ShowNotification($"Victory! +{statsGained} stats!");
            }
            else
            {
                Log("Combat Defeat!");
                UIManager?.ShowNotification("Defeated in combat...");
            }

            ChangeGameState(GameState.WatchingTV);
        }

        /// <summary>
        /// Save current game progress
        /// </summary>
        public void SaveGame()
        {
            if (PlayerData == null)
            {
                Debug.LogError("Cannot save - no player data!");
                return;
            }

            SaveManager.SaveGame(PlayerData);
            UIManager?.ShowNotification("Game Saved!");
            Log("Game Saved");
        }

        /// <summary>
        /// Handle day ending
        /// </summary>
        private void HandleDayEnded(int newDay)
        {
            Log($"Day {newDay - 1} ended. Starting Day {newDay}");

            OnDayChanged?.Invoke(newDay);

            // Auto-save at end of day
            SaveGame();

            // Check if player has met their goal
            if (CheckWinCondition())
            {
                HandleGameWon();
            }
        }

        /// <summary>
        /// Handle deadline reached (Day 10 ended)
        /// </summary>
        private void HandleDeadline()
        {
            Log("Deadline Reached!");

            if (CheckWinCondition())
            {
                HandleGameWon();
            }
            else
            {
                HandleGameOver();
            }
        }

        /// <summary>
        /// Check if player has achieved their career goal
        /// </summary>
        private bool CheckWinCondition()
        {
            if (PlayerData == null) return false;

            // Check if player stats meet career requirements
            return PlayerData.CareerGoal.CheckRequirements(PlayerData.Stats);
        }

        /// <summary>
        /// Handle game over (failed to meet deadline)
        /// </summary>
        private void HandleGameOver()
        {
            Log("Game Over - Failed to meet goal");

            OnGameOver?.Invoke();
            ChangeGameState(GameState.GameOver);

            SceneController.LoadScene("05_EndGame");
            UIManager?.ShowGameOverScreen(false);
        }

        /// <summary>
        /// Handle game won (met career goal)
        /// </summary>
        private void HandleGameWon()
        {
            Log("Game Won - Career goal achieved!");

            OnGameWon?.Invoke();

            SceneController.LoadScene("05_EndGame");
            UIManager?.ShowGameOverScreen(true);
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Clean up current game
            PlayerData = null;
            TimeManager?.ResetTime();

            ChangeGameState(GameState.MainMenu);
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            Log("Quitting Game");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        private void Log(string message)
        {
            if (enableDebugMode)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (TimeManager != null)
            {
                TimeManager.OnDayEnded -= HandleDayEnded;
                TimeManager.OnDeadlineReached -= HandleDeadline;
            }
        }
    }

    /// <summary>
    /// Game state enumeration
    /// </summary>
    public enum GameState
    {
        MainMenu,
        CharacterCreation,
        LivingRoom,
        WatchingTV,
        Combat,
        GameOver
    }
}