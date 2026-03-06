# Project Aegis: Research System Documentation

## Overview

The Research System is the core gameplay loop for "Project Aegis: Drone Dominion". It provides:

- **Technology Tree**: Hierarchical research progression
- **Research Queue**: Manage multiple research projects
- **Offline Progress**: Calculates progress while game is closed
- **Save/Load**: Full persistence support
- **Event System**: Integration with other game systems

## Architecture

```
ResearchSystem/
├── ResearchData (ScriptableObject)
│   └── Static research definition
├── TechnologyTreeData (ScriptableObject)
│   └── Container for all researches and connections
├── ResearchProgress (Runtime)
│   └── Tracks active research progress
├── ResearchManager (MonoBehaviour)
│   └── Main controller for research operations
└── TechTreeManager (MonoBehaviour)
    └── Tree navigation and visualization data
```

## Quick Start

### 1. Setup in Unity

1. Create a new GameObject in your scene: `GameObject -> Create Empty`
2. Name it "ResearchSystem"
3. Add the following components:
   - `ResearchManager`
   - `TechTreeManager`

4. Create the Energy Systems Tech Tree asset:
   - Menu: `Project Aegis -> Research -> Create Energy Systems Tech Tree`

5. Assign the Tech Tree to ResearchManager:
   - Select ResearchSystem GameObject
   - Drag "EnergySystemsTechTree" asset to "Technology Tree" field

### 2. Basic Usage

```csharp
using ProjectAegis.Systems.Research;

public class ResearchExample : MonoBehaviour
{
    void Start()
    {
        // Subscribe to events
        ResearchManager.Instance.OnResearchStarted += OnResearchStarted;
        ResearchManager.Instance.OnResearchCompleted += OnResearchCompleted;
        ResearchManager.Instance.OnResearchProgressUpdated += OnProgressUpdated;

        // Start research
        ResearchManager.Instance.StartResearch("energy_basic_power_cell");
    }

    void OnResearchStarted(string researchId)
    {
        Debug.Log($"Started research: {researchId}");
    }

    void OnResearchCompleted(string researchId)
    {
        Debug.Log($"Completed research: {researchId}");
    }

    void OnProgressUpdated(string researchId, float progress)
    {
        Debug.Log($"Progress: {progress:P0}");
    }
}
```

## API Reference

### ResearchManager

#### Starting Research
```csharp
// Start a research project
bool success = ResearchManager.Instance.StartResearch("energy_basic_power_cell");

// Cancel active research (with 50% refund)
bool cancelled = ResearchManager.Instance.CancelResearch(refundPercentage: 0.5f);

// Skip research (instant completion)
bool skipped = ResearchManager.Instance.SkipResearch();
```

#### Querying State
```csharp
// Check if research is active
bool isResearching = ResearchManager.Instance.IsResearching();

// Get current progress (0-1)
float progress = ResearchManager.Instance.GetProgress();

// Get time remaining
TimeSpan remaining = ResearchManager.Instance.GetTimeRemaining();
string remainingString = ResearchManager.Instance.GetTimeRemainingString(); // "5m 30s"

// Get active research ID
string activeId = ResearchManager.Instance.GetActiveResearchId();

// Get active research data
ResearchData activeResearch = ResearchManager.Instance.GetActiveResearchData();
```

#### Checking Availability
```csharp
// Check if research can be started (prerequisites met)
bool canResearch = ResearchManager.Instance.CanResearch("energy_efficient_cooling");

// Get all available researches
List<ResearchData> available = ResearchManager.Instance.GetAvailableResearch();

// Get available by category
List<ResearchData> energyResearch = ResearchManager.Instance.GetAvailableResearchByCategory(TechCategory.EnergySystems);

// Check if research is completed
bool isCompleted = ResearchManager.Instance.IsResearchCompleted("energy_basic_power_cell");

// Get all completed research
List<string> completedIds = ResearchManager.Instance.GetCompletedResearch();
List<ResearchData> completedData = ResearchManager.Instance.GetCompletedResearchData();
```

#### Queue Management
```csharp
// Get queue count
int queueSize = ResearchManager.Instance.GetQueueCount();

// Get queue contents
List<ResearchData> queue = ResearchManager.Instance.GetResearchQueue();
```

### TechTreeManager

#### Node Queries
```csharp
// Get a specific node
TechTreeNode node = TechTreeManager.Instance.GetNode("energy_basic_power_cell");

// Get all nodes
List<TechTreeNode> allNodes = TechTreeManager.Instance.GetAllNodes();

// Get nodes by category
List<TechTreeNode> energyNodes = TechTreeManager.Instance.GetNodesByCategory(TechCategory.EnergySystems);

// Get nodes by state
List<TechTreeNode> availableNodes = TechTreeManager.Instance.GetNodesByState(TechNodeState.Available);
```

#### Validation
```csharp
// Check if prerequisites are met
bool prereqsMet = TechTreeManager.Instance.ArePrerequisitesMet("energy_advanced_capacitors");

// Get unmet prerequisites
List<ResearchData> unmet = TechTreeManager.Instance.GetUnmetPrerequisites("energy_advanced_capacitors");

// Check if category is unlocked
bool unlocked = TechTreeManager.Instance.IsCategoryUnlocked(TechCategory.AISystems);
```

#### Visual Data
```csharp
// Get tree layers (for rendering)
List<List<TechTreeNode>> layers = TechTreeManager.Instance.GetTreeLayers();

// Get connections (for line drawing)
var connections = TechTreeManager.Instance.GetConnections();
foreach (var (from, to, type) in connections)
{
    // Draw line from -> to
}
```

#### Path Finding
```csharp
// Get research path
List<string> path = TechTreeManager.Instance.GetResearchPath("energy_basic_power_cell", "energy_quantum_energy_core");
// Returns: ["energy_basic_power_cell", "energy_efficient_cooling", "energy_advanced_capacitors", ...]

// Get all required researches for a target
List<string> required = TechTreeManager.Instance.GetRequiredResearchesFor("energy_quantum_energy_core");
```

## Events

### ResearchManager Events
```csharp
// Research started
ResearchManager.Instance.OnResearchStarted += (researchId) => { };

// Research completed
ResearchManager.Instance.OnResearchCompleted += (researchId) => { };

// Progress updated
ResearchManager.Instance.OnResearchProgressUpdated += (researchId, progress) => { };

// Research cancelled
ResearchManager.Instance.OnResearchCancelled += (researchId) => { };

// Module unlocked
ResearchManager.Instance.OnModuleUnlocked += (moduleId) => { };

// New research available
ResearchManager.Instance.OnResearchAvailable += (researchId) => { };
```

## Creating Custom Research

### 1. Create Research Data Asset

```csharp
using ProjectAegis.Systems.Research;
using UnityEngine;

[CreateAssetMenu(fileName = "MyResearch", menuName = "Project Aegis/Research/Custom Research")]
public class MyResearchData : ResearchData { }
```

### 2. Configure in Inspector

```yaml
Research ID: my_custom_research
Display Name: Custom Research
Description: This is my custom research
Cost: 1000
Duration Minutes: 30
Risk Level: Medium
Prerequisite IDs:
  - energy_basic_power_cell
  - energy_efficient_cooling
Tech Category: EnergySystems
Unlocks Modules:
  - module_custom_part
Unlocks Research:
  - my_advanced_research
Icon: [Assign Sprite]
```

### 3. Add to Tech Tree

```csharp
// In your TechTreeData asset, add to allResearches list
// Add connections as needed
techTree.connections.Add(new TechNodeConnection
{
    fromResearchId = "energy_efficient_cooling",
    toResearchId = "my_custom_research",
    connectionType = TechNodeConnection.ConnectionType.Required
});
```

## Integration Points

### Economy System
```csharp
// When starting research, check and deduct cost
public bool TryStartResearch(string researchId)
{
    var research = GetResearch(researchId);
    
    if (EconomyManager.Instance.GetCredits() >= research.cost)
    {
        EconomyManager.Instance.DeductCredits(research.cost);
        return ResearchManager.Instance.StartResearch(researchId);
    }
    
    return false;
}

// When cancelling, refund credits
ResearchManager.Instance.OnResearchCancelled += (researchId) =>
{
    var research = GetResearch(researchId);
    float refund = research.cost * 0.5f;
    EconomyManager.Instance.AddCredits(refund);
};
```

### Module System
```csharp
// When research completes, unlock modules
ResearchManager.Instance.OnModuleUnlocked += (moduleId) =>
{
    ModuleManager.Instance.UnlockModule(moduleId);
    UIManager.Instance.ShowUnlockNotification(moduleId);
};
```

### Save System
```csharp
// ResearchManager auto-saves on application pause/quit
// Manual save/load:
ResearchManager.Instance.Save();
ResearchManager.Instance.Load();

// Custom save integration
public void SaveGame()
{
    ResearchManager.Instance.Save();
    // Save other systems...
}

public void LoadGame()
{
    ResearchManager.Instance.Load();
    // Load other systems...
}
```

### UI Integration
```csharp
public class ResearchUI : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI timeRemainingText;
    [SerializeField] private TextMeshProUGUI researchNameText;

    void Start()
    {
        ResearchManager.Instance.OnResearchStarted += OnResearchStarted;
        ResearchManager.Instance.OnResearchProgressUpdated += OnProgressUpdated;
    }

    void OnResearchStarted(string researchId)
    {
        var research = ResearchManager.Instance.GetActiveResearchData();
        researchNameText.text = research.displayName;
    }

    void OnProgressUpdated(string researchId, float progress)
    {
        progressSlider.value = progress;
        timeRemainingText.text = ResearchManager.Instance.GetTimeRemainingString();
    }
}
```

## Energy Systems Research Tree

| Research | Duration | Cost | Risk | Prerequisites |
|----------|----------|------|------|---------------|
| Basic Power Cell | 5 min | 100 | Low | None |
| Efficient Cooling | 10 min | 250 | Low | Basic Power Cell |
| Advanced Capacitors | 20 min | 600 | Medium | Efficient Cooling |
| Fusion Micro-Cell | 45 min | 1,500 | High | Advanced Capacitors |
| Quantum Energy Core | 90 min | 5,000 | Very High | Fusion Micro-Cell |

**Total**: 170 minutes, 7,450 credits

## File Structure

```
Assets/ProjectAegis/
├── Scripts/
│   └── Systems/
│       └── Research/
│           ├── ResearchData.cs
│           ├── TechnologyTreeData.cs
│           ├── ResearchProgress.cs
│           ├── ResearchManager.cs
│           ├── TechTreeManager.cs
│           └── Data/
│               ├── EnergySystemsResearchData.cs
│               └── EnergySystemsTechTree.cs
└── Data/
    └── Research/
        ├── EnergySystemsTechTree.asset
        └── EnergySystems/
            ├── energy_basic_power_cell.asset
            ├── energy_efficient_cooling.asset
            ├── energy_advanced_capacitors.asset
            ├── energy_fusion_micro_cell.asset
            └── energy_quantum_energy_core.asset
```

## Best Practices

1. **Always check CanResearch() before StartResearch()**
2. **Subscribe to events for UI updates rather than polling**
3. **Use TechTreeManager for tree visualization, ResearchManager for operations**
4. **Save progress regularly (auto-save on pause is built-in)**
5. **Validate tech tree in editor before building**

## Troubleshooting

### Research won't start
- Check `CanResearch()` returns true
- Verify research ID exists in tech tree
- Check if already completed/active/queued

### Progress not updating
- Ensure ResearchManager is in scene
- Check `updateInterval` setting
- Verify game is not paused

### Offline progress not working
- Check `_lastSaveTime` is being set
- Verify `ApplyOfflineProgress()` is called on load
- Ensure system clock is correct

## License

Part of Project Aegis: Drone Dominion - Internal Use Only
