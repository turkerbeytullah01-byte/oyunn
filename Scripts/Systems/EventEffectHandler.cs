using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Events
{
    /// <summary>
    /// Handles application, tracking, and removal of event effects.
    /// Manages effect stacking and provides multipliers to game systems.
    /// </summary>
    public class EventEffectHandler : MonoBehaviour
    {
        #region Singleton
        
        private static EventEffectHandler _instance;
        public static EventEffectHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EventEffectHandler>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EventEffectHandler");
                        _instance = go.AddComponent<EventEffectHandler>();
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
        
        [Header("Settings")]
        [Tooltip("Update interval for effect processing (seconds)")]
        [SerializeField]
        private float _updateInterval = 1f;
        
        [Tooltip("Maximum number of stacked effects of the same type")]
        [SerializeField]
        private int _maxGlobalStackCount = 5;
        
        [Header("Debug")]
        [SerializeField]
        private bool _showDebugLogs = false;
        
        private List<ActiveEventEffect> _activeEffects = new List<ActiveEventEffect>();
        private float _updateTimer;
        private Dictionary<EffectType, float> _effectMultiplierCache = new Dictionary<EffectType, float>();
        private bool _cacheDirty = true;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// All currently active effects
        /// </summary>
        public IReadOnlyList<ActiveEventEffect> ActiveEffects => _activeEffects.AsReadOnly();
        
        /// <summary>
        /// Number of currently active effects
        /// </summary>
        public int ActiveEffectCount => _activeEffects.Count;
        
        /// <summary>
        /// Whether there are any active effects
        /// </summary>
        public bool HasActiveEffects => _activeEffects.Count > 0;

        #endregion

        #region Events
        
        /// <summary>
        /// Called when an effect is applied
        /// </summary>
        public event Action<ActiveEventEffect> OnEffectApplied;
        
        /// <summary>
        /// Called when an effect expires
        /// </summary>
        public event Action<ActiveEventEffect> OnEffectExpired;
        
        /// <summary>
        /// Called when an effect is removed manually
        /// </summary>
        public event Action<ActiveEventEffect> OnEffectRemoved;
        
        /// <summary>
        /// Called when effects are stacked
        /// </summary>
        public event Action<ActiveEventEffect, ActiveEventEffect> OnEffectsStacked;
        
        /// <summary>
        /// Called when any effect changes (applied/expired/stacked)
        /// </summary>
        public event Action OnEffectsChanged;

        #endregion

        #region Lifecycle
        
        private void Update()
        {
            _updateTimer += Time.deltaTime;
            
            if (_updateTimer >= _updateInterval)
            {
                ProcessEffects(_updateTimer / 60f); // Convert to minutes
                _updateTimer = 0f;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                // Calculate elapsed time while app was paused
                // In a real implementation, this would use timestamp comparison
                ProcessEffects(0);
            }
        }
        
        #endregion

        #region Public Methods - Effect Application
        
        /// <summary>
        /// Applies an effect from an event
        /// </summary>
        public ActiveEventEffect ApplyEffect(string sourceEventId, EventEffect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("[EventEffectHandler] Cannot apply null effect");
                return null;
            }
            
            // Check for existing stackable effect
            if (effect.canStack)
            {
                var existingEffect = FindStackableEffect(effect.effectType, effect.targetId);
                if (existingEffect != null)
                {
                    return StackEffect(existingEffect, sourceEventId, effect);
                }
            }
            
            // Create new active effect
            var activeEffect = new ActiveEventEffect(sourceEventId, effect);
            _activeEffects.Add(activeEffect);
            _cacheDirty = true;
            
            if (_showDebugLogs)
                Debug.Log($"[EventEffectHandler] Applied effect: {effect.effectType} (Value: {effect.value}, Duration: {effect.durationMinutes}m)");
            
            // Apply immediate effects
            ApplyImmediateEffect(activeEffect);
            
            OnEffectApplied?.Invoke(activeEffect);
            OnEffectsChanged?.Invoke();
            
            return activeEffect;
        }
        
        /// <summary>
        /// Applies multiple effects at once
        /// </summary>
        public List<ActiveEventEffect> ApplyEffects(string sourceEventId, List<EventEffect> effects)
        {
            var appliedEffects = new List<ActiveEventEffect>();
            
            if (effects == null)
                return appliedEffects;
            
            foreach (var effect in effects)
            {
                var applied = ApplyEffect(sourceEventId, effect);
                if (applied != null)
                    appliedEffects.Add(applied);
            }
            
            return appliedEffects;
        }
        
        /// <summary>
        /// Removes a specific effect by ID
        /// </summary>
        public bool RemoveEffect(string effectId)
        {
            var effect = _activeEffects.FirstOrDefault(e => e.effectId == effectId);
            if (effect != null)
            {
                return RemoveEffect(effect);
            }
            return false;
        }
        
        /// <summary>
        /// Removes a specific active effect
        /// </summary>
        public bool RemoveEffect(ActiveEventEffect effect)
        {
            if (effect == null || !_activeEffects.Contains(effect))
                return false;
            
            _activeEffects.Remove(effect);
            _cacheDirty = true;
            
            if (_showDebugLogs)
                Debug.Log($"[EventEffectHandler] Removed effect: {effect.effectData.effectType}");
            
            OnEffectRemoved?.Invoke(effect);
            OnEffectsChanged?.Invoke();
            
            return true;
        }
        
        /// <summary>
        /// Removes all effects of a specific type
        /// </summary>
        public int RemoveEffectsOfType(EffectType type)
        {
            var effectsToRemove = _activeEffects.Where(e => e.effectData.effectType == type).ToList();
            int count = 0;
            
            foreach (var effect in effectsToRemove)
            {
                if (RemoveEffect(effect))
                    count++;
            }
            
            return count;
        }
        
        /// <summary>
        /// Removes all effects from a specific event
        /// </summary>
        public int RemoveEffectsFromEvent(string eventId)
        {
            var effectsToRemove = _activeEffects.Where(e => e.sourceEventId == eventId).ToList();
            int count = 0;
            
            foreach (var effect in effectsToRemove)
            {
                if (RemoveEffect(effect))
                    count++;
            }
            
            return count;
        }
        
        /// <summary>
        /// Clears all active effects
        /// </summary>
        public void ClearAllEffects()
        {
            var effectsCopy = new List<ActiveEventEffect>(_activeEffects);
            _activeEffects.Clear();
            _cacheDirty = true;
            
            foreach (var effect in effectsCopy)
            {
                OnEffectRemoved?.Invoke(effect);
            }
            
            if (_showDebugLogs)
                Debug.Log($"[EventEffectHandler] Cleared all {effectsCopy.Count} effects");
            
            OnEffectsChanged?.Invoke();
        }

        #endregion

        #region Public Methods - Effect Queries
        
        /// <summary>
        /// Gets all active effects of a specific type
        /// </summary>
        public List<ActiveEventEffect> GetActiveEffectsOfType(EffectType type)
        {
            return _activeEffects.Where(e => e.effectData.effectType == type).ToList();
        }
        
        /// <summary>
        /// Gets the total multiplier for an effect type (combines all active effects)
        /// </summary>
        public float GetEffectMultiplier(EffectType type)
        {
            // Check cache first
            if (!_cacheDirty && _effectMultiplierCache.TryGetValue(type, out float cachedValue))
            {
                return cachedValue;
            }
            
            var effects = GetActiveEffectsOfType(type);
            float multiplier = 1f;
            
            foreach (var effect in effects)
            {
                // Different effect types interpret value differently
                switch (type)
                {
                    case EffectType.ResearchTimeReduction:
                        // Reduction effects multiply (e.g., 0.9 * 0.9 = 0.81 for two 10% reductions)
                        multiplier *= (1f - effect.effectData.value);
                        break;
                        
                    case EffectType.ProductionSpeedBoost:
                    case EffectType.ResearchSpeedBoost:
                    case EffectType.TestSuccessBoost:
                    case EffectType.ContractValueBoost:
                        // Boost effects add together
                        multiplier += effect.effectData.value;
                        break;
                        
                    case EffectType.RiskReduction:
                        // Risk reduction multiplies
                        multiplier *= (1f - effect.effectData.value);
                        break;
                        
                    case EffectType.MoneyBonus:
                    case EffectType.ReputationChange:
                    default:
                        // These are typically instant effects, return sum for any ongoing ones
                        multiplier += effect.effectData.value;
                        break;
                }
            }
            
            // Update cache
            _effectMultiplierCache[type] = multiplier;
            
            return multiplier;
        }
        
        /// <summary>
        /// Gets the total bonus value for an effect type (additive)
        /// </summary>
        public float GetEffectBonus(EffectType type)
        {
            var effects = GetActiveEffectsOfType(type);
            return effects.Sum(e => e.effectData.value * e.stackCount);
        }
        
        /// <summary>
        /// Checks if an effect of a specific type is active
        /// </summary>
        public bool HasEffectOfType(EffectType type)
        {
            return _activeEffects.Any(e => e.effectData.effectType == type);
        }
        
        /// <summary>
        /// Gets the remaining duration of the longest effect of a type
        /// </summary>
        public float GetLongestDuration(EffectType type)
        {
            var effects = GetActiveEffectsOfType(type);
            if (!effects.Any())
                return 0f;
            
            return effects.Max(e => e.remainingDurationMinutes);
        }
        
        /// <summary>
        /// Gets an effect by its ID
        /// </summary>
        public ActiveEventEffect GetEffectById(string effectId)
        {
            return _activeEffects.FirstOrDefault(e => e.effectId == effectId);
        }

        #endregion

        #region Public Methods - Effect Modification
        
        /// <summary>
        /// Extends the duration of all effects of a type
        /// </summary>
        public void ExtendEffectDuration(EffectType type, float additionalMinutes)
        {
            var effects = GetActiveEffectsOfType(type);
            foreach (var effect in effects)
            {
                effect.remainingDurationMinutes += additionalMinutes;
            }
            
            if (effects.Any() && _showDebugLogs)
                Debug.Log($"[EventEffectHandler] Extended {type} effects by {additionalMinutes}m");
        }
        
        /// <summary>
        /// Reduces the duration of all effects of a type
        /// </summary>
        public void ReduceEffectDuration(EffectType type, float reductionMinutes)
        {
            var effects = GetActiveEffectsOfType(type);
            foreach (var effect in effects)
            {
                effect.remainingDurationMinutes = Mathf.Max(0, effect.remainingDurationMinutes - reductionMinutes);
            }
            
            ProcessEffects(0); // Clean up expired effects
        }

        #endregion

        #region Public Methods - Save/Load
        
        /// <summary>
        /// Returns handler state for serialization
        /// </summary>
        public EventEffectHandlerSaveData GetSaveData()
        {
            return new EventEffectHandlerSaveData
            {
                ActiveEffects = _activeEffects.Select(e => new SerializableActiveEffect
                {
                    EffectId = e.effectId,
                    SourceEventId = e.sourceEventId,
                    EffectType = (int)e.effectData.effectType,
                    Value = e.effectData.value,
                    DurationMinutes = e.effectData.durationMinutes,
                    TargetId = e.effectData.targetId,
                    RemainingDuration = e.remainingDurationMinutes,
                    StackCount = e.stackCount,
                    AppliedAt = e.appliedAt.ToBinary()
                }).ToList()
            };
        }
        
        /// <summary>
        /// Restores handler state from serialization
        /// </summary>
        public void LoadSaveData(EventEffectHandlerSaveData data)
        {
            ClearAllEffects();
            
            if (data.ActiveEffects == null)
                return;
            
            foreach (var savedEffect in data.ActiveEffects)
            {
                var effect = new EventEffect
                {
                    effectType = (EffectType)savedEffect.EffectType,
                    value = savedEffect.Value,
                    durationMinutes = savedEffect.DurationMinutes,
                    targetId = savedEffect.TargetId
                };
                
                var activeEffect = new ActiveEventEffect(savedEffect.SourceEventId, effect)
                {
                    effectId = savedEffect.EffectId,
                    remainingDurationMinutes = savedEffect.RemainingDuration,
                    stackCount = savedEffect.StackCount
                };
                
                _activeEffects.Add(activeEffect);
            }
            
            _cacheDirty = true;
            
            if (_showDebugLogs)
                Debug.Log($"[EventEffectHandler] Loaded {data.ActiveEffects.Count} effects");
        }

        #endregion

        #region Private Methods
        
        private void ProcessEffects(float deltaTimeMinutes)
        {
            bool anyExpired = false;
            
            // Update all effects
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.Update(deltaTimeMinutes);
                
                if (effect.IsExpired)
                {
                    var expiredEffect = _activeEffects[i];
                    _activeEffects.RemoveAt(i);
                    _cacheDirty = true;
                    anyExpired = true;
                    
                    if (_showDebugLogs)
                        Debug.Log($"[EventEffectHandler] Effect expired: {expiredEffect.effectData.effectType}");
                    
                    OnEffectExpired?.Invoke(expiredEffect);
                }
            }
            
            if (anyExpired)
            {
                OnEffectsChanged?.Invoke();
            }
        }
        
        private ActiveEventEffect FindStackableEffect(EffectType type, string targetId)
        {
            return _activeEffects.FirstOrDefault(e => 
                e.effectData.effectType == type && 
                e.effectData.targetId == targetId &&
                e.effectData.canStack &&
                (e.effectData.maxStackCount == 0 || e.stackCount < e.effectData.maxStackCount));
        }
        
        private ActiveEventEffect StackEffect(ActiveEventEffect existingEffect, string sourceEventId, EventEffect newEffect)
        {
            // Check global stack limit
            if (existingEffect.stackCount >= _maxGlobalStackCount)
            {
                if (_showDebugLogs)
                    Debug.Log($"[EventEffectHandler] Stack limit reached for {newEffect.effectType}");
                return existingEffect;
            }
            
            // Stack the effects
            existingEffect.stackCount++;
            
            // Refresh duration (take the longer of the two)
            existingEffect.remainingDurationMinutes = Mathf.Max(
                existingEffect.remainingDurationMinutes, 
                newEffect.durationMinutes
            );
            
            _cacheDirty = true;
            
            if (_showDebugLogs)
                Debug.Log($"[EventEffectHandler] Stacked {newEffect.effectType} (Stack: {existingEffect.stackCount})");
            
            OnEffectsStacked?.Invoke(existingEffect, new ActiveEventEffect(sourceEventId, newEffect));
            OnEffectsChanged?.Invoke();
            
            return existingEffect;
        }
        
        private void ApplyImmediateEffect(ActiveEventEffect activeEffect)
        {
            // Handle instant effects (duration = 0)
            if (activeEffect.effectData.durationMinutes <= 0)
            {
                switch (activeEffect.effectData.effectType)
                {
                    case EffectType.MoneyBonus:
                        // Would call: EconomyManager.Instance.AddMoney(activeEffect.effectData.value);
                        if (_showDebugLogs)
                            Debug.Log($"[EventEffectHandler] Instant money bonus: {activeEffect.effectData.value}");
                        break;
                        
                    case EffectType.ReputationChange:
                        // Would call: ReputationManager.Instance.ModifyReputation(activeEffect.effectData.value);
                        if (_showDebugLogs)
                            Debug.Log($"[EventEffectHandler] Instant reputation change: {activeEffect.effectData.value}");
                        break;
                        
                    case EffectType.ResearchTimeReduction:
                        // Would call: ResearchManager.Instance.ReduceCurrentResearchTime(activeEffect.effectData.value);
                        if (_showDebugLogs)
                            Debug.Log($"[EventEffectHandler] Instant research time reduction: {activeEffect.effectData.value}m");
                        break;
                }
            }
        }

        #endregion

        #region Debug
        
        private void OnGUI()
        {
            if (!_showDebugLogs)
                return;
            
            GUILayout.BeginArea(new Rect(10, 140, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"<b>Active Event Effects ({_activeEffects.Count})</b>");
            
            foreach (var effect in _activeEffects)
            {
                string progress = effect.totalDurationMinutes > 0 
                    ? $"{(1f - effect.Progress) * 100:F0}%" 
                    : "∞";
                
                GUILayout.Label($"• {effect.effectData.effectType} (x{effect.stackCount}) - {progress}");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
    
    /// <summary>
    /// Serializable data for effect handler state persistence
    /// </summary>
    [Serializable]
    public struct EventEffectHandlerSaveData
    {
        public List<SerializableActiveEffect> ActiveEffects;
    }
    
    /// <summary>
    /// Serializable representation of an active effect
    /// </summary>
    [Serializable]
    public struct SerializableActiveEffect
    {
        public string EffectId;
        public string SourceEventId;
        public int EffectType;
        public float Value;
        public float DurationMinutes;
        public string TargetId;
        public float RemainingDuration;
        public int StackCount;
        public long AppliedAt;
    }
}
