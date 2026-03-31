Feature: SMB-style stage element presets
  As a generator, designer, or player
  I want clear block families and biome presets
  So that stages use consistent SMB-like vocabulary

  Scenario: Question and brick blocks remain distinct
    Given question blocks and brick blocks are present in a stage
    When the player approaches them during gameplay
    Then the two block families remain visually distinguishable
    And each block occupies a readable near-full-tile footprint

  Scenario: Exhausted question block becomes a used block
    Given a question block contains one authored reward
    When the player hits the block and the reward is dispensed
    Then the block enters the used block state
    And it does not continue dispensing rewards

  Scenario: Hidden block can reveal an authored reward
    Given a hidden block contains one authored reward
    When the player hits the hidden block from below
    Then the hidden block reveals itself
    And the authored reward is dispensed

  Scenario: Small player cannot break a normal brick block
    Given the player is in Small Form
    And a normal brick block is placed above the player
    When the player hits the brick block from below
    Then the block remains intact

  Scenario: Super player breaks an eligible brick block
    Given the player is in Super Form
    And an eligible breakable brick block is placed above the player
    When the player hits the brick block from below
    Then the brick block breaks

  Scenario: Dispensed item emerges upward before normal motion
    Given a question block contains a Mushroom
    When the player hits the block from below
    Then the Mushroom first rises out of the block
    And it only begins normal item motion after emergence completes

  Scenario: Block bump affects an actor resting above
    Given an actor is resting on a strikeable block
    When the player hits that block from below
    Then the actor receives the authored bump interaction for its type

  Scenario: Multi-coin block pays out repeatedly before exhaustion
    Given a multi-coin block has remaining payout charges
    When the player repeatedly hits the block with valid timing
    Then coins continue to dispense until the payout limit is reached
    And the block converts to a used block

  Scenario: Underground preset allows ceiling corridor sections
    Given the active theme preset is underground
    When a stage section is generated with a constrained ceiling corridor
    Then the section is allowed by the preset
    And obstacle and landing readability remain preserved

  Scenario: Theme preset selects biome motifs
    Given an overworld, underground, athletic_sky, or castle preset is active
    When the stage presentation is assembled
    Then the preset's biome motifs are used for that stage
    And those motifs do not reduce gameplay readability

  Scenario: Coin presentation remains readable
    Given collectible coins appear along the main route
    When the player reads the route at normal gameplay scale
    Then the coins remain large enough to read as collectibles rather than tiny decorative dots
