# Project Aegis: Drone Dominion - Complete Documentation

## Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture Overview](#architecture-overview)
3. [System Interactions](#system-interactions)
4. [Manager Reference](#manager-reference)
5. [Data Structures](#data-structures)
6. [How to Extend](#how-to-extend)
7. [Common Tasks](#common-tasks)
8. [Troubleshooting](#troubleshooting)
9. [Quick Start Guide](#quick-start-guide)

---

## Project Overview

**Project Aegis: Drone Dominion** is an idle + deep strategy + R&D tycoon game for Android where players build a drone manufacturing empire through research, production, and contract fulfillment.

### Core Gameplay Loop
1. **Research** new technologies to unlock capabilities
2. **Produce** drones in your factory
3. **Test** prototypes to ensure quality
4. **Fulfill contracts** to earn money and reputation
5. **Expand** your operations and unlock new opportunities

### Key Features
- 15 interconnected manager systems
- Dynamic event system with choices
- Reputation-based progression
- Prototype testing mechanics
- Idle progression with offline gains
- Deep research tree
- Contract-based economy

---

## Architecture Overview

### Design Patterns
- **Singleton Pattern**: All managers use thread-safe singleton implementation
- **Observer Pattern**: Event-driven communication between systems
- **State Pattern**: Game state management through SaveManager
- **Factory Pattern**: Drone and contract creation

### Namespace Organization
```
ProjectAegis/
├── Managers/       # All game manager scripts
├── Data/           # Data structures and ScriptableObjects
├── ScriptableObjects/ # SO definitions
├── UI/             # UI components and controllers
├── Utils/          # Utility classes and helpers
└── ExampleData/    # JSON data for game content
```

### Core Principles
1. **Single Responsibility**: Each manager handles one domain
2. **Dependency Injection**: Managers reference each other through Instance properties
3. **Event-Driven**: Systems communicate via events, not direct calls
4. **Save-Aware**: All stateful systems support save/load

---

## System Interactions

### Initialization Order
```
1. SaveManager          - Persistence layer
2. TimeManager          - Game time control
3. EventManager         - Global event bus
4. PlayerDataManager    - Currency and resources
5. ResearchManager      - Active research tracking
6. TechTreeManager      - Technology unlocks
7. ReputationManager    - Company reputation
8. RiskManager          - Risk assessment
9. ProductionManager    - Production systems
10. IdleManager         - Idle progression
11. DynamicEventManager - Random events
12. ContractManager     - Contract handling
13. PrototypeTestingManager - Quality testing
14. DroneManager        - Drone management
15. UIManager           - User interface
```

### System Dependencies Diagram
```
                    ┌─────────────────┐
                    │  GameInitializer │
                    └────────┬────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
   ┌────▼────┐         ┌─────▼─────┐       ┌─────▼─────┐
   │SaveManager│         │TimeManager│       │EventManager│
   └────┬────┘         └─────┬─────┘       └─────┬─────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
   ┌────▼────┐         ┌─────▼─────┐       ┌─────▼─────┐
   │PlayerData│         │ Research  │       │   Tech    │
   │ Manager │         │  Manager  │       │  Manager  │
   └────┬────┘         └─────┬─────┘       └─────┬─────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
   ┌────▼────┐         ┌─────▼─────┐       ┌─────▼─────┐
   │Reputation│         │   Risk    │       │Production │
   │ Manager │         │  Manager  │       │  Manager  │
   └────┬────┘         └─────┬─────┘       └─────┬─────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
   ┌────▼────┐         ┌─────▼─────┐       ┌─────▼─────┐
   │  Idle   │         │  Dynamic  │       │ Contract  │
   │ Manager │         │  Events   │       │  Manager  │
   └────┬────┘         └─────┬─────┘       └─────┬─────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
   ┌────▼────┐         ┌─────▼─────┐       ┌─────▼─────┐
   │Prototype│         │   Drone   │       │    UI     │
   │ Testing │         │  Manager  │       │  Manager  │
   └─────────┘         └───────────┘       └───────────┘
```

### Event Flow Example: Contract Completion
```
1. ContractManager detects completion
2. Fires OnContractCompleted event
3. ReputationManager receives event → adds reputation
4. PlayerDataManager receives event → adds currency
5. EventManager broadcasts GameEventType.ContractComplete
6. UIManager updates display
7. SaveManager queues auto-save
```

---

## Manager Reference

### ReputationManager
**Purpose**: Manages company reputation affecting contracts and pricing

**Key Properties**:
- `Reputation` (float, 0-100)
- `CurrentLevel` (ReputationLevel)

**Key Methods**:
```csharp
void AddReputation(float amount, ReputationSource source)
void RemoveReputation(float amount, ReputationSource source)
float GetContractBonus()     // Returns percentage bonus
float GetPriceMultiplier()   // Returns price multiplier
```

**Events**:
- `OnReputationChanged`
- `OnReputationLevelChanged`
- `OnReputationGained`
- `OnReputationLost`

### PrototypeTestingManager
**Purpose**: Handles drone prototype testing before production

**Test Types**:
- `FlightTest`: Maneuverability, stability
- `SignalTest`: Range, interference resistance
- `BatteryStressTest`: Endurance, heat management

**Key Methods**:
```csharp
bool StartTest(string droneId, PrototypeTestType testType)
bool IsTestPassed(string droneId, PrototypeTestType testType)
bool AreAllTestsPassed(string droneId)
float GetPassChance(PrototypeTestType testType)
```

### DroneManager
**Purpose**: Manages drone unlocks, production, and inventory

**Key Properties**:
- `ProductionQueue`
- `Inventory`
- `CanAddToQueue`

**Key Methods**:
```csharp
bool UnlockDrone(string droneId)
bool QueueProduction(string droneId, int quantity)
bool DeployDrone(string droneId)
int GetAvailableCount(string droneId)
```

### GameInitializer
**Purpose**: Main entry point, initializes all systems

**Key Methods**:
```csharp
void SaveGame()
void LoadGame()
void NewGame()
T GetManager<T>()
```

---

## Data Structures

### DroneData (ScriptableObject)
```csharp
public class DroneData : ScriptableObject
{
    public string droneId;
    public string displayName;
    public DroneClass droneClass;
    public List<string> requiredTechnologies;
    public StatBlock baseStats;
    public float productionTime;
    public float productionCost;
    public float basePrice;
}
```

### StatBlock
```csharp
[Serializable]
public class StatBlock
{
    public int speed;
    public int maneuverability;
    public int stability;
    public int detectionRange;
    public int signalStrength;
    public int stealth;
    public int batteryCapacity;
    public int powerEfficiency;
    public int heatDissipation;
    public int armor;
    public int reliability;
    public int repairEase;
    public int cargoCapacity;
    public int sensorQuality;
    public int processingPower;
}
```

### ProductionOrder
```csharp
[Serializable]
public class ProductionOrder
{
    public string orderId;
    public string droneId;
    public int quantity;
    public float progress;
    public float timeRemaining;
    public float totalTime;
    public bool isPaused;
}
```

---

## How to Extend

### Adding a New Drone

1. **Create DroneData Asset**:
   ```csharp
   // Right-click in Project window
   // Create > Project Aegis > Drone Data
   ```

2. **Configure Properties**:
   - Set unique `droneId`
   - Define `baseStats`
   - List `requiredTechnologies`
   - Set production values

3. **Add to Resources**:
   - Place in `Assets/Resources/Drones/`

4. **Test**:
   ```csharp
   // In-game test
   DroneManager.Instance.UnlockDrone("your_drone_id");
   ```

### Adding a New Research

1. **Create ResearchData Asset**:
   ```csharp
   // Create > Project Aegis > Research Data
   ```

2. **Configure**:
   - Set prerequisites
   - Define costs
   - List unlocks

3. **Link to Tech Tree**:
   - Add to TechTreeManager's available nodes

### Adding a New Event

1. **Create GameEventData Asset**:
   ```csharp
   // Create > Project Aegis > Event Data
   ```

2. **Configure**:
   - Set trigger conditions
   - Define choices and outcomes
   - Set timing parameters

3. **Register**:
   ```csharp
   // In DynamicEventManager
   availableEvents.Add(yourEventData);
   ```

### Creating a Custom Manager

1. **Create Script**:
   ```csharp
   using UnityEngine;
   
   namespace ProjectAegis
   {
       public class CustomManager : BaseManager<CustomManager>
       {
           protected override void OnInitialize()
           {
               base.OnInitialize();
               // Your initialization code
           }
           
           protected override void OnSetupEventSubscriptions()
           {
               base.OnSetupEventSubscriptions();
               // Subscribe to events
           }
       }
   }
   ```

2. **Add to Initialization Order**:
   - Edit `GameInitializer.MANAGER_ORDER`

3. **Create Save Data**:
   ```csharp
   [Serializable]
   public class CustomManagerSaveData
   {
       // Your save data fields
   }
   ```

---

## Common Tasks

### Saving Game State
```csharp
// Automatic (recommended)
GameInitializer.Instance.SaveGame();

// Or through SaveManager
SaveManager.Instance.SaveGame();
```

### Loading Game State
```csharp
GameInitializer.Instance.LoadGame();
```

### Adding Currency
```csharp
PlayerDataManager.Instance.AddCurrency(1000f);
```

### Starting Research
```csharp
ResearchManager.Instance.StartResearch("research_id");
```

### Accepting Contract
```csharp
ContractManager.Instance.AcceptContract("contract_id");
```

### Deploying Drone
```csharp
DroneManager.Instance.DeployDrone("drone_id");
```

### Triggering Custom Event
```csharp
EventManager.Instance.TriggerEvent(
    GameEventType.CustomEvent,
    new EventContext { StringValue = "event_data" }
);
```

### Getting Manager Instance
```csharp
// Any manager
var manager = ReputationManager.Instance;

// Check if initialized
if (ReputationManager.HasInstance)
{
    // Safe to use
}
```

---

## Troubleshooting

### Common Issues and Solutions

#### Issue: Managers not initializing
**Symptoms**: NullReferenceException, missing functionality

**Solutions**:
1. Check GameInitializer is in scene
2. Verify GameInitializer has Managers GameObject assigned
3. Check console for compilation errors
4. Ensure all manager scripts are in correct namespace

#### Issue: Save/Load not working
**Symptoms**: Progress lost on restart

**Solutions**:
1. Check SaveManager initialization order
2. Verify save data classes are marked `[Serializable]`
3. Check file permissions on device
4. Review PlayerPrefs or file storage limits

#### Issue: Events not triggering
**Symptoms**: Expected events don't occur

**Solutions**:
1. Verify EventManager is initialized
2. Check event subscription in OnSetupEventSubscriptions
3. Ensure event IDs match exactly
4. Review trigger conditions (cooldown, prerequisites)

#### Issue: UI not updating
**Symptoms**: Display shows old values

**Solutions**:
1. Verify event subscriptions in UIManager
2. Check UI update methods are being called
3. Ensure Canvas is properly configured
4. Review Unity Event System setup

#### Issue: Production not progressing
**Symptoms**: Production queue stuck

**Solutions**:
1. Check TimeManager is running (not paused)
2. Verify production order isn't paused
3. Check for sufficient resources
4. Review IdleManager configuration

### Debug Commands
```csharp
// Add to console or debug UI

// Reset reputation
ReputationManager.Instance.LoadSaveData(new ReputationSaveData { reputation = 50f });

// Complete all research
foreach (var tech in TechTreeManager.Instance.AllTechnologies)
{
    TechTreeManager.Instance.UnlockTechnology(tech.nodeId);
}

// Unlock all drones
foreach (var drone in DroneManager.Instance.AllDrones)
{
    DroneManager.Instance.UnlockDrone(drone.droneId);
}

// Add test funds
PlayerDataManager.Instance.AddCurrency(100000f);
PlayerDataManager.Instance.AddResearchPoints(10000f);
```

### Performance Optimization

**Recommended Settings**:
- Target frame rate: 30 FPS (mobile)
- Max concurrent particle effects: 50
- UI update frequency: Every 0.5 seconds (not every frame)
- Save auto-save interval: 60 seconds
- Event check interval: 5 seconds

**Optimization Tips**:
1. Use object pooling for UI elements
2. Cache manager references
3. Batch UI updates
4. Use async/await for file operations
5. Profile with Unity Profiler regularly

---

## Quick Start Guide

### For New Developers

1. **Setup Unity Project**:
   - Unity 2022.3 LTS or newer
   - Android Build Support
   - .NET Standard 2.1

2. **Import Scripts**:
   - Copy all scripts to `Assets/Scripts/ProjectAegis/`
   - Ensure namespace consistency

3. **Create ScriptableObjects**:
   - Use ExampleData JSON files as reference
   - Create assets via Create menu

4. **Setup Main Scene**:
   - Follow MainScene_Setup.md
   - Add all required GameObjects
   - Configure managers

5. **Test**:
   - Enter Play Mode
   - Verify initialization completes
   - Test basic functionality

### For Content Creators

1. **Adding New Drones**:
   - Create DroneData asset
   - Configure stats and requirements
   - Add to Resources/Drones folder
   - Test in-game

2. **Creating Contracts**:
   - Create ContractData asset
   - Set difficulty and rewards
   - Define requirements
   - Test acceptance and completion

3. **Designing Events**:
   - Create GameEventData asset
   - Write compelling narrative
   - Balance rewards/penalties
   - Test trigger conditions

### For Modders

1. **Creating Mods**:
   - Create new ScriptableObject assets
   - Follow existing naming conventions
   - Place in Resources/Mods/

2. **Custom Events**:
   - Extend GameEventData
   - Add custom choice logic
   - Register with DynamicEventManager

---

## API Reference

### EventManager
```csharp
void TriggerEvent(GameEventType type, EventContext context)
void Subscribe(GameEventType type, Action<EventContext> handler)
void Unsubscribe(GameEventType type, Action<EventContext> handler)
```

### TimeManager
```csharp
void Pause()
void Resume()
void SetTimeScale(float scale)
float GetElapsedTime()
```

### PlayerDataManager
```csharp
void AddCurrency(float amount)
void SpendCurrency(float amount)
bool CanAfford(float amount)
void AddResearchPoints(float amount)
```

### TechTreeManager
```csharp
bool UnlockTechnology(string nodeId)
bool IsTechnologyUnlocked(string nodeId)
List<TechnologyNode> GetUnlockableTechnologies()
```

---

## Version History

### v1.0.0 - Initial Release
- All 15 managers implemented
- Complete save/load system
- Full event system
- 3 MVP drones
- 5 research nodes
- 3 contracts
- 6 dynamic events

---

## License

This project is proprietary and confidential.

---

## Contact

For support or questions, contact the development team.

---

**End of Documentation**
