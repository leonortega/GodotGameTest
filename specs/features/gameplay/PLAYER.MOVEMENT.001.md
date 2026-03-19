# Spec: `PLAYER.MOVEMENT.001`

## Metadata
- **Title**: Core Horizontal Movement, Jumping, and Collision
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: High

## Purpose
Define the baseline movement model that makes the platforming feel responsive and predictable.

## Preconditions
- An active stage is loaded.
- The player character is alive and under player control.

## Trigger
- Movement or jump input during active gameplay.

## Requirements
- `PLAYER.MOVEMENT.001-R1`: The system shall allow the player to move left and right while grounded.
- `PLAYER.MOVEMENT.001-R2`: The system shall allow the player to jump from a grounded state.
- `PLAYER.MOVEMENT.001-R2A`: The system shall allow a second jump while airborne when the player triggers the jump input a second time before landing.
- `PLAYER.MOVEMENT.001-R3`: The system shall allow a run state that increases horizontal movement speed.
- `PLAYER.MOVEMENT.001-R4`: The system shall allow directional air control after jump takeoff.
- `PLAYER.MOVEMENT.001-R5`: The system shall use consistent jump behavior for repeated input under identical conditions.
- `PLAYER.MOVEMENT.001-R6`: The system shall prevent the player from moving through solid tiles from the sides, below, or above.
- `PLAYER.MOVEMENT.001-R7`: Falling into a pit or outside the stage bounds shall cause a lost life.
- `PLAYER.MOVEMENT.001-R8`: The player shall be able to strike eligible blocks from below when vertical collision occurs during upward movement.
- `PLAYER.MOVEMENT.001-R9`: The second jump shall be triggered by the same jump action input, including `Space`, `W`, or equivalent mapped jump controls.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Running produces a longer jump than walking
  Given the player is standing on flat ground
  When the player performs a walking jump
  Then the player shall travel a baseline horizontal distance
  When the player performs a running jump from the same ground
  Then the player shall travel farther than the walking jump

Scenario: Solid tiles block lateral movement
  Given the player is moving right toward a solid wall
  When the player reaches the wall
  Then the player shall stop at the collision boundary
  And the player shall not pass through the wall

Scenario: Falling into a pit loses a life
  Given the player is above a bottomless gap
  When the player falls below the valid stage bounds
  Then the current life shall be lost

Scenario: Double jump is available before landing
  Given the player has already performed a grounded jump
  And the player is still airborne
  When the player presses the jump input a second time
  Then the player shall perform a second jump
  And no third jump shall occur before landing
```

## Example Inputs/Outputs
- Example input: Hold right plus run, then press jump near a one-tile gap.
- Expected output: Player clears the gap and lands on the next platform.
- Example input: Press `Space`, then press `Space` again before landing.
- Expected output: Player performs a second airborne jump.

## Edge Cases
- Jump input after the second airborne jump shall not start a third jump before landing.
- Simultaneous left and right input shall resolve deterministically with no jittering movement.
- Upward collision under a non-interactive solid tile shall stop ascent without triggering a reward.

## Non-Functional Constraints
- Movement response should feel immediate with low perceived input latency.
- Collision resolution should remain stable at target frame rate with no tile tunneling in normal play.

## Related Specs
- `PLAYER.INPUT.001`
- `PLAYER.POWERUPS.001`
- `ENEMIES.CORE.001`
- `DIFFICULTY.BALANCE.001`
- `LEVEL.PROGRESSION.001`
- `CAMERA.FOLLOW.001`
- `LEVEL.AUTHORING.001`
