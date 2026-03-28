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

  Scenario: Hazards and uneven terrain are authorable
    Given a designer adds cactus hazards and height variation to a stage
    When the stage is saved
    Then those changes exist through level-scene authoring
    And no stage-specific controller logic is modified

  Scenario: Slope terrain is authorable
    Given a designer adds an uphill run or a paired slope plateau section to a stage
    When the stage is saved
    Then that slope terrain exists through reusable stage data or scene composition
    And runtime collision and terrain presentation remain valid without bespoke per-stage code

  Scenario: Minor placement offsets can be normalized at load time
    Given an enemy or hazard is authored slightly off the supporting terrain
    When the stage loads
    Then runtime normalization places that content on valid support
    And no bespoke per-stage correction script is required

  Scenario: Stage metadata exposes runtime context
    Given stage 1-3 is loaded
    When the runtime requests stage metadata
    Then spawn point, timer, stage identifier, and world bounds are available
