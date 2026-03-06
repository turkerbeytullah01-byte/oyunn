// ============================================================================
// Project Aegis: Drone Dominion
// ResearchSystem - Manages all research operations
// ============================================================================
// Handles research queue, progress tracking, and technology unlocking.
// ============================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectAegis.Core;
using ProjectAegis.Data;

namespace ProjectAegis.Systems
{
    /// <summary>
    /// Manages the research system including queue, progress, and unlocks.
    /// </summary>
    public class ResearchSystem : BaseSystem
    {
        #region System Properties
        
        public override string SystemId => "ResearchSystem";
        public override string SystemName => "Research System";
        public override int UpdatePriority => 50;
        
        #endregion
        
        #region State
        
        /// <summary>
        /// Currently active research project.
        /// </summary>
        public ResearchProgress ActiveResearch { get; private set; }
        
        /// <summary>
        /// Queue of research projects waiting to start.
        /// </summary>
        private Queue<string> _researchQueue = new Queue<string>();
        
        /// <summary>
        /// All available research data.
        /// </summary>
        private Dictionary<string, ResearchData> _researchDatabase = new Dictionary<string, ResearchData>();
        
        /// <summary>
        /// Current research point generation rate.
        /// </summary>
        public float CurrentResearchRate { get; private set; } = 1f;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Called when research is added to queue.
        /// </summary>
        public event System.Action<string> OnResearchQueued;
        
        /// <summary>
        /// Called when research queue changes.
        /// </summary>
        public event System.Action OnQueueChanged;
        
        #endregion
        
        #region Initialization
        
        protected override void OnInitialize()
        {
            // Load research database
            LoadResearchDatabase();
            
            // Register with ServiceLocator
            ServiceLocator.Instance?.Register<IResearchSystem>(this);
            
            Log("ResearchSystem initialized");
        }
        
        protected override void OnPostInitialize()
        {
            // Subscribe to events
            EventManager.Instance.OnResearchCompleted += OnResearchCompleted;
        }
        
        private void LoadResearchDatabase()
        {
            var researchAssets = Resources.LoadAll<ResearchData>("ScriptableObjects/Research");
            foreach (var research in researchAssets)
            {
                _researchDatabase[research.Id] = research;
            }
            Log($"Loaded {_researchDatabase.Count} research projects");
        }
        
        #endregion
        
        #region Update
        
        protected override void OnTick(float deltaTime)
        {
            // Update active research
            ActiveResearch?.Update(deltaTime);
        }
        
        #endregion
        
        #region Research Management
        
        /// <summary>
        /// Attempts to start a research project.
        /// </summary>
        public bool StartResearch(string researchId)
        {
            if (!_researchDatabase.TryGetValue(researchId, out var researchData))
            {
                LogWarning($"Research not found: {researchId}");
                return false;
            }
            
            // Check if already researching
            if (ActiveResearch != null && ActiveResearch.ResearchId == researchId)
            {
                LogWarning($"Already researching: {researchId}");
                return false;
            }
            
            // Check prerequisites
            var playerData = SaveManager.Instance?.GetPlayerData();
            if (!researchData.ArePrerequisitesMet(playerData))
            {
                LogWarning($"Prerequisites not met for: {researchId}");
                return false;
            }
            
            // Check cost
            if (!playerData.CanAfford(researchData.ResearchCost))
            {
                LogWarning($"Cannot afford research: {researchId}");
                return false;
            }
            
            // Deduct cost
            playerData.DeductCost(researchData.ResearchCost, $"Research: {researchData.DisplayName}");
            
            // Create research progress
            ActiveResearch = new ResearchProgress(
                researchId,
                researchData.DisplayName,
                researchData.ResearchPointsRequired
            );
            
            ActiveResearch.ProgressRatePerSecond = CurrentResearchRate;
            ActiveResearch.OnProgressComplete += () => CompleteResearch(researchId);
            ActiveResearch.Start();
            
            Log($"Started research: {researchData.DisplayName}");
            return true;
        }
        
        /// <summary>
        /// Adds research to the queue.
        /// </summary>
        public bool QueueResearch(string researchId)
        {
            if (!_researchDatabase.ContainsKey(researchId))
            {
                LogWarning($"Research not found: {researchId}");
                return false;
            }
            
            _researchQueue.Enqueue(researchId);
            OnResearchQueued?.Invoke(researchId);
            OnQueueChanged?.Invoke();
            
            Log($"Queued research: {researchId}");
            return true;
        }
        
        /// <summary>
        /// Cancels the active research.
        /// </summary>
        public void CancelResearch(bool refund = false)
        {
            if (ActiveResearch == null) return;
            
            ActiveResearch.Cancel(refund);
            ActiveResearch = null;
            
            // Start next in queue
            ProcessQueue();
        }
        
        /// <summary>
        /// Completes a research project and applies rewards.
        /// </summary>
        private void CompleteResearch(string researchId)
        {
            if (!_researchDatabase.TryGetValue(researchId, out var researchData))
                return;
            
            // Record completion
            SaveManager.Instance?.GetPlayerData()?.RecordResearchCompleted(researchId);
            
            // Unlock technologies
            foreach (var techId in researchData.UnlockedTechnologies)
            {
                SaveManager.Instance?.GetPlayerData()?.UnlockTechnology(techId);
            }
            
            // Unlock drones
            foreach (var droneId in researchData.UnlockedDrones)
            {
                SaveManager.Instance?.GetPlayerData()?.UnlockDrone(droneId);
            }
            
            // Apply modifiers
            ApplyResearchModifiers(researchData.Modifiers);
            
            ActiveResearch = null;
            
            // Process queue
            ProcessQueue();
            
            Log($"Research completed: {researchData.DisplayName}");
        }
        
        /// <summary>
        /// Processes the research queue.
        /// </summary>
        private void ProcessQueue()
        {
            if (ActiveResearch != null || _researchQueue.Count == 0)
                return;
            
            string nextResearch = _researchQueue.Dequeue();
            StartResearch(nextResearch);
            OnQueueChanged?.Invoke();
        }
        
        /// <summary>
        /// Applies research modifiers.
        /// </summary>
        private void ApplyResearchModifiers(ResearchModifier[] modifiers)
        {
            if (modifiers == null) return;
            
            foreach (var modifier in modifiers)
            {
                // Apply modifier based on target property
                switch (modifier.TargetProperty)
                {
                    case "ResearchRate":
                        CurrentResearchRate += modifier.Value;
                        break;
                    // Add more modifier targets as needed
                }
            }
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// Gets all available research (meets prerequisites, not completed).
        /// </summary>
        public ResearchData[] GetAvailableResearch()
        {
            var playerData = SaveManager.Instance?.GetPlayerData();
            if (playerData == null) return new ResearchData[0];
            
            return _researchDatabase.Values
                .Where(r => !playerData.CompletedResearchIds.Contains(r.Id))
                .Where(r => r.ArePrerequisitesMet(playerData))
                .ToArray();
        }
        
        /// <summary>
        /// Gets research by ID.
        /// </summary>
        public ResearchData GetResearch(string researchId)
        {
            _researchDatabase.TryGetValue(researchId, out var research);
            return research;
        }
        
        /// <summary>
        /// Gets the current queue as an array.
        /// </summary>
        public string[] GetQueue()
        {
            return _researchQueue.ToArray();
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnResearchCompleted(string researchId, string researchName)
        {
            // Additional handling if needed
        }
        
        #endregion
        
        #region Cleanup
        
        protected override void OnDispose()
        {
            EventManager.Instance.OnResearchCompleted -= OnResearchCompleted;
            ServiceLocator.Instance?.Unregister<IResearchSystem>();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface for the research system.
    /// </summary>
    public interface IResearchSystem
    {
        ResearchProgress ActiveResearch { get; }
        float CurrentResearchRate { get; }
        bool StartResearch(string researchId);
        bool QueueResearch(string researchId);
        void CancelResearch(bool refund = false);
        ResearchData[] GetAvailableResearch();
    }
}
