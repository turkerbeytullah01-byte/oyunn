using System;
using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Contains all factors that modify risk calculations
    /// </summary>
    [Serializable]
    public class RiskModifiers
    {
        [Header("Technology & Expertise")]
        [Range(0, 50)]
        [Tooltip("Reduction from technology level (0-50%)")]
        public float techLevelReduction = 0f;

        [Range(0, 30)]
        [Tooltip("Reduction from relevant expertise/specialization (0-30%)")]
        public float expertiseReduction = 0f;

        [Header("Reputation & Track Record")]
        [Range(0, 25)]
        [Tooltip("Reduction from company reputation (0-25%)")]
        public float reputationReduction = 0f;

        [Range(0, 15)]
        [Tooltip("Bonus from consecutive successful projects (0-15%)")]
        public float previousSuccessBonus = 0f;

        [Range(0, 10)]
        [Tooltip("Penalty from recent failures (0-10%)")]
        public float recentFailurePenalty = 0f;

        [Header("Investments")]
        [Range(0, 40)]
        [Tooltip("Reduction from security investments (0-40%)")]
        public float securityInvestment = 0f;

        [Range(0, 20)]
        [Tooltip("Reduction from quality control investments (0-20%)")]
        public float qualityInvestment = 0f;

        [Header("Timeline & Budget")]
        [Range(-20, 20)]
        [Tooltip("Modifier from timeline pressure (-20% for relaxed, +20% for rushed)")]
        public float timelineModifier = 0f;

        [Range(-10, 10)]
        [Tooltip("Modifier from budget adequacy (-10% for generous, +10% for tight)")]
        public float budgetModifier = 0f;

        [Header("External Factors")]
        [Range(-15, 15)]
        [Tooltip("Temporary event modifiers (-15% to +15%)")]
        public float eventModifier = 0f;

        [Range(0, 20)]
        [Tooltip("Market volatility impact (0-20%)")]
        public float marketVolatility = 0f;

        [Header("Personnel")]
        [Range(0, 15)]
        [Tooltip("Reduction from consultant/advisor help (0-15%)")]
        public float consultantReduction = 0f;

        [Range(0, 10)]
        [Tooltip("Penalty from understaffing (0-10%)")]
        public float understaffingPenalty = 0f;

        /// <summary>
        /// Calculates the total reduction from all positive modifiers
        /// </summary>
        public float GetTotalReduction()
        {
            return techLevelReduction +
                   expertiseReduction +
                   reputationReduction +
                   previousSuccessBonus +
                   securityInvestment +
                   qualityInvestment +
                   consultantReduction;
        }

        /// <summary>
        /// Calculates the total increase from all negative modifiers
        /// </summary>
        public float GetTotalIncrease()
        {
            return recentFailurePenalty +
                   Mathf.Max(0, timelineModifier) +
                   Mathf.Max(0, budgetModifier) +
                   Mathf.Max(0, eventModifier) +
                   marketVolatility +
                   understaffingPenalty;
        }

        /// <summary>
        /// Calculates the net modifier (positive = reduction, negative = increase)
        /// </summary>
        public float GetNetModifier()
        {
            return GetTotalReduction() - GetTotalIncrease();
        }

        /// <summary>
        /// Applies these modifiers to a base failure chance
        /// </summary>
        public float ApplyTo(float baseChance)
        {
            float modified = baseChance - GetNetModifier();
            return Mathf.Clamp(modified, RiskCalculator.MinFailureChance, RiskCalculator.MaxFailureChance);
        }

        /// <summary>
        /// Creates a default modifier with no effects
        /// </summary>
        public static RiskModifiers Default => new RiskModifiers();

        /// <summary>
        /// Creates modifiers for research projects
        /// </summary>
        public static RiskModifiers ForResearch(float techLevel, float reputation, float securityInvestment)
        {
            return new RiskModifiers
            {
                techLevelReduction = Mathf.Min(techLevel * 0.5f, 50f),
                reputationReduction = Mathf.Min(reputation * 0.25f, 25f),
                securityInvestment = Mathf.Min(securityInvestment, 40f)
            };
        }

        /// <summary>
        /// Creates modifiers for contract work
        /// </summary>
        public static RiskModifiers ForContract(float reputation, float timelinePressure, float budgetAdequacy)
        {
            return new RiskModifiers
            {
                reputationReduction = Mathf.Min(reputation * 0.25f, 25f),
                timelineModifier = Mathf.Clamp(timelinePressure, -20f, 20f),
                budgetModifier = Mathf.Clamp(budgetAdequacy, -10f, 10f)
            };
        }

        /// <summary>
        /// Creates a copy of these modifiers
        /// </summary>
        public RiskModifiers Clone()
        {
            return new RiskModifiers
            {
                techLevelReduction = this.techLevelReduction,
                expertiseReduction = this.expertiseReduction,
                reputationReduction = this.reputationReduction,
                previousSuccessBonus = this.previousSuccessBonus,
                recentFailurePenalty = this.recentFailurePenalty,
                securityInvestment = this.securityInvestment,
                qualityInvestment = this.qualityInvestment,
                timelineModifier = this.timelineModifier,
                budgetModifier = this.budgetModifier,
                eventModifier = this.eventModifier,
                marketVolatility = this.marketVolatility,
                consultantReduction = this.consultantReduction,
                understaffingPenalty = this.understaffingPenalty
            };
        }

        public override string ToString()
        {
            return $"Modifiers[Net:{GetNetModifier():F1}% Red:{GetTotalReduction():F1}% Inc:{GetTotalIncrease():F1}%]";
        }
    }
}
