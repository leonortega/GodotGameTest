# Spec: `ENEMIES.CORE.001`

## Metadata
- **Title**: Enemy Behaviors and Player Interaction Rules
- **Version**: `v1.1`
- **Version**: `v1.2`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: High

## Purpose
Define baseline enemy classes and the rules for defeating or being damaged by them.

## Preconditions
- An active stage is loaded.
- At least one enemy entity is present.

## Trigger
- An enemy updates, collides with the world, or collides with the player or a projectile.

## Requirements
- `ENEMIES.CORE.001-R1`: The system shall support at least `Ground`, `Armored`, `Flying`, `ProtectedHead`, and `Shooter` enemy classes in the MVP or immediate post-MVP baseline.
- `ENEMIES.CORE.001-R2`: Ground enemies shall patrol horizontally and reverse direction when blocked by level geometry.
- `ENEMIES.CORE.001-R2A`: Ground enemies shall detect a missing floor tile or gap ahead and reverse direction before walking into the gap.
- `ENEMIES.CORE.001-R3`: Flying enemies shall follow a readable repeating movement pattern.
- `ENEMIES.CORE.001-R4`: Landing on a stompable enemy from above shall defeat that enemy and rebound the player upward.
- `ENEMIES.CORE.001-R4A`: A stomp-defeated enemy shall play a death animation in which the enemy is inverted and falls downward until leaving the visible play space.
- `ENEMIES.CORE.001-R5`: Contact with an enemy from the side or below shall cause player damage unless the player is currently invulnerable.
- `ENEMIES.CORE.001-R6`: Armored enemies shall not be defeated by a standard stomp while the player lacks a special attack state.
- `ENEMIES.CORE.001-R6A`: Protected-head enemies shall reject a stomp when the player contacts the protected top surface, and the collision shall resolve as a non-stomp threat or rebound according to combat rules.
- `ENEMIES.CORE.001-R7`: An enemy hit by a valid ranged attack shall resolve according to its vulnerability rules.
- `ENEMIES.CORE.001-R7A`: A standard enemy hit by a valid player projectile shall be defeated.
- `ENEMIES.CORE.001-R7B`: An armored enemy hit by a valid player projectile shall remain active and reflect that projectile back as a hostile threat.
- `ENEMIES.CORE.001-R7C`: Shooter enemies shall emit readable hostile projectiles according to a controlled firing cadence or trigger condition.
- `ENEMIES.CORE.001-R7D`: Hostile enemy projectiles shall damage the player on valid contact unless blocked by an invulnerability state or other explicit rule.
- `ENEMIES.CORE.001-R8`: Defeated enemies shall award score.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Ground enemies reverse at walls
  Given a ground enemy is walking toward a solid wall
  When the enemy reaches the wall
  Then the enemy shall reverse direction

Scenario: Ground enemies reverse at gaps
  Given a ground enemy is walking toward a gap
  When the enemy reaches the edge detection point before the gap
  Then the enemy shall reverse direction
  And the enemy shall not fall into the gap

Scenario: Stomping a standard enemy defeats it
  Given the player is above a stompable ground enemy
  When the player lands on the enemy from above
  Then the enemy shall be defeated
  And the player shall bounce upward
  And the enemy shall play an inverted falling death animation

Scenario: Side contact damages the player
  Given the player touches an enemy from the side
  When the player is not invulnerable
  Then player damage resolution shall occur

Scenario: Armored enemy resists a standard stomp
  Given the player is not using a special attack state
  When the player lands on an armored enemy from above
  Then the armored enemy shall remain active

Scenario: Protected-head enemy rejects a stomp
  Given the player lands on the protected top of a protected-head enemy
  When the collision resolves
  Then the enemy shall not be crushed by the stomp

Scenario: Shooter enemy fires a hostile projectile
  Given a shooter enemy is active in the stage
  When its fire condition is met
  Then the enemy shall emit a hostile projectile
  And that projectile shall threaten the player

Scenario: Player projectile defeats a standard enemy
  Given the player emits a valid ranged attack
  And the projectile collides with a standard enemy
  When the collision resolves
  Then the enemy shall be defeated
  And score shall be awarded

Scenario: Armored enemy reflects a player projectile
  Given the player emits a valid ranged attack
  And the projectile collides with an armored enemy
  When the collision resolves
  Then the armored enemy shall remain active
  And the projectile shall return as a hostile threat
```

## Example Inputs/Outputs
- Example input: Powered player stomps a basic ground enemy.
- Expected output: Enemy enters an inverted falling death animation, score increases, player rebounds.
- Example input: Small player side-collides with a flying enemy.
- Expected output: Player loses a life.
- Example input: Player lands on a helmeted enemy's head.
- Expected output: The enemy remains active because the top surface is protected.
- Example input: Enhanced player fires at an armored enemy.
- Expected output: The armored enemy remains active and the projectile is reflected back.

## Edge Cases
- Simultaneous stomp and hazard overlap shall prioritize the first resolved collision according to engine tick order.
- An enemy already marked defeated shall not deal damage.
- Off-screen enemies may be culled for performance, but they shall not produce visible respawn artifacts when re-entering active play.
- Gap detection shall not cause visible oscillation at flat ledge transitions.
- Shooter projectile cadence shall not become unreadable spam when multiple shooter enemies share the screen.

## Non-Functional Constraints
- Enemy patterns should remain readable enough for skill-based play.
- Collision results should be consistent across repeated attempts.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `PLAYER.POWERUPS.001`
- `DIFFICULTY.BALANCE.001`
- `LEVEL.PROGRESSION.001`
