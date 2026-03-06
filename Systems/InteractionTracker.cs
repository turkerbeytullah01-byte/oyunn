using System;
using UnityEngine;

namespace ProjectAegis.Systems.Events
{
    /// <summary>
    /// Tracks player interactions to determine if events should trigger.
    /// Events only trigger if player has interacted within the threshold window.
    /// </summary>
    public class InteractionTracker : MonoBehaviour
    {
        #region Singleton
        
        private static InteractionTracker _instance;
        public static InteractionTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InteractionTracker>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InteractionTracker");
                        _instance = go.AddComponent<InteractionTracker>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        #endregion

        #region Fields
        
        [Header("Interaction Settings")]
        [Tooltip("Time in seconds after last interaction when events can no longer trigger")]
        [SerializeField]
        private float _interactionThresholdSeconds = 180f; // 3 minutes
        
        [Tooltip("Whether to track interactions automatically via input")]
        [SerializeField]
        private bool _autoTrackInput = true;
        
        [Tooltip("Minimum time between interaction recordings (prevents spam)")]
        [SerializeField]
        private float _interactionCooldownSeconds = 0.5f;
        
        [Header("Debug")]
        [SerializeField]
        private bool _showDebugLogs = false;
        
        private DateTime _lastInteractionTime;
        private float _timeSinceLastInteraction;
        private float _interactionCooldownTimer;
        private bool _isTracking;
        private int _totalInteractionCount;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// The threshold in seconds for valid interactions (default: 180s = 3 min)
        /// </summary>
        public float InteractionThresholdSeconds
        {
            get => _interactionThresholdSeconds;
            set => _interactionThresholdSeconds = Mathf.Max(1f, value);
        }
        
        /// <summary>
        /// The threshold in minutes for valid interactions
        /// </summary>
        public float InteractionThresholdMinutes
        {
            get => _interactionThresholdSeconds / 60f;
            set => _interactionThresholdSeconds = value * 60f;
        }
        
        /// <summary>
        /// Time elapsed since last interaction in seconds
        /// </summary>
        public float SecondsSinceLastInteraction => (float)DateTime.Now.Subtract(_lastInteractionTime).TotalSeconds;
        
        /// <summary>
        /// Time elapsed since last interaction in minutes
        /// </summary>
        public float MinutesSinceLastInteraction => (float)DateTime.Now.Subtract(_lastInteractionTime).TotalMinutes;
        
        /// <summary>
        /// Whether the player is within the interaction threshold (can trigger events)
        /// </summary>
        public bool CanTriggerEvents => SecondsSinceLastInteraction <= _interactionThresholdSeconds;
        
        /// <summary>
        /// Whether the player is considered "active" (within threshold)
        /// </summary>
        public bool IsPlayerActive => CanTriggerEvents;
        
        /// <summary>
        /// Whether the player is considered "idle" (outside threshold)
        /// </summary>
        public bool IsPlayerIdle => !CanTriggerEvents;
        
        /// <summary>
        /// Progress toward idle state (0 = just interacted, 1 = fully idle)
        /// </summary>
        public float IdleProgress => Mathf.Clamp01(SecondsSinceLastInteraction / _interactionThresholdSeconds);
        
        /// <summary>
        /// Total number of interactions recorded this session
        /// </summary>
        public int TotalInteractionCount => _totalInteractionCount;
        
        /// <summary>
        /// Whether the tracker is currently monitoring interactions
        /// </summary>
        public bool IsTracking => _isTracking;
        
        /// <summary>
        /// Time remaining until player becomes idle (0 if already idle)
        /// </summary>
        public float SecondsUntilIdle => Mathf.Max(0, _interactionThresholdSeconds - SecondsSinceLastInteraction);

        #endregion

        #region Events
        
        /// <summary>
        /// Called when any interaction is recorded
        /// </summary>
        public event Action OnInteractionRecorded;
        
        /// <summary>
        /// Called when player becomes idle (crosses threshold)
        /// </summary>
        public event Action OnPlayerBecameIdle;
        
        /// <summary>
        /// Called when player becomes active (interaction after being idle)
        /// </summary>
        public event Action OnPlayerBecameActive;
        
        /// <summary>
        /// Called every frame with current idle progress
        /// </summary>
        public event Action<float> OnIdleProgressUpdate;

        #endregion

        #region Lifecycle
        
        private void Start()
        {
            // Initialize with current time so player starts as active
            _lastInteractionTime = DateTime.Now;
            _isTracking = true;
            
            if (_showDebugLogs)
                Debug.Log("[InteractionTracker] Started tracking interactions");
        }
        
        private void Update()
        {
            if (!_isTracking)
                return;
            
            // Update cooldown timer
            if (_interactionCooldownTimer > 0)
            {
                _interactionCooldownTimer -= Time.deltaTime;
            }
            
            // Auto-track input if enabled
            if (_autoTrackInput)
            {
                CheckForInput();
            }
            
            // Check for idle state change
            CheckIdleStateChange();
            
            // Report progress
            OnIdleProgressUpdate?.Invoke(IdleProgress);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                // App resumed - record interaction to prevent immediate idle
                RecordInteraction(InteractionType.AppResumed);
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // App gained focus - record interaction
                RecordInteraction(InteractionType.AppFocused);
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Records a player interaction. Call this from UI buttons, game actions, etc.
        /// </summary>
        public void RecordInteraction(InteractionType type = InteractionType.Generic)
        {
            // Check cooldown
            if (_interactionCooldownTimer > 0 && type != InteractionType.AppResumed)
                return;
            
            bool wasIdle = IsPlayerIdle;
            
            _lastInteractionTime = DateTime.Now;
            _totalInteractionCount++;
            _interactionCooldownTimer = _interactionCooldownSeconds;
            
            if (_showDebugLogs)
                Debug.Log($"[InteractionTracker] Interaction recorded: {type} (Total: {_totalInteractionCount})");
            
            // If player was idle and is now active, trigger event
            if (wasIdle)
            {
                OnPlayerBecameActive?.Invoke();
                if (_showDebugLogs)
                    Debug.Log("[InteractionTracker] Player became active");
            }
            
            OnInteractionRecorded?.Invoke();
        }
        
        /// <summary>
        /// Starts tracking interactions
        /// </summary>
        public void StartTracking()
        {
            _isTracking = true;
            RecordInteraction(InteractionType.TrackingStarted);
        }
        
        /// <summary>
        /// Stops tracking interactions
        /// </summary>
        public void StopTracking()
        {
            _isTracking = false;
        }
        
        /// <summary>
        /// Resets the interaction tracking
        /// </summary>
        public void ResetTracking()
        {
            _lastInteractionTime = DateTime.Now;
            _totalInteractionCount = 0;
            _interactionCooldownTimer = 0;
            
            if (_showDebugLogs)
                Debug.Log("[InteractionTracker] Tracking reset");
        }
        
        /// <summary>
        /// Sets the last interaction time (for save/load)
        /// </summary>
        public void SetLastInteractionTime(DateTime time)
        {
            _lastInteractionTime = time;
        }
        
        /// <summary>
        /// Gets the last interaction time
        /// </summary>
        public DateTime GetLastInteractionTime()
        {
            return _lastInteractionTime;
        }
        
        /// <summary>
        /// Returns tracker state for serialization
        /// </summary>
        public InteractionTrackerSaveData GetSaveData()
        {
            return new InteractionTrackerSaveData
            {
                LastInteractionTime = _lastInteractionTime.ToBinary(),
                TotalInteractionCount = _totalInteractionCount,
                IsTracking = _isTracking
            };
        }
        
        /// <summary>
        /// Restores tracker state from serialization
        /// </summary>
        public void LoadSaveData(InteractionTrackerSaveData data)
        {
            _lastInteractionTime = DateTime.FromBinary(data.LastInteractionTime);
            _totalInteractionCount = data.TotalInteractionCount;
            _isTracking = data.IsTracking;
        }

        #endregion

        #region Private Methods
        
        private void CheckForInput()
        {
            // Touch input (mobile)
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    RecordInteraction(InteractionType.Touch);
                }
            }
            
            // Mouse input (editor/PC)
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                RecordInteraction(InteractionType.MouseClick);
            }
            
            // Keyboard input
            if (Input.anyKeyDown)
            {
                RecordInteraction(InteractionType.Keyboard);
            }
        }
        
        private bool _wasIdleLastFrame = false;
        
        private void CheckIdleStateChange()
        {
            bool isIdle = IsPlayerIdle;
            
            if (isIdle && !_wasIdleLastFrame)
            {
                OnPlayerBecameIdle?.Invoke();
                if (_showDebugLogs)
                    Debug.Log("[InteractionTracker] Player became idle");
            }
            
            _wasIdleLastFrame = isIdle;
        }

        #endregion

        #region Debug
        
        private void OnGUI()
        {
            if (!_showDebugLogs)
                return;
            
            GUILayout.BeginArea(new Rect(10, 10, 250, 120));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"<b>Interaction Tracker</b>");
            GUILayout.Label($"Status: {(CanTriggerEvents ? "<color=green>ACTIVE</color>" : "<color=red>IDLE</color>")}");
            GUILayout.Label($"Last Interaction: {SecondsSinceLastInteraction:F1}s ago");
            GUILayout.Label($"Until Idle: {SecondsUntilIdle:F1}s");
            GUILayout.Label($"Total Count: {_totalInteractionCount}");
            GUILayout.Label($"Progress: {IdleProgress:P0}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
    
    /// <summary>
    /// Types of player interactions
    /// </summary>
    public enum InteractionType
    {
        Generic,
        Touch,
        MouseClick,
        Keyboard,
        UIButton,
        ResearchStarted,
        ResearchCompleted,
        ProductionStarted,
        BuildingPlaced,
        ContractAccepted,
        ContractCompleted,
        StaffHired,
        UpgradePurchased,
        MenuOpened,
        AppResumed,
        AppFocused,
        TrackingStarted
    }
    
    /// <summary>
    /// Serializable data for tracker state persistence
    /// </summary>
    [Serializable]
    public struct InteractionTrackerSaveData
    {
        public long LastInteractionTime;
        public int TotalInteractionCount;
        public bool IsTracking;
    }
}
