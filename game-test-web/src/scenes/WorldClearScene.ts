import Phaser from "phaser";
import AudioManager from "../audio/AudioManager";
import { FONT_FAMILY, GAME_HEIGHT, GAME_WIDTH, SCENES } from "../core/Constants";
import GameState from "../core/GameState";

export default class WorldClearScene extends Phaser.Scene {
  private audio!: AudioManager;

  constructor() {
    super(SCENES.worldClear);
  }

  create(): void {
    this.audio = new AudioManager(this);
    this.cameras.main.setBackgroundColor("#000000");
    this.audio.playMusic("world-clear");

    const state = GameState.getState();

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 54, "WORLD CLEAR", {
        fontFamily: FONT_FAMILY,
        fontSize: "30px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5);

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 8, `FINAL SCORE ${state.score.toString().padStart(6, "0")}`, {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#f0db85",
      })
      .setOrigin(0.5);

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 24, `COINS ${state.coins.toString().padStart(2, "0")}   LIVES ${state.lives}`, {
        fontFamily: FONT_FAMILY,
        fontSize: "16px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5);

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 88, "PRESS ENTER OR CLICK TO RETURN TO TITLE", {
        fontFamily: FONT_FAMILY,
        fontSize: "14px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5);

    const restart = (): void => {
      this.audio.stopMusic();
      GameState.resetRun();
      this.scene.start(SCENES.title);
    };

    this.input.keyboard?.once("keydown-ENTER", restart);
    this.input.once("pointerdown", restart);
  }
}
