// ============================================================================
// Project Aegis: Drone Dominion
// BaseSystem - Foundation for all game systems
// ============================================================================
// Provides lifecycle management, state tracking, and common functionality
// for all game systems (Research, Contracts, Idle Generation, etc.)
// ============================================================================

using UnityEngine;
using System;

namespace ProjectAegis.Core
{
    /// <summary>
    /// Abstract base class for all game systems.
    /// Systems are modular components that handle specific game functionality.
    /// </summary>
    public abstract class BaseSystem : IInitializable, ITickable, IPausable, IDisposable
    {
        #region Properties
        
        /// <summary>
        /// The unique identifier for this system.
        /// </summary>
        public abstract string SystemId { get; }
        
        /// <summary>
        /// Display name for debugging and logging.
        /// </summary>
        public abstract string SystemName { get; }
        
        /// <summary>
        /// Whether this system has been initialized.
        /// </summary>
        public bool IsInitialized { get; protected set; }
        
        /// <summary>
        /// Whether this system is currently active and processing.
        /// </summary>
        public bool IsActive { get; protected set; }
        
        /// <summary>
        /// Whether this system is currently paused.
        /// </summary>
        public bool IsPaused { get; protected set; }
        
        /// <summary>
        /// Whether this system is currently processing updates.
        /// </summary>
        public bool IsProcessing => IsActive && !IsPaused && IsInitialized;
        
        /// <summary>
        /// Priority for update order. Lower values update first.
        /// </summary>
        public virtual int UpdatePriority => 100;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Called when the system is initialized.
        /// </summary>
        public event Action OnInitialized;
        
        /// <summary>
        /// Called when the system is activated.
        /// </summary>
        public event Action OnActivated;
        
        /// <summary>
        /// Called when the system is deactivated.
        /// </summary>
        public event Action OnDeactivated;
        
        /// <summary>
        /// Called when the system is paused.
        /// </summary>
        public event Action OnPaused;
        
        /// <summary>
        /// Called when the system is resumed.
        /// </summary>
        public event Action OnResumed;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the system. Must be called before any other operations.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                LogWarning("System already initialized");
                return;
            }
            
            try
            {
                OnInitialize();
                IsInitialized = true;
                OnInitialized?.Invoke();
                
                Log("System initialized");
            }
            catch (Exception ex)
            {
                LogError($"Initialization failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Override this to implement system-specific initialization.
        /// </summary>
        protected abstract void OnInitialize();
        
        /// <summary>
        /// Called after all systems have been initialized.
        /// Use for cross-system setup.
        /// </summary>
        public void PostInitialize()
        {
            if (!IsInitialized)
            {
                LogError("Cannot PostInitialize - system not initialized");
                return;
            }
            
            try
            {
                OnPostInitialize();
            }
            catch (Exception ex)
            {
                LogError($"PostInitialize failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Override this for post-initialization logic.
        /// </summary>
        protected virtual void OnPostInitialize() { }
        
        #endregion
        
        #region Activation
        
        /// <summary>
        /// Activates the system, enabling updates.
        /// </summary>
        public void Activate()
        {
            if (!IsInitialized)
            {
                LogError("Cannot activate - system not initialized");
                return;
            }
            
            if (IsActive)
                return;
            
            IsActive = true;
            OnActivate();
            OnActivated?.Invoke();
            
            Log("System activated");
        }
        
        /// <summary>
        /// Override this to implement activation logic.
        /// </summary>
        protected virtual void OnActivate() { }
        
        /// <summary>
        /// Deactivates the system, disabling updates.
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
                return;
            
            IsActive = false;
            OnDeactivate();
            OnDeactivated?.Invoke();
            
            Log("System deactivated");
        }
        
        /// <summary>
        /// Override this to implement deactivation logic.
        /// </summary>
        protected virtual void OnDeactivate() { }
        
        #endregion
        
        #region Pausable Implementation
        
        /// <summary>
        /// Pauses the system.
        /// </summary>
        public void Pause()
        {
            if (IsPaused)
                return;
            
            IsPaused = true;
            OnPause();
            OnPaused?.Invoke();
            
            Log("System paused");
        }
        
        /// <summary>
        /// Override this to implement pause logic.
        /// </summary>
        protected virtual void OnPause() { }
        
        /// <summary>
        /// Resumes the system.
        /// </summary>
        public void Resume()
        {
            if (!IsPaused)
                return;
            
            IsPaused = false;
            OnResume();
            OnResumed?.Invoke();
            
            Log("System resumed");
        }
        
        /// <summary>
        /// Override this to implement resume logic.
        /// </summary>
        protected virtual void OnResume() { }
        
        #endregion
        
        #region Update
        
        /// <summary>
        /// Updates the system. Called by GameManager.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!IsProcessing)
                return;
            
            try
            {
                OnTick(deltaTime);
            }
            catch (Exception ex)
            {
                LogError($"Tick failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Override this to implement per-frame update logic.
        /// </summary>
        protected virtual void OnTick(float deltaTime) { }
        
        #endregion
        
        #region Cleanup
        
        /// <summary>
        /// Cleans up the system and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (IsActive)
            {
                Deactivate();
            }
            
            try
            {
                OnDispose();
                Log("System disposed");
            }
            catch (Exception ex)
            {
                LogError($"Dispose failed: {ex.Message}");
            }
            
            // Unsubscribe from events
            OnInitialized = null;
            OnActivated = null;
            OnDeactivated = null;
            OnPaused = null;
            OnResumed = null;
        }
        
        /// <summary>
        /// Override this to implement cleanup logic.
        /// </summary>
        protected virtual void OnDispose() { }
        
        #endregion
        
        #region Logging
        
        /// <summary>
        /// Logs a message with system prefix.
        /// </summary>
        protected void Log(string message)
        {
            Debug.Log($"[{SystemName}] {message}");
        }
        
        /// <summary>
        /// Logs a warning with system prefix.
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{SystemName}] {message}");
        }
        
        /// <summary>
        /// Logs an error with system prefix.
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"[{SystemName}] {message}");
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Returns a string representation of this system.
        /// </summary>
        public override string ToString()
        {
            return $"[{SystemName}] Active:{IsActive} Paused:{IsPaused} Initialized:{IsInitialized}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// MonoBehaviour wrapper for systems that need to exist in the scene.
    /// </summary>
    public abstract class BaseSystemBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The underlying system instance.
        /// </summary>
        protected abstract BaseSystem System { get; }
        
        protected virtual void Awake()
        {
            if (System != null && !System.IsInitialized)
            {
                System.Initialize();
            }
        }
        
        protected virtual void Start()
        {
            System?.Activate();
        }
        
        protected virtual void Update()
        {
            System?.Tick(Time.deltaTime);
        }
        
        protected virtual void OnDestroy()
        {
            System?.Dispose();
        }
        
        protected virtual void OnApplicationPause(bool pause)
        {
            if (pause)
                System?.Pause();
            else
                System?.Resume();
        }
    }
}
