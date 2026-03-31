# Spec: `LEVEL.DYNAMICS.002`

## Metadata
- **Title**: Single Falling Blocks
- **Version**: `v1.6`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: Medium

## Purpose
Define the runtime behavior of single falling blocks, while leaving authored line composition and stage-layout limits to `LEVEL.GENERATION.001`.

## Preconditions
- An active stage is loaded.
- A falling block is present in the level.

## Trigger
- The player contacts a falling block during active gameplay.

## Requirements
- `LEVEL.DYNAMICS.002-R1`: The system shall support reusable falling block scene instances whose visual and collision footprint matches a single terrain block.
- `LEVEL.DYNAMICS.002-R2`: A falling block shall remain stable until the player maintains support contact with that block for approximately `0.5` seconds.
- `LEVEL.DYNAMICS.002-R3`: Contact timing for a falling block shall reset if the player breaks contact before the `0.5` second threshold is reached.
- `LEVEL.DYNAMICS.002-R4`: When the `0.5` second threshold is reached, only the contacted falling block shall begin falling downward.
- `LEVEL.DYNAMICS.002-R4A`: Neighboring falling blocks in the same authored line shall remain suspended until they are triggered by separate sustained contact.
- `LEVEL.DYNAMICS.002-R5`: Once triggered, a falling block shall continue falling and shall no longer behave as a stable suspended platform.
- `LEVEL.DYNAMICS.002-R6`: Placement limits, line composition, and route-safety constraints for falling-block sections shall be defined by `LEVEL.GENERATION.001` and `LEVEL.AUTHORING.001`, not duplicated here.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Falling block waits before collapsing
  Given the player is standing on a falling block
  When the player remains in contact for less than 0.5 seconds
  Then the block shall remain suspended

Scenario: Contacted falling block collapses after sustained contact
  Given the player is standing on a falling block
  When the player remains in contact for approximately 0.5 seconds
  Then that falling block shall begin falling downward
  And neighboring falling blocks shall remain suspended until separately triggered

Scenario: Leaving early resets the collapse timer
  Given the player touches a falling block
  When the player leaves the block before 0.5 seconds elapse
  Then the collapse timer shall reset
  And the block shall remain suspended until touched again long enough

```

## Example Inputs/Outputs
- Example input: The player lands on one block in a suspended line of four falling blocks with real gaps between them and no ground beneath them, then remains there for 0.5 seconds.
- Expected output: Only the contacted block begins falling downward after the delay, while neighboring blocks remain suspended.

## Edge Cases
- Brief contact from below or the side shall not falsely consume the full delay if sustained player support contact is broken.
- A triggered falling block shall not return to a stable suspended state during the same run.

## Non-Functional Constraints
- The 0.5 second warning window should still feel readable enough for the player to react.
- Each block fall should read clearly and deterministically once triggered.
- The empty spaces between blocks should remain visually pronounced and should match the real non-walkable collision gaps.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `LEVEL.AUTHORING.001`
- `LEVEL.GENERATION.001`
- `LEVEL.PROGRESSION.001`
- `CAMERA.FOLLOW.001`
