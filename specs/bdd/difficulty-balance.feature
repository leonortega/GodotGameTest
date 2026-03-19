Feature: Difficulty level scaling
  As a player
  I want difficulty options to change enemy density
  So that the game becomes more challenging at higher settings

  Scenario: Hard difficulty increases active enemy count
    Given the same stage is loaded on Normal difficulty
    And the same stage is loaded on Hard difficulty
    When enemy activation rules are applied
    Then Hard difficulty allows more active enemies on screen than Normal difficulty

  Scenario: Normal is the default difficulty
    Given the player starts a new run with no stored difficulty override
    When difficulty settings are initialized
    Then the selected difficulty is Normal
