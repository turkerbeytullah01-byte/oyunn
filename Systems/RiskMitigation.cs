using System;
using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Provides methods for mitigating risk through various investments and strategies
    /// </summary>
    public class RiskMitigation
    {
        #region Constants

        // Security investment effectiveness
        private const float SecurityInvestmentEfficiency = 0.4f; // 40% max reduction
        private const float SecurityDiminishingReturns = 0.7f;   // Diminishing returns factor
        private const float SecurityCostPerPercent = 500f;       // Base cost per % reduction

        // Consultant effectiveness
        private const float ConsultantMaxReduction = 15f;        // 15% max reduction
        private const float ConsultantCostPerPercent = 300f;     // Cost per % reduction
        private const float ConsultantDurationMinutes = 60f;     // Duration of consultant effect

        // Timeline extension effectiveness
        private const float TimelineMaxReduction = 20f;          // 20% max reduction
        private const float TimelineEfficiency = 0.5f;           // 50% efficiency (10% time = 5% reduction)

        // Budget increase effectiveness
        private const float BudgetMaxReduction = 10f;            // 10% max reduction
        private const float BudgetEfficiency = 0.2f;             // 20% efficiency (10% budget = 2% reduction)

        // Quality investment effectiveness
        private const float QualityMaxReduction = 20f;           // 20% max reduction
        private const float QualityCostPerPercent = 400f;        // Cost per % reduction

        // Insurance effectiveness
        private const float InsuranceCostPercent = 0.05f;        // 5% of project cost
        private const float InsuranceCoveragePercent = 0.5f;     // Covers 50% of losses

        #endregion

        #region Security Investment

        /// <summary>
        /// Invests in security to reduce security risk
        /// Returns the actual reduction achieved
        /// </summary>
        public static float InvestInSecurity(float amount, float currentSecurityRisk)
        {
            if (amount <= 0 || currentSecurityRisk <= 0) return 0f;

            // Calculate potential reduction based on investment
            float potentialReduction = (amount / SecurityCostPerPercent) * SecurityInvestmentEfficiency;

            // Apply diminishing returns
            float currentReduction = SecurityInvestmentEfficiency - (currentSecurityRisk / 100f * SecurityInvestmentEfficiency);
            float diminishingFactor = Mathf.Pow(SecurityDiminishingReturns, currentReduction / 10f);

            float actualReduction = potentialReduction * diminishingFactor;

            // Cap at max reduction and available risk
            float maxPossible = Mathf.Min(SecurityInvestmentEfficiency * 100f, currentSecurityRisk);
            actualReduction = Mathf.Min(actualReduction, maxPossible);

            return actualReduction;
        }

        /// <summary>
        /// Calculates the cost to achieve a target security risk reduction
        /// </summary>
        public static float CalculateSecurityCost(float targetReduction, float currentSecurityRisk)
        {
            if (targetReduction <= 0) return 0f;

            float maxPossible = Mathf.Min(SecurityInvestmentEfficiency * 100f, currentSecurityRisk);
            targetReduction = Mathf.Min(targetReduction, maxPossible);

            // Reverse calculation with diminishing returns
            float currentReduction = SecurityInvestmentEfficiency - (currentSecurityRisk / 100f * SecurityInvestmentEfficiency);
            float diminishingFactor = Mathf.Pow(SecurityDiminishingReturns, currentReduction / 10f);

            float cost = (targetReduction / SecurityInvestmentEfficiency) * SecurityCostPerPercent / diminishingFactor;

            return cost;
        }

        #endregion

        #region Consultant Hiring

        /// <summary>
        /// Hires a consultant for temporary risk reduction
        /// Returns the reduction achieved and duration
        /// </summary>
        public static ConsultantResult HireConsultant(float cost, RiskCategory specialization)
        {
            if (cost <= 0) return new ConsultantResult { Reduction = 0f, Duration = 0f };

            // Calculate reduction based on cost
            float potentialReduction = cost / ConsultantCostPerPercent;
            float actualReduction = Mathf.Min(potentialReduction, ConsultantMaxReduction);

            return new ConsultantResult
            {
                Reduction = actualReduction,
                Duration = ConsultantDurationMinutes,
                Specialization = specialization,
                Cost = cost
            };
        }

        /// <summary>
        /// Calculates consultant cost for target reduction
        /// </summary>
        public static float CalculateConsultantCost(float targetReduction)
        {
            targetReduction = Mathf.Min(targetReduction, ConsultantMaxReduction);
            return targetReduction * ConsultantCostPerPercent;
        }

        #endregion

        #region Timeline Extension

        /// <summary>
        /// Extends timeline to reduce risk from time pressure
        /// Returns the risk reduction achieved
        /// </summary>
        public static float ExtendTimeline(float extraTimePercent, float currentTimelineModifier)
        {
            if (extraTimePercent <= 0) return 0f;

            // Calculate reduction based on extra time
            float potentialReduction = extraTimePercent * TimelineEfficiency;
            float actualReduction = Mathf.Min(potentialReduction, TimelineMaxReduction);

            // Only reduce if there's positive time pressure
            if (currentTimelineModifier <= 0)
            {
                actualReduction *= 0.5f; // Less effective if no time pressure
            }

            return actualReduction;
        }

        /// <summary>
        /// Calculates how much time extension is needed for target reduction
        /// </summary>
        public static float CalculateRequiredTimeExtension(float targetReduction)
        {
            targetReduction = Mathf.Min(targetReduction, TimelineMaxReduction);
            return targetReduction / TimelineEfficiency;
        }

        #endregion

        #region Budget Increase

        /// <summary>
        /// Increases budget to reduce financial risk
        /// Returns the risk reduction achieved
        /// </summary>
        public static float IncreaseBudget(float extraBudgetPercent, float currentBudgetModifier)
        {
            if (extraBudgetPercent <= 0) return 0f;

            // Calculate reduction based on extra budget
            float potentialReduction = extraBudgetPercent * BudgetEfficiency;
            float actualReduction = Mathf.Min(potentialReduction, BudgetMaxReduction);

            // Only reduce if there's budget pressure
            if (currentBudgetModifier <= 0)
            {
                actualReduction *= 0.5f; // Less effective if budget is adequate
            }

            return actualReduction;
        }

        /// <summary>
        /// Calculates required budget increase for target reduction
        /// </summary>
        public static float CalculateRequiredBudgetIncrease(float targetReduction)
        {
            targetReduction = Mathf.Min(targetReduction, BudgetMaxReduction);
            return targetReduction / BudgetEfficiency;
        }

        #endregion

        #region Quality Investment

        /// <summary>
        /// Invests in quality control to reduce technical risk
        /// Returns the reduction achieved
        /// </summary>
        public static float InvestInQuality(float amount, float currentTechnicalRisk)
        {
            if (amount <= 0 || currentTechnicalRisk <= 0) return 0f;

            // Calculate reduction based on investment
            float potentialReduction = amount / QualityCostPerPercent;
            float actualReduction = Mathf.Min(potentialReduction, QualityMaxReduction);

            // Cap at available risk
            actualReduction = Mathf.Min(actualReduction, currentTechnicalRisk);

            return actualReduction;
        }

        /// <summary>
        /// Calculates quality investment cost for target reduction
        /// </summary>
        public static float CalculateQualityCost(float targetReduction)
        {
            targetReduction = Mathf.Min(targetReduction, QualityMaxReduction);
            return targetReduction * QualityCostPerPercent;
        }

        #endregion

        #region Insurance

        /// <summary>
        /// Purchases insurance to cover potential losses
        /// Returns the insurance coverage details
        /// </summary>
        public static InsuranceResult PurchaseInsurance(float projectValue)
        {
            float cost = projectValue * InsuranceCostPercent;
            float coverage = projectValue * InsuranceCoveragePercent;

            return new InsuranceResult
            {
                Cost = cost,
                CoverageAmount = coverage,
                CoveragePercent = InsuranceCoveragePercent,
                Duration = -1f // Permanent for this project
            };
        }

        /// <summary>
        /// Calculates insurance payout for a failure
        /// </summary>
        public static float CalculateInsurancePayout(float lossAmount, float coverageAmount)
        {
            return Mathf.Min(lossAmount, coverageAmount);
        }

        #endregion

        #region Combined Mitigation

        /// <summary>
        /// Applies multiple mitigation strategies and returns combined result
        /// </summary>
        public static CombinedMitigationResult ApplyCombinedMitigation(
            RiskProfile baseProfile,
            MitigationOptions options)
        {
            var result = new CombinedMitigationResult();
            var modifiers = new RiskModifiers();

            // Apply security investment
            if (options.SecurityInvestment > 0)
            {
                float secReduction = InvestInSecurity(options.SecurityInvestment, baseProfile.securityRisk);
                modifiers.securityInvestment = secReduction;
                result.TotalCost += options.SecurityInvestment;
                result.SecurityReduction = secReduction;
            }

            // Apply consultant
            if (options.HireConsultant)
            {
                var consultant = HireConsultant(options.ConsultantCost, options.ConsultantSpecialization);
                modifiers.consultantReduction = consultant.Reduction;
                result.TotalCost += consultant.Cost;
                result.ConsultantReduction = consultant.Reduction;
                result.ConsultantDuration = consultant.Duration;
            }

            // Apply timeline extension
            if (options.TimelineExtensionPercent > 0)
            {
                float timeReduction = ExtendTimeline(options.TimelineExtensionPercent, options.CurrentTimelineModifier);
                modifiers.timelineModifier = -timeReduction; // Negative because it reduces the modifier
                result.TimelineReduction = timeReduction;
            }

            // Apply budget increase
            if (options.BudgetIncreasePercent > 0)
            {
                float budgetReduction = IncreaseBudget(options.BudgetIncreasePercent, options.CurrentBudgetModifier);
                modifiers.budgetModifier = -budgetReduction;
                result.BudgetReduction = budgetReduction;
            }

            // Apply quality investment
            if (options.QualityInvestment > 0)
            {
                float qualReduction = InvestInQuality(options.QualityInvestment, baseProfile.technicalRisk);
                modifiers.qualityInvestment = qualReduction;
                result.TotalCost += options.QualityInvestment;
                result.QualityReduction = qualReduction;
            }

            // Apply insurance
            if (options.PurchaseInsurance && options.ProjectValue > 0)
            {
                var insurance = PurchaseInsurance(options.ProjectValue);
                result.Insurance = insurance;
                result.TotalCost += insurance.Cost;
            }

            result.Modifiers = modifiers;
            result.FinalProfile = CalculateMitigatedProfile(baseProfile, modifiers);

            return result;
        }

        /// <summary>
        /// Calculates the mitigated risk profile
        /// </summary>
        private static RiskProfile CalculateMitigatedProfile(RiskProfile baseProfile, RiskModifiers modifiers)
        {
            var mitigated = baseProfile.Clone();

            // Apply reductions
            mitigated.securityRisk = Mathf.Max(0, mitigated.securityRisk - modifiers.securityInvestment);
            mitigated.technicalRisk = Mathf.Max(0, mitigated.technicalRisk - modifiers.qualityInvestment);

            // Apply consultant reduction to all categories
            float consultantReduction = modifiers.consultantReduction;
            mitigated.technicalRisk = Mathf.Max(0, mitigated.technicalRisk - consultantReduction);
            mitigated.financialRisk = Mathf.Max(0, mitigated.financialRisk - consultantReduction);
            mitigated.securityRisk = Mathf.Max(0, mitigated.securityRisk - consultantReduction);

            return mitigated;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the most effective mitigation strategy for a risk profile
        /// </summary>
        public static string GetRecommendedStrategy(RiskProfile profile)
        {
            float techRisk = profile.technicalRisk;
            float finRisk = profile.financialRisk;
            float secRisk = profile.securityRisk;

            if (secRisk >= techRisk && secRisk >= finRisk)
                return "Invest in Security";
            if (techRisk >= finRisk && techRisk >= secRisk)
                return "Invest in Quality Control";
            if (finRisk >= 60)
                return "Increase Budget Buffer";

            return "Hire Consultant";
        }

        /// <summary>
        /// Calculates cost-effectiveness of different mitigation options
        /// </summary>
        public static float CalculateCostEffectiveness(float cost, float riskReduction)
        {
            if (cost <= 0 || riskReduction <= 0) return 0f;
            return riskReduction / cost * 1000f; // Risk reduction per 1000 currency
        }

        #endregion
    }

    #region Result Classes

    /// <summary>
    /// Result from hiring a consultant
    /// </summary>
    public class ConsultantResult
    {
        public float Reduction;
        public float Duration;
        public RiskCategory Specialization;
        public float Cost;
    }

    /// <summary>
    /// Result from purchasing insurance
    /// </summary>
    public class InsuranceResult
    {
        public float Cost;
        public float CoverageAmount;
        public float CoveragePercent;
        public float Duration;
    }

    /// <summary>
    /// Options for combined mitigation
    /// </summary>
    public class MitigationOptions
    {
        public float SecurityInvestment = 0f;
        public bool HireConsultant = false;
        public float ConsultantCost = 0f;
        public RiskCategory ConsultantSpecialization = RiskCategory.Technical;
        public float TimelineExtensionPercent = 0f;
        public float CurrentTimelineModifier = 0f;
        public float BudgetIncreasePercent = 0f;
        public float CurrentBudgetModifier = 0f;
        public float QualityInvestment = 0f;
        public bool PurchaseInsurance = false;
        public float ProjectValue = 0f;
    }

    /// <summary>
    /// Combined result from multiple mitigation strategies
    /// </summary>
    public class CombinedMitigationResult
    {
        public float TotalCost;
        public float SecurityReduction;
        public float ConsultantReduction;
        public float ConsultantDuration;
        public float TimelineReduction;
        public float BudgetReduction;
        public float QualityReduction;
        public InsuranceResult Insurance;
        public RiskModifiers Modifiers;
        public RiskProfile FinalProfile;

        public float TotalRiskReduction => SecurityReduction + ConsultantReduction +
                                          TimelineReduction + BudgetReduction + QualityReduction;

        public override string ToString()
        {
            return $"Mitigation[Cost:{TotalCost:N0} Reduction:{TotalRiskReduction:F1}%]";
        }
    }

    #endregion
}
