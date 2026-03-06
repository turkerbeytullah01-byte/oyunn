// ============================================================================
// Project Aegis: Drone Dominion
// IdleSystem - Manages offline progress and idle generation
// ============================================================================
// Calculates and applies offline earnings, manages production lines,
// and handles passive resource generation.
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectAegis.Core;

namespace ProjectAegis.Systems
{
    /// <summary>
    /// Manages idle progression and offline earnings.
    /// </summary>
    public class IdleSystem : BaseSystem
    {
        #region System Properties
        
        public override string SystemId => "IdleSystem";
        public override string SystemName => "Idle System";
        public override int UpdatePriority => 40;
        
        #endregion
        
        #region State
        
        /// <summary>
        /// Current money generation per second.
        /// </summary>
        public float MoneyPerSecond { get; private set; }
        
        /// <summary>
        /// Current research point generation per second.
        /// </summary>
        public float ResearchPointsPerSecond { get; private set; }
        
        /// <summary>
        /// Production lines for drone manufacturing.
        /// </summary>
        private List<ProductionLine> _productionLines = new List<ProductionLine>();
        
        /// <summary>
        /// Offline progress data from last session.
        /// </summary>
        public OfflineProgressData LastOfflineProgress { get; private set; }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Called when offline progress is applied.
        /// </summary>
        public event Action<OfflineEarnings> OnOfflineEarningsApplied;
        
        /// <summary>
        /// Called when a production cycle completes.
        /// </summary>
        public event Action<string, int> OnProductionCycleComplete;
        
        #endregion
        
        #region Initialization
        
        protected override void OnInitialize()
        {
            // Register with ServiceLocator
            ServiceLocator.Instance?.Register<IIdleSystem>(this);
            
            Log("IdleSystem initialized");
        }
        
        protected override void OnPostInitialize()
        {
            // Calculate offline progress
            CalculateOfflineProgress();
        }
        
        #endregion
        
        #region Update
        
        protected override void OnTick(float deltaTime)
        {
            // Apply passive generation
            ApplyPassiveGeneration(deltaTime);
            
            // Update production lines
            UpdateProductionLines(deltaTime);
        }
        
        #endregion
        
        #region Passive Generation
        
        /// <summary>
        /// Applies passive resource generation.
        /// </summary>
        private void ApplyPassiveGeneration(float deltaTime)
        {
            var playerData = SaveManager.Instance?.GetPlayerData();
            if (playerData == null) return;
            
            // Generate money
            if (MoneyPerSecond > 0)
            {
                long moneyEarned = (long)(MoneyPerSecond * deltaTime);
                if (moneyEarned > 0)
                {
                    playerData.AddMoney(moneyEarned, "Passive Income");
                }
            }
            
            // Generate research points
            if (ResearchPointsPerSecond > 0)
            {
                long rpEarned = (long)(ResearchPointsPerSecond * deltaTime);
                if (rpEarned > 0)
                {
                    playerData.AddResearchPoints(rpEarned, "Passive Research");
                }
            }
        }
        
        /// <summary>
        /// Sets the money generation rate.
        /// </summary>
        public void SetMoneyRate(float rate)
        {
            MoneyPerSecond = Mathf.Max(0, rate);
        }
        
        /// <summary>
        /// Sets the research point generation rate.
        /// </summary>
        public void SetResearchRate(float rate)
        {
            ResearchPointsPerSecond = Mathf.Max(0, rate);
        }
        
        #endregion
        
        #region Offline Progress
        
        /// <summary>
        /// Calculates and applies offline progress.
        /// </summary>
        public void CalculateOfflineProgress()
        {
            var timeManager = TimeManager.Instance;
            if (timeManager == null) return;
            
            LastOfflineProgress = timeManager.CalculateOfflineProgress();
            
            if (!LastOfflineProgress.WasOffline)
                return;
            
            // Calculate offline earnings
            var earnings = CalculateOfflineEarnings(LastOfflineProgress.EffectiveSeconds);
            
            // Apply earnings
            ApplyOfflineEarnings(earnings);
            
            // Notify
            OnOfflineEarningsApplied?.Invoke(earnings);
            EventManager.Instance?.TriggerOfflineProgressCalculated(
                LastOfflineProgress.OfflineSeconds,
                earnings.MoneyEarned,
                earnings.ResearchPointsEarned
            );
            
            Log($"Offline earnings: ${earnings.MoneyEarned} and {earnings.ResearchPointsEarned} RP");
        }
        
        /// <summary>
        /// Calculates earnings for a given offline duration.
        /// </summary>
        private OfflineEarnings CalculateOfflineEarnings(float seconds)
        {
            return new OfflineEarnings
            {
                Duration = seconds,
                MoneyEarned = (long)(MoneyPerSecond * seconds),
                ResearchPointsEarned = (long)(ResearchPointsPerSecond * seconds),
                DronesProduced = CalculateOfflineProduction(seconds)
            };
        }
        
        /// <summary>
        /// Calculates offline drone production.
        /// </summary>
        private int CalculateOfflineProduction(float seconds)
        {
            int totalProduced = 0;
            foreach (var line in _productionLines)
            {
                if (line.IsActive)
                {
                    totalProduced += line.CalculateOfflineProduction(seconds);
                }
            }
            return totalProduced;
        }
        
        /// <summary>
        /// Applies offline earnings to player data.
        /// </summary>
        private void ApplyOfflineEarnings(OfflineEarnings earnings)
        {
            var playerData = SaveManager.Instance?.GetPlayerData();
            if (playerData == null) return;
            
            if (earnings.MoneyEarned > 0)
                playerData.AddMoney(earnings.MoneyEarned, "Offline Earnings");
            
            if (earnings.ResearchPointsEarned > 0)
                playerData.AddResearchPoints(earnings.ResearchPointsEarned, "Offline Research");
        }
        
        #endregion
        
        #region Production Lines
        
        /// <summary>
        /// Adds a production line.
        /// </summary>
        public ProductionLine AddProductionLine(string droneId, float productionTime)
        {
            var line = new ProductionLine
            {
                LineId = Guid.NewGuid().ToString("N").Substring(0, 8),
                DroneId = droneId,
                ProductionTime = productionTime,
                IsActive = false
            };
            
            line.OnCycleComplete += (count) => OnProductionCycleComplete?.Invoke(droneId, count);
            
            _productionLines.Add(line);
            return line;
        }
        
        /// <summary>
        /// Updates all production lines.
        /// </summary>
        private void UpdateProductionLines(float deltaTime)
        {
            foreach (var line in _productionLines)
            {
                if (line.IsActive)
                {
                    line.Update(deltaTime);
                }
            }
        }
        
        /// <summary>
        /// Gets all production lines.
        /// </summary>
        public IReadOnlyList<ProductionLine> GetProductionLines()
        {
            return _productionLines;
        }
        
        #endregion
        
        #region Cleanup
        
        protected override void OnDispose()
        {
            ServiceLocator.Instance?.Unregister<IIdleSystem>();
        }
        
        #endregion
    }
    
    #region Supporting Types
    
    /// <summary>
    /// Represents earnings from offline time.
    /// </summary>
    public struct OfflineEarnings
    {
        public float Duration;
        public long MoneyEarned;
        public long ResearchPointsEarned;
        public int DronesProduced;
    }
    
    /// <summary>
    /// Represents a production line for drone manufacturing.
    /// </summary>
    public class ProductionLine
    {
        public string LineId;
        public string DroneId;
        public float ProductionTime;
        public bool IsActive;
        public int QueueCount;
        
        private float _currentProgress;
        
        public event Action<int> OnCycleComplete;
        
        public void Update(float deltaTime)
        {
            if (!IsActive || QueueCount <= 0) return;
            
            _currentProgress += deltaTime;
            
            while (_currentProgress >= ProductionTime && QueueCount > 0)
            {
                _currentProgress -= ProductionTime;
                QueueCount--;
                OnCycleComplete?.Invoke(1);
            }
        }
        
        public int CalculateOfflineProduction(float seconds)
        {
            if (!IsActive || ProductionTime <= 0) return 0;
            
            float totalProgress = _currentProgress + seconds;
            int cycles = Mathf.FloorToInt(totalProgress / ProductionTime);
            int actualProduced = Mathf.Min(cycles, QueueCount);
            
            return actualProduced;
        }
        
        public float ProgressNormalized => ProductionTime > 0 
            ? Mathf.Clamp01(_currentProgress / ProductionTime) 
            : 0f;
    }
    
    /// <summary>
    /// Interface for the idle system.
    /// </summary>
    public interface IIdleSystem
    {
        float MoneyPerSecond { get; }
        float ResearchPointsPerSecond { get; }
        OfflineProgressData LastOfflineProgress { get; }
        void CalculateOfflineProgress();
        void SetMoneyRate(float rate);
        void SetResearchRate(float rate);
    }
    
    #endregion
}
