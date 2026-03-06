using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Contract debugger for testing contract systems
    /// Allows developers to generate, complete, and manipulate contracts
    /// </summary>
    public class ContractDebugger : MonoBehaviour
    {
        #region Singleton
        private static ContractDebugger _instance;
        public static ContractDebugger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ContractDebugger>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ContractDebugger");
                        _instance = go.AddComponent<ContractDebugger>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region UI References
        [Header("UI References")]
        [SerializeField] private TMP_InputField contractCountInput;
        [SerializeField] private Button generateContractsButton;
        [SerializeField] private Button winAllBidsButton;
        [SerializeField] private Button completeAllContractsButton;
        [SerializeField] private Button failAllContractsButton;
        [SerializeField] private Button resetContractsButton;
        [SerializeField] private Button showBidDetailsButton;
        [SerializeField] private Button autoResolveBidsButton;
        [SerializeField] private Button extendDeadlinesButton;
        [SerializeField] private Button doubleRewardsButton;
        [SerializeField] private TMP_Dropdown contractDropdown;
        [SerializeField] private TextMeshProUGUI activeContractsText;
        [SerializeField] private TextMeshProUGUI bidStatusText;
        [SerializeField] private TextMeshProUGUI contractStatsText;
        [SerializeField] private ScrollRect scrollRect;
        #endregion

        #region Contract Types
        [System.Serializable]
        public class DebugContract
        {
            public string contractId;
            public string contractName;
            public string clientName;
            public ContractType type;
            public ContractDifficulty difficulty;
            public ContractStatus status;
            public float reward;
            public float penalty;
            public float timeLimit; // in hours
            public float remainingTime;
            public float progress;
            public List<ContractRequirement> requirements;
            public List<ContractBid> bids;
            public DateTime createdAt;
            public DateTime deadline;
        }

        [System.Serializable]
        public class ContractRequirement
        {
            public RequirementType type;
            public float value;
            public string description;
        }

        [System.Serializable]
        public class ContractBid
        {
            public string bidderId;
            public string bidderName;
            public float bidAmount;
            public float reputation;
            public float techLevel;
            public float winProbability;
            public bool isPlayer;
            public bool isWinner;
        }

        public enum ContractType
        {
            Surveillance,
            Delivery,
            Mapping,
            Inspection,
            Agriculture,
            Security,
            Research,
            Photography,
            Emergency,
            Military
        }

        public enum ContractDifficulty
        {
            Easy,
            Medium,
            Hard,
            Expert,
            Legendary
        }

        public enum ContractStatus
        {
            Available,
            Bidding,
            Won,
            InProgress,
            Completed,
            Failed,
            Expired
        }

        public enum RequirementType
        {
            MinTechLevel,
            MinReputation,
            SpecificDrone,
            Certification,
            Insurance,
            Experience
        }
        #endregion

        #region Settings
        [Header("Contract Generation")]
        [Tooltip("Base reward for easy contracts")]
        public float baseReward = 5000f;
        
        [Tooltip("Maximum contracts to keep in memory")]
        public int maxContracts = 50;
        
        [Tooltip("Default contract duration in hours")]
        public float defaultDuration = 24f;
        
        [Tooltip("Enable auto-generation")]
        public bool autoGenerate = false;
        
        [Tooltip("Auto-generation interval in minutes")]
        public float autoGenerateInterval = 5f;
        #endregion

        #region Events
        public event Action<string> OnContractGenerated;
        public event Action<string> OnContractCompleted;
        public event Action<string> OnContractFailed;
        public event Action<string> OnBidWon;
        public event Action<string> OnBidLost;
        #endregion

        #region Private Fields
        private List<DebugContract> _contracts = new List<DebugContract>();
        private List<DebugContract> _activeContracts = new List<DebugContract>();
        private List<DebugContract> _availableContracts = new List<DebugContract>();
        private List<DebugContract> _biddingContracts = new List<DebugContract>();
        private int _contractIdCounter = 0;
        private Coroutine _autoGenerateCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(go);
        }

        private void Start()
        {
            SetupUI();
            
            if (autoGenerate)
            {
                StartAutoGeneration();
            }
        }

        private void Update()
        {
            UpdateContracts(Time.deltaTime * GetTimeScale());
        }
        #endregion

        #region UI Setup
        private void SetupUI()
        {
            if (generateContractsButton != null)
                generateContractsButton.onClick.AddListener(() => GenerateTestContractsFromInput());
            
            if (winAllBidsButton != null)
                winAllBidsButton.onClick.AddListener(WinAllBids);
            
            if (completeAllContractsButton != null)
                completeAllContractsButton.onClick.AddListener(CompleteAllContracts);
            
            if (failAllContractsButton != null)
                failAllContractsButton.onClick.AddListener(FailAllContracts);
            
            if (resetContractsButton != null)
                resetContractsButton.onClick.AddListener(ResetContracts);
            
            if (showBidDetailsButton != null)
                showBidDetailsButton.onClick.AddListener(ShowBidCalculationDetails);
            
            if (autoResolveBidsButton != null)
                autoResolveBidsButton.onClick.AddListener(AutoResolveAllBids);
            
            if (extendDeadlinesButton != null)
                extendDeadlinesButton.onClick.AddListener(ExtendAllDeadlines);
            
            if (doubleRewardsButton != null)
                doubleRewardsButton.onClick.AddListener(DoubleAllRewards);
            
            if (contractCountInput != null)
                contractCountInput.text = "5";
        }
        #endregion

        #region Contract Generation
        private void GenerateTestContractsFromInput()
        {
            if (int.TryParse(contractCountInput?.text, out int count))
            {
                GenerateTestContracts(count);
            }
        }

        /// <summary>
        /// Generate test contracts
        /// </summary>
        public void GenerateTestContracts(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GenerateRandomContract();
            }
            
            UpdateDisplay();
            PopulateContractDropdown();
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Generated {count} test contracts");
            }
        }

        private DebugContract GenerateRandomContract()
        {
            _contractIdCounter++;
            
            ContractType type = (ContractType)UnityEngine.Random.Range(0, 10);
            ContractDifficulty difficulty = GetRandomDifficulty();
            
            DebugContract contract = new DebugContract
            {
                contractId = $"contract_{_contractIdCounter}",
                contractName = GenerateContractName(type, difficulty),
                clientName = GenerateClientName(),
                type = type,
                difficulty = difficulty,
                status = ContractStatus.Available,
                reward = CalculateReward(difficulty),
                penalty = CalculatePenalty(difficulty),
                timeLimit = CalculateDuration(difficulty),
                remainingTime = CalculateDuration(difficulty),
                progress = 0f,
                requirements = GenerateRequirements(difficulty),
                bids = new List<ContractBid>(),
                createdAt = DateTime.Now,
                deadline = DateTime.Now.AddHours(CalculateDuration(difficulty))
            };
            
            // Generate AI bids
            GenerateAIBids(contract);
            
            _contracts.Add(contract);
            _availableContracts.Add(contract);
            
            OnContractGenerated?.Invoke(contract.contractId);
            
            return contract;
        }

        private string GenerateContractName(ContractType type, ContractDifficulty difficulty)
        {
            string[] locations = { "Downtown", "Industrial Zone", "Harbor", "Highway", "Forest", "Desert", "Mountain", "Coastal" };
            string location = locations[UnityEngine.Random.Range(0, locations.Length)];
            
            return $"{difficulty} {type} Mission - {location}";
        }

        private string GenerateClientName()
        {
            string[] prefixes = { "Global", "United", "Advanced", "Prime", "Secure", "Rapid", "Elite", "Strategic" };
            string[] suffixes = { "Corp", "Industries", "Solutions", "Systems", "Services", "Technologies", "Logistics", "Defense" };
            
            return $"{prefixes[UnityEngine.Random.Range(0, prefixes.Length)]} {suffixes[UnityEngine.Random.Range(0, suffixes.Length)]}";
        }

        private ContractDifficulty GetRandomDifficulty()
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.4f) return ContractDifficulty.Easy;
            if (roll < 0.7f) return ContractDifficulty.Medium;
            if (roll < 0.9f) return ContractDifficulty.Hard;
            if (roll < 0.98f) return ContractDifficulty.Expert;
            return ContractDifficulty.Legendary;
        }

        private float CalculateReward(ContractDifficulty difficulty)
        {
            float multiplier = difficulty switch
            {
                ContractDifficulty.Easy => 1f,
                ContractDifficulty.Medium => 2f,
                ContractDifficulty.Hard => 4f,
                ContractDifficulty.Expert => 8f,
                ContractDifficulty.Legendary => 20f,
                _ => 1f
            };
            
            return baseReward * multiplier * UnityEngine.Random.Range(0.9f, 1.1f);
        }

        private float CalculatePenalty(ContractDifficulty difficulty)
        {
            return CalculateReward(difficulty) * 0.1f;
        }

        private float CalculateDuration(ContractDifficulty difficulty)
        {
            float multiplier = difficulty switch
            {
                ContractDifficulty.Easy => 0.5f,
                ContractDifficulty.Medium => 1f,
                ContractDifficulty.Hard => 2f,
                ContractDifficulty.Expert => 4f,
                ContractDifficulty.Legendary => 8f,
                _ => 1f
            };
            
            return defaultDuration * multiplier;
        }

        private List<ContractRequirement> GenerateRequirements(ContractDifficulty difficulty)
        {
            List<ContractRequirement> requirements = new List<ContractRequirement>();
            
            int reqCount = difficulty switch
            {
                ContractDifficulty.Easy => 1,
                ContractDifficulty.Medium => 2,
                ContractDifficulty.Hard => 3,
                ContractDifficulty.Expert => 4,
                ContractDifficulty.Legendary => 5,
                _ => 1
            };
            
            for (int i = 0; i < reqCount; i++)
            {
                requirements.Add(new ContractRequirement
                {
                    type = (RequirementType)UnityEngine.Random.Range(0, 6),
                    value = UnityEngine.Random.Range(1f, 10f),
                    description = "Required for contract completion"
                });
            }
            
            return requirements;
        }

        private void GenerateAIBids(DebugContract contract)
        {
            int competitorCount = UnityEngine.Random.Range(2, 6);
            
            for (int i = 0; i < competitorCount; i++)
            {
                contract.bids.Add(new ContractBid
                {
                    bidderId = $"ai_{i}",
                    bidderName = $"Competitor {i + 1}",
                    bidAmount = contract.reward * UnityEngine.Random.Range(0.8f, 1.3f),
                    reputation = UnityEngine.Random.Range(20f, 80f),
                    techLevel = UnityEngine.Random.Range(1f, 8f),
                    isPlayer = false,
                    isWinner = false
                });
            }
        }
        #endregion

        #region Bid Management
        /// <summary>
        /// Win all pending bids
        /// </summary>
        public void WinAllBids()
        {
            int wonCount = 0;
            
            foreach (var contract in _availableContracts.ToList())
            {
                if (contract.status == ContractStatus.Available || contract.status == ContractStatus.Bidding)
                {
                    WinBid(contract);
                    wonCount++;
                }
            }
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Won {wonCount} bids");
            }
            
            UpdateDisplay();
        }

        private void WinBid(DebugContract contract)
        {
            contract.status = ContractStatus.Won;
            
            // Mark player as winner
            foreach (var bid in contract.bids)
            {
                bid.isWinner = bid.isPlayer;
            }
            
            // Add player bid if not exists
            if (!contract.bids.Any(b => b.isPlayer))
            {
                contract.bids.Add(new ContractBid
                {
                    bidderId = "player",
                    bidderName = "Your Company",
                    bidAmount = contract.reward,
                    reputation = 50f,
                    techLevel = 5f,
                    isPlayer = true,
                    isWinner = true
                });
            }
            
            _availableContracts.Remove(contract);
            _activeContracts.Add(contract);
            
            OnBidWon?.Invoke(contract.contractId);
        }

        /// <summary>
        /// Auto-resolve all pending bids
        /// </summary>
        public void AutoResolveAllBids()
        {
            foreach (var contract in _availableContracts.ToList())
            {
                ResolveBid(contract);
            }
            
            UpdateDisplay();
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Auto-resolved all bids");
            }
        }

        private void ResolveBid(DebugContract contract)
        {
            // Calculate win probabilities
            foreach (var bid in contract.bids)
            {
                bid.winProbability = CalculateWinProbability(bid, contract);
            }
            
            // Select winner
            var winner = SelectWinner(contract.bids);
            if (winner != null)
            {
                winner.isWinner = true;
                
                if (winner.isPlayer)
                {
                    contract.status = ContractStatus.Won;
                    _availableContracts.Remove(contract);
                    _activeContracts.Add(contract);
                    OnBidWon?.Invoke(contract.contractId);
                }
                else
                {
                    contract.status = ContractStatus.Failed; // Lost to competitor
                    _availableContracts.Remove(contract);
                    OnBidLost?.Invoke(contract.contractId);
                }
            }
        }

        private float CalculateWinProbability(ContractBid bid, DebugContract contract)
        {
            float baseProb = 1f / contract.bids.Count;
            
            // Adjust based on bid amount (lower is better)
            float lowestBid = contract.bids.Min(b => b.bidAmount);
            float bidFactor = lowestBid / bid.bidAmount;
            
            // Adjust based on reputation
            float repFactor = bid.reputation / 100f;
            
            // Adjust based on tech level
            float techFactor = bid.techLevel / 10f;
            
            return baseProb * bidFactor * (0.8f + repFactor * 0.2f) * (0.8f + techFactor * 0.2f);
        }

        private ContractBid SelectWinner(List<ContractBid> bids)
        {
            float totalProb = bids.Sum(b => b.winProbability);
            float roll = UnityEngine.Random.Range(0f, totalProb);
            
            float cumulative = 0f;
            foreach (var bid in bids)
            {
                cumulative += bid.winProbability;
                if (roll <= cumulative)
                {
                    return bid;
                }
            }
            
            return bids.LastOrDefault();
        }

        /// <summary>
        /// Show detailed bid calculation
        /// </summary>
        public void ShowBidCalculationDetails()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== BID CALCULATION DETAILS ===");
            sb.AppendLine();
            
            foreach (var contract in _availableContracts.Take(3))
            {
                sb.AppendLine($"Contract: {contract.contractName}");
                sb.AppendLine($"Reward: {contract.reward:C}");
                sb.AppendLine("Bids:");
                
                foreach (var bid in contract.bids)
                {
                    sb.AppendLine($"  {bid.bidderName}: {bid.bidAmount:C} " +
                                 $"(Rep: {bid.reputation:F0}, Tech: {bid.techLevel:F1}, " +
                                 $"Win%: {bid.winProbability * 100f:F1}%)");
                }
                
                sb.AppendLine();
            }
            
            if (activeContractsText != null)
            {
                activeContractsText.text = sb.ToString();
            }
            
            UnityEngine.Debug.Log(sb.ToString());
        }
        #endregion

        #region Contract Completion
        /// <summary>
        /// Complete all active contracts
        /// </summary>
        public void CompleteAllContracts()
        {
            int completedCount = 0;
            
            foreach (var contract in _activeContracts.ToList())
            {
                if (contract.status == ContractStatus.InProgress || contract.status == ContractStatus.Won)
                {
                    CompleteContract(contract);
                    completedCount++;
                }
            }
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Completed {completedCount} contracts");
            }
            
            UpdateDisplay();
        }

        private void CompleteContract(DebugContract contract)
        {
            contract.status = ContractStatus.Completed;
            contract.progress = 1f;
            
            _activeContracts.Remove(contract);
            
            OnContractCompleted?.Invoke(contract.contractId);
        }

        /// <summary>
        /// Fail all active contracts
        /// </summary>
        public void FailAllContracts()
        {
            int failedCount = 0;
            
            foreach (var contract in _activeContracts.ToList())
            {
                if (contract.status == ContractStatus.InProgress || contract.status == ContractStatus.Won)
                {
                    FailContract(contract);
                    failedCount++;
                }
            }
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Failed {failedCount} contracts");
            }
            
            UpdateDisplay();
        }

        private void FailContract(DebugContract contract)
        {
            contract.status = ContractStatus.Failed;
            
            _activeContracts.Remove(contract);
            
            OnContractFailed?.Invoke(contract.contractId);
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Reset all contracts
        /// </summary>
        public void ResetContracts()
        {
            _contracts.Clear();
            _activeContracts.Clear();
            _availableContracts.Clear();
            _biddingContracts.Clear();
            _contractIdCounter = 0;
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("All contracts reset");
            }
            
            UpdateDisplay();
            PopulateContractDropdown();
        }

        /// <summary>
        /// Extend all deadlines
        /// </summary>
        public void ExtendAllDeadlines()
        {
            foreach (var contract in _activeContracts)
            {
                contract.remainingTime += 24f; // Add 24 hours
                contract.deadline = contract.deadline.AddHours(24);
            }
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Extended all deadlines by 24 hours");
            }
            
            UpdateDisplay();
        }

        /// <summary>
        /// Double all rewards
        /// </summary>
        public void DoubleAllRewards()
        {
            foreach (var contract in _contracts)
            {
                contract.reward *= 2f;
            }
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Doubled all contract rewards");
            }
            
            UpdateDisplay();
        }

        private void UpdateContracts(float deltaTime)
        {
            foreach (var contract in _activeContracts)
            {
                if (contract.status == ContractStatus.InProgress)
                {
                    contract.remainingTime -= deltaTime / 3600f; // Convert seconds to hours
                    
                    // Auto-progress
                    float duration = contract.timeLimit * 3600f;
                    contract.progress += deltaTime / duration;
                    
                    if (contract.progress >= 1f)
                    {
                        contract.progress = 1f;
                        CompleteContract(contract);
                    }
                    
                    // Check deadline
                    if (contract.remainingTime <= 0)
                    {
                        FailContract(contract);
                    }
                }
            }
        }

        private void PopulateContractDropdown()
        {
            if (contractDropdown == null) return;
            
            contractDropdown.ClearOptions();
            
            var options = new List<TMP_Dropdown.OptionData>();
            options.Add(new TMP_Dropdown.OptionData("Select Contract..."));
            
            foreach (var contract in _contracts.Take(20))
            {
                string statusIcon = contract.status switch
                {
                    ContractStatus.Completed => "[✓] ",
                    ContractStatus.Failed => "[✗] ",
                    ContractStatus.InProgress => "[►] ",
                    _ => "[○] "
                };
                
                options.Add(new TMP_Dropdown.OptionData($"{statusIcon}{contract.contractName}"));
            }
            
            contractDropdown.AddOptions(options);
        }

        private void UpdateDisplay()
        {
            // Active contracts
            if (activeContractsText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("=== ACTIVE CONTRACTS ===");
                sb.AppendLine();
                
                foreach (var contract in _activeContracts.Take(10))
                {
                    sb.AppendLine($"{contract.contractName}");
                    sb.AppendLine($"  Progress: {contract.progress * 100f:F0}%");
                    sb.AppendLine($"  Time Left: {contract.remainingTime:F1}h");
                    sb.AppendLine($"  Reward: {contract.reward:C}");
                    sb.AppendLine();
                }
                
                activeContractsText.text = sb.ToString();
            }
            
            // Stats
            if (contractStatsText != null)
            {
                int total = _contracts.Count;
                int completed = _contracts.Count(c => c.status == ContractStatus.Completed);
                int failed = _contracts.Count(c => c.status == ContractStatus.Failed);
                int active = _activeContracts.Count;
                int available = _availableContracts.Count;
                
                float totalValue = _contracts.Where(c => c.status == ContractStatus.Completed).Sum(c => c.reward);
                
                contractStatsText.text = $"Total: {total} | Active: {active} | Available: {available}\n" +
                                        $"Completed: {completed} | Failed: {failed}\n" +
                                        $"Total Earnings: {totalValue:C}";
            }
        }

        private float GetTimeScale()
        {
            return DebugManager.Instance?.timeScale ?? 1f;
        }

        private void StartAutoGeneration()
        {
            if (_autoGenerateCoroutine != null)
            {
                StopCoroutine(_autoGenerateCoroutine);
            }
            
            _autoGenerateCoroutine = StartCoroutine(AutoGenerateRoutine());
        }

        private IEnumerator AutoGenerateRoutine()
        {
            while (autoGenerate)
            {
                yield return new WaitForSeconds(autoGenerateInterval * 60f / (DebugManager.Instance?.timeScale ?? 1f));
                
                if (_contracts.Count < maxContracts)
                {
                    GenerateRandomContract();
                    UpdateDisplay();
                }
            }
        }
        #endregion

        #region Getters
        /// <summary>
        /// Get all contracts
        /// </summary>
        public List<DebugContract> GetAllContracts()
        {
            return new List<DebugContract>(_contracts);
        }

        /// <summary>
        /// Get active contracts
        /// </summary>
        public List<DebugContract> GetActiveContracts()
        {
            return new List<DebugContract>(_activeContracts);
        }

        /// <summary>
        /// Get available contracts
        /// </summary>
        public List<DebugContract> GetAvailableContracts()
        {
            return new List<DebugContract>(_availableContracts);
        }
        #endregion
    }
}
