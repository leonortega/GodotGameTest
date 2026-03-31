Feature: Player power-up progression and damage handling
  As a player
  I want power-ups to change my capabilities and survivability
  So that upgrades feel meaningful and damage is readable

  Scenario: Growth power-up upgrades the player
    Given the player is in Small Form
    When the player collects a mushroom growth power-up
    Then the player enters Super Form
    And the visible player presentation updates to the Super form art

  Scenario: Flower power-up upgrades the super player
    Given the player is in Super Form
    When the player collects a flower attack power-up
    Then the player enters Fire Form

  Scenario: Fire player downgrades after damage
    Given the player is in Fire Form
    When the player takes contact damage
    Then the player enters Super Form
    And a short invulnerability window begins

  Scenario: Fire player can attack at range
    Given the player is in Fire Form
    When the player presses the attack control
    Then a fire ranged attack projectile is spawned
    And the projectile resolves against enemies according to their vulnerability rules

  Scenario: Projectile count is capped
    Given the player is in Fire Form
    And 2 player projectiles are already active
    When the player presses the attack control
    Then no additional player projectile is spawned

  Scenario: Super Star grants temporary invincibility
    Given the player is in Small Form
    When the player collects a Super Star pickup
    Then the player enters temporary star invincibility

  Scenario: 1-Up Mushroom grants an extra life
    Given the player is in Super Form
    When the player collects a 1-Up Mushroom
    Then the player gains one life
    And the player remains in Super Form

  Scenario: Small player loses a life on damage
    Given the player is in Small Form
    When the player takes contact damage
    Then the player loses one life
