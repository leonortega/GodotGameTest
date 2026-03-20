# Spec: `LEVEL.DYNAMICS.001`

## Metadata
- **Title**: Floating Moving Platforms
- **Version**: `v1.2`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: Medium

## Purpose
Define horizontally moving floating platforms that create airborne traversal without direct terrain support beneath them.

## Preconditions
- An active stage is loaded.
- A floating moving platform is present in the level.

## Trigger
- Stage simulation updates a moving platform or the player interacts with it.

## Requirements
- `LEVEL.DYNAMICS.001-R1`: The system shall support floating moving platforms that have no authored solid terrain directly beneath their traversable surface.
- `LEVEL.DYNAMICS.001-R2`: A floating moving platform shall move horizontally left and right between authored patrol extents or equivalent travel limits.
- `LEVEL.DYNAMICS.001-R3`: A floating moving platform shall remain suspended while moving and shall not require visible support columns or ground contact to operate.
- `LEVEL.DYNAMICS.001-R3A`: The full patrol envelope of a floating moving platform, including its left and right travel extents, shall remain over unsupported air rather than sweeping across grounded support before or after the midpoint.
- `LEVEL.DYNAMICS.001-R3B`: Authored moving platforms shall keep clear empty space before the left travel extent, after the right travel extent, and directly beneath the full patrol envelope so they do not visually or physically collide with surrounding map geometry.
- `LEVEL.DYNAMICS.001-R4`: A player standing on a floating moving platform shall be carried horizontally by that platform while normal grounded movement and jump rules remain available.
- `LEVEL.DYNAMICS.001-R5`: The player shall be able to jump onto and away from a floating moving platform without stopping the platform's patrol behavior.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Floating platform patrols horizontally in the air
  Given a floating moving platform is active in a stage
  And there is no solid ground directly beneath its traversable surface
  When the stage simulation advances
  Then the platform shall move left and right along its authored path

Scenario: Floating platform remains unsupported across its full sweep
  Given a floating moving platform has authored left and right patrol extents
  When the full patrol envelope is reviewed
  Then there shall be no grounded support directly beneath the platform at the midpoint, left extent, or right extent

Scenario: Floating platform keeps a clear traversal corridor
  Given a floating moving platform has authored width and patrol extents
  When the authored corridor is reviewed
  Then the platform shall have empty buffer space before and after its travel envelope
  And the full travel envelope shall remain clear of surrounding map geometry beneath it

Scenario: Player is carried by a moving platform
  Given the player is standing on a floating moving platform
  When the platform moves horizontally
  Then the player shall be carried with the platform
  And the player shall still be able to jump normally

Scenario: Jumping off a moving platform does not halt it
  Given the player is riding a floating moving platform
  When the player jumps away from it
  Then the platform shall continue its horizontal patrol
```

## Example Inputs/Outputs
- Example input: A floating platform patrols over a gap between two grounded sections.
- Expected output: The platform remains airborne, moves left and right, and can carry the player across the gap.
- Example input: A floating platform patrols over an unsupported air gap and its full sweep remains inside that gap.
- Expected output: The platform never passes over grounded support while moving between its left and right extents.

## Edge Cases
- A floating moving platform shall not drift outside the authored world bounds.
- The player shall not lose grounded riding behavior because the platform is airborne.
- The platform's reversal at each patrol extent shall remain readable and deterministic.
- A floating moving platform shall not be authored so wide, or with so much patrol distance, that its left or right sweep passes over supported terrain.
- A floating moving platform shall not be authored so close to neighboring terrain that its body or travel buffer visually clips into adjacent stage geometry.

## Non-Functional Constraints
- Platform motion should remain readable enough for timing-based jumps.
- Carry behavior should not introduce visible jitter between the player and the platform.

## Related Specs
- `PLAYER.MOVEMENT.001`
- `LEVEL.AUTHORING.001`
- `CAMERA.FOLLOW.001`
- `LEVEL.PROGRESSION.001`