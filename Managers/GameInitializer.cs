using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegis
{
    /// <summary>
    /// Main game initializer - entry point for the entire game
    /// Handles manager initialization, save loading, and game startup
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        #region Singleton
        private static GameInitializer _instance;
        public static GameInitializer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameInitializer>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameInitializer");
                        _instance = go.AddComponent<GameInitializer>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        public event Action OnInitializationStarted;
        public event Action<float> OnInitializationProgress;
        public event Action OnInitializationComplete;
        public event Action<string> OnInitializationStep;
        public event Action<string> OnInitializationError;
        #endregion

        #region Properties
        [Header("Initialization Settings")]
        [SerializeField] private bool _loadSaveOnStart = true;
        [SerializeField] private bool _showDebugLogs = true;
        [SerializeField] private float _initializationDelay = 0.1f;
        
        [Header("Manager Prefabs")]
        [SerializeField] private List<GameObject> _managerPrefabs = new List<GameObject>();
        
        public bool IsInitialized { get; private set; }
        public float InitializationProgress { get; private set; }
        public string CurrentStep { get; private set; }
        
        private List<IManager> _initializedManagers = new List<IManager>();
        private bool _isInitializing = false;
        #endregion

        #region Manager Initialization Order
        /// <summary>
        /// Defines the order in which managers must be initialized
        /// </summary>
        private static readonly Type[] MANAGER_ORDER = new Type[]
        {
            typeof(SaveManager),            // 1. Save system first
            typeof(TimeManager),            // 2. Time management
            typeof(EventManager),           // 3. Event system
            typeof(PlayerDataManager),      // 4. Player data
            typeof(ResearchManager),        // 5. Research system
            typeof(TechTreeManager),        // 6. Tech tree
            typeof(ReputationManager),      // 7. Reputation
            typeof(RiskManager),            // 8. Risk system
            typeof(ProductionManager),      // 9. Production
            typeof(IdleManager),            // 10. Idle progression
            typeof(DynamicEventManager),    // 11. Dynamic events
            typeof(ContractManager),        // 12. Contracts
            typeof(PrototypeTestingManager),// 13. Prototype testing
            typeof(DroneManager),           // 14. Drone management
            typeof(UIManager)               // 15. UI last
        };
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (_showDebugLogs)
                Debug.Log("[GameInitializer] Awake - Starting initialization sequence");
        }

        private void Start()
        {
            StartCoroutine(InitializeGame());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                CleanupManagers();
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveGame();
            }
        }
        #endregion

        #region Initialization
        private IEnumerator InitializeGame()
        {
            if (_isInitializing)
            {
                Debug.LogWarning("[GameInitializer] Initialization already in progress");
                yield break;
            }
            
            _isInitializing = true;
            OnInitializationStarted?.Invoke();
            
            if (_showDebugLogs)
                Debug.Log("[GameInitializer] === GAME INITIALIZATION STARTED ===");
            
            // Step 1: Create manager container
            yield return StartCoroutine(CreateManagerContainer());
            
            // Step 2: Initialize managers in order
            yield return StartCoroutine(InitializeManagers());
            
            // Step 3: Setup event subscriptions
            yield return StartCoroutine(SetupEventSubscriptions());
            
            // Step 4: Load save data or create new game
            if (_loadSaveOnStart)
            {
                yield return StartCoroutine(LoadSaveData());
            }
            else
            {
                yield return StartCoroutine(CreateNewGame());
            }
            
            // Step 5: Post-initialization setup
            yield return StartCoroutine(PostInitialization());
            
            // Complete
            IsInitialized = true;
            InitializationProgress = 1f;
            _isInitializing = false;
            
            OnInitializationComplete?.Invoke();
            
            if (_showDebugLogs)
                Debug.Log("[GameInitializer] === GAME INITIALIZATION COMPLETE ===");
        }

        private IEnumerator CreateManagerContainer()
        {
            SetStep("Creating manager container");
            
            GameObject managerContainer = GameObject.Find("Managers");
            if (managerContainer == null)
            {
                managerContainer = new GameObject("Managers");
                DontDestroyOnLoad(managerContainer);
            }
            
            yield return new WaitForSeconds(_initializationDelay);
        }

        private IEnumerator InitializeManagers()
        {
            SetStep("Initializing managers");
            
            int totalManagers = MANAGER_ORDER.Length;
            
            for (int i = 0; i < totalManagers; i++)
            {
                Type managerType = MANAGER_ORDER[i];
                float progress = (float)(i + 1) / totalManagers;
                
                SetStep($"Initializing {managerType.Name}");
                
                // Get or create manager instance
                IManager manager = GetOrCreateManager(managerType);
                
                if (manager != null)
                {
                    try
                    {
                        manager.Initialize();
                        _initializedManagers.Add(manager);
                        
                        if (_showDebugLogs)
                            Debug.Log($"[GameInitializer] Initialized: {managerType.Name}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[GameInitializer] Failed to initialize {managerType.Name}: {ex.Message}");
                        OnInitializationError?.Invoke($"Failed to initialize {managerType.Name}");
                    }
                }
                
                InitializationProgress = progress * 0.6f; // 60% of total progress
                OnInitializationProgress?.Invoke(InitializationProgress);
                
                yield return new WaitForSeconds(_initializationDelay);
            }
        }

        private IEnumerator SetupEventSubscriptions()
        {
            SetStep("Setting up event subscriptions");
            
            foreach (var manager in _initializedManagers)
            {
                try
                {
                    manager.SetupEventSubscriptions();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameInitializer] Failed to setup events for {manager.GetType().Name}: {ex.Message}");
                }
            }
            
            InitializationProgress = 0.7f;
            OnInitializationProgress?.Invoke(InitializationProgress);
            
            yield return new WaitForSeconds(_initializationDelay);
        }

        private IEnumerator LoadSaveData()
        {
            SetStep("Loading save data");
            
            if (SaveManager.HasInstance)
            {
                bool loadSuccess = false;
                
                try
                {
                    loadSuccess = SaveManager.Instance.LoadGame();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameInitializer] Save load error: {ex.Message}");
                }
                
                if (!loadSuccess)
                {
                    Debug.Log("[GameInitializer] No save found or load failed - creating new game");
                    yield return StartCoroutine(CreateNewGame());
                }
                else
                {
                    if (_showDebugLogs)
                        Debug.Log("[GameInitializer] Save data loaded successfully");
                }
            }
            else
            {
                Debug.LogWarning("[GameInitializer] SaveManager not available - creating new game");
                yield return StartCoroutine(CreateNewGame());
            }
            
            InitializationProgress = 0.85f;
            OnInitializationProgress?.Invoke(InitializationProgress);
            
            yield return new WaitForSeconds(_initializationDelay);
        }

        private IEnumerator CreateNewGame()
        {
            SetStep("Creating new game");
            
            // Initialize default player data
            if (PlayerDataManager.HasInstance)
            {
                PlayerDataManager.Instance.SetCurrency(5000f); // Starting funds
                PlayerDataManager.Instance.SetResearchPoints(100f); // Starting RP
            }
            
            // Unlock starting drones
            if (DroneManager.HasInstance)
            {
                // Scout-X1 starts unlocked
                if (!DroneManager.Instance.IsDroneUnlocked("scout_x1"))
                {
                    // Will be auto-unlocked based on DroneData settings
                }
            }
            
            // Set initial reputation
            if (ReputationManager.HasInstance)
            {
                // Already has default value
            }
            
            if (_showDebugLogs)
                Debug.Log("[GameInitializer] New game created");
            
            InitializationProgress = 0.85f;
            OnInitializationProgress?.Invoke(InitializationProgress);
            
            yield return new WaitForSeconds(_initializationDelay);
        }

        private IEnumerator PostInitialization()
        {
            SetStep("Post-initialization setup");
            
            // Start idle progression
            if (IdleManager.HasInstance)
            {
                IdleManager.Instance.StartIdleProgression();
            }
            
            // Start dynamic events
            if (DynamicEventManager.HasInstance)
            {
                DynamicEventManager.Instance.StartEventSystem();
            }
            
            // Generate initial contracts
            if (ContractManager.HasInstance)
            {
                ContractManager.Instance.GenerateInitialContracts();
            }
            
            // Start time manager
            if (TimeManager.HasInstance)
            {
                TimeManager.Instance.Resume();
            }
            
            // Show initial UI
            if (UIManager.HasInstance)
            {
                UIManager.Instance.ShowMainMenu();
            }
            
            InitializationProgress = 1f;
            OnInitializationProgress?.Invoke(InitializationProgress);
            
            yield return new WaitForSeconds(_initializationDelay);
        }
        #endregion

        #region Helper Methods
        private IManager GetOrCreateManager(Type managerType)
        {
            // Check if manager already exists
            var existing = FindObjectOfType(managerType) as IManager;
            if (existing != null)
            {
                return existing;
            }
            
            // Create manager GameObject
            GameObject managerContainer = GameObject.Find("Managers");
            GameObject managerGO = new GameObject(managerType.Name);
            managerGO.transform.SetParent(managerContainer.transform);
            
            // Add manager component
            var manager = managerGO.AddComponent(managerType) as IManager;
            
            return manager;
        }

        private void SetStep(string step)
        {
            CurrentStep = step;
            OnInitializationStep?.Invoke(step);
            
            if (_showDebugLogs)
                Debug.Log($"[GameInitializer] Step: {step}");
        }

        private void CleanupManagers()
        {
            foreach (var manager in _initializedManagers)
            {
                try
                {
                    manager.Cleanup();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameInitializer] Error cleaning up {manager.GetType().Name}: {ex.Message}");
                }
            }
            
            _initializedManagers.Clear();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Save the current game
        /// </summary>
        public void SaveGame()
        {
            if (!IsInitialized) return;
            
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.SaveGame();
                
                if (_showDebugLogs)
                    Debug.Log("[GameInitializer] Game saved");
            }
        }

        /// <summary>
        /// Load a saved game
        /// </summary>
        public void LoadGame()
        {
            StartCoroutine(LoadSaveData());
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void NewGame()
        {
            // Clear save
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.DeleteSave();
            }
            
            // Reload scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Get a manager by type
        /// </summary>
        public T GetManager<T>() where T : class, IManager
        {
            foreach (var manager in _initializedManagers)
            {
                if (manager is T typedManager)
                {
                    return typedManager;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if a manager type is initialized
        /// </summary>
        public bool IsManagerInitialized<T>() where T : class, IManager
        {
            return GetManager<T>() != null;
        }

        /// <summary>
        /// Restart initialization (for debugging)
        /// </summary>
        public void RestartInitialization()
        {
            StopAllCoroutines();
            CleanupManagers();
            IsInitialized = false;
            InitializationProgress = 0f;
            _initializedManagers.Clear();
            StartCoroutine(InitializeGame());
        }
        #endregion
    }

    /// <summary>
    /// Interface for all game managers
    /// </summary>
    public interface IManager
    {
        void Initialize();
        void SetupEventSubscriptions();
        void Cleanup();
        bool IsInitialized { get; }
    }
}
