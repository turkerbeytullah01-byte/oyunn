using UnityEngine;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Abstract base class for all UI screens in Project Aegis
    /// Provides lifecycle methods and common functionality
    /// </summary>
    public abstract class BaseScreen : MonoBehaviour
    {
        #region Fields
        
        [Header("Screen Configuration")]
        [Tooltip("Unique identifier for this screen")]
        public string screenId;
        
        [Tooltip("If true, this screen appears over other screens without hiding them")]
        public bool isModal = false;
        
        [Tooltip("If true, screen remains in memory when hidden")]
        public bool cacheOnHide = true;
        
        [Header("Animation")]
        [SerializeField] protected bool animateOnShow = true;
        [SerializeField] protected bool animateOnHide = true;
        [SerializeField] protected float animationDuration = 0.3f;
        
        [Header("Audio")]
        [SerializeField] protected AudioClip showSound;
        [SerializeField] protected AudioClip hideSound;
        
        // Runtime state
        protected bool _isVisible;
        protected bool _isInitialized;
        
        #endregion
        
        #region Properties
        
        public bool IsVisible => _isVisible;
        public bool IsInitialized => _isInitialized;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }
        
        protected virtual void OnEnable()
        {
            // Subscribe to game events
        }
        
        protected virtual void OnDisable()
        {
            // Unsubscribe from game events
        }
        
        #endregion
        
        #region Abstract Methods
        
        /// <summary>
        /// Called when the screen is first initialized
        /// Use for one-time setup (finding components, etc.)
        /// </summary>
        protected abstract void Initialize();
        
        /// <summary>
        /// Called when the screen is shown
        /// Use for setting up data and refreshing content
        /// </summary>
        public abstract void OnShow();
        
        /// <summary>
        /// Called when the screen is hidden
        /// Use for cleanup and saving state
        /// </summary>
        public abstract void OnHide();
        
        /// <summary>
        /// Called when the screen needs to refresh its data
        /// Use for updating displayed information
        /// </summary>
        public abstract void OnRefresh();
        
        #endregion
        
        #region Virtual Methods
        
        /// <summary>
        /// Called when the screen gains focus (becomes the active screen)
        /// </summary>
        public virtual void OnFocusGained()
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Called when the screen loses focus (another screen becomes active)
        /// </summary>
        public virtual void OnFocusLost()
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Called when the back button is pressed (Android)
        /// Return true to consume the event, false to allow default behavior
        /// </summary>
        public virtual bool OnBackPressed()
        {
            return false; // Default: don't consume
        }
        
        /// <summary>
        /// Called when the screen is about to be destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Cleanup
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Plays the show sound effect
        /// </summary>
        protected void PlayShowSound()
        {
            if (showSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(showSound);
            }
        }
        
        /// <summary>
        /// Plays the hide sound effect
        /// </summary>
        protected void PlayHideSound()
        {
            if (hideSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(hideSound);
            }
        }
        
        /// <summary>
        /// Animates the screen in
        /// </summary>
        protected virtual void AnimateIn()
        {
            if (!animateOnShow) return;
            
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                UIAnimator.FadeIn(canvasGroup, animationDuration);
            }
            else
            {
                UIAnimator.ScaleIn(transform, animationDuration);
            }
        }
        
        /// <summary>
        /// Animates the screen out
        /// </summary>
        protected virtual void AnimateOut(System.Action onComplete)
        {
            if (!animateOnHide)
            {
                onComplete?.Invoke();
                return;
            }
            
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                UIAnimator.FadeOut(canvasGroup, animationDuration);
                Invoke(nameof(CompleteAnimation), animationDuration);
            }
            else
            {
                onComplete?.Invoke();
            }
            
            void CompleteAnimation() => onComplete?.Invoke();
        }
        
        /// <summary>
        /// Shows a loading indicator on this screen
        /// </summary>
        protected void ShowLoading(string message = "")
        {
            UIManager.Instance?.SetLoading(true, message);
        }
        
        /// <summary>
        /// Hides the loading indicator
        /// </summary>
        protected void HideLoading()
        {
            UIManager.Instance?.SetLoading(false);
        }
        
        /// <summary>
        /// Shows a notification
        /// </summary>
        protected void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            UIManager.Instance?.ShowNotification(message, type);
        }
        
        #endregion
    }
}
