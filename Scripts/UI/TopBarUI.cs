using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Persistent top bar displaying key game information
    /// Shows money, reputation, risk level, and provides quick menu access
    /// </summary>
    public class TopBarUI : MonoBehaviour
    {
        #region Fields
        
        [Header("Money Display")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private Image moneyIcon;
        [SerializeField] private Button moneyButton;
        [SerializeField] private GameObject moneyChangeIndicator;
        [SerializeField] private TextMeshProUGUI moneyChangeText;
        
        [Header("Reputation Display")]
        [SerializeField] private TextMeshProUGUI reputationText;
        [SerializeField] private Image reputationIcon;
        [SerializeField] private Button reputationButton;
        
        [Header("Risk Meter")]
        [SerializeField] private RiskMeterUI riskMeter;
        [SerializeField] private Button riskInfoButton;
        
        [Header("Time Display")]
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Sprite playSprite;
        [SerializeField] private Sprite pauseSprite;
        
        [Header("Menu")]
        [SerializeField] private Button menuButton;
        [SerializeField] private Button settingsButton;
        
        [Header("Notifications")]
        [SerializeField] private GameObject notificationBadge;
        [SerializeField] private TextMeshProUGUI notificationCount;
        
        [Header("Animation")]
        [SerializeField] private float numberUpdateDuration = 0.5f;
        [SerializeField] private float moneyChangeDisplayDuration = 2f;
        
        // Runtime state
        private float _displayedMoney;
        private float _targetMoney;
        private float _displayedReputation;
        private float _targetReputation;
        private bool _isMoneyAnimating;
        private bool _isPaused;
        
        #endregion
        
        #region Events
        
        public event Action OnMenuClicked;
        public event Action OnMoneyClicked;
        public event Action OnReputationClicked;
        public event Action OnPauseClicked;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            SetupButtons();
            InitializeDisplay();
        }
        
        private void Update()
        {
            UpdateNumberAnimations();
            UpdateTimeDisplay();
        }
        
        #endregion
        
        #region Initialization
        
        private void SetupButtons()
        {
            if (menuButton != null)
                menuButton.onClick.AddListener(() => OnMenuClicked?.Invoke());
            
            if (moneyButton != null)
                moneyButton.onClick.AddListener(() => OnMoneyClicked?.Invoke());
            
            if (reputationButton != null)
                reputationButton.onClick.AddListener(() => OnReputationClicked?.Invoke());
            
            if (pauseButton != null)
                pauseButton.onClick.AddListener(TogglePause);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenIds.SETTINGS));
            
            if (riskInfoButton != null)
                riskInfoButton.onClick.AddListener(ShowRiskInfo);
        }
        
        private void InitializeDisplay()
        {
            if (GameManager.Instance != null)
            {
                _displayedMoney = GameManager.Instance.Money;
                _targetMoney = _displayedMoney;
                UpdateMoneyText(_displayedMoney);
                
                _displayedReputation = GameManager.Instance.Reputation;
                _targetReputation = _displayedReputation;
                UpdateReputationText(_displayedReputation);
            }
            
            if (moneyChangeIndicator != null)
                moneyChangeIndicator.SetActive(false);
                
            if (notificationBadge != null)
                notificationBadge.SetActive(false);
        }
        
        #endregion
        
        #region Money Display
        
        /// <summary>
        /// Updates the money display with animation
        /// </summary>
        public void UpdateMoney(float amount)
        {
            float previousAmount = _targetMoney;
            _targetMoney = amount;
            
            // Show change indicator
            float change = amount - previousAmount;
            if (Mathf.Abs(change) > 0.01f)
            {
                ShowMoneyChange(change);
            }
            
            _isMoneyAnimating = true;
        }
        
        /// <summary>
        /// Instantly sets the money display without animation
        /// </summary>
        public void SetMoneyInstant(float amount)
        {
            _displayedMoney = amount;
            _targetMoney = amount;
            UpdateMoneyText(amount);
            _isMoneyAnimating = false;
        }
        
        private void UpdateMoneyText(float amount)
        {
            if (moneyText != null)
            {
                moneyText.text = FormatMoney(amount);
            }
        }
        
        private void ShowMoneyChange(float change)
        {
            if (moneyChangeIndicator == null || moneyChangeText == null)
                return;
            
            moneyChangeIndicator.SetActive(true);
            moneyChangeText.text = (change >= 0 ? "+" : "") + FormatMoney(change);
            moneyChangeText.color = change >= 0 ? UIManager.Instance.Theme.successColor : UIManager.Instance.Theme.errorColor;
            
            // Animate the indicator
            CancelInvoke(nameof(HideMoneyChange));
            Invoke(nameof(HideMoneyChange), moneyChangeDisplayDuration);
            
            // Pulse animation
            UIAnimator.Pulse(moneyChangeIndicator.transform, 0.3f);
        }
        
        private void HideMoneyChange()
        {
            if (moneyChangeIndicator != null)
                moneyChangeIndicator.SetActive(false);
        }
        
        private string FormatMoney(float amount)
        {
            if (amount >= 1000000000)
                return "$" + (amount / 1000000000f).ToString("F2") + "B";
            if (amount >= 1000000)
                return "$" + (amount / 1000000f).ToString("F2") + "M";
            if (amount >= 1000)
                return "$" + (amount / 1000f).ToString("F1") + "K";
            
            return "$" + amount.ToString("F0");
        }
        
        #endregion
        
        #region Reputation Display
        
        /// <summary>
        /// Updates the reputation display with animation
        /// </summary>
        public void UpdateReputation(float amount)
        {
            _targetReputation = Mathf.Clamp01(amount);
            
            // Note: Reputation updates instantly for now, can add animation if needed
            _displayedReputation = _targetReputation;
            UpdateReputationText(_displayedReputation);
        }
        
        /// <summary>
        /// Instantly sets the reputation display
        /// </summary>
        public void SetReputationInstant(float amount)
        {
            _displayedReputation = Mathf.Clamp01(amount);
            _targetReputation = _displayedReputation;
            UpdateReputationText(_displayedReputation);
        }
        
        private void UpdateReputationText(float amount)
        {
            if (reputationText != null)
            {
                reputationText.text = FormatReputation(amount);
            }
            
            // Update icon color based on reputation level
            if (reputationIcon != null)
            {
                reputationIcon.color = GetReputationColor(amount);
            }
        }
        
        private string FormatReputation(float amount)
        {
            int percent = Mathf.RoundToInt(amount * 100);
            return percent + "%";
        }
        
        private Color GetReputationColor(float amount)
        {
            if (amount >= 0.8f) return UIManager.Instance.Theme.successColor;
            if (amount >= 0.5f) return UIManager.Instance.Theme.primaryColor;
            if (amount >= 0.3f) return UIManager.Instance.Theme.warningColor;
            return UIManager.Instance.Theme.errorColor;
        }
        
        #endregion
        
        #region Risk Level
        
        /// <summary>
        /// Sets the risk level display
        /// </summary>
        public void SetRiskLevel(RiskLevel risk)
        {
            riskMeter?.SetRiskLevel(risk);
        }
        
        /// <summary>
        /// Sets the risk percentage (0-100)
        /// </summary>
        public void SetRiskPercent(float percent)
        {
            riskMeter?.SetRisk(percent);
        }
        
        private void ShowRiskInfo()
        {
            string title = "Risk Level";
            string message = "Your risk level increases when taking on dangerous contracts or conducting unethical research. " +
                           "High risk may trigger investigations, scandals, or government intervention.";
            
            UIManager.Instance?.ShowPopup(new PopupData
            {
                title = title,
                description = message,
                canDismiss = true
            });
        }
        
        #endregion
        
        #region Time Display
        
        private void UpdateTimeDisplay()
        {
            if (GameManager.Instance == null) return;
            
            if (dateText != null)
            {
                dateText.text = GameManager.Instance.CurrentDate.ToString("MMM dd, yyyy");
            }
            
            if (timeText != null)
            {
                timeText.text = GameManager.Instance.CurrentDate.ToString("HH:mm");
            }
        }
        
        private void TogglePause()
        {
            _isPaused = !_isPaused;
            
            if (pauseButton != null && pauseButton.image != null)
            {
                pauseButton.image.sprite = _isPaused ? playSprite : pauseSprite;
            }
            
            Time.timeScale = _isPaused ? 0f : 1f;
            OnPauseClicked?.Invoke();
        }
        
        /// <summary>
        /// Sets the pause state
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (_isPaused != paused)
            {
                TogglePause();
            }
        }
        
        #endregion
        
        #region Notifications
        
        /// <summary>
        /// Updates the notification badge count
        /// </summary>
        public void SetNotificationCount(int count)
        {
            if (notificationBadge == null || notificationCount == null)
                return;
            
            notificationBadge.SetActive(count > 0);
            notificationCount.text = count.ToString();
            
            // Animate badge appearance
            if (count > 0)
            {
                UIAnimator.Pulse(notificationBadge.transform, 0.3f);
            }
        }
        
        #endregion
        
        #region Animation
        
        private void UpdateNumberAnimations()
        {
            if (_isMoneyAnimating)
            {
                float diff = _targetMoney - _displayedMoney;
                
                if (Mathf.Abs(diff) < 0.01f)
                {
                    _displayedMoney = _targetMoney;
                    _isMoneyAnimating = false;
                }
                else
                {
                    _displayedMoney += diff * Time.deltaTime / numberUpdateDuration;
                }
                
                UpdateMoneyText(_displayedMoney);
            }
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Shows the top bar
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            UIAnimator.SlideIn(GetComponent<RectTransform>(), new Vector2(0, 100), 0.3f);
        }
        
        /// <summary>
        /// Hides the top bar
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Refreshes all displays
        /// </summary>
        public void Refresh()
        {
            if (GameManager.Instance != null)
            {
                SetMoneyInstant(GameManager.Instance.Money);
                SetReputationInstant(GameManager.Instance.Reputation);
            }
        }
        
        #endregion
    }
}
