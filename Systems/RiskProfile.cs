using System;
using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Risk level enumeration with predefined values
    /// </summary>
    public enum RiskLevel
    {
        None = 0,
        VeryLow = 10,
        Low = 25,
        Medium = 50,
        High = 75,
        VeryHigh = 90,
        Critical = 100
    }

    /// <summary>
    /// Risk category for categorizing different types of risks
    /// </summary>
    public enum RiskCategory
    {
        Technical,
        Financial,
        Security,
        Operational,
        Market
    }

    /// <summary>
    /// Serializable risk profile for research, contracts, and operations
    /// </summary>
    [Serializable]
    public class RiskProfile
    {
        [Header("Base Risk Levels")]
        [Range(0, 100)]
        [Tooltip("Technical complexity risk - higher for advanced technologies")]
        public float technicalRisk = 0f;

        [Range(0, 100)]
        [Tooltip("Financial stability risk - higher for expensive projects")]
        public float financialRisk = 0f;

        [Range(0, 100)]
        [Tooltip("Security vulnerability risk - higher for sensitive data/projects")]
        public float securityRisk = 0f;

        [Header("Context")]
        [Tooltip("Optional description of this risk profile")]
        public string description = "";

        [Tooltip("Source of this risk (Research, Contract, etc.)")]
        public string sourceContext = "";

        /// <summary>
        /// Gets the overall risk as an average of all risk categories
        /// </summary>
        public float GetOverallRisk()
        {
            return (technicalRisk + financialRisk + securityRisk) / 3f;
        }

        /// <summary>
        /// Gets the maximum risk from any category
        /// </summary>
        public float GetMaxRisk()
        {
            return Mathf.Max(technicalRisk, financialRisk, securityRisk);
        }

        /// <summary>
        /// Gets the weighted risk with customizable weights
        /// </summary>
        public float GetWeightedRisk(float techWeight = 1f, float financialWeight = 1f, float securityWeight = 1f)
        {
            float totalWeight = techWeight + financialWeight + securityWeight;
            if (totalWeight <= 0) return GetOverallRisk();

            return (technicalRisk * techWeight + financialRisk * financialWeight + securityRisk * securityWeight) / totalWeight;
        }

        /// <summary>
        /// Gets the base failure chance before modifiers
        /// </summary>
        public float GetBaseFailureChance()
        {
            return GetOverallRisk();
        }

        /// <summary>
        /// Gets the failure chance after applying a risk modifier
        /// </summary>
        public float GetFailureChance(RiskModifiers modifiers = null)
        {
            if (modifiers == null)
                return GetBaseFailureChance();

            return RiskCalculator.CalculateFailureChance(this, modifiers);
        }

        /// <summary>
        /// Gets the highest risk category
        /// </summary>
        public RiskCategory GetHighestRiskCategory()
        {
            if (technicalRisk >= financialRisk && technicalRisk >= securityRisk)
                return RiskCategory.Technical;
            if (financialRisk >= technicalRisk && financialRisk >= securityRisk)
                return RiskCategory.Financial;
            return RiskCategory.Security;
        }

        /// <summary>
        /// Gets the risk level for a specific category
        /// </summary>
        public RiskLevel GetRiskLevel(RiskCategory category)
        {
            float value = category switch
            {
                RiskCategory.Technical => technicalRisk,
                RiskCategory.Financial => financialRisk,
                RiskCategory.Security => securityRisk,
                _ => GetOverallRisk()
            };

            return ValueToRiskLevel(value);
        }

        /// <summary>
        /// Sets risk for a specific category
        /// </summary>
        public void SetRisk(RiskCategory category, float value)
        {
            value = Mathf.Clamp(value, 0f, 100f);

            switch (category)
            {
                case RiskCategory.Technical:
                    technicalRisk = value;
                    break;
                case RiskCategory.Financial:
                    financialRisk = value;
                    break;
                case RiskCategory.Security:
                    securityRisk = value;
                    break;
            }
        }

        /// <summary>
        /// Sets risk for a specific category using RiskLevel enum
        /// </summary>
        public void SetRisk(RiskCategory category, RiskLevel level)
        {
            SetRisk(category, (float)level);
        }

        /// <summary>
        /// Clones this risk profile
        /// </summary>
        public RiskProfile Clone()
        {
            return new RiskProfile
            {
                technicalRisk = this.technicalRisk,
                financialRisk = this.financialRisk,
                securityRisk = this.securityRisk,
                description = this.description,
                sourceContext = this.sourceContext
            };
        }

        /// <summary>
        /// Creates a risk profile from RiskLevel values
        /// </summary>
        public static RiskProfile FromRiskLevels(RiskLevel technical, RiskLevel financial, RiskLevel security)
        {
            return new RiskProfile
            {
                technicalRisk = (float)technical,
                financialRisk = (float)financial,
                securityRisk = (float)security
            };
        }

        /// <summary>
        /// Creates a risk profile from float values
        /// </summary>
        public static RiskProfile FromValues(float technical, float financial, float security)
        {
            return new RiskProfile
            {
                technicalRisk = Mathf.Clamp(technical, 0f, 100f),
                financialRisk = Mathf.Clamp(financial, 0f, 100f),
                securityRisk = Mathf.Clamp(security, 0f, 100f)
            };
        }

        /// <summary>
        /// Converts a float value to RiskLevel enum
        /// </summary>
        public static RiskLevel ValueToRiskLevel(float value)
        {
            value = Mathf.Clamp(value, 0f, 100f);

            if (value >= 95f) return RiskLevel.Critical;
            if (value >= 82f) return RiskLevel.VeryHigh;
            if (value >= 62f) return RiskLevel.High;
            if (value >= 37f) return RiskLevel.Medium;
            if (value >= 17f) return RiskLevel.Low;
            if (value >= 5f) return RiskLevel.VeryLow;
            return RiskLevel.None;
        }

        /// <summary>
        /// Converts RiskLevel to a display string
        /// </summary>
        public static string RiskLevelToString(RiskLevel level)
        {
            return level switch
            {
                RiskLevel.None => "None",
                RiskLevel.VeryLow => "Very Low",
                RiskLevel.Low => "Low",
                RiskLevel.Medium => "Medium",
                RiskLevel.High => "High",
                RiskLevel.VeryHigh => "Very High",
                RiskLevel.Critical => "CRITICAL",
                _ => "Unknown"
            };
        }

        public override string ToString()
        {
            return $"Risk[Tech:{technicalRisk:F1}% Fin:{financialRisk:F1}% Sec:{securityRisk:F1}% Overall:{GetOverallRisk():F1}%]";
        }
    }
}
