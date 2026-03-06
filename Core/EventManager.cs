// ============================================================================
// Project Aegis: Drone Dominion
// EventManager - Central event system for decoupled communication
// ============================================================================
// Provides a type-safe, centralized event system for all game components.
// Uses C# events with proper null checking and error handling.
// Supports both strongly-typed and generic event patterns.
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectAegis.Core
{
    /// <summary>
    /// Central event manager for all game events.
    /// Provides decoupled communication between systems.
    /// </summary>
    public class EventManager : BaseManager<EventManager>
    {
        #region Configuration
        
        public override int InitializationPriority => 0; // Initialize first
        
        /// <summary>
        /// Maximum events to queue before processing.
        /// </summary>
        [SerializeField, Range(10, 1000)]
        private int _maxEventQueueSize = 100;
        
        /// <summary>
        /// Whether to log all events for debugging.
        /// </summary>
        [SerializeField]
        private bool _logEvents = false;
        
        #endregion
        
        #region Core Game Events
        
        // ============================================
        // RESOURCE EVENTS
        // ============================================
        
        /// <summary>
        /// Triggered when player's money changes.
        /// Parameters: (long newAmount, long changeAmount, string reason)
        /// </summary>
        public event Action<long, long, string> OnMoneyChanged;
        
        /// <summary>
        /// Triggered when player's reputation changes.
        /// Parameters: (int newAmount, int changeAmount, string reason)
        /// </summary>
        public event Action<int, int, string> OnReputationChanged;
        
        /// <summary>
        /// Triggered when research points change.
        /// Parameters: (long newAmount, long changeAmount, string reason)
        /// </summary>
        public event Action<long, long, string> OnResearchPointsChanged;
        
        /// <summary>
        /// Triggered when energy changes.
        /// Parameters: (int newAmount, int changeAmount, string reason)
        /// </summary>
        public event Action<int, int, string> OnEnergyChanged;
        
        /// <summary>
        /// Triggered when materials change.
        /// Parameters: (int newAmount, int changeAmount, string reason)
        /// </summary>
        public event Action<int, int, string> OnMaterialsChanged;
        
        // ============================================
        // RESEARCH EVENTS
        // ============================================
        
        /// <summary>
        /// Triggered when research is started.
        /// Parameters: (string researchId, string researchName, float duration)
        /// </summary>
        public event Action<string, string, float> OnResearchStarted;
        
        /// <summary>
        /// Triggered when research progress updates.
        /// Parameters: (string researchId, float normalizedProgress, float timeRemaining)
        /// </summary>
        public event Action<string, float, float> OnResearchProgress;
        
        /// <summary>
        /// Triggered when research is completed.
        /// Parameters: (string researchId, string researchName)
        /// </summary>
        public event Action<string, string> OnResearchCompleted;
        
        /// <summary>
        /// Triggered when research is cancelled.
        /// Parameters: (string researchId, string researchName, float progressLost)
        /// </summary>
        public event Action<string, string, float> OnResearchCancelled;
        
        /// <summary>
        /// Triggered when a technology is unlocked.
        /// Parameters: (string techId, string techName)
        /// </summary>
        public event Action<string, string> OnTechnologyUnlocked;
        
        // ============================================
        // DRONE EVENTS
        // ============================================
        
        /// <summary>
        /// Triggered when a drone is unlocked.
        /// Parameters: (string droneId, string droneName)
        /// </summary>
        public event Action<string, string> OnDroneUnlocked;
        
        /// <summary>
        /// Triggered when a drone is produced.
        /// Parameters: (string droneId, string droneName, int quantity)
        /// </summary>
        public event Action<string, string, int> OnDroneProduced;
        
        /// <summary>
        /// Triggered when a drone is sold.
        /// Parameters: (string droneId, string droneName, int quantity, long revenue)
        /// </summary>
        public event Action<string, string, int, long> OnDroneSold;
        
        /// <summary>
        /// Triggered when a prototype test starts.
        /// Parameters: (string droneId, string testType)
        /// </summary>
        public event Action<string, string> OnPrototypeTestStarted;
        
        /// <summary>
        /// Triggered when a prototype test completes.
        /// Parameters: (string droneId, string testType, bool success, float score)
        /// </summary>
        public event Action<string, string, bool, float> OnPrototypeTestCompleted;
        
        // ============================================
        // CONTRACT EVENTS
        // ============================================
        
        /// <summary>
        /// Triggered when a new contract becomes available.
        /// Parameters: (string contractId, string contractName, long reward)
        /// </summary>
        public event Action<string, string, long> OnContractAvailable;
        
        /// <summary>
        /// Triggered when a contract is bid on.
        /// Parameters: (string contractId, string contractName, long bidAmount)
        /// </summary>
        public event Action<string, string, long> OnContractBid;
        
        /// <summary>
        /// Triggered when a contract bid is won.
        /// Parameters: (string contractId, string contractName, long reward)
        /// </summary>
        public event Action<string, string, long> OnContractWon;
        
        /// <summary>
        /// Triggered when a contract bid is lost.
        /// Parameters: (string contractId, string contractName)
        /// </summary>
        public event Action<string, string> OnContractLost;
        
        /// <summary>
        /// Triggered when a contract delivery is made.
        /// Parameters: (string contractId, int dronesDelivered, int dronesRequired)
        /// </summary>
        public event Action<string, int, int> OnContractDelivery;
        
        /// <summary>
        /// Triggered when a contract is completed.
        /// Parameters: (string contractId, string contractName, long reward, int reputationGain)
        /// </summary>
        public event Action<string, string, long, int> OnContractCompleted;
        
        /// <summary>
        /// Triggered when a contract fails.
        /// Parameters: (string contractId, string contractName, string reason)
        /// </summary>
        public event Action<string, string, string> OnContractFailed;
        
        // ============================================
        // IDLE GENERATION EVENTS
        // ============================================
        
        /// <summary>
        /// Triggered when offline progress is calculated.
        /// Parameters: (float offlineSeconds, long moneyEarned, long researchPointsEarned)
        /// </summary>
        public event Action<float, long, long> OnOfflineProgressCalculated;
        
        /// <summary>
        /// Triggered when a production line is started.
        /// Parameters: (string lineId, string droneId)
        /// </summary>
        public event Action<string, string> OnProductionLineStarted;
        
        /// <summary>
        /// Triggered when production completes.
        /// Parameters: (string lineId, string droneId, int quantity)
        /// </summary>
        public event Action<string, string, int> OnProductionCompleted;
        
        // ============================================
        // RISK EVENTS
        // ============================================
        
        /// <summary>
        /// Triggered when a risk event occurs.
        /// Parameters: (string eventId, string eventName, RiskLevel level)
        /// </summary>
        public event Action<string, string, RiskLevel> OnRiskEventTriggered;
        
        /// <summary>
        /// Triggered when a risk event is resolved.
        /// Parameters: (string eventId, string resolution)
        /// </summary>
        public event Action<string, string> OnRiskEventResolved;
        
        /// <summary>
        /// Triggered when reputation risk increases.
        /// Parameters: (float newRiskLevel)
        /// </summary>
        public event Action<float> OnReputationRiskChanged;
        
        /// <summary>
        /// Triggered when financial risk increases.
        /// Parameters: (float newRiskLevel)
        /// </summary>
        public event Action<float> OnFinancialRiskChanged;
        
        // ============================================
        // GAME STATE EVENTS
        // ============================================
        
        /// <summary>
        /// Triggered when the game state changes.
        /// Parameters: (GameState oldState, GameState newState)
        /// </summary>
        public event Action<GameState, GameState> OnGameStateChanged;
        
        /// <summary>
        /// Triggered when the game is saved.
        /// Parameters: (string saveId, DateTime timestamp)
        /// </summary>
        public event Action<string, DateTime> OnGameSaved;
        
        /// <summary>
        /// Triggered when the game is loaded.
        /// Parameters: (string saveId, DateTime timestamp)
        /// </summary>
        public event Action<string, DateTime> OnGameLoaded;
        
        /// <summary>
        /// Triggered when a milestone is reached.
        /// Parameters: (string milestoneId, string milestoneName)
        /// </summary>
        public event Action<string, string> OnMilestoneReached;
        
        /// <summary>
        /// Triggered when an achievement is unlocked.
        /// Parameters: (string achievementId, string achievementName)
        /// </summary>
        public event Action<string, string> OnAchievementUnlocked;
        
        // ============================================
        // UI EVENTS
        // ============================================
        
        /// <summary>
        /// Triggered when a notification should be shown.
        /// Parameters: (string title, string message, NotificationType type)
        /// </summary>
        public event Action<string, string, NotificationType> OnNotificationRequested;
        
        /// <summary>
        /// Triggered when a panel should be opened.
        /// Parameters: (string panelId)
        /// </summary>
        public event Action<string> OnPanelOpenRequested;
        
        /// <summary>
        /// Triggered when a panel should be closed.
        /// Parameters: (string panelId)
        /// </summary>
        public event Action<string> OnPanelCloseRequested;
        
        #endregion
        
        #region Generic Event System
        
        /// <summary>
        /// Dictionary for storing generic events by type.
        /// </summary>
        private Dictionary<Type, Delegate> _genericEvents = new Dictionary<Type, Delegate>();
        
        /// <summary>
        /// Subscribes to a generic event of type T.
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_genericEvents.TryGetValue(type, out var existing))
            {
                _genericEvents[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                _genericEvents[type] = handler;
            }
        }
        
        /// <summary>
        /// Unsubscribes from a generic event of type T.
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_genericEvents.TryGetValue(type, out var existing))
            {
                _genericEvents[type] = Delegate.Remove(existing, handler);
            }
        }
        
        /// <summary>
        /// Triggers a generic event of type T.
        /// </summary>
        public void Trigger<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (_genericEvents.TryGetValue(type, out var handler))
            {
                ((Action<T>)handler)?.Invoke(eventData);
            }
        }
        
        #endregion
        
        #region Event Invocation Methods
        
        // ============================================
        // RESOURCE EVENT TRIGGERS
        // ============================================
        
        public void TriggerMoneyChanged(long newAmount, long changeAmount, string reason)
        {
            SafeInvoke(OnMoneyChanged, newAmount, changeAmount, reason, "MoneyChanged");
        }
        
        public void TriggerReputationChanged(int newAmount, int changeAmount, string reason)
        {
            SafeInvoke(OnReputationChanged, newAmount, changeAmount, reason, "ReputationChanged");
        }
        
        public void TriggerResearchPointsChanged(long newAmount, long changeAmount, string reason)
        {
            SafeInvoke(OnResearchPointsChanged, newAmount, changeAmount, reason, "ResearchPointsChanged");
        }
        
        public void TriggerEnergyChanged(int newAmount, int changeAmount, string reason)
        {
            SafeInvoke(OnEnergyChanged, newAmount, changeAmount, reason, "EnergyChanged");
        }
        
        public void TriggerMaterialsChanged(int newAmount, int changeAmount, string reason)
        {
            SafeInvoke(OnMaterialsChanged, newAmount, changeAmount, reason, "MaterialsChanged");
        }
        
        // ============================================
        // RESEARCH EVENT TRIGGERS
        // ============================================
        
        public void TriggerResearchStarted(string researchId, string researchName, float duration)
        {
            SafeInvoke(OnResearchStarted, researchId, researchName, duration, "ResearchStarted");
        }
        
        public void TriggerResearchProgress(string researchId, float normalizedProgress, float timeRemaining)
        {
            SafeInvoke(OnResearchProgress, researchId, normalizedProgress, timeRemaining, "ResearchProgress");
        }
        
        public void TriggerResearchCompleted(string researchId, string researchName)
        {
            SafeInvoke(OnResearchCompleted, researchId, researchName, "ResearchCompleted");
        }
        
        public void TriggerResearchCancelled(string researchId, string researchName, float progressLost)
        {
            SafeInvoke(OnResearchCancelled, researchId, researchName, progressLost, "ResearchCancelled");
        }
        
        public void TriggerTechnologyUnlocked(string techId, string techName)
        {
            SafeInvoke(OnTechnologyUnlocked, techId, techName, "TechnologyUnlocked");
        }
        
        // ============================================
        // DRONE EVENT TRIGGERS
        // ============================================
        
        public void TriggerDroneUnlocked(string droneId, string droneName)
        {
            SafeInvoke(OnDroneUnlocked, droneId, droneName, "DroneUnlocked");
        }
        
        public void TriggerDroneProduced(string droneId, string droneName, int quantity)
        {
            SafeInvoke(OnDroneProduced, droneId, droneName, quantity, "DroneProduced");
        }
        
        public void TriggerDroneSold(string droneId, string droneName, int quantity, long revenue)
        {
            SafeInvoke(OnDroneSold, droneId, droneName, quantity, revenue, "DroneSold");
        }
        
        public void TriggerPrototypeTestStarted(string droneId, string testType)
        {
            SafeInvoke(OnPrototypeTestStarted, droneId, testType, "PrototypeTestStarted");
        }
        
        public void TriggerPrototypeTestCompleted(string droneId, string testType, bool success, float score)
        {
            SafeInvoke(OnPrototypeTestCompleted, droneId, testType, success, score, "PrototypeTestCompleted");
        }
        
        // ============================================
        // CONTRACT EVENT TRIGGERS
        // ============================================
        
        public void TriggerContractAvailable(string contractId, string contractName, long reward)
        {
            SafeInvoke(OnContractAvailable, contractId, contractName, reward, "ContractAvailable");
        }
        
        public void TriggerContractBid(string contractId, string contractName, long bidAmount)
        {
            SafeInvoke(OnContractBid, contractId, contractName, bidAmount, "ContractBid");
        }
        
        public void TriggerContractWon(string contractId, string contractName, long reward)
        {
            SafeInvoke(OnContractWon, contractId, contractName, reward, "ContractWon");
        }
        
        public void TriggerContractLost(string contractId, string contractName)
        {
            SafeInvoke(OnContractLost, contractId, contractName, "ContractLost");
        }
        
        public void TriggerContractDelivery(string contractId, int dronesDelivered, int dronesRequired)
        {
            SafeInvoke(OnContractDelivery, contractId, dronesDelivered, dronesRequired, "ContractDelivery");
        }
        
        public void TriggerContractCompleted(string contractId, string contractName, long reward, int reputationGain)
        {
            SafeInvoke(OnContractCompleted, contractId, contractName, reward, reputationGain, "ContractCompleted");
        }
        
        public void TriggerContractFailed(string contractId, string contractName, string reason)
        {
            SafeInvoke(OnContractFailed, contractId, contractName, reason, "ContractFailed");
        }
        
        // ============================================
        // IDLE GENERATION EVENT TRIGGERS
        // ============================================
        
        public void TriggerOfflineProgressCalculated(float offlineSeconds, long moneyEarned, long researchPointsEarned)
        {
            SafeInvoke(OnOfflineProgressCalculated, offlineSeconds, moneyEarned, researchPointsEarned, "OfflineProgressCalculated");
        }
        
        public void TriggerProductionLineStarted(string lineId, string droneId)
        {
            SafeInvoke(OnProductionLineStarted, lineId, droneId, "ProductionLineStarted");
        }
        
        public void TriggerProductionCompleted(string lineId, string droneId, int quantity)
        {
            SafeInvoke(OnProductionCompleted, lineId, droneId, quantity, "ProductionCompleted");
        }
        
        // ============================================
        // RISK EVENT TRIGGERS
        // ============================================
        
        public void TriggerRiskEvent(string eventId, string eventName, RiskLevel level)
        {
            SafeInvoke(OnRiskEventTriggered, eventId, eventName, level, "RiskEventTriggered");
        }
        
        public void TriggerRiskEventResolved(string eventId, string resolution)
        {
            SafeInvoke(OnRiskEventResolved, eventId, resolution, "RiskEventResolved");
        }
        
        public void TriggerReputationRiskChanged(float newRiskLevel)
        {
            SafeInvoke(OnReputationRiskChanged, newRiskLevel, "ReputationRiskChanged");
        }
        
        public void TriggerFinancialRiskChanged(float newRiskLevel)
        {
            SafeInvoke(OnFinancialRiskChanged, newRiskLevel, "FinancialRiskChanged");
        }
        
        // ============================================
        // GAME STATE EVENT TRIGGERS
        // ============================================
        
        public void TriggerGameStateChanged(GameState oldState, GameState newState)
        {
            SafeInvoke(OnGameStateChanged, oldState, newState, "GameStateChanged");
        }
        
        public void TriggerGameSaved(string saveId, DateTime timestamp)
        {
            SafeInvoke(OnGameSaved, saveId, timestamp, "GameSaved");
        }
        
        public void TriggerGameLoaded(string saveId, DateTime timestamp)
        {
            SafeInvoke(OnGameLoaded, saveId, timestamp, "GameLoaded");
        }
        
        public void TriggerMilestoneReached(string milestoneId, string milestoneName)
        {
            SafeInvoke(OnMilestoneReached, milestoneId, milestoneName, "MilestoneReached");
        }
        
        public void TriggerAchievementUnlocked(string achievementId, string achievementName)
        {
            SafeInvoke(OnAchievementUnlocked, achievementId, achievementName, "AchievementUnlocked");
        }
        
        // ============================================
        // UI EVENT TRIGGERS
        // ============================================
        
        public void TriggerNotification(string title, string message, NotificationType type)
        {
            SafeInvoke(OnNotificationRequested, title, message, type, "NotificationRequested");
        }
        
        public void TriggerPanelOpen(string panelId)
        {
            SafeInvoke(OnPanelOpenRequested, panelId, "PanelOpenRequested");
        }
        
        public void TriggerPanelClose(string panelId)
        {
            SafeInvoke(OnPanelCloseRequested, panelId, "PanelCloseRequested");
        }
        
        #endregion
        
        #region Safe Invocation
        
        /// <summary>
        /// Safely invokes an event with null checking and error handling.
        /// </summary>
        private void SafeInvoke<T1, T2>(Action<T1, T2> evt, T1 arg1, T2 arg2, string eventName)
        {
            if (evt == null) return;
            
            if (_logEvents)
            {
                Log($"Event: {eventName}({arg1}, {arg2})");
            }
            
            try
            {
                evt.Invoke(arg1, arg2);
            }
            catch (Exception ex)
            {
                LogError($"Error in event handler for {eventName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Safely invokes an event with 3 parameters.
        /// </summary>
        private void SafeInvoke<T1, T2, T3>(Action<T1, T2, T3> evt, T1 arg1, T2 arg2, T3 arg3, string eventName)
        {
            if (evt == null) return;
            
            if (_logEvents)
            {
                Log($"Event: {eventName}({arg1}, {arg2}, {arg3})");
            }
            
            try
            {
                evt.Invoke(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                LogError($"Error in event handler for {eventName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Safely invokes an event with 4 parameters.
        /// </summary>
        private void SafeInvoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> evt, T1 arg1, T2 arg2, T3 arg3, T4 arg4, string eventName)
        {
            if (evt == null) return;
            
            if (_logEvents)
            {
                Log($"Event: {eventName}({arg1}, {arg2}, {arg3}, {arg4})");
            }
            
            try
            {
                evt.Invoke(arg1, arg2, arg3, arg4);
            }
            catch (Exception ex)
            {
                LogError($"Error in event handler for {eventName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Safely invokes an event with 5 parameters.
        /// </summary>
        private void SafeInvoke<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> evt, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, string eventName)
        {
            if (evt == null) return;
            
            if (_logEvents)
            {
                Log($"Event: {eventName}({arg1}, {arg2}, {arg3}, {arg4}, {arg5})");
            }
            
            try
            {
                evt.Invoke(arg1, arg2, arg3, arg4, arg5);
            }
            catch (Exception ex)
            {
                LogError($"Error in event handler for {eventName}: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Initialization
        
        protected override void OnInitialize()
        {
            // EventManager doesn't need special initialization
            Log("EventManager initialized");
        }
        
        #endregion
    }
    
    #region Enums
    
    /// <summary>
    /// Risk levels for events and situations.
    /// </summary>
    public enum RiskLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    
    /// <summary>
    /// Types of notifications that can be displayed.
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Achievement
    }
    
    #endregion
}
