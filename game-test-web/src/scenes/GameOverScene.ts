import Phaser from "phaser";
import AudioManager from "../audio/AudioManager";
import { FONT_FAMILY, GAME_HEIGHT, GAME_WIDTH, SCENES } from "../core/Constants";
import GameState from "../core/GameState";

export default class GameOverScene extends Phaser.Scene {
  private audio!: AudioManager;

  constructor() {
    super(SCENES.gameOver);
  }

  create(): void {
    this.audio = new AudioManager(this);
    this.cameras.main.setBackgroundColor("#000000");
    this.audio.playMusic("game-over", false);

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 42, "GAME OVER", {
        fontFamily: FONT_FAMILY,
        fontSize: "34px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5);

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 10, `SCORE ${GameState.getState().score.toString().padStart(6, "0")}`, {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#f0db85",
      })
      .setOrigin(0.5);

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 76, "ENTER NEW GAME   ESC TITLE", {
        fontFamily: FONT_FAMILY,
        fontSize: "14px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5);

    this.input.keyboard?.once("keydown-ENTER", () => {
      const stageId = GameState.getSelectedStartStage();
      this.audio.stopMusic();
      GameState.resetRun(stageId);
      this.scene.start(SCENES.startCard, { stageId, restart: false });
    });

    this.input.keyboard?.once("keydown-ESC", () => {
      this.audio.stopMusic();
      GameState.resetRun(GameState.getSelectedStartStage());
      this.scene.start(SCENES.title);
    });

    this.input.once("pointerdown", () => {
      this.audio.stopMusic();
      GameState.resetRun(GameState.getSelectedStartStage());
      this.scene.start(SCENES.title);
    });
  }
}
