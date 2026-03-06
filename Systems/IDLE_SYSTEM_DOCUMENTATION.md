# Project Aegis: Idle/Offline Progression System Documentation

## Overview

The Idle System for "Project Aegis: Drone Dominion" is a robust offline progression framework that ensures the game continues to generate resources and progress research while the app is closed. It includes comprehensive anti-cheat measures and handles various edge cases.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        IdleManager                               │
│                    (Main Controller)                             │
├─────────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  Production  │  │  Timestamp   │  │     Save System      │  │
│  │   Manager    │  │   Manager    │  │  (IdleSaveData)      │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Core Components

### 1. IdleManager.cs
**Purpose:** Main controller for offline progression

**Key Responsibilities:**
- App lifecycle management (pause/focus)
- Offline progress calculation
- Progress application on login
- Save/load coordination

**Key Methods:**
```csharp
// Calculate offline progress
OfflineProgressResult result = IdleManager.Instance.CalculateOfflineProgress();

// Apply calculated progress
IdleManager.Instance.ApplyOfflineProgress(result);

// Get/set logout time
DateTime logoutTime = IdleManager.Instance.GetLastLogoutTime();
IdleManager.Instance.SetLastLogoutTime(DateTime.UtcNow);

// Get max offline time (4 hours)
TimeSpan maxOffline = IdleManager.Instance.GetMaxOfflineTime();
```

---

### 2. ProductionManager.cs
**Purpose:** Manages resource generation from production lines

**Key Methods:**
```csharp
// Add a production line
var line = ProductionManager.Instance.AddProductionLine("drone_v1", 100f);

// Get money generation rate
float perMinute = ProductionManager.Instance.GetMoneyPerMinute();
float perSecond = ProductionManager.Instance.GetMoneyPerSecond();

// Calculate offline earnings
float earned = ProductionManager.Instance.CalculateMoneyEarned(TimeSpan.FromHours(2));

// Get production breakdown
var contributions = ProductionManager.Instance.CalculateOfflineProduction(TimeSpan.FromHours(2));
foreach (var kvp in contributions)
{
    Debug.Log($"Line {kvp.Key}: {kvp.Value:F0} money");
}
```

---

### 3. TimestampManager.cs
**Purpose:** Reliable time tracking with anti-cheat detection

**Anti-Cheat Features:**
- Detects device time rollback (cheating)
- Validates using `Time.realtimeSinceStartup`
- Tracks rollback count
- Flags repeated offenders

**Key Methods:**
```csharp
// Get validated current time
DateTime now = TimestampManager.Instance.GetCurrentTime();

// Check for rollback
bool rollback = TimestampManager.Instance.DetectTimeRollback();
int count = TimestampManager.Instance.GetRollbackCount();

// Get safe offline duration
TimeSpan duration = TimestampManager.Instance.GetSafeOfflineDuration(lastLogout);
```

---

### 4. OfflineProgressResult.cs
**Purpose:** Data structure for calculated offline gains

**Properties:**
```csharp
public class OfflineProgressResult
{
    public TimeSpan offlineDuration;        // Total time offline
    public float cappedDurationMinutes;     // Time used for calculation
    public float moneyEarned;               // Total money earned
    public Dictionary<string, float> researchProgress;  // Research progress added
    public List<string> completedResearches;            // Completed research IDs
    public bool timeRollbackDetected;       // Anti-cheat flag
    public bool wasCapped;                  // Was time capped
}
```

---

## How Offline Calculation Works

### Flow Diagram:

```
┌─────────────────┐
│   App Opens     │
└────────┬────────┘
         │
         ▼
┌─────────────────────────┐
│ Load Last Logout Time   │
│ from Save Data          │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Validate Timestamp      │
│ (Anti-cheat check)      │
└────────┬────────────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌────────┐  ┌─────────────┐
│ Valid  │  │ Rollback    │
│ Time   │  │ Detected    │
└───┬────┘  └──────┬──────┘
    │              │
    ▼              ▼
┌─────────────────────────┐  ┌─────────────────────────┐
│ Calculate Duration      │  │ Zero/Cap Progress       │
│ (Apply 4-hour max)      │  │ (Based on settings)     │
└────────┬────────────────┘  └────────┬────────────────┘
         │                            │
         └────────────┬───────────────┘
                      │
                      ▼
         ┌────────────────────────┐
         │ Calculate Production   │
         │ Money = Rate × Time    │
         └────────┬───────────────┘
                  │
                  ▼
         ┌────────────────────────┐
         │ Calculate Research     │
         │ Progress               │
         └────────┬───────────────┘
                  │
                  ▼
         ┌────────────────────────┐
         │ Apply to Game State    │
         │ (Money, Research)      │
         └────────┬───────────────┘
                  │
                  ▼
         ┌────────────────────────┐
         │ Show Offline Popup     │
         │ (If enabled)           │
         └────────────────────────┘
```

### Calculation Formula:

```
Money Earned = Σ(ProductionLine_Rate × Duration × Multipliers)

Where:
- ProductionLine_Rate = baseRate × efficiency
- Duration = min(ActualOfflineTime, MaxOfflineTime)
- Multipliers = globalMultiplier × premiumMultiplier
```

---

## Login Progress Restoration Example

```csharp
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private OfflineProgressPopup popupPrefab;
    
    private void Start()
    {
        // Subscribe to offline progress events
        IdleManager.Instance.OnOfflineProgressCalculated += OnOfflineCalculated;
        IdleManager.Instance.OnOfflineProgressApplied += OnOfflineApplied;
        
        // The idle manager automatically calculates on start
        // But you can also trigger manually:
        // IdleManager.Instance.CalculateAndApplyOfflineProgress();
    }
    
    private void OnOfflineCalculated(OfflineProgressResult result)
    {
        Debug.Log($"Offline for {result.GetFormattedDuration()}");
        Debug.Log($"Earned: {result.moneyEarned:F0} money");
        Debug.Log($"Researches completed: {result.GetCompletedResearchesSummary()}");
        
        if (result.timeRollbackDetected)
        {
            Debug.LogWarning("Time rollback detected!");
        }
    }
    
    private void OnOfflineApplied(OfflineProgressResult result)
    {
        // Show popup with results
        if (result.HasProgress())
        {
            var popup = Instantiate(popupPrefab);
            popup.Show(result);
        }
        
        // Update UI
        UIManager.Instance.UpdateMoneyDisplay();
        ResearchPanel.Instance.Refresh();
    }
}

// Example Offline Progress Popup
public class OfflineProgressPopup : MonoBehaviour
{
    [SerializeField] private Text durationText;
    [SerializeField] private Text moneyText;
    [SerializeField] private Text researchText;
    [SerializeField] private GameObject cappedIndicator;
    
    public void Show(OfflineProgressResult result)
    {
        durationText.text = $"You were away for:\n{result.GetFormattedDuration()}";
        
        if (result.wasCapped)
        {
            durationText.text += $"\n(Capped to {result.cappedDurationMinutes:F0} min)";
            cappedIndicator.SetActive(true);
        }
        
        moneyText.text = $"Money Earned:\n${result.moneyEarned:N0}";
        
        if (result.completedResearches.Count > 0)
        {
            researchText.text = $"Researches Completed: {result.completedResearches.Count}";
            researchText.gameObject.SetActive(true);
        }
        else
        {
            researchText.gameObject.SetActive(false);
        }
        
        gameObject.SetActive(true);
    }
}
```

---

## Edge Case Handling

### 1. Device Time Changed Backwards (Cheating)

**Detection:**
- Compares current time with last saved time
- Uses `Time.realtimeSinceStartup` for validation
- Tracks rollback count

**Response:**
```csharp
// Option 1: Zero all progress (strict)
if (zeroProgressOnRollback)
    return OfflineProgressResult.CreateRollbackDetected(logout, login);

// Option 2: Cap at minimum (lenient)
if (capProgressOnRollback)
    offlineDuration = TimeSpan.FromMinutes(30);
```

**Configuration:**
```csharp
[SerializeField] private bool zeroProgressOnRollback = true;
[SerializeField] private bool capProgressOnRollback = false;
```

---

### 2. Device Time Changed Forwards (Bonus)

**Detection:**
- Calculates expected duration
- If duration exceeds max offline time, cap it

**Response:**
```csharp
TimeSpan maxOffline = TimeSpan.FromHours(4);
if (offlineDuration > maxOffline)
{
    result.wasCapped = true;
    offlineDuration = maxOffline;
}
```

---

### 3. App Killed Without Proper Shutdown

**Handling:**
- Auto-save every 30 seconds during gameplay
- Save on every significant event (purchase, upgrade)
- On next launch, use last saved logout time
- If no logout time exists, skip offline calculation

```csharp
// Auto-save timer
private void Update()
{
    _saveTimer += Time.deltaTime;
    if (_saveTimer >= SAVE_INTERVAL_SECONDS)
    {
        _saveTimer = 0;
        SaveCurrentState();
    }
}
```

---

### 4. Very Long Offline Time

**Handling:**
- Cap at 4 hours (configurable)
- Mark result as capped
- Show indicator in UI

```csharp
public TimeSpan GetMaxOfflineTime()
{
    return TimeSpan.FromHours(maxOfflineHours); // 4 hours
}
```

---

### 5. Multiple Rapid Open/Close

**Handling:**
- Minimum 1 minute threshold for offline calculation
- Prevents spam and unnecessary calculations

```csharp
private void DelayedOfflineCheck()
{
    TimeSpan sinceLastLogout = DateTime.UtcNow - _lastLogoutTime;
    if (sinceLastLogout.TotalMinutes > 1)
    {
        CalculateAndApplyOfflineProgress();
    }
}
```

---

## Save Data Structure

```csharp
[Serializable]
public class IdleSaveData
{
    public int version;                          // Migration support
    public DateTime lastLogoutTime;              // For offline calc
    public DateTime lastSaveTime;                // For debugging
    public DateTime firstInstallTime;            // Analytics
    
    public int totalSessions;                    // Stats
    public float totalPlayTimeMinutes;           // Stats
    public float totalOfflineTimeMinutes;        // Stats
    
    public List<ProductionLineSaveData> productionLines;
    public float globalProductionMultiplier;
    public float premiumMultiplier;
    
    public Dictionary<string, ResearchProgressData> researchProgressAtLogout;
    public Dictionary<string, float> partialProgressValues;
    
    public int detectedRollbackCount;            // Anti-cheat
    public bool wasFlaggedForCheating;           // Anti-cheat
    
    public bool offlineProgressEnabled;          // Settings
    public float maxOfflineHours;                // Settings
    
    public float totalOfflineEarnings;           // Stats
    public int offlineSessionsCount;             // Stats
}
```

---

## Testing & Debugging

### Debug Commands:

```csharp
// Simulate offline period (for testing)
IdleManager.Instance.SimulateOfflinePeriod(2.5f); // 2.5 hours

// Get debug info
Debug.Log(IdleManager.Instance.GetDebugInfo());
Debug.Log(TimestampManager.Instance.GetDebugInfo());
Debug.Log(ProductionManager.Instance.GetDebugInfo());

// Clear all data
IdleManager.Instance.ClearAllData();

// Reset anti-cheat
TimestampManager.Instance.ResetAntiCheatState();
```

### Test Scenarios:

1. **Normal Offline:** Close app for 30 minutes, reopen
2. **Max Cap:** Close app for 6 hours, verify 4-hour cap
3. **Time Rollback:** Change device time back 1 hour
4. **Time Forward:** Change device time forward 1 hour
5. **Rapid Toggle:** Open/close app multiple times quickly
6. **Kill App:** Force kill without proper shutdown

---

## Integration Checklist

- [ ] Add `IdleManager` to persistent scene
- [ ] Add `ProductionManager` to persistent scene
- [ ] Add `TimestampManager` to persistent scene
- [ ] Create offline progress popup UI
- [ ] Connect `EconomyManager` to receive money
- [ ] Connect `ResearchManager` to receive progress
- [ ] Configure max offline time (default: 4 hours)
- [ ] Set anti-cheat preferences
- [ ] Test all edge cases
- [ ] Add analytics tracking

---

## Performance Considerations

1. **Save Frequency:** Auto-save every 30 seconds (not every frame)
2. **Calculation:** Only calculate offline progress once per session
3. **Partial Progress:** Stored as float (0-1) to avoid precision loss
4. **Dictionary Usage:** Fast lookups for research progress
5. **Event-Driven:** Use events instead of polling for updates

---

## Future Enhancements

1. **Server Time:** Optional server time validation
2. **Cloud Save:** Sync offline progress across devices
3. **Push Notifications:** Notify when research completes offline
4. **Offline Boosters:** Premium items to increase offline time
5. **Offline Graphs:** Show production breakdown over time
