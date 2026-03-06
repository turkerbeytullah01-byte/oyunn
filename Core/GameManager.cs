// ============================================================================
// Project Aegis: Drone Dominion
// GameManager - Main controller and coordinator for all game systems
// ============================================================================
// Central singleton that initializes, coordinates, and manages all game systems.
// Handles application lifecycle events, save/load coordination, and provides
// the primary entry point for game state management.
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectAegis.Core
{
    /// <summary>
    /// Main game controller and system coordinator.
    /// Manages initialization, lifecycle, and cross-system communication.
    /// </summary>
    public class GameManager : BaseManager<GameManager>
    {
        #region Configuration
        
        public override int InitializationPriority => 0; // Initialize first
        
        /// <summary>
        /// Target frame rate for the game.
        /// </summary>
        [SerializeField, Range(30, 120)]
        private int _targetFrameRate = 60;
        
        /// <summary>
        /// Whether to run in background (for idle progression).
        /// </summary>
        [SerializeField]
        private bool _runInBackground = true;
        
        /// <summary>
        /// Auto-save interval in seconds.
        /// </summary>
        [SerializeField, Range(30f, 600f)]
        private float _autoSaveInterval = 120f;
        
        /// <summary>
        /// Whether to show debug info in development builds.
        /// </summary>
        [SerializeField]
        private bool _showDebugInfo = true;
        
        #endregion
        
        #region State
        
        /// <summary>
        /// Current state of the game.
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.Initializing;
        
        /// <summary>
        /// Previous state of the game (for state transitions).
        /// </summary>
        public GameState PreviousState { get; private set; } = GameState.Initializing;
        
        /// <summary>
        /// Whether the game has finished initialization.
        /// </summary>
        public bool IsGameReady { get; private set; }
        
        /// <summary>
        /// Whether the game is currently loading.
        /// </summary>
        public bool IsLoading { get; private set; }
        
        /// <summary>
        /// Time elapsed since game started.
        /// </summary>
        public float GameTime => Time.time;
        
        /// <summary>
        /// Timestamp when the game session started.
        /// </summary>
        public DateTime SessionStartTime { get; private set; }
        
        #endregion
        
        #region Systems
        
        /// <summary>
        /// List of all registered game systems.
        /// </summary>
        private List<BaseSystem> _systems = new List<BaseSystem>();
        
        /// <summary>
        /// List of all manager components in the scene.
        /// </summary>
        private List<MonoBehaviour> _managers = new List<MonoBehaviour>();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Called when the game finishes initialization.
        /// </summary>
        public event Action OnGameInitialized;
        
        /// <summary>
        /// Called when the game state changes.
        /// Parameters: (GameState oldState, GameState newState)
        /// </summary>
        public event Action<GameState, GameState> OnGameStateChanged;
        
        /// <summary>
        /// Called when the game is about to save.
        /// </summary>
        public event Action OnBeforeSave;
        
        /// <summary>
        /// Called when the game has finished saving.
        /// </summary>
        public event Action OnAfterSave;
        
        /// <summary>
        /// Called when the game is about to load.
        /// </summary>
        public event Action OnBeforeLoad;
        
        /// <summary>
        /// Called when the game has finished loading.
        /// </summary>
        public event Action OnAfterLoad;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            // Configure application settings
            Application.targetFrameRate = _targetFrameRate;
            Application.runInBackground = _runInBackground;
            
            // Record session start
            SessionStartTime = DateTime.UtcNow;
            
            Log("GameManager awake - configuring application");
        }
        
        private void Start()
        {
            // Begin initialization sequence
            StartCoroutine(InitializeGameSequence());
        }
        
        private void Update()
        {
            if (!IsGameReady) return;
            
            // Update all systems
            float deltaTime = TimeManager.Instance?.DeltaTime ?? Time.deltaTime;
            
            foreach (var system in _systems)
            {
                if (system.IsProcessing)
                {
                    system.Tick(deltaTime);
                }
            }
            
            // Auto-save check
            UpdateAutoSave();
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Log("Application paused - saving game");
                SaveGame();
            }
            else
            {
                Log("Application resumed - calculating offline progress");
                CalculateOfflineProgress();
            }
        }
        
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                // Optional: save on focus loss
            }
        }
        
        private void OnApplicationQuit()
        {
            Log("Application quitting - saving game");
            SaveGame();
        }
        
        #endregion
        
        #region Initialization Sequence
        
        /// <summary>
        /// Coroutine for the game initialization sequence.
        /// Ensures proper order of initialization across all systems.
        /// </summary>
        private System.Collections.IEnumerator InitializeGameSequence()
        {
            Log("=== Starting Game Initialization ===");
            
            // Phase 1: Initialize core managers
            yield return StartCoroutine(InitializeCoreManagers());
            
            // Phase 2: Discover and register systems
            yield return StartCoroutine(DiscoverAndRegisterSystems());
            
            // Phase 3: Initialize all systems
            yield return StartCoroutine(InitializeSystems());
            
            // Phase 4: Post-initialization setup
            yield return StartCoroutine(PostInitialization());
            
            // Phase 5: Load saved game or start new
            yield return StartCoroutine(LoadOrStartNew());
            
            // Phase 6: Finalize
            IsGameReady = true;
            ChangeGameState(GameState.MainMenu);
            
            OnGameInitialized?.Invoke();
            
            Log("=== Game Initialization Complete ===");
        }
        
        private System.Collections.IEnumerator InitializeCoreManagers()
        {
            Log("Phase 1: Initializing Core Managers");
            
            // Initialize EventManager first (priority 0)
            if (EventManager.HasInstance)
            {
                EventManager.Instance.Initialize();
                yield return null;
            }
            
            // Initialize ServiceLocator (priority 1)
            if (ServiceLocator.HasInstance)
            {
                ServiceLocator.Instance.Initialize();
                yield return null;
            }
            
            // Initialize TimeManager (priority 2)
            if (TimeManager.HasInstance)
            {
                TimeManager.Instance.Initialize();
                yield return null;
            }
            
            // Find and initialize other managers
            var managers = FindObjectsOfType<MonoBehaviour>()
                .Where(m => m is IInitializable && m != this)
                .OrderBy(m => (m as IInitializable) is BaseManager<MonoBehaviour> ? 
                    ((BaseManager<MonoBehaviour>)m).InitializationPriority : 100)
                .ToList();
            
            foreach (var manager in managers)
            {
                if (manager is IInitializable initializable && !initializable.IsInitialized)
                {
                    initializable.Initialize();
                    _managers.Add(manager);
                    yield return null;
                }
            }
            
            Log($"Initialized {_managers.Count} managers");
        }
        
        private System.Collections.IEnumerator DiscoverAndRegisterSystems()
        {
            Log("Phase 2: Discovering Systems");
            
            // Systems are typically registered by their respective managers
            // This is a hook for any automatic discovery if needed
            
            yield return null;
        }
        
        private System.Collections.IEnumerator InitializeSystems()
        {
            Log("Phase 3: Initializing Systems");
            
            // Sort systems by priority
            _systems = _systems.OrderBy(s => s.UpdatePriority).ToList();
            
            foreach (var system in _systems)
            {
                if (!system.IsInitialized)
                {
                    system.Initialize();
                    yield return null;
                }
            }
            
            Log($"Initialized {_systems.Count} systems");
        }
        
        private System.Collections.IEnumerator PostInitialization()
        {
            Log("Phase 4: Post-Initialization");
            
            // Call PostInitialize on all managers
            foreach (var manager in _managers)
            {
                if (manager is IInitializable initializable)
                {
                    initializable.PostInitialize();
                }
            }
            
            // Call PostInitialize on all systems
            foreach (var system in _systems)
            {
                system.PostInitialize();
            }
            
            yield return null;
        }
        
        private System.Collections.IEnumerator LoadOrStartNew()
        {
            Log("Phase 5: Loading Game Data");
            
            // Check for existing save
            if (SaveManager.HasInstance && SaveManager.Instance.HasSaveData())
            {
                yield return StartCoroutine(LoadGameCoroutine());
            }
            else
                       {
                Log("No save data found - starting new game");
                yield return StartCoroutine(StartNewGameCoroutine());
            }
        }
        
        #endregion
        
        #region System Management
        
        /// <summary>
        /// Registers a game system.
        /// </summary>
        public void RegisterSystem(BaseSystem system)
        {
            if (system == null)
            {
                LogError("Cannot register null system");
                return;
            }
            
            if (!_systems.Contains(system))
            {
                _systems.Add(system);
                Log($"Registered system: {system.SystemName}");
            }
        }
        
        /// <summary>
        /// Unregisters a game system.
        /// </summary>
        public void UnregisterSystem(BaseSystem system)
        {
            if (system == null) return;
            
            if (_systems.Remove(system))
            {
                system.Dispose();
                Log($"Unregistered system: {system.SystemName}");
            }
        }
        
        /// <summary>
        /// Gets a system by its ID.
        /// </summary>
        public T GetSystem<T>() where T : BaseSystem
        {
            foreach (var system in _systems)
            {
                if (system is T typedSystem)
                {
                    return typedSystem;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Gets all registered systems.
        /// </summary>
        public IReadOnlyList<BaseSystem> GetAllSystems()
        {
            return _systems;
        }
        
        #endregion
        
        #region Game State Management
        
        /// <summary>
        /// Changes the current game state.
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            PreviousState = CurrentState;
            CurrentState = newState;
            
            Log($"Game state changed: {PreviousState} -> {newState}");
            
            // Notify event system
            EventManager.Instance?.TriggerGameStateChanged(PreviousState, newState);
            
            // Notify local listeners
            OnGameStateChanged?.Invoke(PreviousState, newState);
            
            // Handle state-specific logic
            HandleStateChange(PreviousState, newState);
        }
        
        /// <summary>
        /// Handles state-specific logic when game state changes.
        /// </summary>
        private void HandleStateChange(GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Paused:
                    TimeManager.Instance?.Pause();
                    break;
                    
                case GameState.Playing:
                    if (oldState == GameState.Paused)
                    {
                        TimeManager.Instance?.Resume();
                    }
                    break;
                    
                case GameState.GameOver:
                    // Handle game over
                    break;
            }
        }
        
        #endregion
        
        #region Save/Load
        
        private float _lastAutoSaveTime;
        
        private void UpdateAutoSave()
        {
            if (Time.time - _lastAutoSaveTime >= _autoSaveInterval)
            {
                SaveGame();
                _lastAutoSaveTime = Time.time;
            }
        }
        
        /// <summary>
        /// Saves the game.
        /// </summary>
        public void SaveGame()
        {
            if (IsLoading) return;
            
            OnBeforeSave?.Invoke();
            
            // Record save time
            TimeManager.Instance?.RecordSaveTime();
            
            // Trigger save through SaveManager
            SaveManager.Instance?.SaveGame();
            
            _lastAutoSaveTime = Time.time;
            
            OnAfterSave?.Invoke();
            
            Log("Game saved");
        }
        
        /// <summary>
        /// Loads the game.
        /// </summary>
        public void LoadGame()
        {
            StartCoroutine(LoadGameCoroutine());
        }
        
        private System.Collections.IEnumerator LoadGameCoroutine()
        {
            IsLoading = true;
            
            OnBeforeLoad?.Invoke();
            
            yield return null;
            
            // Load through SaveManager
            SaveManager.Instance?.LoadGame();
            
            yield return null;
            
            // Calculate offline progress
            CalculateOfflineProgress();
            
            OnAfterLoad?.Invoke();
            
            IsLoading = false;
            
            Log("Game loaded");
        }
        
        /// <summary>
        /// Starts a new game.
        /// </summary>
        public void StartNewGame()
        {
            StartCoroutine(StartNewGameCoroutine());
        }
        
        private System.Collections.IEnumerator StartNewGameCoroutine()
        {
            Log("Starting new game");
            
            // Reset all systems to default state
            foreach (var system in _systems)
            {
                // Reset system state
            }
            
            // Clear save data
            SaveManager.Instance?.ClearSaveData();
            
            // Initialize starting values
            InitializeStartingValues();
            
            yield return null;
            
            Log("New game started");
        }
        
        private void InitializeStartingValues()
        {
            // Set initial player values
            // This would typically be done through PlayerData or a similar system
        }
        
        /// <summary>
        /// Calculates and applies offline progress.
        /// </summary>
        private void CalculateOfflineProgress()
        {
            var timeManager = TimeManager.Instance;
            if (timeManager == null) return;
            
            var offlineData = timeManager.CalculateOfflineProgress();
            
            if (offlineData.WasOffline)
            {
                Log($"Offline for {offlineData.FormattedDuration}");
                
                // Apply offline progress to systems
                // This would typically be handled by IdleSystem or similar
                
                // Notify event system
                EventManager.Instance?.TriggerOfflineProgressCalculated(
                    offlineData.OfflineSeconds, 
                    offlineData.EffectiveSeconds, 
                    0, 0); // moneyEarned, researchPointsEarned would come from calculations
            }
        }
        
        #endregion
        
        #region Initialization Override
        
        protected override void OnInitialize()
        {
            // GameManager initialization is handled in Start()
            Log("GameManager initialized");
        }
        
        #endregion
        
        #region Debug
        
        private void OnGUI()
        {
            if (!_showDebugInfo) return;
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"Game State: {CurrentState}");
            GUILayout.Label($"Game Ready: {IsGameReady}");
            GUILayout.Label($"Systems: {_systems.Count}");
            GUILayout.Label($"Game Time: {TimeManager.FormatDuration(GameTime)}");
            GUILayout.Label($"Time Scale: {TimeManager.Instance?.TimeScale:F2}x");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
            #endif
        }
        
        #endregion
    }
    
    #region Game State Enum
    
    /// <summary>
    /// Represents the current state of the game.
    /// </summary>
    public enum GameState
    {
        /// <summary>Game is initializing</summary>
        Initializing,
        /// <summary>Main menu is displayed</summary>
        MainMenu,
        /// <summary>Game is actively being played</summary>
        Playing,
        /// <summary>Game is paused</summary>
        Paused,
        /// <summary>A menu or overlay is open</summary>
        MenuOpen,
        /// <summary>Research screen is open</summary>
        ResearchScreen,
        /// <summary>Contract screen is open</summary>
        ContractScreen,
        /// <summary>Game has ended</summary>
        GameOver
    }
    
    #endregion
}
