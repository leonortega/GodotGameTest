Feature: Title flow and global game shell
  As a player
  I want clear startup and session-level transitions
  So that I can reliably start, pause, and restart the game

  Scenario: Startup opens on title screen
    Given the game is launchable
    When the player starts the game
    Then the title screen is the first visible screen
    And Start Game, Controls, Enemies, Configuration, and Difficulty actions are visible
    And a distinct game logo is visible in the title layout
    And the title screen shows the player running from right to left while escaping an enemy
    And the title preview uses real background, terrain, player, and enemy art
    And the title lettering uses an 8-bit style font
    And the title buttons and shell panels use themed retro UI chrome

  Scenario: Start Game loads stage 1-1
    Given the title screen is visible
    When the player selects Start Game
    Then a black pre-stage transition screen is shown
    And the current player appearance and remaining lives are centered on screen
    And after approximately 3 seconds stage 1-1 loads
    And the player spawns at the stage start

  Scenario: Title actions cycle difficulty and open configuration
    Given the title screen is visible
    When the player activates the Difficulty action
    Then the visible difficulty changes to the next option
    When the player activates the Configuration action
    Then the configuration menu opens
    And persisted audio settings can be adjusted

  Scenario: Title actions open controls and enemy guide overlays
    Given the title screen is visible
    When the player activates the Controls action
    Then a help overlay opens
    And the overlay summarizes movement, jump, double-jump, attack, and pause inputs
    When the player activates the Enemies action
    Then an enemy-guide overlay opens
    And the guide shows enemy portraits, names, and short behavior or weakness notes

  Scenario: Pause suspends stage simulation
    Given the player is in an active stage
    When the player pauses the game
    Then stage timer countdown stops
    And enemy movement stops
    And only resume and menu navigation remain active

  Scenario: Game over follows the final lost life
    Given the player has 1 remaining life
    When the player loses that life
    Then the game-over screen is shown
    And the persisted best score is shown
    And the player can start a new game

  Scenario: World clear surfaces saved progression
    Given the player clears stage 1-4
    When the world-clear screen is shown
    Then the final run score is shown
    And the persisted highest clear metric is shown
    And the player can start a new game
