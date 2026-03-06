// ============================================================================
// Project Aegis: Drone Dominion
// BaseManager - Generic singleton pattern implementation
// ============================================================================
// Provides a thread-safe, persistent singleton base class for all managers.
// Handles instance creation, lifecycle management, and scene persistence.
// ============================================================================

using UnityEngine;
using System;

namespace ProjectAegis.Core
{
    /// <summary>
    /// Abstract base class for singleton managers.
    /// Provides thread-safe instance management and lifecycle hooks.
    /// </summary>
    /// <typeparam name="T">The concrete manager type</typeparam>
    public abstract class BaseManager<T> : MonoBehaviour, IInitializable where T : BaseManager<T>
    {
        #region Singleton Implementation
        
        /// <summary>
        /// Thread lock for safe instance creation.
        /// </summary>
        private static readonly object _lock = new object();
        
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static T _instance;
        
        /// <summary>
        /// Gets the singleton instance. Creates one if it doesn't exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<T>();
                            
                            if (_instance == null)
                            {
                                GameObject singletonObject = new GameObject(typeof(T).Name);
                                _instance = singletonObject.AddComponent<T>();
                                
                                #if UNITY_EDITOR
                                Debug.Log($"[BaseManager] Created new instance of {typeof(T).Name}");
                                #endif
                            }
                        }
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Returns whether the singleton instance exists.
        /// </summary>
        public static bool HasInstance => _instance != null;
        
        /// <summary>
        /// Safely attempts to get the instance without creating one.
        /// </summary>
        public static bool TryGetInstance(out T instance)
        {
            instance = _instance;
            return _instance != null;
        }
        
        #endregion
        
        #region Configuration
        
        /// <summary>
        /// If true, this manager will persist across scene loads.
        /// Override in derived classes to change behavior.
        /// </summary>
        protected virtual bool PersistAcrossScenes => true;
        
        /// <summary>
        /// If true, this manager will be marked as DontDestroyOnLoad.
        /// </summary>
        protected virtual bool DontDestroy => true;
        
        /// <summary>
        /// Priority for initialization order. Lower values initialize first.
        /// </summary>
        public virtual int InitializationPriority => 100;
        
        #endregion
        
        #region State
        
        /// <summary>
        /// Whether this manager has been initialized.
        /// </summary>
        public bool IsInitialized { get; protected set; }
        
        /// <summary>
        /// Whether this manager is currently active and running.
        /// </summary>
        public bool IsActive { get; protected set; } = true;
        
        #endregion
        
        #region Unity Lifecycle
        
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Handles singleton setup and persistence.
        /// </summary>
        protected virtual void Awake()
        {
            // Singleton enforcement
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[BaseManager] Duplicate instance of {typeof(T).Name} detected. Destroying.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this as T;
            
            // Setup persistence
            if (DontDestroy)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            
            // Call derived class awake
            OnAwake();
        }
        
        /// <summary>
        /// Override this instead of Awake() in derived classes.
        /// </summary>
        protected virtual void OnAwake() { }
        
        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        protected virtual void OnEnable()
        {
            IsActive = true;
        }
        
        /// <summary>
        /// Called when the behaviour becomes disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            IsActive = false;
        }
        
        /// <summary>
        /// Called when the MonoBehaviour will be destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            
            OnCleanup();
        }
        
        /// <summary>
        /// Override this for cleanup logic in derived classes.
        /// </summary>
        protected virtual void OnCleanup() { }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the manager. Called by GameManager during startup.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"[BaseManager] {typeof(T).Name} already initialized.");
                return;
            }
            
            try
            {
                OnInitialize();
                IsInitialized = true;
                
                #if UNITY_EDITOR
                Debug.Log($"[BaseManager] {typeof(T).Name} initialized successfully.");
                #endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BaseManager] Failed to initialize {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Override this to implement initialization logic.
        /// Called during GameManager startup sequence.
        /// </summary>
        protected abstract void OnInitialize();
        
        /// <summary>
        /// Called after all managers have been initialized.
        /// Use for cross-manager setup that requires other managers to be ready.
        /// </summary>
        public void PostInitialize()
        {
            if (!IsInitialized)
            {
                Debug.LogError($"[BaseManager] Cannot PostInitialize {typeof(T).Name} - not yet initialized.");
                return;
            }
            
            try
            {
                OnPostInitialize();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BaseManager] PostInitialize failed for {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Override this for post-initialization logic.
        /// </summary>
        protected virtual void OnPostInitialize() { }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Logs a message with the manager's type prefix.
        /// </summary>
        protected void Log(string message)
        {
            Debug.Log($"[{typeof(T).Name}] {message}");
        }
        
        /// <summary>
        /// Logs a warning with the manager's type prefix.
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{typeof(T).Name}] {message}");
        }
        
        /// <summary>
        /// Logs an error with the manager's type prefix.
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"[{typeof(T).Name}] {message}");
        }
        
        /// <summary>
        /// Ensures the manager is initialized before proceeding.
        /// </summary>
        protected bool EnsureInitialized()
        {
            if (!IsInitialized)
            {
                LogError("Manager not initialized!");
                return false;
            }
            return true;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Attribute to specify initialization order for managers.
    /// Lower values initialize first.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class InitializationPriorityAttribute : Attribute
    {
        public int Priority { get; }
        
        public InitializationPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
