using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAegis.Systems.Contracts
{
    /// <summary>
    /// Main controller for the contract and bidding system
    /// </summary>
    public class ContractManager : MonoBehaviour
    {
        #region Singleton

        private static ContractManager _instance;
        public static ContractManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ContractManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("ContractManager");
                        _instance = go.AddComponent<ContractManager>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        #endregion

        #region Configuration

        [Header("Generation Settings")]
        [SerializeField] private int maxAvailableContracts = 6;
        [SerializeField] private float contractRefreshInterval = 3600f; // 1 hour in seconds
        [SerializeField] private List<ContractTemplate> contractTemplates = new List<ContractTemplate>();

        [Header("Player References")]
        [SerializeField] private float playerReputation = 10f;
        [SerializeField] private float playerTechLevel = 1f;
        [SerializeField] private int playerCompanyLevel = 1;
        [SerializeField] private int highestDroneTier = 1;
        [SerializeField] private List<string> unlockedTechnologies = new List<string>();

        #endregion

        #region State

        // Available contracts (not yet bid on)
        private List<ContractData> _availableContracts = new List<ContractData>();

        // Active contracts (won and in progress)
        private List<ActiveContract> _activeContracts = new List<ActiveContract>();

        // Completed contracts history
        private List<ActiveContract> _completedContracts = new List<ActiveContract>();

        // Contract generator
        private ContractGenerator _generator;

        // Last refresh time
        private float _lastRefreshTime;

        #endregion

        #region Events

        public event Action<List<ContractData>> OnContractsRefreshed;
        public event Action<ContractData> OnContractAvailable;
        public event Action<BidResult> OnBidSubmitted;
        public event Action<ActiveContract> OnContractStarted;
        public event Action<ActiveContract> OnContractCompleted;
        public event Action<ActiveContract, FailureType> OnContractFailed;
        public event Action<ActiveContract, float> OnContractProgressUpdated;

        #endregion

        #region Initialization

        private void Initialize()
        {
            _generator = new ContractGenerator();
            _lastRefreshTime = Time.time;

            // Generate initial contracts
            RefreshAvailableContracts();
        }

        #endregion

        #region Contract Generation

        /// <summary>
        /// Generate new contracts and refresh the available pool
        /// </summary>
        public void RefreshAvailableContracts()
        {
            int contractsNeeded = maxAvailableContracts - _availableContracts.Count;
            
            if (contractsNeeded > 0)
            {
                var newContracts = _generator.GenerateProgressAppropriateContracts(
                    playerReputation, playerTechLevel, playerCompanyLevel, contractsNeeded);

                foreach (var contract in newContracts)
                {
                    if (!_availableContracts.Any(c => c.contractId == contract.contractId))
                    {
                        _availableContracts.Add(contract);
                        OnContractAvailable?.Invoke(contract);
                    }
                }
            }

            _lastRefreshTime = Time.time;
            OnContractsRefreshed?.Invoke(_availableContracts);
        }

        /// <summary>
        /// Generate a specific number of contracts
        /// </summary>
        public List<ContractData> GenerateContracts(int count)
        {
            return _generator.GenerateContractBatch(count, playerReputation, playerTechLevel);
        }

        /// <summary>
        /// Get all currently available contracts
        /// </summary>
        public List<ContractData> GetAvailableContracts()
        {
            // Remove expired contracts
            _availableContracts.RemoveAll(c => c == null);
            
            return new List<ContractData>(_availableContracts);
        }

        /// <summary>
        /// Get contracts the player can actually bid on
        /// </summary>
        public List<ContractData> GetBidEligibleContracts()
        {
            return _availableContracts
                .Where(c => c.CanPlayerBid(playerTechLevel, playerReputation, unlockedTechnologies, highestDroneTier))
                .ToList();
        }

        #endregion

        #region Bidding System

        /// <summary>
        /// Submit a bid on a contract
        /// </summary>
        public BidResult SubmitBid(ContractData contract, BidParameters bid)
        {
            // Validate bid
            if (!bid.Validate(contract, out string errorMessage))
            {
                Debug.LogWarning($"Invalid bid: {errorMessage}");
                return CreateInvalidBidResult(errorMessage);
            }

            // Check if contract is still available
            if (!_availableContracts.Contains(contract))
            {
                return CreateInvalidBidResult("Contract no longer available");
            }

            // Calculate winning chance and result
            var (winningChance, breakdown, competitors) = BidCalculator.CalculateWinningChanceDetailed(
                contract, bid, playerReputation, playerTechLevel, unlockedTechnologies, highestDroneTier);

            // Determine winner
            bool isWinner = DetermineWinner(breakdown.totalScore, competitors);

            BidResult result;
            if (isWinner)
            {
                result = BidResult.CreateWin(breakdown.totalScore, competitors, breakdown, contract);
                
                // Move contract to active
                AcceptContractInternal(contract, bid);
            }
            else
            {
                float winningScore = competitors.Count > 0 ? competitors[0].finalScore : 0f;
                result = BidResult.CreateLoss(breakdown.totalScore, winningScore, winningChance, competitors, breakdown, contract);
            }

            OnBidSubmitted?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Preview bid outcome without submitting
        /// </summary>
        public (float winningChance, BidScoreBreakdown breakdown) PreviewBid(ContractData contract, BidParameters bid)
        {
            var (winningChance, breakdown, _) = BidCalculator.CalculateWinningChanceDetailed(
                contract, bid, playerReputation, playerTechLevel, unlockedTechnologies, highestDroneTier);

            return (winningChance, breakdown);
        }

        /// <summary>
        /// Get winning chance for a bid
        /// </summary>
        public float GetWinningChance(ContractData contract, BidParameters bid)
        {
            return BidCalculator.CalculateWinningChance(
                contract, bid, playerReputation, playerTechLevel, unlockedTechnologies, highestDroneTier);
        }

        /// <summary>
        /// Determine if player wins against competitors
        /// </summary>
        private bool DetermineWinner(float playerScore, List<CompetitorBid> competitors)
        {
            foreach (var competitor in competitors)
            {
                if (competitor.finalScore > playerScore)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Create result for invalid bid
        /// </summary>
        private BidResult CreateInvalidBidResult(string errorMessage)
        {
            return new BidResult
            {
                isWinner = false,
                winningChance = 0f,
                loseReason = errorMessage,
                improvementSuggestions = new List<string> { "Fix the bid parameters and try again" }
            };
        }

        #endregion

        #region Contract Management

        /// <summary>
        /// Accept a contract (after winning bid)
        /// </summary>
        public void AcceptContract(ContractData contract)
        {
            if (!_availableContracts.Contains(contract))
            {
                Debug.LogWarning("Contract not available to accept");
                return;
            }

            // Create default bid if accepting directly
            var defaultBid = BidParameters.CreateDefault(contract);
            AcceptContractInternal(contract, defaultBid);
        }

        /// <summary>
        /// Internal method to accept contract with bid
        /// </summary>
        private void AcceptContractInternal(ContractData contract, BidParameters bid)
        {
            // Remove from available
            _availableContracts.Remove(contract);

            // Create active contract
            var activeContract = ActiveContract.Create(contract, bid, DateTime.Now);
            _activeContracts.Add(activeContract);

            // Receive upfront payment
            float upfront = contract.GetUpfrontPayment();
            // TODO: Add to player money

            Debug.Log($"Contract accepted: {contract.displayName}. Upfront payment: ${upfront:N0}");

            OnContractStarted?.Invoke(activeContract);
        }

        /// <summary>
        /// Start working on an accepted contract
        /// </summary>
        public void StartContract(ActiveContract contract)
        {
            if (!_activeContracts.Contains(contract))
            {
                Debug.LogWarning("Contract not found in active contracts");
                return;
            }

            contract.Start(DateTime.Now);
        }

        /// <summary>
        /// Complete a contract with quality rating
        /// </summary>
        public void CompleteContract(ActiveContract contract, float quality)
        {
            if (!_activeContracts.Contains(contract))
                return;

            contract.Complete(DateTime.Now);
            
            // Apply rewards
            playerReputation += contract.finalReputationChange;
            // TODO: Add finalReward to player money

            // Move to completed
            _activeContracts.Remove(contract);
            _completedContracts.Add(contract);

            Debug.Log($"Contract completed: {contract.data.displayName}. Reward: ${contract.finalReward:N0}, Rep: {contract.finalReputationChange:F1}");

            OnContractCompleted?.Invoke(contract);

            // Refresh available contracts
            RefreshAvailableContracts();
        }

        /// <summary>
        /// Fail a contract
        /// </summary>
        public void FailContract(ActiveContract contract, FailureType failureType)
        {
            if (!_activeContracts.Contains(contract))
                return;

            contract.Fail(failureType, DateTime.Now);

            // Apply penalties
            playerReputation += contract.finalReputationChange;

            // Move to completed (as failed)
            _activeContracts.Remove(contract);
            _completedContracts.Add(contract);

            Debug.Log($"Contract failed: {contract.data.displayName}. Reason: {failureType}");

            OnContractFailed?.Invoke(contract, failureType);
        }

        /// <summary>
        /// Cancel a contract (player initiated)
        /// </summary>
        public void CancelContract(ActiveContract contract)
        {
            if (!_activeContracts.Contains(contract))
                return;

            contract.CancelByPlayer();
            playerReputation += contract.finalReputationChange;

            _activeContracts.Remove(contract);
            _completedContracts.Add(contract);

            Debug.Log($"Contract cancelled: {contract.data.displayName}");
        }

        /// <summary>
        /// Get all active contracts
        /// </summary>
        public List<ActiveContract> GetActiveContracts()
        {
            return new List<ActiveContract>(_activeContracts);
        }

        /// <summary>
        /// Get contracts by status
        /// </summary>
        public List<ActiveContract> GetContractsByStatus(ContractStatus status)
        {
            return _activeContracts.Where(c => c.status == status).ToList();
        }

        /// <summary>
        /// Get completed contracts history
        /// </summary>
        public List<ActiveContract> GetCompletedContracts()
        {
            return new List<ActiveContract>(_completedContracts);
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            // Auto-refresh contracts
            if (Time.time - _lastRefreshTime > contractRefreshInterval)
            {
                RefreshAvailableContracts();
            }

            // Update active contracts
            UpdateActiveContracts();
        }

        /// <summary>
        /// Update all active contracts
        /// </summary>
        private void UpdateActiveContracts()
        {
            float deltaDays = Time.deltaTime / 86400f; // Convert seconds to days

            foreach (var contract in _activeContracts.ToList())
            {
                if (contract.status == ContractStatus.InProgress)
                {
                    contract.UpdateProgress(deltaDays, DateTime.Now);
                    OnContractProgressUpdated?.Invoke(contract, contract.progress);

                    // Check for auto-fail conditions
                    if (contract.IsOverdue(DateTime.Now))
                    {
                        float daysOverdue = contract.GetDaysOverdue(DateTime.Now);
                        float maxOverdueDays = contract.data.deadlineDays * 0.5f; // 50% of deadline

                        if (daysOverdue > maxOverdueDays)
                        {
                            FailContract(contract, FailureType.MissedDeadline);
                        }
                    }
                }
            }
        }

        #endregion

        #region Player Stats Management

        /// <summary>
        /// Update player stats (call when player progresses)
        /// </summary>
        public void UpdatePlayerStats(float reputation, float techLevel, int companyLevel, int droneTier)
        {
            playerReputation = reputation;
            playerTechLevel = techLevel;
            playerCompanyLevel = companyLevel;
            highestDroneTier = droneTier;
        }

        /// <summary>
        /// Add unlocked technology
        /// </summary>
        public void UnlockTechnology(string techId)
        {
            if (!unlockedTechnologies.Contains(techId))
            {
                unlockedTechnologies.Add(techId);
            }
        }

        /// <summary>
        /// Get player reputation
        /// </summary>
        public float GetPlayerReputation() => playerReputation;

        /// <summary>
        /// Get player tech level
        /// </summary>
        public float GetPlayerTechLevel() => playerTechLevel;

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get recommended bid for a contract
        /// </summary>
        public BidParameters GetRecommendedBid(ContractData contract, float desiredWinChance = 0.7f)
        {
            return BidCalculator.RecommendBid(contract, playerReputation, playerTechLevel, desiredWinChance);
        }

        /// <summary>
        /// Get contract statistics
        /// </summary>
        public ContractStats GetStatistics()
        {
            return new ContractStats
            {
                TotalCompleted = _completedContracts.Count(c => c.status == ContractStatus.Completed),
                TotalFailed = _completedContracts.Count(c => c.status == ContractStatus.Failed),
                TotalCancelled = _completedContracts.Count(c => c.status == ContractStatus.Cancelled),
                CurrentlyActive = _activeContracts.Count,
                AvailableToBid = _availableContracts.Count,
                TotalRevenue = _completedContracts.Sum(c => c.totalEarned),
                TotalReputationGained = _completedContracts.Sum(c => c.finalReputationChange),
                AverageCompletionQuality = _completedContracts.Count > 0 
                    ? _completedContracts.Average(c => c.qualityScore) 
                    : 0f
            };
        }

        /// <summary>
        /// Save contract data
        /// </summary>
        public string SaveToJson()
        {
            var saveData = new ContractSaveData
            {
                PlayerReputation = playerReputation,
                PlayerTechLevel = playerTechLevel,
                ActiveContracts = _activeContracts.Select(c => c.ToJson()).ToList(),
                CompletedContracts = _completedContracts.Select(c => c.ToJson()).ToList()
            };
            return JsonUtility.ToJson(saveData);
        }

        /// <summary>
        /// Load contract data
        /// </summary>
        public void LoadFromJson(string json)
        {
            var saveData = JsonUtility.FromJson<ContractSaveData>(json);
            
            playerReputation = saveData.PlayerReputation;
            playerTechLevel = saveData.PlayerTechLevel;
            
            _activeContracts = saveData.ActiveContracts.Select(ActiveContract.FromJson).ToList();
            _completedContracts = saveData.CompletedContracts.Select(ActiveContract.FromJson).ToList();
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Contract statistics
    /// </summary>
    [Serializable]
    public class ContractStats
    {
        public int TotalCompleted;
        public int TotalFailed;
        public int TotalCancelled;
        public int CurrentlyActive;
        public int AvailableToBid;
        public float TotalRevenue;
        public float TotalReputationGained;
        public float AverageCompletionQuality;
    }

    /// <summary>
    /// Save data structure
    /// </summary>
    [Serializable]
    public class ContractSaveData
    {
        public float PlayerReputation;
        public float PlayerTechLevel;
        public List<string> ActiveContracts;
        public List<string> CompletedContracts;
    }

    #endregion
}
