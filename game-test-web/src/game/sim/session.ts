import { formatSaveData, loadSaveData, persistSaveData } from '../core/save';
import {
  DIFFICULTIES,
  type Difficulty,
  type PlayerForm,
  type StageClearResult,
} from '../core/types';
import { STAGE_ORDER } from './stages';

function stageRank(stageId: string | null): number {
  if (!stageId) {
    return -1;
  }

  const [worldPart, stagePart] = stageId.split('-');
  const world = Number.parseInt(worldPart, 10);
  const stage = Number.parseInt(stagePart, 10);

  if (!Number.isFinite(world) || !Number.isFinite(stage)) {
    return -1;
  }

  return world * 100 + stage;
}

function clampDb(value: number): number {
  return Math.max(-24, Math.min(0, value));
}

export class GameSession {
  readonly saveData = loadSaveData();

  lives = 3;
  score = 0;
  coins = 0;
  form: PlayerForm = 'Small';
  currentStageId: string = STAGE_ORDER[0];
  runActive = false;

  get difficulty(): Difficulty {
    return this.saveData.slot.settings.difficulty;
  }

  startNewRun(): void {
    this.lives = 3;
    this.score = 0;
    this.coins = 0;
    this.form = 'Small';
    this.currentStageId = STAGE_ORDER[0];
    this.runActive = true;
  }

  endRun(): void {
    this.runActive = false;
  }

  setCurrentStage(stageId: string): void {
    this.currentStageId = stageId;
  }

  getNextStageId(stageId = this.currentStageId): string | null {
    const currentIndex = STAGE_ORDER.indexOf(stageId as (typeof STAGE_ORDER)[number]);
    const nextStage = STAGE_ORDER[currentIndex + 1];

    return nextStage ?? null;
  }

  advanceStage(): string | null {
    const nextStageId = this.getNextStageId();

    if (nextStageId) {
      this.currentStageId = nextStageId;
    }

    return nextStageId;
  }

  addScore(points: number): void {
    this.score += Math.max(0, Math.floor(points));
  }

  collectCoins(amount: number): number {
    const normalizedAmount = Math.max(0, Math.floor(amount));
    const previousThreshold = Math.floor(this.coins / 100);

    this.coins += normalizedAmount;
    this.score += normalizedAmount * 100;

    const nextThreshold = Math.floor(this.coins / 100);
    const awardedLives = Math.max(0, nextThreshold - previousThreshold);

    if (awardedLives > 0) {
      this.lives += awardedLives;
    }

    return awardedLives;
  }

  applyReward(reward: 'coin' | 'mushroom' | 'flower'): PlayerForm {
    if (reward === 'coin') {
      this.collectCoins(1);
      return this.form;
    }

    if (reward === 'mushroom') {
      if (this.form === 'Small') {
        this.form = 'Powered';
      }

      return this.form;
    }

    if (this.form === 'Small') {
      this.form = 'Powered';
    } else {
      this.form = 'Enhanced';
    }

    return this.form;
  }

  takeDamage(): 'downgraded' | 'life_lost' {
    if (this.form === 'Enhanced') {
      this.form = 'Powered';
      return 'downgraded';
    }

    if (this.form === 'Powered') {
      this.form = 'Small';
      return 'downgraded';
    }

    this.lives = Math.max(0, this.lives - 1);
    this.form = 'Small';
    return 'life_lost';
  }

  loseLife(): boolean {
    this.lives = Math.max(0, this.lives - 1);
    this.form = 'Small';

    return this.lives > 0;
  }

  resolveStageClear(timeRemaining: number): StageClearResult {
    const bonus = Math.max(0, Math.floor(timeRemaining)) * 10;
    const nextStageId = this.getNextStageId();

    this.score += bonus;
    this.recordStageClear(this.currentStageId);
    this.persist();

    return {
      bonus,
      nextStageId,
      finalStage: nextStageId === null,
    };
  }

  cycleDifficulty(): Difficulty {
    const currentIndex = DIFFICULTIES.indexOf(this.difficulty);
    const nextDifficulty = DIFFICULTIES[(currentIndex + 1) % DIFFICULTIES.length];

    this.saveData.slot.settings.difficulty = nextDifficulty;
    this.persist();

    return nextDifficulty;
  }

  adjustMusicVolume(deltaDb: number): number {
    const nextValue = clampDb(this.saveData.slot.settings.musicVolumeDb + deltaDb);
    this.saveData.slot.settings.musicVolumeDb = nextValue;
    this.persist();
    return nextValue;
  }

  adjustSfxVolume(deltaDb: number): number {
    const nextValue = clampDb(this.saveData.slot.settings.sfxVolumeDb + deltaDb);
    this.saveData.slot.settings.sfxVolumeDb = nextValue;
    this.persist();
    return nextValue;
  }

  toSavePreview(): string {
    return formatSaveData(this.saveData);
  }

  private recordStageClear(stageId: string): void {
    if (stageRank(stageId) > stageRank(this.saveData.slot.highestClearedStage)) {
      this.saveData.slot.highestClearedStage = stageId;
    }

    if (this.score > this.saveData.slot.bestScore) {
      this.saveData.slot.bestScore = this.score;
    }
  }

  private persist(): void {
    persistSaveData(this.saveData);
  }
}
