# Spec: `LEVEL.AUTHORING.001`

## Metadata
- **Title**: Godot Level Scene Authoring with TileSet, TileMap Layers, and Scene Tiles
- **Version**: `v1.6`
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
- `LEVEL.AUTHORING.001-R3A`: Stage presentation shall support reusable background art layering, such as sky, clouds, mountains, hills, and comparable scenic elements, without baking those visuals into the collision layer.
- `LEVEL.AUTHORING.001-R3B`: Terrain, platforms, and hazard structures shall support reusable tile or sprite art for ground tops, fills, and special blocks so that stage presentation does not rely on debug rectangles or flat-color placeholders.
- `LEVEL.AUTHORING.001-R4`: Coins, power-ups, goal markers, and enemy spawns shall be placeable as scene instances or scene tiles rather than hard-coded coordinates in player scripts.
- `LEVEL.AUTHORING.001-R4A`: Blocks intended to be struck from below shall preserve a minimum vertical clearance beneath them of at least the standing player height, whether by direct authoring validation or runtime normalization.
- `LEVEL.AUTHORING.001-R4AA`: Blocks intended to be struck from below shall also remain within a reachable head-hit height from the supported floor beneath them so that the player can jump under the block and activate it without clipping or requiring unsupported movement.
- `LEVEL.AUTHORING.001-R4B`: Environmental obstacles such as cactus hazards shall be authorable as scene instances or hazard tiles without custom one-off code per stage.
- `LEVEL.AUTHORING.001-R4C`: Repeated gameplay actors and hazards, including the player, enemies, and cactus hazards, shall be authorable as reusable packed scenes rather than duplicated one-off node trees.
- `LEVEL.AUTHORING.001-R4D`: Stage load logic may normalize authored enemy, hazard, block, and goal placement onto valid terrain support so that minor authoring offsets do not leave content floating or buried.
- `LEVEL.AUTHORING.001-R4E`: Dynamic traversal elements such as floating moving platforms and timed falling blocks shall be authorable through reusable scene composition without stage-specific gameplay forks.
- `LEVEL.AUTHORING.001-R4F`: Falling block lines shall be authorable as repeated single falling-block scene instances, and grounded enemies shall not rely on those falling blocks as stable authored support.
- `LEVEL.AUTHORING.001-R4G`: Coins, power-ups, interactive blocks, platforms, terrain, hazards, enemy spawns, and goal markers shall be authored without invalid spatial overlap, and authoring validation or runtime normalization shall prevent collisions such as pickups buried in platforms or pickups intersecting interactive blocks.
- `LEVEL.AUTHORING.001-R5`: Level data shall expose stage identifier, timer baseline, spawn point, and world bounds.
- `LEVEL.AUTHORING.001-R6`: World bounds authored in the level scene shall be usable by the camera system to prevent scrolling outside the playable area.
- `LEVEL.AUTHORING.001-R7`: Hidden routes and bonus areas shall remain authorable through scene composition without requiring separate code forks per level.
- `LEVEL.AUTHORING.001-R7A`: Stages shall support varied terrain profiles, including rises, drops, uneven ground segments, floating platforms, and reusable hill clusters, rather than requiring long flat runs as the dominant layout pattern.
- `LEVEL.AUTHORING.001-R7B`: Stages shall support authored slope segments and connected ramp or plateau terrain definitions that the runtime can convert into valid collision and readable terrain presentation without bespoke stage code.
- `LEVEL.AUTHORING.001-R7C`: Platforms and other intended traversal surfaces shall be authored so that the player can reach the next required landing surface using at most the build's supported double jump as the maximum vertical traversal ability.
- `LEVEL.AUTHORING.001-R7D`: Terrain solids and interactive block pieces shall remain proportionate to the authored player and enemy scale so that stage pieces do not read as implausibly oversized or too tiny for readable traversal and combat spacing.

## Acceptance Criteria (BDD)
```gherkin
Scenario: A stage uses layered tile authoring
  Given a level scene is opened in Godot
  When the scene tree and tile data are reviewed
  Then solid terrain and non-solid decoration shall not be authored on the same logical layer

Scenario: Background art and terrain art remain authorable
  Given a designer wants to add scenic background layers and stylized terrain tiles to a stage
  When the level is edited
  Then those visuals shall be authorable through reusable background and terrain assets
  And collision behavior shall remain separate from decorative presentation layers

Scenario: Interactive objects are placeable by scene composition
  Given a designer wants to add a coin or power-up to stage 1-2
  When the level is edited
  Then the object shall be added through scene placement or scene tiles
  And no player script coordinates shall need to be changed

Scenario: Interactive strike blocks maintain playable clearance
  Given a mystery block is placed too close to the floor below it
  When the level is validated or loaded
  Then the block shall be lifted or rejected so the player can hit it from below without clipping into the supporting surface

Scenario: Interactive strike blocks remain reachable from below
  Given a mystery block is authored above valid supporting floor
  When the level is validated or loaded
  Then the block shall remain within reachable jump-hit height for the player
  And the player shall not need movement beyond the supported jump rules to activate it from below

Scenario: Hazards and uneven terrain are authorable
  Given a designer wants to add cactus obstacles and rolling terrain to a stage
  When the level is edited
  Then those hazards and terrain changes shall be authorable through normal level-scene composition
  And no stage-specific gameplay script fork shall be required

Scenario: Intended platforms remain reachable
  Given a designer authors a required platforming path across elevated surfaces
  When the level is validated or loaded
  Then each required landing surface shall be reachable with at most the supported double jump
  And the route shall not require an unintended third jump or debug-only movement ability

Scenario: Gameplay pieces remain proportionate and non-overlapping
  Given a designer authors terrain, interactive blocks, and pickups in the same section
  When the level is validated or loaded
  Then terrain and block dimensions shall remain proportionate to the player and enemy scale
  And pickups, blocks, platforms, and hazards shall not intersect in invalid ways

Scenario: Slope terrain is authorable
  Given a designer wants to add an uphill run or a paired slope plateau section
  When the level is edited
  Then the slope terrain shall be authorable through reusable stage data or scene composition
  And runtime collision and terrain presentation shall remain valid without bespoke per-stage code

Scenario: Authored placements can be normalized at load time
  Given an enemy or hazard is authored slightly above or below valid terrain support
  When the level is validated or loaded
  Then runtime normalization may snap that content to a valid supported position
  And the stage shall not require a bespoke per-stage correction script

Scenario: Dynamic traversal elements are authorable
  Given a designer wants to add a floating moving platform or a falling block line to a stage
  When the level is edited
  Then those traversal elements shall be authorable through normal level-scene composition
  And no bespoke per-stage gameplay script fork shall be required

Scenario: Falling blocks are not enemy support
  Given a designer authors falling blocks in a traversal section
  When grounded enemies are placed for the stage
  Then those enemies shall be placed on stable terrain or other approved support
  And the falling blocks shall not be treated as valid resting support for grounded enemy placement

Scenario: Level metadata provides camera and timer context
  Given stage 1-3 is loaded
  When the runtime reads stage metadata
  Then spawn point, timer baseline, stage identifier, and world bounds shall be available
```

## Example Inputs/Outputs
- Example input: A `1-1` level scene containing TileMap-based ground, reusable hill scenes, floating moving platforms, repeated falling block scene instances, a player spawn marker, enemy packed scenes, cactus hazard scenes, and a goal scene.
- Expected output: The runtime can load the stage, normalize minor placement offsets, and present reusable authored content with layered background art and terrain tiles without stage-specific logic embedded in the player controller.
- Example input: A stage scene containing paired authored slope definitions for a raised plateau.
- Expected output: The runtime builds readable sloped terrain presentation and walkable support without a stage-specific slope script.
- Example input: A floating platform path authored between two terrain shelves.
- Expected output: The route is reachable using the supported double jump and does not require impossible vertical spacing between required landings.
- Example input: A mystery block authored above the floor with a coin nearby.
- Expected output: The block remains within reachable jump-hit height, and the nearby coin does not intersect the block or any supporting platform.

## Edge Cases
- Decorative tiles shall not accidentally inherit solid collision when authored on non-solid layers.
- Replacing a scene tile with a newer version shall not require manual updates to every stage instance.
- A block adjusted to preserve hit clearance shall not be moved into solid terrain above it or outside the authored world bounds.
- A block adjusted to preserve jump-hit reachability shall not be moved so high that the player can stand below it but still cannot activate it.
- Hidden routes shall still obey camera and collision boundaries.
- Uneven terrain shall remain readable enough that jumps, hazards, and enemy placement are still telegraphed to the player.
- Runtime support snapping shall not drag content across large gaps or move hazards into unfair hidden placements.
- Background art density shall not reduce foreground readability for the player, enemies, or pickups.
- Slope definitions shall not generate broken collision seams that trap the player or enemies at slope transitions.
- Reachability validation shall not misclassify optional bonus jumps as required-route failures when the main route remains completable.
- Spatial normalization shall not bury pickups inside terrain, platforms, or interactive blocks.

## Non-Functional Constraints
- Level editing should remain fast for designer iteration in the Godot editor.
- Layer naming and purpose should remain obvious without requiring tribal knowledge.

## Related Specs
- `ENGINE.PROJECT.001`
- `PLAYER.MOVEMENT.001`
- `LEVEL.PROGRESSION.001`
- `CAMERA.FOLLOW.001`
- `LEVEL.DYNAMICS.003`
