Feature: Godot audio system
  As a player
  I want clear music and effects
  So that the game feels readable and responsive

  Scenario: Stage music starts when gameplay becomes active
    Given stage 1-1 loads
    When gameplay begins
    Then stage music plays on the Music bus

  Scenario: Stage music loops continuously
    Given stage music is active
    When the current track reaches its end
    Then the same stage music continues playing without unintended silence

  Scenario: Core events emit distinct sound effects
    Given the player jumps and collects a coin
    When both events resolve
    Then distinct jump and coin sounds play on the SFX bus

  Scenario: Pause applies consistent audio behavior
    Given stage music is active
    When the player pauses the game
    Then the audio system applies the defined pause behavior consistently
