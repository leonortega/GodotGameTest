import {
  EXTRA_LIFE_COIN_THRESHOLD,
  INITIAL_LIVES,
  MAX_VISIBLE_LIVES,
  SCORE_VALUES,
  type PlayerForm,
  type StageId,
} from "./Constants";

export type RunState = {
  lives: number;
  score: number;
  coins: number;
  stageId: StageId;
  form: PlayerForm;
};

class GameStateStore {
  private selectedStartStage: StageId = "1-1";

  private runState: RunState = {
    lives: INITIAL_LIVES,
    score: 0,
    coins: 0,
    stageId: this.selectedStartStage,
    form: "Small",
  };

  resetRun(stageId = this.selectedStartStage): void {
    this.selectedStartStage = stageId;
    this.runState = {
      lives: INITIAL_LIVES,
      score: 0,
      coins: 0,
      stageId,
      form: "Small",
    };
  }

  getState(): RunState {
    return { ...this.runState };
  }

  setStage(stageId: StageId): void {
    this.runState.stageId = stageId;
  }

  getSelectedStartStage(): StageId {
    return this.selectedStartStage;
  }

  setSelectedStartStage(stageId: StageId): void {
    this.selectedStartStage = stageId;
  }

  setForm(form: PlayerForm): void {
    this.runState.form = form;
  }

  addScore(amount: number): void {
    this.runState.score += amount;
  }

  addCoins(amount: number): boolean {
    this.runState.coins += amount;
    this.addScore(SCORE_VALUES.coin * amount);

    let gainedLife = false;
    while (this.runState.coins >= EXTRA_LIFE_COIN_THRESHOLD) {
      this.runState.coins -= EXTRA_LIFE_COIN_THRESHOLD;
      this.addLife();
      gainedLife = true;
    }

    return gainedLife;
  }

  addLife(amount = 1): void {
    this.runState.lives = Math.min(MAX_VISIBLE_LIVES, this.runState.lives + amount);
  }

  loseLife(): number {
    this.runState.lives = Math.max(0, this.runState.lives - 1);
    this.runState.form = "Small";
    return this.runState.lives;
  }

  applyTimeBonus(secondsRemaining: number): number {
    const awarded = Math.max(0, Math.floor(secondsRemaining)) * SCORE_VALUES.timeBonusPerSecond;
    this.addScore(awarded);
    return awarded;
  }
}

const GameState = new GameStateStore();

export default GameState;
