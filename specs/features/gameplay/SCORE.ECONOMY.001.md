# Spec: `SCORE.ECONOMY.001`

## Metadata
- **Title**: SMB1-Style Score, Combo, and Extra-Life Economy
- **Version**: `v1.0`
- **Status**: Approved
- **Context/View**: Core Gameplay
- **Priority**: High

## Purpose
Define the deterministic SMB1-style score and life economy for coins, power-ups, combo chains, brick breaks, and time bonus conversion.

## Preconditions
- A gameplay run is active.

## Trigger
- Scoreable gameplay events occur, such as coin collection, enemy defeat, power-up collection, brick breaking, or stage clear.

## Requirements
- `SCORE.ECONOMY.001-R1`: The system shall use a deterministic authored score profile rather than ad hoc score values.
- `SCORE.ECONOMY.001-R1A`: The score profile shall define at least coin value, brick-break value, standard power-up value, duplicate power-up value, extra-life coin threshold, time-bonus multiplier, stomp combo chain, shell combo chain, and visible life cap.
- `SCORE.ECONOMY.001-R2`: Collecting one coin shall award `200` points in the baseline score profile.
- `SCORE.ECONOMY.001-R3`: Breaking an eligible brick block shall award `50` points in the baseline score profile.
- `SCORE.ECONOMY.001-R4`: Collecting a standard `Mushroom`, `Fire Flower`, or `Super Star` pickup shall award `1000` points in the baseline score profile.
- `SCORE.ECONOMY.001-R4A`: Collecting a duplicate growth or attack pickup while already at or above its target form shall award the authored duplicate power-up score value rather than changing player form.
- `SCORE.ECONOMY.001-R5`: Collecting `100` coins shall award exactly one additional life.
- `SCORE.ECONOMY.001-R5A`: After an extra life is awarded from coins, the coin counter shall continue from the remainder rather than becoming undefined.
- `SCORE.ECONOMY.001-R5B`: Collecting a `1-Up Mushroom` shall award exactly one additional life and shall not require a score substitution.
- `SCORE.ECONOMY.001-R6`: Successive valid airborne stomp defeats without landing shall use the authored stomp combo chain.
- `SCORE.ECONOMY.001-R6A`: The baseline stomp combo chain shall escalate through `100, 200, 400, 500, 800, 1000, 2000, 4000, 5000, 8000`, then award `1-Up` for further authored thresholds.
- `SCORE.ECONOMY.001-R6B`: Successive shell-driven chain defeats shall use the authored shell combo chain.
- `SCORE.ECONOMY.001-R6C`: The baseline shell combo chain shall escalate through `500, 800, 1000, 2000, 4000, 5000, 8000`, then award `1-Up` for further authored thresholds.
- `SCORE.ECONOMY.001-R7`: When a stage-clear time bonus is applied, remaining time shall convert into score using `50` points per second in the baseline score profile.
- `SCORE.ECONOMY.001-R8`: The visible life counter shall use an explicit authored cap and shall not overflow into undefined UI behavior.

## Acceptance Criteria (BDD)
```gherkin
Scenario: Coin awards score and extra life at threshold
  Given the player has 99 coins and a known score
  When the player collects 1 coin
  Then the score shall increase by 200 points
  And the player shall gain 1 life

Scenario: Duplicate power-up awards score instead of changing form
  Given the player is already in Fire Form
  When the player collects a Fire Flower
  Then the player's form shall remain Fire
  And the duplicate power-up score value shall be awarded

Scenario: Stomp combo chain escalates during one airborne sequence
  Given the player defeats multiple valid enemies by stomp without landing
  When the combo chain advances
  Then the awarded values shall follow the authored stomp combo table in order

Scenario: Shell combo chain escalates across repeated defeats
  Given a shell defeats multiple valid targets in one chain
  When the combo chain advances
  Then the awarded values shall follow the authored shell combo table in order

Scenario: Stage clear converts time to score
  Given the player clears a stage with 91 seconds remaining
  When the time bonus is applied
  Then 4550 points shall be awarded from remaining time
```

## Example Inputs/Outputs
- Example input: Player collects the 100th coin of the run.
- Expected output: Score increases by `200`, one additional life is awarded, and the coin counter continues from the post-threshold remainder.
- Example input: Player clears a stage with `87` seconds remaining.
- Expected output: The time bonus awards `4350` points using the `50`-points-per-second rule.

## Edge Cases
- Multiple score events resolving in the same frame shall produce deterministic total score changes.
- Coin-derived extra lives shall not be skipped when score tallying or stage-clear bonus conversion is happening at the same time.
- A visible life cap shall not corrupt internal run state or UI if the player earns additional lives beyond normal expectations.

## Non-Functional Constraints
- Score updates should remain readable and deterministic under rapid chained events.
- Authored score tables should remain data-driven enough to rebalance without rewriting gameplay code.

## Related Specs
- `PLAYER.POWERUPS.001`
- `LEVEL.PROGRESSION.001`
- `LEVEL.ELEMENTS.001`
