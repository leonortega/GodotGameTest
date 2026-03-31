import Phaser from "phaser";
import { GAME_WIDTH, SCENES } from "../core/Constants";

function drawRoundedRect(
  graphics: Phaser.GameObjects.Graphics,
  x: number,
  y: number,
  width: number,
  height: number,
  radius: number,
  color: number,
): void {
  graphics.fillStyle(color, 1);
  graphics.fillRoundedRect(x, y, width, height, radius);
}

export default class BootScene extends Phaser.Scene {
  constructor() {
    super(SCENES.boot);
  }

  preload(): void {
    this.load.image("logo", "/assets/ui/PixelQuest_Logo.png");
    this.load.audio("title", "/assets/audio/music/title.ogg");
    this.load.audio("overworld", "/assets/audio/music/overworld.ogg");
    this.load.audio("underground", "/assets/audio/music/overworld.ogg");
    this.load.audio("athletic-sky", "/assets/audio/music/overworld.ogg");
    this.load.audio("castle", "/assets/audio/music/overworld.ogg");
    this.load.audio("invincibility", "/assets/audio/music/title.ogg");
    this.load.audio("hurry", "/assets/audio/music/world-clear.ogg");
    this.load.audio("game-over", "/assets/audio/music/title.ogg");
    this.load.audio("world-clear", "/assets/audio/music/world-clear.ogg");
    this.load.audio("jump", "/assets/audio/sfx/jump.ogg");
    this.load.audio("landing", "/assets/audio/sfx/bump.ogg");
    this.load.audio("bump", "/assets/audio/sfx/bump.ogg");
    this.load.audio("stomp", "/assets/audio/sfx/bump.ogg");
    this.load.audio("brick-break", "/assets/audio/sfx/bump.ogg");
    this.load.audio("coin", "/assets/audio/sfx/coin.ogg");
    this.load.audio("power-up", "/assets/audio/sfx/coin.ogg");
    this.load.audio("extra-life", "/assets/audio/sfx/select.ogg");
    this.load.audio("hurt", "/assets/audio/sfx/hurt.ogg");
    this.load.audio("power-down", "/assets/audio/sfx/hurt.ogg");
    this.load.audio("death", "/assets/audio/sfx/fire.ogg");
    this.load.audio("select", "/assets/audio/sfx/select.ogg");
    this.load.audio("pause", "/assets/audio/sfx/select.ogg");
    this.load.audio("stage-clear", "/assets/audio/sfx/select.ogg");
    this.load.audio("fire", "/assets/audio/sfx/fire.ogg");
    this.load.audio("enemy-defeat", "/assets/audio/sfx/fire.ogg");

    this.load.image("background-sky", "/assets/art/backgrounds/background_solid_sky.png");
    this.load.image("background-clouds", "/assets/art/backgrounds/background_clouds.png");
    this.load.image("background-hills", "/assets/art/backgrounds/background_color_hills.png");
    this.load.image("background-dirt", "/assets/art/backgrounds/background_solid_dirt.png");
    this.load.image("background-trees", "/assets/art/backgrounds/background_color_trees.png");

    this.load.image("terrain-overworld", "/assets/art/terrain/terrain_grass_block_top.png");
    this.load.image("terrain-underground", "/assets/art/terrain/terrain_stone_block_top.png");
    this.load.image("terrain-athletic_sky", "/assets/art/terrain/terrain_sand_block_top.png");
    this.load.image("terrain-castle", "/assets/art/terrain/terrain_stone_block_top.png");

    this.load.image("block-question", "/assets/art/terrain/block_coin_active.png");
    this.load.image("block-brick", "/assets/art/terrain/brick_brown.png");
    this.load.image("block-used", "/assets/art/terrain/block_empty.png");
    this.load.image("block-hidden", "/assets/art/terrain/block_empty.png");
    this.load.image("goal-marker", "/assets/art/terrain/flag_green_a.png");

    this.load.image("player-small-idle", "/assets/art/characters/character_beige_idle.png");
    this.load.image("player-small-runA", "/assets/art/characters/character_beige_walk_a.png");
    this.load.image("player-small-runB", "/assets/art/characters/character_beige_walk_b.png");
    this.load.image("player-small-jump", "/assets/art/characters/character_beige_jump.png");
    this.load.image("player-small-fall", "/assets/art/characters/character_beige_jump.png");
    this.load.image("player-small-crouch", "/assets/art/characters/character_beige_duck.png");

    this.load.image("player-super-idle", "/assets/art/characters/character_green_idle.png");
    this.load.image("player-super-runA", "/assets/art/characters/character_green_walk_a.png");
    this.load.image("player-super-runB", "/assets/art/characters/character_green_walk_b.png");
    this.load.image("player-super-jump", "/assets/art/characters/character_green_jump.png");
    this.load.image("player-super-fall", "/assets/art/characters/character_green_jump.png");
    this.load.image("player-super-crouch", "/assets/art/characters/character_green_duck.png");

    this.load.image("player-fire-idle", "/assets/art/characters/character_pink_idle.png");
    this.load.image("player-fire-runA", "/assets/art/characters/character_pink_walk_a.png");
    this.load.image("player-fire-runB", "/assets/art/characters/character_pink_walk_b.png");
    this.load.image("player-fire-jump", "/assets/art/characters/character_pink_jump.png");
    this.load.image("player-fire-fall", "/assets/art/characters/character_pink_jump.png");
    this.load.image("player-fire-crouch", "/assets/art/characters/character_pink_duck.png");

    this.load.image("enemy-goomba-rest", "/assets/art/enemies/slime_normal_rest.png");
    this.load.image("enemy-goomba-a", "/assets/art/enemies/slime_normal_walk_a.png");
    this.load.image("enemy-goomba-b", "/assets/art/enemies/slime_normal_walk_b.png");
    this.load.image("enemy-beetle-rest", "/assets/art/enemies/snail_rest.png");
    this.load.image("enemy-beetle-a", "/assets/art/enemies/snail_walk_a.png");
    this.load.image("enemy-beetle-b", "/assets/art/enemies/snail_walk_b.png");

    this.load.image("pickup-coin", "/assets/art/items/coin_gold.png");
    this.load.image("pickup-mushroom", "/assets/art/items/mushroom_red.png");
    this.load.image("pickup-fire-flower", "/assets/art/items/gem_red.png");
    this.load.image("pickup-star", "/assets/art/items/star.png");
    this.load.image("pickup-1up", "/assets/art/items/gem_green.png");
    this.load.image("fireball", "/assets/art/items/fireball.png");
  }

  create(): void {
    this.createUiTexture();
    this.createHudPanel();
    this.createTerrainFillTextures();
    this.createBlockTextures();
    this.scene.start(SCENES.preload);
  }

  private createUiTexture(): void {
    const g = this.add.graphics();
    g.setVisible(false);
    drawRoundedRect(g, 0, 0, 192, 48, 10, 0x20130b);
    g.lineStyle(4, 0xf3d977, 1);
    g.strokeRoundedRect(2, 2, 188, 44, 10);
    g.generateTexture("ui-button", 192, 48);
    g.destroy();
  }

  private createHudPanel(): void {
    const backplate = this.add.graphics();
    backplate.setVisible(false);
    backplate.fillStyle(0x000000, 0.76);
    backplate.fillRect(0, 0, GAME_WIDTH, 68);
    backplate.lineStyle(2, 0xf5d76e, 1);
    backplate.strokeRect(0, 0, GAME_WIDTH, 68);
    backplate.generateTexture("hud-panel", GAME_WIDTH, 68);
    backplate.destroy();
  }

  private createTerrainFillTextures(): void {
    const fills: Array<{ key: string; color: number; accent: number }> = [
      { key: "terrain-fill-overworld", color: 0xc77846, accent: 0x9e562d },
      { key: "terrain-fill-underground", color: 0x656070, accent: 0x4a4553 },
      { key: "terrain-fill-athletic_sky", color: 0xd8b163, accent: 0xb68440 },
      { key: "terrain-fill-castle", color: 0x66525d, accent: 0x4b3942 },
    ];

    fills.forEach(({ key, color, accent }) => {
      const graphics = this.add.graphics();
      graphics.setVisible(false);
      graphics.fillStyle(color, 1);
      graphics.fillRect(0, 0, 32, 32);
      graphics.fillStyle(accent, 1);
      graphics.fillRect(0, 18, 32, 14);
      graphics.lineStyle(2, accent, 1);
      graphics.strokeRect(0, 0, 32, 32);
      graphics.generateTexture(key, 32, 32);
      graphics.destroy();
    });
  }

  private createBlockTextures(): void {
    const graphics = this.add.graphics();
    graphics.setVisible(false);
    graphics.fillStyle(0xb96a3c, 1);
    graphics.fillRect(0, 0, 32, 32);
    graphics.fillStyle(0x94502c, 1);
    graphics.fillRect(0, 8, 32, 4);
    graphics.fillRect(0, 20, 32, 4);
    graphics.fillRect(8, 0, 4, 32);
    graphics.fillRect(20, 0, 4, 32);
    graphics.lineStyle(2, 0x6b3418, 1);
    graphics.strokeRect(0, 0, 32, 32);
    graphics.generateTexture("block-brick-solid", 32, 32);
    graphics.destroy();
  }
}
