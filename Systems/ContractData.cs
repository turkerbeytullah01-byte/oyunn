using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Risk profile for contracts - affects penalties and failure chances
    /// </summary>
    public enum RiskProfile
    {
        None,       // No risk, guaranteed delivery
        Low,        // Small chance of minor issues
        Medium,     // Moderate risk, some penalties possible
        High,       // Significant risk, major penalties
        Critical    // Extreme risk, contract can fail completely
    }

    /// <summary>
    /// Contract difficulty tier - affects requirements and rewards
    /// </summary>
    public enum ContractDifficulty
    {
        Easy,       // Tier 1-2 drones, basic tech
        Normal,     // Tier 2-3 drones, some advanced tech
        Hard,       // Tier 3-4 drones, advanced tech required
        Elite,      // Tier 4-5 drones, cutting edge tech
        Legendary   // Tier 5+ drones, experimental tech
    }

    /// <summary>
    /// Contract category for organizing and filtering
    /// </summary>
    public enum ContractCategory
    {
        Surveillance,
        Security,
        Industrial,
        SearchAndRescue,
        Agriculture,
        Military,
        Research,
        Logistics
    }

    /// <summary>
    /// ScriptableObject defining a contract that players can bid on
    /// </summary>
    [CreateAssetMenu(fileName = "NewContract", menuName = "Project Aegis/Contract")]
    public class ContractData : ScriptableObject
    {
        [Header("Basic Info")]
        public string contractId;
        public string displayName;
        [TextArea(3, 5)]
        public string description;
        public string clientName;
        public ContractCategory category;
        public ContractDifficulty difficulty;
        public Sprite contractIcon;

        [Header("Requirements")]
        [Tooltip("Minimum overall tech level required")]
        public float requiredTechLevel;
        [Tooltip("Specific technologies that must be unlocked")]
        public List<string> requiredTechnologies = new List<string>();
        [Tooltip("Minimum drone tier required (1-5)")]
        public int requiredDroneTier = 1;
        [Tooltip("Minimum reputation to even see this contract")]
        public float minimumReputationRequired;
        [Tooltip("Minimum company level")]
        public int minimumCompanyLevel = 1;

        [Header("Rewards")]
        [Tooltip("Base monetary reward")]
        public float baseReward;
        [Tooltip("Reputation gain on successful completion")]
        public float reputationReward;
        [Tooltip("Bonus percentage for early delivery (0.1 = 10%)")]
        public float bonusForEarlyDelivery = 0.1f;
        [Tooltip("Maximum early delivery bonus amount")]
        public float maxEarlyDeliveryBonus;
        [Tooltip("Bonus for exceeding quality expectations")]
        public float qualityBonusMultiplier = 0.2f;
        [Tooltip("Additional rewards (tech unlocks, special items)")]
        public List<string> specialRewards = new List<string>();

        [Header("Terms")]
        [Tooltip("Contract deadline in days")]
        public float deadlineDays;
        [Tooltip("Penalty per day late (percentage of reward)")]
        public float penaltyPerDayLate = 0.05f;
        [Tooltip("Maximum penalty percentage (0.5 = 50%)")]
        public float maxPenalty = 0.5f;
        [Tooltip("Upfront payment percentage (0.2 = 20%)")]
        public float upfrontPaymentPercent = 0.2f;

        [Header("Risk & Competition")]
        public RiskProfile riskProfile;
        [Tooltip("Number of AI competitors bidding")]
        public int competitorCount = 3;
        [Tooltip("Base chance of random failure events")]
        public float baseFailureChance = 0.05f;
        [Tooltip("Reputation loss on failure")]
        public float reputationPenaltyOnFail = 10f;

        [Header("Generation Data")]
        [Tooltip("Weight for procedural generation (higher = more common)")]
        public float generationWeight = 1f;
        [Tooltip("Minimum player tech level to generate")]
        public float minPlayerTechForGeneration;
        [Tooltip("Maximum player tech level for this contract")]
        public float maxPlayerTechForGeneration = float.MaxValue;

        /// <summary>
        /// Calculate total reward including bonuses
        /// </summary>
        public float CalculateTotalReward(float earlyDeliveryDays = 0, float qualityScore = 1f)
        {
            float total = baseReward;

            // Early delivery bonus
            if (earlyDeliveryDays > 0)
            {
                float earlyBonus = baseReward * bonusForEarlyDelivery * earlyDeliveryDays;
                total += Mathf.Min(earlyBonus, maxEarlyDeliveryBonus);
            }

            // Quality bonus
            if (qualityScore > 1f)
            {
                total += baseReward * qualityBonusMultiplier * (qualityScore - 1f);
            }

            return total;
        }

        /// <summary>
        /// Calculate penalty for late delivery
        /// </summary>
        public float CalculateLatePenalty(float daysLate)
        {
            float penaltyPercent = Mathf.Min(daysLate * penaltyPerDayLate, maxPenalty);
            return baseReward * penaltyPercent;
        }

        /// <summary>
        /// Get upfront payment amount
        /// </summary>
        public float GetUpfrontPayment()
        {
            return baseReward * upfrontPaymentPercent;
        }

        /// <summary>
        /// Check if player meets requirements to bid
        /// </summary>
        public bool CanPlayerBid(float playerTechLevel, float playerReputation, 
            List<string> unlockedTech, int highestDroneTier)
        {
            if (playerTechLevel < requiredTechLevel)
                return false;

            if (playerReputation < minimumReputationRequired)
                return false;

            if (highestDroneTier < requiredDroneTier)
                return false;

            // Check required technologies
            foreach (var tech in requiredTechnologies)
            {
                if (!unlockedTech.Contains(tech))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get difficulty multiplier for rewards
        /// </summary>
        public static float GetDifficultyMultiplier(ContractDifficulty difficulty)
        {
            return difficulty switch
            {
                ContractDifficulty.Easy => 0.6f,
                ContractDifficulty.Normal => 1f,
                ContractDifficulty.Hard => 1.5f,
                ContractDifficulty.Elite => 2.5f,
                ContractDifficulty.Legendary => 5f,
                _ => 1f
            };
        }

        /// <summary>
        /// Get risk multiplier for penalties
        /// </summary>
        public static float GetRiskMultiplier(RiskProfile risk)
        {
            return risk switch
            {
                RiskProfile.None => 0f,
                RiskProfile.Low => 0.5f,
                RiskProfile.Medium => 1f,
                RiskProfile.High => 2f,
                RiskProfile.Critical => 4f,
                _ => 1f
            };
        }

        private void OnValidate()
        {
            // Auto-generate contract ID if empty
            if (string.IsNullOrEmpty(contractId))
            {
                contractId = System.Guid.NewGuid().ToString("N")[..8];
            }
        }
    }
}
