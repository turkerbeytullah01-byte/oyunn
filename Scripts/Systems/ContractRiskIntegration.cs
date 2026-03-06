using System;
using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Integrates the Risk System with Contract work
    /// Handles risk calculation for bidding and delivery
    /// </summary>
    public class ContractRiskIntegration : MonoBehaviour
    {
        public static ContractRiskIntegration Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableContractRisk = true;
        [SerializeField] private float deadlinePressureMultiplier = 1.5f;
        [SerializeField] private float underpricingRiskMultiplier = 1.3f;

        [Header("Events")]
        public Action<string, RiskProfile, float> OnContractBidCalculated;
        public Action<string, RiskRollResult> OnContractDeliveryRoll;
        public Action<string, FailureResult> OnContractFailure;
        public Action<string, float> OnContractSuccess;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #region Bid Risk Calculation

        /// <summary>
        /// Calculates the risk profile for a contract bid
        /// </summary>
        public RiskProfile CalculateBidRisk(
            ContractData contract,
            float companyTechLevel,
            float companyReputation,
            float bidPrice,
            float estimatedCompletionTime)
        {
            if (!enableContractRisk)
                return RiskProfile.FromValues(0, 0, 0);

            // Base risk from contract requirements
            float techRisk = contract.TechRequirement * 1.0f;
            float finRisk = contract.FinancialRequirement * 1.0f;
            float secRisk = contract.SecurityRequirement * 1.2f;

            // Deadline pressure modifier
            float deadlinePressure = CalculateDeadlinePressure(estimatedCompletionTime, contract.DeadlineMinutes);
            float deadlineMod = 1f + (deadlinePressure * deadlinePressureMultiplier);
            techRisk *= deadlineMod;
            finRisk *= deadlineMod;

            // Underpricing risk
            float underpricing = CalculateUnderpricing(bidPrice, contract.EstimatedValue);
            float pricingMod = 1f + (underpricing * underpricingRiskMultiplier);
            finRisk *= pricingMod;

            // Client reputation factor
            float clientMod = 1f + (1f - contract.ClientReputation) * 0.3f;
            finRisk *= clientMod;

            var profile = RiskProfile.FromValues(
                Mathf.Clamp(techRisk, 0, 100),
                Mathf.Clamp(finRisk, 0, 100),
                Mathf.Clamp(secRisk, 0, 100)
            );

            profile.description = $"Contract: {contract.Name}";
            profile.sourceContext = "Contract";

            return profile;
        }

        /// <summary>
        /// Creates risk modifiers for contract work
        /// </summary>
        public RiskModifiers CreateContractModifiers(
            float companyReputation,
            float timelinePressure,
            float budgetAdequacy,
            float previousSuccessBonus = 0f)
        {
            return new RiskModifiers
            {
                reputationReduction = Mathf.Min(companyReputation * 0.25f, 25f),
                timelineModifier = Mathf.Clamp(timelinePressure, -20f, 20f),
                budgetModifier = Mathf.Clamp(budgetAdequacy, -10f, 10f),
                previousSuccessBonus = previousSuccessBonus
            };
        }

        /// <summary>
        /// Calculates the deadline pressure factor
        /// </summary>
        private float CalculateDeadlinePressure(float estimatedTime, float deadline)
        {
            if (deadline <= 0) return 0f;

            float buffer = deadline - estimatedTime;
            float bufferPercent = buffer / deadline;

            // Negative buffer = rushed, Positive buffer = relaxed
            if (bufferPercent < 0)
                return Mathf.Abs(bufferPercent) * 2f; // Double penalty for being over deadline

            return -bufferPercent * 0.5f; // Bonus for generous deadline
        }

        /// <summary>
        /// Calculates underpricing risk
        /// </summary>
        private float CalculateUnderpricing(float bidPrice, float estimatedValue)
        {
            if (estimatedValue <= 0) return 0f;

            float priceRatio = bidPrice / estimatedValue;

            if (priceRatio < 0.7f)
                return (0.7f - priceRatio) * 2f; // Significant risk for underpricing

            if (priceRatio < 0.9f)
                return (0.9f - priceRatio) * 0.5f; // Moderate risk

            return 0f; // No risk for fair pricing
        }

        #endregion

        #region Delivery Risk Rolling

        /// <summary>
        /// Performs a risk roll for contract delivery
        /// </summary>
        public RiskRollResult RollForContractDelivery(
            string contractId,
            RiskProfile riskProfile,
            RiskModifiers modifiers)
        {
            if (!enableContractRisk)
            {
                return new RiskRollResult
                {
                    Success = true,
                    Roll = 100f,
                    FailureChance = 0f,
                    Margin = 100f,
                    IsCritical = false
                };
            }

            // Calculate failure chance
            float failureChance = RiskManager.Instance?.CalculateContractFailureChance(riskProfile, modifiers)
                ?? RiskCalculator.CalculateContractFailureChance(riskProfile, modifiers);

            // Roll for success
            RiskRollResult result = RiskCalculator.RollDetailed(failureChance);

            // Log and notify
            Debug.Log($"[ContractRisk] '{contractId}': {result}");
            OnContractDeliveryRoll?.Invoke(contractId, result);

            // Apply outcomes
            if (result.Success)
            {
                HandleContractSuccess(contractId, result);
            }
            else
            {
                HandleContractFailure(contractId, riskProfile, result);
            }

            return result;
        }

        /// <summary>
        /// Handles successful contract delivery
        /// </summary>
        private void HandleContractSuccess(string contractId, RiskRollResult result)
        {
            // Calculate reputation gain based on margin
            float baseRepGain = 5f;
            float marginBonus = result.Margin > 10f ? 2f : 0f;
            float totalRepGain = baseRepGain + marginBonus;

            OnContractSuccess?.Invoke(contractId, totalRepGain);

            // Notify risk manager
            RiskManager.Instance?.ApplySuccessBonuses("contract", contractId);

            Debug.Log($"[ContractRisk] '{contractId}' delivered successfully! Reputation +{totalRepGain:F1}");
        }

        /// <summary>
        /// Handles contract failure
        /// </summary>
        private void HandleContractFailure(string contractId, RiskProfile riskProfile, RiskRollResult result)
        {
            // Determine failure type
            FailureType failureType = DetermineContractFailureType(result, riskProfile);

            // Get consequences
            FailureResult failure = FailureConsequences.GetContractConsequences(
                failureType,
                RiskProfile.ValueToRiskLevel(riskProfile.GetOverallRisk()),
                5000f, // Base contract value
                0.5f   // Deadline pressure
            );

            failure.failureId = contractId;
            failure.context = "contract";

            // Apply consequences
            OnContractFailure?.Invoke(contractId, failure);
            RiskManager.Instance?.ApplyFailureConsequences(failure);

            Debug.Log($"[ContractRisk] '{contractId}' delivery failed! {failure.failureType}");
        }

        /// <summary>
        /// Determines the type of failure for contracts
        /// </summary>
        private FailureType DetermineContractFailureType(RiskRollResult result, RiskProfile riskProfile)
        {
            float failureMargin = result.FailureChance - result.Roll;
            float riskLevel = riskProfile.GetOverallRisk();

            if (failureMargin > 30f && riskLevel > 80f)
                return FailureType.Catastrophic;
            if (failureMargin > 18f || riskLevel > 75f)
                return FailureType.MajorFailure;
            if (failureMargin > 10f || riskLevel > 60f)
                return FailureType.PartialFailure;
            if (failureMargin > 5f || riskLevel > 45f)
                return FailureType.CostOverrun;
            if (failureMargin > 2f || riskLevel > 30f)
                return FailureType.Delay;

            return FailureType.MinorSetback;
        }

        #endregion

        #region Bid Evaluation

        /// <summary>
        /// Evaluates a bid and returns risk assessment
        /// </summary>
        public BidEvaluation EvaluateBid(
            ContractData contract,
            float bidPrice,
            float estimatedCompletionTime,
            float companyTechLevel,
            float companyReputation)
        {
            RiskProfile riskProfile = CalculateBidRisk(contract, companyTechLevel, companyReputation,
                bidPrice, estimatedCompletionTime);

            RiskModifiers modifiers = CreateContractModifiers(companyReputation, 0f, 0f);

            float failureChance = RiskCalculator.CalculateContractFailureChance(riskProfile, modifiers);

            return new BidEvaluation
            {
                Contract = contract,
                BidPrice = bidPrice,
                EstimatedTime = estimatedCompletionTime,
                RiskProfile = riskProfile,
                FailureChance = failureChance,
                SuccessChance = 100f - failureChance,
                Recommended = failureChance < 50f,
                RiskLevel = RiskProfile.ValueToRiskLevel(failureChance)
            };
        }

        /// <summary>
        /// Gets bid recommendations
        /// </summary>
        public string[] GetBidRecommendations(BidEvaluation evaluation)
        {
            var recommendations = new System.Collections.Generic.List<string>();

            if (evaluation.FailureChance > 70f)
            {
                recommendations.Add("HIGH RISK: Consider increasing bid price or extending timeline");
            }
            else if (evaluation.FailureChance > 50f)
            {
                recommendations.Add("MODERATE RISK: Ensure adequate resources are allocated");
            }

            if (evaluation.RiskProfile.technicalRisk > 60f)
                recommendations.Add("Technical requirements exceed current capabilities");

            if (evaluation.RiskProfile.financialRisk > 60f)
                recommendations.Add("Financial risk is high - consider insurance");

            if (evaluation.RiskProfile.securityRisk > 60f)
                recommendations.Add("Security requirements are demanding");

            if (recommendations.Count == 0)
                recommendations.Add("Risk profile is acceptable for bidding");

            return recommendations.ToArray();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets a display summary for contract risk
        /// </summary>
        public string GetContractRiskSummary(ContractData contract, RiskProfile profile, float failureChance)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Contract Risk: {contract.Name} ===");
            sb.AppendLine();
            sb.AppendLine(RiskDisplay.FormatRiskProfile(profile));
            sb.AppendLine();
            sb.AppendLine($"Failure Chance: {failureChance:F1}%");
            sb.AppendLine($"Success Chance: {100f - failureChance:F1}%");
            sb.AppendLine();

            var evaluation = new BidEvaluation
            {
                RiskProfile = profile,
                FailureChance = failureChance
            };

            sb.AppendLine("Recommendations:");
            foreach (var rec in GetBidRecommendations(evaluation))
            {
                sb.AppendLine($"  • {rec}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats a bid for display
        /// </summary>
        public string FormatBidDisplay(BidEvaluation bid)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Bid: {bid.Contract.Name} ===");
            sb.AppendLine($"Bid Price: ${bid.BidPrice:N0}");
            sb.AppendLine($"Est. Time: {bid.EstimatedTime:F0} min");
            sb.AppendLine();
            sb.AppendLine(RiskDisplay.FormatRiskProfile(bid.RiskProfile, "Risk Assessment"));
            sb.AppendLine();
            sb.AppendLine($"Failure: {bid.FailureChance:F1}% | Success: {bid.SuccessChance:F1}%");
            sb.AppendLine($"Recommendation: {(bid.Recommended ? "BID" : "CAUTION")}");

            return sb.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Data class for contract information
    /// </summary>
    [Serializable]
    public class ContractData
    {
        public string Id;
        public string Name;
        public string Description;
        public string ClientName;

        [Range(0, 100)]
        public float TechRequirement;

        [Range(0, 100)]
        public float FinancialRequirement;

        [Range(0, 100)]
        public float SecurityRequirement;

        [Range(0, 1)]
        public float ClientReputation;

        public float EstimatedValue;
        public float DeadlineMinutes;
        public float PenaltyPerMinute;
    }

    /// <summary>
    /// Result of bid evaluation
    /// </summary>
    [Serializable]
    public class BidEvaluation
    {
        public ContractData Contract;
        public float BidPrice;
        public float EstimatedTime;
        public RiskProfile RiskProfile;
        public float FailureChance;
        public float SuccessChance;
        public bool Recommended;
        public RiskLevel RiskLevel;

        public override string ToString()
        {
            return $"Bid[{Contract?.Name}] Risk:{FailureChance:F1}% Rec:{Recommended}";
        }
    }
}
