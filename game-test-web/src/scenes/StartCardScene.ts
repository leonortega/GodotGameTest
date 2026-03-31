import Phaser from "phaser";
import { FONT_FAMILY, GAME_HEIGHT, GAME_WIDTH, SCENES, type StageId } from "../core/Constants";
import GameState from "../core/GameState";

type StartCardData = {
  stageId: StageId;
  restart?: boolean;
};

export default class StartCardScene extends Phaser.Scene {
  constructor() {
    super(SCENES.startCard);
  }

  create(data: StartCardData): void {
    const stageId = data.stageId ?? GameState.getState().stageId;
    GameState.setStage(stageId);

    const { lives } = GameState.getState();
    this.cameras.main.setBackgroundColor("#000000");

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 76, "WORLD", {
        fontFamily: FONT_FAMILY,
        fontSize: "18px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5);

    this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 36, stageId, {
        fontFamily: FONT_FAMILY,
        fontSize: "30px",
        color: "#fff0ad",
      })
      .setOrigin(0.5);

    this.add.sprite(GAME_WIDTH / 2 - 26, GAME_HEIGHT / 2 + 30, "player-small-idle").setDisplaySize(48, 48);

    this.add
      .text(GAME_WIDTH / 2 + 24, GAME_HEIGHT / 2 + 30, `x ${lives}`, {
        fontFamily: FONT_FAMILY,
        fontSize: "26px",
        color: "#f7f1d1",
      })
      .setOrigin(0.5);

    if (data.restart) {
      this.add
        .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 98, "GET READY", {
          fontFamily: FONT_FAMILY,
          fontSize: "16px",
          color: "#f0db85",
        })
        .setOrigin(0.5);
    }

    this.time.delayedCall(2800, () => {
      this.scene.start(SCENES.game, { stageId });
    });
  }
}
