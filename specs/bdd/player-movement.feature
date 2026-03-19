Feature: Player movement and platforming
  As a player
  I want responsive movement and predictable jumps
  So that success depends on skill rather than randomness

  Scenario: Running jump travels farther than walking jump
    Given the player is standing on flat ground
    When the player performs a walking jump
    Then a baseline horizontal distance is traveled
    When the player performs a running jump from the same ground
    Then the running jump travels farther than the walking jump

  Scenario: Solid wall blocks movement
    Given the player is moving toward a solid wall
    When the player reaches the wall
    Then the player stops at the collision boundary
    And the player does not pass through the wall

  Scenario: Falling into a pit loses a life
    Given the player is above a bottomless gap
    When the player falls below the stage bounds
    Then the player loses one life

  Scenario: Player can double jump before landing
    Given the player has already jumped once
    And the player is still airborne
    When the player presses the jump input again
    Then the player performs a second jump
    And the player cannot perform a third jump before landing
