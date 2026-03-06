using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Events
{
    /// <summary>
    /// Categories of events that can occur in the game
    /// </summary>
    public enum EventType
    {
        ResearchBreakthrough,     // Reduces current research time
        FundingBoost,             // Temporary production speed increase
        SecurityAlert,            // Decision event - choose response
        PrototypeOptimization,    // Improves test success chances
        MarketOpportunity,        // Temporary contract bonus
        EquipmentMalfunction,     // Negative event - setbacks
        StaffInsight,             // Employee-related bonus
        RegulatoryChange,         // Policy/decision event
        CompetitorAction,         // Market-related decision
        RandomDiscovery,          // Unexpected positive finding
    }

    /// <summary>
    /// Rarity tiers for event weighting
    /// </summary>
    public enum EventRarity
    {
        Common,      // Weight: 50
        Uncommon,    // Weight: 30
        Rare,        // Weight: 15
        Epic,        // Weight: 4
        Legendary    // Weight: 1
    }

    /// <summary>
    /// A choice presented to the player in decision events
    /// </summary>
    [Serializable]
    public class EventChoice
    {
        [Tooltip("Unique identifier for this choice")]
        public string choiceId;
        
        [Tooltip("Display text for the choice button")]
        public string displayText;
        
        [Tooltip("Description of what this choice does")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("Icon for this choice")]
        public Sprite icon;
        
        [Tooltip("Effects applied when this choice is selected")]
        public List<EventEffect> outcomes;
        
        [Tooltip("Prerequisites for this choice to be available")]
        public List<ChoicePrerequisite> prerequisites;

        /// <summary>
        /// Checks if this choice is available given current game state
        /// </summary>
        public bool IsAvailable()
        {
            if (prerequisites == null || prerequisites.Count == 0)
                return true;

            foreach (var prereq in prerequisites)
            {
                if (!prereq.IsMet())
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Prerequisite condition for an event choice
    /// </summary>
    [Serializable]
    public class ChoicePrerequisite
    {
        public PrerequisiteType type;
        public string targetId;
        public float minimumValue;

        public bool IsMet()
        {
            // Implementation would check against game state
            // This is a placeholder for actual game integration
            return true;
        }
    }

    public enum PrerequisiteType
    {
        MinimumMoney,
        MinimumReputation,
        TechUnlocked,
        BuildingConstructed,
        StaffCount,
        ResearchCompleted
    }

    /// <summary>
    /// ScriptableObject defining a game event
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Project Aegis/Game Event", order = 1)]
    public class GameEventData : ScriptableObject
    {
        [Header("Event Identity")]
        [Tooltip("Unique identifier for this event")]
        public string eventId;
        
        [Tooltip("Display name shown to player")]
        public string displayName;
        
        [Tooltip("Description of the event")]
        [TextArea(3, 6)]
        public string description;
        
        [Tooltip("Short notification text")]
        public string notificationText;
        
        [Tooltip("Event category")]
        public EventType eventType;
        
        [Tooltip("Event rarity (affects weight)")]
        public EventRarity rarity = EventRarity.Common;
        
        [Tooltip("Event icon")]
        public Sprite icon;

        [Header("Timing Settings")]
        [Tooltip("Minimum minutes before this event can trigger")]
        [Range(1f, 120f)]
        public float minIntervalMinutes = 15f;
        
        [Tooltip("Maximum minutes before this event can trigger")]
        [Range(1f, 120f)]
        public float maxIntervalMinutes = 20f;
        
        [Tooltip("Cooldown after this event triggers (prevents immediate re-trigger)")]
        public float cooldownMinutes = 30f;
        
        [Tooltip("Game session minimum minutes before this event can occur")]
        public float minSessionTimeMinutes = 0f;

        [Header("Selection Weight")]
        [Tooltip("Base weight for random selection (higher = more likely)")]
        [Range(0.1f, 100f)]
        public float weight = 10f;
        
        [Tooltip("Whether weight scales with game progression")]
        public bool scaleWeightWithProgress = false;

        [Header("Event Effects")]
        [Tooltip("Effects applied automatically when event triggers")]
        public List<EventEffect> effects;
        
        [Tooltip("Whether this event requires player decision")]
        public bool isDecisionEvent = false;
        
        [Tooltip("Choices available for decision events")]
        public List<EventChoice> choices;

        [Header("Visual & Audio")]
        [Tooltip("Background image for event popup")]
        public Sprite backgroundImage;
        
        [Tooltip("Sound effect to play")]
        public AudioClip soundEffect;
        
        [Tooltip("Particle effect prefab")]
        public GameObject particleEffectPrefab;

        [Header("Conditions")]
        [Tooltip("Minimum game stage required")]
        public int minGameStage = 0;
        
        [Tooltip("Required technologies")]
        public List<string> requiredTechIds;
        
        [Tooltip("Events that must have occurred first")]
        public List<string> prerequisiteEventIds;
        
        [Tooltip("Maximum times this event can trigger (0 = unlimited)")]
        public int maxTriggers = 0;

        // Runtime tracking
        [System.NonSerialized]
        public int triggerCount = 0;
        
        [System.NonSerialized]
        public DateTime lastTriggeredAt;
        
        [System.NonSerialized]
        public bool isOnCooldown = false;

        /// <summary>
        /// Gets the effective weight for random selection
        /// </summary>
        public float GetEffectiveWeight()
        {
            float effectiveWeight = weight;
            
            // Apply rarity multiplier
            effectiveWeight *= GetRarityMultiplier();
            
            return effectiveWeight;
        }

        private float GetRarityMultiplier()
        {
            return rarity switch
            {
                EventRarity.Common => 1.0f,
                EventRarity.Uncommon => 0.6f,
                EventRarity.Rare => 0.3f,
                EventRarity.Epic => 0.08f,
                EventRarity.Legendary => 0.02f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Checks if this event can trigger based on conditions
        /// </summary>
        public bool CanTrigger(float sessionTimeMinutes)
        {
            // Check max triggers
            if (maxTriggers > 0 && triggerCount >= maxTriggers)
                return false;

            // Check cooldown
            if (isOnCooldown)
            {
                double minutesSinceLastTrigger = DateTime.Now.Subtract(lastTriggeredAt).TotalMinutes;
                if (minutesSinceLastTrigger < cooldownMinutes)
                    return false;
                isOnCooldown = false;
            }

            // Check minimum session time
            if (sessionTimeMinutes < minSessionTimeMinutes)
                return false;

            // Check prerequisites would go here
            // (requires integration with game systems)

            return true;
        }

        /// <summary>
        /// Records that this event has triggered
        /// </summary>
        public void OnTriggered()
        {
            triggerCount++;
            lastTriggeredAt = DateTime.Now;
            isOnCooldown = true;
        }

        /// <summary>
        /// Gets a random interval for this event type
        /// </summary>
        public float GetRandomInterval()
        {
            return UnityEngine.Random.Range(minIntervalMinutes, maxIntervalMinutes);
        }

        /// <summary>
        /// Resets runtime tracking data
        /// </summary>
        public void ResetTracking()
        {
            triggerCount = 0;
            isOnCooldown = false;
        }

        private void OnValidate()
        {
            // Ensure eventId is set
            if (string.IsNullOrEmpty(eventId))
            {
                eventId = Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            // Validate intervals
            if (minIntervalMinutes > maxIntervalMinutes)
            {
                maxIntervalMinutes = minIntervalMinutes;
            }

            // Validate decision events have choices
            if (isDecisionEvent && (choices == null || choices.Count == 0))
            {
                Debug.LogWarning($"Decision event '{displayName}' has no choices defined!");
            }
        }
    }
}
