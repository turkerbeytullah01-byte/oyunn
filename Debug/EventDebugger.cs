using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Event debugger for manual event triggering and testing
    /// Allows developers to test event systems and edge cases
    /// </summary>
    public class EventDebugger : MonoBehaviour
    {
        #region Singleton
        private static EventDebugger _instance;
        public static EventDebugger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EventDebugger>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EventDebugger");
                        _instance = go.AddComponent<EventDebugger>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region UI References
        [Header("UI References")]
        [SerializeField] private TMP_Dropdown eventDropdown;
        [SerializeField] private Button triggerEventButton;
        [SerializeField] private Button triggerRandomEventButton;
        [SerializeField] private Button triggerAllEventsButton;
        [SerializeField] private Button clearEffectsButton;
        [SerializeField] private Button chainEventsButton;
        [SerializeField] private TextMeshProUGUI activeEffectsText;
        [SerializeField] private TextMeshProUGUI eventHistoryText;
        [SerializeField] private ScrollRect eventHistoryScrollRect;
        
        [Header("Event Parameters")]
        [SerializeField] private Slider severitySlider;
        [SerializeField] private TextMeshProUGUI severityText;
        [SerializeField] private Toggle forceSuccessToggle;
        [SerializeField] private Toggle forceFailureToggle;
        [SerializeField] private TMP_InputField customDelayInput;
        #endregion

        #region Settings
        [Header("Settings")]
        [Tooltip("Delay between chained events (seconds)")]
        public float chainEventDelay = 2f;
        
        [Tooltip("Maximum events to keep in history")]
        public int maxEventHistory = 50;
        
        [Tooltip("Log all event triggers")]
        public bool logEventTriggers = true;
        
        [Tooltip("Show notification on event trigger")]
        public bool showNotifications = true;
        #endregion

        #region Event Registry
        [System.Serializable]
        public class DebugEventInfo
        {
            public string eventId;
            public string eventName;
            public string description;
            public EventCategory category;
            public EventSeverity defaultSeverity;
            public bool canBeRandom;
        }

        public enum EventCategory
        {
            Economic,
            Technical,
            Security,
            Market,
            Natural,
            Political,
            Special
        }

        public enum EventSeverity
        {
            Minor,
            Moderate,
            Major,
            Critical,
            Catastrophic
        }

        [Header("Available Events")]
        public List<DebugEventInfo> availableEvents = new List<DebugEventInfo>
        {
            new DebugEventInfo { eventId = "market_crash", eventName = "Market Crash", description = "Stock market crashes, affecting investments", category = EventCategory.Economic, defaultSeverity = EventSeverity.Major, canBeRandom = true },
            new DebugEventInfo { eventId = "tech_breakthrough", eventName = "Tech Breakthrough", description = "Major technological advancement", category = EventCategory.Technical, defaultSeverity = EventSeverity.Minor, canBeRandom = true },
            new DebugEventInfo { eventId = "competitor_launch", eventName = "Competitor Launch", description = "Competitor releases new product", category = EventCategory.Market, defaultSeverity = EventSeverity.Moderate, canBeRandom = true },
            new DebugEventInfo { eventId = "regulatory_change", eventName = "Regulatory Change", description = "New regulations affect operations", category = EventCategory.Political, defaultSeverity = EventSeverity.Moderate, canBeRandom = true },
            new DebugEventInfo { eventId = "supply_shortage", eventName = "Supply Shortage", description = "Component supply chain disrupted", category = EventCategory.Economic, defaultSeverity = EventSeverity.Major, canBeRandom = true },
            new DebugEventInfo { eventId = "reputation_boost", eventName = "Reputation Boost", description = "Positive PR event", category = EventCategory.Market, defaultSeverity = EventSeverity.Minor, canBeRandom = true },
            new DebugEventInfo { eventId = "security_breach", eventName = "Security Breach", description = "Data or physical security compromised", category = EventCategory.Security, defaultSeverity = EventSeverity.Critical, canBeRandom = true },
            new DebugEventInfo { eventId = "natural_disaster", eventName = "Natural Disaster", description = "Weather event affects operations", category = EventCategory.Natural, defaultSeverity = EventSeverity.Major, canBeRandom = true },
            new DebugEventInfo { eventId = "economic_boom", eventName = "Economic Boom", description = "Market conditions improve", category = EventCategory.Economic, defaultSeverity = EventSeverity.Minor, canBeRandom = true },
            new DebugEventInfo { eventId = "hack_attempt", eventName = "Hack Attempt", description = "Cyber attack on systems", category = EventCategory.Security, defaultSeverity = EventSeverity.Moderate, canBeRandom = true },
            new DebugEventInfo { eventId = "key_employee_leaves", eventName = "Key Employee Leaves", description = "Important staff member departs", category = EventCategory.Special, defaultSeverity = EventSeverity.Moderate, canBeRandom = true },
            new DebugEventInfo { eventId = "patent_infringement", eventName = "Patent Infringement", description = "Legal challenge to technology", category = EventCategory.Political, defaultSeverity = EventSeverity.Major, canBeRandom = true },
            new DebugEventInfo { eventId = "viral_marketing", eventName = "Viral Marketing", description = "Product goes viral", category = EventCategory.Market, defaultSeverity = EventSeverity.Minor, canBeRandom = true },
            new DebugEventInfo { eventId = "equipment_failure", eventName = "Equipment Failure", description = "Manufacturing equipment breaks down", category = EventCategory.Technical, defaultSeverity = EventSeverity.Moderate, canBeRandom = true },
            new DebugEventInfo { eventId = "data_corruption", eventName = "Data Corruption", description = "Research data lost or corrupted", category = EventCategory.Technical, defaultSeverity = EventSeverity.Critical, canBeRandom = true }
        };
        #endregion

        #region Events
        public event Action<string> OnEventTriggered;
        public event Action OnAllEventsTriggered;
        public event Action OnEffectsCleared;
        #endregion

        #region Private Fields
        private List<string> _eventHistory = new List<string>();
        private List<ActiveEventEffect> _activeEffects = new List<ActiveEventEffect>();
        private Coroutine _chainEventsCoroutine;
        
        private class ActiveEventEffect
        {
            public string eventId;
            public string eventName;
            public float startTime;
            public float duration;
            public EventSeverity severity;
            public Dictionary<string, float> modifiers;
        }
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
        }

        private void Start()
        {
            PopulateEventList();
            SetupUIListeners();
        }
        #endregion

        #region UI Setup
        private void SetupUIListeners()
        {
            if (triggerEventButton != null)
                triggerEventButton.onClick.AddListener(TriggerSelectedEvent);
            
            if (triggerRandomEventButton != null)
                triggerRandomEventButton.onClick.AddListener(TriggerRandomEvent);
            
            if (triggerAllEventsButton != null)
                triggerAllEventsButton.onClick.AddListener(() => StartCoroutine(TriggerAllEvents()));
            
            if (clearEffectsButton != null)
                clearEffectsButton.onClick.AddListener(ClearActiveEffects);
            
            if (chainEventsButton != null)
                chainEventsButton.onClick.AddListener(() => StartCoroutine(ChainRandomEvents(5)));
            
            if (severitySlider != null)
            {
                severitySlider.onValueChanged.AddListener(OnSeverityChanged);
                OnSeverityChanged(severitySlider.value);
            }
        }

        private void OnSeverityChanged(float value)
        {
            if (severityText != null)
            {
                EventSeverity severity = (EventSeverity)Mathf.FloorToInt(value);
                severityText.text = $"Severity: {severity}";
                severityText.color = GetSeverityColor(severity);
            }
        }
        #endregion

        #region Event Population
        /// <summary>
        /// Populate the event dropdown with available events
        /// </summary>
        public void PopulateEventList()
        {
            if (eventDropdown == null) return;
            
            eventDropdown.ClearOptions();
            
            var options = new List<TMP_Dropdown.OptionData>();
            options.Add(new TMP_Dropdown.OptionData("Select Event..."));
            
            foreach (var evt in availableEvents)
            {
                string displayText = $"[{evt.category}] {evt.eventName}";
                options.Add(new TMP_Dropdown.OptionData(displayText));
            }
            
            eventDropdown.AddOptions(options);
        }

        /// <summary>
        /// Get event info by ID
        /// </summary>
        public DebugEventInfo GetEventInfo(string eventId)
        {
            return availableEvents.Find(e => e.eventId == eventId);
        }

        /// <summary>
        /// Get events by category
        /// </summary>
        public List<DebugEventInfo> GetEventsByCategory(EventCategory category)
        {
            return availableEvents.FindAll(e => e.category == category);
        }
        #endregion

        #region Event Triggering
        /// <summary>
        /// Trigger the currently selected event from dropdown
        /// </summary>
        public void TriggerSelectedEvent()
        {
            if (eventDropdown == null || eventDropdown.value <= 0) return;
            
            int selectedIndex = eventDropdown.value - 1;
            if (selectedIndex >= 0 && selectedIndex < availableEvents.Count)
            {
                var evt = availableEvents[selectedIndex];
                TriggerEvent(evt.eventId);
            }
        }

        /// <summary>
        /// Trigger a specific event by ID
        /// </summary>
        public void TriggerEvent(string eventId)
        {
            var evt = GetEventInfo(eventId);
            if (evt == null)
            {
                UnityEngine.Debug.LogWarning($"[EventDebugger] Event not found: {eventId}");
                return;
            }
            
            EventSeverity severity = GetSelectedSeverity();
            TriggerEvent(evt, severity);
        }

        /// <summary>
        /// Trigger event with specific severity
        /// </summary>
        public void TriggerEvent(DebugEventInfo evt, EventSeverity severity)
        {
            // Apply force success/failure
            bool forceSuccess = forceSuccessToggle != null && forceSuccessToggle.isOn;
            bool forceFailure = forceFailureToggle != null && forceFailureToggle.isOn;
            
            // Log event trigger
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] Triggered: {evt.eventName} ({severity})";
            AddToHistory(logEntry);
            
            if (logEventTriggers)
            {
                UnityEngine.Debug.Log($"[EventDebugger] {logEntry}");
            }
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Event triggered: {evt.eventName}");
            }
            
            // Create and apply event effect
            ApplyEventEffect(evt, severity);
            
            // Notify listeners
            OnEventTriggered?.Invoke(evt.eventId);
            
            // Show notification
            if (showNotifications)
            {
                ShowEventNotification(evt, severity);
            }
        }

        /// <summary>
        /// Trigger a random event
        /// </summary>
        public void TriggerRandomEvent()
        {
            if (availableEvents.Count == 0) return;
            
            // Filter to only random-enabled events
            var randomEvents = availableEvents.FindAll(e => e.canBeRandom);
            if (randomEvents.Count == 0) randomEvents = availableEvents;
            
            int randomIndex = UnityEngine.Random.Range(0, randomEvents.Count);
            var evt = randomEvents[randomIndex];
            
            // Random severity around default
            EventSeverity severity = GetRandomSeverity(evt.defaultSeverity);
            
            TriggerEvent(evt, severity);
        }

        /// <summary>
        /// Trigger all events sequentially
        /// </summary>
        public IEnumerator TriggerAllEvents()
        {
            if (_chainEventsCoroutine != null)
            {
                StopCoroutine(_chainEventsCoroutine);
            }
            
            UnityEngine.Debug.Log("[EventDebugger] Triggering all events sequentially...");
            
            foreach (var evt in availableEvents)
            {
                TriggerEvent(evt.eventId);
                yield return new WaitForSeconds(chainEventDelay);
            }
            
            OnAllEventsTriggered?.Invoke();
            UnityEngine.Debug.Log("[EventDebugger] All events triggered");
        }

        /// <summary>
        /// Chain multiple random events
        /// </summary>
        public IEnumerator ChainRandomEvents(int count)
        {
            UnityEngine.Debug.Log($"[EventDebugger] Chaining {count} random events...");
            
            for (int i = 0; i < count; i++)
            {
                TriggerRandomEvent();
                yield return new WaitForSeconds(chainEventDelay);
            }
            
            UnityEngine.Debug.Log("[EventDebugger] Event chain completed");
        }

        /// <summary>
        /// Trigger events by category
        /// </summary>
        public void TriggerEventsByCategory(EventCategory category)
        {
            var events = GetEventsByCategory(category);
            foreach (var evt in events)
            {
                TriggerEvent(evt.eventId);
            }
        }
        #endregion

        #region Event Effects
        private void ApplyEventEffect(DebugEventInfo evt, EventSeverity severity)
        {
            var effect = new ActiveEventEffect
            {
                eventId = evt.eventId,
                eventName = evt.eventName,
                startTime = Time.time,
                duration = GetEffectDuration(severity),
                severity = severity,
                modifiers = GenerateModifiers(evt, severity)
            };
            
            _activeEffects.Add(effect);
            UpdateActiveEffectsDisplay();
            
            // Apply modifiers to game systems
            ApplyModifiers(effect.modifiers);
        }

        private Dictionary<string, float> GenerateModifiers(DebugEventInfo evt, EventSeverity severity)
        {
            var modifiers = new Dictionary<string, float>();
            float multiplier = GetSeverityMultiplier(severity);
            
            switch (evt.category)
            {
                case EventCategory.Economic:
                    modifiers["money_generation"] = 1f - (0.1f * multiplier);
                    modifiers["investment_returns"] = 1f - (0.15f * multiplier);
                    break;
                    
                case EventCategory.Technical:
                    modifiers["research_speed"] = 1f + (0.2f * multiplier);
                    modifiers["failure_chance"] = 0.05f * multiplier;
                    break;
                    
                case EventCategory.Security:
                    modifiers["security_level"] = 1f - (0.2f * multiplier);
                    modifiers["breach_risk"] = 0.1f * multiplier;
                    break;
                    
                case EventCategory.Market:
                    modifiers["reputation"] = 1f + (0.1f * multiplier);
                    modifiers["contract_chance"] = 1f + (0.15f * multiplier);
                    break;
                    
                case EventCategory.Natural:
                    modifiers["production_speed"] = 1f - (0.25f * multiplier);
                    modifiers["delivery_time"] = 1f + (0.2f * multiplier);
                    break;
                    
                case EventCategory.Political:
                    modifiers["compliance_cost"] = 1f + (0.3f * multiplier);
                    modifiers["regulatory_risk"] = 0.15f * multiplier;
                    break;
            }
            
            return modifiers;
        }

        private void ApplyModifiers(Dictionary<string, float> modifiers)
        {
            // Apply modifiers to relevant game systems
            foreach (var modifier in modifiers)
            {
                UnityEngine.Debug.Log($"[EventDebugger] Modifier applied: {modifier.Key} = {modifier.Value:F2}");
            }
        }

        /// <summary>
        /// Clear all active event effects
        /// </summary>
        public void ClearActiveEffects()
        {
            _activeEffects.Clear();
            UpdateActiveEffectsDisplay();
            
            OnEffectsCleared?.Invoke();
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("All event effects cleared");
            }
        }

        /// <summary>
        /// Remove a specific event effect
        /// </summary>
        public void RemoveEventEffect(string eventId)
        {
            _activeEffects.RemoveAll(e => e.eventId == eventId);
            UpdateActiveEffectsDisplay();
        }

        private void UpdateActiveEffectsDisplay()
        {
            if (activeEffectsText == null) return;
            
            if (_activeEffects.Count == 0)
            {
                activeEffectsText.text = "No active effects";
                return;
            }
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Active Effects:");
            
            foreach (var effect in _activeEffects)
            {
                float remaining = Mathf.Max(0, effect.duration - (Time.time - effect.startTime));
                sb.AppendLine($"- {effect.eventName} ({remaining:F0}s)");
            }
            
            activeEffectsText.text = sb.ToString();
        }
        #endregion

        #region History Management
        private void AddToHistory(string entry)
        {
            _eventHistory.Add(entry);
            
            while (_eventHistory.Count > maxEventHistory)
            {
                _eventHistory.RemoveAt(0);
            }
            
            UpdateHistoryDisplay();
        }

        private void UpdateHistoryDisplay()
        {
            if (eventHistoryText == null) return;
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Event History:");
            
            for (int i = _eventHistory.Count - 1; i >= 0; i--)
            {
                sb.AppendLine(_eventHistory[i]);
            }
            
            eventHistoryText.text = sb.ToString();
        }

        /// <summary>
        /// Clear event history
        /// </summary>
        public void ClearHistory()
        {
            _eventHistory.Clear();
            UpdateHistoryDisplay();
        }

        /// <summary>
        /// Get event history
        /// </summary>
        public List<string> GetEventHistory()
        {
            return new List<string>(_eventHistory);
        }
        #endregion

        #region Utility Methods
        private EventSeverity GetSelectedSeverity()
        {
            if (severitySlider != null)
            {
                return (EventSeverity)Mathf.FloorToInt(severitySlider.value);
            }
            return EventSeverity.Moderate;
        }

        private EventSeverity GetRandomSeverity(EventSeverity baseSeverity)
        {
            int baseValue = (int)baseSeverity;
            int variance = UnityEngine.Random.Range(-1, 2);
            int result = Mathf.Clamp(baseValue + variance, 0, 4);
            return (EventSeverity)result;
        }

        private float GetSeverityMultiplier(EventSeverity severity)
        {
            switch (severity)
            {
                case EventSeverity.Minor: return 0.5f;
                case EventSeverity.Moderate: return 1f;
                case EventSeverity.Major: return 2f;
                case EventSeverity.Critical: return 3f;
                case EventSeverity.Catastrophic: return 5f;
                default: return 1f;
            }
        }

        private float GetEffectDuration(EventSeverity severity)
        {
            switch (severity)
            {
                case EventSeverity.Minor: return 60f;
                case EventSeverity.Moderate: return 180f;
                case EventSeverity.Major: return 300f;
                case EventSeverity.Critical: return 600f;
                case EventSeverity.Catastrophic: return 1200f;
                default: return 180f;
            }
        }

        private Color GetSeverityColor(EventSeverity severity)
        {
            switch (severity)
            {
                case EventSeverity.Minor: return Color.green;
                case EventSeverity.Moderate: return Color.yellow;
                case EventSeverity.Major: return new Color(1f, 0.5f, 0f); // Orange
                case EventSeverity.Critical: return Color.red;
                case EventSeverity.Catastrophic: return new Color(0.5f, 0f, 0f); // Dark red
                default: return Color.white;
            }
        }

        private void ShowEventNotification(DebugEventInfo evt, EventSeverity severity)
        {
            // This would integrate with a notification system
            UnityEngine.Debug.Log($"[Event Notification] {evt.eventName} - {evt.description}");
        }

        /// <summary>
        /// Get active effect count
        /// </summary>
        public int GetActiveEffectCount()
        {
            return _activeEffects.Count;
        }

        /// <summary>
        /// Check if an event effect is active
        /// </summary>
        public bool IsEffectActive(string eventId)
        {
            return _activeEffects.Exists(e => e.eventId == eventId);
        }
        #endregion

        #region Update
        private void Update()
        {
            // Update active effects display
            if (_activeEffects.Count > 0 && activeEffectsText != null)
            {
                UpdateActiveEffectsDisplay();
            }
            
            // Remove expired effects
            _activeEffects.RemoveAll(e => Time.time - e.startTime >= e.duration);
        }
        #endregion
    }
}
