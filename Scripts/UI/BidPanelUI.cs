using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Bid panel for submitting contract bids
    /// </summary>
    public class BidPanelUI : MonoBehaviour
    {
        #region Fields
        
        [Header("Contract Info")]
        [SerializeField] private TextMeshProUGUI contractNameText;
        [SerializeField] private TextMeshProUGUI clientText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI baseRewardText;
        
        [Header("Price Slider")]
        [SerializeField] private Slider priceSlider;
        [SerializeField] private TextMeshProUGUI priceValueText;
        [SerializeField] private TextMeshProUGUI priceLabelText;
        [SerializeField] private Button pricePresetLow;
        [SerializeField] private Button pricePresetMedium;
        [SerializeField] private Button pricePresetHigh;
        
        [Header("Deadline Slider")]
        [SerializeField] private Slider deadlineSlider;
        [SerializeField] private TextMeshProUGUI deadlineValueText;
        [SerializeField] private TextMeshProUGUI deadlineLabelText;
        [SerializeField] private Button deadlinePresetFast;
        [SerializeField] private Button deadlinePresetNormal;
        [SerializeField] private Button deadlinePresetRelaxed;
        
        [Header("Winning Chance")]
        [SerializeField] private TextMeshProUGUI winningChanceText;
        [SerializeField] private Slider chanceSlider;
        [SerializeField] private Image chanceFill;
        [SerializeField] private Color lowChanceColor;
        [SerializeField] private Color mediumChanceColor;
        [SerializeField] private Color highChanceColor;
        
        [Header("Competitors")]
        [SerializeField] private Transform competitorsContainer;
        [SerializeField] private GameObject competitorInfoPrefab;
        [SerializeField] private TextMeshProUGUI competitorCountText;
        
        [Header("Summary")]
        [SerializeField] private TextMeshProUGUI estimatedProfitText;
        [SerializeField] private TextMeshProUGUI reputationImpactText;
        [SerializeField] private TextMeshProUGUI riskImpactText;
        
        [Header("Buttons")]
        [SerializeField] private Button submitBidButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button autoBidButton;
        
        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private float animationDuration = 0.3f;
        
        // Runtime state
        private ContractData _contract;
        private float _currentBidPrice;
        private int _currentDeadline;
        private float _winningChance;
        private bool _isVisible;
        
        #endregion
        
        #region Properties
        
        public bool IsVisible => _isVisible;
        public float CurrentBidPrice => _currentBidPrice;
        public int CurrentDeadline => _currentDeadline;
        
        #endregion
        
        #region Events
        
        public event Action<ContractData, float, int> OnBidSubmitted;
        public event Action OnBidCancelled;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupSliders();
            SetupButtons();
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        #endregion
        
        #region Setup
        
        private void SetupSliders()
        {
            if (priceSlider != null)
            {
                priceSlider.onValueChanged.AddListener(OnPriceChanged);
            }
            
            if (deadlineSlider != null)
            {
                deadlineSlider.onValueChanged.AddListener(OnDeadlineChanged);
            }
        }
        
        private void SetupButtons()
        {
            if (submitBidButton != null)
            {
                submitBidButton.onClick.AddListener(OnSubmitBid);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancel);
            }
            
            if (autoBidButton != null)
            {
                autoBidButton.onClick.AddListener(OnAutoBid);
            }
            
            // Price presets
            if (pricePresetLow != null)
                pricePresetLow.onClick.AddListener(() => SetPricePreset(0.8f));
            if (pricePresetMedium != null)
                pricePresetMedium.onClick.AddListener(() => SetPricePreset(1f));
            if (pricePresetHigh != null)
                pricePresetHigh.onClick.AddListener(() => SetPricePreset(1.2f));
            
            // Deadline presets
            if (deadlinePresetFast != null)
                deadlinePresetFast.onClick.AddListener(() => SetDeadlinePreset(0.7f));
            if (deadlinePresetNormal != null)
                deadlinePresetNormal.onClick.AddListener(() => SetDeadlinePreset(1f));
            if (deadlinePresetRelaxed != null)
                deadlinePresetRelaxed.onClick.AddListener(() => SetDeadlinePreset(1.3f));
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the contract for bidding
        /// </summary>
        public void SetContract(ContractData contract)
        {
            _contract = contract;
            
            // Update contract info
            if (contractNameText != null)
            {
                contractNameText.text = contract.name;
            }
            
            if (clientText != null)
            {
                clientText.text = $"Client: {contract.clientName}";
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = contract.description;
            }
            
            if (baseRewardText != null)
            {
                baseRewardText.text = $"Base Reward: ${contract.reward:N0}";
            }
            
            // Setup slider ranges
            SetupSliderRanges();
            
            // Set default values
            SetPricePreset(1f);
            SetDeadlinePreset(1f);
            
            // Update competitors
            UpdateCompetitors();
            
            // Calculate initial chance
            UpdateWinningChance();
        }
        
        /// <summary>
        /// Shows the bid panel
        /// </summary>
        public void Show()
        {
            _isVisible = true;
            
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                UIAnimator.FadeIn(canvasGroup, animationDuration);
            }
            
            if (panelRect != null)
            {
                UIAnimator.SlideIn(panelRect, new Vector2(0, -500), animationDuration);
            }
        }
        
        /// <summary>
        /// Hides the bid panel
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            
            if (canvasGroup != null)
            {
                UIAnimator.FadeOut(canvasGroup, animationDuration);
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        #endregion
        
        #region Slider Management
        
        private void SetupSliderRanges()
        {
            if (_contract == null) return;
            
            // Price range: 50% - 150% of base reward
            if (priceSlider != null)
            {
                priceSlider.minValue = _contract.reward * 0.5f;
                priceSlider.maxValue = _contract.reward * 1.5f;
            }
            
            // Deadline range: 50% - 150% of base deadline
            if (deadlineSlider != null)
            {
                deadlineSlider.minValue = _contract.deadline * 0.5f;
                deadlineSlider.maxValue = _contract.deadline * 1.5f;
            }
        }
        
        private void OnPriceChanged(float value)
        {
            _currentBidPrice = value;
            
            if (priceValueText != null)
            {
                priceValueText.text = $"${_currentBidPrice:N0}";
            }
            
            UpdatePriceLabel();
            UpdateWinningChance();
            UpdateSummary();
        }
        
        private void OnDeadlineChanged(float value)
        {
            _currentDeadline = Mathf.RoundToInt(value);
            
            if (deadlineValueText != null)
            {
                int days = Mathf.CeilToInt(_currentDeadline / 24f);
                deadlineValueText.text = $"{days} days";
            }
            
            UpdateDeadlineLabel();
            UpdateWinningChance();
            UpdateSummary();
        }
        
        private void SetPricePreset(float multiplier)
        {
            if (_contract == null || priceSlider == null) return;
            
            float targetPrice = _contract.reward * multiplier;
            priceSlider.value = targetPrice;
        }
        
        private void SetDeadlinePreset(float multiplier)
        {
            if (_contract == null || deadlineSlider == null) return;
            
            float targetDeadline = _contract.deadline * multiplier;
            deadlineSlider.value = targetDeadline;
        }
        
        private void UpdatePriceLabel()
        {
            if (priceLabelText == null || _contract == null) return;
            
            float ratio = _currentBidPrice / _contract.reward;
            priceLabelText.text = ratio switch
            {
                < 0.8f => "Low Bid (Better Chance)",
                < 1.1f => "Competitive Bid",
                < 1.3f => "Premium Bid",
                _ => "High Bid (Lower Chance)"
            };
            
            priceLabelText.color = ratio switch
            {
                < 0.8f => UIManager.Instance.Theme.successColor,
                < 1.1f => UIManager.Instance.Theme.primaryColor,
                < 1.3f => UIManager.Instance.Theme.warningColor,
                _ => UIManager.Instance.Theme.errorColor
            };
        }
        
        private void UpdateDeadlineLabel()
        {
            if (deadlineLabelText == null || _contract == null) return;
            
            float ratio = _currentDeadline / _contract.deadline;
            deadlineLabelText.text = ratio switch
            {
                < 0.8f => "Fast Delivery (Better Chance)",
                < 1.1f => "Standard Delivery",
                < 1.3f => "Relaxed Timeline",
                _ => "Extended Timeline (Lower Chance)"
            };
        }
        
        #endregion
        
        #region Winning Chance Calculation
        
        /// <summary>
        /// Updates the winning chance display
        /// </summary>
        public void UpdateWinningChance()
        {
            if (_contract == null) return;
            
            // Calculate base chance
            _winningChance = CalculateWinningChance();
            
            // Update display
            if (winningChanceText != null)
            {
                winningChanceText.text = $"{Mathf.RoundToInt(_winningChance * 100)}%";
            }
            
            if (chanceSlider != null)
            {
                chanceSlider.value = _winningChance;
            }
            
            // Update color
            if (chanceFill != null)
            {
                chanceFill.color = GetChanceColor(_winningChance);
            }
            
            // Update submit button
            if (submitBidButton != null)
            {
                var buttonText = submitBidButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"Submit Bid ({Mathf.RoundToInt(_winningChance * 100)}% chance)";
                }
            }
        }
        
        private float CalculateWinningChance()
        {
            if (_contract == null || ContractManager.Instance == null) return 0.5f;
            
            // Base chance calculation
            float chance = 0.5f;
            
            // Price factor (lower bid = higher chance)
            float priceRatio = _currentBidPrice / _contract.reward;
            chance += (1f - priceRatio) * 0.3f;
            
            // Deadline factor (faster = higher chance)
            float deadlineRatio = _currentDeadline / _contract.deadline;
            chance += (1f - deadlineRatio) * 0.2f;
            
            // Reputation factor
            float reputationBonus = GameManager.Instance?.Reputation ?? 0.5f;
            chance += (reputationBonus - 0.5f) * 0.2f;
            
            // Competitor factor
            int competitors = _contract.competitorCount;
            chance -= competitors * 0.05f;
            
            // Clamp to valid range
            return Mathf.Clamp01(chance);
        }
        
        private Color GetChanceColor(float chance)
        {
            return chance switch
            {
                < 0.3f => lowChanceColor,
                < 0.6f => mediumChanceColor,
                _ => highChanceColor
            };
        }
        
        #endregion
        
        #region Summary & Competitors
        
        private void UpdateSummary()
        {
            if (_contract == null) return;
            
            // Estimated profit
            if (estimatedProfitText != null)
            {
                float profit = _contract.reward - _currentBidPrice + _contract.upfrontPayment;
                estimatedProfitText.text = $"${profit:N0}";
                estimatedProfitText.color = profit >= 0 ? 
                    UIManager.Instance.Theme.successColor : UIManager.Instance.Theme.errorColor;
            }
            
            // Reputation impact
            if (reputationImpactText != null)
            {
                float repImpact = CalculateReputationImpact();
                string sign = repImpact >= 0 ? "+" : "";
                reputationImpactText.text = $"{sign}{repImpact:F1}%";
                reputationImpactText.color = repImpact >= 0 ?
                    UIManager.Instance.Theme.successColor : UIManager.Instance.Theme.errorColor;
            }
            
            // Risk impact
            if (riskImpactText != null)
            {
                riskImpactText.text = $"+{_contract.riskLevel:F0}%";
                riskImpactText.color = _contract.riskLevel > 50 ?
                    UIManager.Instance.Theme.warningColor : UIManager.Instance.Theme.textSecondary;
            }
        }
        
        private float CalculateReputationImpact()
        {
            if (_contract == null) return 0f;
            
            float impact = _contract.reputationGain;
            
            // Adjust based on bid competitiveness
            float priceRatio = _currentBidPrice / _contract.reward;
            if (priceRatio < 0.9f) impact *= 1.2f;
            if (priceRatio > 1.2f) impact *= 0.8f;
            
            return impact;
        }
        
        private void UpdateCompetitors()
        {
            if (_contract == null) return;
            
            if (competitorCountText != null)
            {
                competitorCountText.text = $"{_contract.competitorCount} competitors";
            }
            
            // Clear and recreate competitor info
            if (competitorsContainer != null && competitorInfoPrefab != null)
            {
                foreach (Transform child in competitorsContainer)
                {
                    Destroy(child.gameObject);
                }
                
                // Add competitor indicators
                for (int i = 0; i < Mathf.Min(_contract.competitorCount, 5); i++)
                {
                    Instantiate(competitorInfoPrefab, competitorsContainer);
                }
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnSubmitBid()
        {
            if (_contract == null) return;
            
            OnBidSubmitted?.Invoke(_contract, _currentBidPrice, _currentDeadline);
            Hide();
        }
        
        private void OnCancel()
        {
            OnBidCancelled?.Invoke();
            Hide();
        }
        
        private void OnAutoBid()
        {
            if (_contract == null) return;
            
            // Calculate optimal bid
            float optimalPrice = _contract.reward * 0.95f;
            int optimalDeadline = Mathf.RoundToInt(_contract.deadline * 0.9f);
            
            // Apply to sliders
            if (priceSlider != null)
            {
                priceSlider.value = optimalPrice;
            }
            
            if (deadlineSlider != null)
            {
                deadlineSlider.value = optimalDeadline;
            }
            
            UIManager.Instance?.ShowNotification("Optimal bid calculated", NotificationType.Info);
        }
        
        #endregion
    }
}
