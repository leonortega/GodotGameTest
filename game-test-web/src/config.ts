import Phaser from "phaser";
import { GAME_HEIGHT, GAME_WIDTH, SCENES, WORLD_HEIGHT } from "./core/Constants";
import BootScene from "./scenes/BootScene";
import PreloaderScene from "./scenes/PreloaderScene";
import TitleScene from "./scenes/TitleScene";
import StartCardScene from "./scenes/StartCardScene";
import GameScene from "./scenes/GameScene";
import GameOverScene from "./scenes/GameOverScene";
import WorldClearScene from "./scenes/WorldClearScene";

const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: GAME_WIDTH,
  height: GAME_HEIGHT,
  parent: "app",
  backgroundColor: "#101820",
  pixelArt: true,
  roundPixels: true,
  scale: {
    mode: Phaser.Scale.FIT,
    autoCenter: Phaser.Scale.CENTER_BOTH,
  },
  physics: {
    default: "arcade",
    arcade: {
      gravity: { y: 1700, x: 0 },
      debug: false,
    },
  },
  render: {
    antialias: false,
    pixelArt: true,
  },
  scene: [
    BootScene,
    PreloaderScene,
    TitleScene,
    StartCardScene,
    GameScene,
    GameOverScene,
    WorldClearScene,
  ],
  callbacks: {
    postBoot: (game) => {
      game.registry.set("worldHeight", WORLD_HEIGHT);
      game.registry.set("sceneIds", SCENES);
      game.canvas.setAttribute("tabindex", "0");
      game.canvas.style.outline = "none";
      game.canvas.focus();
    },
  },
};

export default config;
