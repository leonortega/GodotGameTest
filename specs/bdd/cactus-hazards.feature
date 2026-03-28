Feature: Cactus environmental hazards
  As a player
  I want grounded hazard obstacles to be readable and dangerous
  So that stage traversal includes non-enemy threats

  Scenario: Cactus hazard damages the player on side contact
    Given a cactus hazard is present on grounded terrain
    When the player touches the cactus from the side
    Then player damage resolution occurs

  Scenario: Cactus hazard is not defeated by a stomp-like landing
    Given a cactus hazard is present on grounded terrain
    When the player lands on the cactus from above
    Then the cactus hazard remains active
    And player damage or non-safe contact resolution occurs

  Scenario: Cactus hazard stays fixed in place
    Given a cactus hazard is present in a stage
    When stage simulation advances
    Then the cactus hazard remains stationary
