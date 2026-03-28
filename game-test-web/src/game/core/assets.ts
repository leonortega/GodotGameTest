import type { EnemyKind, PlayerForm } from './types';

export const PLAYER_VARIANT_BY_FORM: Record<PlayerForm, string> = {
  Small: 'player',
  Powered: 'adventurer',
  Enhanced: 'soldier',
};

export const PLAYER_POSES = ['idle', 'walk1', 'walk2', 'jump', 'fall', 'hurt', 'duck'] as const;
export type PlayerPose = (typeof PLAYER_POSES)[number];

export const PLAYER_VISUALS = {
  player: {
    idle: '/visuals/characters/player/idle.png',
    walk1: '/visuals/characters/player/walk1.png',
    walk2: '/visuals/characters/player/walk2.png',
    jump: '/visuals/characters/player/jump.png',
    fall: '/visuals/characters/player/fall.png',
    hurt: '/visuals/characters/player/hurt.png',
  },
  adventurer: {
    idle: '/visuals/characters/adventurer/idle.png',
    walk1: '/visuals/characters/adventurer/walk1.png',
    walk2: '/visuals/characters/adventurer/walk2.png',
    jump: '/visuals/characters/adventurer/jump.png',
    fall: '/visuals/characters/adventurer/fall.png',
    hurt: '/visuals/characters/adventurer/hurt.png',
  },
  soldier: {
    idle: '/visuals/characters/soldier/idle.png',
    walk1: '/visuals/characters/soldier/walk1.png',
    walk2: '/visuals/characters/soldier/walk2.png',
    jump: '/visuals/characters/soldier/jump.png',
    fall: '/visuals/characters/soldier/fall.png',
    hurt: '/visuals/characters/soldier/hurt.png',
  },
  zombie: {
    idle: '/visuals/characters/zombie/idle.png',
    walk1: '/visuals/characters/zombie/walk1.png',
    walk2: '/visuals/characters/zombie/walk2.png',
    hurt: '/visuals/characters/zombie/hurt.png',
  },
} as const;

export const TERRAIN_VISUALS = {
  top: { key: 'terrain-top', path: '/visuals/tiles/ground-top.png' },
  fill: { key: 'terrain-fill', path: '/visuals/tiles/ground-fill.png' },
  hazard: { key: 'terrain-hazard', path: '/visuals/tiles/hazard-block.png' },
  cactus: { key: 'hazard-cactus', path: '/visuals/tiles/cactus.png' },
} as const;

export const ENEMY_VISUALS: Record<Exclude<EnemyKind, 'Ground'>, { key: string; path: string }> = {
  Armored: { key: 'enemy-armored', path: '/visuals/tiles/enemy-armored.png' },
  Flying: { key: 'enemy-flying', path: '/visuals/tiles/enemy-flying.png' },
  ProtectedHead: { key: 'enemy-protected', path: '/visuals/tiles/enemy-protected.png' },
  Shooter: { key: 'enemy-shooter', path: '/visuals/tiles/enemy-shooter.png' },
};

export const BACKDROP_VISUALS = {
  sky: { key: 'bg-sky', path: '/visuals/backgrounds/flat/sky.png' },
  clouds: { key: 'bg-clouds', path: '/visuals/backgrounds/flat/clouds.png' },
  mountains: { key: 'bg-mountains', path: '/visuals/backgrounds/flat/mountains.png' },
  hills: { key: 'bg-hills', path: '/visuals/backgrounds/flat/hills.png' },
  tree: { key: 'bg-tree', path: '/visuals/backgrounds/flat/tree.png' },
} as const;

export const MUSIC_ASSETS = {
  title: '/audio/music/title.mp3',
  '1-1': '/audio/music/stage-1-1.mp3',
  '1-2': '/audio/music/stage-1-2.mp3',
  '1-3': '/audio/music/stage-1-3.mp3',
  '1-4': '/audio/music/stage-1-4.mp3',
  'game-over': '/audio/music/game-over.mp3',
  'world-clear': '/audio/music/world-clear.mp3',
} as const;

export const SFX_ASSETS = {
  uiTick: '/audio/sfx/ui/confirm.ogg',
  uiHover: '/audio/sfx/ui/hover.ogg',
  uiBack: '/audio/sfx/ui/menu-back.wav',
  pause: '/audio/sfx/ui/pause.ogg',
  jump: '/audio/sfx/gameplay/jump.wav',
  doubleJump: '/audio/sfx/gameplay/double-jump.wav',
  land: '/audio/sfx/gameplay/land.wav',
  coin: '/audio/sfx/gameplay/coin.wav',
  powerup: '/audio/sfx/gameplay/powerup.wav',
  powerDown: '/audio/sfx/gameplay/power-down.wav',
  enemyDefeat: '/audio/sfx/gameplay/enemy-defeat.wav',
  damage: '/audio/sfx/gameplay/hurt.wav',
  death: '/audio/sfx/gameplay/death.wav',
  shoot: '/audio/sfx/gameplay/shoot.wav',
  stageClear: '/audio/sfx/gameplay/stage-clear.wav',
  extraLife: '/audio/sfx/gameplay/extra-life.wav',
  blockHit: '/audio/sfx/gameplay/block-hit.wav',
} as const;

export function getPlayerTextureKey(form: PlayerForm, pose: PlayerPose): string {
  return `${PLAYER_VARIANT_BY_FORM[form]}-${pose}`;
}

export function getEnemyTextureKey(kind: EnemyKind): string {
  if (kind === 'Ground') {
    return 'zombie-idle';
  }

  return ENEMY_VISUALS[kind].key;
}
