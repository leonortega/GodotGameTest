Feature: Stage progression and scoring flow
  As a player
  I want clear stage completion and life rules
  So that I understand how to progress through the world

  Scenario: Ground-level goal marker clears the stage
    Given the player is alive in stage 1-1
    And the stage endpoint uses a compact ground-level goal marker
    When the player touches the goal marker
    Then stage 1-1 is marked complete
    And a centered stage summary is shown
    And after approximately 3 seconds stage 1-2 loads next

  Scenario: A title-selected stage starts the run
    Given the title screen starting level is set to stage 1-3
    When the player starts a new run
    Then stage 1-3 loads as the first interactive stage of that run

  Scenario: Timer expiration causes a lost life
    Given the player is in an active stage
    And the timer has 1 second remaining
    When the timer reaches zero
    Then the player loses one life
    And the stage restarts if lives remain

  Scenario: Coin threshold awards an extra life
    Given the player has 99 coins
    When the player collects 1 coin
    Then the player gains 1 life

  Scenario: Final stage clear ends the world
    Given the player is alive in stage 1-4
    When the player touches the goal marker
    Then the world-clear screen is shown
