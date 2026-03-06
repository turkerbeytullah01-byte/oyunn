using System;
using UnityEngine;

namespace ProjectAegis.Systems.Save.Data
{
    /// <summary>
    /// Main game save data container
    /// This is the root object that gets serialized/deserialized
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        // Save metadata
        public string version;
        public string saveTimestampSerialized;
        public string lastLogoutTimeSerialized;
        public int saveCount;
        public string deviceId;
        
        // Player data
        public PlayerSaveData playerData;
        
        // Research data
        public ResearchSaveData researchData;
        
        // Technology tree data
        public TechTreeSaveData techTreeData;
        
        // Contract data
        public ContractsSaveData contractsData;
        
        // Production data
        public ProductionSaveData productionData;
        
        // Event data
        public EventsSaveData eventsData;
        
        // Statistics
        public StatisticsSaveData statisticsData;
        
        // Offline progress data
        public OfflineProgressData offlineProgressData;
        
        // Data integrity hash
        public string integrityHash;
        
        // Temporary DateTime fields (not serialized)
        [NonSerialized]
        public DateTime saveTimestamp;
        
        [NonSerialized]
        public DateTime lastLogoutTime;
        
        /// <summary>
        /// Creates a new save data with default values
        /// </summary>
        public static GameSaveData CreateNew()
        {
            var saveData = new GameSaveData
            {
                version = SaveConstants.CURRENT_VERSION,
                saveTimestamp = DateTime.UtcNow,
                lastLogoutTime = DateTime.UtcNow,
                saveCount = 0,
                deviceId = SystemInfo.deviceUniqueIdentifier,
                
                playerData = PlayerSaveData.CreateDefault(),
                researchData = ResearchSaveData.CreateDefault(),
                techTreeData = TechTreeSaveData.CreateDefault(),
                contractsData = ContractsSaveData.CreateDefault(),
                productionData = ProductionSaveData.CreateDefault(),
                eventsData = EventsSaveData.CreateDefault(),
                statisticsData = StatisticsSaveData.CreateDefault(),
                offlineProgressData = OfflineProgressData.CreateDefault()
            };
            
            saveData.SerializeDateTimes();
            return saveData;
        }
        
        /// <summary>
        /// Serializes all DateTime fields to strings
        /// Must be called before saving
        /// </summary>
        public void SerializeDateTimes()
        {
            saveTimestampSerialized = SerializationHelper.DateTimeToString(saveTimestamp);
            lastLogoutTimeSerialized = SerializationHelper.DateTimeToString(lastLogoutTime);
            
            // Serialize nested DateTimes
            researchData?.activeResearch?.SerializeDateTime();
            
            foreach (var contract in contractsData?.activeContracts)
            {
                contract?.SerializeDateTimes();
            }
            
            foreach (var line in productionData?.productionLines)
            {
                line?.SerializeDateTime();
            }
            
            foreach (var price in productionData?.marketPrices)
            {
                price?.SerializeDateTime();
            }
            
            eventsData?.SerializeDateTimes();
            statisticsData?.SerializeDateTimes();
            offlineProgressData?.SerializeDateTime();
        }
        
        /// <summary>
        /// Deserializes all DateTime fields from strings
        /// Must be called after loading
        /// </summary>
        public void DeserializeDateTimes()
        {
            saveTimestamp = SerializationHelper.StringToDateTime(saveTimestampSerialized);
            lastLogoutTime = SerializationHelper.StringToDateTime(lastLogoutTimeSerialized);
            
            // Deserialize nested DateTimes
            researchData?.activeResearch?.DeserializeDateTime();
            
            foreach (var contract in contractsData?.activeContracts)
            {
                contract?.DeserializeDateTimes();
            }
            
            foreach (var line in productionData?.productionLines)
            {
                line?.DeserializeDateTime();
            }
            
            foreach (var price in productionData?.marketPrices)
            {
                price?.DeserializeDateTime();
            }
            
            eventsData?.DeserializeDateTimes();
            statisticsData?.DeserializeDateTimes();
            offlineProgressData?.DeserializeDateTime();
        }
        
        /// <summary>
        /// Validates all data and fixes any invalid values
        /// </summary>
        public void Validate()
        {
            // Ensure version is set
            if (string.IsNullOrEmpty(version))
            {
                version = "0.0.0";
            }
            
            saveCount = Mathf.Max(0, saveCount);
            
            // Validate all sub-data
            playerData ??= PlayerSaveData.CreateDefault();
            playerData.Validate();
            
            researchData ??= ResearchSaveData.CreateDefault();
            researchData.Validate();
            
            techTreeData ??= TechTreeSaveData.CreateDefault();
            techTreeData.Validate();
            
            contractsData ??= ContractsSaveData.CreateDefault();
            contractsData.Validate();
            
            productionData ??= ProductionSaveData.CreateDefault();
            productionData.Validate();
            
            eventsData ??= EventsSaveData.CreateDefault();
            eventsData.Validate();
            
            statisticsData ??= StatisticsSaveData.CreateDefault();
            statisticsData.Validate();
            
            offlineProgressData ??= OfflineProgressData.CreateDefault();
            offlineProgressData.Validate();
        }
        
        /// <summary>
        /// Generates integrity hash for the save data
        /// </summary>
        public void GenerateIntegrityHash()
        {
            // Temporarily clear existing hash
            string existingHash = integrityHash;
            integrityHash = null;
            
            // Serialize to JSON
            string json = SerializationHelper.ToJson(this, false);
            
            // Generate hash
            integrityHash = SerializationHelper.GenerateHash(json);
            
            if (SaveConstants.DEBUG_SAVE_OPERATIONS)
            {
                Debug.Log($"[GameSaveData] Generated integrity hash: {integrityHash}");
            }
        }
        
        /// <summary>
        /// Verifies data integrity
        /// </summary>
        public bool VerifyIntegrity()
        {
            if (string.IsNullOrEmpty(integrityHash))
            {
                Debug.LogWarning("[GameSaveData] No integrity hash found");
                return true; // Can't verify, assume valid
            }
            
            // Store existing hash
            string existingHash = integrityHash;
            integrityHash = null;
            
            // Serialize to JSON
            string json = SerializationHelper.ToJson(this, false);
            
            // Restore hash
            integrityHash = existingHash;
            
            // Verify
            bool isValid = SerializationHelper.VerifyIntegrity(json, existingHash);
            
            if (!isValid)
            {
                Debug.LogError("[GameSaveData] Integrity check failed! Save data may be corrupted.");
            }
            else if (SaveConstants.DEBUG_SAVE_OPERATIONS)
            {
                Debug.Log("[GameSaveData] Integrity check passed");
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Gets time since last logout
        /// </summary>
        public TimeSpan GetOfflineDuration()
        {
            return DateTime.UtcNow - lastLogoutTime;
        }
        
        /// <summary>
        /// Gets offline hours (clamped to max)
        /// </summary>
        public float GetOfflineHours()
        {
            float hours = (float)GetOfflineDuration().TotalHours;
            return Mathf.Min(hours, SaveConstants.MAX_OFFLINE_HOURS);
        }
        
        /// <summary>
        /// Prepares for save by updating timestamps and serializing
        /// </summary>
        public void PrepareForSave()
        {
            saveTimestamp = DateTime.UtcNow;
            saveCount++;
            SerializeDateTimes();
            GenerateIntegrityHash();
        }
        
        /// <summary>
        /// Prepares for load by deserializing timestamps
        /// </summary>
        public void PrepareForLoad()
        {
            DeserializeDateTimes();
        }
        
        /// <summary>
        /// Marks logout time
        /// </summary>
        public void MarkLogout()
        {
            lastLogoutTime = DateTime.UtcNow;
            SerializeDateTimes();
        }
    }
    
    /// <summary>
    /// Offline progress calculation data
    /// </summary>
    [Serializable]
    public class OfflineProgressData
    {
        // Resources at logout
        public float moneyAtLogout;
        public float reputationAtLogout;
        
        // Production state at logout
        public List<OfflineProductionEntry> productionEntries;
        
        // Active research at logout
        public string activeResearchIdAtLogout;
        public float researchProgressAtLogout;
        
        // Active contracts at logout
        public List<OfflineContractEntry> contractEntries;
        
        // Whether offline progress was calculated
        public bool offlineProgressCalculated;
        
        // Offline earnings
        public float offlineMoneyEarned;
        public float offlineReputationEarned;
        public float offlineDronesProduced;
        
        // When offline progress was last calculated
        public string lastCalculationTimeSerialized;
        
        [NonSerialized]
        public DateTime lastCalculationTime;
        
        public static OfflineProgressData CreateDefault()
        {
            return new OfflineProgressData
            {
                productionEntries = new List<OfflineProductionEntry>(),
                contractEntries = new List<OfflineContractEntry>(),
                offlineProgressCalculated = false,
                offlineMoneyEarned = 0,
                offlineReputationEarned = 0,
                offlineDronesProduced = 0,
                lastCalculationTime = DateTime.UtcNow
            };
        }
        
        public void SerializeDateTime()
        {
            lastCalculationTimeSerialized = SerializationHelper.DateTimeToString(lastCalculationTime);
        }
        
        public void DeserializeDateTime()
        {
            lastCalculationTime = SerializationHelper.StringToDateTime(lastCalculationTimeSerialized);
        }
        
        public void Validate()
        {
            productionEntries ??= new List<OfflineProductionEntry>();
            contractEntries ??= new List<OfflineContractEntry>();
            
            moneyAtLogout = Mathf.Max(0, moneyAtLogout);
            reputationAtLogout = Mathf.Max(0, reputationAtLogout);
            offlineMoneyEarned = Mathf.Max(0, offlineMoneyEarned);
            offlineReputationEarned = Mathf.Max(0, offlineReputationEarned);
            offlineDronesProduced = Mathf.Max(0, offlineDronesProduced);
        }
    }
    
    /// <summary>
    /// Offline production entry
    /// </summary>
    [Serializable]
    public class OfflineProductionEntry
    {
        public string lineId;
        public string droneModelId;
        public float productionRatePerHour;
        public float efficiencyMultiplier;
        public float progressAtLogout;
    }
    
    /// <summary>
    /// Offline contract entry
    /// </summary>
    [Serializable]
    public class OfflineContractEntry
    {
        public string contractId;
        public float progressAtLogout;
        public string deadlineSerialized;
        public float completionRatePerHour;
        
        [NonSerialized]
        public DateTime deadline;
        
        public void SerializeDateTime()
        {
            deadlineSerialized = SerializationHelper.DateTimeToString(deadline);
        }
        
        public void DeserializeDateTime()
        {
            deadline = SerializationHelper.StringToDateTime(deadlineSerialized);
        }
    }
}
