# Dynamic Event System - Integration Guide

## Quick Start

### 1. Add to Your Scene

The event system uses singletons that auto-create when accessed:

```csharp
void Start()
{
    // This will automatically create the manager if it doesn't exist
    DynamicEventManager.Instance.StartEventTimer();
}
```

Or manually add to your scene:
1. Create empty GameObject named "EventSystem"
2. Add `DynamicEventManager`, `InteractionTracker`, and `EventEffectHandler` components

### 2. Register Events

```csharp
void Start()
{
    // Register all MVP events
    var events = MVPEvents.CreateAllMVPEvents();
    foreach (var evt in events)
    {
        DynamicEventManager.Instance.RegisterEvent(evt);
    }
    
    // Start the timer
    DynamicEventManager.Instance.StartEventTimer();
}
```

### 3. Track Player Interactions

Add to ALL interactive elements:

```csharp
// UI Buttons
public void OnButtonClick()
{
    DynamicEventManager.Instance.RegisterPlayerInteraction(InteractionType.UIButton);
    // ... your button logic
}

// Research System
public void StartResearch(string researchId)
{
    DynamicEventManager.Instance.RegisterPlayerInteraction(InteractionType.ResearchStarted);
    // ... start research
}

// Production System
public void StartProduction(string productId)
{
    DynamicEventManager.Instance.RegisterPlayerInteraction(InteractionType.ProductionStarted);
    // ... start production
}

// Building System
public void PlaceBuilding(BuildingData building)
{
    DynamicEventManager.Instance.RegisterPlayerInteraction(InteractionType.BuildingPlaced);
    // ... place building
}

// Contract System
public void AcceptContract(ContractData contract)
{
    DynamicEventManager.Instance.RegisterPlayerInteraction(InteractionType.ContractAccepted);
    // ... accept contract
}
```

### 4. Handle Event Callbacks

```csharp
void OnEnable()
{
    DynamicEventManager.Instance.OnEventTriggered += OnEventTriggered;
    DynamicEventManager.Instance.OnEventChoiceMade += OnChoiceMade;
}

void OnDisable()
{
    DynamicEventManager.Instance.OnEventTriggered -= OnEventTriggered;
    DynamicEventManager.Instance.OnEventChoiceMade -= OnChoiceMade;
}

void OnEventTriggered(GameEventData evt)
{
    // Show your UI popup
    if (evt.isDecisionEvent)
    {
        ShowDecisionPopup(evt);
    }
    else
    {
        ShowNotificationPopup(evt);
    }
}

void OnChoiceMade(GameEventData evt, EventChoice choice)
{
    // Handle choice outcome
    Debug.Log($"Player chose: {choice.displayText}");
}
```

### 5. Apply Effects to Game Systems

#### Research Manager
```csharp
public class ResearchManager : MonoBehaviour
{
    public float GetResearchTimeMultiplier()
    {
        // Get time reduction (e.g., 0.85 means 15% faster)
        float reduction = EventEffectHandler.Instance
            .GetEffectMultiplier(EffectType.ResearchTimeReduction);
        
        // Get speed boost (e.g., 1.25 means 25% faster)
        float speedBoost = EventEffectHandler.Instance
            .GetEffectMultiplier(EffectType.ResearchSpeedBoost);
        
        return reduction / speedBoost;
    }
    
    public float GetTestSuccessChanceBonus()
    {
        return EventEffectHandler.Instance
            .GetEffectBonus(EffectType.TestSuccessBoost);
    }
}
```

#### Production Manager
```csharp
public class ProductionManager : MonoBehaviour
{
    public float GetProductionSpeed()
    {
        float baseSpeed = 1f;
        float multiplier = EventEffectHandler.Instance
            .GetEffectMultiplier(EffectType.ProductionSpeedBoost);
        
        return baseSpeed * multiplier;
    }
}
```

#### Economy Manager
```csharp
public class EconomyManager : MonoBehaviour
{
    public float GetContractValueMultiplier()
    {
        return EventEffectHandler.Instance
            .GetEffectMultiplier(EffectType.ContractValueBoost);
    }
    
    public void AddMoney(float amount)
    {
        // Apply contract value boost if active
        float multiplier = GetContractValueMultiplier();
        float finalAmount = amount * multiplier;
        
        // ... add money
    }
}
```

## How the Random Timer Works

```
┌─────────────────────────────────────────────────────────────┐
│                    RANDOM TIMER FLOW                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. START                                                   │
│     └── Generate random interval (15-20 min default)       │
│                                                             │
│  2. UPDATE (every frame)                                    │
│     └── elapsedTime += deltaTime                           │
│                                                             │
│  3. CHECK COMPLETION                                        │
│     └── if elapsedTime >= interval                         │
│         └── Fire OnTimerComplete event                     │
│                                                             │
│  4. ON COMPLETE                                             │
│     └── Check: Can trigger events?                         │
│         └── Player interacted in last 3 min?               │
│             └── YES: Select & trigger random event         │
│             └── NO: Restart timer, try later               │
│                                                             │
│  5. RESTART TIMER                                           │
│     └── Generate new random interval                       │
│     └── Start counting again                               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Timer Configuration

```csharp
// Default: 15-20 minutes
DynamicEventManager.Instance.StartEventTimer();

// Custom: 10-15 minutes
DynamicEventManager.Instance.StartEventTimer(10f, 15f);

// Change intervals at runtime
DynamicEventManager.Instance.SetTimerInterval(5f, 10f);
```

### Timer States

```csharp
var timer = DynamicEventManager.Instance.EventTimer;

// Properties
timer.IsRunning;           // Is timer counting?
timer.IsPaused;            // Is timer paused?
timer.IsComplete;          // Has timer finished?
timer.Progress;            // 0.0 to 1.0
timer.RemainingTimeSeconds;
timer.ElapsedTimeSeconds;

// Methods
timer.Pause();
timer.Resume();
timer.Stop();
timer.Reset();
timer.Restart();
timer.ForceComplete();     // Trigger immediately
```

## Code Examples

### Triggering Events for Testing

```csharp
// Force a random event
DynamicEventManager.Instance.TriggerRandomEvent();

// Force specific event
DynamicEventManager.Instance.TriggerEvent("eureka_moment");

// Force by reference
var evt = MVPEvents.CreateEurekaMoment();
DynamicEventManager.Instance.TriggerEvent(evt);
```

### Creating Custom Events

```csharp
// Method 1: Create ScriptableObject in Unity Editor
// Right-click > Project Aegis > Game Event

// Method 2: Create programmatically
var customEvent = ScriptableObject.CreateInstance<GameEventData>();
customEvent.eventId = "custom_event";
customEvent.displayName = "My Custom Event";
customEvent.description = "This is a custom event.";
customEvent.eventType = EventType.ResearchBreakthrough;
customEvent.weight = 10f;
customEvent.minIntervalMinutes = 15f;
customEvent.maxIntervalMinutes = 25f;

customEvent.effects = new List<EventEffect>
{
    new EventEffect
    {
        effectType = EffectType.ResearchSpeedBoost,
        value = 0.20f,  // +20%
        durationMinutes = 15f,
        canStack = false
    }
};

// Register
DynamicEventManager.Instance.RegisterEvent(customEvent);
```

### Creating Decision Events

```csharp
var decisionEvent = ScriptableObject.CreateInstance<GameEventData>();
decisionEvent.eventId = "critical_decision";
decisionEvent.displayName = "Critical Decision";
decisionEvent.description = "A critical situation requires your attention.";
decisionEvent.isDecisionEvent = true;

decisionEvent.choices = new List<EventChoice>
{
    new EventChoice
    {
        choiceId = "option_a",
        displayText = "Take Safe Route",
        description = "Lower risk, lower reward.",
        outcomes = new List<EventEffect>
        {
            new EventEffect { effectType = EffectType.MoneyBonus, value = 1000f }
        }
    },
    new EventChoice
    {
        choiceId = "option_b",
        displayText = "Take Risky Route",
        description = "Higher risk, higher reward.",
        outcomes = new List<EventEffect>
        {
            new EventEffect { effectType = EffectType.MoneyBonus, value = 5000f },
            new EventEffect { effectType = EffectType.ReputationChange, value = -5f }
        }
    }
};
```

### Adding New Event Types

1. **Add to EventType enum** in `GameEventData.cs`:
```csharp
public enum EventType
{
    // ... existing types
    MyCustomEventType,  // Add your new type
}
```

2. **Add to EffectType enum** in `EventEffect.cs`:
```csharp
public enum EffectType
{
    // ... existing types
    MyCustomEffect,  // Add your new effect
}
```

3. **Handle the new effect** in `EventEffectHandler.GetEffectMultiplier()`:
```csharp
public float GetEffectMultiplier(EffectType type)
{
    switch (type)
    {
        // ... existing cases
        case EffectType.MyCustomEffect:
            // Your custom multiplier logic
            multiplier += effect.effectData.value;
            break;
    }
}
```

4. **Create the event**:
```csharp
var myEvent = ScriptableObject.CreateInstance<GameEventData>();
myEvent.eventType = EventType.MyCustomEventType;
myEvent.effects = new List<EventEffect>
{
    new EventEffect { effectType = EffectType.MyCustomEffect, value = 1.5f }
};
```

## Save/Load Integration

### Saving
```csharp
public class SaveManager : MonoBehaviour
{
    public void SaveGame()
    {
        var saveData = new GameSaveData
        {
            EventManager = DynamicEventManager.Instance.GetSaveData(),
            EffectHandler = EventEffectHandler.Instance.GetSaveData(),
            InteractionTracker = InteractionTracker.Instance.GetSaveData()
        };
        
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("SaveData", json);
    }
}

[Serializable]
public class GameSaveData
{
    public DynamicEventManagerSaveData EventManager;
    public EventEffectHandlerSaveData EffectHandler;
    public InteractionTrackerSaveData InteractionTracker;
}
```

### Loading
```csharp
public void LoadGame()
{
    string json = PlayerPrefs.GetString("SaveData");
    var saveData = JsonUtility.FromJson<GameSaveData>(json);
    
    DynamicEventManager.Instance.LoadSaveData(saveData.EventManager);
    EventEffectHandler.Instance.LoadSaveData(saveData.EffectHandler);
    InteractionTracker.Instance.LoadSaveData(saveData.InteractionTracker);
}
```

## Common Issues & Solutions

### Events Not Triggering
- Check that `RegisterPlayerInteraction()` is being called
- Verify `InteractionTracker.CanTriggerEvents` returns true
- Check that events are registered in the database
- Enable debug logs to see timer status

### Effects Not Applying
- Ensure `EventEffectHandler` is in the scene
- Check effect duration (0 = instant, >0 = timed)
- Verify effect type is handled in `GetEffectMultiplier()`

### Timer Running Too Fast/Slow
- Timer uses `Time.deltaTime` (affected by Time.timeScale)
- Use `Time.unscaledDeltaTime` if you want real-time
- Check for pausing that might affect the timer

## Best Practices

1. **Always register interactions** - Every button click, action, or menu open should call `RegisterPlayerInteraction()`

2. **Subscribe to events early** - Subscribe in `OnEnable()` or `Start()`

3. **Unsubscribe properly** - Always unsubscribe in `OnDisable()` to prevent memory leaks

4. **Use effect multipliers** - Don't hardcode effect values; query the handler

5. **Test with debug methods** - Use the context menu methods to test events

6. **Balance event weights** - Common events should have higher weights than rare ones

7. **Set reasonable cooldowns** - Prevent events from firing too frequently
