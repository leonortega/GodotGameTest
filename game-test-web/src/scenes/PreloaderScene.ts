import Phaser from "phaser";
import { FONT_FAMILY, GAME_HEIGHT, GAME_WIDTH, SCENES } from "../core/Constants";

export default class PreloaderScene extends Phaser.Scene {
  constructor() {
    super(SCENES.preload);
  }

  create(): void {
    this.cameras.main.setBackgroundColor("#000000");
    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2, "READY", {
        fontFamily: FONT_FAMILY,
        fontSize: "22px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5);

    this.time.delayedCall(250, () => {
      this.scene.start(SCENES.title);
    });
  }
}
