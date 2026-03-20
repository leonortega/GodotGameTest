# Spec: `LEVEL.DYNAMICS.002`

## Metadata
- **Title**: Single Falling Blocks
- **Version**: `v1.6`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: Medium

## Purpose
Define single falling blocks as suspended one-block hazards that can be authored in lines with real gaps between them and that fall independently after brief sustained player contact.

## Preconditions
- An active stage is loaded.
- A falling block is present in the level.

## Trigger
- The player contacts a falling block during active gameplay.

## Requirements
- `LEVEL.DYNAMICS.002-R1`: The system shall support reusable falling block scene instances whose visual and collision footprint matches a single terrain block.
- `LEVEL.DYNAMICS.002-R1A`: Falling blocks may be authored in lines of `4` or more blocks for traversal sections, but each block shall remain an independent gameplay instance.
- `LEVEL.DYNAMICS.002-R1B`: An authored falling block line shall keep clear empty space before the left extent, after the right extent, and beneath each block footprint so it does not collide with surrounding map geometry.
- `LEVEL.DYNAMICS.002-R1C`: Adjacent falling blocks in an authored line shall be separated by real gaps of approximately `1.5` terrain blocks.
- `LEVEL.DYNAMICS.002-R2`: A falling block shall remain stable until the player maintains support contact with that block for approximately `0.5` seconds.
- `LEVEL.DYNAMICS.002-R3`: Contact timing for a falling block shall reset if the player breaks contact before the `0.5` second threshold is reached.
- `LEVEL.DYNAMICS.002-R4`: When the `0.5` second threshold is reached, only the contacted falling block shall begin falling downward.
- `LEVEL.DYNAMICS.002-R4A`: Neighboring falling blocks in the same authored line shall remain suspended until they are triggered by separate sustained contact.
- `LEVEL.DYNAMICS.002-R4B`: The empty spaces between adjacent falling blocks shall remain real non-walkable gaps rather than hidden continuous collision.
- `LEVEL.DYNAMICS.002-R5`: Once triggered, a falling block shall continue falling and shall no longer behave as a stable suspended platform.
- `LEVEL.DYNAMICS.002-R6`: Grounded enemies shall not be authored or normalized onto falling block surfaces as stable support.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Falling block waits before collapsing
  Given the player is standing on a falling block
  When the player remains in contact for less than 0.5 seconds
  Then the block shall remain suspended

Scenario: Falling block line contains real gaps between blocks
  Given a falling block line is authored in a stage
  When its layout is reviewed
  Then each falling block shall be separated from the next by empty space
  And the gap shall be approximately 1.5 terrain blocks wide

Scenario: Falling block line keeps a clear traversal corridor
  Given a falling block line has an authored footprint
  When the authored corridor is reviewed
  Then the line shall have empty buffer space before and after its footprint
  And each block footprint shall remain clear of surrounding map geometry beneath it

Scenario: Falling block gaps remain non-walkable
  Given the player is traversing a falling block line
  When the player steps into the empty space between two blocks
  Then the player shall fall through the gap

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

Scenario: Falling blocks are not valid enemy support
  Given a falling block line is authored for gameplay use
  When grounded enemies are placed in the stage
  Then those enemies shall not be placed on the falling blocks
```

## Example Inputs/Outputs
- Example input: The player lands on one block in a suspended line of four falling blocks with real gaps between them and no ground beneath them, then remains there for 0.5 seconds.
- Expected output: Only the contacted block begins falling downward after the delay, while neighboring blocks remain suspended.

## Edge Cases
- A falling block authored below four total blocks in a traversal line may still function, but authored gameplay guidance expects lines of four or more blocks for the intended challenge.
- Brief contact from below or the side shall not falsely consume the full delay if sustained player support contact is broken.
- A triggered falling block shall not return to a stable suspended state during the same run.
- A falling block shall not be authored with ordinary ground directly beneath its footprint.
- A falling block shall not be authored so close to neighboring terrain or placed objects that its footprint or side buffer visually clips into adjacent stage geometry.
- Grounded enemies shall not be authored on falling block lines.

## Non-Functional Constraints
- The 0.5 second warning window should still feel readable enough for the player to react.
- Each block fall should read clearly and deterministically once triggered.
- The empty spaces between blocks should remain visually pronounced and should match the real non-walkable collision gaps.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `LEVEL.AUTHORING.001`
- `LEVEL.PROGRESSION.001`
- `CAMERA.FOLLOW.001`