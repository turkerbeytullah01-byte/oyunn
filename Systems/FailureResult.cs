using System;
using UnityEngine;

namespace ProjectAegis.Systems.Risk
{
    /// <summary>
    /// Types of failure outcomes, from minor to catastrophic
    /// </summary>
    public enum FailureType
    {
        MinorSetback,      // Small delay, minimal impact
        Delay,            // Significant time delay
        CostOverrun,      // Extra money needed to complete
        PartialFailure,   // Some progress lost, need to redo part
        MajorFailure,     // Significant penalties, reputation damage
        Catastrophic      // Rare, severe penalties, major setbacks
    }

    /// <summary>
    /// Represents the result of a failed risk roll
    /// Contains all consequences that should be applied
    /// </summary>
    [Serializable]
    public class FailureResult
    {
        [Header("Failure Information")]
        public FailureType failureType;
        public string failureId;
        public string context; // "research", "contract", "operation"
        public DateTime failureTime;

        [Header("Financial Consequences")]
        [Tooltip("Amount of money lost")]
        public float moneyLost;

        [Tooltip("Additional costs to recover")]
        public float recoveryCost;

        [Tooltip("Percentage of investment lost (0-1)")]
        public float investmentLossPercent;

        [Header("Reputation Consequences")]
        [Tooltip("Reputation points lost")]
        public float reputationLost;

        [Tooltip("Client relationship damage (0-1)")]
        public float clientRelationshipDamage;

        [Header("Time Consequences")]
        [Tooltip("Time delay in minutes")]
        public float timeDelayMinutes;

        [Tooltip("Progress lost (0-1)")]
        public float progressLost;

        [Header("Status")]
        [Tooltip("Whether the failure is recoverable")]
        public bool isRecoverable;

        [Tooltip("Cost to recover from this failure")]
        public float recoveryPrice;

        [Header("Description")]
        [TextArea(3, 5)]
        public string description;

        [TextArea(2, 3)]
        public string recoveryDescription;

        /// <summary>
        /// Total financial impact (money lost + recovery cost)
        /// </summary>
        public float TotalFinancialImpact => moneyLost + recoveryCost;

        /// <summary>
        /// Time delay formatted as readable string
        /// </summary>
        public string TimeDelayFormatted
        {
            get
            {
                if (timeDelayMinutes < 60)
                    return $"{timeDelayMinutes:F0} min";
                if (timeDelayMinutes < 1440)
                    return $"{timeDelayMinutes / 60:F1} hours";
                return $"{timeDelayMinutes / 1440:F1} days";
            }
        }

        /// <summary>
        /// Creates a new failure result
        /// </summary>
        public FailureResult(FailureType type, RiskLevel riskLevel)
        {
            failureType = type;
            failureTime = DateTime.Now;
            isRecoverable = type != FailureType.Catastrophic;
            investmentLossPercent = 0f;
            progressLost = 0f;

            // Generate description based on type
            description = GenerateDescription(type, riskLevel);
            recoveryDescription = GenerateRecoveryDescription(type);
        }

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public FailureResult()
        {
            failureTime = DateTime.Now;
            isRecoverable = true;
        }

        /// <summary>
        /// Generates a description based on failure type and risk level
        /// </summary>
        private string GenerateDescription(FailureType type, RiskLevel riskLevel)
        {
            string riskStr = RiskProfile.RiskLevelToString(riskLevel).ToLower();

            return type switch
            {
                FailureType.MinorSetback => $"A minor setback occurred due to {riskStr} risk factors. " +
                    "The project encountered unexpected complications but remains on track.",

                FailureType.Delay => $"Project delayed due to {riskStr} risk complications. " +
                    "Additional time is needed to address the issues.",

                FailureType.CostOverrun => $"Budget exceeded due to {riskStr} risk factors. " +
                    "Additional funding is required to complete the project.",

                FailureType.PartialFailure => $"Partial failure occurred with {riskStr} risk exposure. " +
                    "Some progress has been lost and must be redone.",

                FailureType.MajorFailure => $"MAJOR FAILURE: {riskStr.ToUpper()} risk factors caused significant problems. " +
                    "Substantial penalties apply and reputation has been damaged.",

                FailureType.Catastrophic => $"CATASTROPHIC FAILURE! Maximum risk exposure resulted in disaster. " +
                    "Severe penalties, major setbacks, and significant reputation damage.",

                _ => "Unknown failure occurred."
            };
        }

        /// <summary>
        /// Generates recovery description
        /// </summary>
        private string GenerateRecoveryDescription(FailureType type)
        {
            return type switch
            {
                FailureType.MinorSetback => "Wait for the minor delay to resolve automatically.",
                FailureType.Delay => "Wait for the delay to pass or pay to expedite recovery.",
                FailureType.CostOverrun => "Invest additional funds to continue the project.",
                FailureType.PartialFailure => "Redo lost progress or pay for accelerated recovery.",
                FailureType.MajorFailure => "Significant recovery effort required. Multiple options available.",
                FailureType.Catastrophic => "Catastrophic damage. Extensive recovery required.",
                _ => "Recovery options available."
            };
        }

        /// <summary>
        /// Creates a copy of this failure result
        /// </summary>
        public FailureResult Clone()
        {
            return new FailureResult
            {
                failureType = this.failureType,
                failureId = this.failureId,
                context = this.context,
                failureTime = this.failureTime,
                moneyLost = this.moneyLost,
                recoveryCost = this.recoveryCost,
                investmentLossPercent = this.investmentLossPercent,
                reputationLost = this.reputationLost,
                clientRelationshipDamage = this.clientRelationshipDamage,
                timeDelayMinutes = this.timeDelayMinutes,
                progressLost = this.progressLost,
                isRecoverable = this.isRecoverable,
                recoveryPrice = this.recoveryPrice,
                description = this.description,
                recoveryDescription = this.recoveryDescription
            };
        }

        /// <summary>
        /// Scales the failure result by a multiplier
        /// </summary>
        public void Scale(float multiplier)
        {
            moneyLost *= multiplier;
            recoveryCost *= multiplier;
            reputationLost *= multiplier;
            timeDelayMinutes *= multiplier;
            recoveryPrice *= multiplier;
        }

        /// <summary>
        /// Gets a formatted summary of the failure
        /// </summary>
        public string GetSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== {failureType} ===");
            sb.AppendLine(description);
            sb.AppendLine();

            if (moneyLost > 0)
                sb.AppendLine($"Money Lost: ${moneyLost:N0}");
            if (recoveryCost > 0)
                sb.AppendLine($"Recovery Cost: ${recoveryCost:N0}");
            if (reputationLost > 0)
                sb.AppendLine($"Reputation Lost: {reputationLost:F1}");
            if (timeDelayMinutes > 0)
                sb.AppendLine($"Time Delay: {TimeDelayFormatted}");
            if (progressLost > 0)
                sb.AppendLine($"Progress Lost: {progressLost * 100:F0}%");
            if (investmentLossPercent > 0)
                sb.AppendLine($"Investment Lost: {investmentLossPercent * 100:F0}%");

            sb.AppendLine();
            sb.AppendLine($"Recoverable: {(isRecoverable ? "Yes" : "No")}");
            if (isRecoverable && recoveryPrice > 0)
                sb.AppendLine($"Recovery Price: ${recoveryPrice:N0}");

            return sb.ToString();
        }

        public override string ToString()
        {
            return $"Failure[{failureType}] Money:{moneyLost:N0} Rep:{reputationLost:F1} Time:{TimeDelayFormatted}";
        }
    }

    /// <summary>
    /// Event arguments for failure events
    /// </summary>
    public class FailureEventArgs : EventArgs
    {
        public FailureResult Failure { get; set; }
        public string Context { get; set; }
        public string Id { get; set; }

        public FailureEventArgs(FailureResult failure, string context, string id)
        {
            Failure = failure;
            Context = context;
            Id = id;
        }
    }
}
