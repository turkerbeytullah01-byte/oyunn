using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// MVP Contract definitions for Project Aegis
    /// These are pre-made contracts for early game testing and balancing
    /// </summary>

    #region Contract 1: Basic Surveillance

    [CreateAssetMenu(fileName = "Contract_BasicSurveillance", menuName = "Project Aegis/MVP Contracts/Basic Surveillance")]
    public class BasicSurveillanceContract : ContractData
    {
        private void OnEnable()
        {
            // Basic Info
            contractId = "MVP_BASIC_SURVEILLANCE_001";
            displayName = "Basic Surveillance Contract";
            description = "Stellar Surveillance Corp requires a small fleet of basic surveillance drones for perimeter monitoring of their industrial facility. This is an entry-level contract perfect for new operators.";
            clientName = "Stellar Surveillance Corp";
            category = ContractCategory.Surveillance;
            difficulty = ContractDifficulty.Easy;

            // Requirements - Very Low
            requiredTechLevel = 1f;
            requiredTechnologies = new List<string>(); // No special tech required
            requiredDroneTier = 1;
            minimumReputationRequired = 0f;
            minimumCompanyLevel = 1;

            // Rewards - Low but fair for beginners
            baseReward = 2500f;
            reputationReward = 5f;
            bonusForEarlyDelivery = 0.1f; // 10% per day early
            maxEarlyDeliveryBonus = 500f;
            qualityBonusMultiplier = 0.15f;

            // Terms - Generous for beginners
            deadlineDays = 7f;
            penaltyPerDayLate = 0.03f; // 3% per day
            maxPenalty = 0.3f; // Max 30%
            upfrontPaymentPercent = 0.25f; // 25% upfront

            // Risk - Low
            riskProfile = RiskProfile.Low;
            competitorCount = 2; // Only 2 competitors
            baseFailureChance = 0.02f;
            reputationPenaltyOnFail = 3f;

            // Generation
            generationWeight = 2f; // Common
            minPlayerTechForGeneration = 0f;
            maxPlayerTechForGeneration = 5f;
        }
    }

    #endregion

    #region Contract 2: Advanced Reconnaissance

    [CreateAssetMenu(fileName = "Contract_AdvancedRecon", menuName = "Project Aegis/MVP Contracts/Advanced Reconnaissance")]
    public class AdvancedReconnaissanceContract : ContractData
    {
        private void OnEnable()
        {
            // Basic Info
            contractId = "MVP_ADVANCED_RECON_002";
            displayName = "Advanced Reconnaissance Mission";
            description = "Quantum Reconnaissance Ltd needs a specialized drone swarm for high-altitude reconnaissance operations. Requires advanced sensors and extended flight capabilities. Previous surveillance experience preferred.";
            clientName = "Quantum Reconnaissance Ltd";
            category = ContractCategory.Surveillance;
            difficulty = ContractDifficulty.Normal;

            // Requirements - Moderate
            requiredTechLevel = 5f;
            requiredTechnologies = new List<string> { "LongRangeSensors" };
            requiredDroneTier = 2;
            minimumReputationRequired = 15f;
            minimumCompanyLevel = 3;

            // Rewards - Moderate
            baseReward = 8000f;
            reputationReward = 15f;
            bonusForEarlyDelivery = 0.08f;
            maxEarlyDeliveryBonus = 1500f;
            qualityBonusMultiplier = 0.2f;

            // Terms - Standard
            deadlineDays = 14f;
            penaltyPerDayLate = 0.05f;
            maxPenalty = 0.4f;
            upfrontPaymentPercent = 0.2f;

            // Risk - Medium
            riskProfile = RiskProfile.Medium;
            competitorCount = 4;
            baseFailureChance = 0.05f;
            reputationPenaltyOnFail = 10f;

            // Generation
            generationWeight = 1.5f;
            minPlayerTechForGeneration = 3f;
            maxPlayerTechForGeneration = 15f;
        }
    }

    #endregion

    #region Contract 3: Elite Defense Contract

    [CreateAssetMenu(fileName = "Contract_EliteDefense", menuName = "Project Aegis/MVP Contracts/Elite Defense")]
    public class EliteDefenseContract : ContractData
    {
        private void OnEnable()
        {
            // Basic Info
            contractId = "MVP_ELITE_DEFENSE_003";
            displayName = "Elite Defense Contract";
            description = "Titan Defense Systems requires an elite-tier autonomous defense drone fleet for critical infrastructure protection. This high-stakes contract demands cutting-edge technology, proven track record, and flawless execution. Only experienced operators need apply.";
            clientName = "Titan Defense Systems";
            category = ContractCategory.Military;
            difficulty = ContractDifficulty.Hard;

            // Requirements - High
            requiredTechLevel = 12f;
            requiredTechnologies = new List<string> { "AdvancedAI", "StealthSystems", "AutonomousTargeting" };
            requiredDroneTier = 4;
            minimumReputationRequired = 50f;
            minimumCompanyLevel = 8;

            // Rewards - High
            baseReward = 25000f;
            reputationReward = 35f;
            bonusForEarlyDelivery = 0.05f;
            maxEarlyDeliveryBonus = 5000f;
            qualityBonusMultiplier = 0.25f;
            specialRewards = new List<string> { "DefenseContractorLicense", "TitanPartnership" };

            // Terms - Strict
            deadlineDays = 21f;
            penaltyPerDayLate = 0.08f;
            maxPenalty = 0.6f;
            upfrontPaymentPercent = 0.15f;

            // Risk - High
            riskProfile = RiskProfile.High;
            competitorCount = 6;
            baseFailureChance = 0.1f;
            reputationPenaltyOnFail = 25f;

            // Generation
            generationWeight = 0.8f; // Less common
            minPlayerTechForGeneration = 10f;
            maxPlayerTechForGeneration = float.MaxValue;
        }
    }

    #endregion

    #region Contract Factory

    /// <summary>
    /// Factory for creating MVP contracts at runtime
    /// </summary>
    public static class MVPContractFactory
    {
        /// <summary>
        /// Create the Basic Surveillance contract
        /// </summary>
        public static ContractData CreateBasicSurveillance()
        {
            var contract = ScriptableObject.CreateInstance<ContractData>();
            
            contract.contractId = "MVP_BASIC_SURVEILLANCE_001";
            contract.displayName = "Basic Surveillance Contract";
            contract.description = "Stellar Surveillance Corp requires a small fleet of basic surveillance drones for perimeter monitoring of their industrial facility. This is an entry-level contract perfect for new operators.";
            contract.clientName = "Stellar Surveillance Corp";
            contract.category = ContractCategory.Surveillance;
            contract.difficulty = ContractDifficulty.Easy;
            
            contract.requiredTechLevel = 1f;
            contract.requiredTechnologies = new List<string>();
            contract.requiredDroneTier = 1;
            contract.minimumReputationRequired = 0f;
            contract.minimumCompanyLevel = 1;
            
            contract.baseReward = 2500f;
            contract.reputationReward = 5f;
            contract.bonusForEarlyDelivery = 0.1f;
            contract.maxEarlyDeliveryBonus = 500f;
            contract.qualityBonusMultiplier = 0.15f;
            
            contract.deadlineDays = 7f;
            contract.penaltyPerDayLate = 0.03f;
            contract.maxPenalty = 0.3f;
            contract.upfrontPaymentPercent = 0.25f;
            
            contract.riskProfile = RiskProfile.Low;
            contract.competitorCount = 2;
            contract.baseFailureChance = 0.02f;
            contract.reputationPenaltyOnFail = 3f;

            return contract;
        }

        /// <summary>
        /// Create the Advanced Reconnaissance contract
        /// </summary>
        public static ContractData CreateAdvancedReconnaissance()
        {
            var contract = ScriptableObject.CreateInstance<ContractData>();
            
            contract.contractId = "MVP_ADVANCED_RECON_002";
            contract.displayName = "Advanced Reconnaissance Mission";
            contract.description = "Quantum Reconnaissance Ltd needs a specialized drone swarm for high-altitude reconnaissance operations. Requires advanced sensors and extended flight capabilities.";
            contract.clientName = "Quantum Reconnaissance Ltd";
            contract.category = ContractCategory.Surveillance;
            contract.difficulty = ContractDifficulty.Normal;
            
            contract.requiredTechLevel = 5f;
            contract.requiredTechnologies = new List<string> { "LongRangeSensors" };
            contract.requiredDroneTier = 2;
            contract.minimumReputationRequired = 15f;
            contract.minimumCompanyLevel = 3;
            
            contract.baseReward = 8000f;
            contract.reputationReward = 15f;
            contract.bonusForEarlyDelivery = 0.08f;
            contract.maxEarlyDeliveryBonus = 1500f;
            contract.qualityBonusMultiplier = 0.2f;
            
            contract.deadlineDays = 14f;
            contract.penaltyPerDayLate = 0.05f;
            contract.maxPenalty = 0.4f;
            contract.upfrontPaymentPercent = 0.2f;
            
            contract.riskProfile = RiskProfile.Medium;
            contract.competitorCount = 4;
            contract.baseFailureChance = 0.05f;
            contract.reputationPenaltyOnFail = 10f;

            return contract;
        }

        /// <summary>
        /// Create the Elite Defense contract
        /// </summary>
        public static ContractData CreateEliteDefense()
        {
            var contract = ScriptableObject.CreateInstance<ContractData>();
            
            contract.contractId = "MVP_ELITE_DEFENSE_003";
            contract.displayName = "Elite Defense Contract";
            contract.description = "Titan Defense Systems requires an elite-tier autonomous defense drone fleet for critical infrastructure protection. High-stakes contract demanding cutting-edge technology.";
            contract.clientName = "Titan Defense Systems";
            contract.category = ContractCategory.Military;
            contract.difficulty = ContractDifficulty.Hard;
            
            contract.requiredTechLevel = 12f;
            contract.requiredTechnologies = new List<string> { "AdvancedAI", "StealthSystems", "AutonomousTargeting" };
            contract.requiredDroneTier = 4;
            contract.minimumReputationRequired = 50f;
            contract.minimumCompanyLevel = 8;
            
            contract.baseReward = 25000f;
            contract.reputationReward = 35f;
            contract.bonusForEarlyDelivery = 0.05f;
            contract.maxEarlyDeliveryBonus = 5000f;
            contract.qualityBonusMultiplier = 0.25f;
            contract.specialRewards = new List<string> { "DefenseContractorLicense", "TitanPartnership" };
            
            contract.deadlineDays = 21f;
            contract.penaltyPerDayLate = 0.08f;
            contract.maxPenalty = 0.6f;
            contract.upfrontPaymentPercent = 0.15f;
            
            contract.riskProfile = RiskProfile.High;
            contract.competitorCount = 6;
            contract.baseFailureChance = 0.1f;
            contract.reputationPenaltyOnFail = 25f;

            return contract;
        }

        /// <summary>
        /// Create all MVP contracts
        /// </summary>
        public static List<ContractData> CreateAllMVPContracts()
        {
            return new List<ContractData>
            {
                CreateBasicSurveillance(),
                CreateAdvancedReconnaissance(),
                CreateEliteDefense()
            };
        }
    }

    #endregion
}
