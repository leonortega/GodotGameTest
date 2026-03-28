Feature: Camera2D follow behavior
  As a player
  I want a readable camera
  So that I can react to hazards without losing orientation

  Scenario: Camera follows horizontal progress
    Given the player moves to the right
    When the camera updates
    Then the view follows the player's horizontal progress

  Scenario: Camera respects level bounds
    Given the player is near a stage edge
    When the camera updates
    Then the view does not expose outside-of-level space

  Scenario: Small jumps do not create distracting vertical shake
    Given the player performs repeated short jumps on flat terrain
    When the camera updates
    Then the view remains readable without excessive vertical jitter

  Scenario: Heavy impact feedback shakes briefly without obscuring play
    Given the player lands from a heavy fall or rebounds from a stomp
    When the camera applies impact feedback
    Then any shake is brief
    And the gameplay view remains readable
