Feature: Godot input map and shared control actions
  As a player
  I want consistent controls across devices
  So that the game feels the same regardless of input method

  Scenario: Keyboard triggers named jump action
    Given the jump key is mapped in Godot Input Map
    When the player presses the jump key
    Then gameplay responds to the `jump` action

  Scenario: Gamepad uses the same action names
    Given a gamepad is connected
    When the player presses the mapped action button
    Then gameplay responds to the same named action used by keyboard input

  Scenario: Pause is available during active gameplay
    Given the player is in an active stage
    When the player presses the mapped pause control
    Then the game enters pause state
