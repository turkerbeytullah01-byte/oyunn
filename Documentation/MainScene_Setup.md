# MainScene Setup Instructions

## Overview
This document provides step-by-step instructions for setting up the main game scene in Unity for Project Aegis: Drone Dominion.

---

## 1. Scene Creation

### Create New Scene
1. In Unity, go to `File > New Scene`
2. Save as `Assets/Scenes/MainScene.unity`
3. Delete the default Main Camera (we'll create a custom setup)

---

## 2. Camera Setup

### Main Camera Configuration
1. Create new Camera: `GameObject > Camera`
2. Rename to "MainCamera"
3. Configure Transform:
   ```
   Position: (0, 15, -20)
   Rotation: (45, 0, 0)
   Scale: (1, 1, 1)
   ```
4. Camera Component Settings:
   ```
   Clear Flags: Solid Color
   Background: #1A1A2E (Dark blue-gray)
   Projection: Perspective
   Field of View: 60
   Clipping Planes: Near: 0.3, Far: 1000
   ```
5. Add Audio Listener component

### UI Camera (Optional)
1. Create second Camera for UI
2. Rename to "UICamera"
3. Configure:
   ```
   Clear Flags: Depth Only
   Culling Mask: UI only
   Depth: 1 (higher than MainCamera)
   ```

---

## 3. Lighting Setup

### Directional Light (Sun)
1. `GameObject > Light > Directional Light`
2. Rename to "Sun"
3. Transform:
   ```
   Position: (0, 0, 0)
   Rotation: (50, -30, 0)
   Scale: (1, 1, 1)
   ```
4. Light Settings:
   ```
   Color: #FFF8E7 (Warm white)
   Intensity: 1.0
   Shadow Type: Soft Shadows
   Shadow Strength: 0.5
   ```

### Ambient Light
1. Open Lighting window: `Window > Rendering > Lighting`
2. Environment Settings:
   ```
   Skybox Material: None (or custom)
   Sun Source: Sun (Directional Light)
   Environment Lighting:
     Source: Color
     Ambient Color: #2A2A4A (Dark blue)
   Environment Reflections:
     Source: Custom
     Resolution: 128
   ```

### Fill Light
1. Create Point Light: `GameObject > Light > Point Light`
2. Rename to "FillLight"
3. Position: (0, 5, 0)
4. Settings:
   ```
   Color: #4A90D9 (Blue)
   Intensity: 0.3
   Range: 50
   ```

---

## 4. Canvas Setup

### Main Canvas
1. `GameObject > UI > Canvas`
2. Rename to "MainCanvas"
3. Canvas Component:
   ```
   Render Mode: Screen Space - Overlay
   Canvas Scaler:
     UI Scale Mode: Scale With Screen Size
     Reference Resolution: 1920 x 1080
     Screen Match Mode: Match Width or Height
     Match: 0.5
   Graphic Raycaster: Default settings
   ```

### Canvas Structure
Create the following hierarchy under MainCanvas:
```
MainCanvas
├── BackgroundLayer (Sorting Order: 0)
│   └── BackgroundImage
├── GameLayer (Sorting Order: 10)
│   ├── TopBar
│   │   ├── CurrencyDisplay
│   │   ├── ResearchPointsDisplay
│   │   └── ReputationDisplay
│   ├── MainContent
│   │   ├── DronePanel
│   │   ├── ResearchPanel
│   │   ├── ContractsPanel
│   │   └── ProductionPanel
│   └── BottomBar
│       ├── NavigationButtons
│       └── QuickActions
├── PopupLayer (Sorting Order: 20)
│   ├── EventPopup
│   ├── ContractDetailsPopup
│   └── ResearchDetailsPopup
└── OverlayLayer (Sorting Order: 30)
    ├── LoadingScreen
    ├── Notifications
    └── TutorialOverlay
```

---

## 5. Manager GameObjects

### Create Managers Container
1. Create Empty GameObject: `GameObject > Create Empty`
2. Rename to "Managers"
3. Reset Transform (all zeros, scale 1)

### Add Manager Components
Attach the following components to the Managers GameObject:

```csharp
// Add these components via Inspector or script:
- GameInitializer (Main entry point)
- SaveManager
- TimeManager
- EventManager
- PlayerDataManager
- ResearchManager
- TechTreeManager
- ReputationManager
- RiskManager
- ProductionManager
- IdleManager
- DynamicEventManager
- ContractManager
- PrototypeTestingManager
- DroneManager
- UIManager
```

### Manager Configuration
1. Select Managers GameObject
2. In Inspector, click "Add Component"
3. Search for each manager and add
4. Configure any serialized fields

---

## 6. UI Layout

### Top Bar (1920x100)
Create under MainCanvas/GameLayer/TopBar:

```
TopBar (Panel)
├── Background (Image - Dark semi-transparent)
├── LeftSection (Horizontal Layout)
│   ├── CompanyLogo (Image)
│   └── CompanyName (Text)
├── CenterSection (Horizontal Layout)
│   ├── CurrencyDisplay
│   │   ├── Icon (Image)
│   │   └── Amount (Text)
│   ├── ResearchPointsDisplay
│   │   ├── Icon (Image)
│   │   └── Amount (Text)
│   └── ReputationDisplay
│       ├── Icon (Image)
│       └── Bar (Slider)
└── RightSection (Horizontal Layout)
    ├── SettingsButton
    ├── HelpButton
n    └── PauseButton
```

### Main Content Area
Create panels for each game section:

#### Drone Panel
```
DronePanel
├── Header (Text: "DRONE FLEET")
├── DroneList (ScrollView)
│   └── Content (Vertical Layout)
│       └── DroneCard (Prefab)
├── ProductionQueue
│   └── QueueItems
└── Actions
    ├── ProduceButton
    └── DeployButton
```

#### Research Panel
```
ResearchPanel
├── Header (Text: "RESEARCH & DEVELOPMENT")
├── TechTree (Custom UI or ScrollView)
├── ActiveResearch
│   └── ProgressBar
└── ResearchPointsDisplay
```

#### Contracts Panel
```
ContractsPanel
├── Header (Text: "ACTIVE CONTRACTS")
├── AvailableContracts (ScrollView)
├── ActiveContracts
└── ContractHistory
```

### Bottom Navigation Bar
```
BottomBar
├── Background (Image)
├── NavigationButtons (Horizontal Layout)
│   ├── DronesButton
│   ├── ResearchButton
│   ├── ContractsButton
│   ├── ProductionButton
│   └── StatsButton
└── QuickActions
    ├── EmergencyButton
    └── NotificationsButton
```

---

## 7. Prefab Setup

### Create Prefabs Folder
Create folder structure:
```
Assets/
├── Prefabs/
│   ├── UI/
│   │   ├── DroneCard.prefab
│   │   ├── ContractCard.prefab
│   │   ├── ResearchNode.prefab
│   │   ├── EventPopup.prefab
│   │   └── Notification.prefab
│   ├── Drones/
│   │   ├── ScoutX1_Model.prefab
│   │   ├── GuardianMk2_Model.prefab
│   │   └── SentinelPro_Model.prefab
│   └── Effects/
│       ├── ProductionComplete.prefab
│       ├── ResearchComplete.prefab
│       └── EventTrigger.prefab
```

### Drone Card Prefab Structure
```
DroneCard (Prefab)
├── Background (Image)
├── DroneIcon (Image)
├── InfoSection
│   ├── NameText
│   ├── ClassText
│   └── StatusText
├── StatsSection
│   ├── SpeedBar
│   ├── BatteryBar
│   └── SignalBar
└── Actions
    ├── ProduceButton
    ├── DeployButton
    └── DetailsButton
```

---

## 8. Visual Effects

### Particle Systems
Create these effects in scene:

#### Production Complete Effect
1. Create Particle System: `GameObject > Effects > Particle System`
2. Rename to "ProductionCompleteEffect"
3. Position: (0, 0, 0) - will be spawned at production location
4. Settings:
   ```
   Duration: 2.0
   Looping: false
   Start Lifetime: 1.5
   Start Speed: 5
   Start Size: 0.5
   Start Color: Green (#00FF00)
   Emission: Burst of 50 particles
   Shape: Sphere
   ```

#### Research Complete Effect
1. Create Particle System
2. Rename to "ResearchCompleteEffect"
3. Settings:
   ```
   Duration: 3.0
   Looping: false
   Start Color: Blue (#0088FF)
   Emission: Burst of 100 particles
   ```

---

## 9. Audio Setup

### Audio Manager
1. Create Empty GameObject: "AudioManager"
2. Add AudioSource components:
   - BGM_Source (Background Music)
   - SFX_Source (Sound Effects)
   - UI_Source (UI Sounds)

### Audio Mixer (Optional)
1. Create Audio Mixer: `Window > Audio > Audio Mixer`
2. Create groups:
   - Master
   - BGM
   - SFX
   - UI
3. Assign to respective AudioSources

---

## 10. Scene Organization

### Recommended Hierarchy
```
MainScene
├── --- MANAGERS ---
├── Managers
│   └── [All Manager Components]
├── --- ENVIRONMENT ---
├── Environment
│   ├── GroundPlane
│   ├── BackgroundElements
│   └── Lighting
├── --- CAMERA ---
├── MainCamera
├── UICamera (optional)
├── --- UI ---
├── MainCanvas
│   └── [All UI elements]
├── --- EFFECTS ---
├── Effects
│   └── [Particle systems]
├── --- AUDIO ---
├── AudioManager
└── --- DEBUG ---
└── DebugTools (optional)
```

---

## 11. ScriptableObject Assets

### Create Asset Folders
```
Assets/
├── Resources/
│   ├── Drones/
│   │   ├── ScoutX1.asset
│   │   ├── GuardianMk2.asset
│   │   └── SentinelPro.asset
│   ├── Research/
│   │   ├── EnergySystems/
│   │   └── Propulsion/
│   ├── Contracts/
│   │   ├── Easy/
│   │   ├── Medium/
│   │   └── Hard/
│   └── Events/
│       ├── Positive/
│       ├── Negative/
│       └── Neutral/
```

### Creating ScriptableObject Assets
1. Right-click in Project window
2. `Create > Project Aegis > [Asset Type]`
3. Configure fields using JSON data from ExampleData folder
4. Save with appropriate name

---

## 12. Build Settings

### Scene List
1. Open Build Settings: `File > Build Settings`
2. Add scenes:
   ```
   0: Assets/Scenes/BootstrapScene.unity (if using)
   1: Assets/Scenes/MainScene.unity
   ```

### Android Settings
1. Switch Platform to Android
2. Configure:
   ```
   Texture Compression: ASTC
   Build System: Gradle
   Target API Level: 30+
   Minimum API Level: 24
   ```

---

## 13. Testing Checklist

### Scene Validation
- [ ] All managers initialize correctly
- [ ] UI displays properly at 1920x1080
- [ ] UI scales correctly at other resolutions
- [ ] Camera shows game area properly
- [ ] Lighting looks correct
- [ ] No missing references or null exceptions

### Manager Validation
- [ ] GameInitializer completes without errors
- [ ] Save/Load works correctly
- [ ] All managers receive events
- [ ] Production system functions
- [ ] Research system functions
- [ ] Contract system functions
- [ ] Event system triggers correctly

### Performance Check
- [ ] Target 60 FPS on mid-range Android device
- [ ] Memory usage under 512MB
- [ ] No memory leaks during extended play
- [ ] Battery drain acceptable

---

## Quick Reference

### Common Issues

**Managers not initializing:**
- Check GameInitializer is in scene
- Verify all manager scripts compile
- Check for null reference exceptions

**UI not displaying:**
- Verify Canvas Render Mode
- Check Canvas Scaler settings
- Ensure UI elements have proper parents

**Events not triggering:**
- Verify EventManager is initialized
- Check event subscription in managers
- Ensure event IDs match

### Keyboard Shortcuts (Editor)
- `Ctrl + Shift + P`: Pause game
- `Ctrl + Shift + S`: Save game
- `Ctrl + Shift + L`: Load game
- `Ctrl + Shift + D`: Toggle debug UI
