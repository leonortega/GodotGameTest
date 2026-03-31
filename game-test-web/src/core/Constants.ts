export const GAME_WIDTH = 960;
export const GAME_HEIGHT = 540;
export const TILE_SIZE = 32;
export const WORLD_HEIGHT_TILES = 17;
export const WORLD_HEIGHT = WORLD_HEIGHT_TILES * TILE_SIZE;

export const PLAYER_TUNING = {
  groundAcceleration: 1800,
  runAcceleration: 2350,
  airAcceleration: 900,
  groundDrag: 2100,
  maxWalkSpeed: 176,
  maxRunSpeed: 264,
  jumpVelocity: -560,
  followUpJumpVelocity: -500,
  maxFallSpeed: 900,
  skidThreshold: 140,
  skidDrag: 3200,
  shortHopGravityMultiplier: 1.85,
  jumpHoldGraceMs: 170,
  damageInvulnerabilityMs: 1400,
  starInvulnerabilityMs: 7000,
  fireballLimit: 2,
  fireballSpeed: 320,
  fireballBounceVelocity: -265,
};

export const SCORE_VALUES = {
  coin: 200,
  brickBreak: 50,
  standardPowerup: 1000,
  duplicatePowerup: 1000,
  timeBonusPerSecond: 50,
  stompCombo: [100, 200, 400, 500, 800, 1000, 2000, 4000, 5000, 8000],
};

export const INITIAL_LIVES = 3;
export const EXTRA_LIFE_COIN_THRESHOLD = 100;
export const MAX_VISIBLE_LIVES = 99;

export const FONT_FAMILY = '"Silkscreen", monospace';

export const DEPTHS = {
  background: 0,
  terrain: 10,
  blocks: 20,
  actors: 30,
  pickups: 35,
  hud: 100,
  overlay: 200,
};

export const SCENES = {
  boot: "boot",
  preload: "preload",
  title: "title",
  startCard: "start-card",
  game: "game",
  gameOver: "game-over",
  worldClear: "world-clear",
} as const;

export const STAGE_ORDER = ["1-1", "1-2", "1-3", "1-4"] as const;

export type StageId = (typeof STAGE_ORDER)[number];
export type ThemeId = "overworld" | "underground" | "athletic_sky" | "castle";
export type PlayerForm = "Small" | "Super" | "Fire";
export type PickupType = "Coin" | "Mushroom" | "Fire Flower" | "Super Star" | "1-Up Mushroom";
export type BlockType = "question" | "brick" | "hidden" | "used";
