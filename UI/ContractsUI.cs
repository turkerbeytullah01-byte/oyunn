using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Contract filter options
    /// </summary>
    public enum ContractFilter
    {
        All,
        Available,
        Bidding,
        Active,
        Completed,
        Failed
    }
    
    /// <summary>
    /// Contract sorting options
    /// </summary>
    public enum ContractSort
    {
        Deadline,
        Reward,
        Difficulty,
        Reputation
    }
    
    /// <summary>
    /// Main contracts screen - displays contract list and handles bidding
    /// </summary>
    public class ContractsUI : BaseScreen
    {
        #region Fields
        
        [Header("Contract List")]
        [SerializeField] private Transform contractListContainer;
        [SerializeField] private ContractCardUI contractCardPrefab;
        [SerializeField] private ScrollRect contractsScrollRect;
        
        [Header("Empty State")]
        [SerializeField] private GameObject emptyStatePanel;
        [SerializeField] private TextMeshProUGUI emptyStateText;
        
        [Header("Filter & Sort")]
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Toggle showUnavailableToggle;
        
        [Header("Bid Panel")]
        [SerializeField] private BidPanelUI bidPanel;
        
        [Header("Contract Detail")]
        [SerializeField] private ContractDetailPanel detailPanel;
        
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI activeContractsText;
        [SerializeField] private TextMeshProUGUI completedContractsText;
        [SerializeField] private TextMeshProUGUI totalEarningsText;
        
        [Header("Refresh")]
        [SerializeField] private Button refreshButton;
        [SerializeField] private float refreshCooldown = 5f;
        
        // Runtime state
        private Dictionary<string, ContractCardUI> _contractCards = new Dictionary<string, ContractCardUI>();
        private ContractFilter _currentFilter = ContractFilter.All;
        private ContractSort _currentSort = ContractSort.Deadline;
        private bool _showUnavailable = false;
        private float _lastRefreshTime;
        
        #endregion
        
        #region BaseScreen Implementation
        
        protected override void Initialize()
        {
            SetupUI();
            
            if (bidPanel != null)
            {
                bidPanel.OnBidSubmitted += OnBidSubmitted;
                bidPanel.OnBidCancelled += OnBidCancelled;
            }
        }
        
        public override void OnShow()
        {
            RefreshContracts();
            UpdateStats();
            AnimateIn();
            
            // Subscribe to contract events
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractAvailable += OnContractAvailable;
                ContractManager.Instance.OnContractBidResult += OnContractBidResult;
                ContractManager.Instance.OnContractCompleted += OnContractCompleted;
            }
        }
        
        public override void OnHide()
        {
            // Hide panels
            bidPanel?.Hide();
            detailPanel?.Hide();
            
            // Unsubscribe from events
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractAvailable -= OnContractAvailable;
                ContractManager.Instance.OnContractBidResult -= OnContractBidResult;
                ContractManager.Instance.OnContractCompleted -= OnContractCompleted;
            }
        }
        
        public override void OnRefresh()
        {
            RefreshContracts();
            UpdateStats();
        }
        
        #endregion
        
        #region Setup
        
        private void SetupUI()
        {
            // Setup filter dropdown
            if (filterDropdown != null)
            {
                filterDropdown.ClearOptions();
                filterDropdown.AddOptions(new List<string> 
                { 
                    "All Contracts", "Available", "Bidding", "Active", 
                    "Completed", "Failed" 
                });
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }
            
            // Setup sort dropdown
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(new List<string> 
                { 
                    "Sort by Deadline", "Sort by Reward", "Sort by Difficulty", 
                    "Sort by Reputation" 
                });
                sortDropdown.onValueChanged.AddListener(OnSortChanged);
            }
            
            // Setup toggle
            if (showUnavailableToggle != null)
            {
                showUnavailableToggle.onValueChanged.AddListener(OnShowUnavailableChanged);
            }
            
            // Setup refresh button
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(OnRefreshClicked);
            }
        }
        
        #endregion
        
        #region Contract List Management
        
        /// <summary>
        /// Refreshes the contract list display
        /// </summary>
        public void RefreshContracts()
        {
            ClearContractList();
            
            if (ContractManager.Instance == null) return;
            
            // Get contracts based on filter
            var contracts = GetFilteredContracts();
            
            // Sort contracts
            contracts = SortContracts(contracts);
            
            // Show empty state if needed
            bool isEmpty = contracts.Count == 0;
            if (emptyStatePanel != null)
            {
                emptyStatePanel.SetActive(isEmpty);
            }
            
            if (isEmpty)
            {
                UpdateEmptyStateMessage();
                return;
            }
            
            // Create contract cards
            foreach (var contract in contracts)
            {
                CreateContractCard(contract);
            }
            
            _lastRefreshTime = Time.time;
        }
        
        private void CreateContractCard(ContractData contract)
        {
            if (contractCardPrefab == null || contractListContainer == null) return;
            
            var card = Instantiate(contractCardPrefab, contractListContainer);
            card.SetData(contract);
            
            // Subscribe to card events
            card.OnDetailsClicked += () => ShowContractDetails(contract);
            card.OnBidClicked += () => ShowBidPanel(contract);
            
            _contractCards[contract.id] = card;
        }
        
        private void ClearContractList()
        {
            foreach (var kvp in _contractCards)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.OnDetailsClicked -= null;
                    kvp.Value.OnBidClicked -= null;
                    Destroy(kvp.Value.gameObject);
                }
            }
            _contractCards.Clear();
        }
        
        private List<ContractData> GetFilteredContracts()
        {
            if (ContractManager.Instance == null) 
                return new List<ContractData>();
            
            var allContracts = ContractManager.Instance.GetAllContracts();
            var filtered = new List<ContractData>();
            
            foreach (var contract in allContracts)
            {
                bool include = _currentFilter switch
                {
                    ContractFilter.All => true,
                    ContractFilter.Available => contract.status == ContractStatus.Available,
                    ContractFilter.Bidding => contract.status == ContractStatus.Bidding,
                    ContractFilter.Active => contract.status == ContractStatus.Active,
                    ContractFilter.Completed => contract.status == ContractStatus.Completed,
                    ContractFilter.Failed => contract.status == ContractStatus.Failed,
                    _ => true
                };
                
                // Filter unavailable if needed
                if (!_showUnavailable && !ContractManager.Instance.CanBid(contract.id))
                {
                    include = false;
                }
                
                if (include)
                {
                    filtered.Add(contract);
                }
            }
            
            return filtered;
        }
        
        private List<ContractData> SortContracts(List<ContractData> contracts)
        {
            return _currentSort switch
            {
                ContractSort.Deadline => contracts.SortBy(c => c.deadline),
                ContractSort.Reward => contracts.SortByDescending(c => c.reward),
                ContractSort.Difficulty => contracts.SortBy(c => c.difficulty),
                ContractSort.Reputation => contracts.SortByDescending(c => c.reputationGain),
                _ => contracts
            };
        }
        
        private void UpdateEmptyStateMessage()
        {
            if (emptyStateText == null) return;
            
            emptyStateText.text = _currentFilter switch
            {
                ContractFilter.Available => "No available contracts at the moment.",
                ContractFilter.Bidding => "You have no active bids.",
                ContractFilter.Active => "No active contracts.",
                ContractFilter.Completed => "No completed contracts yet.",
                ContractFilter.Failed => "No failed contracts.",
                _ => "No contracts found."
            };
        }
        
        #endregion
        
        #region Contract Details & Bidding
        
        /// <summary>
        /// Shows contract details
        /// </summary>
        public void ShowContractDetails(ContractData contract)
        {
            if (detailPanel != null)
            {
                detailPanel.Show(contract);
            }
        }
        
        /// <summary>
        /// Shows the bid panel for a contract
        /// </summary>
        public void ShowBidPanel(ContractData contract)
        {
            if (bidPanel != null)
            {
                bidPanel.SetContract(contract);
                bidPanel.Show();
            }
        }
        
        private void OnBidSubmitted(ContractData contract, float bidPrice, int proposedDays)
        {
            if (ContractManager.Instance == null) return;
            
            bool success = ContractManager.Instance.SubmitBid(contract.id, bidPrice, proposedDays);
            
            if (success)
            {
                UIManager.Instance?.ShowNotification(
                    $"Bid submitted for {contract.name}",
                    NotificationType.Success
                );
                
                RefreshContracts();
            }
            else
            {
                UIManager.Instance?.ShowNotification(
                    "Failed to submit bid. Check your resources.",
                    NotificationType.Error
                );
            }
        }
        
        private void OnBidCancelled()
        {
            bidPanel?.Hide();
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnContractAvailable(ContractData contract)
        {
            // Only add if it matches current filter
            if (_currentFilter == ContractFilter.All || _currentFilter == ContractFilter.Available)
            {
                RefreshContracts();
                
                UIManager.Instance?.ShowNotification(
                    $"New contract available: {contract.name}",
                    NotificationType.Info
                );
            }
        }
        
        private void OnContractBidResult(ContractData contract, bool won)
        {
            RefreshContracts();
            UpdateStats();
            
            if (won)
            {
                UIManager.Instance?.ShowNotification(
                    $"Bid won! Contract: {contract.name}",
                    NotificationType.Success
                );
            }
            else
            {
                UIManager.Instance?.ShowNotification(
                    $"Bid lost for: {contract.name}",
                    NotificationType.Warning
                );
            }
        }
        
        private void OnContractCompleted(ContractData contract, bool success)
        {
            RefreshContracts();
            UpdateStats();
            
            if (success)
            {
                UIManager.Instance?.ShowNotification(
                    $"Contract completed: {contract.name}",
                    NotificationType.Success
                );
            }
            else
            {
                UIManager.Instance?.ShowNotification(
                    $"Contract failed: {contract.name}",
                    NotificationType.Error
                );
            }
        }
        
        #endregion
        
        #region UI Handlers
        
        private void OnFilterChanged(int index)
        {
            _currentFilter = (ContractFilter)index;
            RefreshContracts();
        }
        
        private void OnSortChanged(int index)
        {
            _currentSort = (ContractSort)index;
            RefreshContracts();
        }
        
        private void OnShowUnavailableChanged(bool show)
        {
            _showUnavailable = show;
            RefreshContracts();
        }
        
        private void OnRefreshClicked()
        {
            if (Time.time - _lastRefreshTime < refreshCooldown)
            {
                float remaining = refreshCooldown - (Time.time - _lastRefreshTime);
                UIManager.Instance?.ShowNotification(
                    $"Please wait {Mathf.CeilToInt(remaining)}s before refreshing",
                    NotificationType.Warning
                );
                return;
            }
            
            ContractManager.Instance?.RefreshAvailableContracts();
            RefreshContracts();
            
            UIManager.Instance?.ShowNotification("Contracts refreshed", NotificationType.Info);
        }
        
        #endregion
        
        #region Stats
        
        private void UpdateStats()
        {
            if (ContractManager.Instance == null) return;
            
            if (activeContractsText != null)
            {
                int active = ContractManager.Instance.GetContractsByStatus(ContractStatus.Active).Count;
                activeContractsText.text = $"Active: {active}";
            }
            
            if (completedContractsText != null)
            {
                int completed = ContractManager.Instance.GetContractsByStatus(ContractStatus.Completed).Count;
                completedContractsText.text = $"Completed: {completed}";
            }
            
            if (totalEarningsText != null)
            {
                float earnings = ContractManager.Instance.TotalEarnings;
                totalEarningsText.text = $"Earnings: ${earnings:N0}";
            }
        }
        
        #endregion
        
        #region Utility
        
        public override bool OnBackPressed()
        {
            if (bidPanel != null && bidPanel.IsVisible)
            {
                bidPanel.Hide();
                return true;
            }
            
            if (detailPanel != null && detailPanel.IsVisible)
            {
                detailPanel.Hide();
                return true;
            }
            
            return false;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Extension methods for sorting
    /// </summary>
    public static class ListExtensions
    {
        public static List<T> SortBy<T, TKey>(this List<T> list, System.Func<T, TKey> keySelector)
        {
            list.Sort((a, b) => Comparer<TKey>.Default.Compare(keySelector(a), keySelector(b)));
            return list;
        }
        
        public static List<T> SortByDescending<T, TKey>(this List<T> list, System.Func<T, TKey> keySelector)
        {
            list.Sort((a, b) => Comparer<TKey>.Default.Compare(keySelector(b), keySelector(a)));
            return list;
        }
    }
}
