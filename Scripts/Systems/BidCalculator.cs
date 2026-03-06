using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Static calculator for bid evaluation and winning chance
    /// </summary>
    public static class BidCalculator
    {
        #region Score Weights

        // Default scoring weights (can be modified per contract)
        public const float DEFAULT_REPUTATION_WEIGHT = 0.30f;
        public const float DEFAULT_TECH_WEIGHT = 0.25f;
        public const float DEFAULT_PRICE_WEIGHT = 0.25f;
        public const float DEFAULT_DEADLINE_WEIGHT = 0.20f;
        public const float DEFAULT_QUALITY_WEIGHT = 0.10f;
        public const float DEFAULT_WARRANTY_WEIGHT = 0.05f;

        // Minimum score thresholds
        public const float MIN_WINNING_SCORE = 50f;
        public const float EXCELLENT_SCORE = 90f;
        public const float GOOD_SCORE = 75f;
        public const float AVERAGE_SCORE = 60f;

        #endregion

        #region Winning Chance Calculation

        /// <summary>
        /// Calculate the player's chance of winning a contract
        /// </summary>
        public static float CalculateWinningChance(
            ContractData contract,
            BidParameters playerBid,
            float playerReputation,
            float playerTechLevel,
            List<string> playerUnlockedTech,
            int playerDroneTier,
            int simulationCount = 100)
        {
            // Calculate player score
            float playerScore = CalculatePlayerScore(
                contract, playerBid, playerReputation, 
                playerTechLevel, playerUnlockedTech, playerDroneTier);

            // Run simulations against competitors
            int wins = 0;
            for (int i = 0; i < simulationCount; i++)
            {
                var competitors = GenerateCompetitorScores(contract, playerReputation, playerTechLevel);
                
                bool playerWins = true;
                foreach (var competitor in competitors)
                {
                    if (competitor.finalScore > playerScore)
                    {
                        playerWins = false;
                        break;
                    }
                }

                if (playerWins) wins++;
            }

            return (float)wins / simulationCount;
        }

        /// <summary>
        /// Calculate winning chance with detailed breakdown
        /// </summary>
        public static (float chance, BidScoreBreakdown breakdown, List<CompetitorBid> competitors) 
            CalculateWinningChanceDetailed(
            ContractData contract,
            BidParameters playerBid,
            float playerReputation,
            float playerTechLevel,
            List<string> playerUnlockedTech,
            int playerDroneTier)
        {
            // Calculate player score and breakdown
            var breakdown = CalculateScoreBreakdown(
                contract, playerBid, playerReputation, 
                playerTechLevel, playerUnlockedTech, playerDroneTier);

            float playerScore = breakdown.totalScore;

            // Generate competitors
            var competitors = GenerateCompetitorScores(contract, playerReputation, playerTechLevel);

            // Calculate probability using Bradley-Terry model
            float chance = CalculateBradleyTerryProbability(playerScore, competitors.Select(c => c.finalScore).ToList());

            return (chance, breakdown, competitors);
        }

        #endregion

        #region Score Calculation

        /// <summary>
        /// Calculate player's total score
        /// </summary>
        public static float CalculatePlayerScore(
            ContractData contract,
            BidParameters playerBid,
            float playerReputation,
            float playerTechLevel,
            List<string> playerUnlockedTech,
            int playerDroneTier)
        {
            var breakdown = CalculateScoreBreakdown(
                contract, playerBid, playerReputation, 
                playerTechLevel, playerUnlockedTech, playerDroneTier);
            
            return breakdown.totalScore;
        }

        /// <summary>
        /// Calculate detailed score breakdown
        /// </summary>
        public static BidScoreBreakdown CalculateScoreBreakdown(
            ContractData contract,
            BidParameters playerBid,
            float playerReputation,
            float playerTechLevel,
            List<string> playerUnlockedTech,
            int playerDroneTier)
        {
            var breakdown = new BidScoreBreakdown();

            // 1. Reputation Score (0-100)
            breakdown.reputationScore = CalculateReputationScore(playerReputation, contract);

            // 2. Tech Match Score (0-100)
            breakdown.techMatchScore = CalculateTechMatchScore(
                contract, playerTechLevel, playerUnlockedTech, playerDroneTier);

            // 3. Price Score (0-100+) - lower price = higher score
            breakdown.priceScore = CalculatePriceScore(contract.baseReward, playerBid.proposedPrice);

            // 4. Deadline Score (0-100+) - faster = higher score
            breakdown.deadlineScore = CalculateDeadlineScore(contract.deadlineDays, playerBid.proposedDeadlineDays);

            // 5. Quality Score (0-100)
            breakdown.qualityScore = CalculateQualityScore(playerBid.targetQuality, playerBid.qualityInvestment, contract);

            // 6. Warranty Score (0-100)
            breakdown.warrantyScore = CalculateWarrantyScore(playerBid.warrantyDays, playerBid.warrantyCoverage);

            // Calculate weighted total
            breakdown.totalScore = CalculateWeightedScore(
                breakdown.reputationScore,
                breakdown.techMatchScore,
                breakdown.priceScore,
                breakdown.deadlineScore,
                breakdown.qualityScore,
                breakdown.warrantyScore);

            return breakdown;
        }

        /// <summary>
        /// Calculate weighted total score
        /// </summary>
        public static float CalculateWeightedScore(
            float reputationScore,
            float techScore,
            float priceScore,
            float deadlineScore,
            float qualityScore,
            float warrantyScore,
            float? reputationWeight = null,
            float? techWeight = null,
            float? priceWeight = null,
            float? deadlineWeight = null,
            float? qualityWeight = null,
            float? warrantyWeight = null)
        {
            float repW = reputationWeight ?? DEFAULT_REPUTATION_WEIGHT;
            float techW = techWeight ?? DEFAULT_TECH_WEIGHT;
            float priceW = priceWeight ?? DEFAULT_PRICE_WEIGHT;
            float deadlineW = deadlineWeight ?? DEFAULT_DEADLINE_WEIGHT;
            float qualityW = qualityWeight ?? DEFAULT_QUALITY_WEIGHT;
            float warrantyW = warrantyWeight ?? DEFAULT_WARRANTY_WEIGHT;

            // Normalize weights to sum to 1
            float totalWeight = repW + techW + priceW + deadlineW + qualityW + warrantyW;
            float normalizeFactor = 1f / totalWeight;

            float total = 
                (reputationScore * repW +
                 techScore * techW +
                 priceScore * priceW +
                 deadlineScore * deadlineW +
                 qualityScore * qualityW +
                 warrantyScore * warrantyW) * normalizeFactor;

            return Mathf.Clamp(total, 0f, 150f); // Cap at 150 for exceptional bids
        }

        #endregion

        #region Individual Score Components

        /// <summary>
        /// Calculate reputation score (0-100)
        /// </summary>
        private static float CalculateReputationScore(float playerReputation, ContractData contract)
        {
            // Base score from reputation
            float baseScore = Mathf.Min(playerReputation, 100f);

            // Bonus for exceeding minimum requirements
            float minRep = contract.minimumReputationRequired;
            if (playerReputation > minRep && minRep > 0)
            {
                float excessRatio = (playerReputation - minRep) / minRep;
                baseScore += excessRatio * 10f; // Up to 10 bonus points
            }

            return Mathf.Min(baseScore, 100f);
        }

        /// <summary>
        /// Calculate technology match score (0-100)
        /// </summary>
        private static float CalculateTechMatchScore(
            ContractData contract,
            float playerTechLevel,
            List<string> playerUnlockedTech,
            int playerDroneTier)
        {
            float score = 0f;

            // Tech level match (40 points max)
            float techLevelRatio = playerTechLevel / Mathf.Max(contract.requiredTechLevel, 1f);
            score += Mathf.Min(techLevelRatio * 40f, 40f);

            // Required technologies (30 points max)
            if (contract.requiredTechnologies.Count > 0)
            {
                int matchedTech = 0;
                foreach (var tech in contract.requiredTechnologies)
                {
                    if (playerUnlockedTech.Contains(tech))
                        matchedTech++;
                }
                score += (matchedTech / (float)contract.requiredTechnologies.Count) * 30f;
            }
            else
            {
                score += 30f; // No specific tech required
            }

            // Drone tier (20 points max)
            float droneTierRatio = playerDroneTier / Mathf.Max(contract.requiredDroneTier, 1f);
            score += Mathf.Min(droneTierRatio * 20f, 20f);

            // Bonus for exceeding requirements (10 points max)
            float excessBonus = 0f;
            if (techLevelRatio > 1.5f) excessBonus += 5f;
            if (droneTierRatio > 1.5f) excessBonus += 5f;
            score += excessBonus;

            return Mathf.Min(score, 100f);
        }

        /// <summary>
        /// Calculate price competitiveness score (0-100+)
        /// </summary>
        private static float CalculatePriceScore(float basePrice, float proposedPrice)
        {
            if (proposedPrice <= 0) return 0f;
            
            // Base price / proposed price gives competitiveness
            // 0.8x price = 125 score, 1.0x = 100, 1.5x = 67
            float score = (basePrice / proposedPrice) * 100f;
            
            // Diminishing returns for very low prices
            if (score > 150f)
            {
                score = 150f + (score - 150f) * 0.3f;
            }

            // Penalty for suspiciously low prices (below 50%)
            if (proposedPrice < basePrice * 0.5f)
            {
                score *= 0.7f; // 30% penalty
            }

            return Mathf.Clamp(score, 0f, 200f);
        }

        /// <summary>
        /// Calculate deadline advantage score (0-100+)
        /// </summary>
        private static float CalculateDeadlineScore(float baseDeadline, float proposedDeadline)
        {
            if (proposedDeadline <= 0) return 0f;

            // Base deadline / proposed deadline
            float score = (baseDeadline / proposedDeadline) * 100f;

            // Diminishing returns for very fast delivery
            if (score > 150f)
            {
                score = 150f + (score - 150f) * 0.3f;
            }

            // Penalty for unrealistic deadlines (less than 30% of base)
            if (proposedDeadline < baseDeadline * 0.3f)
            {
                score *= 0.6f; // 40% penalty - likely can't deliver
            }

            return Mathf.Clamp(score, 0f, 200f);
        }

        /// <summary>
        /// Calculate quality commitment score (0-100)
        /// </summary>
        private static float CalculateQualityScore(float targetQuality, float qualityInvestment, ContractData contract)
        {
            float score = 50f; // Base score

            // Target quality bonus
            if (targetQuality > 1f)
            {
                score += (targetQuality - 1f) * 30f; // Up to 30 points
            }

            // Investment bonus
            float investmentRatio = qualityInvestment / Mathf.Max(contract.baseReward, 1f);
            score += investmentRatio * 20f; // Up to 20 points

            return Mathf.Min(score, 100f);
        }

        /// <summary>
        /// Calculate warranty score (0-100)
        /// </summary>
        private static float CalculateWarrantyScore(float warrantyDays, float warrantyCoverage)
        {
            float score = 0f;

            // Warranty period score (up to 60 points)
            score += Mathf.Min(warrantyDays / 30f * 30f, 60f);

            // Coverage score (up to 40 points)
            score += warrantyCoverage * 40f;

            return Mathf.Min(score, 100f);
        }

        #endregion

        #region Competitor Generation

        /// <summary>
        /// Generate competitor bids for a contract
        /// </summary>
        public static List<CompetitorBid> GenerateCompetitorScores(
            ContractData contract,
            float playerReputation,
            float playerTechLevel)
        {
            var competitors = new List<CompetitorBid>();

            for (int i = 0; i < contract.competitorCount; i++)
            {
                var competitor = CompetitorBid.GenerateRandom(
                    contract, playerReputation, playerTechLevel, i);
                
                competitor.CalculateScores(contract);
                competitors.Add(competitor);
            }

            // Sort by score descending
            competitors = competitors.OrderByDescending(c => c.finalScore).ToList();

            return competitors;
        }

        #endregion

        #region Probability Models

        /// <summary>
        /// Calculate probability using Bradley-Terry model
        /// </summary>
        private static float CalculateBradleyTerryProbability(float playerScore, List<float> competitorScores)
        {
            // Apply scaling to scores to avoid overflow
            float maxScore = Mathf.Max(playerScore, competitorScores.DefaultIfEmpty(0).Max());
            float scaleFactor = maxScore > 50f ? 50f / maxScore : 1f;

            float playerScaled = playerScore * scaleFactor;
            float playerExp = Mathf.Exp(playerScaled / 10f);

            float totalExp = playerExp;
            foreach (var score in competitorScores)
            {
                totalExp += Mathf.Exp(score * scaleFactor / 10f);
            }

            return playerExp / totalExp;
        }

        /// <summary>
        /// Calculate probability using simpler comparison model
        /// </summary>
        public static float CalculateSimpleProbability(float playerScore, List<float> competitorScores)
        {
            if (competitorScores.Count == 0) return 1f;

            int wins = 0;
            int total = competitorScores.Count + 1;

            foreach (var competitorScore in competitorScores)
            {
                if (playerScore > competitorScore)
                    wins++;
                else if (Mathf.Approximately(playerScore, competitorScore))
                    wins += 0; // Tie = lose for simplicity
            }

            // Add player to count
            if (wins == competitorScores.Count) wins++; // Player is best

            return (float)wins / total;
        }

        /// <summary>
        /// Calculate probability using ELO-style rating system
        /// </summary>
        public static float CalculateELOProbability(float playerScore, float averageCompetitorScore)
        {
            // ELO expected score formula
            float diff = averageCompetitorScore - playerScore;
            return 1f / (1f + Mathf.Pow(10f, diff / 40f));
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get a qualitative assessment of winning chance
        /// </summary>
        public static string GetChanceAssessment(float winningChance)
        {
            return winningChance switch
            {
                >= 0.90f => "Excellent",
                >= 0.75f => "Very Good",
                >= 0.60f => "Good",
                >= 0.45f => "Fair",
                >= 0.30f => "Challenging",
                >= 0.15f => "Difficult",
                _ => "Very Difficult"
            };
        }

        /// <summary>
        /// Get color for winning chance
        /// </summary>
        public static Color GetChanceColor(float winningChance)
        {
            return winningChance switch
            {
                >= 0.75f => new Color(0.2f, 0.8f, 0.2f),    // Green
                >= 0.50f => new Color(0.8f, 0.8f, 0.2f),    // Yellow
                >= 0.25f => new Color(0.9f, 0.5f, 0.1f),    // Orange
                _ => new Color(0.9f, 0.2f, 0.2f)            // Red
            };
        }

        /// <summary>
        /// Recommend bid parameters based on situation
        /// </summary>
        public static BidParameters RecommendBid(
            ContractData contract,
            float playerReputation,
            float playerTechLevel,
            float desiredWinChance = 0.7f)
        {
            var bid = BidParameters.CreateDefault(contract);

            // Adjust based on desired win chance
            if (desiredWinChance >= 0.8f)
            {
                // Aggressive bid
                bid = BidParameters.CreateAggressiveBid(contract, 0.2f, 0.25f);
            }
            else if (desiredWinChance >= 0.5f)
            {
                // Balanced bid
                bid = BidParameters.CreateBalancedBid(contract, 0.1f);
            }
            else
            {
                // Premium bid - charge more, deliver quality
                bid = BidParameters.CreatePremiumBid(contract, 0.1f, 0.2f);
            }

            return bid;
        }

        #endregion
    }
}
