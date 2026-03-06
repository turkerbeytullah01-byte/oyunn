using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Idle
{
    /// <summary>
    /// Manages all production lines and calculates resource generation.
    /// Handles both active (online) and passive (offline) production.
    /// </summary>
    public class ProductionManager : BaseManager<ProductionManager>
    {
        [Header("Production Lines")]
        [SerializeField] private List<ProductionLine> productionLines = new();
        
        [Header("Global Multipliers")]
        [SerializeField] private float globalProductionMultiplier = 1.0f;
        [SerializeField] private float premiumMultiplier = 1.0f;
        
        [Header("Settings")]
        [SerializeField] private float productionTickInterval = 1.0f; // seconds
        
        // Runtime
        private float _tickTimer;
        private float _totalMoneyPerMinute;
        private bool _isDirty = true;
        
        // Events
        public event Action<float> OnMoneyProduced;
        public event Action<ProductionLine> OnLineAdded;
        public event Action<ProductionLine> OnLineRemoved;
        public event Action OnProductionRatesChanged;

        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            _tickTimer = 0;
        }

        private void Update()
        {
            // Active production tick
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= productionTickInterval)
            {
                _tickTimer = 0;
                ProcessActiveProduction();
            }
        }
        
        #endregion

        #region Production Line Management

        /// <summary>
        /// Adds a new production line
        /// </summary>
        public ProductionLine AddProductionLine(string droneModelId, float baseProductionRate, string lineId = null)
        {
            var line = new ProductionLine(droneModelId, baseProductionRate, lineId);
            productionLines.Add(line);
            
            line.OnProductionTick += OnLineProductionTick;
            
            _isDirty = true;
            OnLineAdded?.Invoke(line);
            OnProductionRatesChanged?.Invoke();
            
            Debug.Log($"[ProductionManager] Added production line: {line.GetDisplayName()}");
            return line;
        }

        /// <summary>
        /// Adds an existing production line (used for loading saved data)
        /// </summary>
        public void AddProductionLine(ProductionLine line)
        {
            if (line == null) return;
            
            // Check for duplicate IDs
            if (productionLines.Any(l => l.lineId == line.lineId))
            {
                Debug.LogWarning($"[ProductionManager] Production line with ID {line.lineId} already exists. Skipping.");
                return;
            }
            
            productionLines.Add(line);
            line.OnProductionTick += OnLineProductionTick;
            
            _isDirty = true;
            OnLineAdded?.Invoke(line);
            OnProductionRatesChanged?.Invoke();
        }

        /// <summary>
        /// Removes a production line by ID
        /// </summary>
        public bool RemoveProductionLine(string lineId)
        {
            var line = productionLines.FirstOrDefault(l => l.lineId == lineId);
            if (line == null) return false;
            
            return RemoveProductionLine(line);
        }

        /// <summary>
        /// Removes a production line
        /// </summary>
        public bool RemoveProductionLine(ProductionLine line)
        {
            if (line == null || !productionLines.Contains(line)) return false;
            
            line.OnProductionTick -= OnLineProductionTick;
            productionLines.Remove(line);
            
            _isDirty = true;
            OnLineRemoved?.Invoke(line);
            OnProductionRatesChanged?.Invoke();
            
            Debug.Log($"[ProductionManager] Removed production line: {line.GetDisplayName()}");
            return true;
        }

        /// <summary>
        /// Gets a production line by ID
        /// </summary>
        public ProductionLine GetLine(string lineId)
        {
            return productionLines.FirstOrDefault(l => l.lineId == lineId);
        }

        /// <summary>
        /// Gets all active production lines
        /// </summary>
        public List<ProductionLine> GetActiveLines()
        {
            return productionLines.Where(l => l.isActive).ToList();
        }

        /// <summary>
        /// Gets all production lines (including inactive)
        /// </summary>
        public List<ProductionLine> GetAllLines()
        {
            return new List<ProductionLine>(productionLines);
        }

        /// <summary>
        /// Gets production lines for a specific drone model
        /// </summary>
        public List<ProductionLine> GetLinesForModel(string droneModelId)
        {
            return productionLines.Where(l => l.droneModelId == droneModelId).ToList();
        }

        #endregion

        #region Production Calculations

        /// <summary>
        /// Gets total money generated per minute from all active lines
        /// </summary>
        public float GetMoneyPerMinute()
        {
            if (_isDirty)
            {
                RecalculateTotals();
            }
            return _totalMoneyPerMinute;
        }

        /// <summary>
        /// Gets money per second (for UI)
        /// </summary>
        public float GetMoneyPerSecond()
        {
            return GetMoneyPerMinute() / 60f;
        }

        /// <summary>
        /// Gets money per hour (for UI)
        /// </summary>
        public float GetMoneyPerHour()
        {
            return GetMoneyPerMinute() * 60f;
        }

        /// <summary>
        /// Calculates money earned over a duration (for offline calculation)
        /// </summary>
        public float CalculateMoneyEarned(TimeSpan duration)
        {
            float total = 0;
            foreach (var line in productionLines.Where(l => l.isActive))
            {
                total += line.CalculateEarnings(duration);
            }
            return total * GetTotalMultiplier();
        }

        /// <summary>
        /// Calculates offline production with detailed breakdown
        /// </summary>
        public Dictionary<string, float> CalculateOfflineProduction(TimeSpan duration)
        {
            var contributions = new Dictionary<string, float>();
            float totalMultiplier = GetTotalMultiplier();
            
            foreach (var line in productionLines.Where(l => l.isActive))
            {
                float earnings = line.SimulateOfflineProduction(duration) * totalMultiplier;
                contributions[line.lineId] = earnings;
            }
            
            return contributions;
        }

        /// <summary>
        /// Gets the total production multiplier
        /// </summary>
        public float GetTotalMultiplier()
        {
            return globalProductionMultiplier * premiumMultiplier;
        }

        private void RecalculateTotals()
        {
            _totalMoneyPerMinute = 0;
            foreach (var line in productionLines.Where(l => l.isActive))
            {
                _totalMoneyPerMinute += line.GetEffectiveRate();
            }
            _totalMoneyPerMinute *= GetTotalMultiplier();
            _isDirty = false;
        }

        private void ProcessActiveProduction()
        {
            float totalProduced = 0;
            foreach (var line in productionLines.Where(l => l.isActive))
            {
                // Calculate production for this tick
                float tickProduction = line.GetEffectiveRate() * GetTotalMultiplier() * (productionTickInterval / 60f);
                if (tickProduction > 0)
                {
                    totalProduced += tickProduction;
                    line.lifetimeEarnings += tickProduction;
                }
            }
            
            if (totalProduced > 0)
            {
                OnMoneyProduced?.Invoke(totalProduced);
            }
        }

        private void OnLineProductionTick(ProductionLine line, float amount)
        {
            // This is called for offline production simulation
        }

        #endregion

        #region Multiplier Management

        /// <summary>
        /// Sets the global production multiplier
        /// </summary>
        public void SetGlobalMultiplier(float multiplier)
        {
            globalProductionMultiplier = Mathf.Max(0, multiplier);
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
        }

        /// <summary>
        /// Sets the premium/booster multiplier
        /// </summary>
        public void SetPremiumMultiplier(float multiplier)
        {
            premiumMultiplier = Mathf.Max(0, multiplier);
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
        }

        /// <summary>
        /// Multiplies the global multiplier
        /// </summary>
        public void MultiplyGlobalMultiplier(float factor)
        {
            globalProductionMultiplier *= factor;
            globalProductionMultiplier = Mathf.Max(0, globalProductionMultiplier);
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
        }

        #endregion

        #region Line State Management

        /// <summary>
        /// Activates all production lines
        /// </summary>
        public void ActivateAllLines()
        {
            foreach (var line in productionLines)
            {
                line.Activate();
            }
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
        }

        /// <summary>
        /// Deactivates all production lines
        /// </summary>
        public void DeactivateAllLines()
        {
            foreach (var line in productionLines)
            {
                line.Deactivate();
            }
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
        }

        /// <summary>
        /// Sets efficiency for all lines of a specific drone model
        /// </summary>
        public void SetEfficiencyForModel(string droneModelId, float efficiency)
        {
            foreach (var line in productionLines.Where(l => l.droneModelId == droneModelId))
            {
                line.SetEfficiency(efficiency);
            }
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
        }

        /// <summary>
        /// Upgrades a production line's base rate
        /// </summary>
        public bool UpgradeLine(string lineId, float newRate)
        {
            var line = GetLine(lineId);
            if (line == null) return false;
            
            line.baseProductionRate = newRate;
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
            return true;
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Gets save data for all production lines
        /// </summary>
        public List<ProductionLineSaveData> GetSaveData()
        {
            return productionLines.Select(l => new ProductionLineSaveData
            {
                lineId = l.lineId,
                droneModelId = l.droneModelId,
                baseProductionRate = l.baseProductionRate,
                efficiencyMultiplier = l.efficiencyMultiplier,
                isActive = l.isActive,
                startTime = l.startTime,
                totalProductionMinutes = l.totalProductionMinutes,
                partialProgress = l.partialProgress,
                lifetimeEarnings = l.lifetimeEarnings
            }).ToList();
        }

        /// <summary>
        /// Loads production lines from save data
        /// </summary>
        public void LoadFromSaveData(List<ProductionLineSaveData> saveData)
        {
            productionLines.Clear();
            
            foreach (var data in saveData)
            {
                var line = new ProductionLine(data.droneModelId, data.baseProductionRate, data.lineId)
                {
                    efficiencyMultiplier = data.efficiencyMultiplier,
                    isActive = data.isActive,
                    startTime = data.startTime,
                    totalProductionMinutes = data.totalProductionMinutes,
                    partialProgress = data.partialProgress,
                    lifetimeEarnings = data.lifetimeEarnings
                };
                
                productionLines.Add(line);
                line.OnProductionTick += OnLineProductionTick;
            }
            
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
            
            Debug.Log($"[ProductionManager] Loaded {productionLines.Count} production lines");
        }

        #endregion

        #region Debug/Testing

        /// <summary>
        /// Gets debug info for all lines
        /// </summary>
        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Production Manager ===");
            sb.AppendLine($"Total Money/Min: {GetMoneyPerMinute():F2}");
            sb.AppendLine($"Global Multiplier: {globalProductionMultiplier:F2}");
            sb.AppendLine($"Premium Multiplier: {premiumMultiplier:F2}");
            sb.AppendLine($"Active Lines: {GetActiveLines().Count}/{productionLines.Count}");
            sb.AppendLine("\nProduction Lines:");
            
            foreach (var line in productionLines)
            {
                sb.AppendLine($"  {line.GetDisplayName()}: {line.GetEffectiveRate():F2}/min (Active: {line.isActive})");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Clears all production lines (for testing)
        /// </summary>
        public void ClearAllLines()
        {
            foreach (var line in productionLines)
            {
                line.OnProductionTick -= OnLineProductionTick;
            }
            productionLines.Clear();
            _isDirty = true;
            OnProductionRatesChanged?.Invoke();
        }

        #endregion
    }

    /// <summary>
    /// Serializable data for saving a production line
    /// </summary>
    [Serializable]
    public class ProductionLineSaveData
    {
        public string lineId;
        public string droneModelId;
        public float baseProductionRate;
        public float efficiencyMultiplier;
        public bool isActive;
        public DateTime startTime;
        public float totalProductionMinutes;
        public float partialProgress;
        public float lifetimeEarnings;
    }
}
