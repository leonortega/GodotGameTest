# Spec: `PLAYER.POWERUPS.001`

## Metadata
- **Title**: Player Form Upgrades and Damage Resolution
- **Version**: `v1.3`
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
- `PLAYER.POWERUPS.001-R1A`: Each supported player form shall have a distinct readable visual presentation using real player art assets so that form changes are recognizable without relying only on HUD text.
- `PLAYER.POWERUPS.001-R2`: The player shall start a new game and a new life in `Small Form`.
- `PLAYER.POWERUPS.001-R3`: Collecting a growth power-up while in `Small Form` shall change the player to `Powered Form`.
- `PLAYER.POWERUPS.001-R3A`: The authored growth power-up for the current build shall be represented as a `mushroom`.
- `PLAYER.POWERUPS.001-R4`: Collecting an attack power-up while in `Powered Form` shall change the player to `Enhanced Form`.
- `PLAYER.POWERUPS.001-R4A`: The authored attack power-up for the current build shall be represented as a `flower`.
- `PLAYER.POWERUPS.001-R5`: Collecting a growth power-up while already in `Powered Form` or `Enhanced Form` shall not downgrade the player.
- `PLAYER.POWERUPS.001-R6`: When the player takes damage in `Enhanced Form`, the player shall downgrade to `Powered Form` and gain a short invulnerability window.
- `PLAYER.POWERUPS.001-R7`: When the player takes damage in `Powered Form`, the player shall downgrade to `Small Form` and gain a short invulnerability window.
- `PLAYER.POWERUPS.001-R8`: When the player takes damage in `Small Form`, the player shall lose a life.
- `PLAYER.POWERUPS.001-R9`: During the invulnerability window, additional damage sources shall not remove another form or life.
- `PLAYER.POWERUPS.001-R10`: `Enhanced Form` shall allow the player to emit a ranged attack while respecting a bounded on-screen projectile limit.
- `PLAYER.POWERUPS.001-R10A`: The current MVP build shall cap active player projectiles at 2 simultaneous shots.
- `PLAYER.POWERUPS.001-R10B`: The authored projectile or attack asset emitted by `Enhanced Form` shall be represented as `fire`.
- `PLAYER.POWERUPS.001-R11`: A valid player projectile emitted from `Enhanced Form` shall resolve against enemies according to enemy vulnerability rules.
- `PLAYER.POWERUPS.001-R11A`: Eligible standard enemies shall be defeated by a valid player projectile, while non-standard enemies may resist or reflect that projectile according to `ENEMIES.CORE.001`.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Growth power-up upgrades the default player state
  Given the player is in Small Form
  When the player collects a mushroom growth power-up
  Then the player shall enter Powered Form
  And the visible player presentation shall update to the Powered form art

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
  Then the system shall spawn a fire ranged attack projectile
  And the projectile shall resolve against enemies according to their vulnerability rules

Scenario: Active projectile count is bounded
  Given the player is in Enhanced Form
  And 2 player projectiles are already active
  When the player presses the attack control again
  Then no additional player projectile shall be spawned
```

## Example Inputs/Outputs
- Example input: Small player hits a mystery block containing a mushroom growth power-up and collects it.
- Expected output: Player state becomes `Powered Form` and the visible player art updates to the Powered form presentation.
- Example input: Powered player collects a flower attack power-up.
- Expected output: Player state becomes `Enhanced Form`.
- Example input: Enhanced player takes damage from a flying enemy.
- Expected output: Player downgrades to `Powered Form` and remains temporarily invulnerable.
- Example input: Enhanced player fires a fire projectile at a standard enemy.
- Expected output: The enemy is defeated if it is vulnerable to projectiles.
- Example input: Enhanced player fires a fire projectile at an armored enemy.
- Expected output: The projectile resolves using the enemy's special vulnerability rule and may be reflected rather than defeating the enemy.

## Edge Cases
- Damage received during the invulnerability window shall have no gameplay effect.
- Collecting an attack power-up in `Small Form` shall first result in a non-smaller upgraded state as defined by game balance.
- The `mushroom` and `flower` item presentations shall remain visually distinguishable at gameplay scale.
- Projectile limit reached shall prevent additional projectile spawn until an active projectile leaves play.
- Projectile resolution shall remain consistent whether the projectile defeats, is blocked by, or is reflected from an enemy.
- The `fire` attack asset shall remain readable against bright backgrounds and enemy sprites.
- Form sprites shall remain readable at gameplay scale and shall not make `Powered` and `Enhanced` visually ambiguous.

## Non-Functional Constraints
- Form transitions should be visually readable and not interrupt control longer than necessary.
- Damage resolution must be deterministic to avoid double-hit frustration.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `ENEMIES.CORE.001`
- `HUD.STATUS.001`
