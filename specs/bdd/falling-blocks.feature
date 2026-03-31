Feature: Falling blocks
  As a player
  I want unstable suspended blocks to collapse after a short delay
  So that platforming hazards stay readable but threatening

  Scenario: Single falling block stays suspended before the delay ends
    Given the player is standing on a falling block
    When the player remains in contact for less than 0.5 seconds
    Then the block remains suspended

  Scenario: Contacted falling block collapses after sustained contact
    Given the player is standing on a falling block
    When the player remains in contact for approximately 0.5 seconds
    Then that falling block begins falling downward
    And neighboring falling blocks remain suspended until they are triggered separately

  Scenario: Leaving early resets the trigger timer
    Given the player touches a falling block
    When the player leaves the block before 0.5 seconds elapse
    Then the collapse timer resets
    And the block remains suspended until touched again long enough

