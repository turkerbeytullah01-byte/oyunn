using UnityEngine;

namespace ProjectAegis.Systems.Save
{
    /// <summary>
    /// Base class for singleton managers
    /// Provides thread-safe singleton pattern with DontDestroyOnLoad
    /// </summary>
    /// <typeparam name="T">The concrete manager type</typeparam>
    public abstract class BaseManager<T> : MonoBehaviour where T : BaseManager<T>
    {
        // Static instance
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isShuttingDown = false;
        
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static T Instance
        {
            get
            {
                // Return null if application is quitting
                if (_isShuttingDown)
                {
                    Debug.LogWarning($"[BaseManager] Instance of {typeof(T)} already destroyed. Returning null.");
                    return null;
                }
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Try to find existing instance
                        _instance = FindObjectOfType<T>();
                        
                        if (_instance == null)
                        {
                            // Create new instance
                            GameObject singletonObject = new GameObject(typeof(T).Name);
                            _instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                            
                            Debug.Log($"[BaseManager] Created new instance of {typeof(T)}");
                        }
                    }
                    
                    return _instance;
                }
            }
        }
        
        /// <summary>
        /// Checks if instance exists without creating one
        /// </summary>
        public static bool HasInstance => _instance != null;
        
        /// <summary>
        /// Called when the script instance is being loaded
        /// </summary>
        protected virtual void Awake()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = this as T;
                    DontDestroyOnLoad(gameObject);
                    OnInitialized();
                }
                else if (_instance != this)
                {
                    Debug.LogWarning($"[BaseManager] Duplicate instance of {typeof(T)} found. Destroying.");
                    Destroy(gameObject);
                }
            }
        }
        
        /// <summary>
        /// Called when the instance is first initialized
        /// Override this instead of Awake in derived classes
        /// </summary>
        protected virtual void OnInitialized()
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Called when the MonoBehaviour will be destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        /// <summary>
        /// Called when the application quits
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _isShuttingDown = true;
        }
        
        /// <summary>
        /// Called when the application is paused/resumed
        /// </summary>
        protected virtual void OnApplicationPause(bool pause)
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Called when the application gains/loses focus
        /// </summary>
        protected virtual void OnApplicationFocus(bool focus)
        {
            // Override in derived classes
        }
    }
}
