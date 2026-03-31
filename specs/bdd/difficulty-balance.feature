Feature: Difficulty level scaling
  As a player
  I want optional difficulty scaling to change enemy density
  So that non-baseline builds can become more challenging at higher settings

  Scenario: Optional hard difficulty increases active enemy count
    Given the same stage is loaded on Normal difficulty
    And the same stage is loaded on Hard difficulty
    When enemy activation rules are applied
    Then Hard difficulty allows more active enemies on screen than Normal difficulty

  Scenario: Optional easy difficulty can thin authored enemy pressure
    Given the same authored stage layout is loaded on Normal difficulty
    And the same authored stage layout is loaded on Easy difficulty
    When difficulty-tuned enemy content is prepared
    Then Easy difficulty may contain fewer active enemy actors than the Normal runtime setup

  Scenario: Optional hard difficulty can add bonus enemy instances
    Given a stage contains authored non-flying enemies
    When the stage is prepared on Hard difficulty
    Then the runtime may add a small number of additional enemy instances
    And those additional enemies spawn on valid readable support

  Scenario: Normal is the default difficulty when optional scaling is enabled
    Given a non-baseline build enables difficulty scaling
    When difficulty settings are initialized
    Then the selected difficulty is Normal
