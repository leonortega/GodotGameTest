# Spec: `APP.SHELL.001`

## Metadata
- **Title**: Title Flow, Menus, and Session-Level State Transitions
- **Version**: `v1.6`
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
- `APP.SHELL.001-R3`: The title screen shall provide `Start Game`, `Controls`, `Enemies`, `Configuration`, and `Difficulty` actions.
- `APP.SHELL.001-R3A`: The title screen shall include a looping attract-style animation in which the player runs from right to left while being chased by an enemy or otherwise fleeing danger.
- `APP.SHELL.001-R3B`: Title-screen lettering and primary shell typography shall use an 8-bit or retro pixel-style font treatment appropriate to the game presentation.
- `APP.SHELL.001-R3C`: The title screen shall present a distinct game logo integrated into the title layout rather than text-only labeling.
- `APP.SHELL.001-R3D`: The title-screen chase vignette shall be able to present more than one readable enemy variant rather than a single fixed chaser asset.
- `APP.SHELL.001-R3E`: The title-screen chase vignette shall use real game presentation assets for the background, terrain, player, and enemy preview rather than abstract placeholder blocks or debug shapes.
- `APP.SHELL.001-R3F`: Shell buttons, ribbons, and panel chrome shall use a consistent retro pixel-art UI treatment rather than plain browser-default or flat placeholder controls.
- `APP.SHELL.001-R3G`: Selecting `Controls` from the title screen shall open a help overlay that summarizes the current build's movement, jump, double-jump, attack, and pause inputs.
- `APP.SHELL.001-R3H`: Selecting `Enemies` from the title screen shall open an enemy-guide overlay that presents readable enemy portraits, enemy names, and short behavior or weakness notes for the enemy set used by the current build.
- `APP.SHELL.001-R4`: Selecting `Start Game` shall begin a new session at stage `1-1`.
- `APP.SHELL.001-R4A`: Before a stage becomes interactive, the shell shall display a temporary transition card on a black screen showing the player's current appearance and remaining lives centered on screen.
- `APP.SHELL.001-R4B`: The pre-stage transition card shall remain visible for approximately 3 seconds before gameplay begins.
- `APP.SHELL.001-R4C`: The title screen shall allow the player to cycle the starting difficulty before beginning the run.
- `APP.SHELL.001-R4D`: The title screen shall allow the player to open a configuration menu and adjust persisted audio settings before beginning the run.
- `APP.SHELL.001-R5`: During gameplay, the system shall allow the player to enter a blocking pause state.
- `APP.SHELL.001-R6`: While paused, player input for gameplay actions shall be ignored except for resume and menu navigation.
- `APP.SHELL.001-R7`: While paused, stage simulation and timer countdown shall be suspended.
- `APP.SHELL.001-R8`: When the player loses the final remaining life, the system shall display a game-over screen.
- `APP.SHELL.001-R8A`: The game-over screen shall surface the persisted best score for the current world scope.
- `APP.SHELL.001-R9`: When the player clears stage `1-4`, the system shall display a world-clear or ending screen.
- `APP.SHELL.001-R9A`: The world-clear screen shall surface the run's final score and the persisted highest cleared stage or equivalent saved world-clear progression metric.
- `APP.SHELL.001-R10`: The shell shall support restarting a new game after game over without relaunching the application.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Startup opens on title screen
  Given the game is installed and launchable
  When the player starts the game
  Then the title screen shall be the first visible screen
  And the title screen shall include Start Game, Controls, Enemies, Configuration, and Difficulty actions
  And the title screen shall show the player running right to left while escaping an enemy
  And the title preview shall use real background, terrain, player, and enemy art
  And the title screen typography shall use an 8-bit style font
  And the game logo shall be visible in the title composition
  And the title buttons and shell panels shall use themed retro UI chrome

Scenario: Start Game begins at stage 1-1
  Given the title screen is visible
  When the player selects Start Game
  Then a black pre-stage transition screen shall appear
  And the player's current appearance and remaining lives shall be shown centered on screen
  And after approximately 3 seconds stage 1-1 shall load
  And the player shall spawn at the stage start position

Scenario: Title screen cycles difficulty and opens configuration
  Given the title screen is visible
  When the player activates the Difficulty action
  Then the displayed difficulty shall advance to the next available difficulty
  When the player activates the Configuration action
  Then the configuration menu shall open
  And persisted audio settings shall be adjustable

Scenario: Title actions open controls and enemy guide overlays
  Given the title screen is visible
  When the player activates the Controls action
  Then a help overlay shall open
  And the overlay shall summarize movement, jump, double-jump, attack, and pause inputs
  When the player activates the Enemies action
  Then an enemy-guide overlay shall open
  And the guide shall show enemy portraits, names, and short behavior or weakness notes

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
  And the persisted best score shall be shown
  And the player shall be able to start a new game

Scenario: World clear surfaces saved progression
  Given the player clears stage 1-4
  When the world-clear screen is displayed
  Then the final run score shall be shown
  And the persisted highest clear metric shall be shown
  And the player shall be able to start a new game
```

## Example Inputs/Outputs
- Example input: Startup with no active stage.
- Expected output: Title screen visible with `Start Game`, `Controls`, `Enemies`, `Configuration`, and `Difficulty`, a dedicated logo, retro pixel lettering, themed pixel-art shell chrome, and an animated chase vignette built from actual background, terrain, player, and enemy assets.
- Example input: Title screen difficulty set to `Hard`, then `Start Game` is selected.
- Expected output: Starting the run opens the normal stage-entry card and then loads stage `1-1` using `Hard` difficulty settings.
- Example input: The player opens `Controls` from the title screen.
- Expected output: A help overlay appears listing move, jump, double jump, attack, and pause instructions for the current keyboard-first build.
- Example input: The player opens `Enemies` from the title screen.
- Expected output: An enemy guide appears with readable portraits, enemy names, and short tactical descriptions for the current enemy roster.
- Example input: The player opens `Configuration` and raises music volume.
- Expected output: The visible music setting updates immediately and is persisted for the next launch.
- Example input: New run begins with `3` lives and `Small` form.
- Expected output: A centered black pre-stage card shows the current player appearance and `3` lives for about `3` seconds before gameplay starts.
- Example input: Pause button pressed during stage `1-2`.
- Expected output: Pause overlay visible and timer frozen.
- Example input: The player reaches game over after losing the final life.
- Expected output: The game-over screen shows the persisted best score and offers restart or title navigation.
- Example input: The player clears stage `1-4`.
- Expected output: The world-clear screen shows the final run score and the saved highest-clear progression metric.

## Edge Cases
- Pressing pause on the title screen shall have no gameplay effect.
- Repeated pause input while already paused shall not duplicate overlays or corrupt state.
- Starting a new game after game over shall reset lives, score, coins, and progression.
- The pre-stage transition card shall reflect the current life count and current visual player form, not stale values from a prior stage.
- The title-screen attract animation shall loop cleanly without freezing the menu state or obscuring the menu actions.
- The logo shall remain readable over the title animation and shall not collide visually with menu actions.
- The title preview art shall remain readable at menu scale and shall not visually overpower the action buttons.
- The `Controls` and `Enemies` overlays shall return to the title screen without corrupting the currently selected difficulty or persisted configuration state.
- Cycling difficulty on the title screen shall update the visible label immediately without leaving the title screen.
- Configuration changes made from the title menu shall update the persisted setting without requiring a gameplay run.

## Non-Functional Constraints
- Transition from title screen to gameplay should feel immediate and consistent.
- Pause and resume should not visibly desynchronize timer, enemy, or animation state.

## Related Specs
- `ENGINE.PROJECT.001`
- `PLAYER.INPUT.001`
- `LEVEL.PROGRESSION.001`
- `HUD.STATUS.001`
- `SAVE.PROGRESSION.001`
