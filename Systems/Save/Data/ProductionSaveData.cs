using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Save.Data
{
    /// <summary>
    /// Production system save data
    /// </summary>
    [Serializable]
    public class ProductionSaveData
    {
        // Production lines
        public List<ProductionLineSaveData> productionLines;
        
        // Unlocked drone models
        public List<string> unlockedDroneModels;
        
        // Inventory
        public List<DroneInventoryEntry> droneInventory;
        
        // Statistics
        public float totalMoneyEarned;
        public float totalDronesProduced;
        public float totalDronesSold;
        public float totalProductionTimeHours;
        
        // Production modifiers
        public float globalEfficiencyMultiplier;
        public float globalQualityMultiplier;
        
        // Market data
        public List<MarketPriceEntry> marketPrices;
        
        /// <summary>
        /// Creates default production data for new game
        /// </summary>
        public static ProductionSaveData CreateDefault()
        {
            return new ProductionSaveData
            {
                productionLines = new List<ProductionLineSaveData>(),
                unlockedDroneModels = new List<string> { "basic_scout" },
                droneInventory = new List<DroneInventoryEntry>(),
                totalMoneyEarned = 0,
                totalDronesProduced = 0,
                totalDronesSold = 0,
                totalProductionTimeHours = 0,
                globalEfficiencyMultiplier = 1f,
                globalQualityMultiplier = 1f,
                marketPrices = new List<MarketPriceEntry>()
            };
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            productionLines ??= new List<ProductionLineSaveData>();
            unlockedDroneModels ??= new List<string>();
            droneInventory ??= new List<DroneInventoryEntry>();
            marketPrices ??= new List<MarketPriceEntry>();
            
            totalMoneyEarned = Mathf.Max(0, totalMoneyEarned);
            totalDronesProduced = Mathf.Max(0, totalDronesProduced);
            totalDronesSold = Mathf.Max(0, totalDronesSold);
            totalProductionTimeHours = Mathf.Max(0, totalProductionTimeHours);
            globalEfficiencyMultiplier = Mathf.Max(0.1f, globalEfficiencyMultiplier);
            globalQualityMultiplier = Mathf.Max(0.1f, globalQualityMultiplier);
            
            // Clean up null entries
            unlockedDroneModels.RemoveAll(string.IsNullOrEmpty);
            
            // Validate production lines
            foreach (var line in productionLines)
            {
                line?.Validate();
            }
            
            // Validate inventory
            foreach (var entry in droneInventory)
            {
                entry?.Validate();
            }
            
            // Remove invalid entries
            productionLines.RemoveAll(l => l == null || string.IsNullOrEmpty(l.droneModelId));
            droneInventory.RemoveAll(i => i == null || string.IsNullOrEmpty(i.droneModelId));
        }
        
        /// <summary>
        /// Gets inventory count for a drone model
        /// </summary>
        public int GetInventoryCount(string droneModelId)
        {
            foreach (var entry in droneInventory)
            {
                if (entry.droneModelId == droneModelId)
                {
                    return entry.count;
                }
            }
            return 0;
        }
        
        /// <summary>
        /// Adds drones to inventory
        /// </summary>
        public void AddToInventory(string droneModelId, int count)
        {
            if (string.IsNullOrEmpty(droneModelId) || count <= 0)
                return;
                
            foreach (var entry in droneInventory)
            {
                if (entry.droneModelId == droneModelId)
                {
                    entry.count += count;
                    return;
                }
            }
            
            // Add new entry
            droneInventory.Add(new DroneInventoryEntry
            {
                droneModelId = droneModelId,
                count = count,
                averageQuality = 0.5f
            });
        }
        
        /// <summary>
        /// Checks if a drone model is unlocked
        /// </summary>
        public bool IsDroneModelUnlocked(string droneModelId)
        {
            return !string.IsNullOrEmpty(droneModelId) && unlockedDroneModels.Contains(droneModelId);
        }
        
        /// <summary>
        /// Unlocks a drone model
        /// </summary>
        public void UnlockDroneModel(string droneModelId)
        {
            if (!string.IsNullOrEmpty(droneModelId) && !unlockedDroneModels.Contains(droneModelId))
            {
                unlockedDroneModels.Add(droneModelId);
            }
        }
    }
    
    /// <summary>
    /// Production line save data
    /// </summary>
    [Serializable]
    public class ProductionLineSaveData
    {
        // Unique ID for this production line
        public string lineId;
        
        // Drone model being produced
        public string droneModelId;
        
        // Efficiency multiplier
        public float efficiencyMultiplier;
        
        // Quality multiplier
        public float qualityMultiplier;
        
        // Is line active
        public bool isActive;
        
        // Is line paused
        public bool isPaused;
        
        // When production started
        public string startTimeSerialized;
        
        // Production progress (0-1)
        public float productionProgress;
        
        // Units produced this session
        public int unitsProduced;
        
        // Units in queue
        public int queueCount;
        
        // Assigned workers
        public int assignedWorkers;
        
        // Line level/upgrades
        public int lineLevel;
        public List<string> installedUpgrades;
        
        // Temporary DateTime field
        [NonSerialized]
        public DateTime startTime;
        
        /// <summary>
        /// Serializes DateTime fields
        /// </summary>
        public void SerializeDateTime()
        {
            startTimeSerialized = SerializationHelper.DateTimeToString(startTime);
        }
        
        /// <summary>
        /// Deserializes DateTime fields
        /// </summary>
        public void DeserializeDateTime()
        {
            startTime = SerializationHelper.StringToDateTime(startTimeSerialized);
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            efficiencyMultiplier = Mathf.Max(0.1f, efficiencyMultiplier);
            qualityMultiplier = Mathf.Max(0.1f, qualityMultiplier);
            productionProgress = Mathf.Clamp01(productionProgress);
            unitsProduced = Mathf.Max(0, unitsProduced);
            queueCount = Mathf.Max(0, queueCount);
            assignedWorkers = Mathf.Max(0, assignedWorkers);
            lineLevel = Mathf.Max(1, lineLevel);
            
            installedUpgrades ??= new List<string>();
            installedUpgrades.RemoveAll(string.IsNullOrEmpty);
            
            if (string.IsNullOrEmpty(lineId))
            {
                lineId = Guid.NewGuid().ToString();
            }
        }
    }
    
    /// <summary>
    /// Drone inventory entry
    /// </summary>
    [Serializable]
    public class DroneInventoryEntry
    {
        public string droneModelId;
        public int count;
        public float averageQuality;
        
        public void Validate()
        {
            count = Mathf.Max(0, count);
            averageQuality = Mathf.Clamp01(averageQuality);
        }
    }
    
    /// <summary>
    /// Market price entry
    /// </summary>
    [Serializable]
    public class MarketPriceEntry
    {
        public string droneModelId;
        public float basePrice;
        public float currentPrice;
        public float demandMultiplier;
        public string lastUpdateSerialized;
        
        [NonSerialized]
        public DateTime lastUpdate;
        
        public void SerializeDateTime()
        {
            lastUpdateSerialized = SerializationHelper.DateTimeToString(lastUpdate);
        }
        
        public void DeserializeDateTime()
        {
            lastUpdate = SerializationHelper.StringToDateTime(lastUpdateSerialized);
        }
        
        public void Validate()
        {
            basePrice = Mathf.Max(0, basePrice);
            currentPrice = Mathf.Max(0, currentPrice);
            demandMultiplier = Mathf.Max(0.1f, demandMultiplier);
        }
    }
}
