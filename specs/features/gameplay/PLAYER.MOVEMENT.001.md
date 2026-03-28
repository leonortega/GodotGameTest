# Spec: `PLAYER.MOVEMENT.001`

## Metadata
- **Title**: Core Horizontal Movement, Jumping, and Collision
- **Version**: `v1.2`
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
- `PLAYER.MOVEMENT.001-R2B`: The grounded jump shall allow a short post-edge grace window so a jump input immediately after leaving a ledge still resolves as a normal jump.
- `PLAYER.MOVEMENT.001-R2C`: A jump input pressed shortly before landing shall be buffered long enough to trigger on the first valid grounded frame rather than being lost.
- `PLAYER.MOVEMENT.001-R2D`: Releasing the jump input early during ascent shall shorten the jump arc compared with holding the jump input.
- `PLAYER.MOVEMENT.001-R3`: The system shall allow a run state that increases horizontal movement speed.
- `PLAYER.MOVEMENT.001-R4`: The system shall allow directional air control after jump takeoff.
- `PLAYER.MOVEMENT.001-R5`: The system shall use consistent jump behavior for repeated input under identical conditions.
- `PLAYER.MOVEMENT.001-R6`: The system shall prevent the player from moving through solid tiles from the sides, below, or above.
- `PLAYER.MOVEMENT.001-R7`: Falling into a pit or outside the stage bounds shall cause a lost life.
- `PLAYER.MOVEMENT.001-R8`: The player shall be able to strike eligible blocks from below when vertical collision occurs during upward movement.
- `PLAYER.MOVEMENT.001-R9`: The second jump shall be triggered by the same jump action input, including `Space`, `W`, or equivalent mapped jump controls.
- `PLAYER.MOVEMENT.001-R9A`: Traversal states such as idle, run or walk, jump, and fall shall remain visually readable through real player pose or animation assets rather than a single static placeholder body.
- `PLAYER.MOVEMENT.001-R9B`: When the player holds the down input while grounded and not meaningfully moving horizontally, the player presentation shall switch to a readable duck or crouch pose.

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

Scenario: Jump grace window forgives a late ledge input
  Given the player runs off the edge of a platform
  When the jump input is pressed immediately after leaving the ledge
  Then the system shall still resolve a grounded-style jump

Scenario: Jump buffer preserves an early landing input
  Given the player is descending toward safe ground
  When the player presses jump shortly before touching down
  Then the next valid grounded frame shall trigger a jump

Scenario: Early release produces a shorter jump
  Given the player starts a normal jump from flat ground
  When the player releases the jump input before the ascent completes
  Then the jump apex shall be lower than a held jump from the same starting state

Scenario: Traversal pose remains readable during movement
  Given the player is moving through idle, running, jumping, and falling states
  When the player state changes during traversal
  Then the visible player sprite or pose shall change to reflect the current movement state

Scenario: Duck pose is visible on grounded down input
  Given the player is standing on stable ground
  When the player holds the down input without meaningful horizontal movement
  Then the visible player sprite or pose shall change to a duck or crouch presentation
```

## Example Inputs/Outputs
- Example input: Hold right plus run, then press jump near a one-tile gap.
- Expected output: Player clears the gap and lands on the next platform while switching from run to jump and then fall presentation states.
- Example input: Press `Space`, then press `Space` again before landing.
- Expected output: Player performs a second airborne jump.
- Example input: Run off a ledge and press `Space` a fraction of a second late.
- Expected output: The player still performs the grounded jump instead of falling with the input ignored.
- Example input: Press jump just before touching down from a fall.
- Expected output: The player immediately jumps on landing because the input was buffered.

## Edge Cases
- Jump input after the second airborne jump shall not start a third jump before landing.
- Simultaneous left and right input shall resolve deterministically with no jittering movement.
- Upward collision under a non-interactive solid tile shall stop ascent without triggering a reward.
- Buffered jump input shall expire if no valid grounded state is reached within the grace window.
- The duck or crouch presentation shall not override airborne jump or fall readability.

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
