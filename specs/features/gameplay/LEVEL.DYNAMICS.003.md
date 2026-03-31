# Spec: `LEVEL.DYNAMICS.003`

## Metadata
- **Title**: Cactus Environmental Hazards
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: Medium

## Purpose
Define cactus hazards as grounded environmental obstacles that damage the player on contact without behaving like stompable enemies.

## Preconditions
- An active stage is loaded.
- At least one cactus hazard is present in the level.

## Trigger
- The player contacts a cactus hazard during active gameplay.

## Requirements
- `LEVEL.DYNAMICS.003-R1`: The system shall support cactus hazards as environmental stage actors distinct from enemy classes.
- `LEVEL.DYNAMICS.003-R1A`: Cactus hazards shall use readable cactus obstacle art rather than generic debug blocks or enemy sprites.
- `LEVEL.DYNAMICS.003-R2`: A cactus hazard shall remain stationary after stage load unless a later spec explicitly defines alternate behavior.
- `LEVEL.DYNAMICS.003-R3`: Contact with a cactus hazard shall resolve using the normal player damage rules for non-stomp threats.
- `LEVEL.DYNAMICS.003-R4`: Jumping or falling onto a cactus hazard shall not defeat the hazard as if it were a stompable enemy.
- `LEVEL.DYNAMICS.003-R5`: Cactus hazards shall be authorable in normal stage composition and may be normalized onto valid terrain support at load time.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Cactus hazard damages the player on side contact
  Given a cactus hazard is present on grounded terrain
  When the player touches the cactus from the side
  Then player damage resolution shall occur

Scenario: Cactus hazard is not defeated by a stomp-like landing
  Given a cactus hazard is present on grounded terrain
  When the player lands on the cactus from above
  Then the cactus hazard shall remain active
  And player damage or non-safe contact resolution shall occur

Scenario: Cactus hazard stays fixed in place
  Given a cactus hazard is present in a stage
  When stage simulation advances
  Then the cactus hazard shall remain stationary
```

## Example Inputs/Outputs
- Example input: Small-form player runs into a cactus obstacle.
- Expected output: The player loses a life according to normal damage rules.
- Example input: Super player lands on a cactus obstacle from above.
- Expected output: The cactus remains active and the player downgrades according to damage rules.

## Edge Cases
- Simultaneous contact with an enemy and a cactus hazard shall resolve deterministically according to runtime collision order.
- A cactus hazard normalized onto terrain shall not be buried below the support surface or left visibly floating.
- Cactus art and hurt bounds shall remain readable against the active terrain and background palette.

## Non-Functional Constraints
- Cactus hazards should be visually readable early enough for the player to react during normal camera follow.
- Hazard collision should feel consistent across repeated attempts.

## Related Specs
- `LEVEL.AUTHORING.001`
- `PLAYER.POWERUPS.001`
- `PLAYER.MOVEMENT.001`
- `CAMERA.FOLLOW.001`
