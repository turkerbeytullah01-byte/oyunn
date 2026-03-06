using System;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Current status of an active contract
    /// </summary>
    public enum ContractStatus
    {
        Bidding,        // Still accepting bids
        Accepted,       // Player won, waiting to start
        InProgress,     // Currently being worked on
        Completed,      // Successfully finished
        Failed,         // Failed to complete
        Expired,        // Deadline passed without completion
        Cancelled,      // Cancelled by player or client
        OnHold          // Temporarily paused
    }

    /// <summary>
    /// Type of failure for a contract
    /// </summary>
    public enum FailureType
    {
        None,
        MissedDeadline,
        QualityTooLow,
        InsufficientResources,
        TechFailure,
        ClientCancelled,
        PlayerCancelled,
        Bankruptcy,
        ForceMajeure
    }

    /// <summary>
    /// Delivery quality rating
    /// </summary>
    public enum DeliveryQuality
    {
        Terrible,       // < 0.5
        Poor,           // 0.5 - 0.7
        Acceptable,     // 0.7 - 0.85
        Good,           // 0.85 - 0.95
        Excellent,      // 0.95 - 1.05
        Outstanding     // > 1.05
    }

    /// <summary>
    /// Runtime state of an active contract
    /// </summary>
    [Serializable]
    public class ActiveContract
    {
        [Header("Identity")]
        public string instanceId;
        public ContractData data;
        public BidParameters acceptedBid;

        [Header("Status")]
        public ContractStatus status;
        public FailureType failureType;

        [Header("Timeline")]
        public DateTime startTime;
        public DateTime deadline;
        public DateTime? completionTime;
        public float totalDurationDays;
        public float elapsedDays;
        public float remainingDays;

        [Header("Progress")]
        [Range(0f, 1f)]
        public float progress;
        public float progressPerDay;
        public float estimatedCompletionDays;

        [Header("Quality")]
        public float qualityScore;
        public float qualityInvestmentRemaining;
        public float accumulatedQuality;

        [Header("Financial")]
        public float upfrontPaymentReceived;
        public float totalEarned;
        public float penaltiesApplied;
        public float bonusesEarned;

        [Header("Resources")]
        public int allocatedDrones;
        public float resourceEfficiency;
        public float droneUtilization;

        [Header("Events")]
        public bool hasHadDelayEvent;
        public bool hasHadQualityEvent;
        public int eventCount;

        [Header("Results")]
        public DeliveryQuality finalDeliveryQuality;
        public float finalReputationChange;
        public float finalReward;
        public string completionNotes;

        // Events
        public event Action<float> OnProgressUpdated;
        public event Action OnContractCompleted;
        public event Action<FailureType> OnContractFailed;
        public event Action<float> OnPenaltyApplied;

        /// <summary>
        /// Create a new active contract from a won bid
        /// </summary>
        public static ActiveContract Create(ContractData contractData, BidParameters bid, DateTime currentTime)
        {
            var active = new ActiveContract
            {
                instanceId = System.Guid.NewGuid().ToString("N")[..12],
                data = contractData,
                acceptedBid = bid,
                status = ContractStatus.Accepted,
                failureType = FailureType.None,
                startTime = currentTime,
                deadline = currentTime.AddDays(bid.proposedDeadlineDays),
                totalDurationDays = bid.proposedDeadlineDays,
                progress = 0f,
                qualityScore = 1f,
                qualityInvestmentRemaining = bid.qualityInvestment,
                upfrontPaymentReceived = contractData.GetUpfrontPayment(),
                totalEarned = contractData.GetUpfrontPayment(),
                allocatedDrones = bid.allocatedDroneCount,
                resourceEfficiency = 1f,
                hasHadDelayEvent = false,
                hasHadQualityEvent = false,
                eventCount = 0
            };

            // Calculate progress rate based on allocation
            active.CalculateProgressRate();

            return active;
        }

        /// <summary>
        /// Start the contract work
        /// </summary>
        public void Start(DateTime currentTime)
        {
            if (status != ContractStatus.Accepted)
            {
                Debug.LogWarning($"Cannot start contract {data.displayName} - status is {status}");
                return;
            }

            status = ContractStatus.InProgress;
            startTime = currentTime;
            deadline = currentTime.AddDays(acceptedBid.proposedDeadlineDays);
        }

        /// <summary>
        /// Update contract progress (call daily or on tick)
        /// </summary>
        public void UpdateProgress(float deltaDays, DateTime currentTime)
        {
            if (status != ContractStatus.InProgress)
                return;

            elapsedDays += deltaDays;
            remainingDays = (float)(deadline - currentTime).TotalDays;

            // Apply progress
            float progressDelta = progressPerDay * deltaDays * resourceEfficiency;
            progress += progressDelta;

            // Apply quality investment
            if (qualityInvestmentRemaining > 0)
            {
                float qualitySpend = Mathf.Min(
                    qualityInvestmentRemaining, 
                    acceptedBid.qualityInvestment * deltaDays / totalDurationDays);
                qualityInvestmentRemaining -= qualitySpend;
                accumulatedQuality += qualitySpend;
            }

            // Update quality score based on progress and investment
            UpdateQualityScore();

            // Check for deadline
            if (currentTime > deadline && progress < 1f)
            {
                ApplyLatePenalty(deltaDays);
            }

            // Clamp progress
            progress = Mathf.Clamp01(progress);

            // Calculate estimated completion
            if (progressPerDay > 0)
            {
                estimatedCompletionDays = (1f - progress) / progressPerDay;
            }

            OnProgressUpdated?.Invoke(progress);

            // Check for completion
            if (progress >= 1f && status == ContractStatus.InProgress)
            {
                Complete(currentTime);
            }
        }

        /// <summary>
        /// Calculate daily progress rate
        /// </summary>
        private void CalculateProgressRate()
        {
            // Base progress: 100% over deadline days
            float baseProgress = 1f / totalDurationDays;

            // Modify by drone allocation
            float droneModifier = Mathf.Sqrt(allocatedDrones) * 0.8f;

            // Modify by priority
            float priorityModifier = acceptedBid.resourcePriority switch
            {
                BidParameters.ResourcePriority.Speed => 1.3f,
                BidParameters.ResourcePriority.Quality => 0.85f,
                BidParameters.ResourcePriority.Cost => 0.9f,
                _ => 1f
            };

            progressPerDay = baseProgress * droneModifier * priorityModifier;
        }

        /// <summary>
        /// Update quality score based on investment and progress
        /// </summary>
        private void UpdateQualityScore()
        {
            float baseQuality = acceptedBid.targetQuality;
            
            // Quality from investment
            float investmentQuality = 1f;
            if (acceptedBid.qualityInvestment > 0)
            {
                float spentRatio = 1f - (qualityInvestmentRemaining / acceptedBid.qualityInvestment);
                investmentQuality = 1f + (spentRatio * 0.3f);
            }

            // Modify by priority
            float priorityModifier = acceptedBid.resourcePriority switch
            {
                BidParameters.ResourcePriority.Quality => 1.2f,
                BidParameters.ResourcePriority.Speed => 0.9f,
                _ => 1f
            };

            qualityScore = baseQuality * investmentQuality * priorityModifier;
        }

        /// <summary>
        /// Apply late penalty
        /// </summary>
        private void ApplyLatePenalty(float daysLate)
        {
            float penalty = data.CalculateLatePenalty(daysLate);
            penaltiesApplied += penalty;
            OnPenaltyApplied?.Invoke(penalty);

            // Check if max penalty reached
            float maxPenaltyAmount = data.baseReward * data.maxPenalty;
            if (penaltiesApplied >= maxPenaltyAmount)
            {
                // Auto-fail if penalties exceed max
                Fail(FailureType.MissedDeadline, DateTime.Now);
            }
        }

        /// <summary>
        /// Complete the contract
        /// </summary>
        public void Complete(DateTime currentTime)
        {
            if (status == ContractStatus.Completed || status == ContractStatus.Failed)
                return;

            status = ContractStatus.Completed;
            completionTime = currentTime;

            // Calculate early delivery bonus
            float daysEarly = (float)(deadline - currentTime).TotalDays;
            if (daysEarly > 0)
            {
                bonusesEarned += Mathf.Min(
                    data.baseReward * data.bonusForEarlyDelivery * daysEarly,
                    data.maxEarlyDeliveryBonus);
            }

            // Calculate quality bonus
            if (qualityScore > 1f)
            {
                bonusesEarned += data.baseReward * data.qualityBonusMultiplier * (qualityScore - 1f);
            }

            // Determine delivery quality rating
            finalDeliveryQuality = GetDeliveryQualityRating(qualityScore);

            // Calculate final reward
            finalReward = data.baseReward + bonusesEarned - penaltiesApplied;
            finalReward = Mathf.Max(0, finalReward);

            totalEarned = upfrontPaymentReceived + finalReward;

            // Calculate reputation change
            finalReputationChange = CalculateReputationChange();

            OnContractCompleted?.Invoke();
        }

        /// <summary>
        /// Fail the contract
        /// </summary>
        public void Fail(FailureType type, DateTime currentTime)
        {
            if (status == ContractStatus.Completed || status == ContractStatus.Failed)
                return;

            status = ContractStatus.Failed;
            failureType = type;
            completionTime = currentTime;

            // Calculate reputation penalty
            finalReputationChange = -data.reputationPenaltyOnFail;

            // Lose upfront payment if failed
            totalEarned = 0;
            finalReward = 0;

            OnContractFailed?.Invoke(type);
        }

        /// <summary>
        /// Cancel the contract (player initiated)
        /// </summary>
        public void CancelByPlayer()
        {
            if (status != ContractStatus.Accepted && status != ContractStatus.InProgress)
                return;

            status = ContractStatus.Cancelled;
            failureType = FailureType.PlayerCancelled;

            // Return partial upfront payment
            totalEarned = upfrontPaymentReceived * 0.5f;
            finalReputationChange = -data.reputationPenaltyOnFail * 0.5f;
        }

        /// <summary>
        /// Get delivery quality rating from score
        /// </summary>
        private DeliveryQuality GetDeliveryQualityRating(float score)
        {
            return score switch
            {
                < 0.5f => DeliveryQuality.Terrible,
                < 0.7f => DeliveryQuality.Poor,
                < 0.85f => DeliveryQuality.Acceptable,
                < 0.95f => DeliveryQuality.Good,
                < 1.05f => DeliveryQuality.Excellent,
                _ => DeliveryQuality.Outstanding
            };
        }

        /// <summary>
        /// Calculate reputation change from completion
        /// </summary>
        private float CalculateReputationChange()
        {
            float baseRep = data.reputationReward;

            // Modify by delivery quality
            float qualityMult = finalDeliveryQuality switch
            {
                DeliveryQuality.Terrible => 0f,
                DeliveryQuality.Poor => 0.5f,
                DeliveryQuality.Acceptable => 0.8f,
                DeliveryQuality.Good => 1f,
                DeliveryQuality.Excellent => 1.3f,
                DeliveryQuality.Outstanding => 1.6f,
                _ => 1f
            };

            // Modify by timeliness
            float timeMult = 1f;
            if (completionTime.HasValue && completionTime.Value > deadline)
            {
                timeMult = 0.7f; // Late delivery penalty
            }
            else if (bonusesEarned > 0)
            {
                timeMult = 1.1f; // Early delivery bonus
            }

            return baseRep * qualityMult * timeMult;
        }

        /// <summary>
        /// Check if contract is overdue
        /// </summary>
        public bool IsOverdue(DateTime currentTime)
        {
            return currentTime > deadline && progress < 1f;
        }

        /// <summary>
        /// Get days overdue
        /// </summary>
        public float GetDaysOverdue(DateTime currentTime)
        {
            if (!IsOverdue(currentTime)) return 0f;
            return (float)(currentTime - deadline).TotalDays;
        }

        /// <summary>
        /// Get completion percentage
        /// </summary>
        public float GetCompletionPercentage()
        {
            return progress * 100f;
        }

        /// <summary>
        /// Get time remaining percentage
        /// </summary>
        public float GetTimeRemainingPercentage()
        {
            if (totalDurationDays <= 0) return 0f;
            return (remainingDays / totalDurationDays) * 100f;
        }

        /// <summary>
        /// Get a summary of the contract state
        /// </summary>
        public string GetStatusSummary()
        {
            return status switch
            {
                ContractStatus.Bidding => "Accepting Bids",
                ContractStatus.Accepted => "Ready to Start",
                ContractStatus.InProgress => $"In Progress ({GetCompletionPercentage():F0}%)",
                ContractStatus.Completed => $"Completed - {finalDeliveryQuality}",
                ContractStatus.Failed => $"Failed - {failureType}",
                ContractStatus.Expired => "Expired",
                ContractStatus.Cancelled => "Cancelled",
                ContractStatus.OnHold => "On Hold",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get financial summary
        /// </summary>
        public string GetFinancialSummary()
        {
            return $"Earned: ${totalEarned:N0} | Penalties: ${penaltiesApplied:N0} | Bonuses: ${bonusesEarned:N0}";
        }

        /// <summary>
        /// Serialize to JSON for save/load
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Deserialize from JSON
        /// </summary>
        public static ActiveContract FromJson(string json)
        {
            return JsonUtility.FromJson<ActiveContract>(json);
        }
    }
}
