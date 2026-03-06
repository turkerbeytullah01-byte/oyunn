using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegis.Mobile
{
    /// <summary>
    /// Handles safe area for notched devices and rounded corners
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        [Header("Safe Area Settings")]
        [SerializeField] private bool applySafeArea = true;
        [SerializeField] private bool ignoreTopSafeArea = false;
        [SerializeField] private bool ignoreBottomSafeArea = false;
        [SerializeField] private Vector2 padding = Vector2.zero;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugOverlay = false;
        [SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
        
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private ScreenOrientation lastOrientation;
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        void Start()
        {
            ApplySafeArea();
        }
        
        void Update()
        {
            // Check for changes
            if (Screen.safeArea != lastSafeArea || Screen.orientation != lastOrientation)
            {
                ApplySafeArea();
            }
        }
        
        void OnRectTransformDimensionsChange()
        {
            ApplySafeArea();
        }
        
        private void ApplySafeArea()
        {
            if (!applySafeArea)
            {
                ResetToFullScreen();
                return;
            }
            
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastOrientation = Screen.orientation;
            
            // Convert safe area to anchor coordinates
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            
            // Apply padding
            anchorMin += padding / new Vector2(Screen.width, Screen.height);
            anchorMax -= padding / new Vector2(Screen.width, Screen.height);
            
            // Ignore specific edges
            if (ignoreTopSafeArea)
            {
                anchorMax.y = 1f;
            }
            if (ignoreBottomSafeArea)
            {
                anchorMin.y = 0f;
            }
            
            // Apply to rect transform
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            
            Debug.Log($"[SafeArea] Applied: {anchorMin} to {anchorMax}");
        }
        
        private void ResetToFullScreen()
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
        }
        
        void OnGUI()
        {
            if (showDebugOverlay && applySafeArea)
            {
                DrawDebugOverlay();
            }
        }
        
        private void DrawDebugOverlay()
        {
            GUI.color = debugColor;
            
            // Draw safe area borders
            Rect safeArea = Screen.safeArea;
            
            // Top bar
            GUI.DrawTexture(new Rect(0, 0, Screen.width, safeArea.y), Texture2D.whiteTexture);
            
            // Bottom bar
            GUI.DrawTexture(new Rect(0, safeArea.yMax, Screen.width, Screen.height - safeArea.yMax), Texture2D.whiteTexture);
            
            // Left bar
            GUI.DrawTexture(new Rect(0, safeArea.y, safeArea.x, safeArea.height), Texture2D.whiteTexture);
            
            // Right bar
            GUI.DrawTexture(new Rect(safeArea.xMax, safeArea.y, Screen.width - safeArea.xMax, safeArea.height), Texture2D.whiteTexture);
            
            GUI.color = Color.white;
        }
        
        /// <summary>
        /// Get the current safe area in screen coordinates
        /// </summary>
        public static Rect GetSafeArea()
        {
            return Screen.safeArea;
        }
        
        /// <summary>
        /// Check if device has a notch
        /// </summary>
        public static bool HasNotch()
        {
            return Screen.safeArea.y > 0 || Screen.safeArea.yMax < Screen.height;
        }
        
        /// <summary>
        /// Get notch height (if any)
        /// </summary>
        public static float GetNotchHeight()
        {
            return Screen.safeArea.y;
        }
    }
}
