using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Central popup system for displaying events, choices, and information
    /// </summary>
    public class PopupSystem : MonoBehaviour
    {
        #region Fields
        
        [Header("Main Panel")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private CanvasGroup popupCanvasGroup;
        [SerializeField] private RectTransform popupRect;
        
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI detailsText;
        [SerializeField] private Image eventIcon;
        [SerializeField] private Image backgroundImage;
        
        [Header("Timer")]
        [SerializeField] private GameObject timerPanel;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private TextMeshProUGUI timerText;
        
        [Header("Choices")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private Transform dismissButtonContainer;
        [SerializeField] private Button dismissButton;
        [SerializeField] private TextMeshProUGUI dismissButtonText;
        
        [Header("Type Styling")]
        [SerializeField] private Image headerBackground;
        [SerializeField] private List<PopupTypeStyle> typeStyles;
        
        [Header("Animation")]
        [SerializeField] private float showAnimationDuration = 0.3f;
        [SerializeField] private float hideAnimationDuration = 0.2f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip defaultShowSound;
        
        // Runtime state
        private PopupData _currentPopup;
        private List<Button> _activeChoiceButtons = new List<Button>();
        private float _timerRemaining;
        private bool _isTimerRunning;
        private bool _isShowing;
        
        #endregion
        
        #region Events
        
        public event Action<PopupData> OnPopupShown;
        public event Action<PopupData> OnPopupDismissed;
        public event Action<PopupData, int> OnChoiceSelected;
        
        #endregion
        
        #region Properties
        
        public bool IsShowing => _isShowing;
        public PopupData CurrentPopup => _currentPopup;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure popup is hidden initially
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
            
            if (timerPanel != null)
            {
                timerPanel.SetActive(false);
            }
        }
        
        private void Update()
        {
            UpdateTimer();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Shows an event popup with the given event data
        /// </summary>
        public void ShowEventPopup(GameEventData eventData)
        {
            var popupData = new PopupData
            {
                title = eventData.eventName,
                description = eventData.description,
                details = eventData.detailedDescription,
                icon = eventData.eventImage,
                choices = eventData.choices,
                canDismiss = false,
                type = PopupType.Event,
                showSound = eventData.eventSound
            };
            
            ShowPopup(popupData);
        }
        
        /// <summary>
        /// Shows a choice popup with multiple options
        /// </summary>
        public void ShowChoicePopup(string title, string description, List<PopupChoice> choices)
        {
            var popupData = new PopupData
            {
                title = title,
                description = description,
                choices = choices,
                canDismiss = true,
                type = PopupType.Confirm
            };
            
            ShowPopup(popupData);
        }
        
        /// <summary>
        /// Shows an info popup with a single dismiss option
        /// </summary>
        public void ShowInfoPopup(string title, string message)
        {
            var popupData = new PopupData
            {
                title = title,
                description = message,
                canDismiss = true,
                type = PopupType.Info
            };
            
            ShowPopup(popupData);
        }
        
        /// <summary>
        /// Shows a warning popup
        /// </summary>
        public void ShowWarningPopup(string title, string message, Action onAcknowledge = null)
        {
            var popupData = new PopupData
            {
                title = title,
                description = message,
                canDismiss = true,
                type = PopupType.Warning
            };
            
            if (onAcknowledge != null)
            {
                popupData.choices.Add(new PopupChoice("Acknowledge", onAcknowledge));
            }
            
            ShowPopup(popupData);
        }
        
        /// <summary>
        /// Shows an error popup
        /// </summary>
        public void ShowErrorPopup(string title, string message)
        {
            var popupData = new PopupData
            {
                title = title,
                description = message,
                canDismiss = true,
                type = PopupType.Error
            };
            
            ShowPopup(popupData);
        }
        
        /// <summary>
        /// Shows a confirmation popup
        /// </summary>
        public void ShowConfirmPopup(string title, string message, Action onConfirm, Action onCancel = null)
        {
            var popupData = new PopupData
            {
                title = title,
                description = message,
                canDismiss = true,
                type = PopupType.Confirm,
                choices = new List<PopupChoice>
                {
                    new PopupChoice("Confirm", onConfirm, Color.green),
                    new PopupChoice("Cancel", onCancel, Color.gray)
                }
            };
            
            ShowPopup(popupData);
        }
        
        /// <summary>
        /// Shows a popup with the given data
        /// </summary>
        public void ShowPopup(PopupData data)
        {
            if (_isShowing)
            {
                // Queue the popup or replace current
                HidePopup(() => ShowPopupInternal(data));
                return;
            }
            
            ShowPopupInternal(data);
        }
        
        /// <summary>
        /// Hides the current popup
        /// </summary>
        public void HidePopup(Action onComplete = null)
        {
            if (!_isShowing) 
            {
                onComplete?.Invoke();
                return;
            }
            
            _isShowing = false;
            _isTimerRunning = false;
            
            // Animate out
            if (popupCanvasGroup != null)
            {
                UIAnimator.FadeOut(popupCanvasGroup, hideAnimationDuration);
            }
            
            // Delay actual hide
            Invoke(nameof(CompleteHide), hideAnimationDuration);
            
            void CompleteHide()
            {
                popupPanel.SetActive(false);
                OnPopupDismissed?.Invoke(_currentPopup);
                onComplete?.Invoke();
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void ShowPopupInternal(PopupData data)
        {
            _currentPopup = data;
            _isShowing = true;
            
            // Setup content
            SetupContent(data);
            
            // Setup choices
            SetupChoices(data);
            
            // Setup timer
            SetupTimer(data);
            
            // Apply styling
            ApplyTypeStyle(data.type);
            
            // Show popup
            popupPanel.SetActive(true);
            
            // Animate in
            if (popupCanvasGroup != null)
            {
                UIAnimator.FadeIn(popupCanvasGroup, showAnimationDuration);
            }
            
            if (popupRect != null)
            {
                UIAnimator.ScaleIn(popupRect, showAnimationDuration);
            }
            
            // Play sound
            PlayShowSound(data);
            
            // Pause game if needed
            if (data.pauseGame)
            {
                Time.timeScale = 0f;
            }
            
            OnPopupShown?.Invoke(data);
        }
        
        private void SetupContent(PopupData data)
        {
            if (titleText != null)
            {
                titleText.text = data.title ?? "";
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = data.description ?? "";
            }
            
            if (detailsText != null)
            {
                bool hasDetails = !string.IsNullOrEmpty(data.details);
                detailsText.gameObject.SetActive(hasDetails);
                if (hasDetails)
                {
                    detailsText.text = data.details;
                }
            }
            
            if (eventIcon != null)
            {
                bool hasIcon = data.icon != null;
                eventIcon.gameObject.SetActive(hasIcon);
                if (hasIcon)
                {
                    eventIcon.sprite = data.icon;
                }
            }
            
            if (backgroundImage != null && data.background != null)
            {
                backgroundImage.sprite = data.background;
            }
        }
        
        private void SetupChoices(PopupData data)
        {
            // Clear existing choices
            ClearChoices();
            
            // Setup dismiss button
            if (dismissButtonContainer != null)
            {
                dismissButtonContainer.gameObject.SetActive(data.canDismiss);
            }
            
            if (dismissButton != null)
            {
                dismissButton.onClick.RemoveAllListeners();
                dismissButton.onClick.AddListener(() => HidePopup());
            }
            
            if (dismissButtonText != null && !string.IsNullOrEmpty(data.dismissText))
            {
                dismissButtonText.text = data.dismissText;
            }
            
            // Create choice buttons
            if (data.choices != null)
            {
                for (int i = 0; i < data.choices.Count; i++)
                {
                    CreateChoiceButton(data.choices[i], i);
                }
            }
        }
        
        private void CreateChoiceButton(PopupChoice choice, int index)
        {
            if (choiceButtonPrefab == null || choicesContainer == null) return;
            
            var button = Instantiate(choiceButtonPrefab, choicesContainer);
            
            // Setup button visuals
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choice.label;
            }
            
            // Apply color
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null && choice.buttonColor != Color.white)
            {
                buttonImage.color = choice.buttonColor;
            }
            
            // Setup icon
            if (choice.icon != null)
            {
                var iconImage = button.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = choice.icon;
                    iconImage.gameObject.SetActive(true);
                }
            }
            
            // Setup availability
            button.interactable = choice.isAvailable;
            
            // Setup click handler
            button.onClick.AddListener(() => OnChoiceClicked(index));
            
            _activeChoiceButtons.Add(button);
        }
        
        private void ClearChoices()
        {
            foreach (var button in _activeChoiceButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    Destroy(button.gameObject);
                }
            }
            _activeChoiceButtons.Clear();
        }
        
        private void SetupTimer(PopupData data)
        {
            _isTimerRunning = data.showTimer && data.timeLimit > 0;
            
            if (timerPanel != null)
            {
                timerPanel.SetActive(_isTimerRunning);
            }
            
            if (_isTimerRunning)
            {
                _timerRemaining = data.timeLimit;
                
                if (timerSlider != null)
                {
                    timerSlider.maxValue = data.timeLimit;
                    timerSlider.value = data.timeLimit;
                }
            }
        }
        
        private void UpdateTimer()
        {
            if (!_isTimerRunning || !_isShowing) return;
            
            _timerRemaining -= Time.unscaledDeltaTime;
            
            if (timerSlider != null)
            {
                timerSlider.value = _timerRemaining;
            }
            
            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(_timerRemaining).ToString();
            }
            
            if (_timerRemaining <= 0)
            {
                OnTimerExpired();
            }
        }
        
        private void OnTimerExpired()
        {
            _isTimerRunning = false;
            
            // Call timeout callback
            _currentPopup?.onTimeExpired?.Invoke();
            
            // Select default choice if specified
            if (_currentPopup?.defaultChoiceOnTimeout >= 0 && 
                _currentPopup.defaultChoiceOnTimeout < _currentPopup.choices.Count)
            {
                OnChoiceClicked(_currentPopup.defaultChoiceOnTimeout);
            }
            else
            {
                HidePopup();
            }
        }
        
        private void OnChoiceClicked(int index)
        {
            if (_currentPopup == null || index < 0 || index >= _currentPopup.choices.Count)
                return;
            
            var choice = _currentPopup.choices[index];
            
            // Invoke choice action
            choice.onSelected?.Invoke();
            
            // Fire event
            OnChoiceSelected?.Invoke(_currentPopup, index);
            
            // Hide popup
            HidePopup();
        }
        
        private void ApplyTypeStyle(PopupType type)
        {
            var style = typeStyles.Find(s => s.type == type);
            if (style == null) return;
            
            if (headerBackground != null)
            {
                headerBackground.color = style.headerColor;
            }
            
            if (titleText != null)
            {
                titleText.color = style.titleColor;
            }
        }
        
        private void PlayShowSound(PopupData data)
        {
            var sound = data.showSound ?? defaultShowSound;
            if (sound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(sound);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Popup type style configuration
    /// </summary>
    [Serializable]
    public class PopupTypeStyle
    {
        public PopupType type;
        public Color headerColor = Color.blue;
        public Color titleColor = Color.white;
        public Color backgroundColor = Color.black;
    }
}
