using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Events
{
    /// <summary>
    /// Main controller for the dynamic event system.
    /// Manages random timers, player interaction requirements, and event triggering.
    /// </summary>
    public class DynamicEventManager : MonoBehaviour
    {
        #region Singleton
        
        private static DynamicEventManager _instance;
        public static DynamicEventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DynamicEventManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DynamicEventManager");
                        _instance = go.AddComponent<DynamicEventManager>();
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
            
            InitializeTimer();
        }
        
        #endregion

        #region Fields
        
        [Header("Event Database")]
        [Tooltip("All available game events")]
        [SerializeField]
        private List<GameEventData> _eventDatabase = new List<GameEventData>();
        
        [Header("Timer Settings")]
        [Tooltip("Default minimum interval between events (minutes)")]
        [SerializeField]
        private float _defaultMinIntervalMinutes = 15f;
        
        [Tooltip("Default maximum interval between events (minutes)")]
        [SerializeField]
        private float _defaultMaxIntervalMinutes = 20f;
        
        [Header("Interaction Requirements")]
        [Tooltip("Require player interaction for events to trigger")]
        [SerializeField]
        private bool _requireInteraction = true;
        
        [Tooltip("Interaction threshold in minutes")]
        [SerializeField]
        private float _interactionThresholdMinutes = 3f;
        
        [Header("Event Settings")]
        [Tooltip("Maximum number of events that can queue up")]
        [SerializeField]
        private int _maxQueuedEvents = 3;
        
        [Tooltip("Whether to allow duplicate events in queue")]
        [SerializeField]
        private bool _allowDuplicateEvents = false;
        
        [Header("Debug")]
        [SerializeField]
        private bool _showDebugLogs = false;
        
        [SerializeField]
        private bool _showTimerDebug = false;
        
        private RandomTimer _eventTimer;
        private Queue<GameEventData> _eventQueue = new Queue<GameEventData>();
        private GameEventData _currentEvent;
        private float _sessionTimeMinutes;
        private bool _isPaused;
        private List<string> _triggeredEventHistory = new List<string>();
        
        #endregion

        #region Properties
        
        /// <summary>
        /// The random timer controlling event intervals
        /// </summary>
        public RandomTimer EventTimer => _eventTimer;
        
        /// <summary>
        /// Time until next event in seconds
        /// </summary>
        public float TimeUntilNextEventSeconds => _eventTimer?.RemainingTimeSeconds ?? 0f;
        
        /// <summary>
        /// Time until next event in minutes
        /// </summary>
        public float TimeUntilNextEventMinutes => TimeUntilNextEventSeconds / 60f;
        
        /// <summary>
        /// Current event being displayed/processed
        /// </summary>
        public GameEventData CurrentEvent => _currentEvent;
        
        /// <summary>
        /// Number of events waiting in queue
        /// </summary>
        public int QueuedEventCount => _eventQueue.Count;
        
        /// <summary>
        /// Whether an event is currently being shown
        /// </summary>
        public bool IsEventActive => _currentEvent != null;
        
        /// <summary>
        /// Whether the event system is running
        /// </summary>
        public bool IsRunning => _eventTimer?.IsRunning ?? false;
        
        /// <summary>
        /// Whether the event system is paused
        /// </summary>
        public bool IsPaused => _isPaused;
        
        /// <summary>
        /// Total session time in minutes
        /// </summary>
        public float SessionTimeMinutes => _sessionTimeMinutes;
        
        /// <summary>
        /// Read-only list of available events
        /// </summary>
        public IReadOnlyList<GameEventData> AvailableEvents => _eventDatabase.AsReadOnly();
        
        /// <summary>
        /// History of triggered event IDs this session
        /// </summary>
        public IReadOnlyList<string> EventHistory => _triggeredEventHistory.AsReadOnly();

        #endregion

        #region Events
        
        /// <summary>
        /// Called when an event is triggered
        /// </summary>
        public event Action<GameEventData> OnEventTriggered;
        
        /// <summary>
        /// Called when an event effect is applied
        /// </summary>
        public event Action<ActiveEventEffect> OnEventEffectApplied;
        
        /// <summary>
        /// Called when an event effect expires
        /// </summary>
        public event Action<ActiveEventEffect> OnEventEffectExpired;
        
        /// <summary>
        /// Called when the event timer starts
        /// </summary>
        public event Action OnEventTimerStarted;
        
        /// <summary>
        /// Called when the event timer is paused
        /// </summary>
        public event Action OnEventTimerPaused;
        
        /// <summary>
        /// Called when the event timer is resumed
        /// </summary>
        public event Action OnEventTimerResumed;
        
        /// <summary>
        /// Called when an event is added to queue
        /// </summary>
        public event Action<GameEventData> OnEventQueued;
        
        /// <summary>
        /// Called when an event is dismissed/completed
        /// </summary>
        public event Action<GameEventData> OnEventCompleted;
        
        /// <summary>
        /// Called when a decision event choice is made
        /// </summary>
        public event Action<GameEventData, EventChoice> OnEventChoiceMade;

        #endregion

        #region Lifecycle
        
        private void Start()
        {
            // Ensure we have references to other systems
            if (InteractionTracker.Instance == null)
            {
                Debug.LogWarning("[DynamicEventManager] InteractionTracker not found! Creating one...");
            }
            
            if (EventEffectHandler.Instance == null)
            {
                Debug.LogWarning("[DynamicEventManager] EventEffectHandler not found! Creating one...");
            }
            
            // Subscribe to effect handler events
            if (EventEffectHandler.Instance != null)
            {
                EventEffectHandler.Instance.OnEffectApplied += OnEffectAppliedHandler;
                EventEffectHandler.Instance.OnEffectExpired += OnEffectExpiredHandler;
            }
            
            // Start the event timer
            StartEventTimer();
            
            if (_showDebugLogs)
                Debug.Log("[DynamicEventManager] Initialized");
        }
        
        private void Update()
        {
            if (_isPaused)
                return;
            
            // Update session time
            _sessionTimeMinutes += Time.deltaTime / 60f;
            
            // Update the timer
            _eventTimer?.Update(Time.deltaTime);
            
            // Process queued events if none active
            if (_currentEvent == null && _eventQueue.Count > 0)
            {
                ProcessNextQueuedEvent();
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (EventEffectHandler.Instance != null)
            {
                EventEffectHandler.Instance.OnEffectApplied -= OnEffectAppliedHandler;
                EventEffectHandler.Instance.OnEffectExpired -= OnEffectExpiredHandler;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PauseEventTimer();
            }
            else
            {
                ResumeEventTimer();
            }
        }
        
        #endregion

        #region Public Methods - Timer Control
        
        /// <summary>
        /// Starts the event timer with default intervals
        /// </summary>
        public void StartEventTimer()
        {
            InitializeTimer();
            _eventTimer.Start();
            OnEventTimerStarted?.Invoke();
            
            if (_showDebugLogs)
                Debug.Log("[DynamicEventManager] Event timer started");
        }
        
        /// <summary>
        /// Starts the event timer with custom intervals
        /// </summary>
        public void StartEventTimer(float minMinutes, float maxMinutes)
        {
            _defaultMinIntervalMinutes = minMinutes;
            _defaultMaxIntervalMinutes = maxMinutes;
            StartEventTimer();
        }
        
        /// <summary>
        /// Stops the event timer
        /// </summary>
        public void StopEventTimer()
        {
            _eventTimer?.Stop();
            
            if (_showDebugLogs)
                Debug.Log("[DynamicEventManager] Event timer stopped");
        }
        
        /// <summary>
        /// Pauses the event timer
        /// </summary>
        public void PauseEventTimer()
        {
            _isPaused = true;
            _eventTimer?.Pause();
            OnEventTimerPaused?.Invoke();
            
            if (_showDebugLogs)
                Debug.Log("[DynamicEventManager] Event timer paused");
        }
        
        /// <summary>
        /// Resumes the event timer
        /// </summary>
        public void ResumeEventTimer()
        {
            _isPaused = false;
            _eventTimer?.Resume();
            OnEventTimerResumed?.Invoke();
            
            if (_showDebugLogs)
                Debug.Log("[DynamicEventManager] Event timer resumed");
        }
        
        /// <summary>
        /// Resets the event timer
        /// </summary>
        public void ResetEventTimer()
        {
            _eventTimer?.Reset();
            
            if (_showDebugLogs)
                Debug.Log("[DynamicEventManager] Event timer reset");
        }
        
        /// <summary>
        /// Restarts the event timer with a new random interval
        /// </summary>
        public void RestartEventTimer()
        {
            _eventTimer?.Restart();
            OnEventTimerStarted?.Invoke();
            
            if (_showDebugLogs)
                Debug.Log("[DynamicEventManager] Event timer restarted");
        }
        
        /// <summary>
        /// Sets the timer interval range
        /// </summary>
        public void SetTimerInterval(float minMinutes, float maxMinutes)
        {
            _defaultMinIntervalMinutes = minMinutes;
            _defaultMaxIntervalMinutes = maxMinutes;
            _eventTimer?.SetIntervalRange(minMinutes * 60f, maxMinutes * 60f);
        }

        #endregion

        #region Public Methods - Event Triggering
        
        /// <summary>
        /// Registers a player interaction. Call this from UI buttons, game actions, etc.
        /// </summary>
        public void RegisterPlayerInteraction(InteractionType type = InteractionType.Generic)
        {
            InteractionTracker.Instance?.RecordInteraction(type);
        }
        
        /// <summary>
        /// Forces a random event to trigger (for testing)
        /// </summary>
        public GameEventData TriggerRandomEvent()
        {
            var eligibleEvents = GetEligibleEvents();
            
            if (eligibleEvents.Count == 0)
            {
                Debug.LogWarning("[DynamicEventManager] No eligible events to trigger");
                return null;
            }
            
            var selectedEvent = SelectWeightedRandomEvent(eligibleEvents);
            TriggerEvent(selectedEvent);
            
            return selectedEvent;
        }
        
        /// <summary>
        /// Forces a specific event to trigger
        /// </summary>
        public bool TriggerEvent(string eventId)
        {
            var eventData = _eventDatabase.FirstOrDefault(e => e.eventId == eventId);
            if (eventData != null)
            {
                TriggerEvent(eventData);
                return true;
            }
            
            Debug.LogWarning($"[DynamicEventManager] Event not found: {eventId}");
            return false;
        }
        
        /// <summary>
        /// Triggers a specific event
        /// </summary>
        public void TriggerEvent(GameEventData eventData)
        {
            if (eventData == null)
            {
                Debug.LogWarning("[DynamicEventManager] Cannot trigger null event");
                return;
            }
            
            // If an event is already active, queue this one
            if (_currentEvent != null)
            {
                if (_eventQueue.Count < _maxQueuedEvents)
                {
                    QueueEvent(eventData);
                }
                else
                {
                    Debug.LogWarning("[DynamicEventManager] Event queue is full");
                }
                return;
            }
            
            // Set as current event
            _currentEvent = eventData;
            _currentEvent.OnTriggered();
            _triggeredEventHistory.Add(eventData.eventId);
            
            if (_showDebugLogs)
                Debug.Log($"[DynamicEventManager] Event triggered: {eventData.displayName}");
            
            // Apply automatic effects
            if (eventData.effects != null && eventData.effects.Count > 0)
            {
                ApplyEventEffects(eventData);
            }
            
            // Notify listeners
            OnEventTriggered?.Invoke(eventData);
            
            // Restart timer for next event
            RestartEventTimer();
        }
        
        /// <summary>
        /// Makes a choice for the current decision event
        /// </summary>
        public void MakeEventChoice(string choiceId)
        {
            if (_currentEvent == null || !_currentEvent.isDecisionEvent)
                return;
            
            var choice = _currentEvent.choices?.FirstOrDefault(c => c.choiceId == choiceId);
            if (choice == null)
            {
                Debug.LogWarning($"[DynamicEventManager] Choice not found: {choiceId}");
                return;
            }
            
            // Apply choice outcomes
            if (choice.outcomes != null && choice.outcomes.Count > 0)
            {
                foreach (var outcome in choice.outcomes)
                {
                    EventEffectHandler.Instance?.ApplyEffect(_currentEvent.eventId, outcome);
                }
            }
            
            OnEventChoiceMade?.Invoke(_currentEvent, choice);
            
            if (_showDebugLogs)
                Debug.Log($"[DynamicEventManager] Choice made: {choice.displayText}");
            
            CompleteCurrentEvent();
        }
        
        /// <summary>
        /// Completes and dismisses the current event
        /// </summary>
        public void CompleteCurrentEvent()
        {
            if (_currentEvent == null)
                return;
            
            var completedEvent = _currentEvent;
            _currentEvent = null;
            
            OnEventCompleted?.Invoke(completedEvent);
            
            if (_showDebugLogs)
                Debug.Log($"[DynamicEventManager] Event completed: {completedEvent.displayName}");
            
            // Process next queued event
            if (_eventQueue.Count > 0)
            {
                ProcessNextQueuedEvent();
            }
        }
        
        /// <summary>
        /// Skips the current event without applying effects
        /// </summary>
        public void SkipCurrentEvent()
        {
            if (_currentEvent == null)
                return;
            
            var skippedEvent = _currentEvent;
            _currentEvent = null;
            
            if (_showDebugLogs)
                Debug.Log($"[DynamicEventManager] Event skipped: {skippedEvent.displayName}");
            
            // Process next queued event
            if (_eventQueue.Count > 0)
            {
                ProcessNextQueuedEvent();
            }
        }

        #endregion

        #region Public Methods - Event Database
        
        /// <summary>
        /// Adds an event to the database
        /// </summary>
        public void RegisterEvent(GameEventData eventData)
        {
            if (eventData == null)
                return;
            
            if (!_eventDatabase.Any(e => e.eventId == eventData.eventId))
            {
                _eventDatabase.Add(eventData);
            }
        }
        
        /// <summary>
        /// Removes an event from the database
        /// </summary>
        public bool UnregisterEvent(string eventId)
        {
            var eventData = _eventDatabase.FirstOrDefault(e => e.eventId == eventId);
            if (eventData != null)
            {
                return _eventDatabase.Remove(eventData);
            }
            return false;
        }
        
        /// <summary>
        /// Gets an event by ID
        /// </summary>
        public GameEventData GetEvent(string eventId)
        {
            return _eventDatabase.FirstOrDefault(e => e.eventId == eventId);
        }
        
        /// <summary>
        /// Gets all events of a specific type
        /// </summary>
        public List<GameEventData> GetEventsOfType(EventType type)
        {
            return _eventDatabase.Where(e => e.eventType == type).ToList();
        }
        
        /// <summary>
        /// Clears the event database
        /// </summary>
        public void ClearEventDatabase()
        {
            _eventDatabase.Clear();
        }

        #endregion

        #region Public Methods - Effect Management
        
        /// <summary>
        /// Applies all effects from an event
        /// </summary>
        public List<ActiveEventEffect> ApplyEventEffects(GameEventData eventData)
        {
            if (eventData?.effects == null)
                return new List<ActiveEventEffect>();
            
            return EventEffectHandler.Instance?.ApplyEffects(eventData.eventId, eventData.effects) 
                ?? new List<ActiveEventEffect>();
        }
        
        /// <summary>
        /// Gets all active effects
        /// </summary>
        public List<ActiveEventEffect> GetActiveEffects()
        {
            return EventEffectHandler.Instance?.ActiveEffects?.ToList() ?? new List<ActiveEventEffect>();
        }
        
        /// <summary>
        /// Gets active effects of a specific type
        /// </summary>
        public List<ActiveEventEffect> GetActiveEffectsOfType(EffectType type)
        {
            return EventEffectHandler.Instance?.GetActiveEffectsOfType(type) ?? new List<ActiveEventEffect>();
        }
        
        /// <summary>
        /// Gets the multiplier for an effect type
        /// </summary>
        public float GetEffectMultiplier(EffectType type)
        {
            return EventEffectHandler.Instance?.GetEffectMultiplier(type) ?? 1f;
        }

        #endregion

        #region Public Methods - Save/Load
        
        /// <summary>
        /// Returns manager state for serialization
        /// </summary>
        public DynamicEventManagerSaveData GetSaveData()
        {
            return new DynamicEventManagerSaveData
            {
                TimerData = _eventTimer?.GetSaveData() ?? new TimerSaveData(),
                SessionTimeMinutes = _sessionTimeMinutes,
                TriggeredEventHistory = new List<string>(_triggeredEventHistory),
                QueuedEventIds = _eventQueue.Select(e => e.eventId).ToList()
            };
        }
        
        /// <summary>
        /// Restores manager state from serialization
        /// </summary>
        public void LoadSaveData(DynamicEventManagerSaveData data)
        {
            _eventTimer?.LoadSaveData(data.TimerData);
            _sessionTimeMinutes = data.SessionTimeMinutes;
            _triggeredEventHistory = data.TriggeredEventHistory ?? new List<string>();
            
            // Restore queued events
            _eventQueue.Clear();
            if (data.QueuedEventIds != null)
            {
                foreach (var eventId in data.QueuedEventIds)
                {
                    var eventData = GetEvent(eventId);
                    if (eventData != null)
                        _eventQueue.Enqueue(eventData);
                }
            }
        }

        #endregion

        #region Private Methods
        
        private void InitializeTimer()
        {
            float minSeconds = _defaultMinIntervalMinutes * 60f;
            float maxSeconds = _defaultMaxIntervalMinutes * 60f;
            
            _eventTimer = new RandomTimer(minSeconds, maxSeconds);
            _eventTimer.OnTimerComplete += OnTimerComplete;
            
            if (_showDebugLogs)
                Debug.Log($"[DynamicEventManager] Timer initialized ({_defaultMinIntervalMinutes}-{_defaultMaxIntervalMinutes} min)");
        }
        
        private void OnTimerComplete()
        {
            if (_showDebugLogs)
                Debug.Log("[DynamicEventManager] Timer complete - checking for event trigger");
            
            // Check if we can trigger events
            if (!CanTriggerEvents())
            {
                if (_showDebugLogs)
                    Debug.Log("[DynamicEventManager] Cannot trigger event - requirements not met");
                
                // Restart timer and try again later
                RestartEventTimer();
                return;
            }
            
            // Trigger a random event
            TriggerRandomEvent();
        }
        
        private bool CanTriggerEvents()
        {
            // Check interaction requirement
            if (_requireInteraction && InteractionTracker.Instance != null)
            {
                if (!InteractionTracker.Instance.CanTriggerEvents)
                {
                    return false;
                }
            }
            
            // Check if we have eligible events
            var eligibleEvents = GetEligibleEvents();
            return eligibleEvents.Count > 0;
        }
        
        private List<GameEventData> GetEligibleEvents()
        {
            return _eventDatabase
                .Where(e => e.CanTrigger(_sessionTimeMinutes))
                .ToList();
        }
        
        private GameEventData SelectWeightedRandomEvent(List<GameEventData> events)
        {
            if (events == null || events.Count == 0)
                return null;
            
            if (events.Count == 1)
                return events[0];
            
            // Calculate total weight
            float totalWeight = events.Sum(e => e.GetEffectiveWeight());
            
            // Random selection
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var eventData in events)
            {
                currentWeight += eventData.GetEffectiveWeight();
                if (randomValue <= currentWeight)
                {
                    return eventData;
                }
            }
            
            // Fallback to last event
            return events[events.Count - 1];
        }
        
        private void QueueEvent(GameEventData eventData)
        {
            if (!_allowDuplicateEvents && _eventQueue.Any(e => e.eventId == eventData.eventId))
                return;
            
            _eventQueue.Enqueue(eventData);
            OnEventQueued?.Invoke(eventData);
            
            if (_showDebugLogs)
                Debug.Log($"[DynamicEventManager] Event queued: {eventData.displayName}");
        }
        
        private void ProcessNextQueuedEvent()
        {
            if (_eventQueue.Count == 0)
                return;
            
            var nextEvent = _eventQueue.Dequeue();
            TriggerEvent(nextEvent);
        }
        
        private void OnEffectAppliedHandler(ActiveEventEffect effect)
        {
            OnEventEffectApplied?.Invoke(effect);
        }
        
        private void OnEffectExpiredHandler(ActiveEventEffect effect)
        {
            OnEventEffectExpired?.Invoke(effect);
        }

        #endregion

        #region Debug
        
        private void OnGUI()
        {
            if (!_showTimerDebug)
                return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"<b>Dynamic Event Manager</b>");
            GUILayout.Label($"Status: {(IsRunning ? "<color=green>Running</color>" : "<color=red>Stopped</color>")}");
            GUILayout.Label($"Paused: {(IsPaused ? "Yes" : "No")}");
            GUILayout.Label($"Session Time: {_sessionTimeMinutes:F1}m");
            GUILayout.Label($"Next Event: {_eventTimer?.GetFormattedRemainingTime() ?? "--:--"}");
            GUILayout.Label($"Progress: {_eventTimer?.Progress ?? 0:P0}");
            GUILayout.Label($"Queue: {_eventQueue.Count}/{_maxQueuedEvents}");
            GUILayout.Label($"Active Event: {(_currentEvent != null ? _currentEvent.displayName : "None")}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
    
    /// <summary>
    /// Serializable data for manager state persistence
    /// </summary>
    [Serializable]
    public struct DynamicEventManagerSaveData
    {
        public TimerSaveData TimerData;
        public float SessionTimeMinutes;
        public List<string> TriggeredEventHistory;
        public List<string> QueuedEventIds;
    }
}
