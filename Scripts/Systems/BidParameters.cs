using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Represents a player's bid on a contract
    /// </summary>
    [Serializable]
    public class BidParameters
    {
        [Header("Pricing")]
        [Tooltip("Proposed price for the contract (can be above or below base)")]
        public float proposedPrice;
        [Tooltip("Whether this bid is below the base reward")]
        public bool isDiscounted;
        [Tooltip("Discount/ markup percentage")]
        public float priceModifierPercent;

        [Header("Timeline")]
        [Tooltip("Proposed deadline in days (can be shorter or longer than base)")]
        public float proposedDeadlineDays;
        [Tooltip("Deadline modifier relative to base")]
        public float deadlineModifier;

        [Header("Quality")]
        [Tooltip("Extra investment for quality (affects final delivery quality)")]
        public float qualityInvestment;
        [Tooltip("Target quality score (1.0 = standard, >1.0 = premium)")]
        public float targetQuality = 1f;

        [Header("Technology")]
        [Tooltip("Technologies promised to be used")]
        public List<string> promisedTech = new List<string>();
        [Tooltip("Primary technology focus")]
        public string primaryTech;

        [Header("Resources")]
        [Tooltip("Number of drones allocated")]
        public int allocatedDroneCount;
        [Tooltip("Drone tier to be used")]
        public int droneTier;
        [Tooltip("Resource allocation priority (affects speed vs quality)")]
        public ResourcePriority resourcePriority = ResourcePriority.Balanced;

        [Header("Warranty")]
        [Tooltip("Warranty period offered in days")]
        public float warrantyDays;
        [Tooltip("Warranty coverage percentage")]
        public float warrantyCoverage;

        /// <summary>
        /// Resource allocation priorities
        /// </summary>
        public enum ResourcePriority
        {
            Speed,      // Prioritize fast delivery
            Quality,    // Prioritize high quality
            Cost,       // Minimize costs
            Balanced    // Even distribution
        }

        /// <summary>
        /// Create a default bid at base values
        /// </summary>
        public static BidParameters CreateDefault(ContractData contract)
        {
            return new BidParameters
            {
                proposedPrice = contract.baseReward,
                isDiscounted = false,
                priceModifierPercent = 0f,
                proposedDeadlineDays = contract.deadlineDays,
                deadlineModifier = 1f,
                qualityInvestment = 0f,
                targetQuality = 1f,
                promisedTech = new List<string>(),
                allocatedDroneCount = 1,
                droneTier = contract.requiredDroneTier,
                resourcePriority = ResourcePriority.Balanced,
                warrantyDays = 0f,
                warrantyCoverage = 0f
            };
        }

        /// <summary>
        /// Create an aggressive bid (lower price, faster delivery)
        /// </summary>
        public static BidParameters CreateAggressiveBid(ContractData contract, float discountPercent = 0.15f, float deadlineReduction = 0.2f)
        {
            var bid = CreateDefault(contract);
            bid.proposedPrice = contract.baseReward * (1f - discountPercent);
            bid.isDiscounted = true;
            bid.priceModifierPercent = -discountPercent;
            bid.proposedDeadlineDays = contract.deadlineDays * (1f - deadlineReduction);
            bid.deadlineModifier = 1f - deadlineReduction;
            bid.resourcePriority = ResourcePriority.Speed;
            return bid;
        }

        /// <summary>
        /// Create a premium bid (higher price, better quality)
        /// </summary>
        public static BidParameters CreatePremiumBid(ContractData contract, float markupPercent = 0.2f, float qualityBoost = 0.3f)
        {
            var bid = CreateDefault(contract);
            bid.proposedPrice = contract.baseReward * (1f + markupPercent);
            bid.isDiscounted = false;
            bid.priceModifierPercent = markupPercent;
            bid.qualityInvestment = contract.baseReward * qualityBoost;
            bid.targetQuality = 1.3f;
            bid.resourcePriority = ResourcePriority.Quality;
            bid.warrantyDays = 30f;
            bid.warrantyCoverage = 0.5f;
            return bid;
        }

        /// <summary>
        /// Create a balanced bid
        /// </summary>
        public static BidParameters CreateBalancedBid(ContractData contract, float slightDiscount = 0.05f)
        {
            var bid = CreateDefault(contract);
            bid.proposedPrice = contract.baseReward * (1f - slightDiscount);
            bid.isDiscounted = true;
            bid.priceModifierPercent = -slightDiscount;
            bid.targetQuality = 1.1f;
            bid.qualityInvestment = contract.baseReward * 0.1f;
            return bid;
        }

        /// <summary>
        /// Calculate price competitiveness score (higher is better)
        /// </summary>
        public float GetPriceCompetitivenessScore(float basePrice)
        {
            if (basePrice <= 0) return 100f;
            return (basePrice / proposedPrice) * 100f;
        }

        /// <summary>
        /// Calculate deadline advantage score (higher is better)
        /// </summary>
        public float GetDeadlineAdvantageScore(float baseDeadline)
        {
            if (baseDeadline <= 0) return 100f;
            return (baseDeadline / proposedDeadlineDays) * 100f;
        }

        /// <summary>
        /// Calculate total bid value (for internal calculations)
        /// </summary>
        public float GetTotalBidValue()
        {
            float value = proposedPrice;
            
            // Quality investment is a cost
            value -= qualityInvestment;

            // Warranty has implicit cost
            value -= (proposedPrice * warrantyCoverage * 0.1f);

            return value;
        }

        /// <summary>
        /// Validate the bid against contract requirements
        /// </summary>
        public bool Validate(ContractData contract, out string errorMessage)
        {
            errorMessage = "";

            if (proposedPrice < 0)
            {
                errorMessage = "Price cannot be negative";
                return false;
            }

            if (proposedDeadlineDays <= 0)
            {
                errorMessage = "Deadline must be positive";
                return false;
            }

            if (droneTier < contract.requiredDroneTier)
            {
                errorMessage = $"Drone tier {droneTier} below required {contract.requiredDroneTier}";
                return false;
            }

            if (allocatedDroneCount < 1)
            {
                errorMessage = "Must allocate at least one drone";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get a summary of the bid
        /// </summary>
        public string GetSummary()
        {
            string priceStr = isDiscounted ? $"-{Mathf.Abs(priceModifierPercent * 100):F0}%" : $"+{priceModifierPercent * 100:F0}%";
            string priorityStr = resourcePriority.ToString();
            
            return $"Price: {priceStr} | Deadline: {deadlineModifier:P0} | Quality: {targetQuality:F1}x | Priority: {priorityStr}";
        }
    }
}
