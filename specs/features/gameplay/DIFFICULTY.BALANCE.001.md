# Spec: `DIFFICULTY.BALANCE.001`

## Metadata
- **Title**: Difficulty Levels and Enemy Density Scaling
- **Version**: `v1.1`
- **Status**: Draft
- **Context/View**: Core Gameplay
- **Priority**: Medium

## Purpose
Document an optional post-SMB1 extension for difficulty scaling, while making clear that player-selectable difficulty is not part of the SMB1 baseline shell flow.

## Preconditions
- An optional non-baseline build enables more than one difficulty level.
- Gameplay balancing values are being applied to a stage.

## Trigger
- A non-baseline build enables difficulty-tuned enemy content.

## Requirements
- `DIFFICULTY.BALANCE.001-R1`: Player-selectable difficulty levels shall not be required for the SMB1 baseline product flow.
- `DIFFICULTY.BALANCE.001-R1A`: If a non-baseline build enables difficulty scaling, the system shall support at least `Easy`, `Normal`, and `Hard` difficulty levels.
- `DIFFICULTY.BALANCE.001-R2`: Higher difficulty levels shall increase the maximum number of simultaneously active enemies on screen.
- `DIFFICULTY.BALANCE.001-R2A`: `Easy` difficulty may reduce pressure by removing a defined subset of authored enemies during stage runtime setup.
- `DIFFICULTY.BALANCE.001-R2B`: `Hard` difficulty may increase pressure by instantiating a small number of additional enemy copies derived from authored non-flying enemies, provided their spawn positions remain valid and readable.
- `DIFFICULTY.BALANCE.001-R3`: If enabled, `Normal` difficulty shall be the default internal selection.
- `DIFFICULTY.BALANCE.001-R4`: Increasing difficulty shall not reduce stage readability below an acceptable platforming baseline.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Optional hard difficulty increases active enemy count
  Given the same stage layout is loaded on Normal difficulty
  And the same stage layout is loaded on Hard difficulty
  When enemy activation rules are applied
  Then Hard difficulty shall allow more active enemies on screen than Normal difficulty

Scenario: Optional easy difficulty can thin authored enemy pressure
  Given the same authored stage layout is loaded on Normal difficulty
  And the same authored stage layout is loaded on Easy difficulty
  When difficulty-tuned enemy content is prepared
  Then Easy difficulty may contain fewer active enemy actors than the Normal runtime setup

Scenario: Optional hard difficulty can add bonus enemy instances
  Given a stage contains authored non-flying enemies
  When the stage is prepared on Hard difficulty
  Then the runtime may add a small number of additional enemy instances
  And those additional enemies shall spawn on valid readable support

Scenario: Optional difficulty defaults to Normal when enabled
  Given a non-baseline build enables difficulty scaling
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
- Easy thinning and Hard bonus spawns shall remain deterministic enough for balancing and debugging.

## Non-Functional Constraints
- If enabled, difficulty scaling should remain understandable to the player.
- Increased enemy density must stay within performance budgets for the target platform.

## Related Specs
- `ENEMIES.CORE.001`
- `LEVEL.PROGRESSION.001`
