Feature: Save slot and progression persistence
  As a returning player
  I want my progress and settings to persist
  So that I do not restart from scratch every launch

  Scenario: Stage clear updates highest cleared stage
    Given the player clears stage 1-2
    When progression is saved
    Then highest cleared stage is persisted

  Scenario: Saved settings restore on relaunch
    Given the player previously changed audio settings
    When the application launches again
    Then the saved settings are restored

  Scenario: Legacy save data migrates forward
    Given a prior local save exists using an older flat JSON structure
    When the game launches on the current build
    Then the stored progression and settings are mapped into the current save structure
    And launch continues without a blocking migration error

  Scenario: Missing save data falls back safely
    Given no prior save file exists
    When the game launches
    Then default settings and zero progression are used
