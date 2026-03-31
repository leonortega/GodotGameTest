# Spec: `PLAYER.POWERUPS.001`

## Metadata
- **Title**: Player Form Upgrades, Special Pickups, and Damage Resolution
- **Version**: `v1.4`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: High

## Purpose
Define the SMB1-style player state model for form upgrades, special pickups, ranged fire attacks, and damage resolution.

## Preconditions
- An active stage is loaded.
- The player character is alive.

## Trigger
- The player collects a power-up or receives damage.

## Requirements
- `PLAYER.POWERUPS.001-R1`: The system shall support `Small Form`, `Super Form`, and `Fire Form`.
- `PLAYER.POWERUPS.001-R1A`: Each supported player form shall have a distinct readable visual presentation using real player art assets so that form changes are recognizable without relying only on HUD text.
- `PLAYER.POWERUPS.001-R1B`: Machine-readable gameplay state shall use the canonical form identifiers `Small`, `Super`, and `Fire`.
- `PLAYER.POWERUPS.001-R2`: The player shall start a new game and a new life in `Small Form`.
- `PLAYER.POWERUPS.001-R3`: Collecting a growth power-up while in `Small Form` shall change the player to `Super Form`.
- `PLAYER.POWERUPS.001-R3A`: The authored growth power-up for the current build shall be represented as a `Mushroom`.
- `PLAYER.POWERUPS.001-R4`: Collecting an attack power-up while in `Super Form` shall change the player to `Fire Form`.
- `PLAYER.POWERUPS.001-R4A`: The authored attack power-up for the current build shall be represented as a `Fire Flower`.
- `PLAYER.POWERUPS.001-R4B`: If authored content substitution causes an attack power-up to be collected while the player is in `Small Form`, the result shall not leave the player below `Super Form`.
- `PLAYER.POWERUPS.001-R5`: Collecting a `Mushroom` while already in `Super Form` or `Fire Form` shall not downgrade the player and shall instead resolve as score according to `SCORE.ECONOMY.001`.
- `PLAYER.POWERUPS.001-R5A`: Collecting a `Fire Flower` while already in `Fire Form` shall award score according to `SCORE.ECONOMY.001`.
- `PLAYER.POWERUPS.001-R6`: When the player takes damage in `Fire Form`, the player shall downgrade to `Super Form` and gain a short damage invulnerability window.
- `PLAYER.POWERUPS.001-R7`: When the player takes damage in `Super Form`, the player shall downgrade to `Small Form` and gain a short damage invulnerability window.
- `PLAYER.POWERUPS.001-R8`: When the player takes damage in `Small Form`, the player shall lose a life.
- `PLAYER.POWERUPS.001-R9`: During the damage invulnerability window, additional damage sources shall not remove another form or life.
- `PLAYER.POWERUPS.001-R9A`: The player state model shall distinguish temporary damage invulnerability from `Star` invincibility.
- `PLAYER.POWERUPS.001-R10`: `Fire Form` shall allow the player to emit a ranged fire attack while respecting a bounded on-screen projectile limit.
- `PLAYER.POWERUPS.001-R10A`: The current MVP build shall cap active player fire projectiles at 2 simultaneous shots.
- `PLAYER.POWERUPS.001-R10B`: The authored projectile emitted by `Fire Form` shall be represented as `fire`.
- `PLAYER.POWERUPS.001-R10C`: A fire projectile shall travel forward, bounce from valid ground contact, and be destroyed by solid blocking geometry or by leaving the active gameplay area.
- `PLAYER.POWERUPS.001-R11`: A valid player projectile emitted from `Fire Form` shall resolve against enemies according to enemy vulnerability rules.
- `PLAYER.POWERUPS.001-R11A`: Eligible standard enemies shall be defeated by a valid player projectile, while non-standard enemies may resist or reflect that projectile according to `ENEMIES.CORE.001`.
- `PLAYER.POWERUPS.001-R12`: The system shall support a `Super Star` pickup that grants temporary star invincibility.
- `PLAYER.POWERUPS.001-R12A`: During star invincibility, valid enemy contact shall resolve as enemy defeat rather than normal contact damage.
- `PLAYER.POWERUPS.001-R13`: The system shall support a `1-Up Mushroom` pickup that awards exactly one additional life without changing the player's current form.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Mushroom upgrades the default player state
  Given the player is in Small Form
  When the player collects a Mushroom growth power-up
  Then the player shall enter Super Form
  And the visible player presentation shall update to the Super form art

Scenario: Fire Flower upgrades the super player
  Given the player is in Super Form
  When the player collects a Fire Flower attack power-up
  Then the player shall enter Fire Form

Scenario: Damage downgrades fire form instead of killing immediately
  Given the player is in Fire Form
  When the player collides with contact damage
  Then the player shall enter Super Form
  And a short damage invulnerability window shall begin

Scenario: Small player loses a life on damage
  Given the player is in Small Form
  When the player collides with contact damage
  Then the player shall lose one life

Scenario: Fire form can launch a ranged attack
  Given the player is in Fire Form
  When the player presses the attack control
  Then the system shall spawn a fire ranged attack projectile
  And the projectile shall resolve against enemies according to their vulnerability rules

Scenario: Active fire projectile count is bounded
  Given the player is in Fire Form
  And 2 player fire projectiles are already active
  When the player presses the attack control again
  Then no additional player fire projectile shall be spawned

Scenario: Super Star grants temporary invincibility
  Given the player is in Small Form
  When the player collects a Super Star pickup
  Then the player shall enter star invincibility for a temporary duration

Scenario: 1-Up Mushroom grants an extra life without changing form
  Given the player is in Super Form
  When the player collects a 1-Up Mushroom
  Then the player shall gain 1 life
  And the player shall remain in Super Form
```

## Example Inputs/Outputs
- Example input: Small player hits a reward block containing a `Mushroom` and collects it.
- Expected output: Player state becomes `Super Form` and the visible player art updates to the Super form presentation.
- Example input: Super player collects a `Fire Flower`.
- Expected output: Player state becomes `Fire Form`.
- Example input: Fire player takes damage from an enemy.
- Expected output: Player downgrades to `Super Form` and remains temporarily damage-invulnerable.
- Example input: Fire player launches a fire projectile along flat ground.
- Expected output: The projectile moves forward, bounces from the ground, and is removed when it hits blocking geometry or leaves active play.
- Example input: The player collects a `Super Star`.
- Expected output: The player enters temporary star invincibility and defeats valid enemies by contact during that window.
- Example input: The player collects a `1-Up Mushroom`.
- Expected output: One additional life is awarded and the current form is unchanged.

## Edge Cases
- Damage received during the damage invulnerability window shall have no gameplay effect.
- The `Mushroom`, `Fire Flower`, `Super Star`, and `1-Up Mushroom` item presentations shall remain visually distinguishable at gameplay scale.
- Projectile limit reached shall prevent additional projectile spawn until an active projectile leaves play.
- Projectile resolution shall remain consistent whether the projectile defeats, is blocked by, or is reflected from an enemy.
- The `fire` attack asset shall remain readable against bright backgrounds and enemy sprites.
- Form sprites shall remain readable at gameplay scale and shall not make `Super` and `Fire` visually ambiguous.
- Star invincibility shall expire cleanly without leaving the player permanently immune to damage.

## Non-Functional Constraints
- Form transitions should be visually readable and not interrupt control longer than necessary.
- Damage and pickup resolution must be deterministic to avoid double-hit frustration or ambiguous form state.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `LEVEL.ELEMENTS.001`
- `SCORE.ECONOMY.001`
- `ENEMIES.CORE.001`
- `HUD.STATUS.001`
