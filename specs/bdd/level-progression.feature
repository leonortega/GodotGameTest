Feature: Stage progression and scoring flow
  As a player
  I want clear SMB1-style stage completion and life rules
  So that I understand how to progress through the world

  Scenario: Ground-level goal marker clears the stage
    Given the player is alive in stage 1-1
    And the stage endpoint uses a compact ground-level goal marker
    When the player touches the goal marker
    Then stage 1-1 is marked complete
    And the stage-clear flow remains minimal and direct
    And after the clear transition stage 1-2 loads next

  Scenario: A new run starts at stage 1-1
    When the player starts a new run
    Then stage 1-1 loads as the first interactive stage of that run

  Scenario: Timer expiration causes a lost life
    Given the player is in an active stage
    And the timer has 1 second remaining
    When the timer reaches zero
    Then the player loses one life
    And the stage restarts if lives remain

  Scenario: Standard full-length stage uses the default timer budget
    Given a standard full-length stage is loaded
    When gameplay begins
    Then the visible stage timer starts at 400

  Scenario: Coin threshold awards an extra life
    Given the player has 99 coins
    When the player collects 1 coin
    Then the player gains 1 life

  Scenario: Stage flow does not enter a hub between clears
    Given the player clears stage 1-2
    When the next stage is prepared
    Then the run moves directly to stage 1-3
    And no world-map or hub screen appears between those stages

  Scenario: Final stage clear ends the world
    Given the player is alive in stage 1-4
    When the player touches the goal marker
    Then the world-clear screen is shown
