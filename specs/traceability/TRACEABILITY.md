# Traceability

## ID Convention
- Pattern: `<CONTEXT>.<AREA>.<NNN>`
- Examples:
  - `APP.SHELL.001`
  - `PLAYER.MOVEMENT.001`
  - `LEVEL.PROGRESSION.001`

## Mapping Rules
1. Every spec ID must have at least one BDD scenario.
2. Every contract file must reference at least one spec ID.
3. Every example fixture must reference exactly one primary spec ID.
4. Related specs should be linked bidirectionally where behavior depends on each other.

## Initial Coverage Matrix

| Spec ID | Feature File | Contract | Examples |
|---|---|---|---|
| `APP.SHELL.001` | `bdd/app-shell.feature` | `contracts/json-schema/game-session.schema.json` | `examples/application-shell/app-shell.startup.json` |
| `ENGINE.PROJECT.001` | `bdd/engine-project.feature` | `contracts/json-schema/project-layout.schema.json` | `examples/godot-project/engine-project.scene-tree.json` |
| `PLAYER.INPUT.001` | `bdd/player-input.feature` | `contracts/json-schema/input-map.schema.json` | `examples/godot-project/input-map.default.json` |
| `PLAYER.MOVEMENT.001` | `bdd/player-movement.feature` | `contracts/json-schema/player-state.schema.json` | `examples/player/player-movement.basic-gap.json` |
| `PLAYER.POWERUPS.001` | `bdd/player-powerups.feature` | `contracts/json-schema/player-state.schema.json`, `contracts/json-schema/game-session.schema.json` | `examples/player/player-powerups.damage-cycle.json` |
| `SCORE.ECONOMY.001` | `bdd/score-economy.feature` | `contracts/json-schema/score-profile.schema.json` | `examples/gameplay/score-economy.smb1-profile.json` |
| `ENEMIES.CORE.001` | `bdd/enemies-core.feature` | `contracts/json-schema/enemy-encounter.schema.json` | `examples/enemies/enemies-core.stomp-armored.json` |
| `DIFFICULTY.BALANCE.001` | `bdd/difficulty-balance.feature` | `-` | `examples/gameplay/difficulty-balance.enemy-density.json` |
| `LEVEL.PROGRESSION.001` | `bdd/level-progression.feature` | `contracts/json-schema/level-result.schema.json`, `contracts/json-schema/game-session.schema.json` | `examples/level-flow/level-progression.stage-clear.json` |
| `HUD.STATUS.001` | `bdd/hud-status.feature` | `contracts/json-schema/hud-state.schema.json` | `examples/hud/hud-status.ingame.json` |
| `LEVEL.AUTHORING.001` | `bdd/level-authoring.feature` | `contracts/json-schema/level-manifest.schema.json` | `examples/levels/level-manifest.1-1.json` |
| `LEVEL.GENERATION.001` | `bdd/level-generation.feature` | `contracts/json-schema/level-generation-profile.schema.json` | `examples/levels/level-generation.overworld-smb.json` |
| `LEVEL.ELEMENTS.001` | `bdd/level-elements.feature` | `contracts/json-schema/level-element-preset.schema.json` | `examples/levels/level-elements.underground-smb.json` |
| `ENGINE.RUNTIME.001` | `bdd/engine-runtime.feature` | `contracts/json-schema/runtime-profile.schema.json` | `examples/godot-project/engine-runtime.smb1.json` |
| `CAMERA.FOLLOW.001` | `bdd/camera-follow.feature` | `contracts/json-schema/camera-profile.schema.json` | `examples/camera/camera-follow.bounds.json` |
| `AUDIO.SYSTEM.001` | `bdd/audio-system.feature` | `contracts/json-schema/audio-buses.schema.json` | `examples/audio/audio-buses.default.json` |
| `SAVE.PROGRESSION.001` | `bdd/save-progression.feature` | `contracts/json-schema/save-slot.schema.json` | `examples/save/save-slot.new-game.json` |
