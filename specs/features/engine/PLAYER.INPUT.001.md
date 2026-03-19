# Spec: `PLAYER.INPUT.001`

## Metadata
- **Title**: Godot Input Map for Gameplay, Menus, and Touch Fallback
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Input
- **Priority**: High

## Purpose
Define a stable Godot Input Map that supports keyboard, gamepad, and optional touch controls without rewriting gameplay logic per device.

## Preconditions
- The project input actions are being configured in Godot.

## Trigger
- Input setup, menu navigation, or gameplay control handling.

## Requirements
- `PLAYER.INPUT.001-R1`: The project shall define named input actions for at least `move_left`, `move_right`, `move_down`, `jump`, `action`, and `pause`.
- `PLAYER.INPUT.001-R2`: Gameplay logic shall read named actions from Godot Input Map instead of binding to physical keys directly.
- `PLAYER.INPUT.001-R3`: The default desktop bindings shall support keyboard play using arrow keys and `A`/`D`, plus `Space` or equivalent for jump.
- `PLAYER.INPUT.001-R4`: The default gameplay bindings shall support gamepad equivalents for movement, jump, action, and pause.
- `PLAYER.INPUT.001-R5`: If the game is exported to a touch-first platform, the project shall provide on-screen controls mapped to the same named actions.
- `PLAYER.INPUT.001-R6`: The `pause` action shall be available from active gameplay and ignored during non-gameplay states when pause behavior is not meaningful.
- `PLAYER.INPUT.001-R7`: Menu interactions shall be controllable with the same input abstraction layer used by gameplay-capable devices.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Gameplay reads named actions
  Given the player presses a configured keyboard key for jump
  When gameplay input is evaluated
  Then the player controller shall respond to the `jump` action

Scenario: Gamepad uses the same action model
  Given a gamepad is connected
  When the player presses the mapped jump button
  Then gameplay shall trigger the same `jump` action used by keyboard input

Scenario: Pause uses a dedicated action
  Given the player is in an active stage
  When the player presses the mapped pause control
  Then the game shall enter pause state
```

## Example Inputs/Outputs
- Example input: `Space` mapped to `jump`, `Left Shift` mapped to `action`.
- Expected output: Player movement code reads actions, not raw key codes.

## Edge Cases
- Multiple physical bindings for one action shall still resolve to one gameplay intent.
- Disconnecting a gamepad shall not break keyboard fallback input.
- Touch overlays shall not create device-specific gameplay rules.

## Non-Functional Constraints
- Input definitions should remain inspectable in Godot Project Settings.
- Action naming should stay stable enough to support future remapping work.

## Related Specs
- `APP.SHELL.001`
- `ENGINE.PROJECT.001`
- `PLAYER.MOVEMENT.001`
- `PLAYER.POWERUPS.001`
