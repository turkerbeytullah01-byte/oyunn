using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Contract status enumeration
    /// </summary>
    public enum ContractStatus
    {
        Available,  // Can bid
        Bidding,    // Bid submitted, waiting result
        Active,     // Contract won, in progress
        Completed,  // Successfully completed
        Failed,     // Failed or lost bid
        Expired     // Deadline passed
    }
    
    /// <summary>
    /// Individual contract card in the contract list
    /// </summary>
    public class ContractCardUI : MonoBehaviour
    {
        #region Fields
        
        [Header("Contract Info")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI clientText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [Header("Financial")]
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI penaltyText;
        [SerializeField] private TextMeshProUGUI upfrontText;
        
        [Header("Requirements")]
        [SerializeField] private TextMeshProUGUI deadlineText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private Transform requirementsContainer;
        [SerializeField] private GameObject requirementPrefab;
        
        [Header("Indicators")]
        [SerializeField] private RiskIndicatorUI riskIndicator;
        [SerializeField] private Image statusIcon;
        [SerializeField] private GameObject newBadge;
        [SerializeField] private GameObject urgentBadge;
        
        [Header("Progress")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        
        [Header("Buttons")]
        [SerializeField] private Button detailsButton;
        [SerializeField] private Button bidButton;
        [SerializeField] private Button cancelButton;
        
        [Header("Visual")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image cardBorder;
        [SerializeField] private Color availableColor;
        [SerializeField] private Color biddingColor;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color completedColor;
        [SerializeField] private Color failedColor;
        
        [Header("Animation")]
        [SerializeField] private float hoverScale = 1.02f;
        
        // Runtime state
        private ContractData _data;
        private ContractStatus _status;
        private Vector3 _originalScale;
        
        #endregion
        
        #region Events
        
        public event Action OnDetailsClicked;
        public event Action OnBidClicked;
        public event Action OnCancelClicked;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _originalScale = transform.localScale;
            SetupButtons();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (detailsButton != null)
            {
                detailsButton.onClick.AddListener(() => OnDetailsClicked?.Invoke());
            }
            
            if (bidButton != null)
            {
                bidButton.onClick.AddListener(() => OnBidClicked?.Invoke());
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            OnDetailsClicked = null;
            OnBidClicked = null;
            OnCancelClicked = null;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the contract data for this card
        /// </summary>
        public void SetData(ContractData data)
        {
            _data = data;
            _status = data.status;
            
            UpdateVisuals();
            UpdateButtons();
            UpdateStatusVisuals();
        }
        
        /// <summary>
        /// Updates the card with current contract state
        /// </summary>
        public void UpdateState()
        {
            if (_data == null) return;
            
            _status = _data.status;
            UpdateStatusVisuals();
            UpdateButtons();
            
            // Update progress if active
            if (_status == ContractStatus.Active && progressSlider != null)
            {
                progressSlider.value = _data.progress;
                
                if (progressText != null)
                {
                    progressText.text = $"{Mathf.RoundToInt(_data.progress * 100)}%";
                }
            }
        }
        
        #endregion
        
        #region Visual Updates
        
        private void UpdateVisuals()
        {
            if (_data == null) return;
            
            // Basic info
            if (nameText != null)
            {
                nameText.text = _data.name;
            }
            
            if (clientText != null)
            {
                clientText.text = _data.clientName;
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = _data.shortDescription;
            }
            
            // Financial
            if (rewardText != null)
            {
                rewardText.text = $"${_data.reward:N0}";
            }
            
            if (penaltyText != null)
            {
                penaltyText.text = $"-${_data.penalty:N0}";
                penaltyText.gameObject.SetActive(_data.penalty > 0);
            }
            
            if (upfrontText != null)
            {
                upfrontText.text = $"+${_data.upfrontPayment:N0}";
                upfrontText.gameObject.SetActive(_data.upfrontPayment > 0);
            }
            
            // Deadline
            if (deadlineText != null)
            {
                int daysRemaining = Mathf.CeilToInt(_data.deadline / 24f);
                deadlineText.text = daysRemaining > 1 ? $"{daysRemaining} days" : "< 1 day";
                
                // Color based on urgency
                if (daysRemaining <= 1)
                {
                    deadlineText.color = UIManager.Instance.Theme.errorColor;
                }
                else if (daysRemaining <= 3)
                {
                    deadlineText.color = UIManager.Instance.Theme.warningColor;
                }
            }
            
            // Difficulty
            if (difficultyText != null)
            {
                difficultyText.text = GetDifficultyString(_data.difficulty);
                difficultyText.color = GetDifficultyColor(_data.difficulty);
            }
            
            // Risk
            if (riskIndicator != null)
            {
                riskIndicator.SetRisk(_data.riskLevel);
            }
            
            // Requirements
            UpdateRequirements();
            
            // Badges
            UpdateBadges();
        }
        
        private void UpdateRequirements()
        {
            if (requirementsContainer == null || requirementPrefab == null) return;
            
            // Clear existing
            foreach (Transform child in requirementsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add requirement icons
            if (_data.requiredTechnologies != null)
            {
                foreach (var tech in _data.requiredTechnologies)
                {
                    var req = Instantiate(requirementPrefab, requirementsContainer);
                    var icon = req.GetComponent<Image>();
                    if (icon != null)
                    {
                        // Set tech icon
                        // icon.sprite = tech.icon;
                    }
                }
            }
        }
        
        private void UpdateBadges()
        {
            if (newBadge != null)
            {
                newBadge.SetActive(_data.isNew);
            }
            
            if (urgentBadge != null)
            {
                float urgencyThreshold = 24f; // 1 day
                urgentBadge.SetActive(_data.deadline <= urgencyThreshold);
            }
        }
        
        private void UpdateButtons()
        {
            if (bidButton != null)
            {
                bidButton.gameObject.SetActive(_status == ContractStatus.Available);
            }
            
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(_status == ContractStatus.Bidding);
            }
            
            if (detailsButton != null)
            {
                // Always show details button
                detailsButton.gameObject.SetActive(true);
            }
        }
        
        private void UpdateStatusVisuals()
        {
            Color statusColor = GetStatusColor(_status);
            
            if (cardBackground != null)
            {
                cardBackground.color = statusColor;
            }
            
            if (cardBorder != null)
            {
                cardBorder.color = _status == ContractStatus.Available ? 
                    UIManager.Instance.Theme.primaryColor : statusColor;
            }
            
            // Update status icon
            if (statusIcon != null)
            {
                // statusIcon.sprite = GetStatusIcon(_status);
            }
            
            // Progress visibility
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(_status == ContractStatus.Active);
            }
            
            if (progressText != null)
            {
                progressText.gameObject.SetActive(_status == ContractStatus.Active);
            }
        }
        
        #endregion
        
        #region Helpers
        
        private string GetDifficultyString(int difficulty)
        {
            return difficulty switch
            {
                <= 2 => "Easy",
                <= 4 => "Medium",
                <= 6 => "Hard",
                <= 8 => "Expert",
                _ => "Extreme"
            };
        }
        
        private Color GetDifficultyColor(int difficulty)
        {
            return difficulty switch
            {
                <= 2 => UIManager.Instance.Theme.successColor,
                <= 4 => UIManager.Instance.Theme.primaryColor,
                <= 6 => UIManager.Instance.Theme.warningColor,
                <= 8 => new Color(1f, 0.4f, 0.2f),
                _ => UIManager.Instance.Theme.errorColor
            };
        }
        
        private Color GetStatusColor(ContractStatus status)
        {
            return status switch
            {
                ContractStatus.Available => availableColor,
                ContractStatus.Bidding => biddingColor,
                ContractStatus.Active => activeColor,
                ContractStatus.Completed => completedColor,
                ContractStatus.Failed => failedColor,
                ContractStatus.Expired => failedColor,
                _ => availableColor
            };
        }
        
        #endregion
        
        #region Animation
        
        public void OnPointerEnter()
        {
            transform.localScale = _originalScale * hoverScale;
        }
        
        public void OnPointerExit()
        {
            transform.localScale = _originalScale;
        }
        
        public void Pulse()
        {
            UIAnimator.Pulse(transform, 0.3f);
        }
        
        #endregion
    }
}
