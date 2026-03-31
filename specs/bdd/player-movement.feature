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

  Scenario: High-speed reversal produces a readable skid
    Given the player is running to the right on flat ground
    When the player presses and holds left
    Then the player enters a brief skid or turnaround state before accelerating left

  Scenario: Falling into a pit loses a life
    Given the player is above a bottomless gap
    When the player falls below the stage bounds
    Then the player loses one life

  Scenario: One airborne follow-up jump is allowed
    Given the player has already jumped once
    And the player is still airborne
    When the player presses the jump input again
    Then the player performs one airborne follow-up jump

  Scenario: A third jump is not allowed before landing
    Given the player has already jumped once
    And the player has already performed the airborne follow-up jump
    And the player is still airborne
    When the player presses the jump input again
    Then the player does not perform a third jump before landing

  Scenario: Late ledge input does not use a grace window
    Given the player runs off the edge of a platform
    When the jump input is pressed immediately after leaving the ledge
    Then the system is not required to resolve a grounded-style jump

  Scenario: Pre-landing jump input does not require buffering
    Given the player is descending toward safe ground
    When the player presses jump shortly before touching down
    Then the system is not required to trigger a jump on the first grounded frame

  Scenario: Early jump release creates a shorter hop
    Given the player starts a normal jump from flat ground
    When the player releases the jump input before the ascent completes
    Then the jump apex is lower than a held jump from the same starting state

  Scenario: Air control remains weaker than grounded control
    Given the player jumps while moving right
    When the player reverses direction in midair
    Then the horizontal response remains weaker than the same reversal on the ground

  Scenario: Movement state changes remain visually readable
    Given the player moves through idle, running, jumping, and falling states
    When the movement state changes
    Then the visible player sprite or pose changes to match the current traversal state

  Scenario: Grounded down input shows a duck pose
    Given the player is standing on stable ground
    When the player holds the down input without meaningful horizontal movement
    Then the visible player sprite or pose changes to a duck or crouch presentation
