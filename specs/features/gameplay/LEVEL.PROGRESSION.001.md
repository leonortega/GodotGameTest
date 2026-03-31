# Spec: `LEVEL.PROGRESSION.001`

## Metadata
- **Title**: Stage Completion, Lives, Timer, and World Advancement
- **Version**: `v1.4`
- **Status**: Approved
- **Context/View**: Level Flow
- **Priority**: High

## Purpose
Define an SMB1-style linear stage flow in which the player starts a stage, loses lives, clears stages, and advances directly without a hub or world map.

## Preconditions
- A gameplay session has started.

## Trigger
- Stage start, player death, timer expiry, coin collection, or goal marker contact.

## Requirements
- `LEVEL.PROGRESSION.001-R1`: A new game shall begin with 3 lives.
- `LEVEL.PROGRESSION.001-R1A`: A new run shall begin at stage `1-1` in the current MVP build.
- `LEVEL.PROGRESSION.001-R2`: The MVP shall contain sequential stages `1-1`, `1-2`, `1-3`, and `1-4`.
- `LEVEL.PROGRESSION.001-R3`: Each stage shall begin with a countdown timer.
- `LEVEL.PROGRESSION.001-R3A`: A standard full-length stage in the current MVP shall default to a timer of `400` seconds.
- `LEVEL.PROGRESSION.001-R3B`: A more compact or denser stage may author a reduced timer of `300` seconds when its route length and optional-detour budget are materially lower than the standard stage profile.
- `LEVEL.PROGRESSION.001-R3C`: The `400`-second default shall be treated as an authored pacing budget derived from an expected first-clear play window of roughly `250` seconds plus about `60%` recovery margin for caution, mistakes, and optional pickups.
- `LEVEL.PROGRESSION.001-R4`: Reaching zero on the stage timer shall cause a lost life.
- `LEVEL.PROGRESSION.001-R5`: Touching the goal marker shall complete the current stage if the player is alive.
- `LEVEL.PROGRESSION.001-R5A`: The goal marker may be represented as a compact ground-level flag or equivalent readable endpoint marker; a tall flagpole is not required for MVP completion behavior.
- `LEVEL.PROGRESSION.001-R6`: Completing a stage shall advance the player to the next stage in sequence.
- `LEVEL.PROGRESSION.001-R6A`: On non-final stage clear, the run shall continue directly toward the next stage without a hub or world-map layer between stages.
- `LEVEL.PROGRESSION.001-R6B`: Stage-clear flow shall use a minimal SMB1-style transition rather than a centered run-summary overlay that pauses progression for manual review.
- `LEVEL.PROGRESSION.001-R7`: Clearing the final MVP stage shall end the session in a world-clear state.
- `LEVEL.PROGRESSION.001-R8`: Collecting coins shall increase a running coin total and score according to `SCORE.ECONOMY.001`.
- `LEVEL.PROGRESSION.001-R9`: Reaching the extra life threshold shall award one additional life according to `SCORE.ECONOMY.001`.
- `LEVEL.PROGRESSION.001-R10`: After a non-final lost life, the current stage shall restart from its stage start position in the MVP baseline.
- `LEVEL.PROGRESSION.001-R11`: Remaining time at stage clear shall convert into bonus score using the active score economy profile.
- `LEVEL.PROGRESSION.001-R12`: After a non-final lost life, the restart path shall return directly to the same stage and shall not route through a hub, world map, or unrelated stage-summary screen.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Goal marker clears a stage
  Given the player is alive in stage 1-1
  When the player touches the goal marker
  Then stage 1-1 shall be marked complete
  And the stage-clear flow shall remain minimal and direct
  And after the clear transition stage 1-2 shall load next

Scenario: A new run starts at stage 1-1
  When the player starts a new run
  Then stage 1-1 shall load as the first interactive stage of that run

Scenario: Timer expiration costs a life
  Given the player is in an active stage
  And the stage timer has 1 second remaining
  When the timer reaches zero
  Then the player shall lose one life
  And the stage shall restart if lives remain

Scenario: Standard full-length stage uses the default timer budget
  Given a standard full-length stage is loaded
  When gameplay begins
  Then the visible stage timer shall start at 400

Scenario: Coin threshold awards an extra life
  Given the player has 99 coins
  When the player collects 1 coin
  Then the player shall gain 1 life

Scenario: Final stage clear ends the MVP world
  Given the player is alive in stage 1-4
  When the player touches the goal marker
  Then the world-clear screen shall be displayed

Scenario: Stage flow does not enter a hub between clears
  Given the player clears stage 1-2
  When the next stage is prepared
  Then the run shall move directly to stage 1-3
  And no world-map or hub screen shall appear between those stages
```

## Example Inputs/Outputs
- Example input: Player touches a ground-level goal marker in stage `1-3` with `87` seconds remaining.
- Expected output: Stage clears, timer converts to score bonus using the active time-bonus rule, and the run transitions directly to stage `1-4` without a hub or centered summary overlay.
- Example input: A standard full-length stage begins.
- Expected output: The stage timer starts at `400` seconds.
- Example input: The player starts a new run from the title screen.
- Expected output: The run opens on stage `1-1`, and later stage clear progression continues to `1-2`, `1-3`, and `1-4` in order.

## Edge Cases
- Coin reward overflow at the extra life threshold shall not skip life grants.
- A death and goal touch occurring in the same frame shall resolve consistently according to collision order.
- Goal marker presentation shall remain readable without obscuring nearby terrain at the stage endpoint.
- The timer shall not continue counting down after stage clear begins.
- The direct stage-advance flow shall not wait for player confirmation or leave the run stalled between non-final stages.
- Starting a new run shall always reset stage progression to `1-1` and clear any prior run-state carryover.
- Non-final death restart shall not accidentally route through a world-clear, title, or hub flow.

## Non-Functional Constraints
- Restart after death should be fast enough to preserve play rhythm.
- Stage transitions should communicate state change clearly without long blocking delays.

## Related Specs
- `APP.SHELL.001`
- `PLAYER.MOVEMENT.001`
- `PLAYER.POWERUPS.001`
- `SCORE.ECONOMY.001`
- `HUD.STATUS.001`
