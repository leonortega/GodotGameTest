Feature: Procedural retro platformer stage generation
  As a generator or validator
  I want layout rules for terrain, gaps, hills, and platforms
  So that random stages remain playable and readable

  Scenario: Generated stage preserves a safe spawn buffer
    Given an overworld generation profile defines a safe start buffer
    When a new stage is generated
    Then the spawn opens on stable ground
    And no mandatory gap or lethal hazard appears inside the safe start buffer

  Scenario: Main route remains completable with guaranteed traversal only
    Given a generation profile defines guaranteed traversal moves
    When a stage is generated from that profile
    Then one continuous main route exists from spawn to goal
    And the main route does not require optional traversal moves

  Scenario: Mandatory gaps are followed by recovery ground
    Given a generation profile defines gap and recovery limits
    When a stage is generated
    Then each mandatory gap on the main route stays within the allowed width
    And each such gap is followed by a valid recovery landing or recovery ground section
    And the leading edge of that recovery ground remains a reliable landing surface

  Scenario: Moving-platform challenge pockets remain boardable and exitable
    Given a generation profile allows required moving platforms
    When a stage is generated with a moving-platform pocket
    Then the pocket includes stable boarding ground before the first required platform
    And the pocket includes stable exit ground after the final required platform
    And the required moving-platform chain does not exceed the allowed maximum

  Scenario: Optional branches do not replace the main route
    Given a generation profile allows hidden routes or bonus branches
    When a stage is generated
    Then the main route remains complete without entering the optional branch
    And the optional branch either reconnects safely or ends in a reward and safe return

  Scenario: Invalid random layouts are rejected or repaired
    Given a candidate stage layout contains an impossible landing or over-wide mandatory gap
    When the validation pass runs
    Then the layout is repaired to satisfy the generation profile or rejected from output

  Scenario: Theme preset controls special structural features
    Given a generation profile references an underground element preset
    When the generator assembles a constrained corridor section
    Then the generator may use an underground ceiling corridor
    And the section still satisfies the playability rules
