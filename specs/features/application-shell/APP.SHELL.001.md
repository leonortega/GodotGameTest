# Spec: `APP.SHELL.001`

## Metadata
- **Title**: Title Flow, Menus, and Session-Level State Transitions
- **Version**: `v1.3`
- **Status**: Approved
- **Context/View**: Application Shell
- **Priority**: High

## Purpose
Define how the game starts, enters play, pauses, and transitions to clear or game-over states.

## Preconditions
- The player launches `Super Pixel Quest`.

## Trigger
- Game startup or shell-level menu interaction.

## Requirements
- `APP.SHELL.001-R1`: The system shall be a side-scrolling platformer named `Super Pixel Quest`.
- `APP.SHELL.001-R2`: On startup, the system shall display a title screen before gameplay begins.
- `APP.SHELL.001-R3`: The title screen shall provide at least `Start Game` and `Controls` actions.
- `APP.SHELL.001-R3A`: The title screen shall include a looping attract-style animation in which the player runs from right to left while being chased by an enemy or otherwise fleeing danger.
- `APP.SHELL.001-R3B`: Title-screen lettering and primary shell typography shall use an 8-bit or retro pixel-style font treatment appropriate to the game presentation.
- `APP.SHELL.001-R3C`: The title screen shall present a distinct game logo integrated into the title layout rather than text-only labeling.
- `APP.SHELL.001-R4`: Selecting `Start Game` shall begin a new session at stage `1-1`.
- `APP.SHELL.001-R4A`: Before a stage becomes interactive, the shell shall display a temporary transition card on a black screen showing the player's current appearance and remaining lives centered on screen.
- `APP.SHELL.001-R4B`: The pre-stage transition card shall remain visible for approximately 3 seconds before gameplay begins.
- `APP.SHELL.001-R5`: During gameplay, the system shall allow the player to enter a blocking pause state.
- `APP.SHELL.001-R6`: While paused, player input for gameplay actions shall be ignored except for resume and menu navigation.
- `APP.SHELL.001-R7`: While paused, stage simulation and timer countdown shall be suspended.
- `APP.SHELL.001-R8`: When the player loses the final remaining life, the system shall display a game-over screen.
- `APP.SHELL.001-R9`: When the player clears stage `1-4`, the system shall display a world-clear or ending screen.
- `APP.SHELL.001-R10`: The shell shall support restarting a new game after game over without relaunching the application.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Startup opens on title screen
  Given the game is installed and launchable
  When the player starts the game
  Then the title screen shall be the first visible screen
  And the title screen shall include Start Game and Controls actions
  And the title screen shall show the player running right to left while escaping an enemy
  And the title screen typography shall use an 8-bit style font
  And the game logo shall be visible in the title composition

Scenario: Start Game begins the first stage
  Given the title screen is visible
  When the player selects Start Game
  Then a black pre-stage transition screen shall appear
  And the player's current appearance and remaining lives shall be shown centered on screen
  And after approximately 3 seconds stage 1-1 shall load
  And the player shall spawn at the stage start position

Scenario: Pause suspends active play
  Given the player is in an active stage
  When the player presses pause
  Then the game shall enter pause state
  And stage timer countdown shall stop
  And enemy movement shall stop

Scenario: Game over appears after the last life is lost
  Given the player has 1 remaining life
  When the player loses that life
  Then the game-over screen shall be displayed
  And the player shall be able to start a new game
```

## Example Inputs/Outputs
- Example input: Startup with no active stage.
- Expected output: Title screen visible with `Start Game` and `Controls`, a dedicated logo, retro pixel lettering, and an animated chase vignette.
- Example input: New run begins with `3` lives and `Small` form.
- Expected output: A centered black pre-stage card shows the current player appearance and `3` lives for about `3` seconds before gameplay starts.
- Example input: Pause button pressed during stage `1-2`.
- Expected output: Pause overlay visible and timer frozen.

## Edge Cases
- Pressing pause on the title screen shall have no gameplay effect.
- Repeated pause input while already paused shall not duplicate overlays or corrupt state.
- Starting a new game after game over shall reset lives, score, coins, and progression.
- The pre-stage transition card shall reflect the current life count and current visual player form, not stale values from a prior stage.
- The title-screen attract animation shall loop cleanly without freezing the menu state or obscuring the menu actions.
- The logo shall remain readable over the title animation and shall not collide visually with menu actions.

## Non-Functional Constraints
- Transition from title screen to gameplay should feel immediate and consistent.
- Pause and resume should not visibly desynchronize timer, enemy, or animation state.

## Related Specs
- `ENGINE.PROJECT.001`
- `PLAYER.INPUT.001`
- `LEVEL.PROGRESSION.001`
- `HUD.STATUS.001`
- `SAVE.PROGRESSION.001`
