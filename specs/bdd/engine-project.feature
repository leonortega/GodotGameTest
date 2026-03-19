Feature: Godot project foundation
  As a developer
  I want a stable Godot architecture
  So that the game can scale beyond a one-off prototype

  Scenario: Main scene controls runtime flow
    Given the project is opened in Godot
    When the main scene is inspected
    Then it supports title, gameplay, pause, game-over, and world-clear flow

  Scenario: Shared state survives scene transitions
    Given the player starts a run from the title screen
    When the project transitions into stage 1-1
    Then run state remains available through a shared service

  Scenario: Repeated objects are reusable scene instances
    Given a level contains multiple enemies and pickups
    When the level scene is reviewed
    Then repeated gameplay objects are represented by reusable scenes or scene tiles
