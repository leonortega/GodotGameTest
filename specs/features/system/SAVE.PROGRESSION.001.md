# Spec: `SAVE.PROGRESSION.001`

## Metadata
- **Title**: Save Slot, Settings Persistence, and World Progress Tracking
- **Version**: `v1.1`
- **Status**: Approved
- **Context/View**: System Persistence
- **Priority**: Medium

## Purpose
Define what progression and settings data persist across launches in the Godot implementation.

## Preconditions
- The player starts, clears, or exits a run.

## Trigger
- New game start, stage clear, settings change, or application shutdown.

## Requirements
- `SAVE.PROGRESSION.001-R1`: The project shall support at least one save slot for local persistence.
- `SAVE.PROGRESSION.001-R2`: The save slot shall preserve player settings needed for subsequent launches, including music volume, SFX volume, and the selected difficulty.
- `SAVE.PROGRESSION.001-R2A`: The shell shall expose a configuration menu that allows the player to adjust persisted audio settings without starting a gameplay run.
- `SAVE.PROGRESSION.001-R3`: The save slot shall preserve progression data at minimum for highest cleared stage and best recorded score for the current world scope.
- `SAVE.PROGRESSION.001-R4`: Starting a new run shall not require existing save data.
- `SAVE.PROGRESSION.001-R5`: Save data shall be written as JSON compatible with Godot file I/O.
- `SAVE.PROGRESSION.001-R6`: Corrupted or missing save data shall fall back to default values without blocking launch.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Stage clear updates persistent progression
  Given the player clears stage 1-2
  When progression is saved
  Then the highest cleared stage shall be persisted

Scenario: Relaunch restores saved settings
  Given the player previously changed audio settings
  When the application launches again
  Then the saved settings shall be restored

Scenario: Configuration changes persist immediately
  Given the title configuration menu is open
  When the player changes music or SFX volume
  Then the updated settings shall be saved to JSON
  And the same values shall be restored on the next launch

Scenario: Missing save data falls back safely
  Given no prior save file exists
  When the game launches
  Then the game shall start with default settings and progression
```

## Example Inputs/Outputs
- Example input: New save JSON after first launch with no cleared stages.
- Expected output: Default difficulty and audio settings plus zero progression recorded.

## Edge Cases
- Save load failure shall not trap the player on a blocking error screen.
- Clearing a later stage shall not regress previously stored best score or highest cleared stage.
- Settings-only changes shall not require a completed gameplay run before being saved.
- Configuration changes shall apply immediately to active audio buses and persist without restarting the application.

## Non-Functional Constraints
- Save handling should remain simple enough for debugging with plain-text inspection when feasible.
- Persistence should avoid frequent write bursts during moment-to-moment gameplay.

## Related Specs
- `APP.SHELL.001`
- `ENGINE.PROJECT.001`
- `LEVEL.PROGRESSION.001`
- `AUDIO.SYSTEM.001`
