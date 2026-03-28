export const DIFFICULTIES = ['Easy', 'Normal', 'Hard'] as const;
export const PLAYER_FORMS = ['Small', 'Powered', 'Enhanced'] as const;

export type Difficulty = (typeof DIFFICULTIES)[number];
export type PlayerForm = (typeof PLAYER_FORMS)[number];
export type EnemyKind =
  | 'Ground'
  | 'Armored'
  | 'Flying'
  | 'ProtectedHead'
  | 'Shooter';
export type RewardKind = 'coin' | 'mushroom' | 'flower';

export interface SaveData {
  slot: {
    highestClearedStage: string | null;
    bestScore: number;
    settings: {
      musicVolumeDb: number;
      sfxVolumeDb: number;
      difficulty: Difficulty;
    };
  };
}

export interface HudState {
  stageId: string;
  score: number;
  coins: number;
  lives: number;
  timeRemaining: number;
  difficulty: Difficulty;
  paused: boolean;
}

export interface StageSummary {
  stageId: string;
  score: number;
  coins: number;
  lives: number;
  timeRemaining: number;
  form: PlayerForm;
  bonus: number;
  difficulty: Difficulty;
}

export interface RectDef {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface SlopeSegmentDef {
  x: number;
  y: number;
  width: number;
  height: number;
  direction: 'upRight' | 'upLeft';
}

export interface MovingPlatformDef extends RectDef {
  minX: number;
  maxX: number;
  speed: number;
}

export interface FallingBlockDef extends RectDef {}

export interface BlockDef {
  x: number;
  y: number;
  reward: RewardKind;
}

export interface CoinDef {
  x: number;
  y: number;
}

export interface CactusHazardDef {
  x: number;
  y: number;
}

export interface EnemyDef {
  kind: EnemyKind;
  x: number;
  y: number;
  patrolMin?: number;
  patrolMax?: number;
  amplitude?: number;
  frequency?: number;
  fireDelayMs?: number;
  minDifficulty?: Difficulty;
}

export interface StageDefinition {
  id: string;
  order: number;
  timerSeconds: number;
  width: number;
  height: number;
  cameraBounds: {
    left: number;
    top: number;
    right: number;
    bottom: number;
  };
  spawn: {
    x: number;
    y: number;
  };
  goal: {
    x: number;
    y: number;
  };
  intro: string;
  terrain: RectDef[];
  slopes?: SlopeSegmentDef[];
  blocks: BlockDef[];
  coins: CoinDef[];
  cactusHazards: CactusHazardDef[];
  movingPlatforms: MovingPlatformDef[];
  fallingBlocks: FallingBlockDef[];
  enemies: EnemyDef[];
}

export interface StageClearResult {
  bonus: number;
  nextStageId: string | null;
  finalStage: boolean;
}
