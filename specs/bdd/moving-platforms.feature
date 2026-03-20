Feature: Floating moving platforms
  As a player
  I want moving airborne platforms to carry me across gaps
  So that traversal can include timed platform rides

  Scenario: Floating platform moves left and right in the air
    Given a floating moving platform is active in a stage
    And there is no solid ground directly beneath its traversable surface
    When the stage simulation advances
    Then the platform moves left and right along its authored path

  Scenario: Floating platform stays unsupported across its full sweep
    Given a floating moving platform has authored left and right patrol extents
    When the full patrol envelope is reviewed
    Then there is no grounded support directly beneath the midpoint, left extent, or right extent

  Scenario: Player is carried by the moving platform
    Given the player is standing on a floating moving platform
    When the platform moves horizontally
    Then the player is carried with it
    And the player can still jump normally

  Scenario: Platform keeps moving after the player jumps away
    Given the player is riding a floating moving platform
    When the player jumps away from it
    Then the platform continues its horizontal patrol