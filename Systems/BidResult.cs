using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Detailed breakdown of why a bid won or lost
    /// </summary>
    [Serializable]
    public class BidScoreBreakdown
    {
        public float reputationScore;
        public float techMatchScore;
        public float priceScore;
        public float deadlineScore;
        public float qualityScore;
        public float warrantyScore;
        public float totalScore;

        public Dictionary<string, float> ToDictionary()
        {
            return new Dictionary<string, float>
            {
                { "Reputation", reputationScore },
                { "Tech Match", techMatchScore },
                { "Price", priceScore },
                { "Deadline", deadlineScore },
                { "Quality", qualityScore },
                { "Warranty", warrantyScore },
                { "Total", totalScore }
            };
        }
    }

    /// <summary>
    /// Represents the result of a bid submission
    /// </summary>
    [Serializable]
    public class BidResult
    {
        [Header("Outcome")]
        public bool isWinner;
        public float winningChance;
        public float playerFinalScore;
        public float winningScore;
        public string winningCompetitorName;

        [Header("Score Breakdown")]
        public BidScoreBreakdown playerBreakdown;
        public List<BidScoreBreakdown> competitorBreakdowns;

        [Header("Competitor Bids")]
        public List<CompetitorBid> competitorBids;
        public int playerRank;
        public int totalBidders;

        [Header("Feedback")]
        public string winReason;
        public string loseReason;
        public List<string> improvementSuggestions;

        [Header("Rewards (if won)")]
        public float expectedReward;
        public float expectedReputationGain;
        public float upfrontPayment;

        [Header("Next Steps")]
        public bool canRebid;
        public float rebidCooldown;
        public float reputationImpact;

        /// <summary>
        /// Create a winning result
        /// </summary>
        public static BidResult CreateWin(
            float playerScore, 
            List<CompetitorBid> competitors,
            BidScoreBreakdown breakdown,
            ContractData contract)
        {
            var result = new BidResult
            {
                isWinner = true,
                winningChance = 1f,
                playerFinalScore = playerScore,
                playerBreakdown = breakdown,
                competitorBids = competitors,
                playerRank = 1,
                totalBidders = competitors.Count + 1,
                winningScore = playerScore,
                expectedReward = contract.baseReward,
                expectedReputationGain = contract.reputationReward,
                upfrontPayment = contract.GetUpfrontPayment(),
                canRebid = false,
                improvementSuggestions = new List<string>()
            };

            result.GenerateWinReason();
            return result;
        }

        /// <summary>
        /// Create a losing result
        /// </summary>
        public static BidResult CreateLoss(
            float playerScore,
            float winningScore,
            float winningChance,
            List<CompetitorBid> competitors,
            BidScoreBreakdown breakdown,
            ContractData contract)
        {
            var winner = competitors.Count > 0 ? competitors[0] : null;
            
            var result = new BidResult
            {
                isWinner = false,
                winningChance = winningChance,
                playerFinalScore = playerScore,
                winningScore = winningScore,
                winningCompetitorName = winner?.competitorName ?? "Unknown",
                playerBreakdown = breakdown,
                competitorBids = competitors,
                totalBidders = competitors.Count + 1,
                expectedReward = 0,
                expectedReputationGain = 0,
                upfrontPayment = 0,
                canRebid = true,
                rebidCooldown = 24f, // 24 hours
                improvementSuggestions = new List<string>()
            };

            // Calculate player rank
            result.playerRank = 1;
            foreach (var competitor in competitors)
            {
                if (competitor.finalScore > playerScore)
                    result.playerRank++;
            }

            result.GenerateLoseReason();
            result.GenerateImprovementSuggestions();
            return result;
        }

        /// <summary>
        /// Generate win reason based on score breakdown
        /// </summary>
        private void GenerateWinReason()
        {
            var reasons = new List<string>();

            if (playerBreakdown.priceScore >= 90f)
                reasons.Add("highly competitive pricing");
            if (playerBreakdown.techMatchScore >= 90f)
                reasons.Add("excellent technical capabilities");
            if (playerBreakdown.deadlineScore >= 90f)
                reasons.Add("fast delivery timeline");
            if (playerBreakdown.reputationScore >= 90f)
                reasons.Add("outstanding reputation");
            if (playerBreakdown.qualityScore >= 90f)
                reasons.Add("premium quality commitment");

            if (reasons.Count == 0)
                winReason = "Your bid was well-rounded and met all requirements.";
            else if (reasons.Count == 1)
                winReason = $"Your bid won due to {reasons[0]}.";
            else
                winReason = $"Your bid won due to {string.Join(", ", reasons.GetRange(0, reasons.Count - 1))} and {reasons[reasons.Count - 1]}.";
        }

        /// <summary>
        /// Generate lose reason based on score breakdown
        /// </summary>
        private void GenerateLoseReason()
        {
            // Find the weakest area
            var scores = new Dictionary<string, float>
            {
                { "pricing", playerBreakdown.priceScore },
                { "technical capabilities", playerBreakdown.techMatchScore },
                { "delivery timeline", playerBreakdown.deadlineScore },
                { "reputation", playerBreakdown.reputationScore },
                { "quality commitment", playerBreakdown.qualityScore }
            };

            string weakest = "";
            float lowestScore = float.MaxValue;
            foreach (var kvp in scores)
            {
                if (kvp.Value < lowestScore)
                {
                    lowestScore = kvp.Value;
                    weakest = kvp.Key;
                }
            }

            if (playerRank == 2)
                loseReason = $"Close call! You were outbid by {winningCompetitorName}. Your {weakest} held you back.";
            else if (playerRank <= 3)
                loseReason = $"You placed {playerRank}rd. The winner {winningCompetitorName} had stronger {weakest}.";
            else
                loseReason = $"Your bid placed {playerRank}th out of {totalBidders}. Focus on improving your {weakest}.";
        }

        /// <summary>
        /// Generate suggestions for improvement
        /// </summary>
        private void GenerateImprovementSuggestions()
        {
            improvementSuggestions.Clear();

            if (playerBreakdown.priceScore < 70f)
                improvementSuggestions.Add("Consider offering a more competitive price");
            if (playerBreakdown.techMatchScore < 70f)
                improvementSuggestions.Add("Upgrade your technology to meet requirements");
            if (playerBreakdown.deadlineScore < 70f)
                improvementSuggestions.Add("Offer a faster delivery timeline");
            if (playerBreakdown.reputationScore < 70f)
                improvementSuggestions.Add("Complete more contracts to build reputation");
            if (playerBreakdown.qualityScore < 70f)
                improvementSuggestions.Add("Invest more in quality assurance");

            if (improvementSuggestions.Count == 0)
                improvementSuggestions.Add("Your bid was competitive - try again with slight adjustments");
        }

        /// <summary>
        /// Get a formatted summary of the result
        /// </summary>
        public string GetSummary()
        {
            var sb = new StringBuilder();

            if (isWinner)
            {
                sb.AppendLine("<color=green>CONTRACT WON!</color>");
                sb.AppendLine(winReason);
                sb.AppendLine($"Expected Reward: ${expectedReward:N0}");
                sb.AppendLine($"Upfront Payment: ${upfrontPayment:N0}");
                sb.AppendLine($"Reputation Gain: +{expectedReputationGain:F1}");
            }
            else
            {
                sb.AppendLine("<color=red>CONTRACT LOST</color>");
                sb.AppendLine(loseReason);
                sb.AppendLine($"Your Rank: {playerRank} of {totalBidders}");
                sb.AppendLine($"Winning Chance: {winningChance:P1}");
                sb.AppendLine();
                sb.AppendLine("Improvement Suggestions:");
                foreach (var suggestion in improvementSuggestions)
                {
                    sb.AppendLine($"- {suggestion}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get detailed comparison with competitors
        /// </summary>
        public string GetDetailedComparison()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bid Comparison:");
            sb.AppendLine($"{'Bidder',-20} {'Score',-8} {'Price',-12} {'Deadline',-10} {'Rank'}");
            sb.AppendLine(new string('-', 65));

            // Add player
            sb.AppendLine($"{'YOU',-20} {playerFinalScore,-8:F1} {'-',-12} {'-',-10} {playerRank}");

            // Add competitors
            int rank = 1;
            foreach (var competitor in competitorBids)
            {
                string marker = rank < playerRank ? "(ahead)" : rank > playerRank ? "" : "(tied)";
                sb.AppendLine($"{competitor.competitorName + marker,-20} {competitor.finalScore,-8:F1} {competitor.bidPrice,-12:C0} {competitor.bidDeadline,-10:F1}d {rank + 1}");
                rank++;
            }

            return sb.ToString();
        }
    }
}
