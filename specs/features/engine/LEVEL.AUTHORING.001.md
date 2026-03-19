# Spec: `LEVEL.AUTHORING.001`

## Metadata
- **Title**: Godot Level Scene Authoring with TileSet, TileMap Layers, and Scene Tiles
- **Version**: `v1.3`
- **Status**: Approved
- **Context/View**: Level Authoring
- **Priority**: High

## Purpose
Define how stages are authored in Godot so that collision, decoration, hazards, pickups, and goal content remain editable without fragile one-off scene setups.

## Preconditions
- A stage is being created or revised in the Godot editor.

## Trigger
- New stage authoring, stage iteration, or level content review.

## Requirements
- `LEVEL.AUTHORING.001-R1`: Each stage shall be represented by its own Godot level scene.
- `LEVEL.AUTHORING.001-R2`: Stages shall use reusable terrain authoring primitives, including shared TileSet-based solids and reusable packed terrain scenes where appropriate.
- `LEVEL.AUTHORING.001-R3`: Level scenes shall separate at least solid terrain, decoration, and hazard or gameplay layers to keep editing responsibilities clear.
- `LEVEL.AUTHORING.001-R4`: Coins, power-ups, goal markers, and enemy spawns shall be placeable as scene instances or scene tiles rather than hard-coded coordinates in player scripts.
- `LEVEL.AUTHORING.001-R4A`: Blocks intended to be struck from below shall preserve a minimum vertical clearance beneath them of at least the standing player height, whether by direct authoring validation or runtime normalization.
- `LEVEL.AUTHORING.001-R4B`: Environmental obstacles such as cactus hazards shall be authorable as scene instances or hazard tiles without custom one-off code per stage.
- `LEVEL.AUTHORING.001-R4C`: Repeated gameplay actors and hazards, including the player, enemies, and cactus hazards, shall be authorable as reusable packed scenes rather than duplicated one-off node trees.
- `LEVEL.AUTHORING.001-R4D`: Stage load logic may normalize authored enemy, hazard, block, and goal placement onto valid terrain support so that minor authoring offsets do not leave content floating or buried.
- `LEVEL.AUTHORING.001-R5`: Level data shall expose stage identifier, timer baseline, spawn point, and world bounds.
- `LEVEL.AUTHORING.001-R6`: World bounds authored in the level scene shall be usable by the camera system to prevent scrolling outside the playable area.
- `LEVEL.AUTHORING.001-R7`: Hidden routes and bonus areas shall remain authorable through scene composition without requiring separate code forks per level.
- `LEVEL.AUTHORING.001-R7A`: Stages shall support varied terrain profiles, including rises, drops, uneven ground segments, floating platforms, and reusable hill clusters, rather than requiring long flat runs as the dominant layout pattern.

## Acceptance Criteria (BDD)
```gherkin
Scenario: A stage uses layered tile authoring
  Given a level scene is opened in Godot
  When the scene tree and tile data are reviewed
  Then solid terrain and non-solid decoration shall not be authored on the same logical layer

Scenario: Interactive objects are placeable by scene composition
  Given a designer wants to add a coin or power-up to stage 1-2
  When the level is edited
  Then the object shall be added through scene placement or scene tiles
  And no player script coordinates shall need to be changed

Scenario: Interactive strike blocks maintain playable clearance
  Given a mystery block is placed too close to the floor below it
  When the level is validated or loaded
  Then the block shall be lifted or rejected so the player can hit it from below without clipping into the supporting surface

Scenario: Hazards and uneven terrain are authorable
  Given a designer wants to add cactus obstacles and rolling terrain to a stage
  When the level is edited
  Then those hazards and terrain changes shall be authorable through normal level-scene composition
  And no stage-specific gameplay script fork shall be required

Scenario: Authored placements can be normalized at load time
  Given an enemy or hazard is authored slightly above or below valid terrain support
  When the level is validated or loaded
  Then runtime normalization may snap that content to a valid supported position
  And the stage shall not require a bespoke per-stage correction script

Scenario: Level metadata provides camera and timer context
  Given stage 1-3 is loaded
  When the runtime reads stage metadata
  Then spawn point, timer baseline, stage identifier, and world bounds shall be available
```

## Example Inputs/Outputs
- Example input: A `1-1` level scene containing TileMap-based ground, reusable hill scenes, a player spawn marker, enemy packed scenes, cactus hazard scenes, and a goal scene.
- Expected output: The runtime can load the stage, normalize minor placement offsets, and present reusable authored content without stage-specific logic embedded in the player controller.

## Edge Cases
- Decorative tiles shall not accidentally inherit solid collision when authored on non-solid layers.
- Replacing a scene tile with a newer version shall not require manual updates to every stage instance.
- A block adjusted to preserve hit clearance shall not be moved into solid terrain above it or outside the authored world bounds.
- Hidden routes shall still obey camera and collision boundaries.
- Uneven terrain shall remain readable enough that jumps, hazards, and enemy placement are still telegraphed to the player.
- Runtime support snapping shall not drag content across large gaps or move hazards into unfair hidden placements.

## Non-Functional Constraints
- Level editing should remain fast for designer iteration in the Godot editor.
- Layer naming and purpose should remain obvious without requiring tribal knowledge.

## Related Specs
- `ENGINE.PROJECT.001`
- `PLAYER.MOVEMENT.001`
- `LEVEL.PROGRESSION.001`
- `CAMERA.FOLLOW.001`
