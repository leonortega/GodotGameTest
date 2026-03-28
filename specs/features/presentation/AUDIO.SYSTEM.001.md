# Spec: `AUDIO.SYSTEM.001`

## Metadata
- **Title**: Music, Sound Effects, and Godot Audio Bus Organization
- **Version**: `v1.3`
- **Status**: Approved
- **Context/View**: Presentation
- **Priority**: Medium

## Purpose
Define an audio system that supports retro readability, stage identity, and maintainable Godot project organization.

## Preconditions
- Audio assets and playback logic are being integrated into the Godot project.

## Trigger
- Stage start, pickup events, damage events, pause, or menu navigation.

## Requirements
- `AUDIO.SYSTEM.001-R1`: The project shall define separate audio handling for background music, sound effects, and UI feedback.
- `AUDIO.SYSTEM.001-R2`: Godot audio buses shall at minimum separate `Music`, `SFX`, and `UI`.
- `AUDIO.SYSTEM.001-R3`: Each stage family shall support a distinct looping background track or intentional reuse strategy.
- `AUDIO.SYSTEM.001-R3A`: Active stage music shall continue looping without unintended silence until it is replaced, paused, or explicitly stopped by a higher-priority game state.
- `AUDIO.SYSTEM.001-R3B`: The shell shall provide authored music coverage for at least the title, active stage, game-over, and world-clear states using real audio assets rather than placeholder silence.
- `AUDIO.SYSTEM.001-R4`: Jump, double jump, landing after a double jump, coin, power-up, power-down, enemy defeat, death, extra life, pause, and stage-clear events shall have distinct sound effect coverage.
- `AUDIO.SYSTEM.001-R4A`: Menu navigation, hover, back, or shell confirmation actions shall provide distinct UI feedback using authored audio clips rather than generated placeholder tones.
- `AUDIO.SYSTEM.001-R4B`: Gameplay and shell feedback sounds may be sourced from reused retro asset packs, provided each covered event remains readable and intentionally mapped.
- `AUDIO.SYSTEM.001-R5`: Audio changes triggered by pause, game over, or world clear shall be readable and not leave overlapping music states active.
- `AUDIO.SYSTEM.001-R6`: Audio service behavior shall remain reachable across scenes without duplicating mixer logic in every level scene.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Stage music starts when gameplay begins
  Given stage 1-1 loads
  When gameplay becomes active
  Then the stage music shall start on the Music bus

Scenario: Stage music does not stop after one pass
  Given stage gameplay music is active
  When the current music track reaches its end
  Then the same stage music shall continue looping without a silent gap caused by missing loop configuration

Scenario: Core actions emit readable sound effects
  Given the player jumps and then collects a coin
  When the actions are resolved
  Then distinct jump and coin sound effects shall be played on the SFX bus

Scenario: Double-jump chain uses distinct traversal sounds
  Given the player performs a grounded jump and then a double jump
  When the player lands on terrain after the double jump
  Then distinct jump, double-jump, and landing sound effects shall be played

Scenario: Power loss and death use distinct cues
  Given the player is damaged once while powered and later loses a life
  When both outcomes resolve
  Then power-down and death shall not reuse the same final sound cue

Scenario: Shell states use authored music assets
  Given the title screen is visible
  When the player remains on the title or reaches game over or world clear
  Then the shell shall use the authored music asset mapped to that state

Scenario: Pause does not leave audio state ambiguous
  Given gameplay music is active
  When the player pauses the game
  Then the audio system shall apply the defined pause behavior consistently
```

## Example Inputs/Outputs
- Example input: Audio bus layout with `Master`, `Music`, `SFX`, and `UI`.
- Expected output: Music and effects can be mixed independently across title, gameplay, game-over, and world-clear scenes while using authored audio files for stage loops, event cues, and UI feedback.
- Example input: The player jumps, double jumps, lands, powers up, powers down, and then dies.
- Expected output: Each event plays its own mapped cue without collapsing traversal, form-change, and death feedback into one reused sound.

## Edge Cases
- Rapid coin collection shall not clip or permanently mute other effects.
- Re-entering the same stage after death shall not stack duplicate background music instances.
- Imported stage music lacking an asset-level loop flag shall still loop through runtime control or safe restart behavior.
- Missing optional audio assets shall fail gracefully without breaking runtime flow.
- Reused asset-pack sounds shall still remain semantically distinct enough that menu, jump, hit, and clear feedback are not confused with one another.
- Double-jump landing audio shall not trigger on a normal grounded step or on a failed landing that ends in a pit death.

## Non-Functional Constraints
- Audio routing should remain simple enough for one-developer maintenance.
- Audio cues should prioritize gameplay feedback over dense layering.

## Related Specs
- `APP.SHELL.001`
- `ENGINE.PROJECT.001`
- `LEVEL.PROGRESSION.001`
