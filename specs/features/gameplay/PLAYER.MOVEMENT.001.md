# Spec: `PLAYER.MOVEMENT.001`

## Metadata
- **Title**: Core Horizontal Movement, Jumping, and Collision
- **Version**: `v1.3`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: High

## Purpose
Define the authored movement model that keeps grounded traversal responsive and predictable while allowing one controlled midair follow-up jump without broader airborne forgiveness windows.

## Preconditions
- An active stage is loaded.
- The player character is alive and under player control.

## Trigger
- Movement or jump input during active gameplay.

## Requirements
- `PLAYER.MOVEMENT.001-R1`: The system shall allow the player to move left and right while grounded.
- `PLAYER.MOVEMENT.001-R1A`: Ground movement shall use deterministic acceleration and deceleration values rather than instant full-speed starts and stops.
- `PLAYER.MOVEMENT.001-R1B`: The baseline movement profile shall define distinct maximum walk speed and maximum run speed, with run speed extending jump distance.
- `PLAYER.MOVEMENT.001-R1C`: Reversing horizontal direction while carrying significant run speed shall produce a brief readable skid or turnaround state before full movement reverses.
- `PLAYER.MOVEMENT.001-R2`: The system shall allow the player to jump from a grounded state.
- `PLAYER.MOVEMENT.001-R2A`: After the initial grounded jump, the system shall allow exactly one additional jump while airborne before the player lands again.
- `PLAYER.MOVEMENT.001-R2AA`: Executing the airborne follow-up jump shall consume the current air-jump allowance until the player next lands on valid ground.
- `PLAYER.MOVEMENT.001-R2AB`: Additional jump input after the airborne follow-up jump has been spent shall not trigger a third jump before landing.
- `PLAYER.MOVEMENT.001-R2B`: The SMB1 baseline shall not require a post-edge grace window that converts a late ledge input into a grounded jump after takeoff has already been lost.
- `PLAYER.MOVEMENT.001-R2C`: The SMB1 baseline shall not require jump-input buffering that automatically converts a pre-landing input into a jump on the first grounded frame.
- `PLAYER.MOVEMENT.001-R2D`: Releasing the jump input early during ascent shall shorten the jump arc compared with holding the jump input.
- `PLAYER.MOVEMENT.001-R2E`: Jump height, gravity, and fall behavior shall be governed by a stable authored movement profile so repeated jumps under the same conditions produce the same arc.
- `PLAYER.MOVEMENT.001-R2F`: The airborne follow-up jump shall use an authored second-jump impulse profile that is stable across repeated input under the same conditions.
- `PLAYER.MOVEMENT.001-R3`: The system shall allow a run state that increases horizontal movement speed.
- `PLAYER.MOVEMENT.001-R4`: The system shall allow directional air control after jump takeoff.
- `PLAYER.MOVEMENT.001-R4A`: Air control shall remain weaker than grounded control and shall not permit instant full horizontal reversal at the top of a jump.
- `PLAYER.MOVEMENT.001-R5`: The system shall use consistent jump behavior for repeated input under identical conditions.
- `PLAYER.MOVEMENT.001-R6`: The system shall prevent the player from moving through solid tiles from the sides, below, or above.
- `PLAYER.MOVEMENT.001-R7`: Falling into a pit or outside the stage bounds shall cause a lost life.
- `PLAYER.MOVEMENT.001-R8`: The player shall be able to strike eligible blocks from below when vertical collision occurs during upward movement.
- `PLAYER.MOVEMENT.001-R9A`: Traversal states such as idle, run or walk, jump, and fall shall remain visually readable through real player pose or animation assets rather than a single static placeholder body.
- `PLAYER.MOVEMENT.001-R9B`: When the player holds the down input while grounded and not meaningfully moving horizontally, the player presentation shall switch to a readable duck or crouch pose.
- `PLAYER.MOVEMENT.001-R9C`: Grounded crouch shall reduce the standing movement profile and shall not behave as a full-speed run state.

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

Scenario: High-speed reversal produces a readable skid
  Given the player is running to the right on flat ground
  When the player presses and holds left
  Then the player shall enter a brief skid or turnaround state before accelerating left

Scenario: Falling into a pit loses a life
  Given the player is above a bottomless gap
  When the player falls below the valid stage bounds
  Then the current life shall be lost

Scenario: One airborne follow-up jump is allowed
  Given the player has already performed a grounded jump
  And the player is still airborne
  When the player presses the jump input a second time
  Then the player shall perform one airborne follow-up jump

Scenario: A third jump is not allowed before landing
  Given the player has already performed a grounded jump
  And the player has already performed the airborne follow-up jump
  And the player is still airborne
  When the player presses the jump input again
  Then the player shall not perform a third jump before landing

Scenario: Late ledge input does not use a grace window
  Given the player runs off the edge of a platform
  When the jump input is pressed immediately after leaving the ledge
  Then the system shall not be required to resolve a grounded-style jump

Scenario: Pre-landing jump input does not require buffering
  Given the player is descending toward safe ground
  When the player presses jump shortly before touching down
  Then the system shall not be required to trigger a jump on the first grounded frame

Scenario: Early release produces a shorter jump
  Given the player starts a normal jump from flat ground
  When the player releases the jump input before the ascent completes
  Then the jump apex shall be lower than a held jump from the same starting state

Scenario: Air control is weaker than grounded control
  Given the player jumps while moving right
  When the player reverses direction in midair
  Then the horizontal response shall remain weaker than the same reversal on the ground

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
- Example input: Run to the right at full speed and then hold left.
- Expected output: The player enters a brief skid before accelerating back to the left.
- Example input: Press `Space`, then press `Space` again before landing.
- Expected output: The player performs the authored airborne follow-up jump and gains additional height or repositioning distance.
- Example input: Press `Space`, press `Space` again in midair, then press `Space` a third time before landing.
- Expected output: The third input does not trigger another jump before landing.
- Example input: Run off a ledge and press `Space` a fraction of a second late.
- Expected output: The late input is not required to convert into a grounded jump after the ledge has already been left.
- Example input: Press jump just before touching down from a fall.
- Expected output: The early input is not required to trigger an automatic jump on landing.

## Edge Cases
- Additional jump input after the airborne follow-up jump has already been spent shall not create a third jump before landing.
- Simultaneous left and right input shall resolve deterministically with no jittering movement.
- Upward collision under a non-interactive solid tile shall stop ascent without triggering a reward.
- The duck or crouch presentation shall not override airborne jump or fall readability.
- Reversal at high speed shall not snap instantly from full run-right speed to full run-left speed in a single frame.

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
