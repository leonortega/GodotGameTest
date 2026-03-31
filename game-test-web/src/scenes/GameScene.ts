import Phaser from "phaser";
import AudioManager, { type MusicKey } from "../audio/AudioManager";
import { DEPTHS, FONT_FAMILY, GAME_HEIGHT, GAME_WIDTH, PLAYER_TUNING, SCENES, SCORE_VALUES, STAGE_ORDER, TILE_SIZE, type PickupType, type PlayerForm, type StageId, type ThemeId } from "../core/Constants";
import GameState from "../core/GameState";
import { getStage, type StageConfig } from "../data/stages";

function moveTowards(current: number, target: number, maxDelta: number): number {
  if (Math.abs(target - current) <= maxDelta) {
    return target;
  }

  return current + Math.sign(target - current) * maxDelta;
}

function getTerrainTextureKey(theme: ThemeId): string {
  return {
    overworld: "terrain-overworld",
    underground: "terrain-underground",
    athletic_sky: "terrain-athletic_sky",
    castle: "terrain-castle",
  }[theme];
}

function getTerrainFillTextureKey(theme: ThemeId): string {
  return {
    overworld: "terrain-fill-overworld",
    underground: "terrain-fill-underground",
    athletic_sky: "terrain-fill-athletic_sky",
    castle: "terrain-fill-castle",
  }[theme];
}

function getPlayerTextureKey(form: PlayerForm, pose: "idle" | "runA" | "runB" | "jump" | "fall" | "crouch"): string {
  return `player-${form.toLowerCase()}-${pose}`;
}

function getEnemyFrameKey(variant: "goomba" | "beetle", frame: "rest" | "a" | "b"): string {
  return variant === "goomba" ? `enemy-goomba-${frame}` : `enemy-beetle-${frame}`;
}

const PAUSE_MENU_OPTIONS = ["Resume", "Restart Level", "Title"] as const;

type BlockSprite = Phaser.Physics.Arcade.Sprite & {
  refreshBody(): void;
  getData(key: string): unknown;
  setData(key: string, value: unknown): BlockSprite;
};

type PickupSprite = Phaser.Physics.Arcade.Sprite & {
  getData(key: string): unknown;
  setData(key: string, value: unknown): PickupSprite;
};

type EnemySprite = Phaser.Physics.Arcade.Sprite & {
  getData(key: string): unknown;
  setData(key: string, value: unknown): EnemySprite;
};

type MovingPlatformSprite = Phaser.Physics.Arcade.Sprite & {
  getData(key: string): unknown;
  setData(key: string, value: unknown): MovingPlatformSprite;
};

type FallingBlockSprite = Phaser.Physics.Arcade.Sprite & {
  getData(key: string): unknown;
  setData(key: string, value: unknown): FallingBlockSprite;
};

export default class GameScene extends Phaser.Scene {
  private stage!: StageConfig;
  private audio!: AudioManager;
  private player!: Phaser.Physics.Arcade.Sprite;
  private terrain!: Phaser.Physics.Arcade.StaticGroup;
  private blocks!: Phaser.Physics.Arcade.StaticGroup;
  private coins!: Phaser.Physics.Arcade.StaticGroup;
  private enemies!: Phaser.Physics.Arcade.Group;
  private movingPlatforms!: Phaser.Physics.Arcade.Group;
  private fallingBlocks!: Phaser.Physics.Arcade.Group;
  private pickups!: Phaser.Physics.Arcade.Group;
  private fireballs!: Phaser.Physics.Arcade.Group;
  private goal!: Phaser.GameObjects.Zone;
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private keyRun!: Phaser.Input.Keyboard.Key;
  private keyRunAlt!: Phaser.Input.Keyboard.Key;
  private keyJump!: Phaser.Input.Keyboard.Key;
  private keyJumpAlt!: Phaser.Input.Keyboard.Key;
  private keyFire!: Phaser.Input.Keyboard.Key;
  private keyPause!: Phaser.Input.Keyboard.Key;
  private keyPauseAlt!: Phaser.Input.Keyboard.Key;
  private keyMenuConfirm!: Phaser.Input.Keyboard.Key;
  private keyA!: Phaser.Input.Keyboard.Key;
  private keyD!: Phaser.Input.Keyboard.Key;
  private keyS!: Phaser.Input.Keyboard.Key;
  private keyW!: Phaser.Input.Keyboard.Key;
  private hudText!: Phaser.GameObjects.Text;
  private tallyText!: Phaser.GameObjects.Text;
  private pauseOverlay?: Phaser.GameObjects.Container;
  private pauseMenuTexts: Phaser.GameObjects.Text[] = [];
  private timerRemaining = 0;
  private stageComplete = false;
  private playerDead = false;
  private paused = false;
  private invulnerableUntil = 0;
  private starUntil = 0;
  private fireCooldownUntil = 0;
  private jumpStartedAt = 0;
  private playerFacing: -1 | 1 = 1;
  private runAnimFrame = false;
  private cameraProgressX = 0;
  private lastEnemyStompAt = 0;
  private stompChain = 0;
  private jumpCountSinceGrounded = 0;
  private pauseMenuSelection = 0;
  private wasGroundedLastFrame = true;
  private hurryStateEntered = false;

  constructor() {
    super(SCENES.game);
  }

  create(data: { stageId: StageId }): void {
    this.stage = getStage(data.stageId);
    this.audio = new AudioManager(this);
    GameState.setStage(this.stage.id);

    this.stageComplete = false;
    this.playerDead = false;
    this.paused = false;
    this.invulnerableUntil = 0;
    this.starUntil = 0;
    this.fireCooldownUntil = 0;
    this.timerRemaining = this.stage.timer;
    this.cameraProgressX = 0;
    this.stompChain = 0;
    this.jumpCountSinceGrounded = 0;
    this.pauseMenuSelection = 0;
    this.wasGroundedLastFrame = true;
    this.hurryStateEntered = false;

    this.physics.world.setBounds(0, 0, this.stage.widthTiles * TILE_SIZE, GAME_HEIGHT + 220);
    this.cameras.main.setBounds(0, 0, this.stage.widthTiles * TILE_SIZE, GAME_HEIGHT);
    this.cameras.main.setRoundPixels(true);
    this.cameras.main.setBackgroundColor("#000000");

    this.drawThemeBackdrop();

    this.terrain = this.physics.add.staticGroup();
    this.blocks = this.physics.add.staticGroup();
    this.coins = this.physics.add.staticGroup();
    this.enemies = this.physics.add.group({ allowGravity: true, immovable: false });
    this.movingPlatforms = this.physics.add.group({ allowGravity: false, immovable: true });
    this.fallingBlocks = this.physics.add.group({ allowGravity: false, immovable: true });
    this.pickups = this.physics.add.group({ allowGravity: true, immovable: false });
    this.fireballs = this.physics.add.group({ allowGravity: true, immovable: false });

    this.buildTerrain();
    this.buildBlocks();
    this.buildCoins();
    this.buildEnemies();
    this.buildMovingPlatforms();
    this.buildFallingBlocks();
    this.buildGoal();
    this.buildPlayer();
    this.buildHud();
    this.buildInput();
    this.buildPauseOverlay();
    this.bindPhysics();

    this.syncMusicState();
  }

  update(time: number, delta: number): void {
    if (Phaser.Input.Keyboard.JustDown(this.keyPause) || Phaser.Input.Keyboard.JustDown(this.keyPauseAlt)) {
      this.togglePause();
      return;
    }

    if (this.paused) {
      this.updatePauseMenu();
      return;
    }

    if (this.stageComplete || this.playerDead) {
      return;
    }

    this.handleMovement(time, delta);
    this.updatePlayerPose(time);
    this.updateEnemies(time);
    this.updateMovingPlatforms();
    this.updateFallingBlocks(time);
    this.updatePickups();
    this.updateFireballs();
    this.updateTimer(delta);
    this.updateTraversalAudio();
    this.syncMusicState();
    this.updateCamera();
    this.updateHud();

    if (this.player.y > GAME_HEIGHT + 100) {
      this.loseLife();
    }
  }

  private updateMovingPlatforms(): void {
    this.movingPlatforms.children.iterate((entry) => {
      if (!entry) {
        return true;
      }

      const platform = entry as MovingPlatformSprite;
      const body = platform.body as Phaser.Physics.Arcade.Body;
      const speed = platform.getData("speed") as number;
      const left = platform.getData("left") as number;
      const right = platform.getData("right") as number;
      if (platform.x <= left) {
        platform.x = left;
        body.setVelocityX(speed);
      } else if (platform.x >= right) {
        platform.x = right;
        body.setVelocityX(-speed);
      }
      return true;
    });
  }

  private updateFallingBlocks(time: number): void {
    this.fallingBlocks.children.iterate((entry) => {
      if (!entry) {
        return true;
      }

      const block = entry as FallingBlockSprite;
      const body = block.body as Phaser.Physics.Arcade.Body;
      const falling = block.getData("falling") as boolean;
      if (falling) {
        if (block.y > GAME_HEIGHT + 120) {
          block.destroy();
        }
        return true;
      }

      const contactStartedAt = block.getData("contactStartedAt") as number;
      const lastContactAt = block.getData("lastContactAt") as number;
      if (contactStartedAt > 0 && time - lastContactAt > 80) {
        block.setData("contactStartedAt", 0);
        return true;
      }

      if (contactStartedAt > 0 && time - contactStartedAt >= 500) {
        block.setData("falling", true);
        block.setTint(0xe0a85f);
        body.setImmovable(false);
        body.setAllowGravity(true);
        body.setVelocityY(24);
      }
      return true;
    });
  }

  private updateTraversalAudio(): void {
    const body = this.player.body as Phaser.Physics.Arcade.Body;
    const onGround = body.blocked.down || body.touching.down;
    if (onGround && !this.wasGroundedLastFrame && !this.playerDead && !this.stageComplete && this.player.y < GAME_HEIGHT + 80) {
      this.audio.playSfx("landing", { volume: 0.14 });
    }

    this.wasGroundedLastFrame = onGround;
  }

  private drawThemeBackdrop(): void {
    const width = this.stage.widthTiles * TILE_SIZE;
    if (this.stage.theme === "overworld") {
      this.add.tileSprite(0, 0, width, GAME_HEIGHT, "background-sky").setOrigin(0).setDepth(DEPTHS.background);
      this.add.tileSprite(0, 0, width, 180, "background-clouds").setOrigin(0).setDepth(1).setAlpha(0.8);
      this.add.tileSprite(0, GAME_HEIGHT - 236, width, 180, "background-hills").setOrigin(0).setDepth(2).setAlpha(0.92);
      return;
    }

    if (this.stage.theme === "athletic_sky") {
      this.add.tileSprite(0, 0, width, GAME_HEIGHT, "background-sky").setOrigin(0).setDepth(DEPTHS.background);
      this.add.tileSprite(0, 0, width, 220, "background-clouds").setOrigin(0).setDepth(1);
      return;
    }

    if (this.stage.theme === "underground") {
      this.add.tileSprite(0, 0, width, GAME_HEIGHT, "background-dirt").setOrigin(0).setDepth(DEPTHS.background).setTint(0x5d5d6f);
      return;
    }

    this.add.tileSprite(0, 0, width, GAME_HEIGHT, "background-dirt").setOrigin(0).setDepth(DEPTHS.background).setTint(0x443a45);
    this.add.tileSprite(0, GAME_HEIGHT - 240, width, 180, "background-trees").setOrigin(0).setDepth(1).setAlpha(0.35).setTint(0x6a5064);
  }

  private buildTerrain(): void {
    const terrainTexture = getTerrainTextureKey(this.stage.theme);
    const terrainFillTexture = getTerrainFillTextureKey(this.stage.theme);
    [...this.stage.ground, ...(this.stage.ceilings ?? [])].forEach((segment) => {
      for (let dx = 0; dx < segment.width; dx += 1) {
        for (let dy = 0; dy < segment.height; dy += 1) {
          const texture = dy === 0 ? terrainTexture : terrainFillTexture;
          const tile = this.terrain
            .create((segment.x + dx) * TILE_SIZE, (segment.y + dy) * TILE_SIZE, texture)
            .setOrigin(0, 0)
            .setDisplaySize(TILE_SIZE, TILE_SIZE)
            .setDepth(DEPTHS.terrain);
          tile.refreshBody();
        }
      }
    });
  }

  private buildBlocks(): void {
    this.stage.blocks.forEach((blockConfig) => {
      const texture = blockConfig.type === "question" ? "block-question" : blockConfig.type === "brick" ? "block-brick-solid" : "block-hidden";
      const block = this.blocks
        .create(blockConfig.x * TILE_SIZE, blockConfig.y * TILE_SIZE, texture)
        .setOrigin(0, 0)
        .setDisplaySize(TILE_SIZE, TILE_SIZE)
        .setDepth(DEPTHS.blocks) as BlockSprite;

      block.refreshBody();
      block.setData("blockType", blockConfig.type);
      block.setData("content", blockConfig.content ?? null);
      block.setData("multiCoinHits", blockConfig.multiCoinHits ?? 0);
      block.setData("breakable", blockConfig.breakable ?? false);
      block.setData("used", false);
      block.setData("cooldownUntil", 0);
      if (blockConfig.type === "hidden") {
        block.setAlpha(0.02);
      }
    });
  }

  private buildCoins(): void {
    this.stage.coins.forEach((coinConfig) => {
      const coin = this.coins
        .create(coinConfig.x * TILE_SIZE + 2, coinConfig.y * TILE_SIZE + 2, "pickup-coin")
        .setOrigin(0, 0)
        .setDisplaySize(28, 28)
        .setDepth(DEPTHS.pickups);
      coin.refreshBody();
      this.tweens.add({
        targets: coin,
        y: coin.y - 6,
        duration: 560,
        yoyo: true,
        repeat: -1,
        ease: "Sine.InOut",
      });
    });
  }

  private buildEnemies(): void {
    this.stage.enemies.forEach((enemyConfig) => {
      const enemy = this.enemies.create(
        enemyConfig.x * TILE_SIZE,
        enemyConfig.y * TILE_SIZE - 4,
        getEnemyFrameKey(enemyConfig.variant, "a"),
      ) as EnemySprite;
      const enemyBody = enemy.body as Phaser.Physics.Arcade.Body;
      enemy.setData("variant", enemyConfig.variant);
      enemy.setData("speed", enemyConfig.variant === "goomba" ? 42 : 54);
      enemy.setDepth(DEPTHS.actors);
      enemy.setDisplaySize(34, 34);
      enemy.setBounce(0);
      enemy.setCollideWorldBounds(false);
      enemyBody.setSize(24, 20).setOffset(2, 8);
      enemy.setVelocityX(-(enemy.getData("speed") as number));
    });
  }

  private buildMovingPlatforms(): void {
    const texture = getTerrainTextureKey(this.stage.theme);
    this.stage.movingPlatforms?.forEach((platformConfig) => {
      const platform = this.movingPlatforms.create(
        platformConfig.x * TILE_SIZE,
        platformConfig.y * TILE_SIZE,
        texture,
      ) as MovingPlatformSprite;
      const body = platform.body as Phaser.Physics.Arcade.Body;
      const platformWidth = platformConfig.width * TILE_SIZE;
      platform.setOrigin(0, 0);
      platform.setDisplaySize(platformWidth, 24);
      platform.setDepth(DEPTHS.blocks);
      platform.setTint(0xf0d16d);
      platform.setData("speed", platformConfig.speed);
      platform.setData("left", platformConfig.left * TILE_SIZE);
      platform.setData("right", platformConfig.right * TILE_SIZE);
      body.setAllowGravity(false);
      body.setImmovable(true);
      body.setSize(platformWidth, 24);
      body.setOffset(0, 0);
      body.setVelocityX(platformConfig.speed);
    });
  }

  private buildFallingBlocks(): void {
    this.stage.fallingBlocks?.forEach((blockConfig) => {
      const fallingBlock = this.fallingBlocks.create(
        blockConfig.x * TILE_SIZE,
        blockConfig.y * TILE_SIZE,
        "block-brick",
      ) as FallingBlockSprite;
      const body = fallingBlock.body as Phaser.Physics.Arcade.Body;
      fallingBlock.setOrigin(0, 0);
      fallingBlock.setDisplaySize(TILE_SIZE, TILE_SIZE);
      fallingBlock.setDepth(DEPTHS.blocks);
      fallingBlock.setTint(0xb18a56);
      fallingBlock.setData("contactStartedAt", 0);
      fallingBlock.setData("lastContactAt", 0);
      fallingBlock.setData("falling", false);
      body.setAllowGravity(false);
      body.setImmovable(true);
      body.setSize(TILE_SIZE, TILE_SIZE);
      body.setOffset(0, 0);
    });
  }

  private buildGoal(): void {
    const goalX = this.stage.goalX * TILE_SIZE;
    const marker = this.add.image(goalX, GAME_HEIGHT - 134, "goal-marker").setOrigin(0, 0).setDepth(DEPTHS.blocks);
    marker.setDisplaySize(40, 84);

    this.goal = this.add.zone(goalX, GAME_HEIGHT - 124, 40, 108).setOrigin(0, 0);
    this.physics.add.existing(this.goal, true);
  }

  private buildPlayer(): void {
    const state = GameState.getState();
    this.player = this.physics.add.sprite(this.stage.playerStartX, GAME_HEIGHT - 98, getPlayerTextureKey(state.form, "idle"));
    this.player.setCollideWorldBounds(false);
    this.player.setDepth(DEPTHS.actors);
    this.applyForm(state.form, true);
  }

  private buildHud(): void {
    this.add.image(0, 0, "hud-panel").setOrigin(0, 0).setScrollFactor(0).setDepth(DEPTHS.hud);
    this.hudText = this.add
      .text(28, 18, "", {
        fontFamily: FONT_FAMILY,
        fontSize: "16px",
        color: "#f7f1d1",
        lineSpacing: 10,
      })
      .setScrollFactor(0)
      .setDepth(DEPTHS.hud + 1);

    this.tallyText = this.add
      .text(GAME_WIDTH / 2, 92, "", {
        fontFamily: FONT_FAMILY,
        fontSize: "15px",
        color: "#fff0ad",
        backgroundColor: "#000000",
        padding: { left: 10, right: 10, top: 6, bottom: 6 },
      })
      .setOrigin(0.5)
      .setScrollFactor(0)
      .setDepth(DEPTHS.overlay)
      .setVisible(false);

    this.updateHud();
  }

  private buildInput(): void {
    const keyboard = this.input.keyboard;
    if (!keyboard) {
      throw new Error("Keyboard input is not available in GameScene.");
    }

    keyboard.enabled = true;
    keyboard.addCapture([
      Phaser.Input.Keyboard.KeyCodes.LEFT,
      Phaser.Input.Keyboard.KeyCodes.RIGHT,
      Phaser.Input.Keyboard.KeyCodes.UP,
      Phaser.Input.Keyboard.KeyCodes.DOWN,
      Phaser.Input.Keyboard.KeyCodes.SPACE,
      Phaser.Input.Keyboard.KeyCodes.SHIFT,
      Phaser.Input.Keyboard.KeyCodes.J,
      Phaser.Input.Keyboard.KeyCodes.K,
      Phaser.Input.Keyboard.KeyCodes.Z,
      Phaser.Input.Keyboard.KeyCodes.X,
      Phaser.Input.Keyboard.KeyCodes.P,
      Phaser.Input.Keyboard.KeyCodes.ENTER,
      Phaser.Input.Keyboard.KeyCodes.ESC,
      Phaser.Input.Keyboard.KeyCodes.W,
      Phaser.Input.Keyboard.KeyCodes.A,
      Phaser.Input.Keyboard.KeyCodes.S,
      Phaser.Input.Keyboard.KeyCodes.D,
    ]);

    this.cursors = keyboard.createCursorKeys();
    this.keyRun = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SHIFT);
  this.keyRunAlt = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.J);
    this.keyJump = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.Z);
  this.keyJumpAlt = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.K);
    this.keyFire = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.X);
    this.keyPause = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.P);
  this.keyPauseAlt = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
  this.keyMenuConfirm = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
    this.keyA = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.A);
    this.keyD = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.D);
    this.keyS = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.S);
    this.keyW = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.W);

    this.scale.canvas.setAttribute("tabindex", "0");
    this.scale.canvas.focus();
    this.input.on("pointerdown", () => this.scale.canvas.focus());
  }

  private buildPauseOverlay(): void {
    const backing = this.add.rectangle(GAME_WIDTH / 2, GAME_HEIGHT / 2, 320, 120, 0x000000, 0.8);
    const label = this.add.text(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 10, "PAUSED", {
      fontFamily: FONT_FAMILY,
      fontSize: "24px",
      color: "#f7f1d1",
    }).setOrigin(0.5);
    const hint = this.add.text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 62, "ESC/P RESUME   ENTER SELECT", {
      fontFamily: FONT_FAMILY,
      fontSize: "12px",
      color: "#f0db85",
    }).setOrigin(0.5);

    this.pauseMenuTexts = PAUSE_MENU_OPTIONS.map((entry, index) => {
      const option = this.add.text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + index * 22 + 6, entry, {
        fontFamily: FONT_FAMILY,
        fontSize: "14px",
        color: "#f7f1d1",
      }).setOrigin(0.5);
      option.setInteractive({ useHandCursor: true });
      option.on("pointerdown", () => {
        this.pauseMenuSelection = index;
        this.renderPauseMenuSelection();
        this.activatePauseAction();
      });
      return option;
    });

    this.pauseOverlay = this.add.container(0, 0, [backing, label, ...this.pauseMenuTexts, hint]).setDepth(DEPTHS.overlay).setScrollFactor(0);
    this.pauseOverlay.setVisible(false);
    this.renderPauseMenuSelection();
  }

  private bindPhysics(): void {
    this.physics.add.collider(this.player, this.terrain);
    this.physics.add.collider(this.player, this.blocks, this.onPlayerBlockCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.collider(this.player, this.movingPlatforms, this.onPlayerMovingPlatformCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.collider(this.player, this.fallingBlocks, this.onPlayerFallingBlockCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.collider(this.enemies, this.terrain, this.onEnemyCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.collider(this.enemies, this.blocks, this.onEnemyCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.collider(this.pickups, this.terrain);
    this.physics.add.collider(this.pickups, this.blocks);
    this.physics.add.collider(this.pickups, this.movingPlatforms);
    this.physics.add.collider(this.pickups, this.fallingBlocks);
    this.physics.add.collider(this.fireballs, this.terrain, this.onFireballTerrainCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.collider(this.fireballs, this.blocks, this.onFireballTerrainCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.collider(this.fireballs, this.movingPlatforms, this.onFireballTerrainCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.collider(this.fireballs, this.fallingBlocks, this.onFireballTerrainCollide as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);

    this.physics.add.overlap(this.player, this.coins, this.onPlayerCoinOverlap as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.overlap(this.player, this.enemies, this.onPlayerEnemyOverlap as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.overlap(this.player, this.pickups, this.onPlayerPickupOverlap as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.overlap(this.player, this.goal, this.onPlayerGoalOverlap as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.overlap(this.player, this.fireballs, this.onPlayerFireballOverlap as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
    this.physics.add.overlap(this.fireballs, this.enemies, this.onFireballEnemyOverlap as Phaser.Types.Physics.Arcade.ArcadePhysicsCallback, undefined, this);
  }

  private handleMovement(time: number, delta: number): void {
    const body = this.player.body as Phaser.Physics.Arcade.Body;
    const dt = delta / 1000;
    const left = this.cursors.left.isDown || this.keyA.isDown;
    const right = this.cursors.right.isDown || this.keyD.isDown;
    const down = this.cursors.down.isDown || this.keyS.isDown;
    const up = this.cursors.up.isDown || this.keyW.isDown;
    const running = this.keyRun.isDown || this.keyRunAlt.isDown;
    const onGround = body.blocked.down || body.touching.down;
    const crouching = onGround && down && Math.abs(body.velocity.x) < 22;
    const moveDir = left === right ? 0 : left ? -1 : 1;
    const maxSpeed = running ? PLAYER_TUNING.maxRunSpeed : PLAYER_TUNING.maxWalkSpeed;

    if (onGround) {
      this.jumpCountSinceGrounded = 0;
    }

    if (moveDir !== 0 && !crouching) {
      this.playerFacing = moveDir as -1 | 1;
      if (onGround && Math.sign(body.velocity.x) !== 0 && Math.sign(body.velocity.x) !== moveDir && Math.abs(body.velocity.x) > PLAYER_TUNING.skidThreshold) {
        const skidDelta = PLAYER_TUNING.skidDrag * dt;
        body.velocity.x = moveTowards(body.velocity.x, 0, skidDelta);
      } else {
        const accel = onGround ? (running ? PLAYER_TUNING.runAcceleration : PLAYER_TUNING.groundAcceleration) : PLAYER_TUNING.airAcceleration;
        body.velocity.x += moveDir * accel * dt;
      }
    } else if (onGround) {
      body.velocity.x = moveTowards(body.velocity.x, 0, PLAYER_TUNING.groundDrag * dt);
    }

    body.velocity.x = Phaser.Math.Clamp(body.velocity.x, -maxSpeed, maxSpeed);

    const jumpPressed = Phaser.Input.Keyboard.JustDown(this.keyJump)
      || Phaser.Input.Keyboard.JustDown(this.keyJumpAlt)
      || Phaser.Input.Keyboard.JustDown(this.cursors.space!)
      || Phaser.Input.Keyboard.JustDown(this.keyW)
      || Phaser.Input.Keyboard.JustDown(this.cursors.up!);

    if (jumpPressed && !crouching) {
      if (onGround) {
        body.velocity.y = PLAYER_TUNING.jumpVelocity;
        this.jumpCountSinceGrounded = 1;
        this.audio.playSfx("jump", { volume: 0.35 });
        this.jumpStartedAt = time;
        this.stompChain = 0;
      } else if (this.jumpCountSinceGrounded === 1) {
        body.velocity.y = PLAYER_TUNING.followUpJumpVelocity;
        this.jumpCountSinceGrounded = 2;
        this.audio.playSfx("jump", { volume: 0.35 });
        this.jumpStartedAt = time;
      }
    }

    if (Phaser.Input.Keyboard.JustDown(this.keyFire)) {
      this.fireProjectile(time);
    }

    const jumpHeld = this.keyJump.isDown || this.keyJumpAlt.isDown || this.cursors.space!.isDown || up;
    if (body.velocity.y < 0 && !jumpHeld && time - this.jumpStartedAt < PLAYER_TUNING.jumpHoldGraceMs) {
      body.setGravityY(1450);
    } else {
      body.setGravityY(0);
    }

    body.velocity.y = Math.min(body.velocity.y, PLAYER_TUNING.maxFallSpeed);
    this.player.setFlipX(this.playerFacing < 0);
  }

  private updatePauseMenu(): void {
    if (Phaser.Input.Keyboard.JustDown(this.cursors.up!) || Phaser.Input.Keyboard.JustDown(this.keyW)) {
      this.pauseMenuSelection = (this.pauseMenuSelection + PAUSE_MENU_OPTIONS.length - 1) % PAUSE_MENU_OPTIONS.length;
      this.renderPauseMenuSelection();
    }

    if (Phaser.Input.Keyboard.JustDown(this.cursors.down!) || Phaser.Input.Keyboard.JustDown(this.keyS)) {
      this.pauseMenuSelection = (this.pauseMenuSelection + 1) % PAUSE_MENU_OPTIONS.length;
      this.renderPauseMenuSelection();
    }

    if (Phaser.Input.Keyboard.JustDown(this.keyMenuConfirm) || Phaser.Input.Keyboard.JustDown(this.keyJump) || Phaser.Input.Keyboard.JustDown(this.keyJumpAlt) || Phaser.Input.Keyboard.JustDown(this.cursors.space!)) {
      this.activatePauseAction();
    }
  }

  private renderPauseMenuSelection(): void {
    this.pauseMenuTexts.forEach((entry, index) => {
      entry.setColor(index === this.pauseMenuSelection ? "#fff0ad" : "#f7f1d1");
      entry.setScale(index === this.pauseMenuSelection ? 1.08 : 1);
    });
  }

  private activatePauseAction(): void {
    const selected = ["resume", "restart", "title"] as const;
    const action = selected[this.pauseMenuSelection];

    if (action === "resume") {
      this.togglePause();
      return;
    }

    this.paused = false;
    this.audio.stopMusic();
    this.physics.resume();
    this.pauseOverlay?.setVisible(false);

    if (action === "restart") {
      this.scene.start(SCENES.startCard, { stageId: this.stage.id, restart: false });
      return;
    }

    this.scene.start(SCENES.title);
  }

  private updatePlayerPose(time: number): void {
    const body = this.player.body as Phaser.Physics.Arcade.Body;
    const onGround = body.blocked.down || body.touching.down;
    const downHeld = this.cursors.down.isDown || this.keyS.isDown;
    const state = GameState.getState();

    let pose = "idle";
    if (!onGround) {
      pose = body.velocity.y < 0 ? "jump" : "fall";
    } else if (downHeld && Math.abs(body.velocity.x) < 22) {
      pose = "crouch";
    } else if (Math.abs(body.velocity.x) > 26) {
      pose = this.runAnimFrame ? "runA" : "runB";
      if (time % 120 < 18) {
        this.runAnimFrame = !this.runAnimFrame;
      }
    }

    this.player.setTexture(getPlayerTextureKey(state.form, pose as "idle" | "runA" | "runB" | "jump" | "fall" | "crouch"));

    if (time < this.starUntil) {
      this.player.setTint(time % 180 < 90 ? 0xffef9a : 0xffffff);
    } else if (time < this.invulnerableUntil) {
      this.player.setAlpha(time % 120 < 60 ? 0.45 : 1);
      this.player.clearTint();
    } else {
      this.player.setAlpha(1);
      this.player.clearTint();
    }
  }

  private updateEnemies(time: number): void {
    this.enemies.children.iterate((entry) => {
      if (!entry) {
        return true;
      }

      const enemy = entry as EnemySprite;
      const body = enemy.body as Phaser.Physics.Arcade.Body;
      if (!enemy.active || !body) {
        return true;
      }

      const speed = enemy.getData("speed") as number;
      const variant = enemy.getData("variant") as "goomba" | "beetle";
      if ((body.blocked.down || body.touching.down) && !this.hasSupportAhead(enemy)) {
        body.velocity.x = body.velocity.x >= 0 ? -speed : speed;
      }

      if (body.blocked.left) {
        body.velocity.x = speed;
      } else if (body.blocked.right) {
        body.velocity.x = -speed;
      }

      const animFrame = Math.abs(body.velocity.x) > 10 ? (time % 240 < 120 ? "a" : "b") : "rest";
      enemy.setTexture(getEnemyFrameKey(variant, animFrame));
      enemy.setFlipX(body.velocity.x > 0);

      if (enemy.y > GAME_HEIGHT + 80) {
        enemy.destroy();
      }
      return true;
    });
  }

  private updatePickups(): void {
    this.pickups.children.iterate((entry) => {
      if (!entry) {
        return true;
      }

      const pickup = entry as PickupSprite;
      const body = pickup.body as Phaser.Physics.Arcade.Body;
      if (!pickup.active || !body) {
        return true;
      }

      if (pickup.getData("emerging")) {
        return true;
      }

      const pickupType = pickup.getData("pickupType") as PickupType;
      if (pickupType === "Fire Flower") {
        body.velocity.x = 0;
      } else if (pickupType === "Super Star") {
        if (body.blocked.down && body.velocity.y >= 0) {
          body.velocity.y = -320;
        }
        if (body.blocked.left) {
          body.velocity.x = 85;
        } else if (body.blocked.right) {
          body.velocity.x = -85;
        }
      } else {
        if (body.blocked.left) {
          body.velocity.x = 65;
        } else if (body.blocked.right) {
          body.velocity.x = -65;
        }
      }
      return true;
    });
  }

  private updateFireballs(): void {
    this.fireballs.children.iterate((entry) => {
      if (!entry) {
        return true;
      }

      const fireball = entry as PickupSprite;
      const body = fireball.body as Phaser.Physics.Arcade.Body;
      if (!fireball.active || !body) {
        return true;
      }

      if (body.blocked.down && body.velocity.y >= 0) {
        body.velocity.y = PLAYER_TUNING.fireballBounceVelocity;
      }
      if (body.blocked.left || body.blocked.right || body.blocked.up || fireball.y > GAME_HEIGHT + 80) {
        fireball.destroy();
      }
      return true;
    });
  }

  private updateTimer(delta: number): void {
    this.timerRemaining = Math.max(0, this.timerRemaining - delta / 1000);
    if (!this.hurryStateEntered && this.timerRemaining < 100) {
      this.hurryStateEntered = true;
    }
    if (this.timerRemaining <= 0) {
      this.loseLife();
    }
  }

  private getStageMusicKey(): MusicKey {
    const musicByTheme: Record<ThemeId, MusicKey> = {
      overworld: "overworld",
      underground: "underground",
      athletic_sky: "athletic-sky",
      castle: "castle",
    };

    return musicByTheme[this.stage.theme];
  }

  private syncMusicState(): void {
    if (this.playerDead || this.stageComplete) {
      return;
    }

    if (this.time.now < this.starUntil) {
      this.audio.playMusic("invincibility");
      return;
    }

    if (this.hurryStateEntered) {
      this.audio.playMusic("hurry");
      return;
    }

    this.audio.playMusic(this.getStageMusicKey());
  }

  private updateCamera(): void {
    const desired = Phaser.Math.Clamp(this.player.x - 240, 0, this.stage.widthTiles * TILE_SIZE - GAME_WIDTH);
    this.cameraProgressX = Math.max(this.cameraProgressX, desired);
    this.cameras.main.scrollX = this.cameraProgressX;
  }

  private updateHud(): void {
    const state = GameState.getState();
    const score = state.score.toString().padStart(6, "0");
    const coins = state.coins.toString().padStart(2, "0");
    const time = Math.ceil(this.timerRemaining).toString().padStart(3, "0");

    this.hudText.setText(
      `SCORE  COINS  WORLD  TIME\n${score}   ${coins}     ${state.stageId}    ${time}`,
    );
  }

  private onPlayerCoinOverlap(_: any, coinObj: any): void {
    const coin = coinObj as Phaser.Physics.Arcade.Sprite;
    if (!coin.active) {
      return;
    }

    coin.destroy();
    const gainedLife = GameState.addCoins(1);
    this.audio.playSfx("coin", { volume: 0.3 });
    if (gainedLife) {
      this.audio.playSfx("extra-life", { volume: 0.36 });
    }
    this.spawnFloatText(coin.x, coin.y, "200");
  }

  private onPlayerEnemyOverlap(playerObj: any, enemyObj: any): void {
    const player = playerObj as Phaser.Physics.Arcade.Sprite;
    const enemy = enemyObj as EnemySprite;
    if (!enemy.active || this.playerDead || this.stageComplete) {
      return;
    }

    const playerBody = player.body as Phaser.Physics.Arcade.Body;
    const enemyBody = enemy.body as Phaser.Physics.Arcade.Body;
    const time = this.time.now;

    if (time < this.starUntil) {
      this.defeatEnemy(enemy, 1000);
      return;
    }

    if (playerBody.velocity.y > 80 && playerBody.bottom <= enemyBody.top + 14) {
      if ((enemy.getData("variant") as "goomba" | "beetle") === "beetle") {
        this.damagePlayer();
        return;
      }

      playerBody.velocity.y = -330;
      this.audio.playSfx("stomp", { volume: 0.2 });
      if (time - this.lastEnemyStompAt > 1000) {
        this.stompChain = 0;
      }
      const comboScore = SCORE_VALUES.stompCombo[Math.min(this.stompChain, SCORE_VALUES.stompCombo.length - 1)];
      this.stompChain += 1;
      this.lastEnemyStompAt = time;
      this.defeatEnemy(enemy, comboScore);
      return;
    }

    this.damagePlayer();
  }

  private onPlayerPickupOverlap(_: any, pickupObj: any): void {
    const pickup = pickupObj as PickupSprite;
    if (!pickup.active || pickup.getData("emerging")) {
      return;
    }

    const pickupType = pickup.getData("pickupType") as PickupType;
    pickup.destroy();

    const state = GameState.getState();
    if (pickupType === "Coin") {
      const gainedLife = GameState.addCoins(1);
      this.audio.playSfx("coin");
      if (gainedLife) {
        this.audio.playSfx("extra-life", { volume: 0.36 });
      }
      return;
    }

    if (pickupType === "Mushroom") {
      if (state.form === "Small") {
        this.applyForm("Super");
      } else {
        GameState.addScore(SCORE_VALUES.duplicatePowerup);
      }
    } else if (pickupType === "Fire Flower") {
      if (state.form === "Fire") {
        GameState.addScore(SCORE_VALUES.duplicatePowerup);
      } else if (state.form === "Small") {
        this.applyForm("Super");
      } else {
        this.applyForm("Fire");
      }
    } else if (pickupType === "Super Star") {
      this.starUntil = this.time.now + PLAYER_TUNING.starInvulnerabilityMs;
      GameState.addScore(SCORE_VALUES.standardPowerup);
      this.syncMusicState();
    } else if (pickupType === "1-Up Mushroom") {
      GameState.addLife();
      this.audio.playSfx("extra-life", { volume: 0.38 });
      this.spawnFloatText(pickup.x, pickup.y, "1UP", "#8dfc8b");
    }

    if (pickupType !== "1-Up Mushroom") {
      GameState.addScore(SCORE_VALUES.standardPowerup);
      this.spawnFloatText(pickup.x, pickup.y, "1000");
    }
    this.audio.playSfx("power-up", { volume: 0.42 });
  }

  private onPlayerGoalOverlap(): void {
    if (this.stageComplete || this.playerDead) {
      return;
    }

    this.stageComplete = true;
    this.physics.pause();
    const awarded = GameState.applyTimeBonus(this.timerRemaining);
    this.timerRemaining = Math.ceil(this.timerRemaining);
    this.updateHud();
    this.audio.playSfx("stage-clear", { volume: 0.36 });
    this.audio.stopMusic();

    this.tallyText.setText(`TIME BONUS ${awarded}`);
    this.tallyText.setVisible(true);

    this.time.delayedCall(1200, () => {
      const stageIndex = STAGE_ORDER.indexOf(this.stage.id);
      if (stageIndex === STAGE_ORDER.length - 1) {
        this.scene.start(SCENES.worldClear);
        return;
      }

      const nextStage = STAGE_ORDER[stageIndex + 1];
      this.scene.start(SCENES.startCard, { stageId: nextStage, restart: false });
    });
  }

  private onPlayerBlockCollide(playerObj: any, blockObj: any): void {
    const player = playerObj as Phaser.Physics.Arcade.Sprite;
    const block = blockObj as BlockSprite;
    const body = player.body as Phaser.Physics.Arcade.Body;
    const now = this.time.now;
    const cooldownUntil = block.getData("cooldownUntil") as number;

    if (body.velocity.y < -80 && player.y > block.y && now >= cooldownUntil) {
      block.setData("cooldownUntil", now + 160);
      this.resolveBlockHit(block);
    }
  }

  private onPlayerMovingPlatformCollide(playerObj: any, platformObj: any): void {
    const player = playerObj as Phaser.Physics.Arcade.Sprite;
    const platform = platformObj as MovingPlatformSprite;
    const playerBody = player.body as Phaser.Physics.Arcade.Body;
    const platformBody = platform.body as Phaser.Physics.Arcade.Body;
    if (playerBody.bottom <= platformBody.top + 12 && playerBody.velocity.y >= 0) {
      const deltaX = platformBody.deltaX();
      if (Math.abs(deltaX) > 0.01) {
        player.x += deltaX;
      }
    }
  }

  private onPlayerFallingBlockCollide(playerObj: any, blockObj: any): void {
    const player = playerObj as Phaser.Physics.Arcade.Sprite;
    const block = blockObj as FallingBlockSprite;
    const playerBody = player.body as Phaser.Physics.Arcade.Body;
    const blockBody = block.body as Phaser.Physics.Arcade.Body;
    if ((block.getData("falling") as boolean) || playerBody.bottom > blockBody.top + 12 || playerBody.velocity.y < 0) {
      return;
    }

    const now = this.time.now;
    if ((block.getData("contactStartedAt") as number) === 0) {
      block.setData("contactStartedAt", now);
    }
    block.setData("lastContactAt", now);
  }

  private onEnemyCollide(enemyObj: any): void {
    const enemy = enemyObj as EnemySprite;
    const body = enemy.body as Phaser.Physics.Arcade.Body;
    if (body.blocked.left) {
      body.velocity.x = enemy.getData("speed") as number;
    } else if (body.blocked.right) {
      body.velocity.x = -(enemy.getData("speed") as number);
    }
  }

  private onFireballTerrainCollide(fireballObj: any): void {
    const fireball = fireballObj as PickupSprite;
    const body = fireball.body as Phaser.Physics.Arcade.Body;
    if (body.blocked.left || body.blocked.right || body.blocked.up) {
      fireball.destroy();
    }
  }

  private onFireballEnemyOverlap(fireballObj: any, enemyObj: any): void {
    const fireball = fireballObj as PickupSprite;
    const enemy = enemyObj as EnemySprite;
    if (!fireball.active || !enemy.active) {
      return;
    }

    if (fireball.getData("hostile")) {
      return;
    }

    if ((enemy.getData("variant") as "goomba" | "beetle") === "beetle") {
      this.reflectProjectile(fireball, enemy);
      return;
    }

    fireball.destroy();
    this.defeatEnemy(enemy, 1000);
  }

  private onPlayerFireballOverlap(playerObj: any, fireballObj: any): void {
    const player = playerObj as Phaser.Physics.Arcade.Sprite;
    const fireball = fireballObj as PickupSprite;
    if (!player.active || !fireball.active || !fireball.getData("hostile")) {
      return;
    }

    fireball.destroy();
    this.damagePlayer();
  }

  private resolveBlockHit(block: BlockSprite): void {
    const blockType = block.getData("blockType") as string;
    const content = block.getData("content") as PickupType | "Coin" | null;
    const multiCoinHits = block.getData("multiCoinHits") as number;
    const used = block.getData("used") as boolean;

    this.audio.playSfx("bump", { volume: 0.26 });
    this.tweens.add({
      targets: block,
      y: block.y - 6,
      duration: 70,
      yoyo: true,
      ease: "Quad.Out",
      onComplete: () => block.refreshBody(),
    });

    this.bumpActorsAbove(block);

    if (blockType === "brick") {
      if (GameState.getState().form !== "Small" && (block.getData("breakable") as boolean)) {
        GameState.addScore(SCORE_VALUES.brickBreak);
        this.audio.playSfx("brick-break", { volume: 0.24 });
        this.spawnFloatText(block.x + 16, block.y, "50");
        block.destroy();
      }
      return;
    }

    if (blockType === "hidden") {
      block.setAlpha(1);
      block.setTexture("block-question");
      block.setData("blockType", "question");
    }

    if (used) {
      return;
    }

    if (multiCoinHits > 0) {
      GameState.addCoins(1);
      this.audio.playSfx("coin", { volume: 0.28 });
      this.spawnPickupBurst(block.x + 16, block.y);
      const nextHits = multiCoinHits - 1;
      block.setData("multiCoinHits", nextHits);
      if (nextHits <= 0) {
        block.setTexture("block-used");
        block.setData("used", true);
      }
      return;
    }

    if (!content) {
      block.setTexture("block-used");
      block.setData("used", true);
      return;
    }

    if (content === "Coin") {
      GameState.addCoins(1);
      this.audio.playSfx("coin", { volume: 0.28 });
      this.spawnPickupBurst(block.x + 16, block.y);
    } else {
      this.spawnItemFromBlock(block, content);
    }

    block.setTexture("block-used");
    block.setData("used", true);
  }

  private bumpActorsAbove(block: BlockSprite): void {
    this.enemies.children.iterate((entry) => {
      if (!entry) {
        return true;
      }

      const enemy = entry as EnemySprite;
      if (!enemy.active) {
        return true;
      }

      if (Math.abs(enemy.x - (block.x + 16)) < 24 && enemy.y > block.y - 38 && enemy.y < block.y) {
        this.defeatEnemy(enemy, 100);
      }
      return true;
    });
  }

  private spawnItemFromBlock(block: BlockSprite, pickupType: PickupType): void {
    const textureMap: Record<PickupType, string> = {
      Coin: "pickup-coin",
      Mushroom: "pickup-mushroom",
      "Fire Flower": "pickup-fire-flower",
      "Super Star": "pickup-star",
      "1-Up Mushroom": "pickup-1up",
    };

    const pickup = this.pickups.create(block.x + 16, block.y + 20, textureMap[pickupType]) as PickupSprite;
    const pickupBody = pickup.body as Phaser.Physics.Arcade.Body;
    pickup.setData("pickupType", pickupType);
    pickup.setData("emerging", true);
    pickup.setDepth(DEPTHS.pickups);
    pickup.setDisplaySize(24, 24);
    pickupBody.setAllowGravity(false);
    pickupBody.setVelocity(0, 0);

    this.tweens.add({
      targets: pickup,
      y: block.y - 12,
      duration: 240,
      ease: "Sine.Out",
      onComplete: () => {
        pickup.setData("emerging", false);
        pickupBody.setAllowGravity(true);
        if (pickupType === "Mushroom" || pickupType === "1-Up Mushroom") {
          pickup.setVelocityX(65);
        } else if (pickupType === "Super Star") {
          pickup.setVelocity(85, -280);
          pickup.setBounce(1, 0.8);
        } else {
          pickup.setVelocityX(0);
        }
      },
    });
  }

  private fireProjectile(time: number): void {
    const state = GameState.getState();
    if (state.form !== "Fire" || time < this.fireCooldownUntil || this.fireballs.countActive(true) >= PLAYER_TUNING.fireballLimit) {
      return;
    }

    this.fireCooldownUntil = time + 220;
    const fireball = this.fireballs.create(this.player.x + this.playerFacing * 18, this.player.y - 18, "fireball") as PickupSprite;
    const fireballBody = fireball.body as Phaser.Physics.Arcade.Body;
    fireball.setDepth(DEPTHS.pickups);
    fireball.setDisplaySize(18, 18);
    fireballBody.setCircle(8, 2, 2);
    fireball.setData("hostile", false);
    fireball.setData("pickupType", "Coin");
    fireball.setVelocity(this.playerFacing * PLAYER_TUNING.fireballSpeed, -140);
    this.audio.playSfx("fire", { volume: 0.3 });
  }

  private applyForm(form: PlayerForm, initial = false): void {
    const current = GameState.getState().form;
    GameState.setForm(form);

    const body = this.player.body as Phaser.Physics.Arcade.Body;
    const previousBottom = this.player.getBottomCenter().y;
    if (form === "Small") {
      this.player.setTexture(getPlayerTextureKey("Small", "idle"));
      this.player.setDisplaySize(44, 44);
      body.setSize(18, 28);
      body.setOffset(0, 0);
    } else {
      this.player.setTexture(getPlayerTextureKey(form, "idle"));
      this.player.setDisplaySize(52, 52);
      body.setSize(20, 42);
      body.setOffset(0, 0);
    }

    if (!initial) {
      this.player.y = previousBottom;
      if (form !== current) {
        this.spawnFloatText(this.player.x, this.player.y - 44, form.toUpperCase(), "#f0db85");
      }
    }
  }

  private damagePlayer(): void {
    const now = this.time.now;
    if (now < this.invulnerableUntil || now < this.starUntil) {
      return;
    }

    const currentForm = GameState.getState().form;

    if (currentForm === "Fire") {
      this.audio.playSfx("power-down", { volume: 0.34 });
      this.applyForm("Super");
      this.invulnerableUntil = now + PLAYER_TUNING.damageInvulnerabilityMs;
      return;
    }

    if (currentForm === "Super") {
      this.audio.playSfx("power-down", { volume: 0.34 });
      this.applyForm("Small");
      this.invulnerableUntil = now + PLAYER_TUNING.damageInvulnerabilityMs;
      return;
    }

    this.loseLife();
  }

  private loseLife(): void {
    if (this.playerDead || this.stageComplete) {
      return;
    }

    this.playerDead = true;
    this.audio.stopMusic();
    this.audio.playSfx("death", { volume: 0.4 });
    this.physics.pause();
    this.player.setTint(0xffb7b7);

    this.time.delayedCall(900, () => {
      const lives = GameState.loseLife();
      if (lives > 0) {
        this.scene.start(SCENES.startCard, { stageId: this.stage.id, restart: true });
      } else {
        this.scene.start(SCENES.gameOver);
      }
    });
  }

  private defeatEnemy(enemy: EnemySprite, score: number): void {
    if (!enemy.active) {
      return;
    }

    GameState.addScore(score);
    this.audio.playSfx("enemy-defeat", { volume: 0.16 });
    this.spawnFloatText(enemy.x, enemy.y - 18, score >= 1000 ? score.toString() : `${score}`);
    const body = enemy.body as Phaser.Physics.Arcade.Body;
    body.enable = false;
    enemy.setActive(false);
    enemy.setFlipY(true);
    this.tweens.add({
      targets: enemy,
      y: enemy.y - 54,
      alpha: 0,
      duration: 420,
      ease: "Sine.In",
      onComplete: () => enemy.destroy(),
    });
  }

  private reflectProjectile(fireball: PickupSprite, enemy: EnemySprite): void {
    const fireballBody = fireball.body as Phaser.Physics.Arcade.Body;
    const incomingDirection = Math.sign(fireballBody.velocity.x) || this.playerFacing;
    fireball.destroy();

    const reflected = this.fireballs.create(enemy.x - incomingDirection * 18, enemy.y - 8, "fireball") as PickupSprite;
    const reflectedBody = reflected.body as Phaser.Physics.Arcade.Body;
    reflected.setDepth(DEPTHS.pickups);
    reflected.setDisplaySize(18, 18);
    reflected.setTint(0xffc37a);
    reflected.setData("hostile", true);
    reflected.setVelocity(-incomingDirection * PLAYER_TUNING.fireballSpeed * 0.85, -140);
    reflectedBody.setCircle(8, 2, 2);
    this.audio.playSfx("bump", { volume: 0.22 });
  }

  private spawnPickupBurst(x: number, y: number): void {
    const burst = this.add.image(x, y, "pickup-coin").setDepth(DEPTHS.pickups).setDisplaySize(20, 20);
    this.tweens.add({
      targets: burst,
      y: y - 36,
      alpha: 0,
      duration: 320,
      ease: "Sine.Out",
      onComplete: () => burst.destroy(),
    });
  }

  private spawnFloatText(x: number, y: number, text: string, color = "#fff0ad"): void {
    const label = this.add.text(x, y, text, {
      fontFamily: FONT_FAMILY,
      fontSize: "14px",
      color,
      stroke: "#000000",
      strokeThickness: 4,
    }).setOrigin(0.5).setDepth(DEPTHS.overlay);

    this.tweens.add({
      targets: label,
      y: y - 24,
      alpha: 0,
      duration: 500,
      ease: "Sine.Out",
      onComplete: () => label.destroy(),
    });
  }

  private togglePause(): void {
    if (this.playerDead || this.stageComplete) {
      return;
    }

    this.paused = !this.paused;
    if (this.paused) {
      this.audio.playSfx("pause", { volume: 0.24 });
      this.audio.pauseMusic();
      this.physics.pause();
      this.pauseMenuSelection = 0;
      this.renderPauseMenuSelection();
      this.pauseOverlay?.setVisible(true);
    } else {
      this.audio.playSfx("pause", { volume: 0.18 });
      this.audio.resumeMusic();
      this.physics.resume();
      this.pauseOverlay?.setVisible(false);
    }
  }

  private hasSupportAhead(enemy: EnemySprite): boolean {
    const body = enemy.body as Phaser.Physics.Arcade.Body;
    const direction = body.velocity.x >= 0 ? 1 : -1;
    const probeX = body.center.x + direction * (body.width / 2 + 6);
    const probeY = body.bottom + 6;
    return this.isSolidAtWorldPoint(probeX, probeY);
  }

  private isSolidAtWorldPoint(worldX: number, worldY: number): boolean {
    const tileSegments = [...this.stage.ground, ...(this.stage.ceilings ?? [])];
    const withinSegments = tileSegments.some((segment) => {
      const left = segment.x * TILE_SIZE;
      const top = segment.y * TILE_SIZE;
      const right = left + segment.width * TILE_SIZE;
      const bottom = top + segment.height * TILE_SIZE;
      return worldX >= left && worldX < right && worldY >= top && worldY < bottom;
    });

    if (withinSegments) {
      return true;
    }

    let blockSupport = false;
    this.blocks.children.iterate((entry) => {
      if (!entry || blockSupport) {
        return !blockSupport;
      }

      const block = entry as BlockSprite;
      if (!block.active) {
        return true;
      }

      blockSupport = worldX >= block.x && worldX < block.x + TILE_SIZE && worldY >= block.y && worldY < block.y + TILE_SIZE;
      return !blockSupport;
    });

    return blockSupport;
  }
}
