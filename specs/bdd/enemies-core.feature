Feature: Core enemy interactions
  As a player
  I want enemies to follow readable rules
  So that I can learn how to avoid or defeat them

  Scenario: Ground enemy reverses at a wall
    Given a ground enemy is moving toward a solid wall
    When the enemy reaches the wall
    Then the enemy reverses direction

  Scenario: Ground enemy reverses before a gap
    Given a ground enemy is moving toward a gap
    When the enemy reaches the edge detection point
    Then the enemy reverses direction
    And the enemy does not fall into the gap

  Scenario: Standard enemy is defeated by a stomp
    Given the player is above a stompable enemy
    When the player lands on the enemy from above
    Then the enemy is defeated
    And the player bounces upward
    And the enemy plays an inverted falling death animation

  Scenario: Side contact damages the player
    Given the player touches an enemy from the side
    When the player is not invulnerable
    Then player damage resolution occurs

  Scenario: Armored enemy resists a standard stomp
    Given the player is not using a special attack
    When the player lands on an armored enemy from above
    Then the armored enemy remains active

  Scenario: Protected-head enemy resists a stomp
    Given the player lands on the protected top of a protected-head enemy
    When the collision resolves
    Then the enemy remains active

  Scenario: Shooter enemy emits a hostile projectile
    Given a shooter enemy is active
    When its firing condition is met
    Then the enemy emits a hostile projectile

  Scenario: Player projectile defeats a standard enemy
    Given the player fires a valid projectile
    When the projectile hits a standard enemy
    Then the enemy is defeated

  Scenario: Armored enemy reflects a player projectile
    Given the player fires a valid projectile
    When the projectile hits an armored enemy
    Then the armored enemy remains active
    And the projectile returns as a hostile threat

  Scenario: Enemy classes stay visually distinct
    Given Ground, Armored, Flying, ProtectedHead, and Shooter enemies appear in gameplay
    When the player encounters them at normal play scale
    Then each enemy class remains visually distinct from the others
