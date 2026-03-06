using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Detail panel for displaying contract information
    /// </summary>
    public class ContractDetailPanel : MonoBehaviour
    {
        #region Fields
        
        [Header("Contract Info")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI clientText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI longDescriptionText;
        
        [Header("Requirements")]
        [SerializeField] private Transform requirementsContainer;
        [SerializeField] private GameObject requirementItemPrefab;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private RiskIndicatorUI riskIndicator;
        
        [Header("Rewards")]
        [SerializeField] private TextMeshProUGUI baseRewardText;
        [SerializeField] private TextMeshProUGUI bonusRewardText;
        [SerializeField] private TextMeshProUGUI reputationText;
        [SerializeField] private TextMeshProUGUI penaltyText;
        
        [Header("Timeline")]
        [SerializeField] private TextMeshProUGUI deadlineText;
        [SerializeField] private TextMeshProUGUI estimatedDurationText;
        
        [Header("Status")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Buttons")]
        [SerializeField] private Button bidButton;
        [SerializeField] private Button closeButton;
        
        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private float animationDuration = 0.3f;
        
        // Runtime state
        private ContractData _currentContract;
        private bool _isVisible;
        
        #endregion
        
        #region Properties
        
        public bool IsVisible => _isVisible;
        public ContractData CurrentContract => _currentContract;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
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
        
        private void SetupButtons()
        {
            if (bidButton != null)
            {
                bidButton.onClick.AddListener(() =>
                {
                    Hide();
                    // Notify parent to show bid panel
                });
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Shows the detail panel for a contract
        /// </summary>
        public void Show(ContractData contract)
        {
            _currentContract = contract;
            
            UpdateContent();
            
            _isVisible = true;
            
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                UIAnimator.FadeIn(canvasGroup, animationDuration);
            }
            
            if (panelRect != null)
            {
                UIAnimator.SlideIn(panelRect, new Vector2(500, 0), animationDuration);
            }
        }
        
        /// <summary>
        /// Hides the detail panel
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
        
        #region Private Methods
        
        private void UpdateContent()
        {
            if (_currentContract == null) return;
            
            // Basic info
            if (nameText != null)
            {
                nameText.text = _currentContract.name;
            }
            
            if (clientText != null)
            {
                clientText.text = $"Client: {_currentContract.clientName}";
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = _currentContract.description;
            }
            
            if (longDescriptionText != null)
            {
                longDescriptionText.text = _currentContract.longDescription;
            }
            
            // Requirements
            UpdateRequirements();
            
            if (difficultyText != null)
            {
                difficultyText.text = $"Difficulty: {_currentContract.difficulty}/10";
                difficultyText.color = GetDifficultyColor(_currentContract.difficulty);
            }
            
            if (riskIndicator != null)
            {
                riskIndicator.SetRisk(_currentContract.riskLevel);
            }
            
            // Rewards
            if (baseRewardText != null)
            {
                baseRewardText.text = $"${_currentContract.reward:N0}";
            }
            
            if (bonusRewardText != null)
            {
                bonusRewardText.text = $"Bonus: ${_currentContract.bonusReward:N0}";
                bonusRewardText.gameObject.SetActive(_currentContract.bonusReward > 0);
            }
            
            if (reputationText != null)
            {
                string sign = _currentContract.reputationGain >= 0 ? "+" : "";
                reputationText.text = $"Reputation: {sign}{_currentContract.reputationGain:F1}%";
                reputationText.color = _currentContract.reputationGain >= 0 ? 
                    UIManager.Instance.Theme.successColor : UIManager.Instance.Theme.errorColor;
            }
            
            if (penaltyText != null)
            {
                penaltyText.text = $"Failure Penalty: ${_currentContract.penalty:N0}";
                penaltyText.gameObject.SetActive(_currentContract.penalty > 0);
            }
            
            // Timeline
            if (deadlineText != null)
            {
                int days = Mathf.CeilToInt(_currentContract.deadline / 24f);
                deadlineText.text = $"Deadline: {days} days";
            }
            
            // Status
            UpdateStatus();
            
            // Update bid button
            if (bidButton != null)
            {
                bidButton.gameObject.SetActive(_currentContract.status == ContractStatus.Available);
            }
        }
        
        private void UpdateRequirements()
        {
            if (requirementsContainer == null || requirementItemPrefab == null) return;
            
            // Clear existing
            foreach (Transform child in requirementsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add required technologies
            if (_currentContract.requiredTechnologies != null)
            {
                foreach (var tech in _currentContract.requiredTechnologies)
                {
                    var item = Instantiate(requirementItemPrefab, requirementsContainer);
                    var text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = $"• {tech}";
                    }
                }
            }
        }
        
        private void UpdateStatus()
        {
            if (statusText != null)
            {
                statusText.text = _currentContract.status switch
                {
                    ContractStatus.Available => "Available for Bidding",
                    ContractStatus.Bidding => "Bid Submitted",
                    ContractStatus.Active => "In Progress",
                    ContractStatus.Completed => "Completed",
                    ContractStatus.Failed => "Failed",
                    _ => "Unknown"
                };
                
                statusText.color = _currentContract.status switch
                {
                    ContractStatus.Available => UIManager.Instance.Theme.primaryColor,
                    ContractStatus.Bidding => UIManager.Instance.Theme.warningColor,
                    ContractStatus.Active => UIManager.Instance.Theme.infoColor,
                    ContractStatus.Completed => UIManager.Instance.Theme.successColor,
                    ContractStatus.Failed => UIManager.Instance.Theme.errorColor,
                    _ => UIManager.Instance.Theme.textSecondary
                };
            }
            
            // Progress
            bool showProgress = _currentContract.status == ContractStatus.Active;
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(showProgress);
                progressSlider.value = _currentContract.progress;
            }
            if (progressText != null)
            {
                progressText.gameObject.SetActive(showProgress);
                progressText.text = $"{Mathf.RoundToInt(_currentContract.progress * 100)}%";
            }
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
        
        #endregion
    }
}
