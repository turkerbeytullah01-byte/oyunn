using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis
{
    /// <summary>
    /// Drone classification types
    /// </summary>
    public enum DroneClass
    {
        Surveillance,   // Focus on detection, range, stealth
        Security,       // Focus on patrol, response, deterrence
        Reconnaissance, // Focus on speed, range, data collection
        Combat,         // Focus on attack, defense (future)
        Support,        // Focus on repair, resupply (future)
        Industrial      // Focus on cargo, construction (future)
    }

    /// <summary>
    /// Drone size categories affecting production and capabilities
    /// </summary>
    public enum DroneSize
    {
        Mini,       // Fast, cheap, limited capabilities
        Small,      // Balanced for basic operations
        Medium,     // Standard size, good all-rounder
        Large,      // Heavy duty, high capacity
        Heavy       // Maximum capacity, slow (future)
    }

    /// <summary>
    /// Complete stat block for a drone
    /// </summary>
    [Serializable]
    public class StatBlock
    {
        [Header("Performance")]
        [Range(1, 100)] public int speed = 50;
        [Range(1, 100)] public int maneuverability = 50;
        [Range(1, 100)] public int stability = 50;
        
        [Header("Systems")]
        [Range(1, 100)] public int detectionRange = 50;
        [Range(1, 100)] public int signalStrength = 50;
        [Range(1, 100)] public int stealth = 50;
        
        [Header("Power")]
        [Range(1, 100)] public int batteryCapacity = 50;
        [Range(1, 100)] public int powerEfficiency = 50;
        [Range(1, 100)] public int heatDissipation = 50;
        
        [Header("Durability")]
        [Range(1, 100)] public int armor = 50;
        [Range(1, 100)] public int reliability = 50;
        [Range(1, 100)] public int repairEase = 50;
        
        [Header("Payload")]
        [Range(1, 100)] public int cargoCapacity = 50;
        [Range(1, 100)] public int sensorQuality = 50;
        [Range(1, 100)] public int processingPower = 50;

        /// <summary>
        /// Calculate overall performance score
        /// </summary>
        public float GetOverallScore()
        {
            return (speed + maneuverability + stability + 
                    detectionRange + signalStrength + stealth +
                    batteryCapacity + powerEfficiency + heatDissipation +
                    armor + reliability + repairEase +
                    cargoCapacity + sensorQuality + processingPower) / 15f;
        }

        /// <summary>
        /// Get score for a specific category
        /// </summary>
        public float GetCategoryScore(string category)
        {
            return category.ToLower() switch
            {
                "performance" => (speed + maneuverability + stability) / 3f,
                "systems" => (detectionRange + signalStrength + stealth) / 3f,
                "power" => (batteryCapacity + powerEfficiency + heatDissipation) / 3f,
                "durability" => (armor + reliability + repairEase) / 3f,
                "payload" => (cargoCapacity + sensorQuality + processingPower) / 3f,
                _ => GetOverallScore()
            };
        }

        /// <summary>
        /// Create a copy of this stat block
        /// </summary>
        public StatBlock Clone()
        {
            return new StatBlock
            {
                speed = this.speed,
                maneuverability = this.maneuverability,
                stability = this.stability,
                detectionRange = this.detectionRange,
                signalStrength = this.signalStrength,
                stealth = this.stealth,
                batteryCapacity = this.batteryCapacity,
                powerEfficiency = this.powerEfficiency,
                heatDissipation = this.heatDissipation,
                armor = this.armor,
                reliability = this.reliability,
                repairEase = this.repairEase,
                cargoCapacity = this.cargoCapacity,
                sensorQuality = this.sensorQuality,
                processingPower = this.processingPower
            };
        }

        /// <summary>
        /// Apply multipliers from tech bonuses
        /// </summary>
        public void ApplyMultipliers(Dictionary<string, float> multipliers)
        {
            if (multipliers.ContainsKey("speed")) speed = Mathf.RoundToInt(speed * multipliers["speed"]);
            if (multipliers.ContainsKey("maneuverability")) maneuverability = Mathf.RoundToInt(maneuverability * multipliers["maneuverability"]);
            if (multipliers.ContainsKey("stability")) stability = Mathf.RoundToInt(stability * multipliers["stability"]);
            if (multipliers.ContainsKey("detectionRange")) detectionRange = Mathf.RoundToInt(detectionRange * multipliers["detectionRange"]);
            if (multipliers.ContainsKey("signalStrength")) signalStrength = Mathf.RoundToInt(signalStrength * multipliers["signalStrength"]);
            if (multipliers.ContainsKey("stealth")) stealth = Mathf.RoundToInt(stealth * multipliers["stealth"]);
            if (multipliers.ContainsKey("batteryCapacity")) batteryCapacity = Mathf.RoundToInt(batteryCapacity * multipliers["batteryCapacity"]);
            if (multipliers.ContainsKey("powerEfficiency")) powerEfficiency = Mathf.RoundToInt(powerEfficiency * multipliers["powerEfficiency"]);
            if (multipliers.ContainsKey("heatDissipation")) heatDissipation = Mathf.RoundToInt(heatDissipation * multipliers["heatDissipation"]);
            if (multipliers.ContainsKey("armor")) armor = Mathf.RoundToInt(armor * multipliers["armor"]);
            if (multipliers.ContainsKey("reliability")) reliability = Mathf.RoundToInt(reliability * multipliers["reliability"]);
            if (multipliers.ContainsKey("repairEase")) repairEase = Mathf.RoundToInt(repairEase * multipliers["repairEase"]);
            if (multipliers.ContainsKey("cargoCapacity")) cargoCapacity = Mathf.RoundToInt(cargoCapacity * multipliers["cargoCapacity"]);
            if (multipliers.ContainsKey("sensorQuality")) sensorQuality = Mathf.RoundToInt(sensorQuality * multipliers["sensorQuality"]);
            if (multipliers.ContainsKey("processingPower")) processingPower = Mathf.RoundToInt(processingPower * multipliers["processingPower"]);
        }
    }

    /// <summary>
    /// ScriptableObject defining a drone type
    /// </summary>
    [CreateAssetMenu(fileName = "NewDrone", menuName = "Project Aegis/Drone Data")]
    public class DroneData : ScriptableObject
    {
        [Header("Basic Info")]
        public string droneId;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public DroneClass droneClass;
        public DroneSize droneSize;
        
        [Header("Requirements")]
        public List<string> requiredTechnologies = new List<string>();
        public int requiredReputationLevel = 0; // 0-4 (Unknown to Legendary)
        
        [Header("Stats")]
        public StatBlock baseStats;
        
        [Header("Production")]
        public float productionTime = 60f;      // In seconds
        public float productionCost = 1000f;
        public int productionBatchSize = 1;
        
        [Header("Economy")]
        public float basePrice = 2000f;
        public float maintenanceCostPerHour = 10f;
        
        [Header("Visual")]
        public Sprite droneIcon;
        public GameObject dronePrefab;
        public Color droneColor = Color.white;
        
        [Header("Audio")]
        public AudioClip engineSound;
        public AudioClip alertSound;
        
        [Header("Unlock")]
        public bool startsUnlocked = false;
        public float unlockCost = 0f;

        /// <summary>
        /// Check if all requirements are met to unlock this drone
        /// </summary>
        public bool CanUnlock()
        {
            // Check technologies
            if (TechTreeManager.HasInstance)
            {
                foreach (string techId in requiredTechnologies)
                {
                    if (!TechTreeManager.Instance.IsTechnologyUnlocked(techId))
                        return false;
                }
            }
            
            // Check reputation
            if (ReputationManager.HasInstance)
            {
                int currentRepLevel = (int)ReputationManager.Instance.GetReputationLevel();
                if (currentRepLevel < requiredReputationLevel)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Get final price with reputation multiplier
        /// </summary>
        public float GetFinalPrice()
        {
            float multiplier = 1f;
            if (ReputationManager.HasInstance)
            {
                multiplier = ReputationManager.Instance.GetPriceMultiplier();
            }
            return basePrice * multiplier;
        }

        /// <summary>
        /// Get production time with tech bonuses
        /// </summary>
        public float GetProductionTime()
        {
            float time = productionTime;
            
            // Apply production speed tech bonuses
            if (TechTreeManager.HasInstance)
            {
                if (TechTreeManager.Instance.IsTechnologyUnlocked("assembly_automation"))
                    time *= 0.9f;
                if (TechTreeManager.Instance.IsTechnologyUnlocked("advanced_manufacturing"))
                    time *= 0.85f;
            }
            
            return time;
        }

        /// <summary>
        /// Get effective stats with all bonuses applied
        /// </summary>
        public StatBlock GetEffectiveStats()
        {
            StatBlock stats = baseStats.Clone();
            
            // Apply tech bonuses
            Dictionary<string, float> multipliers = new Dictionary<string, float>();
            
            if (TechTreeManager.HasInstance)
            {
                // Performance techs
                if (TechTreeManager.Instance.IsTechnologyUnlocked("advanced_propulsion"))
                    multipliers["speed"] = 1.15f;
                if (TechTreeManager.Instance.IsTechnologyUnlocked("flight_stabilization"))
                    multipliers["stability"] = 1.2f;
                    
                // Systems techs
                if (TechTreeManager.Instance.IsTechnologyUnlocked("signal_boosting"))
                    multipliers["signalStrength"] = 1.15f;
                if (TechTreeManager.Instance.IsTechnologyUnlocked("stealth_coating"))
                    multipliers["stealth"] = 1.2f;
                    
                // Power techs
                if (TechTreeManager.Instance.IsTechnologyUnlocked("efficient_cooling"))
                    multipliers["heatDissipation"] = 1.15f;
                if (TechTreeManager.Instance.IsTechnologyUnlocked("advanced_capacitors"))
                    multipliers["batteryCapacity"] = 1.2f;
                    
                // Durability techs
                if (TechTreeManager.Instance.IsTechnologyUnlocked("composite_armor"))
                    multipliers["armor"] = 1.15f;
                    if (TechTreeManager.Instance.IsTechnologyUnlocked("redundant_systems"))
                    multipliers["reliability"] = 1.2f;
            }
            
            stats.ApplyMultipliers(multipliers);
            return stats;
        }

        /// <summary>
        /// Get class-specific description
        /// </summary>
        public string GetClassDescription()
        {
            return droneClass switch
            {
                DroneClass.Surveillance => "Optimized for detection and monitoring operations",
                DroneClass.Security => "Designed for patrol and security enforcement",
                DroneClass.Reconnaissance => "Built for rapid intelligence gathering",
                DroneClass.Combat => "Armed for defensive and offensive operations",
                DroneClass.Support => "Equipped for repair and resupply missions",
                DroneClass.Industrial => "Configured for cargo and construction tasks",
                _ => "Multi-purpose drone platform"
            };
        }

        private void OnValidate()
        {
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(droneId))
            {
                droneId = Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
    }
}
