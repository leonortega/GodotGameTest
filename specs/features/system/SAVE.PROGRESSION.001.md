# Spec: `SAVE.PROGRESSION.001`

## Metadata
- **Title**: Settings Persistence for an SMB1-Style Run Flow
- **Version**: `v1.2`
- **Status**: Approved
- **Context/View**: System Persistence
- **Priority**: Medium

## Purpose
Define the limited settings persistence needed by the SMB1-style shell flow without requiring persistent run progression, best-score surfacing, or difficulty selection in the core menu flow.

## Preconditions
- The player starts, clears, or exits a run.

## Trigger
- New game start, stage clear, settings change, or application shutdown.

## Requirements
- `SAVE.PROGRESSION.001-R1`: The project shall support at least one local settings save payload for persistence across launches.
- `SAVE.PROGRESSION.001-R2`: The save payload shall preserve player settings needed for subsequent launches, including music volume and SFX volume.
- `SAVE.PROGRESSION.001-R3`: Starting a new run shall not require existing save data.
- `SAVE.PROGRESSION.001-R4`: Save data shall be written as JSON compatible with Godot file I/O.
- `SAVE.PROGRESSION.001-R5`: Corrupted or missing save data shall fall back to default values without blocking launch.
- `SAVE.PROGRESSION.001-R5A`: When a prior local save uses an older JSON shape, the runtime shall migrate or map supported settings data into the current save structure without blocking launch.
- `SAVE.PROGRESSION.001-R6`: Persistent best-score messaging, highest-cleared-stage messaging, and difficulty selection shall not be required parts of the SMB1-style shell flow.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Relaunch restores saved settings
  Given the player previously changed audio settings
  When the application launches again
  Then the saved settings shall be restored

Scenario: Settings changes persist immediately
  Given the settings interface is open
  When the player changes music or SFX volume
  Then the updated settings shall be saved to JSON
  And the same values shall be restored on the next launch

Scenario: Missing save data falls back safely
  Given no prior save file exists
  When the game launches
  Then the game shall start with default settings and a fresh run state

Scenario: Legacy save data migrates forward
  Given a prior local save exists using an older flat JSON structure
  When the game launches on the current build
  Then the stored supported settings shall be mapped into the current save structure
  And launch shall continue without a blocking migration error
```

## Example Inputs/Outputs
- Example input: New save JSON after first launch.
- Expected output: Default audio settings are recorded with no requirement for saved run progression.
- Example input: An older save JSON containing top-level `musicVolumeDb` and `sfxVolumeDb`.
- Expected output: The current build reads those settings values and persists them back using the current save shape.

## Edge Cases
- Save load failure shall not trap the player on a blocking error screen.
- Settings-only changes shall not require a completed gameplay run before being saved.
- Configuration changes shall apply immediately to active audio buses and persist without restarting the application.
- Legacy migration shall not silently drop supported audio settings when equivalent current fields exist.

## Non-Functional Constraints
- Save handling should remain simple enough for debugging with plain-text inspection when feasible.
- Persistence should avoid frequent write bursts during moment-to-moment gameplay.

## Related Specs
- `APP.SHELL.001`
- `ENGINE.PROJECT.001`
- `AUDIO.SYSTEM.001`
