# Project Aegis: Drone Dominion - Core Architecture

## Overview

This is the foundational architecture for **Project Aegis: Drone Dominion**, a mobile idle strategy tycoon game focused on drone manufacturing, research & development, and contract management.

## Architecture Philosophy

The architecture follows **SOLID principles** with:
- **Single Responsibility**: Each class has one clear purpose
- **Open/Closed**: Extensible through interfaces and base classes
- **Liskov Substitution**: Derived classes can substitute base classes
- **Interface Segregation**: Small, focused interfaces
- **Dependency Inversion**: Depend on abstractions, not concrete implementations

## Folder Structure

```
ProjectAegis/
├── Scripts/
│   ├── Core/           # Managers, events, services, base classes
│   ├── Systems/        # Game systems (Research, Idle, Contracts, etc.)
│   ├── Data/           # Data models and ScriptableObjects
│   ├── UI/             # UI controllers and views
│   └── Utils/          # Helpers and extensions
├── ScriptableObjects/
│   ├── Research/       # Research project definitions
│   ├── Technologies/   # Technology definitions
│   ├── Drones/         # Drone type definitions
│   └── Events/         # Game event definitions
├── Prefabs/            # Game prefabs
├── Scenes/             # Game scenes
└── Resources/          # Runtime-loaded assets
```

## Core Managers

### 1. GameManager
**Purpose**: Main controller and coordinator
**Responsibilities**:
- Initialize all systems in correct order
- Handle application pause/focus/quit
- Coordinate save/load operations
- Manage game state transitions
- Calculate offline progress

**Initialization Order**:
1. Core Managers (EventManager, ServiceLocator, TimeManager)
2. Discover and register systems
3. Initialize all systems
4. Post-initialization setup
5. Load saved game or start new
6. Finalize and enter MainMenu state

### 2. EventManager
**Purpose**: Central event system for decoupled communication
**Features**:
- Type-safe C# events for all game events
- Safe invocation with null checks
- Generic event system for custom events
- Comprehensive event categories:
  - Resource events (Money, Reputation, ResearchPoints, Energy, Materials)
  - Research events (Started, Progress, Completed, Cancelled)
  - Drone events (Unlocked, Produced, Sold, Test results)
  - Contract events (Available, Bid, Won, Lost, Completed, Failed)
  - Risk events (Triggered, Resolved, Risk level changes)
  - Game state events (State changes, Save/Load, Milestones)

**Usage Example**:
```csharp
// Subscribe to an event
EventManager.Instance.OnResearchCompleted += OnResearchCompleted;

// Trigger an event
EventManager.Instance.TriggerResearchCompleted(researchId, researchName);

// Unsubscribe
EventManager.Instance.OnResearchCompleted -= OnResearchCompleted;
```

### 3. ServiceLocator
**Purpose**: Service provider pattern for dependency management
**Features**:
- Type-safe service registration and retrieval
- Lazy initialization support
- Singleton and transient service support
- Service metadata tracking

**Usage Example**:
```csharp
// Register a service
ServiceLocator.Instance.Register<IResearchSystem>(researchSystem);

// Retrieve a service
var researchSystem = ServiceLocator.Instance.Get<IResearchSystem>();

// Try-get pattern
if (ServiceLocator.Instance.TryGet<IResearchSystem>(out var system))
{
    // Use system
}
```

### 4. TimeManager
**Purpose**: Centralized time handling
**Features**:
- Real time vs game time management
- Pause functionality
- Time scale control for debugging
- Offline progress calculation
- Time formatting utilities

**Usage Example**:
```csharp
// Get adjusted delta time
float deltaTime = TimeManager.Instance.DeltaTime;

// Pause/Resume
TimeManager.Instance.Pause();
TimeManager.Instance.Resume();

// Set debug time scale
TimeManager.Instance.SetDebugTimeScale(10f); // 10x speed

// Calculate offline progress
var offlineData = TimeManager.Instance.CalculateOfflineProgress();
```

### 5. SaveManager
**Purpose**: Handles game save and load operations
**Features**:
- Multiple save slots
- Auto-save rotation
- Optional encryption
- Save metadata tracking
- Cloud save integration hooks

## Base Classes

### BaseManager<T>
**Purpose**: Generic singleton pattern for managers
**Features**:
- Thread-safe instance creation
- Optional scene persistence
- Initialization lifecycle
- Logging utilities

### BaseSystem
**Purpose**: Foundation for all game systems
**Features**:
- Initialization lifecycle (Initialize, PostInitialize)
- Activation lifecycle (Activate, Deactivate)
- Pause/Resume support
- Update loop with Tick
- Proper cleanup with Dispose

### BaseScriptableObject
**Purpose**: Foundation for all game data assets
**Features**:
- Automatic ID generation
- Display information (name, description, icon)
- Category and tag support
- Version tracking
- Validation hooks

## Core Interfaces

### IInitializable
```csharp
public interface IInitializable
{
    bool IsInitialized { get; }
    void Initialize();
    void PostInitialize();
}
```

### ISaveable
```csharp
public interface ISaveable
{
    string SaveKey { get; }
    int SaveVersion { get; }
    object CaptureState();
    void RestoreState(object state);
}
```

### IProgressable
```csharp
public interface IProgressable
{
    float CurrentProgress { get; }
    float TotalProgressRequired { get; }
    float NormalizedProgress { get; }
    bool IsProgressing { get; }
    bool IsComplete { get; }
    void AdvanceProgress(float amount);
    event Action OnProgressComplete;
    event Action<float> OnProgressChanged;
}
```

### ITickable
```csharp
public interface ITickable
{
    void Tick(float deltaTime);
}
```

### IPausable
```csharp
public interface IPausable
{
    bool IsPaused { get; }
    void Pause();
    void Resume();
}
```

## Data Models

### PlayerData
**Purpose**: Core player progression and resource data
**Contains**:
- Resources (Money, ResearchPoints, Reputation, Energy, Materials)
- Lifetime statistics
- Session data
- Unlock tracking (Drones, Technologies, Research, Contracts)
- Event notifications for resource changes

### ResearchProgress
**Purpose**: Active research project state
**Features**:
- Progress tracking with IProgressable
- Time-based advancement
- Queue management
- Save/load support

### GameStateData
**Purpose**: Overall game state for persistence
**Contains**:
- Current game state
- Research state
- Production line states
- Contract states
- Risk levels

## Game Systems (Placeholder)

### ResearchSystem
Manages research queue, progress tracking, and technology unlocking.

### IdleSystem
Handles offline progress calculation and passive resource generation.

### ContractSystem (To be implemented)
Manages contract availability, bidding, and fulfillment.

### RiskSystem (To be implemented)
Tracks and manages reputation and financial risks.

### ProductionSystem (To be implemented)
Manages drone manufacturing and production lines.

## Utility Classes

### GameLogger
Centralized logging with categories and log levels.

### UnityExtensions
Extension methods for common Unity operations:
- Transform utilities
- Vector operations
- Color manipulation
- Number formatting
- List utilities

## Initialization Order

```
1. GameManager.Awake()
2. BaseManager singleton setup
3. GameManager.Start() -> InitializeGameSequence()
   a. InitializeCoreManagers()
      - EventManager (priority 0)
      - ServiceLocator (priority 1)
      - TimeManager (priority 2)
   b. DiscoverAndRegisterSystems()
   c. InitializeSystems()
   d. PostInitialization()
   e. LoadOrStartNew()
   f. Finalize (IsGameReady = true)
```

## Event System Usage Patterns

### Subscribing to Events
```csharp
public class MyClass : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Instance.OnMoneyChanged += HandleMoneyChanged;
    }
    
    private void OnDisable()
    {
        EventManager.Instance.OnMoneyChanged -= HandleMoneyChanged;
    }
    
    private void HandleMoneyChanged(long newAmount, long changeAmount, string reason)
    {
        // Handle event
    }
}
```

### Creating Custom Events
```csharp
// Define event in EventManager
public event Action<string, int> OnCustomEvent;

// Trigger event
public void TriggerCustomEvent(string id, int value)
{
    SafeInvoke(OnCustomEvent, id, value, "CustomEvent");
}
```

## Best Practices

1. **Always unsubscribe from events** in OnDisable/OnDestroy
2. **Use ServiceLocator** for cross-system dependencies
3. **Implement ISaveable** for any data that should persist
4. **Use BaseSystem** for new game systems
5. **Log with context** using the provided logging utilities
6. **Handle null checks** when accessing singleton instances
7. **Use TimeManager.DeltaTime** for time-based calculations
8. **Apply offline progress** through IdleSystem

## File List

### Core Scripts
- `/Scripts/Core/GameManager.cs`
- `/Scripts/Core/EventManager.cs`
- `/Scripts/Core/ServiceLocator.cs`
- `/Scripts/Core/TimeManager.cs`
- `/Scripts/Core/SaveManager.cs`
- `/Scripts/Core/Interfaces.cs`
- `/Scripts/Core/BaseManager.cs`
- `/Scripts/Core/BaseScriptableObject.cs`
- `/Scripts/Core/BaseSystem.cs`

### Data Scripts
- `/Scripts/Data/PlayerData.cs`
- `/Scripts/Data/ResearchProgress.cs`
- `/Scripts/Data/GameStateData.cs`

### System Scripts
- `/Scripts/Systems/ResearchSystem.cs`
- `/Scripts/Systems/IdleSystem.cs`

### Utility Scripts
- `/Scripts/Utils/GameLogger.cs`
- `/Scripts/Utils/UnityExtensions.cs`

## Next Steps

1. Implement ContractSystem for contract bidding and management
2. Implement RiskSystem for reputation and financial risk
3. Implement ProductionSystem for drone manufacturing
4. Create UI controllers for each game screen
5. Define ScriptableObject assets for research, drones, and technologies
6. Implement save/load UI and cloud save integration
7. Add analytics and telemetry hooks
8. Create tutorial system

---

**Version**: 1.0.0  
**Unity Version**: 2022.3 LTS  
**Platform**: Android
