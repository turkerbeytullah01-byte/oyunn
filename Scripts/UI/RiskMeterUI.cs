using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Risk level enumeration
    /// </summary>
    public enum RiskLevel
    {
        None,       // 0%
        Low,        // 1-25%
        Medium,     // 26-50%
        High,       // 51-75%
        Critical    // 76-100%
    }
    
    /// <summary>
    /// Visual risk indicator with color-coded display
    /// Shows current risk level with animated feedback
    /// </summary>
    public class RiskMeterUI : MonoBehaviour
    {
        #region Fields
        
        [Header("Visual Components")]
        [SerializeField] private Image riskFill;
        [SerializeField] private Image riskBackground;
        [SerializeField] private TextMeshProUGUI riskText;
        [SerializeField] private TextMeshProUGUI riskPercentText;
        
        [Header("Color Settings")]
        [SerializeField] private Color noneRiskColor = new Color(0.2f, 0.8f, 0.2f, 1f);      // Green
        [SerializeField] private Color lowRiskColor = new Color(0.4f, 0.9f, 0.4f, 1f);       // Light Green
        [SerializeField] private Color mediumRiskColor = new Color(1f, 0.8f, 0.2f, 1f);      // Yellow
        [SerializeField] private Color highRiskColor = new Color(1f, 0.5f, 0.2f, 1f);        // Orange
        [SerializeField] private Color criticalRiskColor = new Color(1f, 0.2f, 0.2f, 1f);    // Red
        
        [Header("Animation")]
        [SerializeField] private float fillUpdateSpeed = 5f;
        [SerializeField] private float pulseThreshold = 75f;
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float pulseScale = 1.1f;
        
        [Header("Warning Effects")]
        [SerializeField] private GameObject warningIcon;
        [SerializeField] private GameObject criticalEffect;
        [SerializeField] private ParticleSystem riskParticles;
        
        // Runtime state
        private float _currentRiskPercent;
        private float _targetRiskPercent;
        private RiskLevel _currentRiskLevel;
        private bool _isPulsing;
        private Vector3 _originalScale;
        
        #endregion
        
        #region Properties
        
        public float CurrentRisk => _currentRiskPercent;
        public RiskLevel CurrentLevel => _currentRiskLevel;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _originalScale = transform.localScale;
            
            // Initialize with zero risk
            _currentRiskPercent = 0f;
            _targetRiskPercent = 0f;
            
            UpdateVisuals();
        }
        
        private void Update()
        {
            UpdateFillAnimation();
            UpdatePulseEffect();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the risk percentage (0-100)
        /// </summary>
        public void SetRisk(float riskPercent)
        {
            _targetRiskPercent = Mathf.Clamp(riskPercent, 0f, 100f);
            UpdateRiskLevel();
        }
        
        /// <summary>
        /// Sets the risk level directly
        /// </summary>
        public void SetRiskLevel(RiskLevel level)
        {
            _currentRiskLevel = level;
            _targetRiskPercent = level switch
            {
                RiskLevel.None => 0f,
                RiskLevel.Low => 15f,
                RiskLevel.Medium => 40f,
                RiskLevel.High => 65f,
                RiskLevel.Critical => 90f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Instantly sets the risk without animation
        /// </summary>
        public void SetRiskInstant(float riskPercent)
        {
            _currentRiskPercent = Mathf.Clamp(riskPercent, 0f, 100f);
            _targetRiskPercent = _currentRiskPercent;
            UpdateRiskLevel();
            UpdateVisuals();
        }
        
        /// <summary>
        /// Adds risk amount
        /// </summary>
        public void AddRisk(float amount)
        {
            SetRisk(_targetRiskPercent + amount);
        }
        
        /// <summary>
        /// Reduces risk amount
        /// </summary>
        public void ReduceRisk(float amount)
        {
            SetRisk(_targetRiskPercent - amount);
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateRiskLevel()
        {
            RiskLevel newLevel = GetRiskLevel(_targetRiskPercent);
            
            if (newLevel != _currentRiskLevel)
            {
                _currentRiskLevel = newLevel;
                OnRiskLevelChanged();
            }
        }
        
        private RiskLevel GetRiskLevel(float percent)
        {
            return percent switch
            {
                <= 0f => RiskLevel.None,
                <= 25f => RiskLevel.Low,
                <= 50f => RiskLevel.Medium,
                <= 75f => RiskLevel.High,
                _ => RiskLevel.Critical
            };
        }
        
        private void OnRiskLevelChanged()
        {
            // Trigger effects based on new level
            switch (_currentRiskLevel)
            {
                case RiskLevel.Critical:
                    EnableCriticalEffects();
                    break;
                case RiskLevel.High:
                    EnableWarningEffects();
                    break;
                default:
                    DisableEffects();
                    break;
            }
            
            // Notify listeners
            // EventManager.Instance?.TriggerEvent(EventType.RiskLevelChanged, _currentRiskLevel);
        }
        
        private void UpdateFillAnimation()
        {
            if (Mathf.Abs(_targetRiskPercent - _currentRiskPercent) > 0.01f)
            {
                _currentRiskPercent = Mathf.Lerp(_currentRiskPercent, _targetRiskPercent, 
                    Time.deltaTime * fillUpdateSpeed);
                UpdateVisuals();
            }
        }
        
        private void UpdateVisuals()
        {
            // Update fill amount
            if (riskFill != null)
            {
                riskFill.fillAmount = _currentRiskPercent / 100f;
            }
            
            // Update colors
            Color riskColor = GetColorForRisk(_currentRiskPercent);
            if (riskFill != null)
            {
                riskFill.color = riskColor;
            }
            
            // Update text
            if (riskText != null)
            {
                riskText.text = GetRiskText(_currentRiskLevel);
                riskText.color = riskColor;
            }
            
            // Update percentage text
            if (riskPercentText != null)
            {
                riskPercentText.text = Mathf.RoundToInt(_currentRiskPercent) + "%";
                riskPercentText.color = riskColor;
            }
        }
        
        private void UpdatePulseEffect()
        {
            bool shouldPulse = _currentRiskPercent >= pulseThreshold;
            
            if (shouldPulse && !_isPulsing)
            {
                _isPulsing = true;
            }
            else if (!shouldPulse && _isPulsing)
            {
                _isPulsing = false;
                transform.localScale = _originalScale;
            }
            
            if (_isPulsing)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) * 
                    (pulseScale - 1f) * (_currentRiskPercent / 100f);
                transform.localScale = _originalScale * pulse;
            }
        }
        
        private Color GetColorForRisk(float percent)
        {
            return percent switch
            {
                <= 0f => noneRiskColor,
                <= 25f => Color.Lerp(noneRiskColor, lowRiskColor, percent / 25f),
                <= 50f => Color.Lerp(lowRiskColor, mediumRiskColor, (percent - 25f) / 25f),
                <= 75f => Color.Lerp(mediumRiskColor, highRiskColor, (percent - 50f) / 25f),
                _ => Color.Lerp(highRiskColor, criticalRiskColor, (percent - 75f) / 25f)
            };
        }
        
        private string GetRiskText(RiskLevel level)
        {
            return level switch
            {
                RiskLevel.None => "SAFE",
                RiskLevel.Low => "LOW RISK",
                RiskLevel.Medium => "MODERATE",
                RiskLevel.High => "HIGH RISK",
                RiskLevel.Critical => "CRITICAL",
                _ => "UNKNOWN"
            };
        }
        
        #endregion
        
        #region Effects
        
        private void EnableWarningEffects()
        {
            if (warningIcon != null)
                warningIcon.SetActive(true);
            
            if (riskParticles != null && !riskParticles.isPlaying)
                riskParticles.Play();
        }
        
        private void EnableCriticalEffects()
        {
            EnableWarningEffects();
            
            if (criticalEffect != null)
                criticalEffect.SetActive(true);
            
            // Play warning sound
            // AudioManager.Instance?.PlaySFX("RiskWarning");
        }
        
        private void DisableEffects()
        {
            if (warningIcon != null)
                warningIcon.SetActive(false);
            
            if (criticalEffect != null)
                criticalEffect.SetActive(false);
            
            if (riskParticles != null && riskParticles.isPlaying)
                riskParticles.Stop();
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Gets a color for a specific risk level
        /// </summary>
        public static Color GetLevelColor(RiskLevel level)
        {
            return level switch
            {
                RiskLevel.None => new Color(0.2f, 0.8f, 0.2f, 1f),
                RiskLevel.Low => new Color(0.4f, 0.9f, 0.4f, 1f),
                RiskLevel.Medium => new Color(1f, 0.8f, 0.2f, 1f),
                RiskLevel.High => new Color(1f, 0.5f, 0.2f, 1f),
                RiskLevel.Critical => new Color(1f, 0.2f, 0.2f, 1f),
                _ => Color.gray
            };
        }
        
        /// <summary>
        /// Flash the risk meter (for attention)
        /// </summary>
        public void Flash()
        {
            if (riskFill != null)
            {
                var originalColor = riskFill.color;
                riskFill.color = Color.white;
                
                // Simple flash effect - could use DOTween for smoother effect
                Invoke(nameof(RestoreColor), 0.1f);
                
                void RestoreColor()
                {
                    if (riskFill != null)
                        riskFill.color = originalColor;
                }
            }
        }
        
        #endregion
    }
}
