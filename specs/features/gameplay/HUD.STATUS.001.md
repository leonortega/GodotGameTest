# Spec: `HUD.STATUS.001`

## Metadata
- **Title**: In-Game HUD for Score, Coins, Lives, World, and Timer
- **Version**: `v1.3`
- **Status**: Approved
- **Context/View**: Heads-Up Display
- **Priority**: Medium

## Purpose
Define the minimum HUD information required for readable play-state awareness.

## Preconditions
- A stage or shell state is active.

## Trigger
- Stage render or any state change affecting score, coins, lives, world, or timer.

## Requirements
- `HUD.STATUS.001-R1`: During active gameplay, the HUD shall display score, coins, world or stage identifier, remaining lives, remaining time, and current difficulty.
- `HUD.STATUS.001-R2`: HUD values shall update when the underlying gameplay state changes.
- `HUD.STATUS.001-R3`: The active stage identifier shall be shown using the world-stage format, such as `1-2`.
- `HUD.STATUS.001-R4`: The HUD shall remain readable against all MVP stage backgrounds.
- `HUD.STATUS.001-R4A`: The HUD shall use a text-first presentation with consistent label and value alignment rather than depending on per-field sprite icons.
- `HUD.STATUS.001-R4B`: The top-of-screen stats area shall include a dark or black backing treatment sufficient to separate the HUD from the stage background.
- `HUD.STATUS.001-R4C`: The HUD labels and values shall use retro-styled typography consistent with the title and overlay presentation.
- `HUD.STATUS.001-R5`: When the game is paused, the HUD shall remain visible unless a pause overlay intentionally replaces the same information.
- `HUD.STATUS.001-R6`: When a stage clear begins, the HUD shall stop decrementing the visible timer and may present score tally behavior.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Coin pickup updates the HUD
  Given the player is in an active stage with 12 coins
  When the player collects 1 coin
  Then the HUD shall display 13 coins

Scenario: HUD displays the active stage identifier
  Given stage 1-3 is loaded
  When gameplay begins
  Then the HUD shall display 1-3 as the active stage

Scenario: HUD remains visually aligned across text fields
  Given the HUD renders score, coin, stage, life, time, and difficulty fields
  When the stats bar is shown during gameplay
  Then the visible label row and value row shall remain consistently aligned

Scenario: HUD remains readable over bright backgrounds
  Given the stage background is visually bright
  When the HUD is rendered at the top of the screen
  Then the stats area shall remain readable using a dark backing panel or equivalent contrast treatment

Scenario: Timer display freezes after pause
  Given the player is in an active stage
  When the player pauses the game
  Then the visible timer shall stop decreasing
```

## Example Inputs/Outputs
- Example input: Active stage `1-2`, score `004500`, coins `37`, lives `2`, time `251`, difficulty `Hard`.
- Expected output: HUD renders a readable text-first in-game overlay with all values inside a dark top panel.

## Edge Cases
- HUD values shall not show negative time or lives.
- Large score values shall remain legible without overlapping other HUD fields.
- The HUD backing shall not be so transparent that top-row readability is lost against sky or cloud backgrounds.
- Stage clear bonus counting shall not resume normal timer countdown.
- Difficulty labels shall not collide with adjacent stat fields when the player changes setting.

## Non-Functional Constraints
- HUD updates should not flicker during rapid score or coin changes.
- HUD text should remain readable on retro-resolution scaling targets.

## Related Specs
- `APP.SHELL.001`
- `LEVEL.PROGRESSION.001`
- `PLAYER.POWERUPS.001`
- `CAMERA.FOLLOW.001`
