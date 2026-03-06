# Project Aegis: Drone Dominion - Debug Tools

Comprehensive debugging and testing tools for the mobile idle strategy tycoon game.

## Table of Contents
1. [Overview](#overview)
2. [Setup](#setup)
3. [Debug Manager](#debug-manager)
4. [Debug Console UI](#debug-console-ui)
5. [Time Manipulation](#time-manipulation)
6. [Event Debugger](#event-debugger)
7. [Risk Simulator](#risk-simulator)
8. [Research Debugger](#research-debugger)
9. [Contract Debugger](#contract-debugger)
10. [Save Debugger](#save-debugger)
11. [Performance Monitor](#performance-monitor)
12. [Debug Commands](#debug-commands)
13. [Debug Gizmos](#debug-gizmos)
14. [Debug Shortcuts](#debug-shortcuts)
15. [Testing Scenarios](#testing-scenarios)

---

## Overview

The debug system provides comprehensive tools for testing and debugging Project Aegis: Drone Dominion. All tools are hidden from players but easily accessible to developers.

### Key Features
- Time manipulation (fast-forward, skip time)
- Cheat systems (money, reputation, resources)
- Manual event triggering
- Risk simulation with Monte Carlo analysis
- Research testing and manipulation
- Contract generation and management
- Save data inspection and modification
- Real-time performance monitoring
- Visual debugging gizmos
- Console commands
- Keyboard shortcuts

---

## Setup

### 1. Add Debug Manager to Scene
```csharp
// Add DebugManager to a persistent GameObject
GameObject debugObj = new GameObject("DebugManager");
DebugManager debugManager = debugObj.AddComponent<DebugManager>();
```

### 2. Create Debug Panel (Optional)
Use the DebugPanelBuilder to create a complete UI:
```csharp
// Attach DebugPanelBuilder to your Canvas
DebugPanelBuilder builder = canvas.AddComponent<DebugPanelBuilder>();
builder.createOnStart = true;
```

### 3. Enable Debug Mode
```csharp
DebugManager.Instance.enableDebugMode = true;
```

---

## Debug Manager

Main controller for all debug functionality.

### Key Properties
```csharp
public bool enableDebugMode = true;
public KeyCode debugToggleKey = KeyCode.BackQuote; // ` key
public float timeScale = 1f;
public List<float> timeScalePresets = new List<float> { 1f, 2f, 5f, 10f, 50f, 100f };
```

### Methods
```csharp
// Panel Control
DebugManager.Instance.ToggleDebugPanel();
DebugManager.Instance.ShowDebugPanel();
DebugManager.Instance.HideDebugPanel();

// Time Control
DebugManager.Instance.SetTimeScale(10f);
DebugManager.Instance.CycleTimeScale();
DebugManager.Instance.ResetTimeScale();

// Cheats
DebugManager.Instance.AddMoney(10000f);
DebugManager.Instance.SetMoney(50000f);
DebugManager.Instance.AddReputation(10f);
DebugManager.Instance.SetReputation(75f);

// Research
DebugManager.Instance.CompleteCurrentResearch();
DebugManager.Instance.CompleteAllResearch();
DebugManager.Instance.UnlockAllTechnologies();
DebugManager.Instance.ResetResearchProgress();

// Events
DebugManager.Instance.TriggerRandomEvent();
DebugManager.Instance.TriggerEvent("market_crash");
DebugManager.Instance.ClearActiveEffects();

// Contracts
DebugManager.Instance.GenerateTestContracts(5);
DebugManager.Instance.CompleteAllContracts();
DebugManager.Instance.WinAllBids();

// Save/Load
DebugManager.Instance.SaveGame();
DebugManager.Instance.LoadGame();
DebugManager.Instance.DeleteSave();
DebugManager.Instance.ResetAllProgress();

// Time Simulation
DebugManager.Instance.SimulateOfflineTime(24f); // hours
DebugManager.Instance.SimulateOfflineDays(7f);

// Logging
DebugManager.Instance.LogDebugAction("Custom message");
List<string> log = DebugManager.Instance.GetDebugLog();
DebugManager.Instance.ClearDebugLog();
```

---

## Debug Console UI

In-game console interface for debug operations.

### Features
- Collapsible sections for each system
- Real-time log display
- Interactive controls (sliders, buttons, dropdowns)
- Performance metrics display

### Sections
1. **Time Control** - Time scale slider, preset buttons, skip buttons
2. **Cheats** - Money, reputation, research points inputs
3. **Research** - Research dropdown, complete/unlock/reset buttons
4. **Events** - Event dropdown, trigger/clear buttons
5. **Contracts** - Generate, complete, win bids
6. **Risk** - Simulation controls
7. **Save** - Save/load/delete operations
8. **Performance** - FPS and memory display

---

## Time Manipulation

Advanced time control for testing time-based systems.

### Time Skip
```csharp
// Skip time
TimeManipulator.Instance.SkipMinutes(5f);
TimeManipulator.Instance.SkipHours(1f);
TimeManipulator.Instance.SkipDays(7f);

// Skip to specific times
TimeManipulator.Instance.SkipToNextDay();
TimeManipulator.Instance.SkipToNextWeek();
TimeManipulator.Instance.SkipToNextMonth();
```

### Time Scale
```csharp
// Set custom time scale
TimeManipulator.Instance.SetTimeScale(10f);
TimeManipulator.Instance.SetTimeScalePreset(2); // 5x
TimeManipulator.Instance.CycleTimeScale();
TimeManipulator.Instance.ResetTimeScale();

// Smooth transitions
TimeManipulator.Instance.smoothTransitions = true;
TimeManipulator.Instance.transitionDuration = 0.5f;
```

### Simulation
```csharp
// Simulate offline progression
TimeManipulator.Instance.SimulateOfflineTime(24f);

// Configure what to simulate
TimeManipulator.Instance.simulateResources = true;
TimeManipulator.Instance.simulateContracts = true;
TimeManipulator.Instance.simulateResearch = true;
TimeManipulator.Instance.simulateEvents = true;
```

---

## Event Debugger

Manual event triggering and testing system.

### Available Events
- Market Crash
- Tech Breakthrough
- Competitor Launch
- Regulatory Change
- Supply Shortage
- Reputation Boost
- Security Breach
- Natural Disaster
- Economic Boom
- Hack Attempt
- Key Employee Leaves
- Patent Infringement
- Viral Marketing
- Equipment Failure
- Data Corruption

### Usage
```csharp
// Trigger specific event
EventDebugger.Instance.TriggerEvent("market_crash");

// Trigger random event
EventDebugger.Instance.TriggerRandomEvent();

// Trigger with severity
EventDebugger.Instance.TriggerEvent(eventInfo, EventSeverity.Major);

// Chain events
StartCoroutine(EventDebugger.Instance.TriggerAllEvents());
StartCoroutine(EventDebugger.Instance.ChainRandomEvents(5));

// Clear effects
EventDebugger.Instance.ClearActiveEffects();

// Get event history
List<string> history = EventDebugger.Instance.GetEventHistory();
```

---

## Risk Simulator

Monte Carlo simulation for risk analysis.

### Risk Levels
- None (0)
- Very Low (1)
- Low (2)
- Medium (3)
- High (4)
- Very High (5)
- Critical (6)

### Usage
```csharp
// Set test parameters
RiskSimulator.Instance.testTechnicalRisk = RiskLevel.Medium;
RiskSimulator.Instance.testFinancialRisk = RiskLevel.High;
RiskSimulator.Instance.testSecurityRisk = RiskLevel.Low;
RiskSimulator.Instance.testReputation = 50f;
RiskSimulator.Instance.testTechLevel = 5f;

// Run single test
RiskSimulator.Instance.RunSingleTest();

// Run Monte Carlo simulation
RiskSimulator.Instance.RunMonteCarloSimulation(1000);

// Get results
RiskSimulationResult result = RiskSimulator.Instance.GetLastResult();
Debug.Log($"Success Rate: {result.successRate:F1}%");
Debug.Log($"Risk Score: {result.overallRiskScore:F0}/100");
Debug.Log($"VaR 95%: {result.valueAtRisk95:C}");

// Export results
string csv = result.ExportToCSV();
string json = result.ExportToJSON();
```

### Results
- Total runs
- Success/failure counts and rates
- Failure type distribution
- Consequence severity statistics
- Value at Risk (VaR) calculations
- Expected monetary value

---

## Research Debugger

Research system testing and manipulation.

### Usage
```csharp
// Complete research
ResearchDebugger.Instance.CompleteCurrentResearch();
ResearchDebugger.Instance.CompleteResearch("motor_eff_1");
ResearchDebugger.Instance.CompleteAllResearch();

// Unlock technologies
ResearchDebugger.Instance.UnlockAllTechnologies();

// Reset progress
ResearchDebugger.Instance.ResetResearchProgress();

// Queue management
ResearchDebugger.Instance.AddResearchToQueue("ai_navigation");
ResearchDebugger.Instance.ClearResearchQueue();

// Research points
ResearchDebugger.Instance.AddResearchPoints(1000f);
ResearchDebugger.Instance.SetResearchPoints(5000f);

// Get info
string current = ResearchDebugger.Instance.GetCurrentResearchId();
float progress = ResearchDebugger.Instance.GetCurrentResearchProgress();
int completed = ResearchDebugger.Instance.GetCompletedCount();
```

---

## Contract Debugger

Contract system testing and generation.

### Contract Types
- Surveillance
- Delivery
- Mapping
- Inspection
- Agriculture
- Security
- Research
- Photography
- Emergency
- Military

### Usage
```csharp
// Generate contracts
ContractDebugger.Instance.GenerateTestContracts(5);

// Manage bids
ContractDebugger.Instance.WinAllBids();
ContractDebugger.Instance.AutoResolveAllBids();

// Complete contracts
ContractDebugger.Instance.CompleteAllContracts();
ContractDebugger.Instance.FailAllContracts();

// Utilities
ContractDebugger.Instance.ExtendAllDeadlines();
ContractDebugger.Instance.DoubleAllRewards();
ContractDebugger.Instance.ResetContracts();

// Get data
var contracts = ContractDebugger.Instance.GetAllContracts();
var active = ContractDebugger.Instance.GetActiveContracts();
```

---

## Save Debugger

Save system testing and inspection.

### Usage
```csharp
// View save data
SaveDebugger.Instance.ShowSaveData();

// Create/Delete
SaveDebugger.Instance.CreateTestSave();
SaveDebugger.Instance.DeleteSave();

// Corruption testing
SaveDebugger.Instance.CorruptSave();

// Migration testing
SaveDebugger.Instance.TestMigration();

// Import/Export
SaveDebugger.Instance.ExportSaveToClipboard();
SaveDebugger.Instance.ImportSaveFromClipboard();

// Backup
SaveDebugger.Instance.BackupSave();
SaveDebugger.Instance.RestoreBackup();

// Quick operations
SaveDebugger.Instance.QuickSave();
SaveDebugger.Instance.QuickLoad();

// Info
bool exists = SaveDebugger.Instance.SaveExists();
long size = SaveDebugger.Instance.GetSaveFileSize();
string path = SaveDebugger.Instance.GetSavePath();
```

---

## Performance Monitor

Real-time performance tracking.

### Metrics
- FPS (current, average, min)
- Memory usage (current, average)
- GC collections
- Draw calls (editor only)
- Triangles/Vertices (editor only)
- Texture memory (editor only)

### Usage
```csharp
// Start monitoring
PerformanceMonitor.Instance.StartMonitoring();
PerformanceMonitor.Instance.SetVisibility(true);

// Get metrics
float fps = PerformanceMonitor.Instance.GetCurrentFPS();
float avgFps = PerformanceMonitor.Instance.GetAverageFPS();
long memory = PerformanceMonitor.Instance.GetCurrentMemory();

// Get history
float[] fpsHistory = PerformanceMonitor.Instance.GetFPSHistory();
long[] memHistory = PerformanceMonitor.Instance.GetMemoryHistory();

// Take snapshot
PerformanceSnapshot snapshot = PerformanceMonitor.Instance.TakeSnapshot();

// Get report
string report = PerformanceMonitor.Instance.GetPerformanceReport();
```

### Thresholds
```csharp
// Configure thresholds
PerformanceMonitor.Instance.poorFpsThreshold = 30f;
PerformanceMonitor.Instance.criticalFpsThreshold = 15f;
PerformanceMonitor.Instance.highMemoryThreshold = 512; // MB
PerformanceMonitor.Instance.criticalMemoryThreshold = 1024; // MB
```

---

## Debug Commands

Console-like command system.

### Available Commands
```
money.add <amount>          - Add money
money.set <amount>          - Set money
reputation.add <amount>     - Add reputation
reputation.set <amount>     - Set reputation
research.complete <id>      - Complete research
research.completeall        - Complete all research
research.unlockall          - Unlock all tech
research.reset              - Reset research
time.skip <hours>           - Skip time
time.skipdays <days>        - Skip days
time.scale <multiplier>     - Set time scale
time.reset                  - Reset time scale
event.trigger <id>          - Trigger event
event.random                - Trigger random event
event.clear                 - Clear event effects
contract.generate <count>   - Generate contracts
contract.completeall        - Complete all contracts
contract.winall             - Win all bids
save.create                 - Create test save
save.delete                 - Delete save
save.export                 - Export save
system.fps                  - Show FPS
system.memory               - Show memory
system.info                 - System info
debug.help                  - Show help
debug.log <message>         - Add log entry
debug.clear                 - Clear log
debug.reset                 - Reset all progress
```

### Usage
```csharp
// Execute command
string result = DebugCommands.Instance.ExecuteCommand("money.add 10000");

// Register custom command
DebugCommands.Instance.RegisterCommand(
    "custom.command",
    "Description",
    "custom.command <arg>",
    args => { /* implementation */ },
    new Type[] { typeof(float) }
);

// Get help
string help = DebugCommands.Instance.ExecuteCommand("debug.help");
```

---

## Debug Gizmos

Visual debugging in Scene and Game views.

### Visualization Types
```csharp
// Research
DebugGizmos.Instance.showResearchProgress = true;
DebugGizmos.Instance.showResearchConnections = true;

// Contracts
DebugGizmos.Instance.showContractDeadlines = true;
DebugGizmos.Instance.showContractDifficulty = true;

// Drones
DebugGizmos.Instance.showDronePaths = true;
DebugGizmos.Instance.showDroneSensors = true;
DebugGizmos.Instance.showDroneLinks = true;

// Facilities
DebugGizmos.Instance.showFacilityZones = true;
DebugGizmos.Instance.showFacilityConnections = true;

// Events
DebugGizmos.Instance.showEventAreas = true;
DebugGizmos.Instance.showEventRadius = true;

// Grid
DebugGizmos.Instance.showWorldGrid = true;
DebugGizmos.Instance.gridSize = 10f;
```

### Registration
```csharp
// Register objects for visualization
DebugGizmos.Instance.RegisterResearchNode("motor_eff_1", position, prerequisites);
DebugGizmos.Instance.RegisterContract("contract_1", position, 24f, 2, 5000f);
DebugGizmos.Instance.RegisterDrone("drone_1", position, sensorRange);
DebugGizmos.Instance.RegisterFacility("hangar", position, influenceRadius);
DebugGizmos.Instance.RegisterEvent("market_crash", position, radius, isPositive, intensity);

// Update progress
DebugGizmos.Instance.UpdateResearchProgress("motor_eff_1", 0.5f);
DebugGizmos.Instance.CompleteResearch("motor_eff_1");

// Clear
DebugGizmos.Instance.ClearEvents();
DebugGizmos.Instance.ClearAllData();
```

---

## Debug Shortcuts

Keyboard shortcuts for quick access.

### Default Shortcuts
| Key | Action | Modifier |
|-----|--------|----------|
| ` | Toggle Debug Panel | No |
| F1 | Cycle Time Scale | Shift |
| F2 | Add Money | Shift |
| F3 | Complete Current Research | Shift |
| F4 | Trigger Random Event | Shift |
| F5 | Quick Save | Shift |
| F6 | Quick Load | Shift |
| F7 | Toggle Performance Monitor | Shift |
| F8 | Toggle Gizmos | Shift |
| F9 | Reset All Progress | Shift |
| F12 | Screenshot | No |

### Custom Shortcuts
```csharp
// Add custom shortcut
DebugShortcuts.Instance.AddCustomShortcut(
    "Custom Action",
    "Does something custom",
    KeyCode.F10,
    true, // require modifier
    () => { /* action */ }
);

// Enable/Disable
DebugShortcuts.Instance.SetShortcutsEnabled(true);
DebugShortcuts.Instance.DisableShortcut("Reset Progress");

// Get help
string help = DebugShortcuts.Instance.GenerateHelpText();
```

---

## Testing Scenarios

### Scenario 1: New Player Experience
```csharp
// Reset everything
DebugManager.Instance.ResetAllProgress();

// Verify tutorial triggers
// Check initial resources
// Test first contract flow
```

### Scenario 2: Late Game Balance
```csharp
// Set up late game state
DebugManager.Instance.SetMoney(10000000);
DebugManager.Instance.SetReputation(95);
ResearchDebugger.Instance.CompleteAllResearch();
ContractDebugger.Instance.GenerateTestContracts(20);

// Test high-level contracts
// Verify research costs scale properly
// Check event frequency
```

### Scenario 3: Risk Analysis
```csharp
// Test different risk combinations
RiskSimulator.Instance.testTechnicalRisk = RiskLevel.High;
RiskSimulator.Instance.testFinancialRisk = RiskLevel.High;
RiskSimulator.Instance.testSecurityRisk = RiskLevel.High;
RiskSimulator.Instance.RunMonteCarloSimulation(10000);

// Analyze failure distribution
// Adjust game balance based on results
```

### Scenario 4: Time Skip Validation
```csharp
// Record state
float initialMoney = GetMoney();

// Skip time
TimeManipulator.Instance.SkipDays(7);

// Verify calculations
float expectedEarnings = CalculateExpectedEarnings(7);
Assert.AreEqual(initialMoney + expectedEarnings, GetMoney(), 0.01f);
```

### Scenario 5: Event Stress Test
```csharp
// Trigger all events
StartCoroutine(EventDebugger.Instance.TriggerAllEvents());

// Monitor performance
// Check for memory leaks
// Verify no exceptions
```

### Scenario 6: Save/Load Cycle
```csharp
// Create save
SaveDebugger.Instance.CreateTestSave();

// Make changes
DebugManager.Instance.AddMoney(50000);

// Save again
SaveDebugger.Instance.QuickSave();

// Load and verify
SaveDebugger.Instance.QuickLoad();
Assert.AreEqual(expectedMoney, GetMoney());
```

---

## Best Practices

1. **Always disable debug mode for release builds**
   ```csharp
   #if !UNITY_EDITOR
   DebugManager.Instance.enableDebugMode = false;
   #endif
   ```

2. **Use time scale carefully** - High values (>100x) may cause physics issues

3. **Monitor memory** - Frequent save/load operations can increase memory usage

4. **Log important actions** - Use `DebugManager.Instance.LogDebugAction()` for tracking

5. **Clean up after testing** - Reset progress when done testing

6. **Use gizmos sparingly** - Too many gizmos can impact editor performance

---

## Troubleshooting

### Debug panel not showing
- Check `enableDebugMode` is true
- Verify DebugManager exists in scene
- Check if panel is behind other UI elements

### Time scale not working
- Ensure `isPaused` is false
- Check if game uses `Time.unscaledDeltaTime` where appropriate

### Commands not executing
- Verify command name is correct
- Check parameter types match
- Review console for error messages

### Gizmos not appearing
- Ensure gizmo visibility is enabled in Scene view
- Check if object positions are valid
- Verify `OnDrawGizmos()` is being called

---

## File Structure

```
/Scripts/Debug/
├── DebugManager.cs           - Main debug controller
├── DebugConsoleUI.cs         - In-game console UI
├── DebugCommands.cs          - Console command system
├── DebugShortcuts.cs         - Keyboard shortcuts
├── DebugGizmos.cs            - Visual debugging
├── DebugPanelBuilder.cs      - UI builder helper
├── TimeManipulator.cs        - Time control
├── EventDebugger.cs          - Event testing
├── RiskSimulator.cs          - Risk simulation
├── RiskSimulationResult.cs   - Simulation results
├── ResearchDebugger.cs       - Research testing
├── ContractDebugger.cs       - Contract testing
├── SaveDebugger.cs           - Save testing
├── PerformanceMonitor.cs     - Performance tracking
└── README.md                 - This file
```

---

## Support

For issues or questions about the debug tools, contact the development team.
