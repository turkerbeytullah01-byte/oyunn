using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Detail panel for displaying research information and actions
    /// </summary>
    public class ResearchDetailPanel : MonoBehaviour
    {
        #region Fields
        
        [Header("Research Info")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI detailedDescriptionText;
        [SerializeField] private Image researchIcon;
        
        [Header("Requirements")]
        [SerializeField] private Transform prerequisitesContainer;
        [SerializeField] private GameObject prerequisiteItemPrefab;
        [SerializeField] private TextMeshProUGUI requiredPointsText;
        [SerializeField] private TextMeshProUGUI requiredTimeText;
        
        [Header("Unlocks")]
        [SerializeField] private Transform unlocksContainer;
        [SerializeField] private GameObject unlockItemPrefab;
        
        [Header("Status")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image statusBackground;
        
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button queueButton;
        [SerializeField] private Button closeButton;
        
        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private float animationDuration = 0.3f;
        
        // Runtime state
        private ResearchData _currentResearch;
        private NodeState _currentState;
        private bool _isVisible;
        
        #endregion
        
        #region Properties
        
        public bool IsVisible => _isVisible;
        public ResearchData CurrentResearch => _currentResearch;
        
        #endregion
        
        #region Events
        
        public event Action<ResearchData> OnStartResearch;
        public event Action<ResearchData> OnCancelResearch;
        public event Action<ResearchData> OnQueueResearch;
        
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
            if (startButton != null)
            {
                startButton.onClick.AddListener(() =>
                {
                    if (_currentResearch != null)
                        OnStartResearch?.Invoke(_currentResearch);
                });
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() =>
                {
                    if (_currentResearch != null)
                        OnCancelResearch?.Invoke(_currentResearch);
                });
            }
            
            if (queueButton != null)
            {
                queueButton.onClick.AddListener(() =>
                {
                    if (_currentResearch != null)
                        OnQueueResearch?.Invoke(_currentResearch);
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
        /// Shows the detail panel for a research
        /// </summary>
        public void Show(ResearchData research, NodeState state)
        {
            _currentResearch = research;
            _currentState = state;
            
            UpdateContent();
            UpdateButtons();
            
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
        
        /// <summary>
        /// Updates the progress display
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }
            
            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateContent()
        {
            if (_currentResearch == null) return;
            
            // Basic info
            if (nameText != null)
            {
                nameText.text = _currentResearch.name;
            }
            
            if (categoryText != null)
            {
                categoryText.text = _currentResearch.category.ToString();
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = _currentResearch.description;
            }
            
            if (detailedDescriptionText != null)
            {
                detailedDescriptionText.text = _currentResearch.detailedDescription;
            }
            
            if (researchIcon != null && _currentResearch.icon != null)
            {
                researchIcon.sprite = _currentResearch.icon;
            }
            
            // Requirements
            UpdatePrerequisites();
            
            if (requiredPointsText != null)
            {
                requiredPointsText.text = $"{_currentResearch.researchPoints} RP";
            }
            
            if (requiredTimeText != null)
            {
                requiredTimeText.text = FormatTime(_currentResearch.researchTime);
            }
            
            // Unlocks
            UpdateUnlocks();
            
            // Status
            UpdateStatus();
        }
        
        private void UpdatePrerequisites()
        {
            if (prerequisitesContainer == null || prerequisiteItemPrefab == null) return;
            
            // Clear existing
            foreach (Transform child in prerequisitesContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add prerequisites
            if (_currentResearch.prerequisites != null)
            {
                foreach (var prereqId in _currentResearch.prerequisites)
                {
                    var prereqData = ResearchManager.Instance?.GetResearch(prereqId);
                    if (prereqData != null)
                    {
                        var item = Instantiate(prerequisiteItemPrefab, prerequisitesContainer);
                        var text = item.GetComponentInChildren<TextMeshProUGUI>();
                        if (text != null)
                        {
                            bool isCompleted = ResearchManager.Instance.IsResearchCompleted(prereqId);
                            text.text = $"{(isCompleted ? "✓" : "○")} {prereqData.name}";
                            text.color = isCompleted ? UIManager.Instance.Theme.successColor : 
                                UIManager.Instance.Theme.textSecondary;
                        }
                    }
                }
            }
        }
        
        private void UpdateUnlocks()
        {
            if (unlocksContainer == null || unlockItemPrefab == null) return;
            
            // Clear existing
            foreach (Transform child in unlocksContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add unlocks
            if (_currentResearch.unlocks != null)
            {
                foreach (var unlock in _currentResearch.unlocks)
                {
                    var item = Instantiate(unlockItemPrefab, unlocksContainer);
                    var text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = $"► {unlock}";
                    }
                }
            }
        }
        
        private void UpdateStatus()
        {
            if (statusText != null)
            {
                statusText.text = _currentState switch
                {
                    NodeState.Locked => "Locked - Prerequisites Required",
                    NodeState.Available => "Available for Research",
                    NodeState.InProgress => "Research in Progress",
                    NodeState.Completed => "Research Completed",
                    _ => "Unknown"
                };
                
                statusText.color = _currentState switch
                {
                    NodeState.Locked => UIManager.Instance.Theme.textSecondary,
                    NodeState.Available => UIManager.Instance.Theme.primaryColor,
                    NodeState.InProgress => UIManager.Instance.Theme.warningColor,
                    NodeState.Completed => UIManager.Instance.Theme.successColor,
                    _ => UIManager.Instance.Theme.textPrimary
                };
            }
            
            if (statusBackground != null)
            {
                statusBackground.color = _currentState switch
                {
                    NodeState.Locked => new Color(0.3f, 0.3f, 0.3f, 0.5f),
                    NodeState.Available => new Color(0f, 0.83f, 1f, 0.2f),
                    NodeState.InProgress => new Color(1f, 0.8f, 0.2f, 0.2f),
                    NodeState.Completed => new Color(0.2f, 0.9f, 0.4f, 0.2f),
                    _ => Color.clear
                };
            }
            
            // Progress visibility
            bool showProgress = _currentState == NodeState.InProgress;
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(showProgress);
            }
            if (progressText != null)
            {
                progressText.gameObject.SetActive(showProgress);
            }
        }
        
        private void UpdateButtons()
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(_currentState == NodeState.Available);
            }
            
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(_currentState == NodeState.InProgress);
            }
            
            if (queueButton != null)
            {
                queueButton.gameObject.SetActive(_currentState == NodeState.Available || 
                    _currentState == NodeState.Locked);
            }
        }
        
        private string FormatTime(float seconds)
        {
            if (seconds < 60)
                return $"{Mathf.CeilToInt(seconds)} seconds";
            if (seconds < 3600)
                return $"{Mathf.CeilToInt(seconds / 60)} minutes";
            if (seconds < 86400)
                return $"{Mathf.CeilToInt(seconds / 3600)} hours";
            
            return $"{Mathf.CeilToInt(seconds / 86400)} days";
        }
        
        #endregion
    }
}
