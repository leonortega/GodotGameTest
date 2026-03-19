Feature: Player power-up progression and damage handling
  As a player
  I want power-ups to change my capabilities and survivability
  So that upgrades feel meaningful and damage is readable

  Scenario: Growth power-up upgrades the player
    Given the player is in Small Form
    When the player collects a growth power-up
    Then the player enters Powered Form

  Scenario: Powered player shrinks after damage
    Given the player is in Powered Form
    When the player takes contact damage
    Then the player enters Small Form
    And a short invulnerability window begins

  Scenario: Enhanced player can attack at range
    Given the player is in Enhanced Form
    When the player presses the attack control
    Then a ranged attack projectile is spawned
    And the projectile can defeat eligible enemies

  Scenario: Small player loses a life on damage
    Given the player is in Small Form
    When the player takes contact damage
    Then the player loses one life
