// ============================================================================
// Project Aegis: Drone Dominion
// PlayerData - Core player progression and resource data
// ============================================================================
// Contains all player-related data including resources, progress tracking,
// and statistics. Implements ISaveable for persistence.
// ============================================================================

using UnityEngine;
using System;
using ProjectAegis.Core;

namespace ProjectAegis.Data
{
    /// <summary>
    /// Contains all player-related data and resources.
    /// This is the primary data structure for save/load operations.
    /// </summary>
    [Serializable]
    public class PlayerData : ISaveable
    {
        #region ISaveable Implementation
        
        public string SaveKey => "PlayerData";
        public int SaveVersion => 1;
        
        public object CaptureState()
        {
            return new PlayerDataSnapshot
            {
                Money = Money,
                ResearchPoints = ResearchPoints,
                Reputation = Reputation,
                Energy = Energy,
                Materials = Materials,
                
                TotalMoneyEarned = TotalMoneyEarned,
                TotalResearchPointsEarned = TotalResearchPointsEarned,
                TotalReputationEarned = TotalReputationEarned,
                
                DronesProduced = DronesProduced,
                DronesSold = DronesSold,
                ContractsCompleted = ContractsCompleted,
                ContractsFailed = ContractsFailed,
                ResearchProjectsCompleted = ResearchProjectsCompleted,
                
                PlayTimeSeconds = PlayTimeSeconds,
                SessionCount = SessionCount,
                FirstPlayDate = FirstPlayDate,
                LastPlayDate = LastPlayDate,
                
                UnlockedDroneIds = UnlockedDroneIds,
                UnlockedTechnologyIds = UnlockedTechnologyIds,
                CompletedResearchIds = CompletedResearchIds,
                ActiveContractIds = ActiveContractIds,
                CompletedContractIds = CompletedContractIds,
                
                Version = SaveVersion
            };
        }
        
        public void RestoreState(object state)
        {
            if (state is PlayerDataSnapshot snapshot)
            {
                Money = snapshot.Money;
                ResearchPoints = snapshot.ResearchPoints;
                Reputation = snapshot.Reputation;
                Energy = snapshot.Energy;
                Materials = snapshot.Materials;
                
                TotalMoneyEarned = snapshot.TotalMoneyEarned;
                TotalResearchPointsEarned = snapshot.TotalResearchPointsEarned;
                TotalReputationEarned = snapshot.TotalReputationEarned;
                
                DronesProduced = snapshot.DronesProduced;
                DronesSold = snapshot.DronesSold;
                ContractsCompleted = snapshot.ContractsCompleted;
                ContractsFailed = snapshot.ContractsFailed;
                ResearchProjectsCompleted = snapshot.ResearchProjectsCompleted;
                
                PlayTimeSeconds = snapshot.PlayTimeSeconds;
                SessionCount = snapshot.SessionCount;
                FirstPlayDate = snapshot.FirstPlayDate;
                LastPlayDate = snapshot.LastPlayDate;
                
                UnlockedDroneIds = snapshot.UnlockedDroneIds ?? new System.Collections.Generic.List<string>();
                UnlockedTechnologyIds = snapshot.UnlockedTechnologyIds ?? new System.Collections.Generic.List<string>();
                CompletedResearchIds = snapshot.CompletedResearchIds ?? new System.Collections.Generic.List<string>();
                ActiveContractIds = snapshot.ActiveContractIds ?? new System.Collections.Generic.List<string>();
                CompletedContractIds = snapshot.CompletedContractIds ?? new System.Collections.Generic.List<string>();
            }
        }
        
        #endregion
        
        #region Resources
        
        /// <summary>
        /// Current money balance.
        /// </summary>
        public long Money { get; private set; }
        
        /// <summary>
        /// Current research points balance.
        /// </summary>
        public long ResearchPoints { get; private set; }
        
        /// <summary>
        /// Current reputation score.
        /// </summary>
        public int Reputation { get; private set; }
        
        /// <summary>
        /// Current energy available.
        /// </summary>
        public int Energy { get; private set; }
        
        /// <summary>
        /// Current materials inventory.
        /// </summary>
        public int Materials { get; private set; }
        
        #endregion
        
        #region Lifetime Statistics
        
        /// <summary>
        /// Total money earned over all time.
        /// </summary>
        public long TotalMoneyEarned { get; private set; }
        
        /// <summary>
        /// Total research points earned over all time.
        /// </summary>
        public long TotalResearchPointsEarned { get; private set; }
        
        /// <summary>
        /// Total reputation earned over all time.
        /// </summary>
        public int TotalReputationEarned { get; private set; }
        
        /// <summary>
        /// Total drones produced.
        /// </summary>
        public int DronesProduced { get; private set; }
        
        /// <summary>
        /// Total drones sold.
        /// </summary>
        public int DronesSold { get; private set; }
        
        /// <summary>
        /// Total contracts completed successfully.
        /// </summary>
        public int ContractsCompleted { get; private set; }
        
        /// <summary>
        /// Total contracts failed.
        /// </summary>
        public int ContractsFailed { get; private set; }
        
        /// <summary>
        /// Total research projects completed.
        /// </summary>
        public int ResearchProjectsCompleted { get; private set; }
        
        #endregion
        
        #region Session Data
        
        /// <summary>
        /// Total play time in seconds.
        /// </summary>
        public float PlayTimeSeconds { get; set; }
        
        /// <summary>
        /// Number of game sessions played.
        /// </summary>
        public int SessionCount { get; private set; }
        
        /// <summary>
        /// Date of first play session.
        /// </summary>
        public DateTime FirstPlayDate { get; private set; }
        
        /// <summary>
        /// Date of last play session.
        /// </summary>
        public DateTime LastPlayDate { get; private set; }
        
        #endregion
        
        #region Unlock Tracking
        
        /// <summary>
        /// IDs of unlocked drone types.
        /// </summary>
        public System.Collections.Generic.List<string> UnlockedDroneIds { get; private set; } = 
            new System.Collections.Generic.List<string>();
        
        /// <summary>
        /// IDs of unlocked technologies.
        /// </summary>
        public System.Collections.Generic.List<string> UnlockedTechnologyIds { get; private set; } = 
            new System.Collections.Generic.List<string>();
        
        /// <summary>
        /// IDs of completed research projects.
        /// </summary>
        public System.Collections.Generic.List<string> CompletedResearchIds { get; private set; } = 
            new System.Collections.Generic.List<string>();
        
        /// <summary>
        /// IDs of active contracts.
        /// </summary>
        public System.Collections.Generic.List<string> ActiveContractIds { get; private set; } = 
            new System.Collections.Generic.List<string>();
        
        /// <summary>
        /// IDs of completed contracts.
        /// </summary>
        public System.Collections.Generic.List<string> CompletedContractIds { get; private set; } = 
            new System.Collections.Generic.List<string>();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Called when money changes.
        /// Parameters: (long newAmount, long changeAmount)
        /// </summary>
        public event Action<long, long> OnMoneyChanged;
        
        /// <summary>
        /// Called when research points change.
        /// Parameters: (long newAmount, long changeAmount)
        /// </summary>
        public event Action<long, long> OnResearchPointsChanged;
        
        /// <summary>
        /// Called when reputation changes.
        /// Parameters: (int newAmount, int changeAmount)
        /// </summary>
        public event Action<int, int> OnReputationChanged;
        
        /// <summary>
        /// Called when energy changes.
        /// Parameters: (int newAmount, int changeAmount)
        /// </summary>
        public event Action<int, int> OnEnergyChanged;
        
        /// <summary>
        /// Called when materials change.
        /// Parameters: (int newAmount, int changeAmount)
        /// </summary>
        public event Action<int, int> OnMaterialsChanged;
        
        #endregion
        
        #region Resource Modification
        
        /// <summary>
        /// Adds money to the player's balance.
        /// </summary>
        public void AddMoney(long amount, string reason = "")
        {
            if (amount <= 0) return;
            
            Money += amount;
            TotalMoneyEarned += amount;
            
            OnMoneyChanged?.Invoke(Money, amount);
            EventManager.Instance?.TriggerMoneyChanged(Money, amount, reason);
        }
        
        /// <summary>
        /// Removes money from the player's balance.
        /// </summary>
        /// <returns>True if the transaction was successful</returns>
        public bool RemoveMoney(long amount, string reason = "")
        {
            if (amount <= 0) return true;
            if (Money < amount) return false;
            
            Money -= amount;
            
            OnMoneyChanged?.Invoke(Money, -amount);
            EventManager.Instance?.TriggerMoneyChanged(Money, -amount, reason);
            
            return true;
        }
        
        /// <summary>
        /// Checks if the player can afford a cost.
        /// </summary>
        public bool CanAfford(Cost cost)
        {
            return Money >= cost.Money &&
                   ResearchPoints >= cost.ResearchPoints &&
                   Reputation >= cost.Reputation &&
                   Energy >= cost.Energy &&
                   Materials >= cost.Materials;
        }
        
        /// <summary>
        /// Attempts to deduct a cost from player resources.
        /// </summary>
        /// <returns>True if all costs were successfully deducted</returns>
        public bool DeductCost(Cost cost, string reason = "")
        {
            if (!CanAfford(cost)) return false;
            
            RemoveMoney(cost.Money, reason);
            RemoveResearchPoints(cost.ResearchPoints, reason);
            RemoveReputation(cost.Reputation, reason);
            RemoveEnergy(cost.Energy, reason);
            RemoveMaterials(cost.Materials, reason);
            
            return true;
        }
        
        /// <summary>
        /// Adds research points.
        /// </summary>
        public void AddResearchPoints(long amount, string reason = "")
        {
            if (amount <= 0) return;
            
            ResearchPoints += amount;
            TotalResearchPointsEarned += amount;
            
            OnResearchPointsChanged?.Invoke(ResearchPoints, amount);
            EventManager.Instance?.TriggerResearchPointsChanged(ResearchPoints, amount, reason);
        }
        
        /// <summary>
        /// Removes research points.
        /// </summary>
        public bool RemoveResearchPoints(long amount, string reason = "")
        {
            if (amount <= 0) return true;
            if (ResearchPoints < amount) return false;
            
            ResearchPoints -= amount;
            
            OnResearchPointsChanged?.Invoke(ResearchPoints, -amount);
            EventManager.Instance?.TriggerResearchPointsChanged(ResearchPoints, -amount, reason);
            
            return true;
        }
        
        /// <summary>
        /// Adds reputation.
        /// </summary>
        public void AddReputation(int amount, string reason = "")
        {
            if (amount <= 0) return;
            
            Reputation += amount;
            TotalReputationEarned += amount;
            
            OnReputationChanged?.Invoke(Reputation, amount);
            EventManager.Instance?.TriggerReputationChanged(Reputation, amount, reason);
        }
        
        /// <summary>
        /// Removes reputation.
        /// </summary>
        public bool RemoveReputation(int amount, string reason = "")
        {
            if (amount <= 0) return true;
            if (Reputation < amount) return false;
            
            Reputation -= amount;
            
            OnReputationChanged?.Invoke(Reputation, -amount);
            EventManager.Instance?.TriggerReputationChanged(Reputation, -amount, reason);
            
            return true;
        }
        
        /// <summary>
        /// Adds energy.
        /// </summary>
        public void AddEnergy(int amount, string reason = "")
        {
            if (amount <= 0) return;
            
            Energy += amount;
            
            OnEnergyChanged?.Invoke(Energy, amount);
            EventManager.Instance?.TriggerEnergyChanged(Energy, amount, reason);
        }
        
        /// <summary>
        /// Removes energy.
        /// </summary>
        public bool RemoveEnergy(int amount, string reason = "")
        {
            if (amount <= 0) return true;
            if (Energy < amount) return false;
            
            Energy -= amount;
            
            OnEnergyChanged?.Invoke(Energy, -amount);
            EventManager.Instance?.TriggerEnergyChanged(Energy, -amount, reason);
            
            return true;
        }
        
        /// <summary>
        /// Adds materials.
        /// </summary>
        public void AddMaterials(int amount, string reason = "")
        {
            if (amount <= 0) return;
            
            Materials += amount;
            
            OnMaterialsChanged?.Invoke(Materials, amount);
            EventManager.Instance?.TriggerMaterialsChanged(Materials, amount, reason);
        }
        
        /// <summary>
        /// Removes materials.
        /// </summary>
        public bool RemoveMaterials(int amount, string reason = "")
        {
            if (amount <= 0) return true;
            if (Materials < amount) return false;
            
            Materials -= amount;
            
            OnMaterialsChanged?.Invoke(Materials, -amount);
            EventManager.Instance?.TriggerMaterialsChanged(Materials, -amount, reason);
            
            return true;
        }
        
        #endregion
        
        #region Statistics Tracking
        
        /// <summary>
        /// Records a drone production.
        /// </summary>
        public void RecordDroneProduced(int quantity = 1)
        {
            DronesProduced += quantity;
        }
        
        /// <summary>
        /// Records a drone sale.
        /// </summary>
        public void RecordDroneSold(int quantity = 1)
        {
            DronesSold += quantity;
        }
        
        /// <summary>
        /// Records a completed contract.
        /// </summary>
        public void RecordContractCompleted(string contractId)
        {
            ContractsCompleted++;
            ActiveContractIds.Remove(contractId);
            if (!CompletedContractIds.Contains(contractId))
            {
                CompletedContractIds.Add(contractId);
            }
        }
        
        /// <summary>
        /// Records a failed contract.
        /// </summary>
        public void RecordContractFailed(string contractId)
        {
            ContractsFailed++;
            ActiveContractIds.Remove(contractId);
        }
        
        /// <summary>
        /// Records a completed research project.
        /// </summary>
        public void RecordResearchCompleted(string researchId)
        {
            ResearchProjectsCompleted++;
            if (!CompletedResearchIds.Contains(researchId))
            {
                CompletedResearchIds.Add(researchId);
            }
        }
        
        #endregion
        
        #region Unlock Management
        
        /// <summary>
        /// Unlocks a drone type.
        /// </summary>
        public void UnlockDrone(string droneId)
        {
            if (!UnlockedDroneIds.Contains(droneId))
            {
                UnlockedDroneIds.Add(droneId);
                EventManager.Instance?.TriggerDroneUnlocked(droneId, droneId);
            }
        }
        
        /// <summary>
        /// Checks if a drone type is unlocked.
        /// </summary>
        public bool IsDroneUnlocked(string droneId)
        {
            return UnlockedDroneIds.Contains(droneId);
        }
        
        /// <summary>
        /// Unlocks a technology.
        /// </summary>
        public void UnlockTechnology(string techId)
        {
            if (!UnlockedTechnologyIds.Contains(techId))
            {
                UnlockedTechnologyIds.Add(techId);
                EventManager.Instance?.TriggerTechnologyUnlocked(techId, techId);
            }
        }
        
        /// <summary>
        /// Checks if a technology is unlocked.
        /// </summary>
        public bool IsTechnologyUnlocked(string techId)
        {
            return UnlockedTechnologyIds.Contains(techId);
        }
        
        /// <summary>
        /// Adds an active contract.
        /// </summary>
        public void AddActiveContract(string contractId)
        {
            if (!ActiveContractIds.Contains(contractId))
            {
                ActiveContractIds.Add(contractId);
            }
        }
        
        #endregion
        
        #region Session Management
        
        /// <summary>
        /// Called when a new session starts.
        /// </summary>
        public void OnSessionStart()
        {
            SessionCount++;
            
            if (FirstPlayDate == default)
            {
                FirstPlayDate = DateTime.UtcNow;
            }
            
            LastPlayDate = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Called when the session ends.
        /// </summary>
        public void OnSessionEnd()
        {
            LastPlayDate = DateTime.UtcNow;
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Gets formatted play time string.
        /// </summary>
        public string GetFormattedPlayTime()
        {
            var timeSpan = TimeSpan.FromSeconds(PlayTimeSeconds);
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        
        /// <summary>
        /// Creates a new PlayerData with default starting values.
        /// </summary>
        public static PlayerData CreateDefault()
        {
            return new PlayerData
            {
                Money = 10000, // Starting money
                ResearchPoints = 0,
                Reputation = 10, // Starting reputation
                Energy = 100, // Starting energy
                Materials = 50, // Starting materials
                FirstPlayDate = DateTime.UtcNow,
                LastPlayDate = DateTime.UtcNow
            };
        }
        
        #endregion
    }
    
    #region Snapshot for Serialization
    
    /// <summary>
    /// Serializable snapshot of PlayerData for save/load operations.
    /// </summary>
    [Serializable]
    public struct PlayerDataSnapshot
    {
        public long Money;
        public long ResearchPoints;
        public int Reputation;
        public int Energy;
        public int Materials;
        
        public long TotalMoneyEarned;
        public long TotalResearchPointsEarned;
        public int TotalReputationEarned;
        
        public int DronesProduced;
        public int DronesSold;
        public int ContractsCompleted;
        public int ContractsFailed;
        public int ResearchProjectsCompleted;
        
        public float PlayTimeSeconds;
        public int SessionCount;
        public DateTime FirstPlayDate;
        public DateTime LastPlayDate;
        
        public System.Collections.Generic.List<string> UnlockedDroneIds;
        public System.Collections.Generic.List<string> UnlockedTechnologyIds;
        public System.Collections.Generic.List<string> CompletedResearchIds;
        public System.Collections.Generic.List<string> ActiveContractIds;
        public System.Collections.Generic.List<string> CompletedContractIds;
        
        public int Version;
    }
    
    #endregion
}
