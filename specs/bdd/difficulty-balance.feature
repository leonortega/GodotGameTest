Feature: Difficulty level scaling
  As a player
  I want difficulty options to change enemy density
  So that the game becomes more challenging at higher settings

  Scenario: Hard difficulty increases active enemy count
    Given the same stage is loaded on Normal difficulty
    And the same stage is loaded on Hard difficulty
    When enemy activation rules are applied
    Then Hard difficulty allows more active enemies on screen than Normal difficulty

  Scenario: Easy difficulty can thin authored enemy pressure
    Given the same authored stage layout is loaded on Normal difficulty
    And the same authored stage layout is loaded on Easy difficulty
    When difficulty-tuned enemy content is prepared
    Then Easy difficulty may contain fewer active enemy actors than the Normal runtime setup

  Scenario: Hard difficulty can add bonus enemy instances
    Given a stage contains authored non-flying enemies
    When the stage is prepared on Hard difficulty
    Then the runtime may add a small number of additional enemy instances
    And those additional enemies spawn on valid readable support

  Scenario: Normal is the default difficulty
    Given the player starts a new run with no stored difficulty override
    When difficulty settings are initialized
    Then the selected difficulty is Normal
