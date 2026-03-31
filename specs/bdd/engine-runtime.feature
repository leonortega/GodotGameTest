Feature: Runtime feel constraints
  As a developer
  I want deterministic runtime rules
  So that movement, timing, and activation feel consistent

  Scenario: Fixed-step simulation keeps timer stable
    Given the game renders at varying visual frame rates
    When the stage timer runs for 1 real second
    Then gameplay timer progression remains consistent with the fixed simulation rate

  Scenario: Off-screen actors outside activation window do not attack
    Given a hostile actor is outside the authored activation window
    When runtime simulation advances
    Then that actor does not damage or attack the player from unseen space

  Scenario: Input is sampled on fixed simulation steps
    Given the player presses jump during active play
    When the next fixed simulation step resolves
    Then the jump input is available to gameplay logic on that step

  Scenario: Runtime caps preserve critical gameplay actors
    Given the runtime active-object cap is reached
    When a distant non-critical actor and a visible critical actor compete for activation
    Then the visible critical actor remains active
