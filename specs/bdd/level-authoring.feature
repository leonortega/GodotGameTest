Feature: Godot level authoring
  As a designer
  I want stages authored through tile layers and scene placement
  So that content iteration is fast and reliable

  Scenario: Terrain and decoration are authored separately
    Given a level scene is open in Godot
    When the tile layers are reviewed
    Then solid terrain and decoration are not authored on the same logical layer

  Scenario: Background and terrain art remain reusable
    Given a designer adds scenic background layers and stylized terrain tiles to a stage
    When the stage is saved
    Then those visuals exist through reusable background or terrain assets
    And collision remains separate from decorative presentation

  Scenario: Interactive objects are placeable without controller changes
    Given a designer adds a coin to stage 1-2
    When the stage is saved
    Then the coin exists through scene placement or scene tiles
    And no player-controller coordinates are modified

  Scenario: Interactive strike blocks remain reachable from below
    Given a designer places a mystery block above valid floor support
    When the stage is saved
    Then the player can jump under the block and hit it using supported movement
    And the block keeps enough standing clearance beneath it

  Scenario: Terrain and hazard vocabulary are authorable
    Given a designer adds cactus hazards, hills, height variation, and platform sections to a stage
    When the stage is saved
    Then those stage elements exist through level-scene authoring
    And no stage-specific controller logic is modified

  Scenario: Layout validation defers to generation rules
    Given a designer authors spawn, goal, gaps, elevated surfaces, and moving-platform sections in one stage
    When the stage is saved
    Then route continuity, jump reach, start safety, goal safety, and recovery landings are checked through LEVEL.GENERATION.001

  Scenario: Slope terrain is authorable
    Given a designer adds an uphill run or a paired slope plateau section to a stage
    When the stage is saved
    Then that slope terrain exists through reusable stage data or scene composition
    And runtime collision and terrain presentation remain valid without bespoke per-stage code

  Scenario: Gameplay pieces do not overlap invalidly
    Given a designer authors coins, blocks, platforms, and hazards in one section
    When the stage is saved
    Then those gameplay pieces do not intersect in invalid ways
    And terrain and interactive blocks remain proportionate to the player and enemies

  Scenario: Minor placement offsets can be normalized at load time
    Given an enemy or hazard is authored slightly off the supporting terrain
    When the stage loads
    Then runtime normalization places that content on valid support
    And no bespoke per-stage correction script is required

  Scenario: Stage metadata exposes runtime context
    Given stage 1-3 is loaded
    When the runtime requests stage metadata
    Then spawn point, timer, stage identifier, and world bounds are available
