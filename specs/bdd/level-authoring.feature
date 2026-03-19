Feature: Godot level authoring
  As a designer
  I want stages authored through tile layers and scene placement
  So that content iteration is fast and reliable

  Scenario: Terrain and decoration are authored separately
    Given a level scene is open in Godot
    When the tile layers are reviewed
    Then solid terrain and decoration are not authored on the same logical layer

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

  Scenario: Stage metadata exposes runtime context
    Given stage 1-3 is loaded
    When the runtime requests stage metadata
    Then spawn point, timer, stage identifier, and world bounds are available
