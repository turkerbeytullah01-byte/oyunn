using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Debug shortcuts and hotkeys for Project Aegis: Drone Dominion
    /// Provides keyboard shortcuts for common debug actions
    /// </summary>
    public class DebugShortcuts : MonoBehaviour
    {
        #region Singleton
        private static DebugShortcuts _instance;
        public static DebugShortcuts Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DebugShortcuts>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DebugShortcuts");
                        _instance = go.AddComponent<DebugShortcuts>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Shortcut Definitions
        [System.Serializable]
        public class DebugShortcut
        {
            public string name;
            public string description;
            public KeyCode key;
            public KeyCode modifier;
            public bool requireModifier;
            public Action action;
            public bool enabled = true;
        }

        [Header("Core Shortcuts")]
        [Tooltip("Toggle debug panel visibility")]
        public KeyCode toggleDebugPanelKey = KeyCode.BackQuote; // ` key
        
        [Tooltip("Cycle time scale")]
        public KeyCode cycleTimeScaleKey = KeyCode.F1;
        
        [Tooltip("Add money")]
        public KeyCode addMoneyKey = KeyCode.F2;
        
        [Tooltip("Complete current research")]
        public KeyCode completeResearchKey = KeyCode.F3;
        
        [Tooltip("Trigger random event")]
        public KeyCode triggerEventKey = KeyCode.F4;
        
        [Tooltip("Quick save")]
        public KeyCode quickSaveKey = KeyCode.F5;
        
        [Tooltip("Quick load")]
        public KeyCode quickLoadKey = KeyCode.F6;
        
        [Tooltip("Toggle performance monitor")]
        public KeyCode togglePerformanceKey = KeyCode.F7;
        
        [Tooltip("Toggle gizmos")]
        public KeyCode toggleGizmosKey = KeyCode.F8;
        
        [Tooltip("Reset all progress")]
        public KeyCode resetProgressKey = KeyCode.F9;
        
        [Tooltip("Take screenshot")]
        public KeyCode screenshotKey = KeyCode.F12;
        #endregion

        #region Modifier Settings
        [Header("Modifier Settings")]
        [Tooltip("Require modifier key for function keys")]
        public bool requireModifierForFKeys = true;
        
        [Tooltip("Primary modifier key")]
        public KeyCode primaryModifier = KeyCode.LeftShift;
        
        [Tooltip("Secondary modifier key")]
        public KeyCode secondaryModifier = KeyCode.RightShift;
        #endregion

        #region Shortcut Lists
        [Header("Custom Shortcuts")]
        public List<DebugShortcut> customShortcuts = new List<DebugShortcut>();
        
        private List<DebugShortcut> _allShortcuts = new List<DebugShortcut>();
        private Dictionary<KeyCode, List<DebugShortcut>> _shortcutMap = new Dictionary<KeyCode, List<DebugShortcut>>();
        #endregion

        #region Events
        public event Action<string> OnShortcutExecuted;
        public event Action OnDebugPanelToggled;
        #endregion

        #region Private Fields
        private bool _shortcutsEnabled = true;
        private float _lastScreenshotTime;
        private const float SCREENSHOT_COOLDOWN = 0.5f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(go);
            
            InitializeShortcuts();
        }

        private void Update()
        {
            if (!_shortcutsEnabled) return;
            if (!DebugManager.Instance?.enableDebugMode ?? false) return;
            
            ProcessInput();
        }
        #endregion

        #region Initialization
        private void InitializeShortcuts()
        {
            // Register core shortcuts
            RegisterCoreShortcuts();
            
            // Register custom shortcuts
            foreach (var shortcut in customShortcuts)
            {
                if (shortcut.action != null)
                {
                    RegisterShortcut(shortcut);
                }
            }
        }

        private void RegisterCoreShortcuts()
        {
            // Toggle debug panel
            RegisterShortcut(new DebugShortcut
            {
                name = "Toggle Debug Panel",
                description = "Show/hide debug panel",
                key = toggleDebugPanelKey,
                requireModifier = false,
                action = () =>
                {
                    DebugManager.Instance?.ToggleDebugPanel();
                    OnDebugPanelToggled?.Invoke();
                }
            });
            
            // Cycle time scale
            RegisterShortcut(new DebugShortcut
            {
                name = "Cycle Time Scale",
                description = "Cycle through time scale presets",
                key = cycleTimeScaleKey,
                requireModifier = true,
                action = () =>
                {
                    TimeManipulator.Instance?.CycleTimeScale();
                }
            });
            
            // Add money
            RegisterShortcut(new DebugShortcut
            {
                name = "Add Money",
                description = $"Add {DebugManager.Instance?.moneyCheatAmount ?? 10000} money",
                key = addMoneyKey,
                requireModifier = true,
                action = () =>
                {
                    float amount = DebugManager.Instance?.moneyCheatAmount ?? 10000f;
                    DebugManager.Instance?.AddMoney(amount);
                }
            });
            
            // Complete research
            RegisterShortcut(new DebugShortcut
            {
                name = "Complete Research",
                description = "Complete current research",
                key = completeResearchKey,
                requireModifier = true,
                action = () =>
                {
                    ResearchDebugger.Instance?.CompleteCurrentResearch();
                }
            });
            
            // Trigger event
            RegisterShortcut(new DebugShortcut
            {
                name = "Trigger Event",
                description = "Trigger random event",
                key = triggerEventKey,
                requireModifier = true,
                action = () =>
                {
                    EventDebugger.Instance?.TriggerRandomEvent();
                }
            });
            
            // Quick save
            RegisterShortcut(new DebugShortcut
            {
                name = "Quick Save",
                description = "Save game",
                key = quickSaveKey,
                requireModifier = true,
                action = () =>
                {
                    SaveDebugger.Instance?.QuickSave();
                }
            });
            
            // Quick load
            RegisterShortcut(new DebugShortcut
            {
                name = "Quick Load",
                description = "Load game",
                key = quickLoadKey,
                requireModifier = true,
                action = () =>
                {
                    SaveDebugger.Instance?.QuickLoad();
                }
            });
            
            // Toggle performance monitor
            RegisterShortcut(new DebugShortcut
            {
                name = "Toggle Performance",
                description = "Show/hide performance monitor",
                key = togglePerformanceKey,
                requireModifier = true,
                action = () =>
                {
                    PerformanceMonitor.Instance?.ToggleVisibility();
                }
            });
            
            // Toggle gizmos
            RegisterShortcut(new DebugShortcut
            {
                name = "Toggle Gizmos",
                description = "Toggle debug gizmos",
                key = toggleGizmosKey,
                requireModifier = true,
                action = () =>
                {
                    DebugGizmos.Instance?.ToggleAllGizmos(true);
                }
            });
            
            // Reset progress
            RegisterShortcut(new DebugShortcut
            {
                name = "Reset Progress",
                description = "Reset all progress (requires confirmation)",
                key = resetProgressKey,
                requireModifier = true,
                action = () =>
                {
                    DebugManager.Instance?.ResetAllProgress();
                }
            });
            
            // Screenshot
            RegisterShortcut(new DebugShortcut
            {
                name = "Screenshot",
                description = "Take screenshot",
                key = screenshotKey,
                requireModifier = false,
                action = TakeScreenshot
            });
        }

        private void RegisterShortcut(DebugShortcut shortcut)
        {
            _allShortcuts.Add(shortcut);
            
            if (!_shortcutMap.ContainsKey(shortcut.key))
            {
                _shortcutMap[shortcut.key] = new List<DebugShortcut>();
            }
            
            _shortcutMap[shortcut.key].Add(shortcut);
        }
        #endregion

        #region Input Processing
        private void ProcessInput()
        {
            bool modifierPressed = Input.GetKey(primaryModifier) || Input.GetKey(secondaryModifier);
            
            foreach (var kvp in _shortcutMap)
            {
                if (Input.GetKeyDown(kvp.Key))
                {
                    foreach (var shortcut in kvp.Value)
                    {
                        if (!shortcut.enabled) continue;
                        
                        bool canExecute = !shortcut.requireModifier || 
                                         (shortcut.requireModifier && modifierPressed) ||
                                         (!requireModifierForFKeys && IsFunctionKey(shortcut.key));
                        
                        if (canExecute)
                        {
                            ExecuteShortcut(shortcut);
                            break; // Only execute first matching shortcut
                        }
                    }
                }
            }
        }

        private bool IsFunctionKey(KeyCode key)
        {
            return key >= KeyCode.F1 && key <= KeyCode.F15;
        }

        private void ExecuteShortcut(DebugShortcut shortcut)
        {
            try
            {
                shortcut.action?.Invoke();
                OnShortcutExecuted?.Invoke(shortcut.name);
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction($"Shortcut executed: {shortcut.name}");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[DebugShortcuts] Error executing shortcut '{shortcut.name}': {e.Message}");
            }
        }
        #endregion

        #region Screenshot
        private void TakeScreenshot()
        {
            if (Time.time - _lastScreenshotTime < SCREENSHOT_COOLDOWN)
            {
                return;
            }
            
            _lastScreenshotTime = Time.time;
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"Aegis_Screenshot_{timestamp}.png";
            string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
            
            ScreenCapture.CaptureScreenshot(path);
            
            UnityEngine.Debug.Log($"[DebugShortcuts] Screenshot saved: {path}");
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction($"Screenshot saved: {filename}");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enable or disable all shortcuts
        /// </summary>
        public void SetShortcutsEnabled(bool enabled)
        {
            _shortcutsEnabled = enabled;
        }

        /// <summary>
        /// Check if shortcuts are enabled
        /// </summary>
        public bool AreShortcutsEnabled()
        {
            return _shortcutsEnabled;
        }

        /// <summary>
        /// Enable a specific shortcut
        /// </summary>
        public void EnableShortcut(string name)
        {
            var shortcut = _allShortcuts.Find(s => s.name == name);
            if (shortcut != null)
            {
                shortcut.enabled = true;
            }
        }

        /// <summary>
        /// Disable a specific shortcut
        /// </summary>
        public void DisableShortcut(string name)
        {
            var shortcut = _allShortcuts.Find(s => s.name == name);
            if (shortcut != null)
            {
                shortcut.enabled = false;
            }
        }

        /// <summary>
        /// Add a custom shortcut
        /// </summary>
        public void AddCustomShortcut(string name, string description, KeyCode key, bool requireModifier, Action action)
        {
            var shortcut = new DebugShortcut
            {
                name = name,
                description = description,
                key = key,
                requireModifier = requireModifier,
                action = action
            };
            
            customShortcuts.Add(shortcut);
            RegisterShortcut(shortcut);
        }

        /// <summary>
        /// Remove a custom shortcut
        /// </summary>
        public void RemoveCustomShortcut(string name)
        {
            var shortcut = customShortcuts.Find(s => s.name == name);
            if (shortcut != null)
            {
                customShortcuts.Remove(shortcut);
                _allShortcuts.Remove(shortcut);
                
                if (_shortcutMap.ContainsKey(shortcut.key))
                {
                    _shortcutMap[shortcut.key].Remove(shortcut);
                }
            }
        }

        /// <summary>
        /// Get all shortcuts
        /// </summary>
        public List<DebugShortcut> GetAllShortcuts()
        {
            return new List<DebugShortcut>(_allShortcuts);
        }

        /// <summary>
        /// Get shortcut by name
        /// </summary>
        public DebugShortcut GetShortcut(string name)
        {
            return _allShortcuts.Find(s => s.name == name);
        }

        /// <summary>
        /// Generate help text
        /// </summary>
        public string GenerateHelpText()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("=== DEBUG SHORTCUTS ===");
            sb.AppendLine();
            sb.AppendLine($"Modifier: {primaryModifier} or {secondaryModifier} (hold for F-keys)");
            sb.AppendLine();
            
            foreach (var shortcut in _allShortcuts)
            {
                string modifierStr = shortcut.requireModifier ? $"[{primaryModifier}] + " : "";
                string statusStr = shortcut.enabled ? "" : " [DISABLED]";
                sb.AppendLine($"{modifierStr}{shortcut.key}: {shortcut.description}{statusStr}");
            }
            
            return sb.ToString();
        }
        #endregion
    }
}
