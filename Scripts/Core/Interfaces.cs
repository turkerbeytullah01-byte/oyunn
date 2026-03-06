// ============================================================================
// Project Aegis: Drone Dominion
// Core Interfaces - Foundation for all game systems
// ============================================================================
// This file contains all core interfaces used throughout the game architecture.
// Interfaces define contracts that classes must implement, ensuring consistency
// and enabling polymorphic behavior across systems.
// ============================================================================

using System;
using UnityEngine;

namespace ProjectAegis.Core
{
    #region Lifecycle Interfaces
    
    /// <summary>
    /// Interface for objects that require initialization.
    /// Implement this when a class needs a one-time setup before use.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Returns whether the object has been initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Initializes the object. Should be called once before any other operations.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Called after all systems have been initialized.
        /// Use for cross-system setup that requires other systems to be ready.
        /// </summary>
        void PostInitialize();
    }
    
    /// <summary>
    /// Interface for objects that need to be cleaned up when no longer needed.
    /// Implement this to ensure proper resource disposal and prevent memory leaks.
    /// </summary>
    public interface IDisposable
    {
        /// <summary>
        /// Cleans up resources and unsubscribes from events.
        /// </summary>
        void Dispose();
    }
    
    #endregion
    
    #region Update Interfaces
    
    /// <summary>
    /// Interface for objects that need to update every frame.
    /// Use sparingly - prefer event-driven architecture when possible.
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// Called every frame. Use for visual updates only.
        /// </summary>
        void Tick(float deltaTime);
    }
    
    /// <summary>
    /// Interface for objects that need fixed timestep updates.
    /// Use for physics-related or time-sensitive calculations.
    /// </summary>
    public interface IFixedTickable
    {
        /// <summary>
        /// Called at fixed intervals (default 0.02s).
        /// </summary>
        void FixedTick(float fixedDeltaTime);
    }
    
    /// <summary>
    /// Interface for objects that need occasional updates.
    /// More efficient than Tick for non-critical updates.
    /// </summary>
    public interface ILateTickable
    {
        /// <summary>
        /// Called after all Tick methods have completed.
        /// </summary>
        void LateTick(float deltaTime);
    }
    
    #endregion
    
    #region Save/Load Interfaces
    
    /// <summary>
    /// Interface for objects that support save/load functionality.
    /// Implement this for any data that should persist between sessions.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Returns a unique identifier for this saveable object.
        /// Used as the key in save data dictionaries.
        /// </summary>
        string SaveKey { get; }
        
        /// <summary>
        /// Serializes the object's state to a save data structure.
        /// </summary>
        /// <returns>Object containing all serializable state</returns>
        object CaptureState();
        
        /// <summary>
        /// Restores the object's state from saved data.
        /// </summary>
        /// <param name="state">The saved state to restore from</param>
        void RestoreState(object state);
        
        /// <summary>
        /// Returns the version of the save data format.
        /// Increment when making breaking changes to saved data structure.
        /// </summary>
        int SaveVersion { get; }
    }
    
    /// <summary>
    /// Interface for objects that track progress over time.
    /// Used for research, construction, training, etc.
    /// </summary>
    public interface IProgressable
    {
        /// <summary>
        /// Current progress value (0.0 to 1.0 or absolute time).
        /// </summary>
        float CurrentProgress { get; }
        
        /// <summary>
        /// Total progress required to complete.
        /// </summary>
        float TotalProgressRequired { get; }
        
        /// <summary>
        /// Returns progress as a normalized value (0.0 to 1.0).
        /// </summary>
        float NormalizedProgress => TotalProgressRequired > 0 
            ? Mathf.Clamp01(CurrentProgress / TotalProgressRequired) 
            : 0f;
        
        /// <summary>
        /// Whether the progress is currently active and advancing.
        /// </summary>
        bool IsProgressing { get; }
        
        /// <summary>
        /// Whether the progress has reached completion.
        /// </summary>
        bool IsComplete => CurrentProgress >= TotalProgressRequired;
        
        /// <summary>
        /// Advances progress by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to advance</param>
        void AdvanceProgress(float amount);
        
        /// <summary>
        /// Called when progress reaches completion.
        /// </summary>
        event Action OnProgressComplete;
        
        /// <summary>
        /// Called whenever progress changes.
        /// </summary>
        event Action<float> OnProgressChanged;
    }
    
    #endregion
    
    #region Game System Interfaces
    
    /// <summary>
    /// Interface for systems that can be paused and resumed.
    /// </summary>
    public interface IPausable
    {
        /// <summary>
        /// Whether the system is currently paused.
        /// </summary>
        bool IsPaused { get; }
        
        /// <summary>
        /// Pauses the system.
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resumes the system.
        /// </summary>
        void Resume();
    }
    
    /// <summary>
    /// Interface for objects that can be locked/unlocked.
    /// Used for research nodes, features, contracts, etc.
    /// </summary>
    public interface ILockable
    {
        /// <summary>
        /// Whether the object is currently unlocked and accessible.
        /// </summary>
        bool IsUnlocked { get; }
        
        /// <summary>
        /// Conditions that must be met to unlock this object.
        /// </summary>
        IUnlockCondition[] UnlockConditions { get; }
        
        /// <summary>
        /// Attempts to unlock the object if all conditions are met.
        /// </summary>
        /// <returns>True if successfully unlocked</returns>
        bool TryUnlock();
        
        /// <summary>
        /// Called when the object is unlocked.
        /// </summary>
        event Action OnUnlocked;
    }
    
    /// <summary>
    /// Interface defining conditions for unlocking objects.
    /// </summary>
    public interface IUnlockCondition
    {
        /// <summary>
        /// Description of the condition for display to players.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Whether the condition is currently satisfied.
        /// </summary>
        bool IsSatisfied { get; }
        
        /// <summary>
        /// Checks if the condition is satisfied.
        /// </summary>
        bool CheckCondition();
    }
    
    /// <summary>
    /// Interface for objects that have a cost associated with them.
    /// </summary>
    public interface ICostable
    {
        /// <summary>
        /// The cost to acquire/activate this object.
        /// </summary>
        Cost GetCost();
        
        /// <summary>
        /// Whether the player can currently afford this object.
        /// </summary>
        bool CanAfford();
        
        /// <summary>
        /// Attempts to deduct the cost from player resources.
        /// </summary>
        /// <returns>True if cost was successfully deducted</returns>
        bool DeductCost();
    }
    
    #endregion
    
    #region Data Interfaces
    
    /// <summary>
    /// Interface for objects that have a unique identifier.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Unique identifier for this object.
        /// </summary>
        string Id { get; }
    }
    
    /// <summary>
    /// Interface for objects that have a display name and description.
    /// </summary>
    public interface IDisplayable
    {
        /// <summary>
        /// Display name for UI.
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Description for UI tooltips and info panels.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Icon for UI representation.
        /// </summary>
        Sprite Icon { get; }
    }
    
    /// <summary>
    /// Interface for objects that can be modified by modifiers.
    /// </summary>
    public interface IModifiable
    {
        /// <summary>
        /// Applies a modifier to this object.
        /// </summary>
        void ApplyModifier(IModifier modifier);
        
        /// <summary>
        /// Removes a modifier from this object.
        /// </summary>
        void RemoveModifier(IModifier modifier);
        
        /// <summary>
        /// Recalculates all modified values.
        /// </summary>
        void RecalculateModifiers();
    }
    
    /// <summary>
    /// Interface for modifiers that can be applied to modifiable objects.
    /// </summary>
    public interface IModifier
    {
        /// <summary>
        /// Unique identifier for this modifier type.
        /// </summary>
        string ModifierId { get; }
        
        /// <summary>
        /// The stat or property this modifier affects.
        /// </summary>
        string TargetProperty { get; }
        
        /// <summary>
        /// The type of modification (additive, multiplicative, etc.)
        /// </summary>
        ModifierType Type { get; }
        
        /// <summary>
        /// The value of the modification.
        /// </summary>
        float Value { get; }
    }
    
    /// <summary>
    /// Types of modifiers that can be applied.
    /// </summary>
    public enum ModifierType
    {
        /// <summary>Adds to the base value</summary>
        Additive,
        /// <summary>Multiplies the base value</summary>
        Multiplicative,
        /// <summary>Sets a flat override value</summary>
        Override,
        /// <summary>Adds a percentage of the base value</summary>
        PercentageAdd
    }
    
    #endregion
    
    #region Event Interfaces
    
    /// <summary>
    /// Interface for objects that can trigger game events.
    /// </summary>
    public interface IEventTrigger
    {
        /// <summary>
        /// Triggers the event.
        /// </summary>
        void Trigger();
        
        /// <summary>
        /// Whether this event can currently be triggered.
        /// </summary>
        bool CanTrigger { get; }
        
        /// <summary>
        /// Cooldown duration before the event can trigger again.
        /// </summary>
        float CooldownDuration { get; }
        
        /// <summary>
        /// Time remaining on the current cooldown.
        /// </summary>
        float CooldownRemaining { get; }
    }
    
    /// <summary>
    /// Interface for objects that listen to game events.
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        /// Called when a relevant game event occurs.
        /// </summary>
        /// <param name="eventType">Type of event that occurred</param>
        /// <param name="eventData">Data associated with the event</param>
        void OnEventReceived(string eventType, object eventData);
    }
    
    #endregion
    
    #region Cost Structure
    
    /// <summary>
    /// Represents a cost in multiple resource types.
    /// </summary>
    [Serializable]
    public struct Cost
    {
        public long Money;
        public long ResearchPoints;
        public int Reputation;
        public int Energy;
        public int Materials;
        
        public static Cost Zero => new Cost();
        
        public bool IsZero => Money == 0 && ResearchPoints == 0 && 
                              Reputation == 0 && Energy == 0 && Materials == 0;
        
        public static Cost operator +(Cost a, Cost b)
        {
            return new Cost
            {
                Money = a.Money + b.Money,
                ResearchPoints = a.ResearchPoints + b.ResearchPoints,
                Reputation = a.Reputation + b.Reputation,
                Energy = a.Energy + b.Energy,
                Materials = a.Materials + b.Materials
            };
        }
        
        public static Cost operator -(Cost a, Cost b)
        {
            return new Cost
            {
                Money = a.Money - b.Money,
                ResearchPoints = a.ResearchPoints - b.ResearchPoints,
                Reputation = a.Reputation - b.Reputation,
                Energy = a.Energy - b.Energy,
                Materials = a.Materials - b.Materials
            };
        }
        
        public static Cost operator *(Cost cost, float multiplier)
        {
            return new Cost
            {
                Money = (long)(cost.Money * multiplier),
                ResearchPoints = (long)(cost.ResearchPoints * multiplier),
                Reputation = (int)(cost.Reputation * multiplier),
                Energy = (int)(cost.Energy * multiplier),
                Materials = (int)(cost.Materials * multiplier)
            };
        }
    }
    
    #endregion
}
