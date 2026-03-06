using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Save.Data
{
    /// <summary>
    /// Events system save data
    /// </summary>
    [Serializable]
    public class EventsSaveData
    {
        // Time until next random event
        public float timeUntilNextEvent;
        
        // Active effects
        public List<ActiveEffectSaveData> activeEffects;
        
        // Event history
        public List<EventHistoryEntry> eventHistory;
        
        // Last interaction time (for offline calculations)
        public string lastInteractionTimeSerialized;
        
        // Event cooldowns
        public List<EventCooldownEntry> eventCooldowns;
        
        // Pending events (queued to show)
        public List<PendingEventSaveData> pendingEvents;
        
        // Statistics
        public int totalEventsTriggered;
        public int totalEventsCompleted;
        public int totalEventsIgnored;
        
        // Temporary DateTime field
        [NonSerialized]
        public DateTime lastInteractionTime;
        
        /// <summary>
        /// Creates default events data for new game
        /// </summary>
        public static EventsSaveData CreateDefault()
        {
            return new EventsSaveData
            {
                timeUntilNextEvent = 600f, // 10 minutes
                activeEffects = new List<ActiveEffectSaveData>(),
                eventHistory = new List<EventHistoryEntry>(),
                eventCooldowns = new List<EventCooldownEntry>(),
                pendingEvents = new List<PendingEventSaveData>(),
                totalEventsTriggered = 0,
                totalEventsCompleted = 0,
                totalEventsIgnored = 0,
                lastInteractionTime = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Serializes DateTime fields
        /// </summary>
        public void SerializeDateTimes()
        {
            lastInteractionTimeSerialized = SerializationHelper.DateTimeToString(lastInteractionTime);
            
            foreach (var effect in activeEffects)
            {
                effect?.SerializeDateTime();
            }
            
            foreach (var entry in eventHistory)
            {
                entry?.SerializeDateTime();
            }
            
            foreach (var cooldown in eventCooldowns)
            {
                cooldown?.SerializeDateTime();
            }
            
            foreach (var pending in pendingEvents)
            {
                pending?.SerializeDateTime();
            }
        }
        
        /// <summary>
        /// Deserializes DateTime fields
        /// </summary>
        public void DeserializeDateTimes()
        {
            lastInteractionTime = SerializationHelper.StringToDateTime(lastInteractionTimeSerialized);
            
            foreach (var effect in activeEffects)
            {
                effect?.DeserializeDateTime();
            }
            
            foreach (var entry in eventHistory)
            {
                entry?.DeserializeDateTime();
            }
            
            foreach (var cooldown in eventCooldowns)
            {
                cooldown?.DeserializeDateTime();
            }
            
            foreach (var pending in pendingEvents)
            {
                pending?.DeserializeDateTime();
            }
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            timeUntilNextEvent = Mathf.Max(0, timeUntilNextEvent);
            
            activeEffects ??= new List<ActiveEffectSaveData>();
            eventHistory ??= new List<EventHistoryEntry>();
            eventCooldowns ??= new List<EventCooldownEntry>();
            pendingEvents ??= new List<PendingEventSaveData>();
            
            totalEventsTriggered = Mathf.Max(0, totalEventsTriggered);
            totalEventsCompleted = Mathf.Max(0, totalEventsCompleted);
            totalEventsIgnored = Mathf.Max(0, totalEventsIgnored);
            
            // Validate active effects
            foreach (var effect in activeEffects)
            {
                effect?.Validate();
            }
            
            // Validate history
            foreach (var entry in eventHistory)
            {
                entry?.Validate();
            }
            
            // Validate cooldowns
            foreach (var cooldown in eventCooldowns)
            {
                cooldown?.Validate();
            }
            
            // Validate pending events
            foreach (var pending in pendingEvents)
            {
                pending?.Validate();
            }
            
            // Remove expired effects
            activeEffects.RemoveAll(e => e == null || e.IsExpired());
            
            // Remove expired cooldowns
            eventCooldowns.RemoveAll(c => c == null || c.IsExpired());
            
            // Remove invalid entries
            eventHistory.RemoveAll(h => h == null || string.IsNullOrEmpty(h.eventId));
            pendingEvents.RemoveAll(p => p == null || string.IsNullOrEmpty(p.eventId));
        }
        
        /// <summary>
        /// Adds an effect to active effects
        /// </summary>
        public void AddEffect(ActiveEffectSaveData effect)
        {
            if (effect == null || string.IsNullOrEmpty(effect.effectId))
                return;
                
            // Remove existing effect of same type if not stackable
            if (!effect.isStackable)
            {
                activeEffects.RemoveAll(e => e.effectType == effect.effectType);
            }
            
            activeEffects.Add(effect);
        }
        
        /// <summary>
        /// Gets total multiplier for an effect type
        /// </summary>
        public float GetEffectMultiplier(EffectType effectType)
        {
            float multiplier = 1f;
            foreach (var effect in activeEffects)
            {
                if (effect.effectType == effectType && !effect.IsExpired())
                {
                    multiplier *= effect.value;
                }
            }
            return multiplier;
        }
    }
    
    /// <summary>
    /// Effect type enum
    /// </summary>
    public enum EffectType
    {
        MoneyMultiplier,
        ReputationMultiplier,
        ResearchSpeedMultiplier,
        ProductionSpeedMultiplier,
        ContractValueMultiplier,
        MarketPriceMultiplier,
        EventFrequencyMultiplier,
        CostReduction,
        QualityBoost,
        None
    }
    
    /// <summary>
    /// Active effect save data
    /// </summary>
    [Serializable]
    public class ActiveEffectSaveData
    {
        public string effectId;
        public EffectType effectType;
        public float value;
        public string expirationTimeSerialized;
        public bool isStackable;
        public string sourceEventId;
        public string displayName;
        public string description;
        
        [NonSerialized]
        public DateTime expirationTime;
        
        public void SerializeDateTime()
        {
            expirationTimeSerialized = SerializationHelper.DateTimeToString(expirationTime);
        }
        
        public void DeserializeDateTime()
        {
            expirationTime = SerializationHelper.StringToDateTime(expirationTimeSerialized);
        }
        
        public void Validate()
        {
            value = Mathf.Max(0, value);
            
            if (!Enum.IsDefined(typeof(EffectType), effectType))
            {
                effectType = EffectType.None;
            }
            
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = effectId ?? "Unknown Effect";
            }
        }
        
        public bool IsExpired()
        {
            return DateTime.UtcNow >= expirationTime;
        }
        
        public TimeSpan GetTimeRemaining()
        {
            return expirationTime - DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Event history entry
    /// </summary>
    [Serializable]
    public class EventHistoryEntry
    {
        public string eventId;
        public string eventName;
        public string timestampSerialized;
        public EventOutcome outcome;
        public float moneyChange;
        public float reputationChange;
        
        [NonSerialized]
        public DateTime timestamp;
        
        public void SerializeDateTime()
        {
            timestampSerialized = SerializationHelper.DateTimeToString(timestamp);
        }
        
        public void DeserializeDateTime()
        {
            timestamp = SerializationHelper.StringToDateTime(timestampSerialized);
        }
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(eventName))
            {
                eventName = eventId ?? "Unknown Event";
            }
        }
    }
    
    /// <summary>
    /// Event outcome enum
    /// </summary>
    public enum EventOutcome
    {
        Completed,
        Failed,
        Ignored,
        Expired
    }
    
    /// <summary>
    /// Event cooldown entry
    /// </summary>
    [Serializable]
    public class EventCooldownEntry
    {
        public string eventId;
        public string expirationTimeSerialized;
        
        [NonSerialized]
        public DateTime expirationTime;
        
        public void SerializeDateTime()
        {
            expirationTimeSerialized = SerializationHelper.DateTimeToString(expirationTime);
        }
        
        public void DeserializeDateTime()
        {
            expirationTime = SerializationHelper.StringToDateTime(expirationTimeSerialized);
        }
        
        public void Validate()
        {
            // Nothing to validate
        }
        
        public bool IsExpired()
        {
            return DateTime.UtcNow >= expirationTime;
        }
    }
    
    /// <summary>
    /// Pending event save data
    /// </summary>
    [Serializable]
    public class PendingEventSaveData
    {
        public string eventId;
        public string triggerTimeSerialized;
        public bool isUrgent;
        public bool hasBeenViewed;
        
        [NonSerialized]
        public DateTime triggerTime;
        
        public void SerializeDateTime()
        {
            triggerTimeSerialized = SerializationHelper.DateTimeToString(triggerTime);
        }
        
        public void DeserializeDateTime()
        {
            triggerTime = SerializationHelper.StringToDateTime(triggerTimeSerialized);
        }
        
        public void Validate()
        {
            // Nothing to validate
        }
        
        public bool ShouldTrigger()
        {
            return DateTime.UtcNow >= triggerTime;
        }
    }
}
