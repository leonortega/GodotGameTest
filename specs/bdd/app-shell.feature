Feature: Title flow and global game shell
  As a player
  I want clear startup and session-level transitions
  So that I can reliably start, pause, and restart the game

  Scenario: Startup opens on title screen
    Given the game is launchable
    When the player starts the game
    Then the title screen is the first visible screen
    And Start Game and Controls actions are visible
    And a distinct game logo is visible in the title layout
    And the title screen shows the player running from right to left while escaping an enemy
    And the title lettering uses an 8-bit style font

  Scenario: Start Game loads the first stage
    Given the title screen is visible
    When the player selects Start Game
    Then a black pre-stage transition screen is shown
    And the current player appearance and remaining lives are centered on screen
    And after approximately 3 seconds stage 1-1 loads
    And the player spawns at the stage start

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
    And the player can start a new game
