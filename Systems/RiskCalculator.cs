using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Static utility class for risk calculations
    /// </summary>
    public static class RiskCalculator
    {
        // Constants for failure chance clamping
        public const float MinFailureChance = 5f;   // Always at least 5% chance of failure
        public const float MaxFailureChance = 95f;  // Always at least 5% chance of success
        public const float CriticalThreshold = 85f; // Threshold for critical risk warnings

        // Weight multipliers for different contexts
        public const float ResearchTechWeight = 1.5f;
        public const float ResearchFinancialWeight = 1.0f;
        public const float ResearchSecurityWeight = 0.8f;

        public const float ContractTechWeight = 1.0f;
        public const float ContractFinancialWeight = 1.3f;
        public const float ContractSecurityWeight = 1.2f;

        /// <summary>
        /// Calculates the final failure chance based on risk profile and modifiers
        /// Formula: Base = (Tech + Financial + Security) / 3
        ///          Modified = Base - TotalReductions + TotalIncreases
        ///          Final = Clamp(Modified, 5%, 95%)
        /// </summary>
        public static float CalculateFailureChance(RiskProfile profile, RiskModifiers modifiers)
        {
            if (profile == null)
            {
                Debug.LogWarning("[RiskCalculator] RiskProfile is null, using default");
                profile = new RiskProfile();
            }

            if (modifiers == null)
            {
                modifiers = RiskModifiers.Default;
            }

            // Calculate base failure chance from risk profile
            float baseChance = profile.GetOverallRisk();

            // Apply all modifiers
            float modifiedChance = baseChance - modifiers.GetNetModifier();

            // Clamp to valid range (always some chance of success/failure)
            float finalChance = Mathf.Clamp(modifiedChance, MinFailureChance, MaxFailureChance);

            return finalChance;
        }

        /// <summary>
        /// Calculates weighted failure chance for research projects
        /// </summary>
        public static float CalculateResearchFailureChance(RiskProfile profile, RiskModifiers modifiers)
        {
            if (profile == null) profile = new RiskProfile();
            if (modifiers == null) modifiers = RiskModifiers.Default;

            float weightedRisk = profile.GetWeightedRisk(
                ResearchTechWeight,
                ResearchFinancialWeight,
                ResearchSecurityWeight
            );

            float modifiedChance = weightedRisk - modifiers.GetNetModifier();
            return Mathf.Clamp(modifiedChance, MinFailureChance, MaxFailureChance);
        }

        /// <summary>
        /// Calculates weighted failure chance for contracts
        /// </summary>
        public static float CalculateContractFailureChance(RiskProfile profile, RiskModifiers modifiers)
        {
            if (profile == null) profile = new RiskProfile();
            if (modifiers == null) modifiers = RiskModifiers.Default;

            float weightedRisk = profile.GetWeightedRisk(
                ContractTechWeight,
                ContractFinancialWeight,
                ContractSecurityWeight
            );

            float modifiedChance = weightedRisk - modifiers.GetNetModifier();
            return Mathf.Clamp(modifiedChance, MinFailureChance, MaxFailureChance);
        }

        /// <summary>
        /// Rolls for success against a failure chance
        /// Returns true if successful, false if failed
        /// </summary>
        public static bool RollForSuccess(float failureChance)
        {
            float roll = Random.Range(0f, 100f);
            return roll >= failureChance;
        }

        /// <summary>
        /// Rolls for success with detailed result
        /// </summary>
        public static RiskRollResult RollDetailed(float failureChance)
        {
            float roll = Random.Range(0f, 100f);
            bool success = roll >= failureChance;
            float margin = success ? roll - failureChance : failureChance - roll;

            return new RiskRollResult
            {
                Success = success,
                Roll = roll,
                FailureChance = failureChance,
                Margin = margin,
                IsCritical = margin < 5f // Within 5% of threshold
            };
        }

        /// <summary>
        /// Calculates risk level from a failure chance
        /// </summary>
        public static RiskLevel GetRiskLevelFromChance(float failureChance)
        {
            return RiskProfile.ValueToRiskLevel(failureChance);
        }

        /// <summary>
        /// Determines if a risk is considered critical
        /// </summary>
        public static bool IsCriticalRisk(float failureChance)
        {
            return failureChance >= CriticalThreshold;
        }

        /// <summary>
        /// Determines if a risk is considered safe (low chance of failure)
        /// </summary>
        public static bool IsSafeRisk(float failureChance)
        {
            return failureChance <= 25f;
        }

        /// <summary>
        /// Calculates the success chance from failure chance
        /// </summary>
        public static float GetSuccessChance(float failureChance)
        {
            return 100f - failureChance;
        }

        /// <summary>
        /// Gets the risk trend based on modifier direction
        /// </summary>
        public static RiskTrend GetRiskTrend(RiskModifiers modifiers)
        {
            float net = modifiers?.GetNetModifier() ?? 0f;

            if (net > 10f) return RiskTrend.Improving;
            if (net < -10f) return RiskTrend.Worsening;
            return RiskTrend.Stable;
        }

        /// <summary>
        /// Calculates combined risk for multiple profiles (e.g., parallel projects)
        /// </summary>
        public static float CalculateCombinedRisk(params RiskProfile[] profiles)
        {
            if (profiles == null || profiles.Length == 0) return 0f;

            float totalRisk = 0f;
            foreach (var profile in profiles)
            {
                if (profile != null)
                    totalRisk += profile.GetOverallRisk();
            }

            // Combined risk is higher than average due to complexity
            float averageRisk = totalRisk / profiles.Length;
            float complexityFactor = 1f + (profiles.Length - 1) * 0.1f; // +10% per additional project

            return Mathf.Min(averageRisk * complexityFactor, 100f);
        }

        /// <summary>
        /// Estimates potential losses from a failure
        /// </summary>
        public static float EstimatePotentialLoss(RiskProfile profile, float investmentAmount)
        {
            float riskFactor = profile.GetOverallRisk() / 100f;
            return investmentAmount * riskFactor * Random.Range(0.5f, 1.5f);
        }

        /// <summary>
        /// Gets recommended mitigation strategies
        /// </summary>
        public static string[] GetRecommendedMitigation(RiskProfile profile)
        {
            var recommendations = new System.Collections.Generic.List<string>();

            if (profile.technicalRisk > 50f)
                recommendations.Add("Invest in R&D infrastructure");

            if (profile.financialRisk > 50f)
                recommendations.Add("Secure additional funding or insurance");

            if (profile.securityRisk > 50f)
                recommendations.Add("Increase security investments");

            if (recommendations.Count == 0)
                recommendations.Add("Risk levels are acceptable");

            return recommendations.ToArray();
        }
    }

    /// <summary>
    /// Result of a risk roll with detailed information
    /// </summary>
    public struct RiskRollResult
    {
        public bool Success;
        public float Roll;
        public float FailureChance;
        public float Margin;
        public bool IsCritical;

        public override string ToString()
        {
            string status = Success ? "SUCCESS" : "FAILURE";
            string critical = IsCritical ? " (CRITICAL!)" : "";
            return $"{status}{critical} - Roll: {Roll:F1}% vs {FailureChance:F1}% threshold (margin: {Margin:F1}%)";
        }
    }

    /// <summary>
    /// Risk trend direction
    /// </summary>
    public enum RiskTrend
    {
        Improving,  // Risk is decreasing
        Stable,     // Risk is stable
        Worsening   // Risk is increasing
    }
}
