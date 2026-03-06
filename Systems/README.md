# Project Aegis: Dynamic Event System

A complete random event system for idle strategy tycoon games with player interaction tracking, weighted random selection, and configurable event effects.

## Features

- **Random Timer System**: Configurable intervals (default 15-20 minutes)
- **Player Interaction Tracking**: Events only trigger if player interacted within 3 minutes
- **Weighted Random Selection**: Events have weights and rarity tiers
- **Effect System**: Stackable, timed effects with automatic expiration
- **Decision Events**: Events with multiple player choices
- **Save/Load Support**: Full persistence for timers, effects, and event history
- **Debug Visualization**: Optional on-screen debug info

## Core Components

### 1. DynamicEventManager
Main controller that manages the event timer and triggers events.

```csharp
// Start the event system
DynamicEventManager.Instance.StartEventTimer();

// Register player interactions
DynamicEventManager.Instance.RegisterPlayerInteraction(InteractionType.UIButton);

// Get time until next event
float minutesUntilNext = DynamicEventManager.Instance.TimeUntilNextEventMinutes;
```

### 2. InteractionTracker
Tracks player activity to determine if events should trigger.

```csharp
// Record any player action
InteractionTracker.Instance.RecordInteraction(InteractionType.ResearchStarted);

// Check if player is active
bool canTrigger = InteractionTracker.Instance.CanTriggerEvents;
```

### 3. EventEffectHandler
Manages active effects and provides multipliers to game systems.

```csharp
// Get production speed multiplier
float speedMultiplier = EventEffectHandler.Instance.GetEffectMultiplier(EffectType.ProductionSpeedBoost);

// Check active effects
bool hasBoost = EventEffectHandler.Instance.HasEffectOfType(EffectType.ResearchSpeedBoost);
```

### 4. RandomTimer
Standalone configurable random interval timer.

```csharp
var timer = new RandomTimer(15f * 60f, 20f * 60f); // 15-20 minutes
timer.OnTimerComplete += () => Debug.Log("Timer complete!");
timer.Start();
```

## Event Types

### Built-in Effect Types
- `ResearchTimeReduction` - Reduces remaining research time
- `ProductionSpeedBoost` - Increases production speed temporarily
- `ResearchSpeedBoost` - Increases research speed
- `MoneyBonus` - Instant money reward/penalty
- `ReputationChange` - Affects company reputation
- `RiskReduction` - Reduces project risk
- `TestSuccessBoost` - Increases prototype test success chance
- `ContractValueBoost` - Increases contract rewards

### Built-in Event Types
- `ResearchBreakthrough` - Positive research event
- `FundingBoost` - Temporary speed boost
- `SecurityAlert` - Decision event
- `PrototypeOptimization` - Test improvement
- `MarketOpportunity` - Contract bonus
- `EquipmentMalfunction` - Negative setback

## MVP Events

### 1. Eureka Moment (ResearchBreakthrough)
Reduces current research time by 5 minutes. Can stack up to 3 times.

### 2. Investor Confidence (FundingBoost)
+10% production speed for 10 minutes. Does not stack.

### 3. Security Breach Attempt (SecurityAlert)
Decision event with 3 choices:
- Hire security team: -$5000, +5 reputation
- Handle internally: -10 reputation, -2 min research
- Ignore: -25 reputation, -$10000

### 4. Design Flaw Discovered (PrototypeOptimization)
+15% test success chance for next prototype test.

### 5. Emergency Contract (MarketOpportunity)
+50% contract value for 15 minutes.

### 6. Power Surge (EquipmentMalfunction)
Loses 2 minutes of research progress.

## Creating Custom Events

```csharp
// Create a new event asset in Unity
// Right-click > Project Aegis > Game Event

// Or programmatically:
var myEvent = ScriptableObject.CreateInstance<GameEventData>();
myEvent.eventId = "my_custom_event";
myEvent.displayName = "My Custom Event";
myEvent.eventType = EventType.ResearchBreakthrough;
myEvent.weight = 10f;
myEvent.minIntervalMinutes = 15f;
myEvent.maxIntervalMinutes = 20f;

myEvent.effects = new List<EventEffect>
{
    new EventEffect
    {
        effectType = EffectType.ResearchSpeedBoost,
        value = 0.25f, // +25%
        durationMinutes = 10f,
        canStack = false
    }
};

// Register with the manager
DynamicEventManager.Instance.RegisterEvent(myEvent);
```

## Integration with Game Systems

### Research Manager
```csharp
public class ResearchManager : MonoBehaviour
{
    public float GetResearchSpeedMultiplier()
    {
        return EventEffectHandler.Instance.GetEffectMultiplier(EffectType.ResearchSpeedBoost);
    }
    
    public void ApplyResearchTimeReduction()
    {
        float multiplier = EventEffectHandler.Instance.GetEffectMultiplier(EffectType.ResearchTimeReduction);
        // Apply to current research
    }
}
```

### Production Manager
```csharp
public class ProductionManager : MonoBehaviour
{
    public float GetProductionSpeed()
    {
        float baseSpeed = 1f;
        float multiplier = EventEffectHandler.Instance.GetEffectMultiplier(EffectType.ProductionSpeedBoost);
        return baseSpeed * multiplier;
    }
}
```

### UI Integration
```csharp
public class ResearchButton : MonoBehaviour
{
    public void OnClick()
    {
        // Register interaction
        DynamicEventManager.Instance.RegisterPlayerInteraction(InteractionType.UIButton);
        
        // Start research
        ResearchManager.Instance.StartResearch();
    }
}
```

## Save/Load

```csharp
// Save
var saveData = new GameSaveData
{
    EventManagerData = DynamicEventManager.Instance.GetSaveData(),
    EffectHandlerData = EventEffectHandler.Instance.GetSaveData(),
    InteractionTrackerData = InteractionTracker.Instance.GetSaveData()
};

// Load
DynamicEventManager.Instance.LoadSaveData(saveData.EventManagerData);
EventEffectHandler.Instance.LoadSaveData(saveData.EffectHandlerData);
InteractionTracker.Instance.LoadSaveData(saveData.InteractionTrackerData);
```

## Events & Callbacks

```csharp
// Subscribe to events
DynamicEventManager.Instance.OnEventTriggered += OnEventTriggered;
DynamicEventManager.Instance.OnEventEffectApplied += OnEffectApplied;
DynamicEventManager.Instance.OnEventEffectExpired += OnEffectExpired;
DynamicEventManager.Instance.OnEventChoiceMade += OnChoiceMade;

void OnEventTriggered(GameEventData evt)
{
    Debug.Log($"Event triggered: {evt.displayName}");
    // Show popup UI
}

void OnChoiceMade(GameEventData evt, EventChoice choice)
{
    Debug.Log($"Player chose: {choice.displayText}");
}
```

## Debug Options

Enable debug visualization in the inspector:
- `Show Debug Logs` - Console logging
- `Show Timer Debug` - On-screen timer display

## Performance

- Effect updates run once per second (configurable)
- Timer uses unscaled delta time for accuracy
- Effect multipliers are cached and only recalculated when effects change
- Minimal garbage collection through object pooling

## Namespace

All event system classes are in:
```csharp
using ProjectAegis.Systems.Events;
```
