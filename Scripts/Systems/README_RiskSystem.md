# Project Aegis: Risk System Documentation

## Overview

The Risk System is a core strategic element of Project Aegis: Drone Dominion. Every research project and contract carries three types of risk: **Technical**, **Financial**, and **Security**. Higher risk means higher failure chance, but also better rewards.

---

## Core Files

| File | Purpose |
|------|---------|
| `RiskProfile.cs` | Risk data structure with 3 categories (Technical, Financial, Security) |
| `RiskLevel.cs` | Enum defining risk levels (None to Critical) |
| `RiskModifiers.cs` | Factors that affect risk calculations |
| `RiskCalculator.cs` | Static utility for risk calculations and dice rolls |
| `RiskManager.cs` | Main controller - handles rolls, consequences, streaks |
| `FailureResult.cs` | Data structure for failure outcomes |
| `FailureConsequences.cs` | Defines consequences for each failure type |
| `RiskMitigation.cs` | Methods to reduce risk through investments |
| `RiskDisplay.cs` | UI visualization helpers |
| `ResearchRiskIntegration.cs` | Integration with research system |
| `ContractRiskIntegration.cs` | Integration with contract system |
| `RiskSystemExamples.cs` | Usage examples and demonstrations |

---

## Risk Calculation Formula

```
Base Failure Chance = (TechnicalRisk + FinancialRisk + SecurityRisk) / 3

Modified Chance = Base Chance
    - TechLevelReduction
    - (Reputation / 10)
    - SecurityInvestment
    + DeadlinePressure
    + EventModifiers
    + RecentFailurePenalty
    - SuccessStreakBonus

Final Chance = Clamp(Modified Chance, 5%, 95%)
```

### Key Points:
- **Always 5-95% range**: Never guaranteed success or failure
- **Streak bonuses**: Consecutive successes give up to 15% reduction
- **Streak penalties**: Consecutive failures add up to 10% penalty
- **Weighted for context**: Research weights technical higher, contracts weight financial higher

---

## Risk Levels

| Level | Value | Description |
|-------|-------|-------------|
| None | 0% | No significant risk |
| VeryLow | 10% | Minimal risk |
| Low | 25% | Some minor issues possible |
| Medium | 50% | Expect challenges |
| High | 75% | Significant problems likely |
| VeryHigh | 90% | Major problems probable |
| Critical | 100% | Severe consequences likely |

---

## Failure Types

| Type | Severity | Typical Consequences |
|------|----------|---------------------|
| MinorSetback | ★☆☆☆☆ | Small delay, ~$100 loss |
| Delay | ★★☆☆☆ | Time delay, ~$300 loss |
| CostOverrun | ★★★☆☆ | Extra funding needed, ~$500 loss |
| PartialFailure | ★★★☆☆ | 20-50% progress lost, ~$800 loss |
| MajorFailure | ★★★★☆ | Significant penalties, ~$2000 loss |
| Catastrophic | ★★★★★ | Severe consequences, ~$5000+ loss |

---

## Risk Mitigation Options

### 1. Security Investment
- Max reduction: 40%
- Cost: ~$500 per % reduction
- Diminishing returns apply

### 2. Hire Consultant
- Max reduction: 15%
- Cost: ~$300 per % reduction
- Duration: 60 minutes

### 3. Extend Timeline
- Max reduction: 20%
- Efficiency: 10% time = 5% risk reduction

### 4. Increase Budget
- Max reduction: 10%
- Efficiency: 10% budget = 2% risk reduction

### 5. Quality Investment
- Max reduction: 20%
- Cost: ~$400 per % reduction

### 6. Insurance
- Cost: 5% of project value
- Coverage: 50% of losses

---

## Usage Examples

### Basic Risk Profile
```csharp
var riskProfile = new RiskProfile
{
    technicalRisk = 65f,
    financialRisk = 40f,
    securityRisk = 30f
};

float overallRisk = riskProfile.GetOverallRisk(); // 45%
```

### Calculate Failure Chance
```csharp
var modifiers = new RiskModifiers
{
    techLevelReduction = 15f,
    reputationReduction = 10f,
    securityInvestment = 5f
};

float failureChance = RiskCalculator.CalculateFailureChance(riskProfile, modifiers);
// Result: ~15% (45% - 30%)
```

### Roll for Success
```csharp
bool success = RiskCalculator.RollForSuccess(failureChance);

// Or get detailed result
RiskRollResult result = RiskCalculator.RollDetailed(failureChance);
Debug.Log($"Rolled {result.Roll:F1}% against {failureChance:F1}%");
```

### Research Risk Roll
```csharp
// Using integration
var result = ResearchRiskIntegration.Instance.RollForResearchCompletion(
    "research_id",
    riskProfile,
    modifiers
);

if (result.Success)
{
    // Unlock technology, gain reputation
}
else
{
    // Apply failure consequences
}
```

### Contract Bid Evaluation
```csharp
var evaluation = ContractRiskIntegration.Instance.EvaluateBid(
    contractData,
    bidPrice,
    estimatedTime,
    companyTechLevel,
    companyReputation
);

if (evaluation.Recommended)
{
    // Bid is acceptable
}
```

### Apply Mitigation
```csharp
var options = new MitigationOptions
{
    SecurityInvestment = 5000f,
    HireConsultant = true,
    ConsultantCost = 3000f,
    TimelineExtensionPercent = 20f
};

var result = RiskMitigation.ApplyCombinedMitigation(riskProfile, options);
float newFailureChance = result.FinalProfile.GetOverallRisk();
```

---

## Risk Visualization

### Risk Bar
```csharp
string bar = RiskDisplay.GetRiskBar(65f);
// Result: "■■■■■■■□□□"
```

### Risk Color
```csharp
Color color = RiskDisplay.GetRiskColor(65f);
// Returns orange color
```

### Risk Advice
```csharp
string advice = RiskDisplay.GetRiskAdvice(riskProfile);
// Returns detailed recommendations
```

---

## Events

The RiskManager provides events for integration:

```csharp
// Subscribe to events
RiskManager.Instance.OnRiskRollCompleted += OnRiskRoll;
RiskManager.Instance.OnFailureOccurred += OnFailure;
RiskManager.Instance.OnSuccessAchieved += OnSuccess;

void OnRiskRoll(RiskRollResult result)
{
    // Update UI, log analytics
}

void OnFailure(FailureResult failure)
{
    // Apply penalties, show notification
}

void OnSuccess(string context, float reputationGain)
{
    // Apply rewards, update stats
}
```

---

## Save/Load

```csharp
// Save
RiskManagerSaveData saveData = RiskManager.Instance.GetSaveData();
// Serialize saveData

// Load
RiskManager.Instance.LoadSaveData(saveData);
```

---

## Integration Checklist

- [ ] Add RiskManager to scene
- [ ] Add ResearchRiskIntegration for R&D
- [ ] Add ContractRiskIntegration for contracts
- [ ] Subscribe to events for UI updates
- [ ] Implement save/load for persistence
- [ ] Create UI using RiskDisplay helpers

---

## Balance Guidelines

| Risk Level | Recommended Failure Chance | Reward Multiplier |
|------------|---------------------------|-------------------|
| Low (<25%) | 5-15% | 1.0x |
| Medium (25-50%) | 20-35% | 1.3x |
| High (50-75%) | 40-55% | 1.7x |
| Critical (>75%) | 60-80% | 2.5x |

---

## Testing

Use `RiskSystemExamples` component to test:
1. Attach to GameObject
2. Check "Run Examples On Start" or use context menu
3. View results in console

---

## Namespace

All risk system files use:
```csharp
namespace ProjectAegis.Systems.Risk
```
