using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Possible reward entry for procedural generation
    /// </summary>
    [Serializable]
    public class PossibleReward
    {
        public RewardType rewardType;
        public float minAmount;
        public float maxAmount;
        public float weight = 1f;
        [Tooltip("Tech level multiplier - higher tech = higher rewards")]
        public float techLevelMultiplier = 0.1f;
    }

    /// <summary>
    /// Types of rewards contracts can offer
    /// </summary>
    public enum RewardType
    {
        Money,
        Reputation,
        ResearchPoints,
        TechUnlock,
        SpecialComponent,
        DroneBlueprint,
        ExclusiveClient
    }

    /// <summary>
    /// Possible requirement entry for procedural generation
    /// </summary>
    [Serializable]
    public class PossibleRequirement
    {
        public RequirementType requirementType;
        public float minValue;
        public float maxValue;
        public float weight = 1f;
        public List<string> possibleTechIds = new List<string>();
    }

    /// <summary>
    /// Types of requirements contracts can have
    /// </summary>
    public enum RequirementType
    {
        TechLevel,
        SpecificTech,
        DroneTier,
        Reputation,
        CompanyLevel,
        PreviousContracts,
        Specialization
    }

    /// <summary>
    /// Client template for generating fictional clients
    /// </summary>
    [Serializable]
    public class ClientTemplate
    {
        public string companyName;
        public string[] possiblePrefixes;
        public string[] possibleSuffixes;
        public ContractCategory preferredCategory;
        public float reputationModifier = 1f;
        public float budgetModifier = 1f;
        public float deadlineModifier = 1f;
    }

    /// <summary>
    /// Template for procedurally generating contracts
    /// </summary>
    [CreateAssetMenu(fileName = "ContractTemplate", menuName = "Project Aegis/Contract Template")]
    public class ContractTemplate : ScriptableObject
    {
        [Header("Basic Info")]
        public string templateName;
        [TextArea(2, 4)]
        public string templateDescription;
        public ContractCategory category;
        public List<ContractDifficulty> possibleDifficulties = new List<ContractDifficulty>();

        [Header("Naming")]
        public string[] contractNameFormats;
        public string[] contractAdjectives;
        public string[] contractNouns;

        [Header("Rewards")]
        public List<PossibleReward> possibleRewards = new List<PossibleReward>();
        [Tooltip("Base reward range (will be scaled by difficulty)")]
        public Vector2 baseRewardRange = new Vector2(1000, 10000);
        [Tooltip("Reputation reward range")]
        public Vector2 reputationRewardRange = new Vector2(5, 50);

        [Header("Requirements")]
        public List<PossibleRequirement> possibleRequirements = new List<PossibleRequirement>();
        [Tooltip("Base tech level requirement range")]
        public Vector2 techLevelRange = new Vector2(1, 10);
        [Tooltip("Base drone tier requirement range")]
        public Vector2Int droneTierRange = new Vector2Int(1, 5);

        [Header("Timing")]
        [Tooltip("Deadline range in days")]
        public Vector2 deadlineRange = new Vector2(3, 30);
        [Tooltip("Penalty per day late")]
        public float penaltyPerDayLate = 0.05f;
        [Tooltip("Maximum penalty percentage")]
        public float maxPenalty = 0.5f;

        [Header("Competition")]
        [Tooltip("Number of AI competitors range")]
        public Vector2Int competitorRange = new Vector2Int(2, 6);
        [Tooltip("Base competitor reputation range")]
        public Vector2 competitorReputationRange = new Vector2(10, 100);

        [Header("Risk")]
        public List<RiskProfile> possibleRiskProfiles = new List<RiskProfile>();
        [Tooltip("Chance of each risk profile (should match possibleRiskProfiles count)")]
        public float[] riskProfileWeights;

        [Header("Client Generation")]
        public List<ClientTemplate> possibleClients = new List<ClientTemplate>();

        [Header("Scaling")]
        [Tooltip("How much rewards scale with player tech level")]
        public float rewardScalingFactor = 0.15f;
        [Tooltip("How much requirements scale with player tech level")]
        public float requirementScalingFactor = 0.1f;
        [Tooltip("Minimum generation weight")]
        public float minGenerationWeight = 0.5f;
        [Tooltip("Maximum generation weight")]
        public float maxGenerationWeight = 2f;

        /// <summary>
        /// Generate a random contract name
        /// </summary>
        public string GenerateContractName(ContractDifficulty difficulty)
        {
            string format = contractNameFormats.Length > 0 
                ? contractNameFormats[UnityEngine.Random.Range(0, contractNameFormats.Length)]
                : "{adj} {noun} Contract";

            string adj = contractAdjectives.Length > 0
                ? contractAdjectives[UnityEngine.Random.Range(0, contractAdjectives.Length)]
                : "Standard";

            string noun = contractNouns.Length > 0
                ? contractNouns[UnityEngine.Random.Range(0, contractNouns.Length)]
                : "Service";

            // Add difficulty prefix
            string difficultyPrefix = difficulty switch
            {
                ContractDifficulty.Easy => "Basic ",
                ContractDifficulty.Normal => "",
                ContractDifficulty.Hard => "Advanced ",
                ContractDifficulty.Elite => "Elite ",
                ContractDifficulty.Legendary => "Legendary ",
                _ => ""
            };

            return difficultyPrefix + format.Replace("{adj}", adj).Replace("{noun}", noun);
        }

        /// <summary>
        /// Generate a random client name
        /// </summary>
        public string GenerateClientName()
        {
            if (possibleClients.Count == 0)
                return "Unknown Client";

            var client = possibleClients[UnityEngine.Random.Range(0, possibleClients.Count)];
            
            if (!string.IsNullOrEmpty(client.companyName))
                return client.companyName;

            string prefix = client.possiblePrefixes.Length > 0
                ? client.possiblePrefixes[UnityEngine.Random.Range(0, client.possiblePrefixes.Length)]
                : "Generic";

            string suffix = client.possibleSuffixes.Length > 0
                ? client.possibleSuffixes[UnityEngine.Random.Range(0, client.possibleSuffixes.Length)]
                : "Corp";

            return $"{prefix} {suffix}";
        }

        /// <summary>
        /// Generate a random description
        /// </summary>
        public string GenerateDescription(ContractDifficulty difficulty, string clientName)
        {
            string[] descriptions = new string[]
            {
                $"{clientName} requires a {difficulty.ToString().ToLower()} drone solution for their operations.",
                $"A {difficulty.ToString().ToLower()} contract from {clientName} with strict requirements.",
                $"{clientName} is seeking a reliable partner for {category.ToString().ToLower()} services.",
                $"High-priority {category.ToString().ToLower()} contract from {clientName}.",
                $"{clientName} needs specialized drones for {category.ToString().ToLower()} operations."
            };

            return descriptions[UnityEngine.Random.Range(0, descriptions.Length)];
        }

        /// <summary>
        /// Calculate base reward for a difficulty level
        /// </summary>
        public float CalculateBaseReward(ContractDifficulty difficulty, float playerTechLevel)
        {
            float baseAmount = UnityEngine.Random.Range(baseRewardRange.x, baseRewardRange.y);
            float difficultyMult = ContractData.GetDifficultyMultiplier(difficulty);
            float scalingMult = 1f + (playerTechLevel * rewardScalingFactor);

            return baseAmount * difficultyMult * scalingMult;
        }

        /// <summary>
        /// Calculate tech level requirement
        /// </summary>
        public float CalculateTechLevelRequirement(ContractDifficulty difficulty, float playerTechLevel)
        {
            float baseTech = UnityEngine.Random.Range(techLevelRange.x, techLevelRange.y);
            float difficultyMult = difficulty switch
            {
                ContractDifficulty.Easy => 0.5f,
                ContractDifficulty.Normal => 1f,
                ContractDifficulty.Hard => 1.5f,
                ContractDifficulty.Elite => 2.5f,
                ContractDifficulty.Legendary => 4f,
                _ => 1f
            };

            // Scale with player progress but keep it challenging
            float targetTech = playerTechLevel * (0.8f + (difficultyMult * 0.2f));
            return Mathf.Max(baseTech * difficultyMult, targetTech * 0.5f);
        }

        /// <summary>
        /// Get random risk profile based on weights
        /// </summary>
        public RiskProfile GetRandomRiskProfile()
        {
            if (possibleRiskProfiles.Count == 0)
                return RiskProfile.Medium;

            if (riskProfileWeights == null || riskProfileWeights.Length != possibleRiskProfiles.Count)
            {
                return possibleRiskProfiles[UnityEngine.Random.Range(0, possibleRiskProfiles.Count)];
            }

            float totalWeight = 0f;
            foreach (var w in riskProfileWeights) totalWeight += w;

            float random = UnityEngine.Random.Range(0f, totalWeight);
            float current = 0f;

            for (int i = 0; i < possibleRiskProfiles.Count; i++)
            {
                current += riskProfileWeights[i];
                if (random <= current)
                    return possibleRiskProfiles[i];
            }

            return possibleRiskProfiles[possibleRiskProfiles.Count - 1];
        }

        /// <summary>
        /// Get generation weight based on player progress
        /// </summary>
        public float GetGenerationWeight(float playerTechLevel)
        {
            // Templates become less common as player outgrows them
            float techRatio = playerTechLevel / (techLevelRange.y * 2f);
            float weight = Mathf.Lerp(maxGenerationWeight, minGenerationWeight, techRatio);
            return Mathf.Max(weight, minGenerationWeight);
        }
    }
}
