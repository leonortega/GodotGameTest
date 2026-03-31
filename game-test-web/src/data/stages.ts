import type { PickupType, StageId, ThemeId } from "../core/Constants";

export type RectSegment = {
  x: number;
  y: number;
  width: number;
  height: number;
};

export type BlockConfig = {
  x: number;
  y: number;
  type: "question" | "brick" | "hidden";
  content?: PickupType | "Coin";
  multiCoinHits?: number;
  breakable?: boolean;
};

export type CoinConfig = {
  x: number;
  y: number;
};

export type EnemyConfig = {
  x: number;
  y: number;
  variant: "goomba" | "beetle";
};

export type MovingPlatformConfig = {
  x: number;
  y: number;
  width: number;
  left: number;
  right: number;
  speed: number;
};

export type FallingBlockConfig = {
  x: number;
  y: number;
};

export type StageConfig = {
  id: StageId;
  name: string;
  theme: ThemeId;
  timer: number;
  widthTiles: number;
  playerStartX: number;
  ground: RectSegment[];
  ceilings?: RectSegment[];
  blocks: BlockConfig[];
  coins: CoinConfig[];
  enemies: EnemyConfig[];
  movingPlatforms?: MovingPlatformConfig[];
  fallingBlocks?: FallingBlockConfig[];
  goalX: number;
};

const MAX_BLOCK_CLEARANCE_TILES = 4;
const MAX_COIN_CLEARANCE_TILES = 4;
const MAX_PLATFORM_CLEARANCE_TILES = 3;
const MAX_FALLING_BLOCK_CLEARANCE_TILES = 3;

function overlapsHorizontally(x: number, width: number, segment: RectSegment): boolean {
  const start = x;
  const end = x + width;
  return end > segment.x && start < segment.x + segment.width;
}

function findNearestSupportTop(stage: StageConfig, x: number, width: number, objectY: number): number | undefined {
  return stage.ground
    .filter((segment) => segment.y > objectY && overlapsHorizontally(x, width, segment))
    .reduce<number | undefined>((nearest, segment) => {
      if (nearest === undefined) {
        return segment.y;
      }

      return Math.min(nearest, segment.y);
    }, undefined);
}

function clampHeightToSupport(stage: StageConfig, x: number, width: number, y: number, maxClearanceTiles: number): number {
  const supportTop = findNearestSupportTop(stage, x, width, y);
  if (supportTop === undefined) {
    return y;
  }

  return Math.max(y, supportTop - maxClearanceTiles);
}

function normalizeStageLayout(stage: StageConfig): StageConfig {
  return {
    ...stage,
    blocks: stage.blocks.map((block) => ({
      ...block,
      y: clampHeightToSupport(stage, block.x, 1, block.y, MAX_BLOCK_CLEARANCE_TILES),
    })),
    coins: stage.coins.map((coin) => ({
      ...coin,
      y: clampHeightToSupport(stage, coin.x, 1, coin.y, MAX_COIN_CLEARANCE_TILES),
    })),
    movingPlatforms: stage.movingPlatforms?.map((platform) => ({
      ...platform,
      y: clampHeightToSupport(stage, platform.left, platform.right - platform.left + platform.width, platform.y, MAX_PLATFORM_CLEARANCE_TILES),
    })),
    fallingBlocks: stage.fallingBlocks?.map((block) => ({
      ...block,
      y: clampHeightToSupport(stage, block.x, 1, block.y, MAX_FALLING_BLOCK_CLEARANCE_TILES),
    })),
  };
}

export const STAGES: Record<StageId, StageConfig> = {
  "1-1": {
    id: "1-1",
    name: "Sunlit Plains",
    theme: "overworld",
    timer: 400,
    widthTiles: 150,
    playerStartX: 96,
    ground: [
      { x: 0, y: 15, width: 18, height: 2 },
      { x: 19, y: 15, width: 11, height: 2 },
      { x: 33, y: 15, width: 15, height: 2 },
      { x: 51, y: 15, width: 8, height: 2 },
      { x: 61, y: 15, width: 14, height: 2 },
      { x: 78, y: 14, width: 8, height: 3 },
      { x: 88, y: 15, width: 10, height: 2 },
      { x: 101, y: 15, width: 18, height: 2 },
      { x: 121, y: 14, width: 7, height: 3 },
      { x: 130, y: 15, width: 20, height: 2 },
    ],
    blocks: [
      { x: 8, y: 10, type: "question", content: "Mushroom" },
      { x: 9, y: 10, type: "brick", breakable: true },
      { x: 10, y: 10, type: "question", content: "Coin", multiCoinHits: 4 },
      { x: 27, y: 9, type: "question", content: "Coin" },
      { x: 39, y: 10, type: "brick", breakable: true },
      { x: 40, y: 10, type: "question", content: "Coin" },
      { x: 41, y: 10, type: "brick", breakable: true },
      { x: 68, y: 9, type: "question", content: "Fire Flower" },
      { x: 69, y: 9, type: "brick", breakable: true },
      { x: 70, y: 9, type: "brick", breakable: true },
      { x: 109, y: 8, type: "hidden", content: "1-Up Mushroom" },
      { x: 123, y: 9, type: "question", content: "Coin", multiCoinHits: 5 },
    ],
    coins: [
      { x: 12, y: 9 }, { x: 13, y: 9 }, { x: 14, y: 9 }, { x: 22, y: 10 }, { x: 23, y: 9 },
      { x: 24, y: 10 }, { x: 36, y: 8 }, { x: 37, y: 8 }, { x: 38, y: 8 }, { x: 55, y: 10 },
      { x: 56, y: 10 }, { x: 57, y: 10 }, { x: 82, y: 8 }, { x: 83, y: 7 }, { x: 84, y: 8 },
      { x: 94, y: 10 }, { x: 95, y: 10 }, { x: 96, y: 10 }, { x: 124, y: 8 }, { x: 125, y: 8 },
    ],
    enemies: [
      { x: 19, y: 14, variant: "goomba" },
      { x: 34, y: 14, variant: "goomba" },
      { x: 48, y: 14, variant: "beetle" },
      { x: 72, y: 14, variant: "goomba" },
      { x: 106, y: 14, variant: "goomba" },
      { x: 118, y: 14, variant: "beetle" },
    ],
    goalX: 144,
  },
  "1-2": {
    id: "1-2",
    name: "Low Ceiling Run",
    theme: "underground",
    timer: 400,
    widthTiles: 148,
    playerStartX: 96,
    ground: [
      { x: 0, y: 15, width: 24, height: 2 },
      { x: 27, y: 15, width: 14, height: 2 },
      { x: 44, y: 14, width: 10, height: 3 },
      { x: 57, y: 15, width: 17, height: 2 },
      { x: 77, y: 15, width: 12, height: 2 },
      { x: 92, y: 14, width: 10, height: 3 },
      { x: 105, y: 15, width: 18, height: 2 },
      { x: 126, y: 15, width: 22, height: 2 },
    ],
    ceilings: [
      { x: 5, y: 3, width: 30, height: 1 },
      { x: 43, y: 4, width: 18, height: 1 },
      { x: 80, y: 3, width: 26, height: 1 },
    ],
    blocks: [
      { x: 11, y: 10, type: "question", content: "Coin" },
      { x: 12, y: 10, type: "question", content: "Coin", multiCoinHits: 6 },
      { x: 13, y: 10, type: "brick", breakable: true },
      { x: 46, y: 9, type: "hidden", content: "Super Star" },
      { x: 64, y: 10, type: "question", content: "Mushroom" },
      { x: 65, y: 10, type: "brick", breakable: true },
      { x: 66, y: 10, type: "brick", breakable: true },
      { x: 95, y: 8, type: "question", content: "Coin" },
      { x: 96, y: 8, type: "question", content: "Fire Flower" },
      { x: 111, y: 9, type: "brick", breakable: true },
      { x: 112, y: 9, type: "brick", breakable: true },
      { x: 113, y: 9, type: "hidden", content: "1-Up Mushroom" },
    ],
    coins: [
      { x: 15, y: 11 }, { x: 16, y: 11 }, { x: 17, y: 11 }, { x: 33, y: 10 }, { x: 34, y: 10 },
      { x: 35, y: 10 }, { x: 50, y: 8 }, { x: 51, y: 8 }, { x: 52, y: 8 }, { x: 69, y: 10 },
      { x: 70, y: 10 }, { x: 71, y: 10 }, { x: 98, y: 7 }, { x: 99, y: 7 }, { x: 100, y: 7 },
      { x: 130, y: 10 }, { x: 131, y: 10 }, { x: 132, y: 10 },
    ],
    enemies: [
      { x: 21, y: 14, variant: "goomba" },
      { x: 42, y: 14, variant: "beetle" },
      { x: 59, y: 14, variant: "goomba" },
      { x: 74, y: 14, variant: "goomba" },
      { x: 91, y: 13, variant: "beetle" },
      { x: 123, y: 14, variant: "goomba" },
    ],
    fallingBlocks: [
      { x: 24, y: 12 },
      { x: 25, y: 12 },
      { x: 26, y: 12 },
    ],
    goalX: 142,
  },
  "1-3": {
    id: "1-3",
    name: "Sky Walk",
    theme: "athletic_sky",
    timer: 400,
    widthTiles: 140,
    playerStartX: 96,
    ground: [
      { x: 0, y: 15, width: 7, height: 2 },
      { x: 9, y: 13, width: 5, height: 1 },
      { x: 16, y: 11, width: 6, height: 1 },
      { x: 25, y: 13, width: 5, height: 1 },
      { x: 32, y: 10, width: 8, height: 1 },
      { x: 43, y: 12, width: 5, height: 1 },
      { x: 51, y: 9, width: 7, height: 1 },
      { x: 61, y: 13, width: 6, height: 1 },
      { x: 70, y: 11, width: 8, height: 1 },
      { x: 81, y: 13, width: 6, height: 1 },
      { x: 90, y: 10, width: 8, height: 1 },
      { x: 101, y: 12, width: 6, height: 1 },
      { x: 110, y: 9, width: 7, height: 1 },
      { x: 120, y: 13, width: 8, height: 1 },
      { x: 130, y: 15, width: 10, height: 2 },
    ],
    blocks: [
      { x: 18, y: 8, type: "question", content: "Coin" },
      { x: 36, y: 7, type: "question", content: "Mushroom" },
      { x: 55, y: 6, type: "question", content: "Fire Flower" },
      { x: 75, y: 8, type: "brick", breakable: true },
      { x: 76, y: 8, type: "brick", breakable: true },
      { x: 94, y: 7, type: "question", content: "Coin", multiCoinHits: 4 },
      { x: 115, y: 6, type: "hidden", content: "Super Star" },
    ],
    coins: [
      { x: 10, y: 11 }, { x: 11, y: 10 }, { x: 12, y: 11 }, { x: 27, y: 10 }, { x: 28, y: 9 },
      { x: 29, y: 10 }, { x: 45, y: 9 }, { x: 46, y: 8 }, { x: 47, y: 9 }, { x: 63, y: 10 },
      { x: 64, y: 9 }, { x: 65, y: 10 }, { x: 83, y: 10 }, { x: 84, y: 9 }, { x: 85, y: 10 },
      { x: 102, y: 9 }, { x: 103, y: 8 }, { x: 104, y: 9 }, { x: 123, y: 10 }, { x: 124, y: 10 },
    ],
    enemies: [
      { x: 17, y: 10, variant: "goomba" },
      { x: 38, y: 9, variant: "goomba" },
      { x: 62, y: 12, variant: "beetle" },
      { x: 92, y: 9, variant: "goomba" },
      { x: 133, y: 14, variant: "goomba" },
    ],
    movingPlatforms: [
      { x: 8, y: 12, width: 2, left: 7, right: 14, speed: 52 },
      { x: 106, y: 10, width: 2, left: 104, right: 116, speed: 58 },
    ],
    goalX: 136,
  },
  "1-4": {
    id: "1-4",
    name: "Coal Keep",
    theme: "castle",
    timer: 400,
    widthTiles: 150,
    playerStartX: 96,
    ground: [
      { x: 0, y: 15, width: 16, height: 2 },
      { x: 18, y: 15, width: 12, height: 2 },
      { x: 33, y: 14, width: 12, height: 3 },
      { x: 48, y: 15, width: 10, height: 2 },
      { x: 60, y: 13, width: 12, height: 4 },
      { x: 75, y: 15, width: 10, height: 2 },
      { x: 88, y: 14, width: 11, height: 3 },
      { x: 102, y: 15, width: 12, height: 2 },
      { x: 117, y: 13, width: 11, height: 4 },
      { x: 130, y: 15, width: 20, height: 2 },
    ],
    ceilings: [
      { x: 0, y: 4, width: 40, height: 1 },
      { x: 46, y: 4, width: 34, height: 1 },
      { x: 88, y: 4, width: 30, height: 1 },
    ],
    blocks: [
      { x: 14, y: 10, type: "brick", breakable: true },
      { x: 21, y: 10, type: "question", content: "Coin" },
      { x: 39, y: 9, type: "question", content: "Mushroom" },
      { x: 64, y: 8, type: "question", content: "Super Star" },
      { x: 93, y: 9, type: "brick", breakable: true },
      { x: 94, y: 9, type: "brick", breakable: true },
      { x: 121, y: 8, type: "question", content: "Fire Flower" },
    ],
    coins: [
      { x: 23, y: 10 }, { x: 24, y: 10 }, { x: 25, y: 10 }, { x: 41, y: 8 }, { x: 42, y: 8 },
      { x: 43, y: 8 }, { x: 67, y: 7 }, { x: 68, y: 7 }, { x: 69, y: 7 }, { x: 96, y: 8 },
      { x: 97, y: 8 }, { x: 98, y: 8 }, { x: 123, y: 7 }, { x: 124, y: 7 },
    ],
    enemies: [
      { x: 17, y: 14, variant: "beetle" },
      { x: 35, y: 13, variant: "goomba" },
      { x: 57, y: 14, variant: "beetle" },
      { x: 83, y: 14, variant: "goomba" },
      { x: 108, y: 14, variant: "beetle" },
      { x: 129, y: 14, variant: "goomba" },
    ],
    movingPlatforms: [
      { x: 72, y: 12, width: 2, left: 70, right: 84, speed: 60 },
    ],
    goalX: 145,
  },
};

export function getStage(stageId: StageId): StageConfig {
  return normalizeStageLayout(STAGES[stageId]);
}
