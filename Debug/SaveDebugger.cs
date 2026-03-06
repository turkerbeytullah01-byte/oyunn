using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectAegis.Debug
{
    /// <summary>
    /// Save debugger for testing save/load systems
    /// Allows developers to inspect, modify, and test save data
    /// </summary>
    public class SaveDebugger : MonoBehaviour
    {
        #region Singleton
        private static SaveDebugger _instance;
        public static SaveDebugger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SaveDebugger>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SaveDebugger");
                        _instance = go.AddComponent<SaveDebugger>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region UI References
        [Header("UI References")]
        [SerializeField] private Button showSaveDataButton;
        [SerializeField] private Button deleteSaveButton;
        [SerializeField] private Button createTestSaveButton;
        [SerializeField] private Button corruptSaveButton;
        [SerializeField] private Button testMigrationButton;
        [SerializeField] private Button exportSaveButton;
        [SerializeField] private Button importSaveButton;
        [SerializeField] private Button quickSaveButton;
        [SerializeField] private Button quickLoadButton;
        [SerializeField] private Button backupSaveButton;
        [SerializeField] private Button restoreBackupButton;
        [SerializeField] private TextMeshProUGUI saveDataText;
        [SerializeField] private TextMeshProUGUI saveInfoText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_InputField customSaveInput;
        #endregion

        #region Settings
        [Header("Save Settings")]
        [Tooltip("Save file name")]
        public string saveFileName = "aegis_save.dat";
        
        [Tooltip("Backup file name")]
        public string backupFileName = "aegis_save_backup.dat";
        
        [Tooltip("Save file version")]
        public int saveVersion = 1;
        
        [Tooltip("Maximum backup count")]
        public int maxBackups = 5;
        
        [Tooltip("Auto-save interval in minutes")]
        public float autoSaveInterval = 5f;
        
        [Tooltip("Enable auto-save")]
        public bool enableAutoSave = false;
        #endregion

        #region Save Data Structure
        [System.Serializable]
        public class DebugSaveData
        {
            public int version;
            public string saveDate;
            public string playtime;
            public PlayerData player;
            public CompanyData company;
            public ResearchData research;
            public ContractsData contracts;
            public SettingsData settings;
            public Dictionary<string, object> customData;
        }

        [System.Serializable]
        public class PlayerData
        {
            public float money;
            public float reputation;
            public int level;
            public float experience;
            public List<string> achievements;
            public Dictionary<string, float> statistics;
        }

        [System.Serializable]
        public class CompanyData
        {
            public string companyName;
            public int employeeCount;
            public int droneCount;
            public List<string> ownedFacilities;
            public Dictionary<string, int> inventory;
        }

        [System.Serializable]
        public class ResearchData
        {
            public List<string> completedResearch;
            public List<string> unlockedTechnologies;
            public string currentResearch;
            public float researchProgress;
            public float totalResearchPoints;
        }

        [System.Serializable]
        public class ContractsData
        {
            public List<string> completedContracts;
            public List<string> activeContracts;
            public List<string> failedContracts;
            public float totalEarnings;
            public int contractsWon;
            public int contractsLost;
        }

        [System.Serializable]
        public class SettingsData
        {
            public float musicVolume;
            public float sfxVolume;
            public bool notificationsEnabled;
            public string language;
            public Dictionary<string, bool> tutorialFlags;
        }
        #endregion

        #region Events
        public event Action OnSaveCreated;
        public event Action OnSaveLoaded;
        public event Action OnSaveDeleted;
        public event Action OnSaveCorrupted;
        public event Action OnMigrationCompleted;
        #endregion

        #region Private Fields
        private DebugSaveData _currentSaveData;
        private string _savePath;
        private string _backupPath;
        private Coroutine _autoSaveCoroutine;
        private List<string> _saveHistory = new List<string>();
        private const int MAX_HISTORY = 20;
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
            
            _savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            _backupPath = Path.Combine(Application.persistentDataPath, backupFileName);
        }

        private void Start()
        {
            SetupUI();
            
            if (enableAutoSave)
            {
                StartAutoSave();
            }
        }
        #endregion

        #region UI Setup
        private void SetupUI()
        {
            if (showSaveDataButton != null)
                showSaveDataButton.onClick.AddListener(ShowSaveData);
            
            if (deleteSaveButton != null)
                deleteSaveButton.onClick.AddListener(DeleteSave);
            
            if (createTestSaveButton != null)
                createTestSaveButton.onClick.AddListener(CreateTestSave);
            
            if (corruptSaveButton != null)
                corruptSaveButton.onClick.AddListener(CorruptSave);
            
            if (testMigrationButton != null)
                testMigrationButton.onClick.AddListener(TestMigration);
            
            if (exportSaveButton != null)
                exportSaveButton.onClick.AddListener(ExportSaveToClipboard);
            
            if (importSaveButton != null)
                importSaveButton.onClick.AddListener(ImportSaveFromClipboard);
            
            if (quickSaveButton != null)
                quickSaveButton.onClick.AddListener(QuickSave);
            
            if (quickLoadButton != null)
                quickLoadButton.onClick.AddListener(QuickLoad);
            
            if (backupSaveButton != null)
                backupSaveButton.onClick.AddListener(BackupSave);
            
            if (restoreBackupButton != null)
                restoreBackupButton.onClick.AddListener(RestoreBackup);
        }
        #endregion

        #region Save Operations
        /// <summary>
        /// Show current save data
        /// </summary>
        public void ShowSaveData()
        {
            if (!File.Exists(_savePath))
            {
                UpdateDisplay("No save file found.");
                return;
            }
            
            try
            {
                string json = File.ReadAllText(_savePath);
                _currentSaveData = JsonUtility.FromJson<DebugSaveData>(json);
                
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("=== SAVE DATA ===");
                sb.AppendLine();
                sb.AppendLine($"Version: {_currentSaveData.version}");
                sb.AppendLine($"Save Date: {_currentSaveData.saveDate}");
                sb.AppendLine($"Playtime: {_currentSaveData.playtime}");
                sb.AppendLine();
                
                if (_currentSaveData.player != null)
                {
                    sb.AppendLine("--- PLAYER ---");
                    sb.AppendLine($"Money: {_currentSaveData.player.money:C}");
                    sb.AppendLine($"Reputation: {_currentSaveData.player.reputation:F1}");
                    sb.AppendLine($"Level: {_currentSaveData.player.level}");
                    sb.AppendLine($"Experience: {_currentSaveData.player.experience:F0}");
                    sb.AppendLine();
                }
                
                if (_currentSaveData.company != null)
                {
                    sb.AppendLine("--- COMPANY ---");
                    sb.AppendLine($"Name: {_currentSaveData.company.companyName}");
                    sb.AppendLine($"Employees: {_currentSaveData.company.employeeCount}");
                    sb.AppendLine($"Drones: {_currentSaveData.company.droneCount}");
                    sb.AppendLine();
                }
                
                if (_currentSaveData.research != null)
                {
                    sb.AppendLine("--- RESEARCH ---");
                    sb.AppendLine($"Completed: {_currentSaveData.research.completedResearch?.Count ?? 0}");
                    sb.AppendLine($"Unlocked: {_currentSaveData.research.unlockedTechnologies?.Count ?? 0}");
                    sb.AppendLine($"Current: {_currentSaveData.research.currentResearch ?? "None"}");
                    sb.AppendLine($"Progress: {_currentSaveData.research.researchProgress * 100f:F1}%");
                    sb.AppendLine();
                }
                
                if (_currentSaveData.contracts != null)
                {
                    sb.AppendLine("--- CONTRACTS ---");
                    sb.AppendLine($"Completed: {_currentSaveData.contracts.completedContracts?.Count ?? 0}");
                    sb.AppendLine($"Active: {_currentSaveData.contracts.activeContracts?.Count ?? 0}");
                    sb.AppendLine($"Failed: {_currentSaveData.contracts.failedContracts?.Count ?? 0}");
                    sb.AppendLine($"Total Earnings: {_currentSaveData.contracts.totalEarnings:C}");
                    sb.AppendLine();
                }
                
                // Raw JSON preview
                sb.AppendLine("--- RAW JSON (first 500 chars) ---");
                sb.AppendLine(json.Substring(0, Math.Min(500, json.Length)));
                
                UpdateDisplay(sb.ToString());
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Save data displayed");
                }
            }
            catch (Exception e)
            {
                UpdateDisplay($"Error reading save: {e.Message}");
            }
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public void DeleteSave()
        {
            if (File.Exists(_savePath))
            {
                File.Delete(_savePath);
                OnSaveDeleted?.Invoke();
                
                UpdateDisplay("Save file deleted.");
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Save file deleted");
                }
            }
            else
            {
                UpdateDisplay("No save file to delete.");
            }
        }

        /// <summary>
        /// Create a test save file
        /// </summary>
        public void CreateTestSave()
        {
            var testSave = new DebugSaveData
            {
                version = saveVersion,
                saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                playtime = "00:00:00",
                player = new PlayerData
                {
                    money = 100000f,
                    reputation = 50f,
                    level = 5,
                    experience = 2500f,
                    achievements = new List<string> { "first_contract", "first_research", "millionaire" },
                    statistics = new Dictionary<string, float>
                    {
                        { "contracts_completed", 10f },
                        { "research_completed", 5f },
                        { "money_earned", 500000f }
                    }
                },
                company = new CompanyData
                {
                    companyName = "Test Corp",
                    employeeCount = 25,
                    droneCount = 15,
                    ownedFacilities = new List<string> { "main_hangar", "research_lab" },
                    inventory = new Dictionary<string, int>
                    {
                        { "drone_parts", 100 },
                        { "batteries", 50 },
                        { "motors", 30 }
                    }
                },
                research = new ResearchData
                {
                    completedResearch = new List<string> { "motor_eff_1", "battery_density_1" },
                    unlockedTechnologies = new List<string> { "motor_eff_2", "ai_navigation" },
                    currentResearch = "motor_eff_2",
                    researchProgress = 0.5f,
                    totalResearchPoints = 5000f
                },
                contracts = new ContractsData
                {
                    completedContracts = new List<string> { "contract_1", "contract_2" },
                    activeContracts = new List<string> { "contract_3" },
                    failedContracts = new List<string>(),
                    totalEarnings = 75000f,
                    contractsWon = 12,
                    contractsLost = 3
                },
                settings = new SettingsData
                {
                    musicVolume = 0.7f,
                    sfxVolume = 0.8f,
                    notificationsEnabled = true,
                    language = "en",
                    tutorialFlags = new Dictionary<string, bool>
                    {
                        { "intro_seen", true },
                        { "contracts_explained", true }
                    }
                }
            };
            
            string json = JsonUtility.ToJson(testSave, true);
            File.WriteAllText(_savePath, json);
            
            _currentSaveData = testSave;
            OnSaveCreated?.Invoke();
            
            UpdateDisplay("Test save created successfully.");
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Test save created");
            }
        }

        /// <summary>
        /// Corrupt save file for testing error handling
        /// </summary>
        public void CorruptSave()
        {
            if (!File.Exists(_savePath))
            {
                UpdateDisplay("No save file to corrupt.");
                return;
            }
            
            try
            {
                string json = File.ReadAllText(_savePath);
                
                // Corrupt the JSON by removing random parts
                int corruptionPoint = UnityEngine.Random.Range(json.Length / 4, json.Length / 2);
                string corruptedJson = json.Substring(0, corruptionPoint) + "CORRUPTED_DATA" + json.Substring(corruptionPoint + 20);
                
                File.WriteAllText(_savePath, corruptedJson);
                
                OnSaveCorrupted?.Invoke();
                UpdateDisplay("Save file corrupted for testing.");
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Save file corrupted (test)");
                }
            }
            catch (Exception e)
            {
                UpdateDisplay($"Error corrupting save: {e.Message}");
            }
        }

        /// <summary>
        /// Test save migration from older version
        /// </summary>
        public void TestMigration()
        {
            // Create an old version save
            var oldSave = new Dictionary<string, object>
            {
                { "version", 0 },
                { "money", 50000f },
                { "reputation", 30f },
                { "level", 3 }
            };
            
            // Simulate migration
            var migratedSave = MigrateSave(oldSave);
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== MIGRATION TEST ===");
            sb.AppendLine();
            sb.AppendLine("Old Version Data:");
            foreach (var kvp in oldSave)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            sb.AppendLine();
            sb.AppendLine("Migrated Data:");
            sb.AppendLine($"  Version: {migratedSave.version}");
            sb.AppendLine($"  Money: {migratedSave.player?.money}");
            sb.AppendLine($"  Reputation: {migratedSave.player?.reputation}");
            sb.AppendLine($"  Level: {migratedSave.player?.level}");
            
            UpdateDisplay(sb.ToString());
            OnMigrationCompleted?.Invoke();
            
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.LogDebugAction("Save migration tested");
            }
        }

        private DebugSaveData MigrateSave(Dictionary<string, object> oldSave)
        {
            var newSave = new DebugSaveData
            {
                version = saveVersion,
                saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                playtime = "00:00:00",
                player = new PlayerData
                {
                    money = oldSave.ContainsKey("money") ? Convert.ToSingle(oldSave["money"]) : 0f,
                    reputation = oldSave.ContainsKey("reputation") ? Convert.ToSingle(oldSave["reputation"]) : 0f,
                    level = oldSave.ContainsKey("level") ? Convert.ToInt32(oldSave["level"]) : 1,
                    experience = 0f,
                    achievements = new List<string>(),
                    statistics = new Dictionary<string, float>()
                },
                company = new CompanyData
                {
                    companyName = "Migrated Company",
                    employeeCount = 0,
                    droneCount = 0,
                    ownedFacilities = new List<string>(),
                    inventory = new Dictionary<string, int>()
                },
                research = new ResearchData
                {
                    completedResearch = new List<string>(),
                    unlockedTechnologies = new List<string>(),
                    researchProgress = 0f,
                    totalResearchPoints = 0f
                },
                contracts = new ContractsData
                {
                    completedContracts = new List<string>(),
                    activeContracts = new List<string>(),
                    failedContracts = new List<string>(),
                    totalEarnings = 0f,
                    contractsWon = 0,
                    contractsLost = 0
                },
                settings = new SettingsData
                {
                    musicVolume = 0.7f,
                    sfxVolume = 0.8f,
                    notificationsEnabled = true,
                    language = "en",
                    tutorialFlags = new Dictionary<string, bool>()
                }
            };
            
            return newSave;
        }
        #endregion

        #region Import/Export
        /// <summary>
        /// Export save to clipboard
        /// </summary>
        public void ExportSaveToClipboard()
        {
            if (!File.Exists(_savePath))
            {
                UpdateDisplay("No save file to export.");
                return;
            }
            
            try
            {
                string json = File.ReadAllText(_savePath);
                GUIUtility.systemCopyBuffer = json;
                
                UpdateDisplay("Save data exported to clipboard.");
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Save exported to clipboard");
                }
            }
            catch (Exception e)
            {
                UpdateDisplay($"Error exporting save: {e.Message}");
            }
        }

        /// <summary>
        /// Import save from clipboard
        /// </summary>
        public void ImportSaveFromClipboard()
        {
            try
            {
                string json = GUIUtility.systemCopyBuffer;
                
                if (string.IsNullOrEmpty(json))
                {
                    UpdateDisplay("Clipboard is empty.");
                    return;
                }
                
                // Validate JSON
                var testData = JsonUtility.FromJson<DebugSaveData>(json);
                if (testData == null)
                {
                    UpdateDisplay("Invalid save data in clipboard.");
                    return;
                }
                
                File.WriteAllText(_savePath, json);
                UpdateDisplay("Save data imported from clipboard.");
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Save imported from clipboard");
                }
            }
            catch (Exception e)
            {
                UpdateDisplay($"Error importing save: {e.Message}");
            }
        }
        #endregion

        #region Quick Save/Load
        /// <summary>
        /// Quick save
        /// </summary>
        public void QuickSave()
        {
            CreateTestSave(); // For now, creates test save
            UpdateDisplay("Quick save completed.");
        }

        /// <summary>
        /// Quick load
        /// </summary>
        public void QuickLoad()
        {
            ShowSaveData();
            OnSaveLoaded?.Invoke();
        }
        #endregion

        #region Backup Management
        /// <summary>
        /// Create backup of save
        /// </summary>
        public void BackupSave()
        {
            if (!File.Exists(_savePath))
            {
                UpdateDisplay("No save file to backup.");
                return;
            }
            
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = Path.Combine(Application.persistentDataPath, $"aegis_save_{timestamp}.bak");
                
                File.Copy(_savePath, backupFile, true);
                
                _saveHistory.Add(backupFile);
                while (_saveHistory.Count > maxBackups)
                {
                    string oldBackup = _saveHistory[0];
                    _saveHistory.RemoveAt(0);
                    if (File.Exists(oldBackup))
                    {
                        File.Delete(oldBackup);
                    }
                }
                
                UpdateDisplay($"Backup created: {backupFile}");
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Save backup created");
                }
            }
            catch (Exception e)
            {
                UpdateDisplay($"Error creating backup: {e.Message}");
            }
        }

        /// <summary>
        /// Restore from backup
        /// </summary>
        public void RestoreBackup()
        {
            if (_saveHistory.Count == 0)
            {
                UpdateDisplay("No backups available.");
                return;
            }
            
            string latestBackup = _saveHistory[_saveHistory.Count - 1];
            
            if (!File.Exists(latestBackup))
            {
                UpdateDisplay("Backup file not found.");
                return;
            }
            
            try
            {
                // Backup current save first
                if (File.Exists(_savePath))
                {
                    string tempBackup = _savePath + ".temp";
                    File.Copy(_savePath, tempBackup, true);
                }
                
                File.Copy(latestBackup, _savePath, true);
                UpdateDisplay("Save restored from backup.");
                
                if (DebugManager.Instance != null)
                {
                    DebugManager.Instance.LogDebugAction("Save restored from backup");
                }
            }
            catch (Exception e)
            {
                UpdateDisplay($"Error restoring backup: {e.Message}");
            }
        }
        #endregion

        #region Auto Save
        private void StartAutoSave()
        {
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
            }
            
            _autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
        }

        private IEnumerator AutoSaveRoutine()
        {
            while (enableAutoSave)
            {
                yield return new WaitForSeconds(autoSaveInterval * 60f);
                QuickSave();
            }
        }
        #endregion

        #region Utility
        private void UpdateDisplay(string message)
        {
            if (saveDataText != null)
            {
                saveDataText.text = message;
            }
            
            // Update info text
            if (saveInfoText != null)
            {
                bool exists = File.Exists(_savePath);
                long fileSize = exists ? new FileInfo(_savePath).Length : 0;
                
                saveInfoText.text = $"Save: {(exists ? "EXISTS" : "NONE")} | " +
                                   $"Size: {fileSize} bytes | " +
                                   $"Path: {_savePath}";
            }
            
            // Auto-scroll
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// Check if save exists
        /// </summary>
        public bool SaveExists()
        {
            return File.Exists(_savePath);
        }

        /// <summary>
        /// Get save file size
        /// </summary>
        public long GetSaveFileSize()
        {
            if (!File.Exists(_savePath)) return 0;
            return new FileInfo(_savePath).Length;
        }

        /// <summary>
        /// Get save file path
        /// </summary>
        public string GetSavePath()
        {
            return _savePath;
        }
        #endregion
    }
}
