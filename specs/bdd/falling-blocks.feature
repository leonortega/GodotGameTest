Feature: Falling blocks
  As a player
  I want unstable suspended blocks to collapse after a short delay
  So that platforming hazards stay readable but threatening

  Scenario: Single falling block stays suspended before the delay ends
    Given the player is standing on a falling block
    When the player remains in contact for less than 0.5 seconds
    Then the block remains suspended

  Scenario: Falling block line uses real gaps between blocks
    Given a falling block line is authored in a stage
    When its layout is reviewed
    Then each falling block is separated from the next by empty space
    And the gap is approximately 1.5 terrain blocks wide

  Scenario: Falling block line keeps a clear traversal corridor
    Given a falling block line has an authored footprint
    When the authored corridor is reviewed
    Then the line has empty buffer space before and after its footprint
    And each block footprint remains clear of surrounding map geometry beneath it

  Scenario: Falling block gaps remain non-walkable
    Given the player is traversing a falling block line
    When the player steps into the empty space between two blocks
    Then the player falls through the gap

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

  Scenario: Falling blocks are not valid enemy support
    Given a falling block line is authored for gameplay use
    When grounded enemies are placed in the stage
    Then those enemies are not placed on the falling blocks