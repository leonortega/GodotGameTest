Feature: In-game HUD
  As a player
  I want the HUD to show critical state clearly
  So that I can make decisions while playing

  Scenario: Coin pickup updates coin display
    Given the player has 12 coins
    When the player collects 1 coin
    Then the HUD shows 13 coins

  Scenario: HUD shows the active stage identifier
    Given stage 1-3 is loaded
    When gameplay begins
    Then the HUD shows 1-3 as the stage identifier

  Scenario: HUD field order remains fixed
    Given the HUD renders score, coins, stage, and time fields
    When the values change during active play
    Then the left-to-right field order remains unchanged

  Scenario: HUD fields remain aligned across text-only stats
    Given the HUD renders score, coins, stage, and time fields
    When the header row is displayed
    Then the labels and values remain consistently aligned

  Scenario: HUD uses a dark backing for readability
    Given the stage background behind the HUD is bright
    When the top stats row is rendered
    Then a dark or black backing keeps the HUD readable

  Scenario: HUD chrome remains decorative and consistent
    Given the HUD renders score, coins, stage, and time fields
    When themed pixel panel chrome is applied
    Then the stat values remain text-first and aligned
    And the chrome remains visually consistent with the shell menus

  Scenario: Pause freezes the visible timer
    Given the player is in an active stage
    When the player pauses the game
    Then the visible timer stops decreasing
