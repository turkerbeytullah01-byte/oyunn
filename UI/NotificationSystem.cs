using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Notification type enumeration
    /// </summary>
    public enum NotificationType
    {
        Info,       // General information
        Success,    // Positive outcome
        Warning,    // Caution needed
        Error,      // Something went wrong
        Research,   // Research-related
        Contract,   // Contract-related
        Event,      // Game event
        Achievement // Unlocked achievement
    }
    
    /// <summary>
    /// Toast notification system for displaying temporary messages
    /// </summary>
    public class NotificationSystem : MonoBehaviour
    {
        #region Fields
        
        [Header("Container")]
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private RectTransform containerRect;
        
        [Header("Notification Prefab")]
        [SerializeField] private NotificationItem notificationPrefab;
        
        [Header("Settings")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float slideInDistance = 100f;
        [SerializeField] private float spacing = 10f;
        [SerializeField] private int maxNotifications = 5;
        
        [Header("Type Icons")]
        [SerializeField] private Sprite infoIcon;
        [SerializeField] private Sprite successIcon;
        [SerializeField] private Sprite warningIcon;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite researchIcon;
        [SerializeField] private Sprite contractIcon;
        [SerializeField] private Sprite eventIcon;
        [SerializeField] private Sprite achievementIcon;
        
        [Header("Type Colors")]
        [SerializeField] private Color infoColor = new Color(0.3f, 0.7f, 1f, 1f);
        [SerializeField] private Color successColor = new Color(0.3f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color researchColor = new Color(0.6f, 0.3f, 0.9f, 1f);
        [SerializeField] private Color contractColor = new Color(0.9f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color eventColor = new Color(0.9f, 0.4f, 0.7f, 1f);
        [SerializeField] private Color achievementColor = new Color(1f, 0.8f, 0.3f, 1f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip notificationSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip achievementSound;
        
        // Runtime state
        private Queue<NotificationItem> _notificationPool = new Queue<NotificationItem>();
        private List<NotificationItem> _activeNotifications = new List<NotificationItem>();
        private Queue<PendingNotification> _pendingNotifications = new Queue<PendingNotification>();
        
        #endregion
        
        #region Structs
        
        private struct PendingNotification
        {
            public string message;
            public NotificationType type;
            public float duration;
            public Sprite customIcon;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Pre-warm pool
            WarmupPool();
        }
        
        private void Update()
        {
            ProcessPendingNotifications();
            UpdateNotificationPositions();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Shows a notification message
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float? duration = null)
        {
            if (_activeNotifications.Count >= maxNotifications)
            {
                // Queue for later
                _pendingNotifications.Enqueue(new PendingNotification
                {
                    message = message,
                    type = type,
                    duration = duration ?? displayDuration
                });
                return;
            }
            
            CreateNotification(message, type, duration);
        }
        
        /// <summary>
        /// Shows a research completion notification
        /// </summary>
        public void ShowResearchComplete(string researchName)
        {
            string message = $"Research Complete: {researchName}";
            ShowNotification(message, NotificationType.Research, displayDuration * 1.5f);
            PlaySound(successSound);
        }
        
        /// <summary>
        /// Shows a money earned notification
        /// </summary>
        public void ShowMoneyEarned(float amount)
        {
            string message = $"+${amount:N0}";
            ShowNotification(message, NotificationType.Success, 2f);
        }
        
        /// <summary>
        /// Shows a money spent notification
        /// </summary>
        public void ShowMoneySpent(float amount)
        {
            string message = $"-${amount:N0}";
            ShowNotification(message, NotificationType.Info, 2f);
        }
        
        /// <summary>
        /// Shows an event triggered notification
        /// </summary>
        public void ShowEventTriggered(string eventName)
        {
            string message = $"Event: {eventName}";
            ShowNotification(message, NotificationType.Event, displayDuration * 1.5f);
            PlaySound(notificationSound);
        }
        
        /// <summary>
        /// Shows a contract notification
        /// </summary>
        public void ShowContractNotification(string message, bool isPositive)
        {
            ShowNotification(message, isPositive ? NotificationType.Contract : NotificationType.Warning);
        }
        
        /// <summary>
        /// Shows an achievement unlocked notification
        /// </summary>
        public void ShowAchievementUnlocked(string achievementName)
        {
            string message = $"Achievement Unlocked: {achievementName}";
            ShowNotification(message, NotificationType.Achievement, displayDuration * 2f);
            PlaySound(achievementSound);
        }
        
        /// <summary>
        /// Shows a warning notification
        /// </summary>
        public void ShowWarning(string message)
        {
            ShowNotification(message, NotificationType.Warning);
            PlaySound(warningSound);
        }
        
        /// <summary>
        /// Shows an error notification
        /// </summary>
        public void ShowError(string message)
        {
            ShowNotification(message, NotificationType.Error, displayDuration * 1.5f);
            PlaySound(errorSound);
        }
        
        /// <summary>
        /// Clears all active notifications
        /// </summary>
        public void ClearAllNotifications()
        {
            foreach (var notification in _activeNotifications)
            {
                if (notification != null)
                {
                    notification.OnDismiss -= OnNotificationDismissed;
                    ReturnToPool(notification);
                }
            }
            
            _activeNotifications.Clear();
            _pendingNotifications.Clear();
        }
        
        #endregion
        
        #region Private Methods
        
        private void CreateNotification(string message, NotificationType type, float? duration)
        {
            NotificationItem notification = GetFromPool();
            if (notification == null) return;
            
            // Setup notification
            notification.SetMessage(message);
            notification.SetIcon(GetIconForType(type));
            notification.SetColor(GetColorForType(type));
            notification.SetDuration(duration ?? displayDuration);
            
            // Subscribe to dismiss event
            notification.OnDismiss += OnNotificationDismissed;
            
            // Add to active list
            _activeNotifications.Add(notification);
            
            // Position notification
            PositionNotification(notification, _activeNotifications.Count - 1);
            
            // Show with animation
            notification.Show(fadeInDuration, slideInDistance);
            
            // Play sound
            PlaySoundForType(type);
        }
        
        private void OnNotificationDismissed(NotificationItem notification)
        {
            if (_activeNotifications.Contains(notification))
            {
                _activeNotifications.Remove(notification);
                ReturnToPool(notification);
            }
        }
        
        private void ProcessPendingNotifications()
        {
            while (_pendingNotifications.Count > 0 && _activeNotifications.Count < maxNotifications)
            {
                var pending = _pendingNotifications.Dequeue();
                CreateNotification(pending.message, pending.type, pending.duration);
            }
        }
        
        private void UpdateNotificationPositions()
        {
            for (int i = 0; i < _activeNotifications.Count; i++)
            {
                PositionNotification(_activeNotifications[i], i);
            }
        }
        
        private void PositionNotification(NotificationItem notification, int index)
        {
            if (notification == null || notification.RectTransform == null) return;
            
            RectTransform rect = notification.RectTransform;
            rect.SetParent(notificationContainer, false);
            
            // Calculate position (stack from top)
            float yOffset = -(index * (rect.sizeDelta.y + spacing));
            rect.anchoredPosition = new Vector2(0, yOffset);
        }
        
        #endregion
        
        #region Pool Management
        
        private void WarmupPool()
        {
            if (notificationPrefab == null) return;
            
            for (int i = 0; i < maxNotifications; i++)
            {
                var notification = Instantiate(notificationPrefab, notificationContainer);
                notification.gameObject.SetActive(false);
                _notificationPool.Enqueue(notification);
            }
        }
        
        private NotificationItem GetFromPool()
        {
            if (_notificationPool.Count > 0)
            {
                return _notificationPool.Dequeue();
            }
            
            if (notificationPrefab != null)
            {
                return Instantiate(notificationPrefab, notificationContainer);
            }
            
            return null;
        }
        
        private void ReturnToPool(NotificationItem notification)
        {
            if (notification == null) return;
            
            notification.OnDismiss -= OnNotificationDismissed;
            notification.gameObject.SetActive(false);
            _notificationPool.Enqueue(notification);
        }
        
        #endregion
        
        #region Helpers
        
        private Sprite GetIconForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => infoIcon,
                NotificationType.Success => successIcon,
                NotificationType.Warning => warningIcon,
                NotificationType.Error => errorIcon,
                NotificationType.Research => researchIcon,
                NotificationType.Contract => contractIcon,
                NotificationType.Event => eventIcon,
                NotificationType.Achievement => achievementIcon,
                _ => infoIcon
            };
        }
        
        private Color GetColorForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => infoColor,
                NotificationType.Success => successColor,
                NotificationType.Warning => warningColor,
                NotificationType.Error => errorColor,
                NotificationType.Research => researchColor,
                NotificationType.Contract => contractColor,
                NotificationType.Event => eventColor,
                NotificationType.Achievement => achievementColor,
                _ => infoColor
            };
        }
        
        private void PlaySoundForType(NotificationType type)
        {
            var sound = type switch
            {
                NotificationType.Success => successSound,
                NotificationType.Warning => warningSound,
                NotificationType.Error => errorSound,
                NotificationType.Achievement => achievementSound,
                _ => notificationSound
            };
            
            PlaySound(sound);
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Individual notification item
    /// </summary>
    public class NotificationItem : MonoBehaviour
    {
        #region Fields
        
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Button dismissButton;
        
        private float _duration;
        private float _elapsedTime;
        private bool _isShowing;
        
        #endregion
        
        #region Properties
        
        public RectTransform RectTransform => rectTransform;
        
        #endregion
        
        #region Events
        
        public event Action<NotificationItem> OnDismiss;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (dismissButton != null)
            {
                dismissButton.onClick.AddListener(Dismiss);
            }
        }
        
        private void Update()
        {
            if (_isShowing)
            {
                _elapsedTime += Time.deltaTime;
                
                if (_elapsedTime >= _duration)
                {
                    Dismiss();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }
        
        public void SetIcon(Sprite icon)
        {
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
        }
        
        public void SetColor(Color color)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }
        
        public void SetDuration(float duration)
        {
            _duration = duration;
            _elapsedTime = 0f;
        }
        
        public void Show(float fadeDuration, float slideDistance)
        {
            _isShowing = true;
            gameObject.SetActive(true);
            
            // Animate in
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                StartCoroutine(FadeIn(fadeDuration));
            }
            
            // Slide in
            if (rectTransform != null)
            {
                Vector2 targetPosition = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = targetPosition + new Vector2(slideDistance, 0);
                StartCoroutine(SlideIn(targetPosition, fadeDuration));
            }
        }
        
        public void Dismiss()
        {
            if (!_isShowing) return;
            
            _isShowing = false;
            StartCoroutine(FadeOutAndDismiss());
        }
        
        #endregion
        
        #region Coroutines
        
        private System.Collections.IEnumerator FadeIn(float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = elapsed / duration;
                }
                yield return null;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
        
        private System.Collections.IEnumerator SlideIn(Vector2 targetPosition, float duration)
        {
            float elapsed = 0f;
            Vector2 startPosition = rectTransform.anchoredPosition;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0, 1, t);
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPosition;
        }
        
        private System.Collections.IEnumerator FadeOutAndDismiss()
        {
            float elapsed = 0f;
            float duration = 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - (elapsed / duration);
                }
                yield return null;
            }
            
            OnDismiss?.Invoke(this);
        }
        
        #endregion
    }
}
