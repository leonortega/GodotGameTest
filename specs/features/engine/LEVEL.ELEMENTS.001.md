# Spec: `LEVEL.ELEMENTS.001`

## Metadata
- **Title**: SMB-Style Stage Element Presets for Blocks, Biomes, and Structural Vocabularies
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Level Elements
- **Priority**: High

## Purpose
Define the reusable SMB-style stage-element presets that control block and item families, exhausted block states, hidden rewards, biome motifs, underground ceiling corridors, and theme-specific generation vocabularies.

## Preconditions
- A stage theme or generation preset is being selected, or block content is being authored in a stage.

## Trigger
- Stage generation, level authoring, or block interaction during gameplay.

## Requirements
- `LEVEL.ELEMENTS.001-R1`: The system shall support distinct `Question Block`, `Brick Block`, `Hidden Block`, and `Used Block` stage elements.
- `LEVEL.ELEMENTS.001-R1A`: `Question Block`, `Brick Block`, and `Used Block` presentations shall remain visually distinguishable at gameplay scale without relying on HUD text.
- `LEVEL.ELEMENTS.001-R1C`: `Question Block`, `Brick Block`, and `Used Block` presentations shall occupy a readable near-full-tile footprint at gameplay scale rather than appearing as undersized sprites inside their authored tile space.
- `LEVEL.ELEMENTS.001-R1B`: `Hidden Block` behavior shall be authorable for optional rewards without requiring a visible pre-hit block sprite.
- `LEVEL.ELEMENTS.001-R2`: A `Question Block` or `Hidden Block` shall resolve authored contents such as coins, power-ups, or repeated coin payout according to its configured content rule.
- `LEVEL.ELEMENTS.001-R2A`: After a `Question Block` has exhausted its authored contents, it shall convert to the `Used Block` state and shall not continue dispensing rewards.
- `LEVEL.ELEMENTS.001-R2B`: Authored block contents shall support at least `Coin`, `Mushroom`, `Fire Flower`, `Super Star`, and `1-Up Mushroom`.
- `LEVEL.ELEMENTS.001-R2D`: Coin presentation shall remain readable at normal gameplay scale and shall not shrink to a decorative-dot size that obscures collectibility or route readability.
- `LEVEL.ELEMENTS.001-R2C`: When a block dispenses a physical item, that item shall first emerge upward out of the block before transitioning into its normal collectible or movement state.
- `LEVEL.ELEMENTS.001-R3`: A `Brick Block` shall behave as a solid strikeable block when hit from below by the player.
- `LEVEL.ELEMENTS.001-R3A`: A player in `Small Form` shall not break a normal `Brick Block` from below.
- `LEVEL.ELEMENTS.001-R3B`: A player in `Super Form` or `Fire Form`, as allowed by the active element preset, shall be able to break eligible `Brick Block` instances from below.
- `LEVEL.ELEMENTS.001-R3C`: Hitting a strikeable block from below shall apply a bump interaction to actors resting on top of that block.
- `LEVEL.ELEMENTS.001-R3D`: Bump interaction from below shall be able to displace or defeat actors above the block according to their actor-specific rules.
- `LEVEL.ELEMENTS.001-R4`: The system shall support authored `Multi-Coin Block` behavior in which a configured question block dispenses multiple coins across repeated valid hits before becoming a `Used Block`.
- `LEVEL.ELEMENTS.001-R4A`: A `Multi-Coin Block` shall have a bounded payout count or payout window defined by the active element preset.
- `LEVEL.ELEMENTS.001-R5`: Stage generation and authoring shall support theme-specific element presets for at least `overworld`, `underground`, `athletic_sky`, and `castle`.
- `LEVEL.ELEMENTS.001-R5A`: The system may reserve a `water` preset for later use, provided its status is explicitly marked as planned or inactive rather than silently treated as a normal shipped theme.
- `LEVEL.ELEMENTS.001-R6`: Each active theme preset shall define decorative but gameplay-relevant biome motifs, such as background layers, silhouette accents, and structural art cues, that reinforce the theme without obscuring collision readability.
- `LEVEL.ELEMENTS.001-R6A`: Biome motifs shall remain subordinate to gameplay readability and shall not conceal block silhouettes, pipes, gaps, ceilings, landing surfaces, or enemies.
- `LEVEL.ELEMENTS.001-R7`: The `underground` theme preset shall support low-ceiling corridor generation as part of its structural vocabulary.
- `LEVEL.ELEMENTS.001-R7A`: An underground ceiling corridor shall preserve enough horizontal and vertical readability that the player can still identify obstacles, gaps, blocks, and required movement through the constrained section.
- `LEVEL.ELEMENTS.001-R8`: `LEVEL.GENERATION.001` and `LEVEL.AUTHORING.001` shall consume an active stage-element preset rather than redefining block families, biome motifs, or ceiling-corridor allowances independently.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Question and brick blocks remain distinct
  Given question blocks and brick blocks are present in a stage
  When the player approaches them during gameplay
  Then the two block families shall remain visually distinguishable
  And each block shall occupy a readable near-full-tile footprint

Scenario: Exhausted question block becomes a used block
  Given a question block contains one authored reward
  When the player hits the block and the reward is dispensed
  Then the block shall enter the used block state
  And it shall not continue dispensing rewards

Scenario: Hidden block can reveal an authored reward
  Given a hidden block contains one authored reward
  When the player hits the hidden block from below
  Then the hidden block shall reveal itself
  And the authored reward shall be dispensed

Scenario: Small player cannot break a normal brick block
  Given the player is in Small Form
  And a normal brick block is placed above the player
  When the player hits the brick block from below
  Then the block shall remain intact

Scenario: Super player breaks an eligible brick block
  Given the player is in Super Form
  And an eligible breakable brick block is placed above the player
  When the player hits the brick block from below
  Then the brick block shall break

Scenario: Dispensed item emerges upward before normal motion
  Given a question block contains a Mushroom
  When the player hits the block from below
  Then the Mushroom shall first rise out of the block
  And it shall only begin normal item motion after emergence completes

Scenario: Block bump affects an actor resting above
  Given an actor is resting on a strikeable block
  When the player hits that block from below
  Then the actor shall receive the authored bump interaction for its type

Scenario: Multi-coin block pays out repeatedly before exhaustion
  Given a multi-coin block has remaining payout charges
  When the player repeatedly hits the block with valid timing
  Then coins shall continue to dispense until the payout limit is reached
  And the block shall then convert to a used block

Scenario: Underground preset allows ceiling corridor sections
  Given the active theme preset is underground
  When a stage section is generated with a constrained ceiling corridor
  Then the section shall be allowed by the preset
  And obstacle and landing readability shall remain preserved

Scenario: Theme preset selects biome motifs
  Given an overworld, underground, athletic_sky, or castle preset is active
  When the stage presentation is assembled
  Then the preset's biome motifs shall be used for that stage
  And those motifs shall not reduce gameplay readability

Scenario: Coin presentation remains readable
  Given collectible coins appear along the main route
  When the player reads the route at normal gameplay scale
  Then the coins shall remain large enough to read as collectibles rather than tiny decorative dots
```

## Example Inputs/Outputs
- Example input: An underground preset with brick blocks, question blocks, hidden blocks, multi-coin blocks, lantern and rock motifs, and enabled ceiling-corridor sections.
- Expected output: Underground stages can generate readable ceiling-constrained passages, use the underground motif set, and resolve question-block, hidden-block, brick-block, used-block, and multi-coin behavior according to the preset.
- Example input: A Super player hits an eligible brick block from below.
- Expected output: The brick block breaks, while an exhausted question block elsewhere remains in the used state.

## Edge Cases
- A used block shall not visually revert to an unspent question block after reload or camera culling.
- A multi-coin block shall not exceed its configured payout cap even if the player can hit it rapidly.
- A breakable brick block shall not break for `Small Form` due to stale form state during a power-down transition.
- Underground ceiling corridors shall not compress the playable space so tightly that required hazards or block interactions become unreadable.
- Theme motifs shall not be so dense that they hide a dark brick ceiling against a dark underground background.
- A planned `water` preset shall not be selected by normal generation unless explicitly enabled by content configuration.

## Non-Functional Constraints
- Stage-element presets should remain concise enough to drive deterministic generation and validation.
- Block state changes should remain visually obvious without introducing confusion between active and exhausted blocks.

## Related Specs
- `LEVEL.GENERATION.001`
- `LEVEL.AUTHORING.001`
- `PLAYER.MOVEMENT.001`
- `PLAYER.POWERUPS.001`
- `SCORE.ECONOMY.001`
