# Spec: `PLAYER.INPUT.001`

## Metadata
- **Title**: Godot Input Map for Keyboard-First Gameplay and Menus
- **Version**: `v1.2`
- **Status**: Approved
- **Context/View**: Input
- **Priority**: High

## Purpose
Define the stable Godot Input Map used by the current keyboard-first build without rewriting gameplay logic around raw key checks.

## Preconditions
- The project input actions are being configured in Godot.

## Trigger
- Input setup, menu navigation, or gameplay control handling.

## Requirements
- `PLAYER.INPUT.001-R1`: The project shall define named input actions for at least `move_left`, `move_right`, `move_down`, `jump`, `action`, and `pause`.
- `PLAYER.INPUT.001-R2`: Gameplay logic shall read named actions from Godot Input Map instead of binding to physical keys directly.
- `PLAYER.INPUT.001-R3`: The default desktop bindings shall support keyboard play using `A` or Left Arrow for `move_left`, `D` or Right Arrow for `move_right`, and `S` or Down Arrow for `move_down`.
- `PLAYER.INPUT.001-R4`: The default jump binding shall support `W`, Up Arrow, `Space`, and the secondary keyboard fallback key used by the current build.
- `PLAYER.INPUT.001-R5`: The default action binding shall support `Shift` and the secondary keyboard fallback key used by the current build.
- `PLAYER.INPUT.001-R6`: The default pause binding shall support `Escape` as the primary desktop pause key and a keyboard fallback pause key.
- `PLAYER.INPUT.001-R7`: The `pause` action shall be available from active gameplay and ignored during non-gameplay states when pause behavior is not meaningful.
- `PLAYER.INPUT.001-R8`: Menu interactions shall remain consistent with the same keyboard-focused input abstraction used by gameplay.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Gameplay reads named actions
  Given the player presses a configured keyboard key for jump
  When gameplay input is evaluated
  Then the player controller shall respond to the `jump` action

Scenario: Keyboard fallback keys resolve to the same action
  Given `Shift` and the fallback attack key are both mapped to the `action` input
  When the player presses either key during valid gameplay
  Then gameplay shall trigger the same `action` behavior

Scenario: Pause uses a dedicated action
  Given the player is in an active stage
  When the player presses the mapped pause control
  Then the game shall enter pause state

Scenario: Escape resolves to the pause action
  Given Escape is mapped to the pause input
  When the player presses Escape during active gameplay
  Then gameplay shall resolve the named `pause` action
```

## Example Inputs/Outputs
- Example input: `Space`, `W`, Up Arrow, and `K` mapped to `jump`, with `Shift` and `J` mapped to `action`, and `Escape` plus `P` mapped to `pause`.
- Expected output: Player movement and attack code read named actions, not raw key codes.

## Edge Cases
- Multiple physical bindings for one action shall still resolve to one gameplay intent.
- Keyboard fallback bindings shall remain usable if the player prefers letter keys over arrows.

## Non-Functional Constraints
- Input definitions should remain inspectable in Godot Project Settings.
- Action naming should stay stable enough to support future remapping work.

## Related Specs
- `APP.SHELL.001`
- `ENGINE.PROJECT.001`
- `PLAYER.MOVEMENT.001`
- `PLAYER.POWERUPS.001`
