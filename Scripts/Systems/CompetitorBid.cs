using System;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Represents an AI competitor's bid on a contract
    /// </summary>
    [Serializable]
    public class CompetitorBid
    {
        [Header("Identity")]
        public string competitorId;
        public string competitorName;
        public string companyType;
        public Sprite companyLogo;

        [Header("Bid Details")]
        public float bidPrice;
        public float bidDeadline;
        public float bidQuality;
        public int droneTier;

        [Header("Stats")]
        public float competitorReputation;
        public float competitorTechLevel;
        public float specializationBonus;
        public ContractCategory specialization;

        [Header("Score")]
        public float finalScore;
        public float priceScore;
        public float techScore;
        public float deadlineScore;
        public float reputationScore;

        [Header("Behavior")]
        public CompetitorAggressiveness aggressiveness;
        public float bidVariance; // How much their bids vary

        /// <summary>
        /// AI competitor aggressiveness levels
        /// </summary>
        public enum CompetitorAggressiveness
        {
            Conservative,   // High prices, safe bids
            Moderate,       // Balanced approach
            Aggressive,     // Low prices to win
            Desperate       // Will bid at loss to win
        }

        /// <summary>
        /// Predefined competitor company names
        /// </summary>
        public static readonly string[] CompanyPrefixes = new string[]
        {
            "Aether", "Nexus", "Stellar", "Titan", "Quantum",
            "Omni", "Cyber", "Nova", "Fusion", "Prime",
            "Apex", "Zenith", "Vertex", "Helix", "Orbit",
            "Pulse", "Vortex", "Echo", "Horizon", "Nebula"
        };

        public static readonly string[] CompanySuffixes = new string[]
        {
            "Dynamics", "Solutions", "Corp", "Industries", "Systems",
            "Technologies", "Enterprises", "Group", "Holdings", "Ltd",
            "Innovations", "Labs", "Networks", "Security", "Defense",
            "Aviation", "Robotics", "Automation", "Services", "Partners"
        };

        public static readonly string[] CompanyTypes = new string[]
        {
            "Startup", "Established", "Veteran", "Elite", "Budget",
            "Specialist", "Generalist", "Premium", "Economy", "Cutting-Edge"
        };

        /// <summary>
        /// Generate a random competitor
        /// </summary>
        public static CompetitorBid GenerateRandom(
            ContractData contract, 
            float playerReputation,
            float playerTechLevel,
            int seed = 0)
        {
            if (seed != 0)
                UnityEngine.Random.InitState(seed);

            var competitor = new CompetitorBid
            {
                competitorId = System.Guid.NewGuid().ToString("N")[..8],
                competitorName = GenerateCompanyName(),
                companyType = CompanyTypes[UnityEngine.Random.Range(0, CompanyTypes.Length)],
                specialization = (ContractCategory)UnityEngine.Random.Range(0, 8),
                aggressiveness = (CompetitorAggressiveness)UnityEngine.Random.Range(0, 4)
            };

            // Generate stats relative to player
            competitor.GenerateStats(playerReputation, playerTechLevel, contract);
            
            // Generate bid
            competitor.GenerateBid(contract);

            return competitor;
        }

        /// <summary>
        /// Generate company name
        /// </summary>
        private static string GenerateCompanyName()
        {
            string prefix = CompanyPrefixes[UnityEngine.Random.Range(0, CompanyPrefixes.Length)];
            string suffix = CompanySuffixes[UnityEngine.Random.Range(0, CompanySuffixes.Length)];
            return $"{prefix} {suffix}";
        }

        /// <summary>
        /// Generate competitor stats
        /// </summary>
        private void GenerateStats(float playerReputation, float playerTechLevel, ContractData contract)
        {
            // Reputation varies around player level
            float repVariance = UnityEngine.Random.Range(-20f, 30f);
            competitorReputation = Mathf.Max(0, playerReputation + repVariance);

            // Tech level varies
            float techVariance = UnityEngine.Random.Range(-2f, 3f);
            competitorTechLevel = Mathf.Max(1, playerTechLevel + techVariance);

            // Specialization bonus if matching category
            specializationBonus = (specialization == contract.category) ? 1.2f : 1f;

            // Drone tier
            droneTier = Mathf.Clamp(contract.requiredDroneTier + UnityEngine.Random.Range(-1, 2), 1, 5);

            // Variance based on aggressiveness
            bidVariance = aggressiveness switch
            {
                CompetitorAggressiveness.Conservative => 0.05f,
                CompetitorAggressiveness.Moderate => 0.1f,
                CompetitorAggressiveness.Aggressive => 0.15f,
                CompetitorAggressiveness.Desperate => 0.25f,
                _ => 0.1f
            };
        }

        /// <summary>
        /// Generate bid values
        /// </summary>
        private void GenerateBid(ContractData contract)
        {
            // Base modifiers
            float priceModifier = GetPriceModifier();
            float deadlineModifier = GetDeadlineModifier();
            float qualityModifier = GetQualityModifier();

            // Apply variance
            priceModifier += UnityEngine.Random.Range(-bidVariance, bidVariance);
            deadlineModifier += UnityEngine.Random.Range(-bidVariance * 0.5f, bidVariance * 0.5f);

            // Calculate bid values
            bidPrice = contract.baseReward * priceModifier;
            bidDeadline = contract.deadlineDays * deadlineModifier;
            bidQuality = 1f * qualityModifier;

            // Ensure minimums
            bidPrice = Mathf.Max(contract.baseReward * 0.5f, bidPrice);
            bidDeadline = Mathf.Max(contract.deadlineDays * 0.5f, bidDeadline);
        }

        /// <summary>
        /// Get price modifier based on aggressiveness
        /// </summary>
        private float GetPriceModifier()
        {
            return aggressiveness switch
            {
                CompetitorAggressiveness.Conservative => 1.15f,
                CompetitorAggressiveness.Moderate => 1.0f,
                CompetitorAggressiveness.Aggressive => 0.85f,
                CompetitorAggressiveness.Desperate => 0.7f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Get deadline modifier based on aggressiveness
        /// </summary>
        private float GetDeadlineModifier()
        {
            return aggressiveness switch
            {
                CompetitorAggressiveness.Conservative => 1.2f,
                CompetitorAggressiveness.Moderate => 1.0f,
                CompetitorAggressiveness.Aggressive => 0.85f,
                CompetitorAggressiveness.Desperate => 0.75f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Get quality modifier based on aggressiveness
        /// </summary>
        private float GetQualityModifier()
        {
            return aggressiveness switch
            {
                CompetitorAggressiveness.Conservative => 1.2f,
                CompetitorAggressiveness.Moderate => 1.0f,
                CompetitorAggressiveness.Aggressive => 0.9f,
                CompetitorAggressiveness.Desperate => 0.8f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Calculate all score components
        /// </summary>
        public void CalculateScores(ContractData contract)
        {
            // Price score (lower price = higher score)
            priceScore = (contract.baseReward / bidPrice) * 100f;

            // Tech score
            float techMatch = Mathf.Min(competitorTechLevel / contract.requiredTechLevel, 2f);
            techScore = techMatch * 50f * specializationBonus;

            // Deadline score (faster = higher score)
            deadlineScore = (contract.deadlineDays / bidDeadline) * 100f;

            // Reputation score
            reputationScore = Mathf.Min(competitorReputation / 2f, 100f);

            // Calculate final weighted score
            finalScore = BidCalculator.CalculateWeightedScore(
                reputationScore,
                techScore,
                priceScore,
                deadlineScore,
                50f, // default quality score
                0f   // no warranty
            );
        }

        /// <summary>
        /// Get a summary of this competitor
        /// </summary>
        public string GetSummary()
        {
            return $"{competitorName} ({companyType}) - Rep: {competitorReputation:F0}, Tech: {competitorTechLevel:F1}";
        }

        /// <summary>
        /// Get bid details
        /// </summary>
        public string GetBidDetails()
        {
            return $"Price: {bidPrice:C0} | Deadline: {bidDeadline:F1}d | Score: {finalScore:F1}";
        }
    }
}
