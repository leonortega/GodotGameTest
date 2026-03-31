Feature: Title flow and global game shell
  As a player
  I want clear SMB1-style startup and session-level transitions
  So that I can reliably start, pause, and restart the game

  Scenario: Startup opens on title screen
    Given the game is launchable
    When the player starts the game
    Then the title screen is the first visible screen
    And Start Game is visible
    And a distinct game logo is visible in the title layout
    And the title screen shows the player running from right to left while escaping an enemy
    And the title preview uses real background, terrain, player, and enemy art
    And the title lettering uses an 8-bit style font
    And the title buttons and shell panels use themed retro UI chrome

  Scenario: Start Game loads stage 1-1
    Given the title screen is visible
    When the player selects Start Game
    Then a black pre-stage transition screen is shown
    And the current world-stage identifier and remaining lives are centered on screen
    And the stage card uses a minimal centered SMB1-style layout
    And after approximately 3 seconds stage 1-1 loads
    And the player spawns at the stage start

  Scenario: Pause suspends stage simulation
    Given the player is in an active stage
    When the player pauses the game
    Then stage timer countdown stops
    And enemy movement stops
    And only resume and menu navigation remain active

  Scenario: Pause menu offers restart and title actions
    Given the player is in an active stage
    When the player presses Escape
    Then the pause menu is shown
    And Resume, Restart Level, and Title are visible
    When the player selects Restart Level
    Then the same stage reloads from its stage start
    And no additional life is consumed

  Scenario: Non-final death restarts the current stage
    Given the player has more than 1 remaining life
    When the player loses a life
    Then a short death-to-restart transition occurs
    And the current world-stage identifier and updated remaining lives are shown
    And the same stage restarts from its stage start

  Scenario: Game over follows the final lost life
    Given the player has 1 remaining life
    When the player loses that life
    Then the game-over screen is shown
    And the game-over screen remains minimal and text-first
    And the player can start a new game

  Scenario: World clear ends the linear run without a hub
    Given the player clears stage 1-4
    When the world-clear screen is shown
    Then no world-map or hub screen appears after the clear
    And the player can start a new game
