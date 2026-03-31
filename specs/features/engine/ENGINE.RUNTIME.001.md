# Spec: `ENGINE.RUNTIME.001`

## Metadata
- **Title**: Runtime Feel Constraints for Fixed-Step Play, Activation Windows, and Object Budgets
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Engine Runtime
- **Priority**: High

## Purpose
Define the low-level runtime constraints that keep SMB1-style movement, timing, collision, and actor behavior consistent.

## Preconditions
- Runtime simulation settings are being configured or reviewed.

## Trigger
- Engine bootstrap, physics tuning, or runtime performance review.

## Requirements
- `ENGINE.RUNTIME.001-R1`: Terrain, block, and collision authoring shall use a `16x16` logic tile grid or an exact authored multiple of that grid.
- `ENGINE.RUNTIME.001-R2`: Core gameplay simulation shall run on a fixed-step model equivalent to `60 Hz`.
- `ENGINE.RUNTIME.001-R3`: Input sampling for gameplay actions shall occur every fixed simulation step and shall not depend solely on render-frame timing.
- `ENGINE.RUNTIME.001-R4`: Stage timers, movement, and collision resolution shall remain simulation-rate driven and shall not speed up or slow down when render framerate changes.
- `ENGINE.RUNTIME.001-R5`: Dynamic actors and hazards shall simulate only while inside an authored activation window around the camera.
- `ENGINE.RUNTIME.001-R5A`: The activation window shall include a forward margin larger than or equal to the rear margin so upcoming threats become active before the player reaches them.
- `ENGINE.RUNTIME.001-R5B`: Actors outside the activation window shall not continue damaging, attacking, or unexpectedly traversing into the player from unseen space.
- `ENGINE.RUNTIME.001-R6`: The runtime profile shall define explicit caps for active hostile actors, active player projectiles, and active dynamic interactables.
- `ENGINE.RUNTIME.001-R6A`: Runtime object caps shall favor preserving active player, mandatory platforms, HUD, and clear-state logic over distant or non-critical actors.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Fixed-step simulation keeps timer stable
  Given the game renders at varying visual frame rates
  When the stage timer runs for 1 real second
  Then gameplay timer progression shall remain consistent with the fixed simulation rate

Scenario: Off-screen actors outside activation window do not attack
  Given a hostile actor is outside the authored activation window
  When runtime simulation advances
  Then that actor shall not damage or attack the player from unseen space

Scenario: Input is sampled on fixed simulation steps
  Given the player presses jump during active play
  When the next fixed simulation step resolves
  Then the jump input shall be available to gameplay logic on that step

Scenario: Runtime caps preserve critical gameplay actors
  Given the runtime active-object cap is reached
  When a distant non-critical actor and a visible critical actor compete for activation
  Then the visible critical actor shall remain active
```

## Example Inputs/Outputs
- Example input: Runtime profile defines `tileSizePx = 16`, `fixedSimulationHz = 60`, and explicit activation margins around the camera.
- Expected output: Movement, timer, collision, and actor activation stay consistent across normal rendering variance.

## Edge Cases
- Short render stalls shall not permanently desynchronize timers, collision checks, or player inputs.
- Activation-window culling shall not remove the current goal state, mandatory moving platform, or other immediately relevant critical actor.
- Runtime caps shall not suppress player-fired projectiles below the authored projectile limit due to unrelated distant actor load.

## Non-Functional Constraints
- Runtime constraints should preserve both readability and deterministic debugging.
- Fixed-step tuning should remain simple enough to reason about without per-platform gameplay divergence.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `CAMERA.FOLLOW.001`
- `LEVEL.GENERATION.001`
