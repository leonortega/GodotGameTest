import {
  type Difficulty,
  type EnemyDef,
  type RectDef,
  type StageDefinition,
} from '../core/types';
import { PLAYER_MOVEMENT } from './player';

export const STAGE_ORDER = ['1-1', '1-2', '1-3', '1-4'] as const;

const DIFFICULTY_RANK: Record<Difficulty, number> = {
  Easy: 0,
  Normal: 1,
  Hard: 2,
};

const PLAYER_STANDING_HEIGHT = 72;
const BLOCK_SIZE = 48;
const COIN_SIZE = 22;
const PICKUP_PADDING = 8;
const ELEVATED_TERRAIN_THRESHOLD = 480;
const MIN_ELEVATED_PLATFORM_WIDTH = 96;
const MIN_ELEVATED_PLATFORM_HEIGHT = 20;
const MAX_ELEVATED_PLATFORM_HEIGHT = 72;
const MIN_MOVING_PLATFORM_WIDTH = 120;
const MAX_MOVING_PLATFORM_WIDTH = 220;
const MIN_MOVING_PLATFORM_HEIGHT = 18;
const MAX_MOVING_PLATFORM_HEIGHT = 28;
const MIN_FALLING_BLOCK_WIDTH = 40;
const MAX_FALLING_BLOCK_WIDTH = 64;
const MIN_FALLING_BLOCK_HEIGHT = 20;
const MAX_FALLING_BLOCK_HEIGHT = 32;
const MAX_DOUBLE_JUMP_RISE =
  Math.ceil(
    (Math.abs(PLAYER_MOVEMENT.groundJumpVelocity) ** 2 +
      Math.abs(PLAYER_MOVEMENT.doubleJumpVelocity) ** 2) /
      (2 * 1180),
  ) + 8;
const MAX_DOUBLE_JUMP_GAP = 320;
const MIN_BLOCK_CLEARANCE = PLAYER_STANDING_HEIGHT;
const MAX_BLOCK_CLEARANCE =
  PLAYER_STANDING_HEIGHT +
  Math.ceil((Math.abs(PLAYER_MOVEMENT.groundJumpVelocity) ** 2) / (2 * 1180)) +
  8;

type SurfaceKind = 'terrain' | 'moving' | 'falling';

interface SurfaceRef {
  kind: SurfaceKind;
  index: number;
  xMin: number;
  xMax: number;
  topY: number;
  width: number;
  height: number;
}

function shouldSpawnAtDifficulty(
  currentDifficulty: Difficulty,
  minimumDifficulty: Difficulty | undefined,
): boolean {
  return DIFFICULTY_RANK[currentDifficulty] >= DIFFICULTY_RANK[minimumDifficulty ?? 'Easy'];
}

function coinsLine(x: number, y: number, count: number, spacing = 52) {
  return Array.from({ length: count }, (_, index) => ({
    x: x + index * spacing,
    y,
  }));
}

function normalizeRect(rect: RectDef): RectDef {
  return {
    x: Math.round(rect.x),
    y: Math.round(rect.y),
    width: Math.max(8, Math.round(rect.width)),
    height: Math.max(8, Math.round(rect.height)),
  };
}

function clamp(value: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, value));
}

function centeredRect(x: number, y: number, width: number, height: number): RectDef {
  return {
    x: Math.round(x - width / 2),
    y: Math.round(y - height / 2),
    width: Math.round(width),
    height: Math.round(height),
  };
}

function rectsOverlap(a: RectDef, b: RectDef, padding = 0): boolean {
  return !(
    a.x + a.width + padding <= b.x ||
    b.x + b.width + padding <= a.x ||
    a.y + a.height + padding <= b.y ||
    b.y + b.height + padding <= a.y
  );
}

function horizontalGap(a: SurfaceRef, b: SurfaceRef): number {
  if (a.xMin > b.xMax) {
    return a.xMin - b.xMax;
  }

  if (b.xMin > a.xMax) {
    return b.xMin - a.xMax;
  }

  return 0;
}

function cloneStage(stage: StageDefinition): StageDefinition {
  return {
    ...stage,
    cameraBounds: { ...stage.cameraBounds },
    spawn: { ...stage.spawn },
    goal: { ...stage.goal },
    terrain: stage.terrain.map((rect) => ({ ...rect })),
    slopes: stage.slopes?.map((slope) => ({ ...slope })),
    blocks: stage.blocks.map((block) => ({ ...block })),
    coins: stage.coins.map((coin) => ({ ...coin })),
    cactusHazards: stage.cactusHazards.map((hazard) => ({ ...hazard })),
    movingPlatforms: stage.movingPlatforms.map((platform) => ({ ...platform })),
    fallingBlocks: stage.fallingBlocks.map((block) => ({ ...block })),
    enemies: stage.enemies.map((enemy) => ({ ...enemy })),
  };
}

function normalizeTerrainSizing(stage: StageDefinition): void {
  stage.terrain = stage.terrain.map((rect) => {
    const normalized = normalizeRect(rect);

    if (normalized.y < ELEVATED_TERRAIN_THRESHOLD) {
      normalized.width = Math.max(MIN_ELEVATED_PLATFORM_WIDTH, normalized.width);
      normalized.height = clamp(
        normalized.height,
        MIN_ELEVATED_PLATFORM_HEIGHT,
        MAX_ELEVATED_PLATFORM_HEIGHT,
      );
    }

    return normalized;
  });

  stage.movingPlatforms = stage.movingPlatforms.map((platform) => ({
    ...platform,
    width: clamp(Math.round(platform.width), MIN_MOVING_PLATFORM_WIDTH, MAX_MOVING_PLATFORM_WIDTH),
    height: clamp(
      Math.round(platform.height),
      MIN_MOVING_PLATFORM_HEIGHT,
      MAX_MOVING_PLATFORM_HEIGHT,
    ),
    x: Math.round(platform.x),
    y: Math.round(platform.y),
    minX: Math.round(platform.minX),
    maxX: Math.round(platform.maxX),
  }));

  stage.fallingBlocks = stage.fallingBlocks.map((block) => ({
    ...block,
    width: clamp(Math.round(block.width), MIN_FALLING_BLOCK_WIDTH, MAX_FALLING_BLOCK_WIDTH),
    height: clamp(
      Math.round(block.height),
      MIN_FALLING_BLOCK_HEIGHT,
      MAX_FALLING_BLOCK_HEIGHT,
    ),
    x: Math.round(block.x),
    y: Math.round(block.y),
  }));
}

function buildTraversalSurfaces(stage: StageDefinition): SurfaceRef[] {
  const terrainSurfaces = stage.terrain.map((rect, index) => ({
    kind: 'terrain' as const,
    index,
    xMin: rect.x,
    xMax: rect.x + rect.width,
    topY: rect.y,
    width: rect.width,
    height: rect.height,
  }));
  const movingSurfaces = stage.movingPlatforms.map((platform, index) => {
    const rect = centeredRect(platform.x, platform.y, platform.width, platform.height);

    return {
      kind: 'moving' as const,
      index,
      xMin: rect.x,
      xMax: rect.x + rect.width,
      topY: rect.y,
      width: rect.width,
      height: rect.height,
    };
  });
  const fallingSurfaces = stage.fallingBlocks.map((block, index) => {
    const rect = centeredRect(block.x, block.y, block.width, block.height);

    return {
      kind: 'falling' as const,
      index,
      xMin: rect.x,
      xMax: rect.x + rect.width,
      topY: rect.y,
      width: rect.width,
      height: rect.height,
    };
  });

  return [...terrainSurfaces, ...movingSurfaces, ...fallingSurfaces];
}

function applySurfaceTop(stage: StageDefinition, surface: SurfaceRef, nextTopY: number): void {
  const roundedTop = Math.round(nextTopY);

  if (surface.kind === 'terrain') {
    stage.terrain[surface.index].y = roundedTop;
    return;
  }

  if (surface.kind === 'moving') {
    stage.movingPlatforms[surface.index].y = Math.round(
      roundedTop + stage.movingPlatforms[surface.index].height / 2,
    );
    return;
  }

  stage.fallingBlocks[surface.index].y = Math.round(
    roundedTop + stage.fallingBlocks[surface.index].height / 2,
  );
}

function normalizeReachableSurfaces(stage: StageDefinition): void {
  const surfaces = buildTraversalSurfaces(stage).sort((a, b) => b.topY - a.topY);

  surfaces.forEach((surface) => {
    if (surface.topY >= ELEVATED_TERRAIN_THRESHOLD) {
      return;
    }

    const supports = surfaces.filter((candidate) => {
      if (candidate.kind === surface.kind && candidate.index === surface.index) {
        return false;
      }

      if (candidate.topY <= surface.topY) {
        return false;
      }

      return horizontalGap(surface, candidate) <= MAX_DOUBLE_JUMP_GAP;
    });

    const reachable = supports.some(
      (support) => support.topY - surface.topY <= MAX_DOUBLE_JUMP_RISE,
    );

    if (reachable || supports.length === 0) {
      return;
    }

    const nearestSupport = supports.reduce((best, candidate) =>
      candidate.topY < best.topY ? candidate : best,
    );
    const nextTopY = nearestSupport.topY - MAX_DOUBLE_JUMP_RISE;

    applySurfaceTop(stage, surface, nextTopY);
    surface.topY = nextTopY;
  });
}

function findSupportTopForBlock(stage: StageDefinition, blockX: number, blockY: number): number | null {
  const supports = buildTraversalSurfaces(stage);
  let nearestTop: number | null = null;

  supports.forEach((surface) => {
    const insideX = blockX >= surface.xMin + 8 && blockX <= surface.xMax - 8;
    const reachableY = surface.topY >= blockY && surface.topY <= blockY + 220;

    if (!insideX || !reachableY) {
      return;
    }

    if (nearestTop === null || surface.topY < nearestTop) {
      nearestTop = surface.topY;
    }
  });

  return nearestTop;
}

function normalizeBlocks(stage: StageDefinition): void {
  stage.blocks = stage.blocks.map((block) => {
    const supportTop = findSupportTopForBlock(stage, block.x, block.y);

    if (supportTop === null) {
      return {
        ...block,
        x: Math.round(block.x),
        y: Math.round(block.y),
      };
    }

    const currentBottom = block.y + BLOCK_SIZE / 2;
    const currentClearance = supportTop - currentBottom;
    const targetClearance = clamp(currentClearance, MIN_BLOCK_CLEARANCE, MAX_BLOCK_CLEARANCE);

    return {
      ...block,
      x: Math.round(block.x),
      y: Math.round(supportTop - targetClearance - BLOCK_SIZE / 2),
    };
  });
}

function buildOccupiedRects(stage: StageDefinition): RectDef[] {
  const terrain = getStageCollisionRects(stage);
  const moving = stage.movingPlatforms.map((platform) =>
    centeredRect(platform.x, platform.y, platform.width, platform.height),
  );
  const falling = stage.fallingBlocks.map((block) =>
    centeredRect(block.x, block.y, block.width, block.height),
  );
  const blocks = stage.blocks.map((block) => centeredRect(block.x, block.y, BLOCK_SIZE, BLOCK_SIZE));
  const cacti = stage.cactusHazards.map((hazard) =>
    centeredRect(hazard.x, hazard.y - 34, 48, 68),
  );

  return [...terrain, ...moving, ...falling, ...blocks, ...cacti];
}

function normalizeCoins(stage: StageDefinition): void {
  const occupiedRects = buildOccupiedRects(stage);
  const placedCoins: RectDef[] = [];

  stage.coins = stage.coins.map((coin, index) => {
    let nextCoin = {
      x: Math.round(coin.x),
      y: Math.round(coin.y),
    };
    let coinRect = centeredRect(nextCoin.x, nextCoin.y, COIN_SIZE, COIN_SIZE);
    let guard = 0;

    while (
      [...occupiedRects, ...placedCoins.slice(0, index)].some((rect) =>
        rectsOverlap(coinRect, rect, PICKUP_PADDING),
      ) &&
      guard < 8
    ) {
      nextCoin = {
        x: nextCoin.x,
        y: nextCoin.y - (COIN_SIZE + 10),
      };
      coinRect = centeredRect(nextCoin.x, nextCoin.y, COIN_SIZE, COIN_SIZE);
      guard += 1;
    }

    placedCoins.push(coinRect);
    return nextCoin;
  });
}

function normalizeStageLayout(stage: StageDefinition): StageDefinition {
  const normalized = cloneStage(stage);

  normalizeTerrainSizing(normalized);
  normalizeReachableSurfaces(normalized);
  normalizeBlocks(normalized);
  normalizeCoins(normalized);

  return normalized;
}

function resolveSlopeRects(stage: StageDefinition): RectDef[] {
  const slopes = stage.slopes ?? [];

  return slopes.flatMap((slope) => {
    const stepCount = Math.max(2, Math.ceil(slope.width / 24));
    const stepWidth = slope.width / stepCount;

    return Array.from({ length: stepCount }, (_, index) => {
      const ratio =
        slope.direction === 'upRight'
          ? (index + 1) / stepCount
          : (stepCount - index) / stepCount;
      const topY = slope.y - slope.height * ratio;

      return normalizeRect({
        x: slope.x + index * stepWidth,
        y: topY,
        width: stepWidth,
        height: slope.y - topY,
      });
    });
  });
}

export function getStageCollisionRects(stage: StageDefinition): RectDef[] {
  return [...stage.terrain, ...resolveSlopeRects(stage)].map(normalizeRect);
}

function findSupportTop(stage: StageDefinition, x: number, sampleY: number): number | null {
  const supportRects = getStageCollisionRects(stage);
  let nearestTop: number | null = null;

  supportRects.forEach((rect) => {
    const insideX = x >= rect.x + 4 && x <= rect.x + rect.width - 4;
    const reachableY = rect.y >= sampleY - 12 && rect.y <= sampleY + 72;

    if (!insideX || !reachableY) {
      return;
    }

    if (nearestTop === null || rect.y < nearestTop) {
      nearestTop = rect.y;
    }
  });

  return nearestTop;
}

function enemyFootOffset(kind: EnemyDef['kind']): number {
  return kind === 'Flying' ? 0 : 30;
}

function thinEnemiesForEasy(enemies: EnemyDef[], difficulty: Difficulty): EnemyDef[] {
  if (difficulty !== 'Easy' || enemies.length <= 2) {
    return enemies;
  }

  return enemies.filter((_, index) => index % 3 !== 2);
}

function cloneEnemiesForHard(stage: StageDefinition, enemies: EnemyDef[], difficulty: Difficulty): EnemyDef[] {
  if (difficulty !== 'Hard') {
    return enemies;
  }

  const cloneSources = enemies.filter((enemy) => enemy.kind !== 'Flying').slice(0, 2);
  const clones: EnemyDef[] = [];

  cloneSources.forEach((source, sourceIndex) => {
    const preferredOffset = sourceIndex % 2 === 0 ? 96 : -96;
    const candidateOffsets = [
      preferredOffset,
      -preferredOffset,
      preferredOffset * 1.5,
      -preferredOffset * 1.5,
      0,
    ];

    for (const offset of candidateOffsets) {
      const candidateX = Math.max(64, Math.min(stage.width - 64, source.x + offset));
      const supportTop = findSupportTop(stage, candidateX, source.y + enemyFootOffset(source.kind));

      if (supportTop === null) {
        continue;
      }

      const deltaX = candidateX - source.x;
      clones.push({
        ...source,
        x: Math.round(candidateX),
        y: Math.round(supportTop - enemyFootOffset(source.kind)),
        patrolMin: source.patrolMin === undefined ? undefined : source.patrolMin + deltaX,
        patrolMax: source.patrolMax === undefined ? undefined : source.patrolMax + deltaX,
      });
      break;
    }
  });

  return [...enemies, ...clones];
}

export const STAGES: Record<(typeof STAGE_ORDER)[number], StageDefinition> = {
  '1-1': {
    id: '1-1',
    order: 1,
    timerSeconds: 300,
    width: 3200,
    height: 540,
    cameraBounds: {
      left: 0,
      top: 0,
      right: 3200,
      bottom: 540,
    },
    spawn: {
      x: 140,
      y: 430,
    },
    goal: {
      x: 3070,
      y: 418,
    },
    intro: 'Open grassland with safe jumps, mystery blocks, and early patrol routes.',
    terrain: [
      { x: 0, y: 500, width: 700, height: 48 },
      { x: 830, y: 500, width: 720, height: 48 },
      { x: 1690, y: 500, width: 760, height: 48 },
      { x: 2580, y: 500, width: 620, height: 48 },
      { x: 2268, y: 436, width: 96, height: 64 },
      { x: 520, y: 390, width: 180, height: 22 },
      { x: 980, y: 340, width: 190, height: 22 },
      { x: 1880, y: 360, width: 180, height: 22 },
      { x: 2260, y: 310, width: 160, height: 22 },
    ],
    slopes: [
      { x: 2140, y: 500, width: 128, height: 64, direction: 'upRight' },
      { x: 2364, y: 500, width: 128, height: 64, direction: 'upLeft' },
    ],
    blocks: [
      { x: 574, y: 340, reward: 'coin' },
      { x: 1026, y: 290, reward: 'mushroom' },
    ],
    coins: [
      ...coinsLine(210, 430, 5),
      ...coinsLine(860, 440, 4),
      ...coinsLine(1010, 290, 3),
      ...coinsLine(1870, 310, 4),
      ...coinsLine(2640, 430, 6),
    ],
    cactusHazards: [
      { x: 1460, y: 470 },
      { x: 2790, y: 470 },
    ],
    movingPlatforms: [],
    fallingBlocks: [],
    enemies: [
      { kind: 'Ground', x: 1110, y: 470, patrolMin: 890, patrolMax: 1500 },
      { kind: 'Ground', x: 1980, y: 470, patrolMin: 1720, patrolMax: 2410 },
      {
        kind: 'Flying',
        x: 2370,
        y: 250,
        patrolMin: 2240,
        patrolMax: 2480,
        amplitude: 26,
        frequency: 0.004,
        minDifficulty: 'Normal',
      },
    ],
  },
  '1-2': {
    id: '1-2',
    order: 2,
    timerSeconds: 280,
    width: 3600,
    height: 540,
    cameraBounds: {
      left: 0,
      top: 0,
      right: 3600,
      bottom: 540,
    },
    spawn: {
      x: 120,
      y: 430,
    },
    goal: {
      x: 3460,
      y: 418,
    },
    intro: 'Aerial traversals built around unsupported moving platforms.',
    terrain: [
      { x: 0, y: 500, width: 840, height: 48 },
      { x: 1700, y: 500, width: 560, height: 48 },
      { x: 2470, y: 500, width: 1130, height: 48 },
      { x: 2600, y: 355, width: 180, height: 22 },
      { x: 2960, y: 315, width: 160, height: 22 },
    ],
    slopes: [],
    blocks: [
      { x: 615, y: 360, reward: 'coin' },
      { x: 2730, y: 305, reward: 'flower' },
    ],
    coins: [
      ...coinsLine(300, 430, 5),
      ...coinsLine(955, 300, 4),
      ...coinsLine(1835, 300, 4),
      ...coinsLine(2570, 305, 4),
      ...coinsLine(3010, 265, 3),
      ...coinsLine(3290, 430, 4),
    ],
    cactusHazards: [
      { x: 2440, y: 470 },
    ],
    movingPlatforms: [
      { x: 1120, y: 360, width: 150, height: 20, minX: 930, maxX: 1510, speed: 92 },
      { x: 1410, y: 275, width: 150, height: 20, minX: 1180, maxX: 1620, speed: 116 },
      { x: 2280, y: 300, width: 150, height: 20, minX: 2060, maxX: 2380, speed: 102 },
    ],
    fallingBlocks: [],
    enemies: [
      { kind: 'Ground', x: 420, y: 470, patrolMin: 120, patrolMax: 780 },
      {
        kind: 'ProtectedHead',
        x: 1920,
        y: 470,
        patrolMin: 1740,
        patrolMax: 2230,
        minDifficulty: 'Normal',
      },
      {
        kind: 'Flying',
        x: 2770,
        y: 245,
        patrolMin: 2540,
        patrolMax: 3080,
        amplitude: 36,
        frequency: 0.0045,
      },
      {
        kind: 'Ground',
        x: 3220,
        y: 470,
        patrolMin: 2520,
        patrolMax: 3470,
        minDifficulty: 'Hard',
      },
    ],
  },
  '1-3': {
    id: '1-3',
    order: 3,
    timerSeconds: 260,
    width: 3800,
    height: 540,
    cameraBounds: {
      left: 0,
      top: 0,
      right: 3800,
      bottom: 540,
    },
    spawn: {
      x: 120,
      y: 430,
    },
    goal: {
      x: 3660,
      y: 418,
    },
    intro: 'Collapse hazards arrive in lines with real gaps and no hidden support.',
    terrain: [
      { x: 0, y: 500, width: 760, height: 48 },
      { x: 1550, y: 500, width: 760, height: 48 },
      { x: 2860, y: 500, width: 940, height: 48 },
      { x: 470, y: 360, width: 200, height: 22 },
      { x: 1790, y: 320, width: 180, height: 22 },
      { x: 3080, y: 350, width: 210, height: 22 },
    ],
    slopes: [],
    blocks: [
      { x: 534, y: 310, reward: 'mushroom' },
      { x: 1838, y: 270, reward: 'coin' },
    ],
    coins: [
      ...coinsLine(210, 430, 4),
      ...coinsLine(490, 305, 3),
      ...coinsLine(845, 280, 4),
      ...coinsLine(1670, 430, 4),
      ...coinsLine(1810, 265, 3),
      ...coinsLine(2400, 270, 5),
      ...coinsLine(3120, 300, 4),
      ...coinsLine(3400, 430, 4),
    ],
    cactusHazards: [
      { x: 1490, y: 470 },
      { x: 2980, y: 470 },
    ],
    movingPlatforms: [],
    fallingBlocks: [
      { x: 928, y: 340, width: 48, height: 24 },
      { x: 1048, y: 340, width: 48, height: 24 },
      { x: 1168, y: 340, width: 48, height: 24 },
      { x: 1288, y: 340, width: 48, height: 24 },
      { x: 2480, y: 300, width: 48, height: 24 },
      { x: 2600, y: 300, width: 48, height: 24 },
      { x: 2720, y: 300, width: 48, height: 24 },
      { x: 2840, y: 300, width: 48, height: 24 },
    ],
    enemies: [
      { kind: 'Ground', x: 360, y: 470, patrolMin: 120, patrolMax: 720 },
      {
        kind: 'Armored',
        x: 1940,
        y: 470,
        patrolMin: 1600,
        patrolMax: 2270,
      },
      {
        kind: 'Flying',
        x: 3280,
        y: 240,
        patrolMin: 3050,
        patrolMax: 3500,
        amplitude: 30,
        frequency: 0.005,
        minDifficulty: 'Normal',
      },
      {
        kind: 'Shooter',
        x: 3440,
        y: 470,
        fireDelayMs: 2200,
        minDifficulty: 'Hard',
      },
    ],
  },
  '1-4': {
    id: '1-4',
    order: 4,
    timerSeconds: 240,
    width: 4200,
    height: 540,
    cameraBounds: {
      left: 0,
      top: 0,
      right: 4200,
      bottom: 540,
    },
    spawn: {
      x: 120,
      y: 430,
    },
    goal: {
      x: 4040,
      y: 418,
    },
    intro: 'Final gauntlet mixing projectiles, protected enemies, and tight timing.',
    terrain: [
      { x: 0, y: 500, width: 860, height: 48 },
      { x: 990, y: 500, width: 620, height: 48 },
      { x: 1720, y: 500, width: 720, height: 48 },
      { x: 2620, y: 500, width: 640, height: 48 },
      { x: 3400, y: 500, width: 800, height: 48 },
      { x: 550, y: 360, width: 190, height: 22 },
      { x: 1320, y: 330, width: 180, height: 22 },
      { x: 2140, y: 300, width: 180, height: 22 },
      { x: 2870, y: 340, width: 170, height: 22 },
      { x: 3540, y: 300, width: 220, height: 22 },
    ],
    slopes: [],
    blocks: [
      { x: 612, y: 310, reward: 'coin' },
      { x: 1366, y: 280, reward: 'flower' },
      { x: 3590, y: 250, reward: 'mushroom' },
    ],
    coins: [
      ...coinsLine(190, 430, 4),
      ...coinsLine(572, 305, 3),
      ...coinsLine(1170, 430, 3),
      ...coinsLine(1334, 280, 3),
      ...coinsLine(1860, 430, 4),
      ...coinsLine(2150, 250, 3),
      ...coinsLine(2760, 430, 3),
      ...coinsLine(2890, 290, 3),
      ...coinsLine(3450, 430, 4),
      ...coinsLine(3580, 250, 4),
    ],
    cactusHazards: [
      { x: 1640, y: 470 },
      { x: 3340, y: 470 },
    ],
    movingPlatforms: [
      { x: 918, y: 290, width: 150, height: 20, minX: 860, maxX: 1100, speed: 88 },
      { x: 3230, y: 260, width: 150, height: 20, minX: 3180, maxX: 3380, speed: 112 },
    ],
    fallingBlocks: [
      { x: 1600, y: 330, width: 48, height: 24 },
      { x: 1720, y: 330, width: 48, height: 24 },
      { x: 1840, y: 330, width: 48, height: 24 },
      { x: 1960, y: 330, width: 48, height: 24 },
    ],
    enemies: [
      { kind: 'Ground', x: 360, y: 470, patrolMin: 120, patrolMax: 820 },
      { kind: 'ProtectedHead', x: 1220, y: 470, patrolMin: 1040, patrolMax: 1580 },
      { kind: 'Armored', x: 2000, y: 470, patrolMin: 1760, patrolMax: 2400 },
      { kind: 'Shooter', x: 2980, y: 470, fireDelayMs: 1900 },
      {
        kind: 'Flying',
        x: 3640,
        y: 230,
        patrolMin: 3470,
        patrolMax: 3890,
        amplitude: 34,
        frequency: 0.0052,
      },
      {
        kind: 'Shooter',
        x: 3870,
        y: 470,
        fireDelayMs: 1600,
        minDifficulty: 'Hard',
      },
    ],
  },
};

export function getStageDefinition(stageId: string): StageDefinition {
  const stage = STAGES[stageId as (typeof STAGE_ORDER)[number]];

  if (!stage) {
    throw new Error(`Unknown stage id: ${stageId}`);
  }

  return normalizeStageLayout(stage);
}

export function getSpawnableEnemies(stage: StageDefinition, difficulty: Difficulty) {
  const authoredEnemies = stage.enemies.filter((enemy) =>
    shouldSpawnAtDifficulty(difficulty, enemy.minDifficulty),
  );
  const thinnedEnemies = thinEnemiesForEasy(authoredEnemies, difficulty);

  return cloneEnemiesForHard(stage, thinnedEnemies, difficulty);
}
