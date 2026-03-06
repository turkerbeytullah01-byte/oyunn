using UnityEngine;
using ProjectAegis.Systems.Events;

namespace ProjectAegis.Examples
{
    /// <summary>
    /// Example implementation showing how to use the Dynamic Event System
    /// </summary>
    public class EventSystemExample : MonoBehaviour
    {
        [Header("Event System References")]
        [SerializeField] private DynamicEventManager _eventManager;
        [SerializeField] private InteractionTracker _interactionTracker;
        [SerializeField] private EventEffectHandler _effectHandler;
        
        [Header("Test Events")]
        [SerializeField] private bool _triggerTestEventOnStart = false;
        [SerializeField] private string _testEventId = "eureka_moment";
        
        private void Start()
        {
            // Get or create singleton instances
            _eventManager = DynamicEventManager.Instance;
            _interactionTracker = InteractionTracker.Instance;
            _effectHandler = EventEffectHandler.Instance;
            
            // Subscribe to events
            SubscribeToEvents();
            
            // Register MVP events
            RegisterMVPEvents();
            
            // Start the event system
            _eventManager.StartEventTimer();
            
            // Optional: Trigger test event
            if (_triggerTestEventOnStart)
            {
                Invoke(nameof(TriggerTestEvent), 2f);
            }
        }
        
        private void SubscribeToEvents()
        {
            // Event triggered
            _eventManager.OnEventTriggered += (evt) =>
            {
                Debug.Log($"[EventSystemExample] Event triggered: {evt.displayName}");
                Debug.Log($"[EventSystemExample] Description: {evt.description}");
                
                // Show UI popup here
                ShowEventPopup(evt);
            };
            
            // Effect applied
            _eventManager.OnEventEffectApplied += (effect) =>
            {
                Debug.Log($"[EventSystemExample] Effect applied: {effect.effectData.effectType} = {effect.effectData.value}");
            };
            
            // Effect expired
            _eventManager.OnEventEffectExpired += (effect) =>
            {
                Debug.Log($"[EventSystemExample] Effect expired: {effect.effectData.effectType}");
            };
            
            // Choice made (for decision events)
            _eventManager.OnEventChoiceMade += (evt, choice) =>
            {
                Debug.Log($"[EventSystemExample] Player chose '{choice.displayText}' for event '{evt.displayName}'");
            };
            
            // Timer events
            _eventManager.OnEventTimerStarted += () =>
            {
                Debug.Log("[EventSystemExample] Event timer started");
            };
            
            _eventManager.OnEventTimerPaused += () =>
            {
                Debug.Log("[EventSystemExample] Event timer paused");
            };
            
            // Interaction tracker events
            _interactionTracker.OnInteractionRecorded += () =>
            {
                // Debug.Log("[EventSystemExample] Player interaction recorded");
            };
            
            _interactionTracker.OnPlayerBecameIdle += () =>
            {
                Debug.Log("[EventSystemExample] Player became idle - events will not trigger");
            };
            
            _interactionTracker.OnPlayerBecameActive += () =>
            {
                Debug.Log("[EventSystemExample] Player became active - events can trigger");
            };
        }
        
        private void RegisterMVPEvents()
        {
            // Create and register all MVP events
            var mvpEvents = MVPEvents.CreateAllMVPEvents();
            
            foreach (var evt in mvpEvents)
            {
                _eventManager.RegisterEvent(evt);
                Debug.Log($"[EventSystemExample] Registered event: {evt.displayName}");
            }
        }
        
        private void TriggerTestEvent()
        {
            Debug.Log($"[EventSystemExample] Triggering test event: {_testEventId}");
            _eventManager.TriggerEvent(_testEventId);
        }
        
        /// <summary>
        /// Shows an event popup UI
        /// </summary>
        private void ShowEventPopup(GameEventData evt)
        {
            // This would integrate with your UI system
            // Example implementation:
            
            if (evt.isDecisionEvent)
            {
                // Show decision popup with choices
                Debug.Log($"[EventSystemExample] Showing decision popup with {evt.choices?.Count ?? 0} choices");
                
                foreach (var choice in evt.choices)
                {
                    Debug.Log($"  - {choice.displayText}: {choice.description}");
                }
            }
            else
            {
                // Show notification popup
                Debug.Log($"[EventSystemExample] Showing notification: {evt.notificationText}");
                
                // Auto-dismiss after delay or wait for player input
                Invoke(nameof(DismissCurrentEvent), 3f);
            }
        }
        
        private void DismissCurrentEvent()
        {
            _eventManager.CompleteCurrentEvent();
        }
        
        #region Example UI Button Methods
        
        /// <summary>
        /// Call this from a UI button to register interaction
        /// </summary>
        public void OnUIButtonClick()
        {
            _eventManager.RegisterPlayerInteraction(InteractionType.UIButton);
            Debug.Log("[EventSystemExample] UI Button clicked - interaction registered");
        }
        
        /// <summary>
        /// Call this when starting research
        /// </summary>
        public void OnResearchStarted()
        {
            _eventManager.RegisterPlayerInteraction(InteractionType.ResearchStarted);
            
            // Apply research speed boost if active
            float speedMultiplier = _effectHandler.GetEffectMultiplier(EffectType.ResearchSpeedBoost);
            Debug.Log($"[EventSystemExample] Research started with speed multiplier: {speedMultiplier:P0}");
        }
        
        /// <summary>
        /// Call this when placing a building
        /// </summary>
        public void OnBuildingPlaced()
        {
            _eventManager.RegisterPlayerInteraction(InteractionType.BuildingPlaced);
        }
        
        /// <summary>
        /// Call this when accepting a contract
        /// </summary>
        public void OnContractAccepted()
        {
            _eventManager.RegisterPlayerInteraction(InteractionType.ContractAccepted);
            
            // Apply contract value boost if active
            float valueMultiplier = _effectHandler.GetEffectMultiplier(EffectType.ContractValueBoost);
            Debug.Log($"[EventSystemExample] Contract accepted with value multiplier: {valueMultiplier:P0}");
        }
        
        /// <summary>
        /// Make a choice for the current decision event
        /// </summary>
        public void MakeEventChoice(string choiceId)
        {
            _eventManager.MakeEventChoice(choiceId);
        }
        
        #endregion
        
        #region Debug Methods
        
        [ContextMenu("Force Random Event")]
        private void DebugForceRandomEvent()
        {
            var evt = _eventManager.TriggerRandomEvent();
            if (evt == null)
            {
                Debug.LogWarning("[EventSystemExample] No eligible events to trigger");
            }
        }
        
        [ContextMenu("Force Eureka Moment")]
        private void DebugForceEurekaMoment()
        {
            _eventManager.TriggerEvent("eureka_moment");
        }
        
        [ContextMenu("Force Security Breach")]
        private void DebugForceSecurityBreach()
        {
            _eventManager.TriggerEvent("security_breach_attempt");
        }
        
        [ContextMenu("Print Active Effects")]
        private void DebugPrintActiveEffects()
        {
            var effects = _effectHandler.ActiveEffects;
            Debug.Log($"[EventSystemExample] Active effects: {effects.Count}");
            
            foreach (var effect in effects)
            {
                Debug.Log($"  - {effect.effectData.effectType}: {effect.effectData.value} (Remaining: {effect.remainingDurationMinutes:F1}m)");
            }
        }
        
        [ContextMenu("Print Timer Status")]
        private void DebugPrintTimerStatus()
        {
            var timer = _eventManager.EventTimer;
            Debug.Log($"[EventSystemExample] Timer Status:");
            Debug.Log($"  - Running: {timer.IsRunning}");
            Debug.Log($"  - Paused: {timer.IsPaused}");
            Debug.Log($"  - Remaining: {timer.GetFormattedRemainingTime()}");
            Debug.Log($"  - Progress: {timer.Progress:P0}");
        }
        
        [ContextMenu("Print Interaction Status")]
        private void DebugPrintInteractionStatus()
        {
            Debug.Log($"[EventSystemExample] Interaction Status:");
            Debug.Log($"  - Can Trigger Events: {_interactionTracker.CanTriggerEvents}");
            Debug.Log($"  - Seconds Since Last: {_interactionTracker.SecondsSinceLastInteraction:F1}s");
            Debug.Log($"  - Is Player Active: {_interactionTracker.IsPlayerActive}");
            Debug.Log($"  - Idle Progress: {_interactionTracker.IdleProgress:P0}");
        }
        
        #endregion
        
        private void Update()
        {
            // Example: Check for keyboard shortcuts in development
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugForceRandomEvent();
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                DebugForceEurekaMoment();
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                DebugPrintActiveEffects();
            }
            #endif
        }
    }
}
