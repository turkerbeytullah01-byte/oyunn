using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Compact risk indicator for use in cards and lists
    /// </summary>
    public class RiskIndicatorUI : MonoBehaviour
    {
        #region Fields
        
        [Header("Visual Components")]
        [SerializeField] private Image riskBar;
        [SerializeField] private Image riskBackground;
        [SerializeField] private TextMeshProUGUI riskLabel;
        
        [Header("Color Settings")]
        [SerializeField] private Color lowRiskColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color mediumRiskColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color highRiskColor = new Color(1f, 0.4f, 0.2f, 1f);
        [SerializeField] private Color criticalRiskColor = new Color(1f, 0.2f, 0.2f, 1f);
        
        [Header("Display Mode")]
        [SerializeField] private bool showPercentage = true;
        [SerializeField] private bool showLabel = true;
        [SerializeField] private bool useGradient = true;
        
        // Runtime state
        private float _currentRisk;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the risk percentage (0-100)
        /// </summary>
        public void SetRisk(float riskPercent)
        {
            _currentRisk = Mathf.Clamp(riskPercent, 0f, 100f);
            UpdateVisuals();
        }
        
        /// <summary>
        /// Sets the risk level directly
        /// </summary>
        public void SetRiskLevel(RiskLevel level)
        {
            float percent = level switch
            {
                RiskLevel.None => 0f,
                RiskLevel.Low => 15f,
                RiskLevel.Medium => 40f,
                RiskLevel.High => 65f,
                RiskLevel.Critical => 90f,
                _ => 0f
            };
            
            SetRisk(percent);
        }
        
        /// <summary>
        /// Sets the risk color directly without percentage
        /// </summary>
        public void SetRiskColor(Color color)
        {
            if (riskBar != null)
            {
                riskBar.color = color;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateVisuals()
        {
            // Update bar fill
            if (riskBar != null)
            {
                riskBar.fillAmount = _currentRisk / 100f;
                riskBar.color = GetRiskColor(_currentRisk);
            }
            
            // Update label
            if (riskLabel != null)
            {
                riskLabel.gameObject.SetActive(showLabel);
                
                if (showLabel)
                {
                    if (showPercentage)
                    {
                        riskLabel.text = $"{Mathf.RoundToInt(_currentRisk)}%";
                    }
                    else
                    {
                        riskLabel.text = GetRiskText(_currentRisk);
                    }
                    
                    riskLabel.color = GetRiskColor(_currentRisk);
                }
            }
        }
        
        private Color GetRiskColor(float percent)
        {
            if (useGradient)
            {
                return percent switch
                {
                    < 25f => Color.Lerp(lowRiskColor, mediumRiskColor, percent / 25f),
                    < 50f => Color.Lerp(mediumRiskColor, highRiskColor, (percent - 25f) / 25f),
                    < 75f => Color.Lerp(highRiskColor, criticalRiskColor, (percent - 50f) / 25f),
                    _ => criticalRiskColor
                };
            }
            
            return percent switch
            {
                < 25f => lowRiskColor,
                < 50f => mediumRiskColor,
                < 75f => highRiskColor,
                _ => criticalRiskColor
            };
        }
        
        private string GetRiskText(float percent)
        {
            return percent switch
            {
                < 25f => "Low",
                < 50f => "Medium",
                < 75f => "High",
                _ => "Critical"
            };
        }
        
        #endregion
    }
}
