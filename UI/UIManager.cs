using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Main UI controller for Project Aegis: Drone Dominion
    /// Manages screens, popups, notifications, and UI event routing
    /// </summary>
    public class UIManager : BaseManager<UIManager>
    {
        #region Fields
        
        [Header("Screen Management")]
        [SerializeField] private Transform screenContainer;
        [SerializeField] private List<BaseScreen> screenPrefabs;
        
        [Header("Popup System")]
        [SerializeField] private PopupSystem popupSystem;
        [SerializeField] private Transform popupOverlay;
        
        [Header("Notification System")]
        [SerializeField] private NotificationSystem notificationSystem;
        
        [Header("Top Bar")]
        [SerializeField] private TopBarUI topBar;
        
        [Header("Loading Screen")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private CanvasGroup loadingCanvasGroup;
        
        [Header("UI Theme")]
        [SerializeField] private UITheme uiTheme;
        
        // Runtime state
        private Dictionary<string, BaseScreen> _screenCache = new Dictionary<string, BaseScreen>();
        private Stack<BaseScreen> _screenStack = new Stack<BaseScreen>();
        private BaseScreen _currentScreen;
        private bool _isPopupOpen;
        private bool _isLoading;
        
        // Events
        public event Action<string> OnScreenShown;
        public event Action<string> OnScreenHidden;
        public event Action OnPopupShown;
        public event Action OnPopupHidden;
        
        #endregion
        
        #region Properties
        
        public UITheme Theme => uiTheme;
        public BaseScreen CurrentScreen => _currentScreen;
        public bool IsPopupOpen => _isPopupOpen;
        public bool IsLoading => _isLoading;
        public TopBarUI TopBar => topBar;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            InitializeScreens();
        }
        
        private void Start()
        {
            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
                GameManager.Instance.OnReputationChanged += OnReputationChanged;
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
                GameManager.Instance.OnReputationChanged -= OnReputationChanged;
            }
        }
        
        #endregion
        
        #region Screen Management
        
        /// <summary>
        /// Shows a screen by its ID
        /// </summary>
        public void ShowScreen(string screenId, bool addToStack = true)
        {
            if (_currentScreen != null && _currentScreen.screenId == screenId)
                return;
            
            BaseScreen screen = GetOrCreateScreen(screenId);
            if (screen == null)
            {
                Debug.LogError($"[UIManager] Screen not found: {screenId}");
                return;
            }
            
            // Hide current screen if not modal
            if (_currentScreen != null && !screen.isModal)
            {
                HideCurrentScreen(addToStack);
            }
            
            // Show new screen
            screen.gameObject.SetActive(true);
            screen.OnShow();
            
            // Animate in
            AnimateScreenIn(screen);
            
            _currentScreen = screen;
            OnScreenShown?.Invoke(screenId);
            
            Debug.Log($"[UIManager] Screen shown: {screenId}");
        }
        
        /// <summary>
        /// Hides a specific screen by ID
        /// </summary>
        public void HideScreen(string screenId)
        {
            if (_screenCache.TryGetValue(screenId, out BaseScreen screen))
            {
                if (screen.gameObject.activeSelf)
                {
                    AnimateScreenOut(screen, () =>
                    {
                        screen.OnHide();
                        if (!screen.cacheOnHide)
                        {
                            screen.gameObject.SetActive(false);
                        }
                    });
                    
                    OnScreenHidden?.Invoke(screenId);
                }
            }
        }
        
        /// <summary>
        /// Goes back to the previous screen in the stack
        /// </summary>
        public void GoBack()
        {
            if (_screenStack.Count > 0)
            {
                BaseScreen previousScreen = _screenStack.Pop();
                ShowScreen(previousScreen.screenId, false);
            }
            else
            {
                ShowScreen(ScreenIds.MAIN_MENU);
            }
        }
        
        /// <summary>
        /// Refreshes the current screen
        /// </summary>
        public void RefreshCurrentScreen()
        {
            _currentScreen?.OnRefresh();
        }
        
        private void HideCurrentScreen(bool addToStack)
        {
            if (_currentScreen != null)
            {
                if (addToStack)
                {
                    _screenStack.Push(_currentScreen);
                }
                
                AnimateScreenOut(_currentScreen, () =>
                {
                    _currentScreen.OnHide();
                    if (!_currentScreen.cacheOnHide)
                    {
                        _currentScreen.gameObject.SetActive(false);
                    }
                });
            }
        }
        
        private BaseScreen GetOrCreateScreen(string screenId)
        {
            // Check cache first
            if (_screenCache.TryGetValue(screenId, out BaseScreen cachedScreen))
            {
                return cachedScreen;
            }
            
            // Find and instantiate from prefabs
            BaseScreen prefab = screenPrefabs.Find(s => s.screenId == screenId);
            if (prefab != null)
            {
                BaseScreen instance = Instantiate(prefab, screenContainer);
                instance.name = $"Screen_{screenId}";
                _screenCache[screenId] = instance;
                return instance;
            }
            
            return null;
        }
        
        private void InitializeScreens()
        {
            // Pre-cache all screen prefabs
            foreach (var prefab in screenPrefabs)
            {
                if (!_screenCache.ContainsKey(prefab.screenId))
                {
                    var instance = Instantiate(prefab, screenContainer);
                    instance.name = $"Screen_{prefab.screenId}";
                    instance.gameObject.SetActive(false);
                    _screenCache[prefab.screenId] = instance;
                }
            }
        }
        
        #endregion
        
        #region Popup Management
        
        /// <summary>
        /// Shows a popup with the given data
        /// </summary>
        public void ShowPopup(PopupData data)
        {
            if (popupSystem == null)
            {
                Debug.LogError("[UIManager] PopupSystem not assigned!");
                return;
            }
            
            _isPopupOpen = true;
            popupOverlay.gameObject.SetActive(true);
            
            if (data.choices != null && data.choices.Count > 0)
            {
                popupSystem.ShowChoicePopup(data.title, data.description, data.choices);
            }
            else
            {
                popupSystem.ShowInfoPopup(data.title, data.description);
            }
            
            OnPopupShown?.Invoke();
        }
        
        /// <summary>
        /// Shows an event popup
        /// </summary>
        public void ShowEventPopup(GameEventData eventData)
        {
            if (popupSystem == null) return;
            
            _isPopupOpen = true;
            popupOverlay.gameObject.SetActive(true);
            popupSystem.ShowEventPopup(eventData);
            
            OnPopupShown?.Invoke();
        }
        
        /// <summary>
        /// Hides the current popup
        /// </summary>
        public void HidePopup()
        {
            if (popupSystem == null) return;
            
            popupSystem.HidePopup();
            popupOverlay.gameObject.SetActive(false);
            _isPopupOpen = false;
            
            OnPopupHidden?.Invoke();
        }
        
        #endregion
        
        #region Notifications
        
        /// <summary>
        /// Shows a notification message
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            notificationSystem?.ShowNotification(message, type);
        }
        
        /// <summary>
        /// Shows a research completion notification
        /// </summary>
        public void ShowResearchComplete(string researchName)
        {
            notificationSystem?.ShowResearchComplete(researchName);
        }
        
        /// <summary>
        /// Shows a money earned notification
        /// </summary>
        public void ShowMoneyEarned(float amount)
        {
            notificationSystem?.ShowMoneyEarned(amount);
        }
        
        /// <summary>
        /// Shows an event triggered notification
        /// </summary>
        public void ShowEventTriggered(string eventName)
        {
            notificationSystem?.ShowEventTriggered(eventName);
        }
        
        #endregion
        
        #region Resource Display
        
        /// <summary>
        /// Updates the resource display in the top bar
        /// </summary>
        public void UpdateResourceDisplay()
        {
            if (GameManager.Instance != null && topBar != null)
            {
                topBar.UpdateMoney(GameManager.Instance.Money);
                topBar.UpdateReputation(GameManager.Instance.Reputation);
            }
        }
        
        private void OnMoneyChanged(float newAmount, float delta)
        {
            topBar?.UpdateMoney(newAmount);
            
            if (delta > 0)
            {
                ShowMoneyEarned(delta);
            }
        }
        
        private void OnReputationChanged(float newAmount, float delta)
        {
            topBar?.UpdateReputation(newAmount);
        }
        
        #endregion
        
        #region Loading Screen
        
        /// <summary>
        /// Shows or hides the loading screen
        /// </summary>
        public void SetLoading(bool loading, string message = "")
        {
            _isLoading = loading;
            
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(loading);
                
                if (loading && loadingCanvasGroup != null)
                {
                    UIAnimator.FadeIn(loadingCanvasGroup, 0.3f);
                }
            }
            
            // Update loading message if available
            if (!string.IsNullOrEmpty(message))
            {
                var messageText = loadingScreen?.GetComponentInChildren<TextMeshProUGUI>();
                if (messageText != null)
                {
                    messageText.text = message;
                }
            }
        }
        
        #endregion
        
        #region Animations
        
        private void AnimateScreenIn(BaseScreen screen)
        {
            var canvasGroup = screen.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                UIAnimator.FadeIn(canvasGroup, 0.3f);
            }
            else
            {
                UIAnimator.ScaleIn(screen.transform, 0.3f);
            }
        }
        
        private void AnimateScreenOut(BaseScreen screen, Action onComplete)
        {
            var canvasGroup = screen.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                UIAnimator.FadeOut(canvasGroup, 0.2f);
                Invoke(nameof(InvokeComplete), 0.2f);
            }
            else
            {
                onComplete?.Invoke();
            }
            
            void InvokeComplete() => onComplete?.Invoke();
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Gets a screen by ID (returns null if not cached)
        /// </summary>
        public BaseScreen GetScreen(string screenId)
        {
            _screenCache.TryGetValue(screenId, out BaseScreen screen);
            return screen;
        }
        
        /// <summary>
        /// Checks if a screen is currently visible
        /// </summary>
        public bool IsScreenVisible(string screenId)
        {
            return _currentScreen != null && _currentScreen.screenId == screenId;
        }
        
        /// <summary>
        /// Clears the screen navigation stack
        /// </summary>
        public void ClearScreenStack()
        {
            _screenStack.Clear();
        }
        
        #endregion
    }
}
