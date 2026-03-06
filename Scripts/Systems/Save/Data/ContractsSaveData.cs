using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Save.Data
{
    /// <summary>
    /// Contract system save data
    /// </summary>
    [Serializable]
    public class ContractsSaveData
    {
        // Active contracts
        public List<ActiveContractSaveData> activeContracts;
        
        // Available contract IDs (not yet accepted)
        public List<string> availableContractIds;
        
        // Completed contract IDs
        public List<string> completedContractIds;
        
        // Failed contract IDs
        public List<string> failedContractIds;
        
        // Statistics
        public float totalContractsCompleted;
        public float totalContractsFailed;
        public float totalMoneyEarnedFromContracts;
        public float totalReputationEarnedFromContracts;
        
        // Contract refresh timer
        public float timeUntilNextRefresh;
        
        // Contract generation seed for consistency
        public int contractSeed;
        
        /// <summary>
        /// Creates default contracts data for new game
        /// </summary>
        public static ContractsSaveData CreateDefault()
        {
            return new ContractsSaveData
            {
                activeContracts = new List<ActiveContractSaveData>(),
                availableContractIds = new List<string>(),
                completedContractIds = new List<string>(),
                failedContractIds = new List<string>(),
                totalContractsCompleted = 0,
                totalContractsFailed = 0,
                totalMoneyEarnedFromContracts = 0,
                totalReputationEarnedFromContracts = 0,
                timeUntilNextRefresh = 300f, // 5 minutes
                contractSeed = UnityEngine.Random.Range(0, int.MaxValue)
            };
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            activeContracts ??= new List<ActiveContractSaveData>();
            availableContractIds ??= new List<string>();
            completedContractIds ??= new List<string>();
            failedContractIds ??= new List<string>();
            
            totalContractsCompleted = Mathf.Max(0, totalContractsCompleted);
            totalContractsFailed = Mathf.Max(0, totalContractsFailed);
            totalMoneyEarnedFromContracts = Mathf.Max(0, totalMoneyEarnedFromContracts);
            totalReputationEarnedFromContracts = Mathf.Max(0, totalReputationEarnedFromContracts);
            timeUntilNextRefresh = Mathf.Max(0, timeUntilNextRefresh);
            
            // Clean up null entries
            availableContractIds.RemoveAll(string.IsNullOrEmpty);
            completedContractIds.RemoveAll(string.IsNullOrEmpty);
            failedContractIds.RemoveAll(string.IsNullOrEmpty);
            
            // Validate active contracts
            foreach (var contract in activeContracts)
            {
                contract?.Validate();
            }
            
            // Remove invalid contracts
            activeContracts.RemoveAll(c => c == null || string.IsNullOrEmpty(c.contractId));
        }
        
        /// <summary>
        /// Gets the number of active contracts
        /// </summary>
        public int GetActiveContractCount()
        {
            return activeContracts?.Count ?? 0;
        }
        
        /// <summary>
        /// Checks if a contract is completed
        /// </summary>
        public bool IsContractCompleted(string contractId)
        {
            return !string.IsNullOrEmpty(contractId) && completedContractIds.Contains(contractId);
        }
        
        /// <summary>
        /// Gets success rate percentage
        /// </summary>
        public float GetSuccessRate()
        {
            float total = totalContractsCompleted + totalContractsFailed;
            if (total <= 0) return 100f;
            return (totalContractsCompleted / total) * 100f;
        }
    }
    
    /// <summary>
    /// Contract status enum
    /// </summary>
    public enum ContractStatus
    {
        Pending,        // Waiting for bid
        Accepted,       // Contract accepted, in progress
        InProgress,     // Actively being worked on
        Completed,      // Successfully completed
        Failed,         // Failed to complete
        Expired,        // Time ran out
        Cancelled       // Cancelled by player
    }
    
    /// <summary>
    /// Active contract save data
    /// </summary>
    [Serializable]
    public class ActiveContractSaveData
    {
        // Contract ID
        public string contractId;
        
        // Progress (0-1)
        public float progress;
        
        // Deadline
        public string deadlineSerialized;
        
        // Current status
        public ContractStatus status;
        
        // Bid parameters
        public BidParametersSaveData bidParameters;
        
        // Contract value (may differ from base if negotiated)
        public float negotiatedValue;
        
        // Penalty for failure
        public float failurePenalty;
        
        // Quality requirements
        public float requiredQuality;
        public float currentQuality;
        
        // Resources allocated
        public int allocatedEngineers;
        public int allocatedDrones;
        
        // Start time
        public string startTimeSerialized;
        
        // Temporary DateTime fields
        [NonSerialized]
        public DateTime deadline;
        
        [NonSerialized]
        public DateTime startTime;
        
        /// <summary>
        /// Serializes DateTime fields
        /// </summary>
        public void SerializeDateTimes()
        {
            deadlineSerialized = SerializationHelper.DateTimeToString(deadline);
            startTimeSerialized = SerializationHelper.DateTimeToString(startTime);
        }
        
        /// <summary>
        /// Deserializes DateTime fields
        /// </summary>
        public void DeserializeDateTimes()
        {
            deadline = SerializationHelper.StringToDateTime(deadlineSerialized);
            startTime = SerializationHelper.StringToDateTime(startTimeSerialized);
        }
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            progress = Mathf.Clamp01(progress);
            negotiatedValue = Mathf.Max(0, negotiatedValue);
            failurePenalty = Mathf.Max(0, failurePenalty);
            requiredQuality = Mathf.Clamp01(requiredQuality);
            currentQuality = Mathf.Clamp01(currentQuality);
            allocatedEngineers = Mathf.Max(0, allocatedEngineers);
            allocatedDrones = Mathf.Max(0, allocatedDrones);
            
            bidParameters?.Validate();
            
            // Ensure valid status
            if (!Enum.IsDefined(typeof(ContractStatus), status))
            {
                status = ContractStatus.Pending;
            }
        }
        
        /// <summary>
        /// Gets time remaining until deadline
        /// </summary>
        public TimeSpan GetTimeRemaining()
        {
            return deadline - DateTime.UtcNow;
        }
        
        /// <summary>
        /// Checks if contract is overdue
        /// </summary>
        public bool IsOverdue()
        {
            return DateTime.UtcNow > deadline;
        }
    }
    
    /// <summary>
    /// Bid parameters save data
    /// </summary>
    [Serializable]
    public class BidParametersSaveData
    {
        // Bid amount
        public float bidAmount;
        
        // Estimated completion time
        public float estimatedDays;
        
        // Promised quality
        public float promisedQuality;
        
        // Number of drones offered
        public int dronesOffered;
        
        // Negotiation attempts made
        public int negotiationAttempts;
        
        /// <summary>
        /// Validates and fixes any invalid data
        /// </summary>
        public void Validate()
        {
            bidAmount = Mathf.Max(0, bidAmount);
            estimatedDays = Mathf.Max(0.1f, estimatedDays);
            promisedQuality = Mathf.Clamp01(promisedQuality);
            dronesOffered = Mathf.Max(0, dronesOffered);
            negotiationAttempts = Mathf.Max(0, negotiationAttempts);
        }
    }
}
