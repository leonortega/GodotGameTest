# Spec: `CAMERA.FOLLOW.001`

## Metadata
- **Title**: Camera2D Follow, Limits, and Readability Rules
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Presentation
- **Priority**: High

## Purpose
Define a Godot Camera2D behavior that supports forward-looking platforming without showing outside authored stage bounds.

## Preconditions
- A stage is loaded and the player is active.

## Trigger
- Player movement, stage load, or camera configuration review.

## Requirements
- `CAMERA.FOLLOW.001-R1`: Gameplay camera behavior shall be implemented with Godot `Camera2D`.
- `CAMERA.FOLLOW.001-R2`: The camera shall follow the player horizontally through the stage.
- `CAMERA.FOLLOW.001-R3`: The camera shall not reveal space outside authored world bounds.
- `CAMERA.FOLLOW.001-R4`: The camera shall bias readability toward forward movement so upcoming hazards can be seen in time.
- `CAMERA.FOLLOW.001-R5`: Vertical camera movement shall avoid jitter from short jumps and minor landing corrections.
- `CAMERA.FOLLOW.001-R6`: Camera settings shall be consistent across stages unless a stage-specific override is intentional and documented.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Camera follows the player through the stage
  Given the player moves to the right in stage 1-1
  When the player advances through the level
  Then the camera shall track horizontal progress

Scenario: Camera remains inside stage bounds
  Given the player is near the left or right edge of a stage
  When the camera updates
  Then the camera view shall not expose outside-of-level space

Scenario: Minor jumps do not cause distracting vertical shake
  Given the player performs repeated short jumps on flat terrain
  When the camera follows the player
  Then the view shall remain readable without excessive vertical jitter
```

## Example Inputs/Outputs
- Example input: Stage metadata defines left and right camera limits for `1-3`.
- Expected output: The Camera2D follows the player while staying inside those limits.

## Edge Cases
- Entering a hidden route shall still use valid stage bounds or a documented sub-area override.
- Camera smoothing shall not lag so far behind that platforming becomes unreadable.
- Respawn after death shall restore camera position cleanly near the spawn point.

## Non-Functional Constraints
- Camera behavior should preserve readable reaction time for hazards and enemies.
- Any smoothing or drag margins should favor gameplay clarity over cinematic motion.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `LEVEL.AUTHORING.001`
- `HUD.STATUS.001`
