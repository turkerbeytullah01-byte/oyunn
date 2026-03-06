// ============================================================================
// Project Aegis: Drone Dominion
// GameStateData - Game state and configuration data structures
// ============================================================================
// Contains game state enums, configuration data, and global game settings.
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectAegis.Data
{
    /// <summary>
    /// Global game configuration and settings.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Project Aegis/Game Configuration")]
    public class GameConfig : ScriptableObject
    {
        #region Game Settings
        
        [Header("Game Settings")]
        [SerializeField, Tooltip("Game version string")]
        private string _gameVersion = "1.0.0";
        
        public string GameVersion => _gameVersion;
        
        [SerializeField, Tooltip("Save data version")]
        private int _saveDataVersion = 1;
        
        public int SaveDataVersion => _saveDataVersion;
        
        [SerializeField, Tooltip("Maximum offline hours to calculate")]
        private float _maxOfflineHours = 24f;
        
        public float MaxOfflineHours => _maxOfflineHours;
        
        [SerializeField, Tooltip("Auto-save interval in seconds")]
        private float _autoSaveInterval = 120f;
        
        public float AutoSaveInterval => _autoSaveInterval;
        
        #endregion
        
        #region Starting Values
        
        [Header("Starting Values")]
        [SerializeField, Tooltip("Starting money")]
        private long _startingMoney = 10000;
        
        public long StartingMoney => _startingMoney;
        
        [SerializeField, Tooltip("Starting reputation")]
        private int _startingReputation = 10;
        
        public int StartingReputation => _startingReputation;
        
        [SerializeField, Tooltip("Starting energy")]
        private int _startingEnergy = 100;
        
        public int StartingEnergy => _startingEnergy;
        
        [SerializeField, Tooltip("Starting materials")]
        private int _startingMaterials = 50;
        
        public int StartingMaterials => _startingMaterials;
        
        #endregion
        
        #region Balance Settings
        
        [Header("Balance Settings")]
        [SerializeField, Tooltip("Base research points per second")]
        private float _baseResearchRate = 1f;
        
        public float BaseResearchRate => _baseResearchRate;
        
        [SerializeField, Tooltip("Research rate multiplier per scientist")]
        private float _researchRatePerScientist = 0.5f;
        
        public float ResearchRatePerScientist => _researchRatePerScientist;
        
        [SerializeField, Tooltip("Base energy regeneration per second")]
        private float _baseEnergyRegen = 0.1f;
        
        public float BaseEnergyRegen => _baseEnergyRegen;
        
        [SerializeField, Tooltip("Maximum energy capacity")]
        private int _maxEnergy = 100;
        
        public int MaxEnergy => _maxEnergy;
        
        [SerializeField, Tooltip("Contract generation interval in seconds")]
        private float _contractGenerationInterval = 300f;
        
        public float ContractGenerationInterval => _contractGenerationInterval;
        
        [SerializeField, Tooltip("Maximum active contracts")]
        private int _maxActiveContracts = 5;
        
        public int MaxActiveContracts => _maxActiveContracts;
        
        #endregion
        
        #region Risk Settings
        
        [Header("Risk Settings")]
        [SerializeField, Tooltip("Base risk event chance per hour")]
        private float _baseRiskEventChance = 0.1f;
        
        public float BaseRiskEventChance => _baseRiskEventChance;
        
        [SerializeField, Tooltip="Reputation risk threshold for warnings")]
        private float _reputationRiskThreshold = 0.7f;
        
        public float ReputationRiskThreshold => _reputationRiskThreshold;
        
        [SerializeField, Tooltip("Financial risk threshold for warnings")]
        private float _financialRiskThreshold = 0.7f;
        
        public float FinancialRiskThreshold => _financialRiskThreshold;
        
        #endregion
    }
    
    /// <summary>
    /// Serializable game state for save/load operations.
    /// </summary>
    [Serializable]
    public class GameStateData : ISaveable
    {
        #region ISaveable Implementation
        
        public string SaveKey => "GameState";
        public int SaveVersion => 1;
        
        public object CaptureState()
        {
            return new GameStateSnapshot
            {
                GameVersion = GameVersion,
                SaveDataVersion = SaveVersion,
                CurrentGameState = CurrentGameState,
                SessionStartTime = SessionStartTime,
                TotalPlayTime = TotalPlayTime,
                SaveTimestamp = DateTime.UtcNow,
                
                // Research state
                ActiveResearchId = ActiveResearchId,
                ResearchQueue = ResearchQueue,
                
                // Production state
                ActiveProductionLines = ActiveProductionLines,
                
                // Contract state
                AvailableContractIds = AvailableContractIds,
                
                // Risk state
                CurrentReputationRisk = CurrentReputationRisk,
                CurrentFinancialRisk = CurrentFinancialRisk,
                
                Version = SaveVersion
            };
        }
        
        public void RestoreState(object state)
        {
            if (state is GameStateSnapshot snapshot)
            {
                GameVersion = snapshot.GameVersion;
                CurrentGameState = snapshot.CurrentGameState;
                SessionStartTime = snapshot.SessionStartTime;
                TotalPlayTime = snapshot.TotalPlayTime;
                
                ActiveResearchId = snapshot.ActiveResearchId;
                ResearchQueue = snapshot.ResearchQueue ?? new List<string>();
                ActiveProductionLines = snapshot.ActiveProductionLines ?? new List<ProductionLineState>();
                AvailableContractIds = snapshot.AvailableContractIds ?? new List<string>();
                
                CurrentReputationRisk = snapshot.CurrentReputationRisk;
                CurrentFinancialRisk = snapshot.CurrentFinancialRisk;
            }
        }
        
        #endregion
        
        #region Game Info
        
        /// <summary>
        /// Game version when this save was created.
        /// </summary>
        public string GameVersion { get; set; }
        
        /// <summary>
        /// Current game state.
        /// </summary>
        public Core.GameState CurrentGameState { get; set; }
        
        /// <summary>
        /// When the current session started.
        /// </summary>
        public DateTime SessionStartTime { get; set; }
        
        /// <summary>
        /// Total play time across all sessions.
        /// </summary>
        public float TotalPlayTime { get; set; }
        
        #endregion
        
        #region Research State
        
        /// <summary>
        /// ID of currently active research, if any.
        /// </summary>
        public string ActiveResearchId { get; set; }
        
        /// <summary>
        /// Queue of research IDs waiting to be started.
        /// </summary>
        public List<string> ResearchQueue { get; set; } = new List<string>();
        
        #endregion
        
        #region Production State
        
        /// <summary>
        /// State of all active production lines.
        /// </summary>
        public List<ProductionLineState> ActiveProductionLines { get; set; } = new List<ProductionLineState>();
        
        #endregion
        
        #region Contract State
        
        /// <summary>
        /// IDs of currently available contracts.
        /// </summary>
        public List<string> AvailableContractIds { get; set; } = new List<string>();
        
        #endregion
        
        #region Risk State
        
        /// <summary>
        /// Current reputation risk level (0.0 to 1.0).
        /// </summary>
        public float CurrentReputationRisk { get; set; }
        
        /// <summary>
        /// Current financial risk level (0.0 to 1.0).
        /// </summary>
        public float CurrentFinancialRisk { get; set; }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    /// <summary>
    /// State of a production line.
    /// </summary>
    [Serializable]
    public struct ProductionLineState
    {
        public string LineId;
        public string DroneId;
        public int QueueCount;
        public float Progress;
        public bool IsActive;
        public DateTime StartTime;
    }
    
    /// <summary>
    /// Serializable snapshot of GameStateData.
    /// </summary>
    [Serializable]
    public struct GameStateSnapshot
    {
        public string GameVersion;
        public int SaveDataVersion;
        public Core.GameState CurrentGameState;
        public DateTime SessionStartTime;
        public float TotalPlayTime;
        public DateTime SaveTimestamp;
        
        public string ActiveResearchId;
        public List<string> ResearchQueue;
        public List<ProductionLineState> ActiveProductionLines;
        public List<string> AvailableContractIds;
        
        public float CurrentReputationRisk;
        public float CurrentFinancialRisk;
        
        public int Version;
    }
    
    #endregion
}
