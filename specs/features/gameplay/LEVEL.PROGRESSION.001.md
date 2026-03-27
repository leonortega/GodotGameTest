# Spec: `LEVEL.PROGRESSION.001`

## Metadata
- **Title**: Stage Completion, Lives, Timer, and World Advancement
- **Version**: `v1.2`
- **Status**: Approved
- **Context/View**: Level Flow
- **Priority**: High

## Purpose
Define how the player clears stages, loses lives, and advances through the MVP world.

## Preconditions
- A gameplay session has started.

## Trigger
- Stage start, player death, timer expiry, coin collection, or goal marker contact.

## Requirements
- `LEVEL.PROGRESSION.001-R1`: A new game shall begin with 3 lives.
- `LEVEL.PROGRESSION.001-R1A`: A new run shall begin at stage `1-1` in the current MVP build.
- `LEVEL.PROGRESSION.001-R2`: The MVP shall contain sequential stages `1-1`, `1-2`, `1-3`, and `1-4`.
- `LEVEL.PROGRESSION.001-R3`: Each stage shall begin with a countdown timer.
- `LEVEL.PROGRESSION.001-R4`: Reaching zero on the stage timer shall cause a lost life.
- `LEVEL.PROGRESSION.001-R5`: Touching the goal marker shall complete the current stage if the player is alive.
- `LEVEL.PROGRESSION.001-R5A`: The goal marker may be represented as a compact ground-level flag or equivalent readable endpoint marker; a tall flagpole is not required for MVP completion behavior.
- `LEVEL.PROGRESSION.001-R6`: Completing a stage shall advance the player to the next stage in sequence.
- `LEVEL.PROGRESSION.001-R6A`: On non-final stage clear, the game shall show a centered stage summary overlay containing the player's current run statistics before automatically advancing.
- `LEVEL.PROGRESSION.001-R6B`: The stage summary overlay shall remain visible for approximately 3 seconds and shall not require a manual `Continue` action.
- `LEVEL.PROGRESSION.001-R7`: Clearing the final MVP stage shall end the session in a world-clear state.
- `LEVEL.PROGRESSION.001-R8`: Collecting coins shall increase a running coin total and score.
- `LEVEL.PROGRESSION.001-R9`: Reaching the extra life threshold shall award one additional life.
- `LEVEL.PROGRESSION.001-R10`: After a non-final lost life, the current stage shall restart from its stage start position in the MVP baseline.
- `LEVEL.PROGRESSION.001-R11`: Remaining time at stage clear shall convert into bonus score.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Goal marker clears a stage
  Given the player is alive in stage 1-1
  When the player touches the goal marker
  Then stage 1-1 shall be marked complete
  And a centered stage summary shall be shown
  And after approximately 3 seconds stage 1-2 shall load next

Scenario: A new run starts at stage 1-1
  When the player starts a new run
  Then stage 1-1 shall load as the first interactive stage of that run

Scenario: Timer expiration costs a life
  Given the player is in an active stage
  And the stage timer has 1 second remaining
  When the timer reaches zero
  Then the player shall lose one life
  And the stage shall restart if lives remain

Scenario: Coin threshold awards an extra life
  Given the player has 99 coins
  When the player collects 1 coin
  Then the player shall gain 1 life

Scenario: Final stage clear ends the MVP world
  Given the player is alive in stage 1-4
  When the player touches the goal marker
  Then the world-clear screen shall be displayed
```

## Example Inputs/Outputs
- Example input: Player touches a ground-level goal marker in stage `1-3` with `87` seconds remaining.
- Expected output: Stage clears, a centered run-summary overlay is shown for about `3` seconds, timer converts to score bonus, and next stage is `1-4`.
- Example input: The player starts a new run from the title screen.
- Expected output: The run opens on stage `1-1`, and later stage clear progression continues to `1-2`, `1-3`, and `1-4` in order.

## Edge Cases
- Coin reward overflow at the extra life threshold shall not skip life grants.
- A death and goal touch occurring in the same frame shall resolve consistently according to collision order.
- Goal marker presentation shall remain readable without obscuring nearby terrain at the stage endpoint.
- The timer shall not continue counting down after stage clear begins.
- The automatic stage-advance summary shall not wait for player confirmation or leave the run stalled between non-final stages.
- Starting a new run shall always reset stage progression to `1-1` without corrupting saved highest-cleared-stage tracking.

## Non-Functional Constraints
- Restart after death should be fast enough to preserve play rhythm.
- Stage transitions should communicate state change clearly without long blocking delays.

## Related Specs
- `APP.SHELL.001`
- `PLAYER.MOVEMENT.001`
- `PLAYER.POWERUPS.001`
- `HUD.STATUS.001`
