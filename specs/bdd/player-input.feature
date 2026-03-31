Feature: Godot input map and shared control actions
  As a player
  I want consistent keyboard-first controls
  So that the game feels predictable across gameplay and menus

  Scenario: Keyboard triggers named jump action
    Given the jump key is mapped in Godot Input Map
    When the player presses the jump key
    Then gameplay responds to the `jump` action

  Scenario: Keyboard fallback keys map to the same action
    Given Shift and J are both mapped to the action input
    When the player presses either key during valid gameplay
    Then gameplay responds to the same named action

  Scenario: Pause is available during active gameplay
    Given the player is in an active stage
    When the player presses the mapped pause control
    Then the game enters pause state

  Scenario: Escape triggers the pause action
    Given Escape is mapped to the pause input
    When the player presses Escape during active gameplay
    Then gameplay responds to the same named pause action
