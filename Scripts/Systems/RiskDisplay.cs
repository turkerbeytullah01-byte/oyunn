using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Static utility class for risk visualization and UI display
    /// </summary>
    public static class RiskDisplay
    {
        #region Constants

        private const int RiskBarSegments = 10;
        private const char FilledSegment = '■';
        private const char EmptySegment = '□';
        private const char WarningSegment = '▲';

        #endregion

        #region Risk Bar

        /// <summary>
        /// Gets a visual risk bar string (e.g., "■■■□□□□□□□")
        /// </summary>
        public static string GetRiskBar(float risk, bool useWarnings = true)
        {
            risk = Mathf.Clamp(risk, 0f, 100f);
            int filledSegments = Mathf.RoundToInt(risk / 100f * RiskBarSegments);

            var bar = new System.Text.StringBuilder();

            for (int i = 0; i < RiskBarSegments; i++)
            {
                if (i < filledSegments)
                {
                    // Use warning character for critical segments
                    if (useWarnings && i >= RiskBarSegments - 2 && risk > 70f)
                        bar.Append(WarningSegment);
                    else
                        bar.Append(FilledSegment);
                }
                else
                {
                    bar.Append(EmptySegment);
                }
            }

            return bar.ToString();
        }

        /// <summary>
        /// Gets a detailed risk bar with percentage
        /// </summary>
        public static string GetDetailedRiskBar(float risk, bool useWarnings = true)
        {
            return $"{GetRiskBar(risk, useWarnings)} {risk:F0}%";
        }

        /// <summary>
        /// Gets a multi-category risk bar for a risk profile
        /// </summary>
        public static string GetMultiCategoryRiskBar(RiskProfile profile)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Tech:  {GetDetailedRiskBar(profile.technicalRisk)}");
            sb.AppendLine($"Fin:   {GetDetailedRiskBar(profile.financialRisk)}");
            sb.AppendLine($"Sec:   {GetDetailedRiskBar(profile.securityRisk)}");
            sb.AppendLine($"Avg:   {GetDetailedRiskBar(profile.GetOverallRisk())}");
            return sb.ToString();
        }

        #endregion

        #region Risk Colors

        /// <summary>
        /// Gets the color associated with a risk level
        /// </summary>
        public static Color GetRiskColor(float risk)
        {
            risk = Mathf.Clamp(risk, 0f, 100f);

            if (risk < 10f) return new Color(0.2f, 0.8f, 0.2f);      // Green - Very Low
            if (risk < 25f) return new Color(0.5f, 0.9f, 0.3f);      // Light Green - Low
            if (risk < 50f) return new Color(0.9f, 0.9f, 0.2f);      // Yellow - Medium
            if (risk < 75f) return new Color(0.95f, 0.6f, 0.1f);     // Orange - High
            if (risk < 90f) return new Color(0.95f, 0.3f, 0.1f);     // Red-Orange - Very High
            return new Color(0.9f, 0.1f, 0.1f);                       // Red - Critical
        }

        /// <summary>
        /// Gets the color associated with a risk level enum
        /// </summary>
        public static Color GetRiskColor(RiskLevel level)
        {
            return GetRiskColor((float)level);
        }

        /// <summary>
        /// Gets a darker version of the risk color (for backgrounds)
        /// </summary>
        public static Color GetRiskColorDark(float risk)
        {
            Color baseColor = GetRiskColor(risk);
            return new Color(baseColor.r * 0.5f, baseColor.g * 0.5f, baseColor.b * 0.5f);
        }

        /// <summary>
        /// Gets color as hex string for UI
        /// </summary>
        public static string GetRiskColorHex(float risk)
        {
            Color color = GetRiskColor(risk);
            return $"#{Mathf.RoundToInt(color.r * 255):X2}{Mathf.RoundToInt(color.g * 255):X2}{Mathf.RoundToInt(color.b * 255):X2}";
        }

        /// <summary>
        /// Gets a gradient between two risk colors
        /// </summary>
        public static Color GetGradientColor(float risk, float minRisk, float maxRisk)
        {
            float t = Mathf.InverseLerp(minRisk, maxRisk, risk);
            Color lowColor = GetRiskColor(minRisk);
            Color highColor = GetRiskColor(maxRisk);
            return Color.Lerp(lowColor, highColor, t);
        }

        #endregion

        #region Risk Descriptions

        /// <summary>
        /// Gets a description for a risk level
        /// </summary>
        public static string GetRiskDescription(RiskLevel level)
        {
            return level switch
            {
                RiskLevel.None => "No significant risk detected. Project should proceed smoothly.",
                RiskLevel.VeryLow => "Minimal risk. Minor complications possible but unlikely.",
                RiskLevel.Low => "Low risk. Some minor issues may arise but easily managed.",
                RiskLevel.Medium => "Moderate risk. Expect some challenges and plan contingencies.",
                RiskLevel.High => "High risk. Significant challenges likely. Mitigation recommended.",
                RiskLevel.VeryHigh => "Very high risk. Major problems probable. Strong mitigation required.",
                RiskLevel.Critical => "CRITICAL RISK! Severe consequences likely. Reconsider approach.",
                _ => "Unknown risk level."
            };
        }

        /// <summary>
        /// Gets a description for a risk value
        /// </summary>
        public static string GetRiskDescription(float risk)
        {
            return GetRiskDescription(RiskProfile.ValueToRiskLevel(risk));
        }

        /// <summary>
        /// Gets a short label for a risk level
        /// </summary>
        public static string GetRiskLabel(RiskLevel level)
        {
            return RiskProfile.RiskLevelToString(level);
        }

        /// <summary>
        /// Gets a short label for a risk value
        /// </summary>
        public static string GetRiskLabel(float risk)
        {
            return GetRiskLabel(RiskProfile.ValueToRiskLevel(risk));
        }

        #endregion

        #region Risk Advice

        /// <summary>
        /// Gets advice for a risk profile
        /// </summary>
        public static string GetRiskAdvice(RiskProfile profile)
        {
            var sb = new System.Text.StringBuilder();
            float overallRisk = profile.GetOverallRisk();

            // Overall assessment
            sb.AppendLine($"=== Risk Assessment: {GetRiskLabel(overallRisk)} ===");
            sb.AppendLine();

            // Category-specific advice
            if (profile.technicalRisk > 50f)
            {
                sb.AppendLine("Technical Risk is HIGH:");
                sb.AppendLine("  • Invest in quality control");
                sb.AppendLine("  • Consider hiring technical consultants");
                sb.AppendLine("  • Break project into smaller milestones");
                sb.AppendLine();
            }

            if (profile.financialRisk > 50f)
            {
                sb.AppendLine("Financial Risk is HIGH:");
                sb.AppendLine("  • Secure additional funding or credit line");
                sb.AppendLine("  • Consider project insurance");
                sb.AppendLine("  • Build in budget buffer (15-20%)");
                sb.AppendLine();
            }

            if (profile.securityRisk > 50f)
            {
                sb.AppendLine("Security Risk is HIGH:");
                sb.AppendLine("  • Invest in security infrastructure");
                sb.AppendLine("  • Conduct security audit");
                sb.AppendLine("  • Implement data protection measures");
                sb.AppendLine();
            }

            // General advice based on overall risk
            if (overallRisk < 25f)
            {
                sb.AppendLine("Overall risk is LOW. Standard precautions should suffice.");
            }
            else if (overallRisk < 50f)
            {
                sb.AppendLine("Overall risk is MODERATE. Monitor closely and have backup plans.");
            }
            else if (overallRisk < 75f)
            {
                sb.AppendLine("Overall risk is HIGH. Strongly recommend mitigation strategies.");
            }
            else
            {
                sb.AppendLine("WARNING: Overall risk is VERY HIGH!");
                sb.AppendLine("Consider delaying project or significantly increasing investments.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets brief advice for a risk profile
        /// </summary>
        public static string GetBriefRiskAdvice(RiskProfile profile)
        {
            float overallRisk = profile.GetOverallRisk();

            if (overallRisk < 25f)
                return "Low risk - proceed with standard precautions";
            if (overallRisk < 50f)
                return "Moderate risk - monitor and plan contingencies";
            if (overallRisk < 75f)
                return "High risk - mitigation strongly recommended";

            return "CRITICAL RISK - reconsider approach or invest heavily";
        }

        #endregion

        #region Failure Type Display

        /// <summary>
        /// Gets display info for a failure type
        /// </summary>
        public static FailureTypeDisplay GetFailureTypeDisplay(FailureType type)
        {
            return type switch
            {
                FailureType.MinorSetback => new FailureTypeDisplay
                {
                    Title = "Minor Setback",
                    Icon = "⚠",
                    Color = new Color(0.9f, 0.9f, 0.3f),
                    Description = "Small delay with minimal impact"
                },
                FailureType.Delay => new FailureTypeDisplay
                {
                    Title = "Delay",
                    Icon = "⏱",
                    Color = new Color(0.95f, 0.7f, 0.2f),
                    Description = "Significant time delay"
                },
                FailureType.CostOverrun => new FailureTypeDisplay
                {
                    Title = "Cost Overrun",
                    Icon = "💰",
                    Color = new Color(0.95f, 0.5f, 0.2f),
                    Description = "Additional funds required"
                },
                FailureType.PartialFailure => new FailureTypeDisplay
                {
                    Title = "Partial Failure",
                    Icon = "⚡",
                    Color = new Color(0.95f, 0.4f, 0.2f),
                    Description = "Some progress lost"
                },
                FailureType.MajorFailure => new FailureTypeDisplay
                {
                    Title = "MAJOR FAILURE",
                    Icon = "🔥",
                    Color = new Color(0.95f, 0.2f, 0.2f),
                    Description = "Significant penalties apply"
                },
                FailureType.Catastrophic => new FailureTypeDisplay
                {
                    Title = "CATASTROPHIC FAILURE",
                    Icon = "☠",
                    Color = new Color(0.7f, 0.1f, 0.1f),
                    Description = "Severe consequences!"
                },
                _ => new FailureTypeDisplay
                {
                    Title = "Unknown",
                    Icon = "?",
                    Color = Color.gray,
                    Description = "Unknown failure type"
                }
            };
        }

        #endregion

        #region Risk Comparison

        /// <summary>
        /// Compares two risk profiles and returns a summary
        /// </summary>
        public static string CompareRiskProfiles(RiskProfile profile1, RiskProfile profile2, string label1 = "Current", string label2 = "Modified")
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Risk Comparison ===");
            sb.AppendLine();

            float risk1 = profile1.GetOverallRisk();
            float risk2 = profile2.GetOverallRisk();
            float diff = risk2 - risk1;

            sb.AppendLine($"{label1}: {GetDetailedRiskBar(risk1)}");
            sb.AppendLine($"{label2}: {GetDetailedRiskBar(risk2)}");
            sb.AppendLine();

            if (Mathf.Abs(diff) < 0.1f)
            {
                sb.AppendLine("No significant change in risk level.");
            }
            else if (diff < 0)
            {
                sb.AppendLine($"Risk DECREASED by {Mathf.Abs(diff):F1}% ✓");
            }
            else
            {
                sb.AppendLine($"Risk INCREASED by {diff:F1}% ⚠");
            }

            return sb.ToString();
        }

        #endregion

        #region Formatting Utilities

        /// <summary>
        /// Formats a risk value with color tags for rich text
        /// </summary>
        public static string FormatRiskWithColor(float risk)
        {
            string hexColor = GetRiskColorHex(risk);
            return $"<color={hexColor}>{risk:F0}%</color>";
        }

        /// <summary>
        /// Formats a risk profile for display
        /// </summary>
        public static string FormatRiskProfile(RiskProfile profile, string title = "Risk Profile")
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== {title} ===");
            sb.AppendLine($"Technical:  {GetDetailedRiskBar(profile.technicalRisk)}");
            sb.AppendLine($"Financial:  {GetDetailedRiskBar(profile.financialRisk)}");
            sb.AppendLine($"Security:   {GetDetailedRiskBar(profile.securityRisk)}");
            sb.AppendLine($"Overall:    {GetDetailedRiskBar(profile.GetOverallRisk())}");

            if (!string.IsNullOrEmpty(profile.description))
            {
                sb.AppendLine();
                sb.AppendLine(profile.description);
            }

            return sb.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Display information for a failure type
    /// </summary>
    public struct FailureTypeDisplay
    {
        public string Title;
        public string Icon;
        public Color Color;
        public string Description;
    }
}
