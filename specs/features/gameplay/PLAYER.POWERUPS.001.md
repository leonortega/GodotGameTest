# Spec: `PLAYER.POWERUPS.001`

## Metadata
- **Title**: Player Form Upgrades and Damage Resolution
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: High

## Purpose
Define how the player gains power-ups, changes form, and resolves damage.

## Preconditions
- An active stage is loaded.
- The player character is alive.

## Trigger
- The player collects a power-up or receives damage.

## Requirements
- `PLAYER.POWERUPS.001-R1`: The system shall support `Small Form`, `Powered Form`, and `Enhanced Form`.
- `PLAYER.POWERUPS.001-R2`: The player shall start a new game and a new life in `Small Form`.
- `PLAYER.POWERUPS.001-R3`: Collecting a growth power-up while in `Small Form` shall change the player to `Powered Form`.
- `PLAYER.POWERUPS.001-R4`: Collecting an attack power-up while in `Powered Form` shall change the player to `Enhanced Form`.
- `PLAYER.POWERUPS.001-R5`: Collecting a growth power-up while already in `Powered Form` or `Enhanced Form` shall not downgrade the player.
- `PLAYER.POWERUPS.001-R6`: When the player takes damage in `Enhanced Form`, the player shall downgrade to `Powered Form` and gain a short invulnerability window.
- `PLAYER.POWERUPS.001-R7`: When the player takes damage in `Powered Form`, the player shall downgrade to `Small Form` and gain a short invulnerability window.
- `PLAYER.POWERUPS.001-R8`: When the player takes damage in `Small Form`, the player shall lose a life.
- `PLAYER.POWERUPS.001-R9`: During the invulnerability window, additional damage sources shall not remove another form or life.
- `PLAYER.POWERUPS.001-R10`: `Enhanced Form` shall allow the player to emit a ranged attack while respecting a bounded on-screen projectile limit.
- `PLAYER.POWERUPS.001-R11`: A valid player projectile emitted from `Enhanced Form` shall be able to defeat eligible enemies.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Growth power-up upgrades the default player state
  Given the player is in Small Form
  When the player collects a growth power-up
  Then the player shall enter Powered Form

Scenario: Damage downgrades instead of killing a powered player
  Given the player is in Powered Form
  When the player collides with contact damage
  Then the player shall enter Small Form
  And a short invulnerability window shall begin

Scenario: Small player loses a life on damage
  Given the player is in Small Form
  When the player collides with contact damage
  Then the player shall lose one life

Scenario: Enhanced form can launch a ranged attack
  Given the player is in Enhanced Form
  When the player presses the attack control
  Then the system shall spawn a ranged attack projectile
  And the projectile shall be able to defeat eligible enemies
```

## Example Inputs/Outputs
- Example input: Small player hits a mystery block containing a growth power-up and collects it.
- Expected output: Player state becomes `Powered Form`.
- Example input: Enhanced player takes damage from a flying enemy.
- Expected output: Player downgrades to `Powered Form` and remains temporarily invulnerable.
- Example input: Enhanced player fires a projectile at a standard enemy.
- Expected output: The enemy is defeated if it is vulnerable to projectiles.

## Edge Cases
- Damage received during the invulnerability window shall have no gameplay effect.
- Collecting an attack power-up in `Small Form` shall first result in a non-smaller upgraded state as defined by game balance.
- Projectile limit reached shall prevent additional projectile spawn until an active projectile leaves play.

## Non-Functional Constraints
- Form transitions should be visually readable and not interrupt control longer than necessary.
- Damage resolution must be deterministic to avoid double-hit frustration.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `ENEMIES.CORE.001`
- `HUD.STATUS.001`
