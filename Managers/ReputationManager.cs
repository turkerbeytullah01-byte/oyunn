using System;
using UnityEngine;

namespace ProjectAegis
{
    /// <summary>
    /// Source of reputation change for analytics and logging
    /// </summary>
    public enum ReputationSource
    {
        ContractSuccess,
        ContractFailure,
        TechBreakthrough,
        PrototypeSuccess,
        PrototypeFailure,
        Event,
        Milestone,
        Cheat
    }

    /// <summary>
    /// Reputation level tiers
    /// </summary>
    public enum ReputationLevel
    {
        Unknown,      // 0-20
        Recognized,   // 21-40
        Respected,    // 41-60
        Renowned,     // 61-80
        Legendary     // 81-100
    }

    /// <summary>
    /// Manages company reputation which affects contracts, pricing, and opportunities
    /// </summary>
    public class ReputationManager : BaseManager<ReputationManager>
    {
        #region Constants
        private const float MIN_REPUTATION = 0f;
        private const float MAX_REPUTATION = 100f;
        private const float UNKNOWN_THRESHOLD = 20f;
        private const float RECOGNIZED_THRESHOLD = 40f;
        private const float RESPECTED_THRESHOLD = 60f;
        private const float RENOWNED_THRESHOLD = 80f;
        
        // Bonus multipliers
        private const float UNKNOWN_CONTRACT_BONUS = 0f;
        private const float RECOGNIZED_CONTRACT_BONUS = 0.05f;
        private const float RESPECTED_CONTRACT_BONUS = 0.10f;
        private const float RENOWNED_CONTRACT_BONUS = 0.20f;
        private const float LEGENDARY_CONTRACT_BONUS = 0.35f;
        
        // Price multipliers
        private const float UNKNOWN_PRICE_MULTIPLIER = 0.90f;
        private const float RECOGNIZED_PRICE_MULTIPLIER = 1.00f;
        private const float RESPECTED_PRICE_MULTIPLIER = 1.10f;
        private const float RENOWNED_PRICE_MULTIPLIER = 1.25f;
        private const float LEGENDARY_PRICE_MULTIPLIER = 1.50f;
        #endregion

        #region Events
        public event Action<float, float> OnReputationChanged; // oldValue, newValue
        public event Action<ReputationLevel, ReputationLevel> OnReputationLevelChanged; // oldLevel, newLevel
        public event Action<float, ReputationSource> OnReputationGained;
        public event Action<float, ReputationSource> OnReputationLost;
        #endregion

        #region Properties
        [SerializeField] private float _reputation = 10f; // Start with small reputation
        
        public float Reputation 
        { 
            get => _reputation;
            private set
            {
                float oldValue = _reputation;
                _reputation = Mathf.Clamp(value, MIN_REPUTATION, MAX_REPUTATION);
                
                if (!Mathf.Approximately(oldValue, _reputation))
                {
                    OnReputationChanged?.Invoke(oldValue, _reputation);
                    
                    ReputationLevel oldLevel = GetReputationLevelFromValue(oldValue);
                    ReputationLevel newLevel = GetReputationLevelFromValue(_reputation);
                    
                    if (oldLevel != newLevel)
                    {
                        OnReputationLevelChanged?.Invoke(oldLevel, newLevel);
                        Debug.Log($"[ReputationManager] Level changed from {oldLevel} to {newLevel}");
                    }
                }
            }
        }
        
        public float ReputationPercentage => Reputation / MAX_REPUTATION;
        public ReputationLevel CurrentLevel => GetReputationLevelFromValue(Reputation);
        #endregion

        #region Initialization
        protected override void OnInitialize()
        {
            base.OnInitialize();
            Debug.Log($"[ReputationManager] Initialized with reputation: {Reputation:F1}");
        }

        protected override void OnSetupEventSubscriptions()
        {
            base.OnSetupEventSubscriptions();
            
            // Subscribe to contract completion events
            if (ContractManager.HasInstance)
            {
                ContractManager.Instance.OnContractCompleted += HandleContractCompleted;
                ContractManager.Instance.OnContractFailed += HandleContractFailed;
            }
            
            // Subscribe to tech unlock events
            if (TechTreeManager.HasInstance)
            {
                TechTreeManager.Instance.OnTechnologyUnlocked += HandleTechnologyUnlocked;
            }
            
            // Subscribe to prototype testing events
            if (PrototypeTestingManager.HasInstance)
            {
                PrototypeTestingManager.Instance.OnTestPassed += HandlePrototypeSuccess;
                PrototypeTestingManager.Instance.OnTestFailed += HandlePrototypeFailure;
            }
            
            // Subscribe to milestone events
            if (DynamicEventManager.HasInstance)
            {
                DynamicEventManager.Instance.OnMilestoneReached += HandleMilestoneReached;
            }
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            
            if (ContractManager.HasInstance)
            {
                ContractManager.Instance.OnContractCompleted -= HandleContractCompleted;
                ContractManager.Instance.OnContractFailed -= HandleContractFailed;
            }
            
            if (TechTreeManager.HasInstance)
            {
                TechTreeManager.Instance.OnTechnologyUnlocked -= HandleTechnologyUnlocked;
            }
            
            if (PrototypeTestingManager.HasInstance)
            {
                PrototypeTestingManager.Instance.OnTestPassed -= HandlePrototypeSuccess;
                PrototypeTestingManager.Instance.OnTestFailed -= HandlePrototypeFailure;
            }
            
            if (DynamicEventManager.HasInstance)
            {
                DynamicEventManager.Instance.OnMilestoneReached -= HandleMilestoneReached;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add reputation points
        /// </summary>
        public void AddReputation(float amount, ReputationSource source)
        {
            if (amount <= 0) return;
            
            float oldReputation = Reputation;
            Reputation += amount;
            
            OnReputationGained?.Invoke(amount, source);
            Debug.Log($"[ReputationManager] Gained {amount:F1} reputation from {source}. Total: {Reputation:F1}");
        }

        /// <summary>
        /// Remove reputation points
        /// </summary>
        public void RemoveReputation(float amount, ReputationSource source)
        {
            if (amount <= 0) return;
            
            float oldReputation = Reputation;
            Reputation -= amount;
            
            OnReputationLost?.Invoke(amount, source);
            Debug.Log($"[ReputationManager] Lost {amount:F1} reputation from {source}. Total: {Reputation:F1}");
        }

        /// <summary>
        /// Get current reputation value
        /// </summary>
        public float GetReputation() => Reputation;

        /// <summary>
        /// Get current reputation level
        /// </summary>
        public ReputationLevel GetReputationLevel() => CurrentLevel;

        /// <summary>
        /// Get contract bonus percentage based on reputation level
        /// </summary>
        public float GetContractBonus()
        {
            return CurrentLevel switch
            {
                ReputationLevel.Unknown => UNKNOWN_CONTRACT_BONUS,
                ReputationLevel.Recognized => RECOGNIZED_CONTRACT_BONUS,
                ReputationLevel.Respected => RESPECTED_CONTRACT_BONUS,
                ReputationLevel.Renowned => RENOWNED_CONTRACT_BONUS,
                ReputationLevel.Legendary => LEGENDARY_CONTRACT_BONUS,
                _ => 0f
            };
        }

        /// <summary>
        /// Get price multiplier based on reputation level
        /// </summary>
        public float GetPriceMultiplier()
        {
            return CurrentLevel switch
            {
                ReputationLevel.Unknown => UNKNOWN_PRICE_MULTIPLIER,
                ReputationLevel.Recognized => RECOGNIZED_PRICE_MULTIPLIER,
                ReputationLevel.Respected => RESPECTED_PRICE_MULTIPLIER,
                ReputationLevel.Renowned => RENOWNED_PRICE_MULTIPLIER,
                ReputationLevel.Legendary => LEGENDARY_PRICE_MULTIPLIER,
                _ => 1f
            };
        }

        /// <summary>
        /// Get the reputation threshold for a specific level
        /// </summary>
        public float GetThresholdForLevel(ReputationLevel level)
        {
            return level switch
            {
                ReputationLevel.Unknown => 0f,
                ReputationLevel.Recognized => UNKNOWN_THRESHOLD,
                ReputationLevel.Respected => RECOGNIZED_THRESHOLD,
                ReputationLevel.Renowned => RESPECTED_THRESHOLD,
                ReputationLevel.Legendary => RENOWNED_THRESHOLD,
                _ => 0f
            };
        }

        /// <summary>
        /// Get progress to next level (0-1)
        /// </summary>
        public float GetProgressToNextLevel()
        {
            float currentThreshold = GetThresholdForLevel(CurrentLevel);
            float nextThreshold = CurrentLevel switch
            {
                ReputationLevel.Unknown => RECOGNIZED_THRESHOLD,
                ReputationLevel.Recognized => RESPECTED_THRESHOLD,
                ReputationLevel.Respected => RENOWNED_THRESHOLD,
                ReputationLevel.Renowned => MAX_REPUTATION,
                ReputationLevel.Legendary => MAX_REPUTATION,
                _ => MAX_REPUTATION
            };
            
            if (CurrentLevel == ReputationLevel.Legendary) return 1f;
            
            return (Reputation - currentThreshold) / (nextThreshold - currentThreshold);
        }

        /// <summary>
        /// Get reputation needed for next level
        /// </summary>
        public float GetReputationToNextLevel()
        {
            float nextThreshold = CurrentLevel switch
            {
                ReputationLevel.Unknown => RECOGNIZED_THRESHOLD,
                ReputationLevel.Recognized => RESPECTED_THRESHOLD,
                ReputationLevel.Respected => RENOWNED_THRESHOLD,
                ReputationLevel.Renowned => MAX_REPUTATION,
                ReputationLevel.Legendary => MAX_REPUTATION,
                _ => MAX_REPUTATION
            };
            
            return Mathf.Max(0, nextThreshold - Reputation);
        }
        #endregion

        #region Private Methods
        private ReputationLevel GetReputationLevelFromValue(float value)
        {
            return value switch
            {
                <= UNKNOWN_THRESHOLD => ReputationLevel.Unknown,
                <= RECOGNIZED_THRESHOLD => ReputationLevel.Recognized,
                <= RESPECTED_THRESHOLD => ReputationLevel.Respected,
                <= RENOWNED_THRESHOLD => ReputationLevel.Renowned,
                _ => ReputationLevel.Legendary
            };
        }
        #endregion

        #region Event Handlers
        private void HandleContractCompleted(ContractData contract)
        {
            // Base reputation gain from contract
            float baseGain = contract.difficulty switch
            {
                ContractDifficulty.Easy => 1f,
                ContractDifficulty.Medium => 2f,
                ContractDifficulty.Hard => 4f,
                ContractDifficulty.Elite => 7f,
                _ => 1f
            };
            
            // Bonus for early completion
            float completionBonus = 0f;
            if (ContractManager.HasInstance)
            {
                var activeContract = ContractManager.Instance.GetActiveContract(contract.contractId);
                if (activeContract != null)
                {
                    float timeRatio = activeContract.RemainingTime / activeContract.TotalDuration;
                    if (timeRatio > 0.5f) completionBonus = baseGain * 0.5f;
                }
            }
            
            AddReputation(baseGain + completionBonus, ReputationSource.ContractSuccess);
        }

        private void HandleContractFailed(ContractData contract)
        {
            float loss = contract.difficulty switch
            {
                ContractDifficulty.Easy => 0.5f,
                ContractDifficulty.Medium => 1f,
                ContractDifficulty.Hard => 2f,
                ContractDifficulty.Elite => 4f,
                _ => 1f
            };
            
            RemoveReputation(loss, ReputationSource.ContractFailure);
        }

        private void HandleTechnologyUnlocked(TechnologyNode tech)
        {
            // Reputation gain from unlocking technologies
            float gain = tech.researchCost / 1000f; // Scale with research cost
            gain = Mathf.Clamp(gain, 0.5f, 5f);
            
            AddReputation(gain, ReputationSource.TechBreakthrough);
        }

        private void HandlePrototypeSuccess(PrototypeTest test)
        {
            float gain = test.testType switch
            {
                PrototypeTestType.Flight => 0.5f,
                PrototypeTestType.Signal => 0.5f,
                PrototypeTestType.BatteryStress => 0.75f,
                _ => 0.5f
            };
            
            AddReputation(gain, ReputationSource.PrototypeSuccess);
        }

        private void HandlePrototypeFailure(PrototypeTest test)
        {
            float loss = 0.25f;
            RemoveReputation(loss, ReputationSource.PrototypeFailure);
        }

        private void HandleMilestoneReached(MilestoneType milestone, int tier)
        {
            float gain = tier switch
            {
                1 => 2f,
                2 => 5f,
                3 => 10f,
                4 => 20f,
                _ => 1f
            };
            
            AddReputation(gain, ReputationSource.Milestone);
        }
        #endregion

        #region Save/Load
        public ReputationSaveData GetSaveData()
        {
            return new ReputationSaveData
            {
                reputation = Reputation
            };
        }

        public void LoadSaveData(ReputationSaveData data)
        {
            if (data != null)
            {
                Reputation = data.reputation;
                Debug.Log($"[ReputationManager] Loaded reputation: {Reputation:F1}");
            }
        }
        #endregion
    }

    #region Save Data
    [Serializable]
    public class ReputationSaveData
    {
        public float reputation;
    }
    #endregion
}
