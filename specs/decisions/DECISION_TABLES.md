# Decision Tables

## Damage and Form Resolution (`PLAYER.POWERUPS.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Starting form | `Small` | Applies on new game and new life |
| Growth pickup in `Small` | Upgrade to `Powered` | Basic survivability upgrade |
| Attack pickup in `Powered` | Upgrade to `Enhanced` | Enables ranged attack |
| Damage in `Enhanced` | Downgrade to `Powered` | Starts invulnerability window |
| Damage in `Powered` | Downgrade to `Small` | Starts invulnerability window |
| Damage in `Small` | Lose life | No extra buffer |
| Damage during invulnerability | Ignore | No stacking downgrade or life loss |
| Projectile count limit reached | Reject new projectile spawn | Existing projectiles remain active |

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

## Stage Flow and Rewards (`LEVEL.PROGRESSION.001`, `HUD.STATUS.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Starting lives | `3` | New session baseline |
| Stage order | `1-1` -> `1-2` -> `1-3` -> `1-4` | MVP world only |
| Title-selected starting stage | Allowed before run start | Progression continues forward from selected stage |
| Title branding | Use dedicated game logo in title layout | Must remain readable over animation |
| Title-screen attract motion | Player runs right to left while fleeing an enemy | Keeps title screen lively without entering gameplay |
| Title-screen chaser | Choose from readable enemy variants | Current build rotates among a small curated set |
| Stage entry presentation | Centered black transition card for about `3` seconds | Shows current player appearance and remaining lives |
| Goal marker touched while alive | Clear current stage | Timer stops immediately |
| Non-final stage clear presentation | Show centered summary for about `3` seconds, then auto-advance | No `Continue` prompt required |
| Timer reaches `0` | Lose life | Stage restarts if lives remain |
| Coin threshold for extra life | `100` coins | Coin counter may roll or persist by implementation |
| Time bonus at stage clear | Remaining time converts to score | Exact multiplier may be tuned later |
| Post-death restart point | Stage start | No checkpoint requirement in MVP |
| Goal marker presentation | Compact ground-level endpoint marker allowed | Tall pole not required for MVP |
| HUD timer during pause | Freeze visible countdown | Matches paused simulation |

## Movement and Collision Rules (`PLAYER.MOVEMENT.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Run input active | Increase horizontal speed | Also extends jump distance |
| First airborne jump input after takeoff | Allow second jump | Double-jump window remains active until landing |
| Additional jump input after double jump | Ignore | No third jump before landing |
| Simultaneous opposite directions | Resolve deterministically | Avoid jitter |
| Upward collision with interactive block | Trigger block response | Example: coin or power-up release |
| Upward collision with solid tile | Stop upward motion | No reward |
| Fall below bounds | Lose life | Used for pits and missed jumps |
| Authored strike block too close to support below | Lift or reject placement | Must preserve minimum playable hit clearance |

## Difficulty Scaling Rules (`DIFFICULTY.BALANCE.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| No explicit difficulty selected | Use `Normal` | Baseline balance profile |
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
| Desktop input bindings | Keyboard-first named actions | Current build uses multi-key fallbacks for jump, action, and pause |

## Godot Level and Camera Authoring Rules (`LEVEL.AUTHORING.001`, `CAMERA.FOLLOW.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Stage container | One Godot scene per stage | Supports isolated level iteration |
| Terrain authoring | Shared TileSet with separated TileMap layers | Solid, hazard, and decoration concerns stay distinct |
| Interactive placement | Scene instances or scene tiles | Coins, pickups, enemies, goal markers |
| Hazard placement | Scene instances or hazard tiles | Example: cactus obstacles |
| Metadata source | Level scene plus lightweight manifest data | Provides stage id, timer, spawn, and bounds |
| Terrain profile | Allow rises, drops, and uneven ground | Avoid over-reliance on long flat paths |
| Camera implementation | `Camera2D` | Uses bounds and readability-focused follow rules |
| Camera outside bounds | Never allowed | Prevents exposing non-authored space |
| Vertical jitter policy | Suppressed for small jumps | Favors readability over strict centering |
| HUD presentation | Text-first stat row with retro styling | Avoids sprite-icon readability problems |
| HUD readability backing | Use dark top-band panel or equivalent contrast treatment | Prevent bright backgrounds from washing out stats |

## Audio and Save Rules (`AUDIO.SYSTEM.001`, `SAVE.PROGRESSION.001`)

| Decision Point | Default Rule | Notes |
|---|---|---|
| Audio buses | `Music`, `SFX`, `UI` minimum | `Master` remains project root |
| Stage music ownership | Central audio service | Prevents duplicate playback when scenes reload |
| Music reaches end during active stage | Continue looping same cue | Runtime may force loop when asset import settings do not |
| Save slot count | One local slot minimum | Expandable later |
| Save format | Simple Godot-compatible serialized file | Human-inspectable when feasible |
| Persisted progression | Highest cleared stage and best score | Current world scope only |
| Persisted settings | Audio and presentation preferences | Restored on next launch |
| Corrupt save handling | Fall back to defaults | Never block startup |
