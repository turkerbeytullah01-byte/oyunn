using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Static class that defines failure consequences for each failure type and risk level
    /// </summary>
    public static class FailureConsequences
    {
        // Base multipliers for scaling
        private const float BaseMoneyMultiplier = 1000f;
        private const float BaseReputationMultiplier = 2f;
        private const float BaseTimeMultiplier = 10f;

        /// <summary>
        /// Gets the appropriate consequences for a failure type and risk level
        /// </summary>
        public static FailureResult GetConsequences(FailureType type, RiskLevel riskLevel)
        {
            FailureResult result = new FailureResult(type, riskLevel);
            float riskMultiplier = (float)riskLevel / 100f;

            switch (type)
            {
                case FailureType.MinorSetback:
                    ApplyMinorSetbackConsequences(result, riskMultiplier);
                    break;

                case FailureType.Delay:
                    ApplyDelayConsequences(result, riskMultiplier);
                    break;

                case FailureType.CostOverrun:
                    ApplyCostOverrunConsequences(result, riskMultiplier);
                    break;

                case FailureType.PartialFailure:
                    ApplyPartialFailureConsequences(result, riskMultiplier);
                    break;

                case FailureType.MajorFailure:
                    ApplyMajorFailureConsequences(result, riskMultiplier);
                    break;

                case FailureType.Catastrophic:
                    ApplyCatastrophicConsequences(result, riskMultiplier);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets consequences with custom base values
        /// </summary>
        public static FailureResult GetConsequences(FailureType type, RiskLevel riskLevel,
            float baseInvestment, float baseReputation, float baseTimeMinutes)
        {
            FailureResult result = GetConsequences(type, riskLevel);

            // Scale based on provided base values
            result.moneyLost = Mathf.Max(result.moneyLost, baseInvestment * result.investmentLossPercent);
            result.reputationLost = Mathf.Max(result.reputationLost, baseReputation * 0.1f);
            result.timeDelayMinutes = Mathf.Max(result.timeDelayMinutes, baseTimeMinutes * 0.2f);

            return result;
        }

        #region Individual Failure Type Consequences

        private static void ApplyMinorSetbackConsequences(FailureResult result, float riskMultiplier)
        {
            // Minor setbacks: small time delay, minimal financial impact
            result.moneyLost = BaseMoneyMultiplier * 0.1f * riskMultiplier;
            result.recoveryCost = 0f;
            result.reputationLost = BaseReputationMultiplier * 0.1f * riskMultiplier;
            result.clientRelationshipDamage = 0.02f * riskMultiplier;
            result.timeDelayMinutes = BaseTimeMultiplier * 5f * (1f + riskMultiplier);
            result.progressLost = 0f;
            result.investmentLossPercent = 0f;
            result.isRecoverable = true;
            result.recoveryPrice = 0f; // Auto-recovers
        }

        private static void ApplyDelayConsequences(FailureResult result, float riskMultiplier)
        {
            // Delays: significant time impact, small financial cost
            result.moneyLost = BaseMoneyMultiplier * 0.3f * riskMultiplier;
            result.recoveryCost = BaseMoneyMultiplier * 0.2f * riskMultiplier;
            result.reputationLost = BaseReputationMultiplier * 0.3f * riskMultiplier;
            result.clientRelationshipDamage = 0.05f * riskMultiplier;
            result.timeDelayMinutes = BaseTimeMultiplier * 30f * (1f + riskMultiplier * 2f);
            result.progressLost = 0f;
            result.investmentLossPercent = 0f;
            result.isRecoverable = true;
            result.recoveryPrice = BaseMoneyMultiplier * 0.5f * riskMultiplier; // Can pay to reduce delay
        }

        private static void ApplyCostOverrunConsequences(FailureResult result, float riskMultiplier)
        {
            // Cost overruns: financial impact is primary
            result.moneyLost = BaseMoneyMultiplier * 0.5f * riskMultiplier;
            result.recoveryCost = BaseMoneyMultiplier * (0.5f + riskMultiplier);
            result.reputationLost = BaseReputationMultiplier * 0.2f * riskMultiplier;
            result.clientRelationshipDamage = 0.03f * riskMultiplier;
            result.timeDelayMinutes = BaseTimeMultiplier * 10f * riskMultiplier;
            result.progressLost = 0f;
            result.investmentLossPercent = 0.1f * riskMultiplier;
            result.isRecoverable = true;
            result.recoveryPrice = result.recoveryCost; // Must pay to continue
        }

        private static void ApplyPartialFailureConsequences(FailureResult result, float riskMultiplier)
        {
            // Partial failures: lose some progress
            result.moneyLost = BaseMoneyMultiplier * (0.3f + riskMultiplier * 0.5f);
            result.recoveryCost = BaseMoneyMultiplier * 0.4f * riskMultiplier;
            result.reputationLost = BaseReputationMultiplier * (0.5f + riskMultiplier);
            result.clientRelationshipDamage = 0.1f * riskMultiplier;
            result.timeDelayMinutes = BaseTimeMultiplier * 20f * (1f + riskMultiplier);
            result.progressLost = 0.2f + (riskMultiplier * 0.3f); // 20-50% progress lost
            result.progressLost = Mathf.Min(result.progressLost, 0.5f);
            result.investmentLossPercent = 0.15f * riskMultiplier;
            result.isRecoverable = true;
            result.recoveryPrice = BaseMoneyMultiplier * (0.8f + riskMultiplier);
        }

        private static void ApplyMajorFailureConsequences(FailureResult result, float riskMultiplier)
        {
            // Major failures: significant penalties
            result.moneyLost = BaseMoneyMultiplier * (1f + riskMultiplier * 2f);
            result.recoveryCost = BaseMoneyMultiplier * (1f + riskMultiplier);
            result.reputationLost = BaseReputationMultiplier * (1f + riskMultiplier * 2f);
            result.clientRelationshipDamage = 0.2f + (riskMultiplier * 0.2f);
            result.timeDelayMinutes = BaseTimeMultiplier * 60f * (1f + riskMultiplier * 3f);
            result.progressLost = 0.4f + (riskMultiplier * 0.3f); // 40-70% progress lost
            result.progressLost = Mathf.Min(result.progressLost, 0.7f);
            result.investmentLossPercent = 0.3f + (riskMultiplier * 0.2f);
            result.isRecoverable = true;
            result.recoveryPrice = BaseMoneyMultiplier * (2f + riskMultiplier * 2f);
        }

        private static void ApplyCatastrophicConsequences(FailureResult result, float riskMultiplier)
        {
            // Catastrophic failures: severe penalties, may not be recoverable
            result.moneyLost = BaseMoneyMultiplier * (3f + riskMultiplier * 5f);
            result.recoveryCost = BaseMoneyMultiplier * (2f + riskMultiplier * 3f);
            result.reputationLost = BaseReputationMultiplier * (3f + riskMultiplier * 5f);
            result.clientRelationshipDamage = 0.5f + (riskMultiplier * 0.3f);
            result.timeDelayMinutes = BaseTimeMultiplier * 180f * (1f + riskMultiplier * 5f);
            result.progressLost = 0.7f + (riskMultiplier * 0.25f); // 70-95% progress lost
            result.progressLost = Mathf.Min(result.progressLost, 0.95f);
            result.investmentLossPercent = 0.5f + (riskMultiplier * 0.4f);
            result.isRecoverable = false; // Cannot easily recover
            result.recoveryPrice = BaseMoneyMultiplier * (5f + riskMultiplier * 5f);
        }

        #endregion

        #region Risk Level Specific Consequences

        /// <summary>
        /// Gets additional consequences specific to the risk level
        /// </summary>
        public static void ApplyRiskLevelModifiers(FailureResult result, RiskLevel level)
        {
            switch (level)
            {
                case RiskLevel.None:
                case RiskLevel.VeryLow:
                    // No additional modifiers
                    break;

                case RiskLevel.Low:
                    result.reputationLost *= 0.9f;
                    break;

                case RiskLevel.Medium:
                    // Base consequences
                    break;

                case RiskLevel.High:
                    result.moneyLost *= 1.2f;
                    result.reputationLost *= 1.3f;
                    break;

                case RiskLevel.VeryHigh:
                    result.moneyLost *= 1.5f;
                    result.reputationLost *= 1.6f;
                    result.timeDelayMinutes *= 1.3f;
                    break;

                case RiskLevel.Critical:
                    result.moneyLost *= 2f;
                    result.reputationLost *= 2f;
                    result.timeDelayMinutes *= 1.5f;
                    result.progressLost = Mathf.Min(result.progressLost * 1.2f, 0.95f);
                    break;
            }
        }

        #endregion

        #region Context-Specific Consequences

        /// <summary>
        /// Gets consequences tailored for research projects
        /// </summary>
        public static FailureResult GetResearchConsequences(FailureType type, RiskLevel riskLevel,
            float researchCost, float researchTimeMinutes)
        {
            FailureResult result = GetConsequences(type, riskLevel);

            // Research-specific scaling
            float costMultiplier = researchCost / BaseMoneyMultiplier;
            float timeMultiplier = researchTimeMinutes / (BaseTimeMultiplier * 60f);

            result.moneyLost *= Mathf.Max(0.5f, costMultiplier * 0.5f);
            result.recoveryCost *= Mathf.Max(0.5f, costMultiplier * 0.5f);
            result.timeDelayMinutes *= Mathf.Max(0.5f, timeMultiplier * 0.5f);
            result.reputationLost *= 0.8f; // Research failures less damaging to reputation

            return result;
        }

        /// <summary>
        /// Gets consequences tailored for contracts
        /// </summary>
        public static FailureResult GetContractConsequences(FailureType type, RiskLevel riskLevel,
            float contractValue, float deadlinePressure)
        {
            FailureResult result = GetConsequences(type, riskLevel);

            // Contract-specific scaling
            float valueMultiplier = contractValue / BaseMoneyMultiplier;
            float deadlineMultiplier = 1f + deadlinePressure;

            result.moneyLost *= Mathf.Max(0.8f, valueMultiplier * 0.8f);
            result.recoveryCost *= Mathf.Max(0.8f, valueMultiplier * 0.8f);
            result.reputationLost *= 1.2f; // Contract failures more damaging to reputation
            result.reputationLost *= deadlineMultiplier; // Worse if deadline pressure was high
            result.clientRelationshipDamage *= deadlineMultiplier;

            return result;
        }

        /// <summary>
        /// Gets consequences for operational failures
        /// </summary>
        public static FailureResult GetOperationConsequences(FailureType type, RiskLevel riskLevel,
            float operationScale)
        {
            FailureResult result = GetConsequences(type, riskLevel);

            // Operations scale with operation size
            float scaleMultiplier = Mathf.Max(0.3f, operationScale);

            result.moneyLost *= scaleMultiplier;
            result.recoveryCost *= scaleMultiplier;
            result.reputationLost *= Mathf.Min(scaleMultiplier, 2f);

            return result;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the base probability weight for each failure type
        /// Used when randomly determining failure type
        /// </summary>
        public static float GetFailureTypeWeight(FailureType type)
        {
            return type switch
            {
                FailureType.MinorSetback => 30f,  // Most common
                FailureType.Delay => 25f,
                FailureType.CostOverrun => 20f,
                FailureType.PartialFailure => 15f,
                FailureType.MajorFailure => 8f,
                FailureType.Catastrophic => 2f,   // Rare
                _ => 10f
            };
        }

        /// <summary>
        /// Gets a human-readable description of a failure type
        /// </summary>
        public static string GetFailureTypeDescription(FailureType type)
        {
            return type switch
            {
                FailureType.MinorSetback => "Minor Setback - Small delay with minimal impact",
                FailureType.Delay => "Delay - Significant time delay",
                FailureType.CostOverrun => "Cost Overrun - Additional funds required",
                FailureType.PartialFailure => "Partial Failure - Some progress lost",
                FailureType.MajorFailure => "Major Failure - Significant penalties",
                FailureType.Catastrophic => "Catastrophic Failure - Severe consequences",
                _ => "Unknown Failure"
            };
        }

        /// <summary>
        /// Gets the severity rating (1-10) for a failure type
        /// </summary>
        public static int GetSeverityRating(FailureType type)
        {
            return type switch
            {
                FailureType.MinorSetback => 2,
                FailureType.Delay => 4,
                FailureType.CostOverrun => 5,
                FailureType.PartialFailure => 6,
                FailureType.MajorFailure => 8,
                FailureType.Catastrophic => 10,
                _ => 5
            };
        }

        #endregion
    }
}
