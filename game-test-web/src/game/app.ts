import Phaser from 'phaser';
import { RetroAudio } from './core/audio';
import { ENEMY_VISUALS, PLAYER_VISUALS } from './core/assets';
import { SHELL_SHORTCUTS } from './core/input';
import type { EnemyKind, HudState, StageSummary } from './core/types';
import { getStageDefinition } from './sim/stages';
import { GameSession } from './sim/session';
import { PlayScene } from './scenes/PlayScene';

type OverlayState =
  | 'title'
  | 'modal'
  | 'transition'
  | 'pause'
  | 'summary'
  | 'gameover'
  | 'worldclear'
  | 'none';
type ModalState = 'controls' | 'enemies' | 'config' | null;

function formatDb(value: number): string {
  return `${value >= 0 ? '+' : ''}${value} dB`;
}

function getEnemyGuideItems(): Array<{
  kind: EnemyKind;
  name: string;
  note: string;
  portrait: string;
}> {
  return [
    {
      kind: 'Ground',
      name: 'Ground Slime',
      note: 'Walks left and right, turning at walls and edges. Safe to stomp from above.',
      portrait: PLAYER_VISUALS.zombie.idle,
    },
    {
      kind: 'Armored',
      name: 'Armored Shell',
      note: 'Slow ground patrol that rejects standard stomps and reflects player shots.',
      portrait: ENEMY_VISUALS.Armored.path,
    },
    {
      kind: 'ProtectedHead',
      name: 'Spike Slime',
      note: 'Fast patrol with an unsafe top surface. Approach from the side or use projectiles.',
      portrait: ENEMY_VISUALS.ProtectedHead.path,
    },
    {
      kind: 'Flying',
      name: 'Flying Slime',
      note: 'Loops through the air instead of respecting ground gaps, so read its vertical motion.',
      portrait: ENEMY_VISUALS.Flying.path,
    },
    {
      kind: 'Shooter',
      name: 'Barnacle Shooter',
      note: 'Holds position and fires hostile shots on a readable cadence that controls space.',
      portrait: ENEMY_VISUALS.Shooter.path,
    },
  ];
}

export class BrowserGameApp {
  readonly session = new GameSession();
  readonly audio = new RetroAudio(this.session.saveData);

  private readonly scene: PlayScene;
  private readonly game: Phaser.Game;
  private readonly root: HTMLElement;

  private overlayState: OverlayState = 'title';
  private modalState: ModalState = null;
  private pendingTimer: number | null = null;
  private paused = false;

  private readonly gameHost: HTMLDivElement;
  private readonly hudBar: HTMLDivElement;
  private readonly hudScore: HTMLSpanElement;
  private readonly hudCoins: HTMLSpanElement;
  private readonly hudStage: HTMLSpanElement;
  private readonly hudLives: HTMLSpanElement;
  private readonly hudTime: HTMLSpanElement;
  private readonly hudDifficulty: HTMLSpanElement;

  private readonly titleScreen: HTMLDivElement;
  private readonly titleDifficultyValue: HTMLSpanElement;
  private readonly titleProgress: HTMLParagraphElement;
  private readonly titleStartButton: HTMLButtonElement;
  private readonly titleControlsButton: HTMLButtonElement;
  private readonly titleEnemiesButton: HTMLButtonElement;
  private readonly titleConfigButton: HTMLButtonElement;
  private readonly titleDifficultyButton: HTMLButtonElement;

  private readonly modalScreen: HTMLDivElement;
  private readonly modalTitle: HTMLHeadingElement;
  private readonly modalBody: HTMLDivElement;
  private readonly modalBackButton: HTMLButtonElement;

  private readonly transitionScreen: HTMLDivElement;
  private readonly transitionStage: HTMLHeadingElement;
  private readonly transitionMeta: HTMLParagraphElement;
  private readonly transitionIntro: HTMLParagraphElement;

  private readonly summaryScreen: HTMLDivElement;
  private readonly summaryStage: HTMLHeadingElement;
  private readonly summaryBody: HTMLDivElement;

  private readonly pauseScreen: HTMLDivElement;
  private readonly pauseResumeButton: HTMLButtonElement;
  private readonly pauseTitleButton: HTMLButtonElement;

  private readonly gameOverScreen: HTMLDivElement;
  private readonly gameOverBody: HTMLDivElement;
  private readonly gameOverRestartButton: HTMLButtonElement;
  private readonly gameOverTitleButton: HTMLButtonElement;

  private readonly worldClearScreen: HTMLDivElement;
  private readonly worldClearBody: HTMLDivElement;
  private readonly worldClearReplayButton: HTMLButtonElement;
  private readonly worldClearTitleButton: HTMLButtonElement;

  constructor(root: HTMLElement) {
    this.root = root;
    this.root.innerHTML = this.buildMarkup();

    this.gameHost = this.query('#game-host');
    this.hudBar = this.query('#hud-bar');
    this.hudScore = this.query('#hud-score');
    this.hudCoins = this.query('#hud-coins');
    this.hudStage = this.query('#hud-stage');
    this.hudLives = this.query('#hud-lives');
    this.hudTime = this.query('#hud-time');
    this.hudDifficulty = this.query('#hud-difficulty');

    this.titleScreen = this.query('#title-screen');
    this.titleDifficultyValue = this.query('#title-difficulty-value');
    this.titleProgress = this.query('#title-progress');
    this.titleStartButton = this.query('#title-start');
    this.titleControlsButton = this.query('#title-controls');
    this.titleEnemiesButton = this.query('#title-enemies');
    this.titleConfigButton = this.query('#title-config');
    this.titleDifficultyButton = this.query('#title-difficulty');

    this.modalScreen = this.query('#modal-screen');
    this.modalTitle = this.query('#modal-title');
    this.modalBody = this.query('#modal-body');
    this.modalBackButton = this.query('#modal-back');

    this.transitionScreen = this.query('#transition-screen');
    this.transitionStage = this.query('#transition-stage');
    this.transitionMeta = this.query('#transition-meta');
    this.transitionIntro = this.query('#transition-intro');

    this.summaryScreen = this.query('#summary-screen');
    this.summaryStage = this.query('#summary-stage');
    this.summaryBody = this.query('#summary-body');

    this.pauseScreen = this.query('#pause-screen');
    this.pauseResumeButton = this.query('#pause-resume');
    this.pauseTitleButton = this.query('#pause-title');

    this.gameOverScreen = this.query('#gameover-screen');
    this.gameOverBody = this.query('#gameover-body');
    this.gameOverRestartButton = this.query('#gameover-restart');
    this.gameOverTitleButton = this.query('#gameover-title');

    this.worldClearScreen = this.query('#worldclear-screen');
    this.worldClearBody = this.query('#worldclear-body');
    this.worldClearReplayButton = this.query('#worldclear-replay');
    this.worldClearTitleButton = this.query('#worldclear-title');

    this.bindUi();
    this.scene = new PlayScene(this);

    this.game = new Phaser.Game({
      type: Phaser.AUTO,
      parent: this.gameHost,
      backgroundColor: '#12141f',
      scale: {
        mode: Phaser.Scale.FIT,
        autoCenter: Phaser.Scale.CENTER_BOTH,
        width: 960,
        height: 540,
      },
      physics: {
        default: 'arcade',
        arcade: {
          gravity: {
            x: 0,
            y: 1180,
          },
          debug: false,
        },
      },
      pixelArt: true,
      scene: [this.scene],
    });

    this.updateTitleState();
    this.showTitle();
  }

  updateHud(hudState: HudState): void {
    this.hudScore.textContent = hudState.score.toString().padStart(6, '0');
    this.hudCoins.textContent = hudState.coins.toString().padStart(2, '0');
    this.hudStage.textContent = hudState.stageId;
    this.hudLives.textContent = hudState.lives.toString();
    this.hudTime.textContent = hudState.timeRemaining.toString().padStart(3, '0');
    this.hudDifficulty.textContent = hudState.difficulty;
  }

  onStageLifeLost(): void {
    this.paused = false;

    if (this.session.lives > 0) {
      this.queueStageTransition(this.session.currentStageId);
      return;
    }

    this.session.endRun();
    this.scene.showBackdrop();
    this.audio.stopMusic();
    this.audio.startTheme('game-over');
    this.renderGameOver();
    this.setHudVisible(false);
    this.setOverlay('gameover');
  }

  onStageCleared(stageSummary: StageSummary): void {
    const result = this.session.resolveStageClear(stageSummary.timeRemaining);

    if (result.finalStage) {
      this.session.endRun();
      this.scene.showBackdrop();
      this.audio.stopMusic();
      this.audio.startTheme('world-clear');
      this.setHudVisible(false);
      this.renderWorldClear(result.bonus);
      this.setOverlay('worldclear');
      return;
    }

    const summary: StageSummary = {
      ...stageSummary,
      score: this.session.score,
      coins: this.session.coins,
      lives: this.session.lives,
      difficulty: this.session.difficulty,
      bonus: result.bonus,
    };

    this.renderSummary(summary);
    this.setOverlay('summary');
    this.clearPendingTimer();
    this.pendingTimer = window.setTimeout(() => {
      this.session.advanceStage();
      this.queueStageTransition(this.session.currentStageId);
    }, 3000);
  }

  onGameplayPauseToggle(): void {
    if (!this.session.runActive) {
      return;
    }

    this.paused = !this.paused;
    this.scene.setPaused(this.paused);
    this.audio.playPause();
    this.setOverlay(this.paused ? 'pause' : 'none');
  }

  async startGame(): Promise<void> {
    this.clearPendingTimer();
    await this.audio.unlock();
    this.audio.playUiTick();
    this.session.startNewRun();
    this.paused = false;
    this.queueStageTransition(this.session.currentStageId);
  }

  showTitle(): void {
    this.clearPendingTimer();
    this.paused = false;
    this.session.endRun();
    this.scene.setPaused(false);
    this.scene.showBackdrop();
    this.setHudVisible(false);
    this.updateTitleState();
    this.audio.stopMusic();
    void this.audio
      .unlock()
      .then(() => {
        this.audio.startTheme('title');
      })
      .catch(() => undefined);
    this.setOverlay('title');
  }

  private bindUi(): void {
    this.bindButtonHoverAudio();

    this.titleStartButton.addEventListener('click', () => {
      void this.startGame();
    });
    this.titleControlsButton.addEventListener('click', () => {
      this.audio.playUiTick();
      this.openControls();
    });
    this.titleEnemiesButton.addEventListener('click', () => {
      this.audio.playUiTick();
      this.openEnemies();
    });
    this.titleConfigButton.addEventListener('click', () => {
      this.audio.playUiTick();
      this.openConfiguration();
    });
    this.titleDifficultyButton.addEventListener('click', () => {
      this.audio.playUiTick();
      this.cycleDifficulty();
    });
    this.modalBackButton.addEventListener('click', () => {
      this.audio.playUiBack();
      this.setOverlay('title');
    });
    this.pauseResumeButton.addEventListener('click', () => {
      this.onGameplayPauseToggle();
    });
    this.pauseTitleButton.addEventListener('click', () => {
      this.audio.playUiBack();
      this.showTitle();
    });
    this.gameOverRestartButton.addEventListener('click', () => {
      void this.startGame();
    });
    this.gameOverTitleButton.addEventListener('click', () => {
      this.audio.playUiBack();
      this.showTitle();
    });
    this.worldClearReplayButton.addEventListener('click', () => {
      void this.startGame();
    });
    this.worldClearTitleButton.addEventListener('click', () => {
      this.audio.playUiBack();
      this.showTitle();
    });

    window.addEventListener('keydown', (event) => {
      if (this.overlayState === 'title') {
        if (event.code === SHELL_SHORTCUTS.start) {
          event.preventDefault();
          void this.startGame();
        }

        if (event.code === SHELL_SHORTCUTS.controls) {
          event.preventDefault();
          this.audio.playUiTick();
          this.openControls();
        }

        if (event.code === SHELL_SHORTCUTS.enemies) {
          event.preventDefault();
          this.audio.playUiTick();
          this.openEnemies();
        }

        if (event.code === SHELL_SHORTCUTS.config) {
          event.preventDefault();
          this.audio.playUiTick();
          this.openConfiguration();
        }

        if (event.code === SHELL_SHORTCUTS.difficulty) {
          event.preventDefault();
          this.audio.playUiTick();
          this.cycleDifficulty();
        }
      } else if (this.overlayState === 'modal' && event.code === SHELL_SHORTCUTS.back) {
        event.preventDefault();
        this.audio.playUiBack();
        this.setOverlay('title');
      } else if (
        this.overlayState === 'pause' &&
        (event.code === 'Escape' || event.code === 'Enter')
      ) {
        event.preventDefault();
        this.onGameplayPauseToggle();
      } else if (
        (this.overlayState === 'gameover' || this.overlayState === 'worldclear') &&
        event.code === 'Enter'
      ) {
        event.preventDefault();
        void this.startGame();
      }
    });
  }

  private bindButtonHoverAudio(): void {
    this.root.querySelectorAll<HTMLButtonElement>('button').forEach((button) => {
      button.addEventListener('mouseenter', () => {
        this.audio.playUiHover();
      });
      button.addEventListener('focus', () => {
        this.audio.playUiHover();
      });
    });
  }

  private query<T extends HTMLElement>(selector: string): T {
    const element = this.root.querySelector<T>(selector);

    if (!element) {
      throw new Error(`Missing required element: ${selector}`);
    }

    return element;
  }

  private openControls(): void {
    this.modalState = 'controls';
    this.modalTitle.textContent = 'Controls';
    this.modalBody.innerHTML = `
      <div class="info-grid">
        <div class="info-row"><span>Move</span><strong>A / D or Arrow Keys</strong></div>
        <div class="info-row"><span>Jump / Double Jump</span><strong>Space / W / Up / K</strong></div>
        <div class="info-row"><span>Run / Attack</span><strong>Shift / J</strong></div>
        <div class="info-row"><span>Pause</span><strong>Escape / P</strong></div>
      </div>
      <p class="panel-note">
        Shift acts as the run modifier and also fires fire shots once the player reaches
        Enhanced Form.
      </p>
    `;
    this.setOverlay('modal');
  }

  private openEnemies(): void {
    this.modalState = 'enemies';
    this.modalTitle.textContent = 'Enemies';
    this.modalBody.innerHTML = `<div class="enemy-list">${getEnemyGuideItems()
      .map(
        (enemy) => `
          <div class="enemy-card">
            <div class="enemy-card__portrait-wrap">
              <img class="enemy-card__portrait" src="${enemy.portrait}" alt="" />
            </div>
            <div class="enemy-card__copy">
              <strong>${enemy.name}</strong>
              <span>${enemy.note}</span>
            </div>
          </div>
        `,
      )
      .join('')}</div>`;
    this.setOverlay('modal');
  }

  private openConfiguration(): void {
    this.modalState = 'config';
    this.modalTitle.textContent = 'Configuration';
    this.renderConfigurationBody();
    this.setOverlay('modal');
  }

  private renderConfigurationBody(): void {
    this.modalBody.innerHTML = `
      <div class="config-stack">
        <div class="config-row">
          <span>Music Volume</span>
          <div class="config-controls">
            <button type="button" class="small-button" data-config-action="music-down">-</button>
            <strong id="config-music-value">${formatDb(
              this.session.saveData.slot.settings.musicVolumeDb,
            )}</strong>
            <button type="button" class="small-button" data-config-action="music-up">+</button>
          </div>
        </div>
        <div class="config-row">
          <span>SFX Volume</span>
          <div class="config-controls">
            <button type="button" class="small-button" data-config-action="sfx-down">-</button>
            <strong id="config-sfx-value">${formatDb(
              this.session.saveData.slot.settings.sfxVolumeDb,
            )}</strong>
            <button type="button" class="small-button" data-config-action="sfx-up">+</button>
          </div>
        </div>
        <div class="config-row">
          <span>Difficulty</span>
          <div class="config-controls">
            <button type="button" class="small-button" data-config-action="difficulty-cycle">Cycle</button>
            <strong id="config-difficulty-value">${this.session.difficulty}</strong>
          </div>
        </div>
        <div class="config-preview">
          <span>Persisted JSON Shape</span>
          <pre id="config-save-preview">${this.session.toSavePreview()}</pre>
        </div>
      </div>
    `;

    this.modalBody.querySelectorAll<HTMLButtonElement>('[data-config-action]').forEach((button) => {
      button.addEventListener('click', () => {
        const action = button.dataset.configAction;

        this.audio.playUiTick();

        if (action === 'music-down') {
          const nextValue = this.session.adjustMusicVolume(-2);
          this.audio.setMusicVolumeDb(nextValue);
        }

        if (action === 'music-up') {
          const nextValue = this.session.adjustMusicVolume(2);
          this.audio.setMusicVolumeDb(nextValue);
        }

        if (action === 'sfx-down') {
          const nextValue = this.session.adjustSfxVolume(-2);
          this.audio.setSfxVolumeDb(nextValue);
        }

        if (action === 'sfx-up') {
          const nextValue = this.session.adjustSfxVolume(2);
          this.audio.setSfxVolumeDb(nextValue);
        }

        if (action === 'difficulty-cycle') {
          this.session.cycleDifficulty();
        }

        this.updateTitleState();
        this.renderConfigurationBody();
      });
    });
  }

  private cycleDifficulty(): void {
    this.session.cycleDifficulty();
    this.updateTitleState();

    if (this.modalState === 'config') {
      this.renderConfigurationBody();
    }
  }

  private queueStageTransition(stageId: string): void {
    const stage = getStageDefinition(stageId);

    this.scene.setPaused(false);
    this.scene.showBackdrop();
    this.renderTransition(stageId, stage.intro);
    this.setHudVisible(false);
    this.setOverlay('transition');
    this.clearPendingTimer();
    this.pendingTimer = window.setTimeout(() => {
      this.scene.startStage(stageId);
      this.audio.startTheme(stageId);
      this.setHudVisible(true);
      this.setOverlay('none');
    }, 3000);
  }

  private renderTransition(stageId: string, intro: string): void {
    this.transitionStage.textContent = `STAGE ${stageId}`;
    this.transitionMeta.textContent = `${this.session.form} Form • ${this.session.lives} Lives`;
    this.transitionIntro.textContent = intro;
  }

  private renderSummary(stageSummary: StageSummary): void {
    this.summaryStage.textContent = `Stage ${stageSummary.stageId} Clear`;
    this.summaryBody.innerHTML = `
      <div class="info-grid">
        <div class="info-row"><span>Time Bonus</span><strong>${stageSummary.bonus}</strong></div>
        <div class="info-row"><span>Score</span><strong>${stageSummary.score}</strong></div>
        <div class="info-row"><span>Coins</span><strong>${stageSummary.coins}</strong></div>
        <div class="info-row"><span>Lives</span><strong>${stageSummary.lives}</strong></div>
        <div class="info-row"><span>Time Remaining</span><strong>${stageSummary.timeRemaining}</strong></div>
        <div class="info-row"><span>Form</span><strong>${stageSummary.form}</strong></div>
        <div class="info-row"><span>Difficulty</span><strong>${stageSummary.difficulty}</strong></div>
      </div>
      <p class="panel-note">Advancing automatically to the next stage.</p>
    `;
  }

  private renderGameOver(): void {
    this.gameOverBody.innerHTML = `
      <div class="info-grid">
        <div class="info-row"><span>Best Score</span><strong>${this.session.saveData.slot.bestScore}</strong></div>
      </div>
      <p class="panel-note">Start a new run without relaunching the application.</p>
    `;
  }

  private renderWorldClear(bonus: number): void {
    const highestClear = this.session.saveData.slot.highestClearedStage ?? 'None';

    this.worldClearBody.innerHTML = `
      <div class="info-grid">
        <div class="info-row"><span>Final Score</span><strong>${this.session.score}</strong></div>
        <div class="info-row"><span>Highest Clear</span><strong>${highestClear}</strong></div>
        <div class="info-row"><span>Coins</span><strong>${this.session.coins}</strong></div>
        <div class="info-row"><span>Lives Remaining</span><strong>${this.session.lives}</strong></div>
        <div class="info-row"><span>Final Time Bonus</span><strong>${bonus}</strong></div>
        <div class="info-row"><span>Difficulty</span><strong>${this.session.difficulty}</strong></div>
      </div>
      <p class="panel-note">The web MVP world is complete. Save progress is already updated.</p>
    `;
  }

  private updateTitleState(): void {
    const highestCleared = this.session.saveData.slot.highestClearedStage ?? 'None';
    this.titleDifficultyValue.textContent = this.session.difficulty;
    this.titleProgress.textContent = `Best Score ${this.session.saveData.slot.bestScore} • Highest Clear ${highestCleared}`;
  }

  private setHudVisible(visible: boolean): void {
    this.hudBar.classList.toggle('is-hidden', !visible);
  }

  private setOverlay(nextState: OverlayState): void {
    this.overlayState = nextState;

    this.titleScreen.classList.toggle('is-visible', nextState === 'title');
    this.modalScreen.classList.toggle('is-visible', nextState === 'modal');
    this.transitionScreen.classList.toggle('is-visible', nextState === 'transition');
    this.summaryScreen.classList.toggle('is-visible', nextState === 'summary');
    this.pauseScreen.classList.toggle('is-visible', nextState === 'pause');
    this.gameOverScreen.classList.toggle('is-visible', nextState === 'gameover');
    this.worldClearScreen.classList.toggle('is-visible', nextState === 'worldclear');
  }

  private clearPendingTimer(): void {
    if (this.pendingTimer !== null) {
      window.clearTimeout(this.pendingTimer);
      this.pendingTimer = null;
    }
  }

  private buildMarkup(): string {
    return `
      <div class="web-shell">
        <header class="hud-bar is-hidden" id="hud-bar">
          <div class="hud-cell hud-cell--score"><span>Score</span><strong id="hud-score">000000</strong></div>
          <div class="hud-cell hud-cell--coins"><span>Coins</span><strong id="hud-coins">00</strong></div>
          <div class="hud-cell hud-cell--stage"><span>Stage</span><strong id="hud-stage">1-1</strong></div>
          <div class="hud-cell hud-cell--lives"><span>Lives</span><strong id="hud-lives">3</strong></div>
          <div class="hud-cell hud-cell--time"><span>Time</span><strong id="hud-time">300</strong></div>
          <div class="hud-cell hud-cell--difficulty"><span>Difficulty</span><strong id="hud-difficulty">Normal</strong></div>
        </header>
        <div class="viewport-shell">
          <div id="game-host" class="game-host"></div>
          <div class="overlay-root">
            <section class="screen title-screen is-visible" id="title-screen">
              <div class="title-left">
                <div class="logo-lockup">
                  <p class="logo-kicker">SPEC-DRIVEN WEB MVP</p>
                  <h1>SUPER PIXEL QUEST</h1>
                  <p class="logo-copy">
                    Browser implementation using the current shell, HUD, progression,
                    enemy, and save definitions.
                  </p>
                  <p class="title-progress" id="title-progress"></p>
                </div>
                <div class="title-actions">
                  <button type="button" id="title-start">Start Game</button>
                  <button type="button" id="title-controls">Controls</button>
                  <button type="button" id="title-enemies">Enemies</button>
                  <button type="button" id="title-config">Configuration</button>
                  <button type="button" id="title-difficulty">
                    Difficulty
                    <span id="title-difficulty-value">Normal</span>
                  </button>
                </div>
              </div>
              <div class="title-right">
                <div class="attract-stage">
                  <div class="attract-badge">Stage Preview</div>
                  <div class="preview-layer preview-layer-sky"></div>
                  <div class="preview-layer preview-layer-clouds"></div>
                  <div class="preview-layer preview-layer-mountains"></div>
                  <div class="preview-layer preview-layer-hills"></div>
                  <div class="preview-platform preview-platform--high"></div>
                  <div class="preview-platform preview-platform--mid"></div>
                  <div class="preview-ground">
                    <div class="preview-ground-top"></div>
                    <div class="preview-ground-fill"></div>
                  </div>
                  <img class="preview-sprite preview-sprite--player frame-a" src="${PLAYER_VISUALS.player.walk1}" alt="" />
                  <img class="preview-sprite preview-sprite--player frame-b" src="${PLAYER_VISUALS.player.walk2}" alt="" />
                  <img class="preview-sprite preview-sprite--zombie frame-a" src="${PLAYER_VISUALS.zombie.walk1}" alt="" />
                  <img class="preview-sprite preview-sprite--zombie frame-b" src="${PLAYER_VISUALS.zombie.walk2}" alt="" />
                  <img class="preview-sprite preview-sprite--armored" src="${ENEMY_VISUALS.Armored.path}" alt="" />
                  <img class="preview-sprite preview-sprite--flying" src="${ENEMY_VISUALS.Flying.path}" alt="" />
                  <img class="preview-sprite preview-sprite--shooter" src="${ENEMY_VISUALS.Shooter.path}" alt="" />
                </div>
                <p class="title-hint">
                  Enter to start, C for controls, E for enemies, O for configuration, D to cycle difficulty.
                </p>
              </div>
            </section>

            <section class="screen modal-screen" id="modal-screen">
              <div class="panel-shell">
                <div>
                  <p class="panel-kicker">Title Menu</p>
                  <h2 id="modal-title">Panel</h2>
                </div>
                <div class="panel-body" id="modal-body"></div>
                <div class="panel-footer">
                  <button type="button" id="modal-back">Back</button>
                </div>
              </div>
            </section>

            <section class="screen transition-screen" id="transition-screen">
              <div class="stage-card">
                <p class="panel-kicker">Run Start</p>
                <h2 id="transition-stage">STAGE 1-1</h2>
                <p class="stage-meta" id="transition-meta">Small Form • 3 Lives</p>
                <p class="stage-copy" id="transition-intro"></p>
              </div>
            </section>

            <section class="screen summary-screen" id="summary-screen">
              <div class="panel-shell">
                <p class="panel-kicker">Stage Clear</p>
                <h2 id="summary-stage">Stage 1-1 Clear</h2>
                <div class="panel-body" id="summary-body"></div>
              </div>
            </section>

            <section class="screen pause-screen" id="pause-screen">
              <div class="panel-shell compact">
                <p class="panel-kicker">Paused</p>
                <h2>Run Suspended</h2>
                <p class="panel-note">Timer and stage simulation remain frozen until resume.</p>
                <div class="panel-footer">
                  <button type="button" id="pause-resume">Resume</button>
                  <button type="button" id="pause-title">Return To Title</button>
                </div>
              </div>
            </section>

            <section class="screen gameover-screen" id="gameover-screen">
              <div class="panel-shell">
                <p class="panel-kicker">Run Ended</p>
                <h2>Game Over</h2>
                <div class="panel-body" id="gameover-body"></div>
                <div class="panel-footer">
                  <button type="button" id="gameover-restart">Start New Game</button>
                  <button type="button" id="gameover-title">Title Screen</button>
                </div>
              </div>
            </section>

            <section class="screen worldclear-screen" id="worldclear-screen">
              <div class="panel-shell">
                <p class="panel-kicker">World Clear</p>
                <h2>1-4 Complete</h2>
                <div class="panel-body" id="worldclear-body"></div>
                <div class="panel-footer">
                  <button type="button" id="worldclear-replay">Replay World</button>
                  <button type="button" id="worldclear-title">Title Screen</button>
                </div>
              </div>
            </section>
          </div>
        </div>
      </div>
    `;
  }
}
