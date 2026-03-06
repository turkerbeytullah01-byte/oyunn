# Project Aegis: Drone Dominion - UI System

A comprehensive, modular UI system for the mobile idle strategy tycoon game.

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Core Components](#core-components)
3. [Screen Navigation](#screen-navigation)
4. [Adding New Screens](#adding-new-screens)
5. [UI Theme Colors](#ui-theme-colors)
6. [File Structure](#file-structure)

---

## Architecture Overview

The UI System follows a modular, data-driven architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                        UIManager                            │
│  (Singleton - Controls all UI flow)                         │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┼────────────┬──────────────┐
        │            │            │              │
   ┌────▼────┐ ┌────▼────┐ ┌─────▼─────┐ ┌─────▼──────┐
   │ Screens │ │ Popups  │ │TopBar    │ │Notifications│
   └────┬────┘ └────┬────┘ └───────────┘ └─────────────┘
        │           │
   ┌────▼───────────▼────┐
   │   UI Animator       │
   │   (Visual Effects)  │
   └─────────────────────┘
```

### Design Principles
- **Separation of Concerns**: UI logic separated from game logic
- **Data-Driven**: UI updates based on data changes, not direct manipulation
- **Event-Based**: Components communicate through events
- **Pool-Friendly**: Object pooling for performance
- **Mobile-First**: Touch-friendly with 44px minimum touch targets

---

## Core Components

### 1. UIManager.cs
Main controller for the entire UI system.

**Key Methods:**
```csharp
// Screen Management
UIManager.Instance.ShowScreen(ScreenIds.RESEARCH_TREE);
UIManager.Instance.HideScreen(ScreenIds.SETTINGS);
UIManager.Instance.GoBack();

// Popup Management
UIManager.Instance.ShowPopup(new PopupData { title = "Alert", description = "..." });
UIManager.Instance.HidePopup();

// Notifications
UIManager.Instance.ShowNotification("Research Complete!", NotificationType.Success);
UIManager.Instance.ShowResearchComplete("Advanced AI");
UIManager.Instance.ShowMoneyEarned(50000);

// Loading
UIManager.Instance.SetLoading(true, "Loading...");
```

### 2. BaseScreen.cs
Abstract base class for all screens.

**Lifecycle Methods:**
```csharp
public class MyScreen : BaseScreen
{
    protected override void Initialize() { /* One-time setup */ }
    public override void OnShow() { /* Called when shown */ }
    public override void OnHide() { /* Called when hidden */ }
    public override void OnRefresh() { /* Update data */ }
}
```

### 3. ScreenIds.cs
Centralized screen identifiers.

**Available Screens:**
- `MAIN_MENU` - Main menu
- `GAME_HUD` - In-game HUD
- `RESEARCH_TREE` - Research/tech tree
- `CONTRACTS` - Contract list
- `BIDDING` - Bid submission
- `PRODUCTION` - Production management
- `PROTOTYPE_TEST` - Prototype testing
- `SETTINGS` - Game settings
- `INTELLIGENCE` - Intel dashboard
- `FINANCE` - Financial overview

### 4. TopBarUI.cs
Persistent top bar with resources.

**Features:**
- Money display with animated updates
- Reputation percentage
- Risk meter
- Date/time display
- Pause button

### 5. ResearchTreeUI.cs
Scrollable technology tree.

**Features:**
- Category filtering
- Zoom controls
- Search functionality
- Node state visualization
- Progress tracking

### 6. ContractsUI.cs
Contract management interface.

**Features:**
- Filter by status
- Sort by various criteria
- Bid panel with chance calculation
- Real-time updates

### 7. PopupSystem.cs
Modal popup handler.

**Popup Types:**
- Info - Simple message
- Warning - Attention needed
- Error - Something went wrong
- Confirm - Yes/No choice
- Timed - Time-limited decision
- Event - Story events

### 8. NotificationSystem.cs
Toast notifications.

**Notification Types:**
- Info, Success, Warning, Error
- Research, Contract, Event, Achievement

---

## Screen Navigation

### Navigation Flow
```
Main Menu
    ├── Game HUD (persistent)
    │       └── Top Bar (persistent)
    │
    ├── Research Tree
    │       └── Research Detail Panel
    │
    ├── Contracts
    │       ├── Contract Detail Panel
    │       └── Bid Panel
    │
    ├── Production
    │
    ├── Intelligence
    │       ├── Market Intel
    │       └── Rival Intel
    │
    ├── Finance
    │
    └── Settings
```

### Screen Stack
The UIManager maintains a screen stack for back navigation:
```csharp
// Navigate forward
UIManager.Instance.ShowScreen(ScreenIds.RESEARCH_TREE);

// Navigate back
UIManager.Instance.GoBack(); // Returns to previous screen
```

### Modal Screens
Some screens appear over others without hiding them:
```csharp
public class MyModalScreen : BaseScreen
{
    public bool isModal = true; // Set in inspector
}
```

---

## Adding New Screens

### Step 1: Add Screen ID
Add to `ScreenIds.cs`:
```csharp
public static class ScreenIds
{
    public const string MY_NEW_SCREEN = "MyNewScreen";
}
```

### Step 2: Create Screen Script
Create `MyNewScreen.cs`:
```csharp
using UnityEngine;
using TMPro;

namespace ProjectAegis.UI
{
    public class MyNewScreen : BaseScreen
    {
        [SerializeField] private TextMeshProUGUI titleText;
        
        protected override void Initialize()
        {
            // Find components, setup buttons
        }
        
        public override void OnShow()
        {
            // Refresh data, animate in
            titleText.text = "My New Screen";
        }
        
        public override void OnHide()
        {
            // Cleanup, save state
        }
        
        public override void OnRefresh()
        {
            // Update displayed data
        }
    }
}
```

### Step 3: Create Prefab
1. Create empty GameObject in scene
2. Add `MyNewScreen` component
3. Set `screenId` to match constant
4. Configure `isModal` and `cacheOnHide`
5. Add CanvasGroup for animations
6. Save as prefab in `Prefabs/UI/Screens/`

### Step 4: Register in UIManager
1. Select UIManager in scene
2. Add prefab to `screenPrefabs` list

### Step 5: Navigate to Screen
```csharp
UIManager.Instance.ShowScreen(ScreenIds.MY_NEW_SCREEN);
```

---

## UI Theme Colors

### Background Colors
| Name | Hex | Usage |
|------|-----|-------|
| Background | #1a1a2e | Main background |
| Panel | #292940 | Panel backgrounds |
| Card | #33334d | Card elements |
| Elevated | #404059 | Hover/elevated |

### Accent Colors
| Name | Hex | Usage |
|------|-----|-------|
| Primary | #00d4ff | Buttons, highlights |
| Secondary | #994de6 | Secondary accents |
| Tertiary | #ff9933 | Warnings, special |

### Status Colors
| Name | Hex | Usage |
|------|-----|-------|
| Success | #33e666 | Positive outcomes |
| Warning | #ffcc33 | Cautions |
| Error | #ff4d4d | Failures |
| Info | #4db3ff | Information |

### Text Colors
| Name | Hex | Usage |
|------|-----|-------|
| Primary | #ffffff | Main text |
| Secondary | #b3b3bf | Subtitles |
| Tertiary | #80808c | Hints |
| Disabled | #4d4d59 | Disabled |

### Risk Level Colors
| Level | Range | Color |
|-------|-------|-------|
| None | 0% | #33cc33 |
| Low | 1-25% | #66ff66 |
| Medium | 26-50% | #ffcc33 |
| High | 51-75% | #ff8022 |
| Critical | 76-100% | #ff3333 |

---

## File Structure

```
ProjectAegis/Scripts/UI/
├── Core/
│   ├── UIManager.cs          # Main UI controller
│   ├── BaseScreen.cs         # Screen base class
│   ├── ScreenIds.cs          # Screen constants
│   └── UITheme.cs            # Visual theme
│
├── Screens/
│   ├── ResearchTreeUI.cs     # Research screen
│   ├── ContractsUI.cs        # Contracts screen
│   └── [Other screens...]
│
├── Components/
│   ├── TopBarUI.cs           # Persistent top bar
│   ├── RiskMeterUI.cs        # Risk indicator
│   ├── RiskIndicatorUI.cs    # Compact risk display
│   ├── ResearchNodeUI.cs     # Tech tree node
│   ├── ContractCardUI.cs     # Contract list item
│   ├── BidPanelUI.cs         # Bid submission
│   ├── ResearchDetailPanel.cs
│   └── ContractDetailPanel.cs
│
├── Popups/
│   ├── PopupSystem.cs        # Popup controller
│   └── PopupData.cs          # Popup data structures
│
├── Notifications/
│   └── NotificationSystem.cs # Toast notifications
│
├── Utils/
│   └── UIAnimator.cs         # Animation utilities
│
└── README.md                 # This file
```

---

## Best Practices

### 1. Touch Targets
- Minimum 44x44 pixels for buttons
- Add padding around interactive elements
- Use visual feedback on touch

### 2. Performance
- Use object pooling for dynamic elements
- Cache component references
- Update UI only when data changes

### 3. Accessibility
- Maintain color contrast ratios
- Support screen reader labels
- Provide visual + audio feedback

### 4. Localization
- Use TextMeshPro for RTL support
- Externalize all strings
- Support dynamic font sizing

---

## Quick Reference

### Show a Notification
```csharp
UIManager.Instance.ShowNotification("Message", NotificationType.Success);
```

### Show a Popup
```csharp
var popup = new PopupData
{
    title = "Title",
    description = "Description",
    canDismiss = true
};
UIManager.Instance.ShowPopup(popup);
```

### Update Resource Display
```csharp
UIManager.Instance.UpdateResourceDisplay();
```

### Animate an Element
```csharp
UIAnimator.FadeIn(canvasGroup, 0.3f);
UIAnimator.SlideIn(rectTransform, new Vector2(100, 0), 0.3f);
UIAnimator.ScaleIn(transform, 0.3f);
UIAnimator.Pulse(transform, 1f);
```

---

## Dependencies

- Unity 2022.3 LTS or newer
- TextMeshPro package
- Unity UI package
- (Optional) DOTween for advanced animations

---

## License

Part of Project Aegis: Drone Dominion - Internal Use Only
