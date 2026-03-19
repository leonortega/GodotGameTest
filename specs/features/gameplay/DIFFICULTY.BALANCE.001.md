# Spec: `DIFFICULTY.BALANCE.001`

## Metadata
- **Title**: Difficulty Levels and Enemy Density Scaling
- **Version**: `v1.0`
- **Status**: Draft
- **Context/View**: Core Gameplay
- **Priority**: Medium

## Purpose
Define selectable difficulty levels and how enemy density increases as difficulty rises.

## Preconditions
- The game supports more than one difficulty level.
- A run is starting or gameplay balancing values are being applied to a stage.

## Trigger
- The player selects a difficulty level or the stage runtime loads difficulty-tuned enemy content.

## Requirements
- `DIFFICULTY.BALANCE.001-R1`: The system shall support at least `Easy`, `Normal`, and `Hard` difficulty levels.
- `DIFFICULTY.BALANCE.001-R2`: Higher difficulty levels shall increase the maximum number of simultaneously active enemies on screen.
- `DIFFICULTY.BALANCE.001-R3`: `Normal` difficulty shall be the default selection unless overridden by saved preference or explicit player choice.
- `DIFFICULTY.BALANCE.001-R4`: Increasing difficulty shall not reduce stage readability below an acceptable platforming baseline.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Hard difficulty increases active enemy count
  Given the same stage layout is loaded on Normal difficulty
  And the same stage layout is loaded on Hard difficulty
  When enemy activation rules are applied
  Then Hard difficulty shall allow more active enemies on screen than Normal difficulty

Scenario: Default difficulty is Normal
  Given the player starts a new run with no stored override
  When gameplay settings are initialized
  Then the selected difficulty shall be Normal
```

## Example Inputs/Outputs
- Example input: Stage `1-2` started on `Easy`.
- Expected output: Fewer simultaneously active enemies than `Normal`.
- Example input: Stage `1-2` started on `Hard`.
- Expected output: More simultaneously active enemies than `Normal`.

## Edge Cases
- Difficulty changes shall take effect at a clearly defined boundary such as new run start, stage load, or checkpoint reload.
- Increasing enemy count shall not spawn enemies inside solid geometry or overlapping the player spawn.

## Non-Functional Constraints
- Difficulty scaling should remain understandable to the player.
- Increased enemy density must stay within performance budgets for the target platform.

## Related Specs
- `ENEMIES.CORE.001`
- `LEVEL.PROGRESSION.001`
