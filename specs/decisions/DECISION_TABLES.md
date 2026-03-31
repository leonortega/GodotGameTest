# Decision Tables

## Damage and Form Resolution (`PLAYER.POWERUPS.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Starting form | `Small` | Applies on new game and new life |
| Growth pickup in `Small` | Upgrade to `Super` | Basic survivability upgrade |
| Attack pickup in `Super` | Upgrade to `Fire` | Enables ranged attack |
| Damage in `Fire` | Downgrade to `Super` | Starts damage invulnerability window |
| Damage in `Super` | Downgrade to `Small` | Starts damage invulnerability window |
| Damage in `Small` | Lose life | No extra buffer |
| Damage during invulnerability | Ignore | No stacking downgrade or life loss |
| `Super Star` collected | Enter temporary star invincibility | Overrides normal contact damage rules |
| `1-Up Mushroom` collected | Add 1 life | Current form remains unchanged |
| Fire projectile count limit reached | Reject new projectile spawn | Existing projectiles remain active |

## Enemy Interaction Rules (`ENEMIES.CORE.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Ground enemy wall collision | Reverse direction | Basic patrol behavior |
| Ground enemy detects gap ahead | Reverse direction | Enemy does not intentionally walk into pits |
| Flying enemy motion | Repeat fixed pattern | Must remain readable |
| Top-down stomp on stompable enemy | Defeat enemy, bounce player, play inverted falling death animation | Awards score |
| Side or below enemy contact | Apply damage resolution | Unless invulnerable |
| Standard stomp on armored enemy | Enemy remains active | May still threaten player |
| Projectile hit on armored enemy | Reflect projectile, remain active | Returned shot becomes a hostile threat |
| Stomp on protected-head enemy top | Enemy remains active | Top protection prevents crush result |
| Shooter fire condition met | Emit hostile projectile | Cadence must remain readable |
| Projectile hit on vulnerable enemy | Defeat enemy | Awards score |

## Stage Flow and Rewards (`LEVEL.PROGRESSION.001`, `HUD.STATUS.001`, `APP.SHELL.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Starting lives | `3` | New session baseline |
| Stage order | `1-1` -> `1-2` -> `1-3` -> `1-4` | MVP world only |
| Starting stage | `1-1` for every new run | Progression continues forward through the authored stage order |
| Title branding | Use dedicated game logo in title layout | Must remain readable over animation |
| Title-screen attract motion | Player runs right to left while fleeing an enemy | Keeps title screen lively without entering gameplay |
| Title-screen chaser | Choose from readable enemy variants | Current build rotates among a small curated set |
| Stage entry presentation | Centered black transition card for about `3` seconds | Shows world-stage and remaining lives |
| Stage entry layout | Minimal centered card with world-stage over player/life marker | SMB1-style presentation |
| Pause action default key | `Escape` with fallback pause key | Routed through named `pause` input |
| Pause menu options | `Resume`, `Restart Level`, `Title` | Restart does not consume an extra life |
| Goal marker touched while alive | Clear current stage | Timer stops immediately |
| Non-final stage clear presentation | Use minimal direct transition to next stage | No centered summary overlay or `Continue` prompt |
| Standard stage timer | `400` seconds | Baseline budget for a full-length route |
| Compact stage timer | `300` seconds | Use only when the route budget is materially shorter |
| Timer reaches `0` | Lose life | Stage restarts if lives remain |
| Post-death restart point | Stage start | No checkpoint requirement in MVP |
| Goal marker presentation | Compact ground-level endpoint marker allowed | Tall pole not required for MVP |
| HUD timer during pause | Freeze visible countdown | Matches paused simulation |
| HUD field order | Score, Coins, World, Time | Stable left-to-right parsing order |
| Hub between stages | None | SMB1 baseline uses direct stage-to-stage flow |
| Game-over screen | Minimal text-first presentation | No persistent best-score wall |
| World-clear screen | Minimal text-first presentation | No stat-heavy dashboard |

## Score and Economy Rules (`SCORE.ECONOMY.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Coin score value | `200` points | Applies to coin collection from any supported source |
| Brick break score value | `50` points | Applies to eligible brick breaks |
| Standard power-up score value | `1000` points | Applies to Mushroom, Fire Flower, and Super Star collection |
| Duplicate power-up collection | Award score instead of changing form | See `PLAYER.POWERUPS.001` |
| Extra life coin threshold | `100` coins | Awards 1 life, then continues tracking from remainder |
| Time bonus multiplier | `50` points per second | Applied at stage clear when time bonus is used |
| Stomp combo chain | `100, 200, 400, 500, 800, 1000, 2000, 4000, 5000, 8000, 1-Up...` | Repeats 1-Up after the authored threshold |
| Shell combo chain | `500, 800, 1000, 2000, 4000, 5000, 8000, 1-Up...` | Repeats 1-Up after the authored threshold |
| Visible life count cap | Explicit authored cap required | Baseline should avoid overflow bugs or ambiguous counters |

## Movement and Collision Rules (`PLAYER.MOVEMENT.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Ground acceleration | Authored deterministic ramp-up | Avoid instant full-speed starts |
| Ground deceleration | Authored deterministic slowdown | Avoid instant full-speed stops |
| Run input active | Increase horizontal speed | Also extends jump distance |
| Reverse direction at run speed | Enter skid, then reverse | Keeps turnaround readable |
| Second jump input after takeoff | Consume one authored airborne follow-up jump | Available exactly once before landing |
| Additional jump input after airborne follow-up jump | Ignore until landing | No third jump before landing |
| Late ledge jump input | No required grace conversion | SMB1 baseline does not depend on coyote time |
| Pre-landing jump input | No required input buffering | SMB1 baseline does not depend on jump buffering |
| Air control strength | Weaker than ground control | Prevent instant midair reversal |
| Simultaneous opposite directions | Resolve deterministically | Avoid jitter |
| Upward collision with interactive block | Trigger block response | Example: coin or power-up release |
| Upward collision with solid tile | Stop upward motion | No reward |
| Fall below bounds | Lose life | Used for pits and missed jumps |
| Authored strike block too close to support below | Lift or reject placement | Must preserve minimum playable hit clearance |

## Dynamic Traversal Rules (`LEVEL.DYNAMICS.001`, `LEVEL.DYNAMICS.002`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Dynamic traversal layout source | See `LEVEL.GENERATION.001` | Start buffers, route spacing, recovery landings, and challenge placement are defined there |
| Floating platform has no support below | Remains airborne and patrols horizontally | No visible support column required |
| Player stands on floating moving platform | Carry player with platform motion | Normal jump remains available |
| Player jumps off moving platform | Platform continues patrol | Player detaches normally |
| Falling block width | Match one terrain block | Visual and collision footprint stay aligned |
| Falling block touched briefly | Remain suspended | Trigger delay not yet met |
| Falling block touched for `0.5` seconds | Only the contacted block falls | Neighboring blocks remain suspended until triggered separately |
| Player stands on a falling block | Start that block's trigger timer | Contact is per block, not per authored line |
| Player leaves a falling block early | Reset that block's trigger timer | Must be touched again long enough |

## Difficulty Scaling Rules (`DIFFICULTY.BALANCE.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| SMB1 baseline difficulty selection | Not exposed to the player | Core shell flow does not use a difficulty menu |
| Optional difficulty-enabled build | Use `Normal` by default | Applies only outside the SMB1 baseline |
| Difficulty increased | Increase simultaneous on-screen enemy count | Higher pressure, same core rules |
| Difficulty changed mid-run | Apply at next defined boundary | Example: next stage load |

## Godot Project and Input Rules (`ENGINE.PROJECT.001`, `PLAYER.INPUT.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Engine target | Godot `4.x` | Validate exact patch version during project bootstrap |
| Shared runtime state | Autoload singleton | Used for run state and cross-scene access |
| Shared audio access | Autoload or globally reachable audio service | Avoid duplicate mixer logic in stage scenes |
| Repeated gameplay objects | `PackedScene` instances or scene tiles | Avoid duplicated scene trees |
| Input handling | Named Input Map actions | No direct gameplay dependence on raw key codes |
| Desktop input bindings | Keyboard-first named actions | Current build uses multi-key fallbacks for jump, action, and pause, with `Escape` as primary pause key |

## Godot Level and Camera Authoring Rules (`LEVEL.AUTHORING.001`, `CAMERA.FOLLOW.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Stage container | One Godot scene per stage | Supports isolated level iteration |
| Terrain authoring | Shared TileSet with separated TileMap layers | Solid, hazard, and decoration concerns stay distinct |
| Interactive placement | Scene instances or scene tiles | Coins, pickups, enemies, goal markers |
| Dynamic traversal placement | Reusable scene composition | Moving platforms and single falling block scenes |
| Hazard placement | Scene instances or hazard tiles | Example: cactus obstacles |
| Metadata source | Level scene plus lightweight manifest data | Provides stage id, timer, spawn, and bounds |
| Layout playability source | See `LEVEL.GENERATION.001` | Terrain rhythm, gap budgets, and route-safety rules are centralized there |
| Camera implementation | `Camera2D` | Uses bounds and readability-focused follow rules |
| Baseline backtracking | Not freely scrollable after camera advance | SMB1-style forward progression bias |
| Camera outside bounds | Never allowed | Prevents exposing non-authored space |
| Vertical jitter policy | Suppressed for small jumps | Favors readability over strict centering |
| HUD presentation | Text-first stat row with retro styling | Avoids sprite-icon readability problems |
| HUD readability backing | Use dark top-band panel or equivalent contrast treatment | Prevent bright backgrounds from washing out stats |

## Stage Element Preset Rules (`LEVEL.ELEMENTS.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Reward block families | Support `Question`, `Brick`, `Hidden`, and `Used` blocks | Core SMB-like block vocabulary |
| Question block exhaustion | Convert to `Used` after contents are spent | No infinite payout after exhaustion |
| Supported authored block contents | `Coin`, `Mushroom`, `Fire Flower`, `Super Star`, `1-Up Mushroom` | Baseline pickup vocabulary |
| Item emergence from block | Rise out before normal motion | Keeps reward state readable |
| Small-form brick hit | Do not break normal bricks | Still resolves as an upward strike |
| Super- or Fire-form brick hit | Break eligible bricks | Applies only where the active preset allows it |
| Multi-coin block payout cap | `10` coins in underground baseline example | Exact cap belongs to the active preset |
| Block bump affecting actors above | Enabled | Actor-specific resolution applies after a strike from below |
| Active shipped presets | `overworld`, `underground`, `athletic_sky`, `castle` | Available for normal generation |
| Water preset status | Planned only by default | Not selected unless explicitly enabled |
| Underground ceiling corridors | Allowed only when enabled by the preset | Must preserve obstacle and landing readability |
| Biome motifs | Use preset-defined layers and structural cues | Never hide gameplay-critical silhouettes |

## Procedural Stage Generation Rules (`LEVEL.GENERATION.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Generation basis | Use a generation profile plus element preset per stage theme | Holds traversal, terrain, gap, platform, and stage-element limits |
| Main route completion | Preserve one continuous route from spawn to goal | Required route uses guaranteed traversal only |
| Optional branch completion | Never required | May reconnect or end in reward and safe return |
| Safe start buffer | Keep first `10` tiles stable and free of forced danger in overworld baseline | Gives immediate reaction time after spawn |
| Safe goal approach | Keep final `12` tiles readable and stable | Avoid unfair last-second failures |
| Minimum main-route ground coverage | `60%` in overworld baseline | Prevents excessive empty space |
| Mandatory gap width | Max `4` tiles in overworld baseline | Validate against generation profile |
| Consecutive mandatory gaps | Max `2` before recovery ground | Prevents fatigue and unfair chaining |
| Recovery ground after forced challenge | At least `6` tiles | Lets the player reset rhythm |
| Elevation step between required landings | Max `4` tiles in overworld baseline | Prevents blind vertical spikes |
| Flat span without a traversal beat | Max `18` tiles in overworld baseline | Prevents dead air and monotony |
| Required moving-platform chain | Max `2` platforms in overworld baseline | Must include stable boarding and exit ground |
| Moving-platform position in stage flow | Not first forced obstacle and not last forced obstacle before goal | Protects opening clarity and ending fairness |
| Underground ceiling corridor usage | Allow only under underground preset | Controlled by active element preset |
| Decorative motif source | Use active element preset | Keeps biome visuals and block vocabulary consistent |

## Audio and Save Rules (`AUDIO.SYSTEM.001`, `SAVE.PROGRESSION.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Audio buses | `Music`, `SFX`, `UI` minimum | `Master` remains project root |
| Stage music ownership | Central audio service | Prevents duplicate playback when scenes reload |
| Music reaches end during active stage | Continue looping same cue | Runtime may force loop when asset import settings do not |
| Theme music families | Title, Overworld, Underground, Athletic/Sky, Castle, Invincibility, Game Over, World Clear | Water remains optional if that theme is not shipped |
| Low-time music behavior | Enter hurry-state music when timer falls below `100` | Must remain clearly urgent |
| Save slot count | One local slot minimum | Expandable later |
| Save format | Simple Godot-compatible serialized file | Human-inspectable when feasible |
| Persisted progression | Not required for SMB1 shell flow | Runs start fresh from title |
| Persisted settings | Audio volume minimum | Restored on next launch when supported |
| Corrupt save handling | Fall back to defaults | Never block startup |

## Runtime Feel Rules (`ENGINE.RUNTIME.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Terrain tile size | `16x16` logic grid | Keeps collision and block spacing predictable |
| Fixed simulation rate | `60 Hz` | Gameplay timing should not depend on render FPS |
| Input sampling | Every fixed simulation step | Avoids avoidable input lag |
| Off-screen actor simulation | Limit to activation window around camera | Prevents unfair unseen attacks |
| Forward activation margin | Small positive margin ahead of camera | Preserves reaction time |
| Rear activation margin | Smaller than forward margin | Favors forward play while allowing local cleanup |
| Active dynamic object budget | Explicit authored runtime cap required | Must protect readability and performance |
