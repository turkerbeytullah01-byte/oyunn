using UnityEngine;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// UI Theme ScriptableObject for Project Aegis: Drone Dominion
    /// Defines colors, fonts, and visual styling for the entire UI
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Project Aegis/UI Theme")]
    public class UITheme : ScriptableObject
    {
        #region Colors - Background
        
        [Header("Background Colors")]
        [Tooltip("Main background color (darkest)")]
        [ColorUsage(true, true)]
        public Color backgroundColor = new Color(0.102f, 0.102f, 0.18f, 1f); // #1a1a2e
        
        [Tooltip("Panel background color (slightly lighter)")]
        [ColorUsage(true, true)]
        public Color panelColor = new Color(0.16f, 0.16f, 0.25f, 1f); // #292940
        
        [Tooltip("Card/element background color")]
        [ColorUsage(true, true)]
        public Color cardColor = new Color(0.2f, 0.2f, 0.3f, 1f); // #33334d
        
        [Tooltip("Elevated/hover background color")]
        [ColorUsage(true, true)]
        public Color elevatedColor = new Color(0.25f, 0.25f, 0.35f, 1f); // #404059
        
        #endregion
        
        #region Colors - Accent
        
        [Header("Accent Colors")]
        [Tooltip("Primary accent color (cyan)")]
        [ColorUsage(true, true)]
        public Color primaryColor = new Color(0f, 0.83f, 1f, 1f); // #00d4ff
        
        [Tooltip("Secondary accent color (purple)")]
        [ColorUsage(true, true)]
        public Color secondaryColor = new Color(0.6f, 0.3f, 0.9f, 1f); // #994de6
        
        [Tooltip("Tertiary accent color (orange)")]
        [ColorUsage(true, true)]
        public Color tertiaryColor = new Color(1f, 0.6f, 0.2f, 1f); // #ff9933
        
        #endregion
        
        #region Colors - Status
        
        [Header("Status Colors")]
        [Tooltip("Success/green color")]
        [ColorUsage(true, true)]
        public Color successColor = new Color(0.2f, 0.9f, 0.4f, 1f); // #33e666
        
        [Tooltip("Warning/yellow color")]
        [ColorUsage(true, true)]
        public Color warningColor = new Color(1f, 0.8f, 0.2f, 1f); // #ffcc33
        
        [Tooltip("Error/red color")]
        [ColorUsage(true, true)]
        public Color errorColor = new Color(1f, 0.3f, 0.3f, 1f); // #ff4d4d
        
        [Tooltip("Info/blue color")]
        [ColorUsage(true, true)]
        public Color infoColor = new Color(0.3f, 0.7f, 1f, 1f); // #4db3ff
        
        #endregion
        
        #region Colors - Text
        
        [Header("Text Colors")]
        [Tooltip("Primary text color (white/light)")]
        [ColorUsage(true, true)]
        public Color textPrimary = new Color(1f, 1f, 1f, 1f); // #ffffff
        
        [Tooltip("Secondary text color (dimmed)")]
        [ColorUsage(true, true)]
        public Color textSecondary = new Color(0.7f, 0.7f, 0.75f, 1f); // #b3b3bf
        
        [Tooltip("Tertiary text color (very dimmed)")]
        [ColorUsage(true, true)]
        public Color textTertiary = new Color(0.5f, 0.5f, 0.55f, 1f); // #80808c
        
        [Tooltip("Disabled text color")]
        [ColorUsage(true, true)]
        public Color textDisabled = new Color(0.3f, 0.3f, 0.35f, 1f); // #4d4d59
        
        #endregion
        
        #region Colors - Interactive
        
        [Header("Interactive Colors")]
        [Tooltip("Button normal color")]
        [ColorUsage(true, true)]
        public Color buttonNormal = new Color(0.25f, 0.25f, 0.35f, 1f);
        
        [Tooltip("Button hover color")]
        [ColorUsage(true, true)]
        public Color buttonHover = new Color(0.35f, 0.35f, 0.45f, 1f);
        
        [Tooltip("Button pressed color")]
        [ColorUsage(true, true)]
        public Color buttonPressed = new Color(0.15f, 0.15f, 0.25f, 1f);
        
        [Tooltip("Button disabled color")]
        [ColorUsage(true, true)]
        public Color buttonDisabled = new Color(0.2f, 0.2f, 0.25f, 0.5f);
        
        [Tooltip("Toggle on color")]
        [ColorUsage(true, true)]
        public Color toggleOn = new Color(0f, 0.83f, 1f, 1f);
        
        [Tooltip("Toggle off color")]
        [ColorUsage(true, true)]
        public Color toggleOff = new Color(0.3f, 0.3f, 0.4f, 1f);
        
        #endregion
        
        #region Colors - Borders & Dividers
        
        [Header("Borders & Dividers")]
        [Tooltip("Border color")]
        [ColorUsage(true, true)]
        public Color borderColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);
        
        [Tooltip("Divider color")]
        [ColorUsage(true, true)]
        public Color dividerColor = new Color(0.25f, 0.25f, 0.35f, 0.3f);
        
        [Tooltip("Outline color")]
        [ColorUsage(true, true)]
        public Color outlineColor = new Color(0f, 0.83f, 1f, 0.5f);
        
        #endregion
        
        #region Fonts
        
        [Header("Fonts")]
        [Tooltip("Primary font for body text")]
        public TMP_FontAsset primaryFont;
        
        [Tooltip("Header font for titles")]
        public TMP_FontAsset headerFont;
        
        [Tooltip("Monospace font for numbers/data")]
        public TMP_FontAsset monoFont;
        
        #endregion
        
        #region Typography
        
        [Header("Typography")]
        [Tooltip("Base font size")]
        public int baseFontSize = 14;
        
        [Tooltip("Header font size")]
        public int headerFontSize = 24;
        
        [Tooltip("Title font size")]
        public int titleFontSize = 18;
        
        [Tooltip("Small font size")]
        public int smallFontSize = 12;
        
        [Tooltip("Line spacing multiplier")]
        public float lineSpacing = 1.2f;
        
        #endregion
        
        #region Spacing & Sizing
        
        [Header("Spacing & Sizing")]
        [Tooltip("Base spacing unit (8px grid)")]
        public int spacingUnit = 8;
        
        [Tooltip("Small padding")]
        public int paddingSmall = 8;
        
        [Tooltip("Medium padding")]
        public int paddingMedium = 16;
        
        [Tooltip("Large padding")]
        public int paddingLarge = 24;
        
        [Tooltip("Border radius")]
        public int borderRadius = 8;
        
        [Tooltip("Minimum touch target size")]
        public int minTouchSize = 44;
        
        #endregion
        
        #region Animation
        
        [Header("Animation")]
        [Tooltip("Default animation duration")]
        public float animationDuration = 0.3f;
        
        [Tooltip("Fast animation duration")]
        public float fastAnimationDuration = 0.15f;
        
        [Tooltip("Slow animation duration")]
        public float slowAnimationDuration = 0.5f;
        
        [Tooltip("Animation easing curve")]
        public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Gets a color by name (for dynamic theming)
        /// </summary>
        public Color GetColor(string colorName)
        {
            return colorName.ToLower() switch
            {
                "background" => backgroundColor,
                "panel" => panelColor,
                "card" => cardColor,
                "elevated" => elevatedColor,
                "primary" => primaryColor,
                "secondary" => secondaryColor,
                "tertiary" => tertiaryColor,
                "success" => successColor,
                "warning" => warningColor,
                "error" => errorColor,
                "info" => infoColor,
                "textprimary" => textPrimary,
                "textsecondary" => textSecondary,
                "texttertiary" => textTertiary,
                "textdisabled" => textDisabled,
                "border" => borderColor,
                "divider" => dividerColor,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// Gets a color with modified alpha
        /// </summary>
        public Color GetColorWithAlpha(string colorName, float alpha)
        {
            Color color = GetColor(colorName);
            color.a = alpha;
            return color;
        }
        
        /// <summary>
        /// Applies this theme to a Graphic component
        /// </summary>
        public void ApplyToGraphic(Graphic graphic, string colorName)
        {
            if (graphic != null)
            {
                graphic.color = GetColor(colorName);
            }
        }
        
        /// <summary>
        /// Applies this theme to a TextMeshPro component
        /// </summary>
        public void ApplyToText(TextMeshProUGUI text, string colorName, int fontSize = 0)
        {
            if (text == null) return;
            
            text.color = GetColor(colorName);
            
            if (fontSize > 0)
            {
                text.fontSize = fontSize;
            }
            
            if (primaryFont != null)
            {
                text.font = primaryFont;
            }
        }
        
        /// <summary>
        /// Gets spacing in pixels
        /// </summary>
        public int GetSpacing(int units)
        {
            return units * spacingUnit;
        }
        
        #endregion
    }
}
