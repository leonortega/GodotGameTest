import Phaser from "phaser";
import AudioManager from "../audio/AudioManager";
import { DEPTHS, FONT_FAMILY, GAME_HEIGHT, GAME_WIDTH, SCENES, STAGE_ORDER } from "../core/Constants";
import GameState from "../core/GameState";
import Settings, { type AudioSettings } from "../core/Settings";
import { getStage } from "../data/stages";

export default class TitleScene extends Phaser.Scene {
  private audio!: AudioManager;
  private starting = false;
  private musicValue!: Phaser.GameObjects.Text;
  private sfxValue!: Phaser.GameObjects.Text;
  private selectedStageIndex = 0;
  private stageValue!: Phaser.GameObjects.Text;
  private stageName!: Phaser.GameObjects.Text;

  constructor() {
    super(SCENES.title);
  }

  create(): void {
    this.selectedStageIndex = STAGE_ORDER.indexOf(GameState.getSelectedStartStage());
    if (this.selectedStageIndex < 0) {
      this.selectedStageIndex = 0;
    }
    GameState.resetRun(STAGE_ORDER[this.selectedStageIndex]);
    this.audio = new AudioManager(this);
    this.audio.playMusic("title");
    this.drawBackdrop();
    this.drawAttractStrip();
    this.createChrome();
    this.createInput();
  }

  private drawBackdrop(): void {
    this.add.tileSprite(0, 0, GAME_WIDTH, GAME_HEIGHT, "background-sky").setOrigin(0).setDepth(DEPTHS.background);
    this.add.tileSprite(0, 0, GAME_WIDTH, 180, "background-clouds").setOrigin(0).setDepth(1).setAlpha(0.8);
    this.add.tileSprite(0, GAME_HEIGHT - 240, GAME_WIDTH, 180, "background-hills").setOrigin(0).setDepth(2).setAlpha(0.92);
  }

  private drawAttractStrip(): void {
    const stripY = 420;
    for (let x = 0; x < GAME_WIDTH + 32; x += 32) {
      this.add.image(x, stripY, "terrain-overworld").setOrigin(0, 0).setDisplaySize(32, 32).setDepth(DEPTHS.terrain);
      this.add.image(x, stripY + 32, "terrain-overworld").setOrigin(0, 0).setDisplaySize(32, 32).setDepth(DEPTHS.terrain);
    }

    const runner = this.add.sprite(GAME_WIDTH - 160, stripY - 24, "player-small-runA").setDepth(DEPTHS.actors);
    runner.setDisplaySize(66, 66);

    const chaserA = this.add.sprite(GAME_WIDTH - 86, stripY - 10, "enemy-goomba-a").setDepth(DEPTHS.actors);
    chaserA.setDisplaySize(40, 40);

    const chaserB = this.add.sprite(GAME_WIDTH - 36, stripY - 8, "enemy-beetle-a").setDepth(DEPTHS.actors);
    chaserB.setDisplaySize(40, 40);

    this.tweens.add({
      targets: [runner, chaserA, chaserB],
      x: "-=820",
      duration: 4200,
      repeat: -1,
      ease: "Linear",
      onRepeat: () => {
        runner.x = GAME_WIDTH + 120;
        chaserA.x = GAME_WIDTH + 180;
        chaserB.x = GAME_WIDTH + 240;
      },
    });

    this.time.addEvent({
      delay: 120,
      loop: true,
      callback: () => {
        runner.setTexture(runner.texture.key === "player-small-runA" ? "player-small-runB" : "player-small-runA");
        chaserA.setTexture(chaserA.texture.key === "enemy-goomba-a" ? "enemy-goomba-b" : "enemy-goomba-a");
        chaserB.setTexture(chaserB.texture.key === "enemy-beetle-a" ? "enemy-beetle-b" : "enemy-beetle-a");
      },
    });
  }

  private createChrome(): void {
    this.add.image(GAME_WIDTH / 2, 118, "logo").setDepth(DEPTHS.overlay).setScale(0.72);

    this.add
      .text(GAME_WIDTH / 2, 206, "SMB1-STYLE RUN", {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#fff4c7",
        stroke: "#2a1409",
        strokeThickness: 6,
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);

    const button = this.add.image(GAME_WIDTH / 2, 286, "ui-button").setDepth(DEPTHS.overlay);
    button.setInteractive({ useHandCursor: true });

    const label = this.add
      .text(GAME_WIDTH / 2, 286, "START GAME", {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay + 1);

    const help = this.add
      .text(GAME_WIDTH / 2, 374, "ARROWS OR WASD   SHIFT/J RUN   Z/SPACE/K JUMP   X FIRE   ESC/P PAUSE", {
        fontFamily: FONT_FAMILY,
        fontSize: "13px",
        color: "#f7f1d1",
        align: "center",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);

    const subline = this.add
      .text(GAME_WIDTH / 2, 404, "PRESS ENTER OR CLICK TO START", {
        fontFamily: FONT_FAMILY,
        fontSize: "13px",
        color: "#f0db85",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);

    this.tweens.add({
      targets: [button, label, help, subline],
      alpha: 0.78,
      duration: 760,
      yoyo: true,
      repeat: -1,
      ease: "Sine.InOut",
    });

    button.on("pointerdown", () => this.startRun());

    this.add
      .text(GAME_WIDTH / 2, 336, "STARTING STAGE", {
        fontFamily: FONT_FAMILY,
        fontSize: "14px",
        color: "#fff0ad",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);

    const left = this.add
      .text(GAME_WIDTH / 2 - 110, 336, "<", {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay)
      .setInteractive({ useHandCursor: true });

    this.stageValue = this.add
      .text(GAME_WIDTH / 2, 336, "", {
        fontFamily: FONT_FAMILY,
        fontSize: "16px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);

    const right = this.add
      .text(GAME_WIDTH / 2 + 110, 336, ">", {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay)
      .setInteractive({ useHandCursor: true });

    this.stageName = this.add
      .text(GAME_WIDTH / 2, 354, "", {
        fontFamily: FONT_FAMILY,
        fontSize: "12px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);

    left.on("pointerdown", () => this.shiftSelectedStage(-1));
    right.on("pointerdown", () => this.shiftSelectedStage(1));
    this.renderSelectedStage();

    this.add
      .text(GAME_WIDTH / 2, 438, "AUDIO SETTINGS", {
        fontFamily: FONT_FAMILY,
        fontSize: "14px",
        color: "#fff0ad",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);

    this.musicValue = this.createSettingRow(474, "MUSIC", () => this.adjustMusic(-2), () => this.adjustMusic(2));
    this.sfxValue = this.createSettingRow(510, "SFX", () => this.adjustSfx(-2), () => this.adjustSfx(2));
    this.renderSettings();

    this.add
      .text(GAME_WIDTH / 2, 532, "LEFT/RIGHT STAGE   Q/E MUSIC   A/D SFX", {
        fontFamily: FONT_FAMILY,
        fontSize: "12px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);
  }

  private createInput(): void {
    this.input.keyboard?.on("keydown-ENTER", () => this.startRun());
    this.input.keyboard?.on("keydown-SPACE", () => this.startRun());
    this.input.keyboard?.on("keydown-LEFT", () => this.shiftSelectedStage(-1));
    this.input.keyboard?.on("keydown-RIGHT", () => this.shiftSelectedStage(1));
    this.input.keyboard?.on("keydown-Q", () => this.adjustMusic(-2));
    this.input.keyboard?.on("keydown-E", () => this.adjustMusic(2));
    this.input.keyboard?.on("keydown-A", () => this.adjustSfx(-2));
    this.input.keyboard?.on("keydown-D", () => this.adjustSfx(2));
  }

  private shiftSelectedStage(direction: number): void {
    if (this.starting) {
      return;
    }

    this.selectedStageIndex = Phaser.Math.Wrap(this.selectedStageIndex + direction, 0, STAGE_ORDER.length);
    this.renderSelectedStage();
    this.audio.playSfx("select", { volume: 0.18 });
  }

  private renderSelectedStage(): void {
    const stageId = STAGE_ORDER[this.selectedStageIndex];
    const stage = getStage(stageId);
    GameState.setSelectedStartStage(stageId);
    this.stageValue.setText(stageId);
    this.stageName.setText(stage.name.toUpperCase());
  }

  private createSettingRow(
    y: number,
    label: string,
    decrease: () => void,
    increase: () => void,
  ): Phaser.GameObjects.Text {
    this.add
      .text(GAME_WIDTH / 2 - 108, y, label, {
        fontFamily: FONT_FAMILY,
        fontSize: "14px",
        color: "#f7f1d1",
      })
      .setOrigin(0, 0.5)
      .setDepth(DEPTHS.overlay);

    const minus = this.add
      .text(GAME_WIDTH / 2 - 4, y, "-", {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#fff0ad",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay)
      .setInteractive({ useHandCursor: true });

    const value = this.add
      .text(GAME_WIDTH / 2 + 44, y, "", {
        fontFamily: FONT_FAMILY,
        fontSize: "14px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay);

    const plus = this.add
      .text(GAME_WIDTH / 2 + 94, y, "+", {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#fff0ad",
      })
      .setOrigin(0.5)
      .setDepth(DEPTHS.overlay)
      .setInteractive({ useHandCursor: true });

    minus.on("pointerdown", decrease);
    plus.on("pointerdown", increase);

    return value;
  }

  private renderSettings(): void {
    const settings = Settings.getAudioSettings();
    this.musicValue.setText(`${settings.musicVolumeDb} dB`);
    this.sfxValue.setText(`${settings.sfxVolumeDb} dB`);
  }

  private adjustMusic(deltaDb: number): void {
    if (this.starting) {
      return;
    }

    const settings = Settings.updateAudioSettings({
      musicVolumeDb: Settings.getAudioSettings().musicVolumeDb + deltaDb,
    });
    this.renderAdjustedSettings(settings);
  }

  private adjustSfx(deltaDb: number): void {
    if (this.starting) {
      return;
    }

    const settings = Settings.updateAudioSettings({
      sfxVolumeDb: Settings.getAudioSettings().sfxVolumeDb + deltaDb,
    });
    this.renderAdjustedSettings(settings);
    this.audio.playSfx("coin", { volume: 0.24 });
  }

  private renderAdjustedSettings(settings: AudioSettings): void {
    this.musicValue.setText(`${settings.musicVolumeDb} dB`);
    this.sfxValue.setText(`${settings.sfxVolumeDb} dB`);
    this.audio.refreshMusicVolume();
  }

  private startRun(): void {
    if (this.starting) {
      return;
    }

    this.starting = true;
    const stageId = STAGE_ORDER[this.selectedStageIndex];
    GameState.setSelectedStartStage(stageId);
    GameState.resetRun(stageId);
    this.audio.playSfx("select", { volume: 0.4 });
    this.time.delayedCall(120, () => {
      this.audio.stopMusic();
      this.scene.start(SCENES.startCard, { stageId, restart: false });
    });
  }
}
