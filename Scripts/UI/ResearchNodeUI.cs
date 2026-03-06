using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Research node state enumeration
    /// </summary>
    public enum NodeState
    {
        Locked,         // Prerequisites not met
        Available,      // Can be researched
        InProgress,     // Currently researching
        Completed       // Research finished
    }
    
    /// <summary>
    /// Individual research node in the tech tree
    /// </summary>
    public class ResearchNodeUI : MonoBehaviour
    {
        #region Fields
        
        [Header("Visual Components")]
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private Image glowEffect;
        
        [Header("State Icons")]
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject completeIcon;
        [SerializeField] private GameObject inProgressIcon;
        
        [Header("Progress")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image progressFill;
        
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        
        [Header("Colors")]
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color availableColor = new Color(0.2f, 0.6f, 0.8f, 1f);
        [SerializeField] private Color inProgressColor = new Color(0.9f, 0.7f, 0.2f, 1f);
        [SerializeField] private Color completedColor = new Color(0.2f, 0.8f, 0.4f, 1f);
        [SerializeField] private Color highlightedColor = new Color(0f, 0.8f, 1f, 1f);
        
        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 1.05f;
        [SerializeField] private ParticleSystem completionParticles;
        
        [Header("Touch")]
        [SerializeField] private Button button;
        
        // Runtime state
        private ResearchData _data;
        private NodeState _currentState;
        private bool _isHighlighted;
        private float _currentProgress;
        private Vector3 _originalScale;
        
        #endregion
        
        #region Properties
        
        public ResearchData Data => _data;
        public NodeState CurrentState => _currentState;
        public bool IsHighlighted => _isHighlighted;
        
        #endregion
        
        #region Events
        
        public event Action<ResearchNodeUI> OnClicked;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _originalScale = transform.localScale;
            
            if (button != null)
            {
                button.onClick.AddListener(() => OnClicked?.Invoke(this));
            }
            
            // Initialize state
            SetState(NodeState.Locked);
            SetProgress(0f);
        }
        
        private void Update()
        {
            UpdatePulseEffect();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the research data for this node
        /// </summary>
        public void SetData(ResearchData data)
        {
            _data = data;
            
            // Update visuals
            if (nameText != null)
            {
                nameText.text = data.name;
            }
            
            if (icon != null && data.icon != null)
            {
                icon.sprite = data.icon;
            }
            
            if (costText != null)
            {
                costText.text = $"{data.researchPoints} RP";
            }
            
            gameObject.name = $"Node_{data.id}";
        }
        
        /// <summary>
        /// Sets the node state with visual updates
        /// </summary>
        public void SetState(NodeState state)
        {
            _currentState = state;
            
            // Update colors
            Color stateColor = GetStateColor(state);
            if (background != null)
            {
                background.color = stateColor;
            }
            
            if (border != null)
            {
                border.color = state == NodeState.Available ? highlightedColor : stateColor;
            }
            
            // Update state icons
            UpdateStateIcons();
            
            // Update progress visibility
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(state == NodeState.InProgress);
            }
            
            if (timeText != null)
            {
                timeText.gameObject.SetActive(state == NodeState.InProgress);
            }
            
            // Update interactability
            if (button != null)
            {
                button.interactable = state != NodeState.Locked;
            }
            
            // Update glow
            if (glowEffect != null)
            {
                glowEffect.gameObject.SetActive(state == NodeState.Available || state == NodeState.InProgress);
            }
        }
        
        /// <summary>
        /// Updates the progress bar
        /// </summary>
        public void UpdateProgress(float progress)
        {
            _currentProgress = Mathf.Clamp01(progress);
            
            if (progressSlider != null)
            {
                progressSlider.value = _currentProgress;
            }
            
            if (timeText != null && _data != null)
            {
                float remainingTime = _data.researchTime * (1f - _currentProgress);
                timeText.text = FormatTime(remainingTime);
            }
        }
        
        /// <summary>
        /// Sets whether this node is highlighted
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            _isHighlighted = highlighted;
            
            if (border != null)
            {
                border.color = highlighted ? highlightedColor : GetStateColor(_currentState);
                border.transform.localScale = highlighted ? Vector3.one * 1.1f : Vector3.one;
            }
        }
        
        /// <summary>
        /// Plays the completion effect
        /// </summary>
        public void PlayCompletionEffect()
        {
            if (completionParticles != null)
            {
                completionParticles.Play();
            }
            
            // Scale animation
            UIAnimator.ScaleIn(transform, 0.3f);
            
            // Flash effect
            if (background != null)
            {
                StartCoroutine(FlashEffect());
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateStateIcons()
        {
            if (lockIcon != null)
                lockIcon.SetActive(_currentState == NodeState.Locked);
            
            if (completeIcon != null)
                completeIcon.SetActive(_currentState == NodeState.Completed);
            
            if (inProgressIcon != null)
                inProgressIcon.SetActive(_currentState == NodeState.InProgress);
        }
        
        private Color GetStateColor(NodeState state)
        {
            return state switch
            {
                NodeState.Locked => lockedColor,
                NodeState.Available => availableColor,
                NodeState.InProgress => inProgressColor,
                NodeState.Completed => completedColor,
                _ => lockedColor
            };
        }
        
        private void UpdatePulseEffect()
        {
            if (_currentState == NodeState.InProgress || _currentState == NodeState.Available)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1f) * 0.5f;
                transform.localScale = _originalScale * pulse;
            }
            else
            {
                transform.localScale = _originalScale;
            }
        }
        
        private string FormatTime(float seconds)
        {
            if (seconds < 60)
                return $"{Mathf.CeilToInt(seconds)}s";
            
            if (seconds < 3600)
                return $"{Mathf.CeilToInt(seconds / 60)}m";
            
            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            return $"{hours}h {minutes}m";
        }
        
        private System.Collections.IEnumerator FlashEffect()
        {
            Color originalColor = background.color;
            background.color = Color.white;
            
            yield return new WaitForSeconds(0.1f);
            
            background.color = originalColor;
        }
        
        #endregion
    }
}
