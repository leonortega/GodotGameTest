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

  Scenario: Super Star overrides stage music temporarily
    Given stage music is active
    When the player collects a Super Star
    Then invincibility music replaces the active stage music
    And normal stage music behavior resumes after invincibility ends

  Scenario: Low time triggers hurry music behavior
    Given stage music is active
    And the visible stage timer is above 100
    When the visible timer falls below 100
    Then the audio system enters the authored hurry-state music behavior

  Scenario: Core events emit distinct sound effects
    Given the player jumps and collects a coin
    When both events resolve
    Then distinct jump and coin sounds play on the SFX bus

  Scenario: Jump and landing use distinct traversal sounds
    Given the player performs a grounded jump and then lands on terrain
    When the traversal resolves
    Then distinct jump and landing sounds play

  Scenario: Power-down and death remain distinct
    Given the player loses a form and later loses a life
    When both outcomes resolve
    Then the power-down cue is distinct from the death cue

  Scenario: Shell states use authored music assets
    Given the title screen is visible
    When the player remains on title or reaches game over or world clear
    Then the shell uses the authored music asset mapped to that state

  Scenario: Pause applies consistent audio behavior
    Given stage music is active
    When the player pauses the game
    Then the audio system applies the defined pause behavior consistently
