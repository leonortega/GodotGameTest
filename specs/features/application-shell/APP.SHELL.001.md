# Spec: `APP.SHELL.001`

## Metadata
- **Title**: Title Flow, Menus, and Session-Level State Transitions
- **Version**: `v1.7`
- **Status**: Approved
- **Context/View**: Application Shell
- **Priority**: High

## Purpose
Define a linear retro platformer shell flow with a simple title screen, direct stage entry, no world-map hub, minimal player-facing transition screens, and a compact pause menu.

## Preconditions
- The player launches `Super Pixel Quest`.

## Trigger
- Game startup or shell-level menu interaction.

## Requirements
- `APP.SHELL.001-R1`: The system shall be a side-scrolling platformer named `Super Pixel Quest`.
- `APP.SHELL.001-R2`: On startup, the system shall display a simple title screen before gameplay begins.
- `APP.SHELL.001-R3`: The title screen shall provide a primary `Start Game` action for the single-player SMB1-style run flow.
- `APP.SHELL.001-R3A`: The title screen shall include a looping attract-style animation in which the player runs from right to left while being chased by an enemy or otherwise fleeing danger.
- `APP.SHELL.001-R3B`: Title-screen lettering and primary shell typography shall use an 8-bit or retro pixel-style font treatment appropriate to the game presentation.
- `APP.SHELL.001-R3C`: The title screen shall present a distinct game logo integrated into the title layout rather than text-only labeling.
- `APP.SHELL.001-R3D`: The title-screen chase vignette shall be able to present more than one readable enemy variant rather than a single fixed chaser asset.
- `APP.SHELL.001-R3E`: The title-screen chase vignette shall use real game presentation assets for the background, terrain, player, and enemy preview rather than abstract placeholder blocks or debug shapes.
- `APP.SHELL.001-R3F`: Shell buttons, ribbons, and panel chrome shall use a consistent retro pixel-art UI treatment rather than plain browser-default or flat placeholder controls.
- `APP.SHELL.001-R4`: Selecting `Start Game` shall begin a new session at stage `1-1`.
- `APP.SHELL.001-R4A`: Before a stage becomes interactive, the shell shall display a temporary transition card on a black screen showing the current world-stage identifier and remaining lives centered on screen.
- `APP.SHELL.001-R4B`: The pre-stage transition card shall remain visible for approximately 3 seconds before gameplay begins.
- `APP.SHELL.001-R4C`: The pre-stage transition card shall use a minimal SMB1-style centered layout that presents the world-stage identifier above a centered player marker and remaining-life count.
- `APP.SHELL.001-R5`: During gameplay, the system shall allow the player to enter a blocking pause state.
- `APP.SHELL.001-R5A`: The default desktop pause control shall include `Escape`.
- `APP.SHELL.001-R5B`: While paused, the shell shall present a minimal pause menu offering `Resume`, `Restart Level`, and `Title`.
- `APP.SHELL.001-R5C`: Selecting `Restart Level` from the pause menu shall reload the current stage from its stage start without consuming an additional life.
- `APP.SHELL.001-R5D`: Selecting `Title` from the pause menu shall abandon the active run and return to the title screen.
- `APP.SHELL.001-R5E`: Pressing the pause control again while the pause menu is open may resume play directly if the current build supports pause-toggle behavior.
- `APP.SHELL.001-R6`: While paused, player input for gameplay actions shall be ignored except for resume, pause-menu navigation, and pause-menu confirmation.
- `APP.SHELL.001-R7`: While paused, stage simulation and timer countdown shall be suspended.
- `APP.SHELL.001-R8`: When the player loses a life and still has remaining lives, the shell shall transition through a short death-to-restart flow and then reload the same stage from its stage start.
- `APP.SHELL.001-R8A`: The non-final death restart flow shall reuse the stage-entry-style black transition card and shall show the current world-stage identifier and updated remaining lives.
- `APP.SHELL.001-R9`: When the player loses the final remaining life, the system shall display a simple `Game Over` screen.
- `APP.SHELL.001-R9A`: The game-over screen shall not depend on persisted best-score or progression messaging to communicate the outcome.
- `APP.SHELL.001-R9B`: The game-over screen shall remain text-first and minimal, offering only the actions required to begin a fresh run or return to title.
- `APP.SHELL.001-R10`: When the player clears stage `1-4`, the system shall display a simple world-clear or ending screen appropriate to the end of the SMB1-style run.
- `APP.SHELL.001-R10A`: World progression shall move directly from one stage to the next without a hub or world-map screen between stages.
- `APP.SHELL.001-R10B`: World-clear presentation shall remain minimal and shall not expand into a modern stat-heavy summary dashboard.
- `APP.SHELL.001-R11`: The shell shall support restarting a new game after game over without relaunching the application.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Startup opens on title screen
  Given the game is installed and launchable
  When the player starts the game
  Then the title screen shall be the first visible screen
  And the title screen shall include a Start Game action
  And the title screen shall show the player running right to left while escaping an enemy
  And the title preview shall use real background, terrain, player, and enemy art
  And the title screen typography shall use an 8-bit style font
  And the game logo shall be visible in the title composition
  And the title buttons and shell panels shall use themed retro UI chrome

Scenario: Start Game begins at stage 1-1
  Given the title screen is visible
  When the player selects Start Game
  Then a black pre-stage transition screen shall appear
  And the current world-stage identifier and remaining lives shall be shown centered on screen
  And after approximately 3 seconds stage 1-1 shall load
  And the player shall spawn at the stage start position

Scenario: Pause suspends active play
  Given the player is in an active stage
  When the player presses pause
  Then the game shall enter pause state
  And stage timer countdown shall stop
  And enemy movement shall stop

Scenario: Pause menu offers restart and title navigation
  Given the player is in an active stage
  When the player presses Escape
  Then the pause menu shall be displayed
  And the menu shall offer Resume, Restart Level, and Title
  When the player selects Restart Level
  Then the current stage shall reload from its stage start
  And no additional life shall be consumed

Scenario: Non-final death restarts the current stage
  Given the player has more than 1 remaining life
  When the player loses a life
  Then the shell shall enter a short death-to-restart transition
  And the current world-stage identifier and updated remaining lives shall be shown
  And the same stage shall restart from its stage start

Scenario: Game over appears after the last life is lost
  Given the player has 1 remaining life
  When the player loses that life
  Then the game-over screen shall be displayed
  And the player shall be able to start a new game

Scenario: World clear ends the linear run without a hub
  Given the player clears stage 1-4
  When the world-clear screen is displayed
  Then the completed run shall end without entering a world-map or hub screen
  And the player shall be able to start a new game
```

## Example Inputs/Outputs
- Example input: Startup with no active stage.
- Expected output: Title screen visible with `Start Game`, a dedicated logo, retro pixel lettering, themed pixel-art shell chrome, and an animated chase vignette built from actual background, terrain, player, and enemy assets.
- Example input: New run begins with `3` lives and `Small` form.
- Expected output: A centered black pre-stage card shows world `1-1` and `3` lives for about `3` seconds before gameplay starts.
- Example input: `Escape` pressed during stage `1-2`.
- Expected output: Pause menu visible with `Resume`, `Restart Level`, and `Title`, while the timer is frozen.
- Example input: The player loses a life in stage `1-2` but still has remaining lives.
- Expected output: The death transition completes and stage `1-2` reloads from its start after showing the restart card with updated lives.
- Example input: The player reaches game over after losing the final life.
- Expected output: The game-over screen shows `Game Over` and offers restart or title navigation.
- Example input: The player clears stage `1-4`.
- Expected output: The world-clear screen ends the linear run without entering a hub or world-map layer.

## Edge Cases
- Pressing pause on the title screen shall have no gameplay effect.
- Repeated pause input while already paused shall not duplicate overlays or corrupt state.
- Selecting `Restart Level` from pause shall not decrement lives unless a separate lost-life event has already resolved.
- Selecting `Title` from pause shall not leave the abandoned run partially active behind the title screen.
- Starting a new game after game over shall reset lives, score, coins, and progression.
- The pre-stage transition card shall reflect the current world-stage identifier and current life count, not stale values from a prior stage.
- The title-screen attract animation shall loop cleanly without freezing the menu state or obscuring the menu actions.
- The logo shall remain readable over the title animation and shall not collide visually with menu actions.
- The title preview art shall remain readable at menu scale and shall not visually overpower the action buttons.
- Non-final death restart shall not route through a hub, title screen, or unrelated summary overlay before gameplay resumes.

## Non-Functional Constraints
- Transition from title screen to gameplay should feel immediate and consistent.
- Pause and resume should not visibly desynchronize timer, enemy, or animation state.

## Related Specs
- `ENGINE.PROJECT.001`
- `PLAYER.INPUT.001`
- `LEVEL.PROGRESSION.001`
- `HUD.STATUS.001`
- `SAVE.PROGRESSION.001`
