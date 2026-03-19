# Spec: `ENGINE.PROJECT.001`

## Metadata
- **Title**: Godot Project Architecture, Scene Composition, and Shared Services
- **Version**: `v1.1`
- **Status**: Approved
- **Context/View**: Engine Foundation
- **Priority**: High

## Purpose
Define the Godot project structure required to implement the game cleanly and keep gameplay scenes reusable.

## Preconditions
- The project is being authored in Godot 4.x.

## Trigger
- Project bootstrap or architecture review before gameplay implementation.

## Requirements
- `ENGINE.PROJECT.001-R1`: The game shall target Godot 4.x for all authored scenes and scripts.
- `ENGINE.PROJECT.001-R2`: The project shall separate reusable gameplay elements into instantiable scenes, including player, enemy, pickup, HUD, and level scenes.
- `ENGINE.PROJECT.001-R3`: The main runtime flow shall be represented by a root scene that can transition between title, gameplay, pause, game-over, and world-clear states.
- `ENGINE.PROJECT.001-R4`: Shared cross-scene state shall be managed through at least one Autoload responsible for run/session state.
- `ENGINE.PROJECT.001-R5`: Shared audio control shall be managed through an Autoload or equivalent globally reachable audio service.
- `ENGINE.PROJECT.001-R6`: Scene composition shall favor `PackedScene` instancing over duplicated scene trees for repeated content.
- `ENGINE.PROJECT.001-R7`: Input actions shall be defined in the Godot Input Map rather than embedded directly in gameplay scripts.
- `ENGINE.PROJECT.001-R8`: The project shall organize scripts, scenes, art, audio, and data resources into stable directories to support scaling beyond the MVP world.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Root scene controls the major runtime states
  Given the Godot project is opened
  When the main scene is inspected
  Then it shall support title, gameplay, pause, game-over, and world-clear flow

Scenario: Shared runtime state is available across scenes
  Given the player enters gameplay from the title screen
  When the scene changes from title to stage
  Then run state such as lives, score, coins, and current stage shall remain available

Scenario: Reusable scenes are instanced instead of duplicated
  Given multiple enemy or pickup objects are placed in a level
  When the level scene is reviewed
  Then repeated gameplay objects shall be represented as reusable scene instances or scene tiles
```

## Example Inputs/Outputs
- Example input: Main scene loads title, applies the selected starting stage, and then instantiates the chosen gameplay stage.
- Expected output: Session state persists without duplicating gameplay logic across scenes, regardless of which valid title-selected stage starts the run.

## Edge Cases
- Reloading a stage after death shall not duplicate Autoload singletons.
- Returning to title from a paused game shall reset run state only when explicitly starting a new game.
- Reusing the same enemy scene in multiple stages shall not require copy-pasted scripts.

## Non-Functional Constraints
- Scene ownership should remain readable for solo or small-team iteration.
- Project structure should allow feature work to stay localized to one subsystem at a time.

## Related Specs
- `APP.SHELL.001`
- `PLAYER.INPUT.001`
- `LEVEL.AUTHORING.001`
- `AUDIO.SYSTEM.001`
- `SAVE.PROGRESSION.001`
