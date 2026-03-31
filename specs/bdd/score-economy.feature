Feature: Score and life economy
  As a player
  I want score and extra-life rules to behave predictably
  So that rewards feel fair and readable

  Scenario: Coin awards score and extra life at threshold
    Given the player has 99 coins and a known score
    When the player collects 1 coin
    Then the score increases by 200 points
    And the player gains 1 life

  Scenario: Duplicate power-up awards score instead of changing form
    Given the player is already in Fire Form
    When the player collects a Fire Flower
    Then the player's form remains Fire
    And the duplicate power-up score value is awarded

  Scenario: Stomp combo chain escalates during one airborne sequence
    Given the player defeats multiple valid enemies by stomp without landing
    When the combo chain advances
    Then the awarded values follow the authored stomp combo table in order

  Scenario: Shell combo chain escalates across repeated defeats
    Given a shell defeats multiple valid targets in one chain
    When the combo chain advances
    Then the awarded values follow the authored shell combo table in order

  Scenario: Stage clear converts time to score
    Given the player clears a stage with 91 seconds remaining
    When the time bonus is applied
    Then 4550 points are awarded from remaining time
