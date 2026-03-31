# Spec: `LEVEL.GENERATION.001`

## Metadata
- **Title**: Procedural Stage Layout Rules for Playable Retro Platformer Levels
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Level Generation
- **Priority**: High

## Purpose
Define the rules a procedural stage generator and validation pass shall follow so randomly built stages remain usable, readable, and completable in a Super-Mario-style side-scrolling platformer, using `LEVEL.ELEMENTS.001` for theme and block vocabularies.

## Preconditions
- A stage seed or regeneration request has been issued.
- A generation profile is available for the target stage theme.

## Trigger
- Procedural stage generation, regeneration, or post-generation validation.

## Requirements
- `LEVEL.GENERATION.001-R1`: The system shall support a generation profile that defines guaranteed traversal moves, optional traversal moves, terrain limits, gap limits, platform limits, route-safety limits, and an active element preset for the target stage theme.
- `LEVEL.GENERATION.001-R1A`: The active element preset shall be sourced from `LEVEL.ELEMENTS.001` and shall define available block families, biome motifs, and structural allowances such as underground ceiling corridors.
- `LEVEL.GENERATION.001-R2`: A generated stage shall contain one continuous main route from spawn to goal.
- `LEVEL.GENERATION.001-R2A`: The main route shall be completable using only the profile's guaranteed traversal moves.
- `LEVEL.GENERATION.001-R2B`: Optional branches, bonus pockets, and hidden routes may assume optional traversal moves, but they shall never be required for stage completion.
- `LEVEL.GENERATION.001-R3`: The stage start shall provide a safe start buffer of stable ground with no mandatory gap, lethal hazard, or enemy contact pressure inside the profile-defined opening distance.
- `LEVEL.GENERATION.001-R4`: The goal approach shall provide a stable and readable final approach with no unfair forced hazard or blind landing immediately before the goal marker.
- `LEVEL.GENERATION.001-R5`: Main-route terrain generation shall obey the profile's minimum ground coverage and maximum mandatory gap width.
- `LEVEL.GENERATION.001-R5A`: The generator shall not place more consecutive mandatory gaps than the profile allows before inserting recovery ground.
- `LEVEL.GENERATION.001-R5B`: Each mandatory gap, drop, or moving-platform pocket on the main route shall be followed by a recovery landing or recovery ground section of at least the profile-defined minimum size.
- `LEVEL.GENERATION.001-R5C`: Recovery ground after a mandatory gap shall begin with a stable leading landing edge, and validation shall repair or reject layouts where the first intended recovery tile would fail as a reliable landing surface.
- `LEVEL.GENERATION.001-R6`: Hills, stairs, plateaus, and other elevation changes on the main route shall obey the profile's maximum required step height and maximum blind-drop depth.
- `LEVEL.GENERATION.001-R6A`: If an ascent exceeds one direct step within the guaranteed traversal profile, the generator shall add intermediate landings, stairs, ramps, or equivalent climbable support.
- `LEVEL.GENERATION.001-R6B`: The generator shall bound non-interactive flat spans and dead-air spans so the stage does not degrade into long empty corridors or off-screen leaps with no readable support.
- `LEVEL.GENERATION.001-R7`: Floating static platforms on the main route shall provide readable boarding and exit surfaces and shall respect the profile's spacing limits.
- `LEVEL.GENERATION.001-R7A`: Required floating moving platforms shall be generated as bounded challenge pockets with stable boarding ground, stable exit ground, and a required chain length no greater than the profile-defined maximum.
- `LEVEL.GENERATION.001-R7B`: A required moving-platform pocket shall not be the first forced obstacle after spawn or the final forced obstacle before the goal approach.
- `LEVEL.GENERATION.001-R8`: The generator shall not place enemies, hazards, blocks, or decorative occluders where they invalidate a required jump arc, landing zone, boarding zone, exit zone, or recovery landing.
- `LEVEL.GENERATION.001-R9`: Hidden routes, bonus pockets, and sub-area entrances may branch from the main route, but they shall either reconnect safely to the run or terminate in a clear reward and safe return or exit path.
- `LEVEL.GENERATION.001-R10`: The generated stage shall distribute major traversal beats across an intro, build, relief, and finale rhythm rather than clustering all difficult terrain in one narrow region.
- `LEVEL.GENERATION.001-R11`: A generated layout that violates any required playability rule shall be repaired or rejected by validation rather than emitted as a playable stage.
- `LEVEL.GENERATION.001-R12`: Theme-specific structural features, including underground ceiling corridors, shall only be generated when allowed by the active element preset.
- `LEVEL.GENERATION.001-R13`: Decorative biome motifs shall be selected from the active element preset and shall remain readable enough that collision surfaces, gaps, blocks, and hazards stay visually legible.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Generated stage preserves a safe spawn buffer
  Given an overworld generation profile defines a safe start buffer
  When a new stage is generated
  Then the spawn opens on stable ground
  And no mandatory gap or lethal hazard appears inside the safe start buffer

Scenario: Main route remains completable with guaranteed traversal only
  Given a generation profile defines guaranteed traversal moves
  When a stage is generated from that profile
  Then one continuous main route exists from spawn to goal
  And the main route does not require optional traversal moves

Scenario: Mandatory gaps are followed by recovery ground
  Given a generation profile defines gap and recovery limits
  When a stage is generated
  Then each mandatory gap on the main route stays within the allowed width
  And each such gap is followed by a valid recovery landing or recovery ground section
  And the leading edge of that recovery ground remains a reliable landing surface

Scenario: Moving-platform challenge pockets remain boardable and exitable
  Given a generation profile allows required moving platforms
  When a stage is generated with a moving-platform pocket
  Then the pocket includes stable boarding ground before the first required platform
  And the pocket includes stable exit ground after the final required platform
  And the required moving-platform chain does not exceed the allowed maximum

Scenario: Optional branches do not replace the main route
  Given a generation profile allows hidden routes or bonus branches
  When a stage is generated
  Then the main route remains complete without entering the optional branch
  And the optional branch either reconnects safely or ends in a reward and safe return

Scenario: Invalid random layouts are rejected or repaired
  Given a candidate stage layout contains an impossible landing or over-wide mandatory gap
  When the validation pass runs
  Then the layout is repaired to satisfy the generation profile or rejected from output

Scenario: Theme preset controls special structural features
  Given a generation profile references an underground element preset
  When the generator assembles a constrained corridor section
  Then the generator may use an underground ceiling corridor
  And the section shall still satisfy the playability rules
```

## Example Inputs/Outputs
- Example input: Generate an overworld stage using the `overworld_smb_safe` generation profile and a fresh random seed.
- Expected output: The stage contains a safe start, a readable main route, bounded hills and gaps, optional side rewards, and a stable goal approach without impossible jumps.
- Example input: Generate a stage segment containing floating and moving platforms over a pit.
- Expected output: The segment has stable boarding ground, no more than the allowed required platform chain, and a stable recovery landing after the challenge.

## Edge Cases
- A hidden route entrance shall not consume the only safe main-route landing surface in the same section.
- A hill, staircase, or plateau shall not create a blind ceiling collision that invalidates the next required jump.
- A moving-platform pocket shall not require boarding from a slope or from a tile already occupied by a hazard or enemy.
- Recovery ground shall not be counted if the landing surface is immediately obstructed by an unavoidable enemy or hazard.
- A decorative sky gap or parallax opening shall not be misclassified as usable traversal space.

## Non-Functional Constraints
- Generated layouts should remain readable enough for first-attempt interpretation rather than trial-and-error memorization.
- Validation should be deterministic for a given seed and generation profile.

## Related Specs
- `LEVEL.AUTHORING.001`
- `LEVEL.ELEMENTS.001`
- `PLAYER.MOVEMENT.001`
- `LEVEL.PROGRESSION.001`
- `CAMERA.FOLLOW.001`
