Feature: Settings persistence
  As a returning player
  I want my audio settings to persist
  So that my preferred presentation returns on every launch

  Scenario: Saved settings restore on relaunch
    Given the player previously changed audio settings
    When the application launches again
    Then the saved settings are restored

  Scenario: Legacy save data migrates forward
    Given a prior local save exists using an older flat JSON structure
    When the game launches on the current build
    Then the stored supported settings are mapped into the current save structure
    And launch continues without a blocking migration error

  Scenario: Missing save data falls back safely
    Given no prior save file exists
    When the game launches
    Then default settings are used
