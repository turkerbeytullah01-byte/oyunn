using System;
using UnityEngine;

namespace ProjectAegis.Systems.Events
{
    /// <summary>
    /// Types of effects that events can apply to the game
    /// </summary>
    public enum EffectType
    {
        ResearchTimeReduction,    // Reduces remaining research time
        ProductionSpeedBoost,     // Increases production speed temporarily
        MoneyBonus,               // Instant money reward
        ReputationChange,         // Affects company reputation
        RiskReduction,            // Reduces project risk
        TestSuccessBoost,         // Increases prototype test success chance
        ResearchSpeedBoost,       // Increases research speed
        ContractValueBoost,       // Increases contract rewards
        MaintenanceCostReduction, // Reduces facility maintenance costs
        UnlockTech,               // Unlocks a technology temporarily
    }

    /// <summary>
    /// Defines a single effect that an event can apply
    /// </summary>
    [Serializable]
    public class EventEffect
    {
        [Tooltip("Type of effect to apply")]
        public EffectType effectType;
        
        [Tooltip("Magnitude of the effect (interpretation depends on effect type)")]
        public float value;
        
        [Tooltip("Duration in minutes (0 = instant/permanent)")]
        public float durationMinutes;
        
        [Tooltip("Optional target ID for specific systems/entities")]
        public string targetId = "";
        
        [Tooltip("Whether this effect can stack with similar effects")]
        public bool canStack = true;
        
        [Tooltip("Maximum stack count (0 = unlimited)")]
        public int maxStackCount = 0;

        /// <summary>
        /// Creates a copy of this effect
        /// </summary>
        public EventEffect Clone()
        {
            return new EventEffect
            {
                effectType = this.effectType,
                value = this.value,
                durationMinutes = this.durationMinutes,
                targetId = this.targetId,
                canStack = this.canStack,
                maxStackCount = this.maxStackCount
            };
        }
    }

    /// <summary>
    /// Represents an active effect instance with runtime data
    /// </summary>
    [Serializable]
    public class ActiveEventEffect
    {
        public string effectId;
        public string sourceEventId;
        public EventEffect effectData;
        public float remainingDurationMinutes;
        public float totalDurationMinutes;
        public DateTime appliedAt;
        public int stackCount = 1;

        public bool IsExpired => remainingDurationMinutes <= 0;
        public float Progress => totalDurationMinutes > 0 ? 1f - (remainingDurationMinutes / totalDurationMinutes) : 0f;
        public float TimeRemainingSeconds => remainingDurationMinutes * 60f;

        public ActiveEventEffect(string sourceEventId, EventEffect effect)
        {
            this.effectId = Guid.NewGuid().ToString("N").Substring(0, 8);
            this.sourceEventId = sourceEventId;
            this.effectData = effect.Clone();
            this.remainingDurationMinutes = effect.durationMinutes;
            this.totalDurationMinutes = effect.durationMinutes;
            this.appliedAt = DateTime.Now;
        }

        /// <summary>
        /// Updates the effect timer
        /// </summary>
        public void Update(float deltaTimeMinutes)
        {
            if (remainingDurationMinutes > 0)
            {
                remainingDurationMinutes -= deltaTimeMinutes;
                if (remainingDurationMinutes < 0)
                    remainingDurationMinutes = 0;
            }
        }

        /// <summary>
        /// Attempts to stack this effect with another of the same type
        /// </summary>
        public bool TryStack(ActiveEventEffect other)
        {
            if (!effectData.canStack)
                return false;

            if (effectData.maxStackCount > 0 && stackCount >= effectData.maxStackCount)
                return false;

            if (effectData.effectType != other.effectData.effectType)
                return false;

            // Stack by refreshing duration and incrementing stack count
            remainingDurationMinutes = Mathf.Max(remainingDurationMinutes, other.remainingDurationMinutes);
            stackCount++;
            return true;
        }
    }
}
