using System;
using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Integrates the Risk System with Research projects
    /// Handles risk calculation, rolling, and consequence application for R&D
    /// </summary>
    public class ResearchRiskIntegration : MonoBehaviour
    {
        public static ResearchRiskIntegration Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableResearchRisk = true;
        [SerializeField] private float baseTechRiskMultiplier = 1.0f;
        [SerializeField] private float baseFinancialRiskMultiplier = 1.0f;
        [SerializeField] private float baseSecurityRiskMultiplier = 0.8f;

        [Header("Events")]
        public Action<string, RiskRollResult> OnResearchRiskRoll;
        public Action<string, FailureResult> OnResearchFailure;
        public Action<string, float> OnResearchSuccess;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Calculates the risk profile for a research project
        /// </summary>
        public RiskProfile CalculateResearchRisk(
            ResearchProjectData project,
            float companyTechLevel,
            float companyReputation,
            float securityInvestment)
        {
            if (!enableResearchRisk)
                return RiskProfile.FromValues(0, 0, 0);

            // Base risk from project difficulty
            float techRisk = project.Difficulty * baseTechRiskMultiplier;
            float finRisk = project.CostRisk * baseFinancialRiskMultiplier;
            float secRisk = project.SecurityRequirement * baseSecurityRiskMultiplier;

            // Apply complexity modifier
            float complexityMod = 1f + (project.Complexity - 1) * 0.2f;
            techRisk *= complexityMod;

            // Apply innovation modifier (more innovative = more risky)
            float innovationMod = 1f + project.InnovationLevel * 0.15f;
            techRisk *= innovationMod;
            secRisk *= innovationMod;

            var profile = RiskProfile.FromValues(
                Mathf.Clamp(techRisk, 0, 100),
                Mathf.Clamp(finRisk, 0, 100),
                Mathf.Clamp(secRisk, 0, 100)
            );

            profile.description = $"Research: {project.Name}";
            profile.sourceContext = "Research";

            return profile;
        }

        /// <summary>
        /// Creates risk modifiers for a research project
        /// </summary>
        public RiskModifiers CreateResearchModifiers(
            float companyTechLevel,
            float companyReputation,
            float securityInvestment,
            float previousSuccessBonus = 0f)
        {
            return new RiskModifiers
            {
                techLevelReduction = Mathf.Min(companyTechLevel * 0.5f, 50f),
                reputationReduction = Mathf.Min(companyReputation * 0.25f, 25f),
                securityInvestment = Mathf.Min(securityInvestment, 40f),
                previousSuccessBonus = previousSuccessBonus
            };
        }

        /// <summary>
        /// Performs a risk roll for research completion
        /// Returns the roll result
        /// </summary>
        public RiskRollResult RollForResearchCompletion(
            string researchId,
            RiskProfile riskProfile,
            RiskModifiers modifiers)
        {
            if (!enableResearchRisk)
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
            float failureChance = RiskManager.Instance?.CalculateResearchFailureChance(riskProfile, modifiers)
                ?? RiskCalculator.CalculateResearchFailureChance(riskProfile, modifiers);

            // Roll for success
            RiskRollResult result = RiskCalculator.RollDetailed(failureChance);

            // Log and notify
            Debug.Log($"[ResearchRisk] '{researchId}': {result}");
            OnResearchRiskRoll?.Invoke(researchId, result);

            // Apply outcomes
            if (result.Success)
            {
                HandleResearchSuccess(researchId, result);
            }
            else
            {
                HandleResearchFailure(researchId, riskProfile, result);
            }

            return result;
        }

        /// <summary>
        /// Handles successful research completion
        /// </summary>
        private void HandleResearchSuccess(string researchId, RiskRollResult result)
        {
            // Calculate reputation gain based on margin
            float baseRepGain = 2f;
            float marginBonus = result.Margin > 10f ? 1f : 0f;
            float totalRepGain = baseRepGain + marginBonus;

            OnResearchSuccess?.Invoke(researchId, totalRepGain);

            // Notify risk manager
            RiskManager.Instance?.ApplySuccessBonuses("research", researchId);

            Debug.Log($"[ResearchRisk] '{researchId}' completed successfully! Reputation +{totalRepGain:F1}");
        }

        /// <summary>
        /// Handles research failure
        /// </summary>
        private void HandleResearchFailure(string researchId, RiskProfile riskProfile, RiskRollResult result)
        {
            // Determine failure type
            FailureType failureType = DetermineResearchFailureType(result, riskProfile);

            // Get consequences
            FailureResult failure = FailureConsequences.GetResearchConsequences(
                failureType,
                RiskProfile.ValueToRiskLevel(riskProfile.GetOverallRisk()),
                1000f, // Base research cost
                60f    // Base research time
            );

            failure.failureId = researchId;
            failure.context = "research";

            // Apply consequences
            OnResearchFailure?.Invoke(researchId, failure);
            RiskManager.Instance?.ApplyFailureConsequences(failure);

            Debug.Log($"[ResearchRisk] '{researchId}' failed! {failure.failureType}");
        }

        /// <summary>
        /// Determines the type of failure for research
        /// </summary>
        private FailureType DetermineResearchFailureType(RiskRollResult result, RiskProfile riskProfile)
        {
            float failureMargin = result.FailureChance - result.Roll;
            float riskLevel = riskProfile.GetOverallRisk();

            if (failureMargin > 25f && riskLevel > 80f)
                return FailureType.Catastrophic;
            if (failureMargin > 15f || riskLevel > 70f)
                return FailureType.MajorFailure;
            if (failureMargin > 8f || riskLevel > 55f)
                return FailureType.PartialFailure;
            if (failureMargin > 4f || riskLevel > 40f)
                return FailureType.CostOverrun;
            if (failureMargin > 1f || riskLevel > 25f)
                return FailureType.Delay;

            return FailureType.MinorSetback;
        }

        /// <summary>
        /// Gets the recommended mitigation for a research project
        /// </summary>
        public string[] GetRecommendedMitigation(ResearchProjectData project)
        {
            var recommendations = new System.Collections.Generic.List<string>();

            if (project.Difficulty > 60)
                recommendations.Add("Invest in R&D infrastructure");

            if (project.SecurityRequirement > 50)
                recommendations.Add("Increase security investments");

            if (project.CostRisk > 60)
                recommendations.Add("Secure additional funding");

            if (project.Complexity > 3)
                recommendations.Add("Hire technical consultants");

            if (recommendations.Count == 0)
                recommendations.Add("Risk level is acceptable");

            return recommendations.ToArray();
        }

        /// <summary>
        /// Gets a display summary for research risk
        /// </summary>
        public string GetResearchRiskSummary(ResearchProjectData project, RiskProfile profile, float failureChance)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Research Risk: {project.Name} ===");
            sb.AppendLine();
            sb.AppendLine(RiskDisplay.FormatRiskProfile(profile));
            sb.AppendLine();
            sb.AppendLine($"Failure Chance: {failureChance:F1}%");
            sb.AppendLine($"Success Chance: {100f - failureChance:F1}%");
            sb.AppendLine();
            sb.AppendLine("Recommendations:");
            foreach (var rec in GetRecommendedMitigation(project))
            {
                sb.AppendLine($"  • {rec}");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Data class for research project information
    /// </summary>
    [Serializable]
    public class ResearchProjectData
    {
        public string Id;
        public string Name;
        public string Description;

        [Range(0, 100)]
        public float Difficulty; // 0-100

        [Range(0, 100)]
        public float CostRisk; // 0-100

        [Range(0, 100)]
        public float SecurityRequirement; // 0-100

        [Range(1, 5)]
        public int Complexity; // 1-5

        [Range(0, 1)]
        public float InnovationLevel; // 0-1

        public float BaseCost;
        public float BaseTimeMinutes;
    }
}
