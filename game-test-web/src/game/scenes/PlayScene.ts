import Phaser from 'phaser';
import type { BrowserGameApp } from '../app';
import {
  BACKDROP_VISUALS,
  ENEMY_VISUALS,
  getEnemyTextureKey,
  getPlayerTextureKey,
  PLAYER_VISUALS,
  TERRAIN_VISUALS,
} from '../core/assets';
import {
  createGameplayInputMap,
  isActionDown,
  isActionJustPressed,
  type GameplayInputMap,
} from '../core/input';
import type {
  EnemyKind,
  FallingBlockDef,
  RectDef,
  RewardKind,
  StageDefinition,
  StageSummary,
} from '../core/types';
import { PLAYER_MOVEMENT, resolvePlayerPose } from '../sim/player';
import { getSpawnableEnemies, getStageCollisionRects, getStageDefinition } from '../sim/stages';

type ArcadeBody = Phaser.Physics.Arcade.Body;
type DynamicSprite = Phaser.Physics.Arcade.Sprite;
const PLAYER_SCALE = 0.48;

export class PlayScene extends Phaser.Scene {
  private readonly app: BrowserGameApp;

  private currentStage: StageDefinition | null = null;
  private player: DynamicSprite | null = null;
  private terrainGroup: Phaser.Physics.Arcade.StaticGroup | null = null;
  private blockGroup: Phaser.Physics.Arcade.StaticGroup | null = null;
  private coinGroup: Phaser.Physics.Arcade.Group | null = null;
  private cactusGroup: Phaser.Physics.Arcade.StaticGroup | null = null;
  private powerupGroup: Phaser.Physics.Arcade.Group | null = null;
  private enemyGroup: Phaser.Physics.Arcade.Group | null = null;
  private playerProjectiles: Phaser.Physics.Arcade.Group | null = null;
  private enemyProjectiles: Phaser.Physics.Arcade.Group | null = null;
  private goal: Phaser.Physics.Arcade.Image | null = null;

  private readonly movingPlatforms: DynamicSprite[] = [];
  private readonly fallingBlocks: DynamicSprite[] = [];
  private readonly stageDecor: Phaser.GameObjects.GameObject[] = [];
  private readonly stageColliders: Phaser.Physics.Arcade.Collider[] = [];
  private terrainCollisionRects: RectDef[] = [];

  private inputMap!: GameplayInputMap;

  private stageActive = false;
  private pausedByShell = false;
  private stageResolving = false;
  private stageTimeRemaining = 0;
  private playerInvulnerableUntil = 0;
  private jumpsUsed = 0;
  private facing = 1;
  private wasPlayerGrounded = false;
  private doubleJumpLandingArmed = false;
  private coyoteJumpExpiresAt = 0;
  private jumpBufferedUntil = 0;
  private lastAirborneVelocityY = 0;
  private ducking = false;

  constructor(app: BrowserGameApp) {
    super('play-scene');
    this.app = app;
  }

  preload(): void {
    Object.entries(PLAYER_VISUALS).forEach(([variant, poseSet]) => {
      Object.entries(poseSet).forEach(([pose, path]) => {
        this.load.image(`${variant}-${pose}`, path);
      });
    });

    Object.values(TERRAIN_VISUALS).forEach((asset) => {
      this.load.image(asset.key, asset.path);
    });

    Object.values(ENEMY_VISUALS).forEach((asset) => {
      this.load.image(asset.key, asset.path);
    });

    Object.values(BACKDROP_VISUALS).forEach((asset) => {
      this.load.image(asset.key, asset.path);
    });
  }

  create(): void {
    this.createTextures();
    this.setupInput();
    this.showBackdrop();
  }

  update(time: number, delta: number): void {
    this.handlePauseInput();

    if (!this.stageActive || this.pausedByShell || !this.player || !this.currentStage) {
      return;
    }

    const deltaSeconds = delta / 1000;

    this.updateMovingPlatforms(deltaSeconds);
    this.updateFallingBlocks(deltaSeconds);
    this.updatePlayer(time);
    this.updateEnemies(time, deltaSeconds);
    this.updateProjectiles();
    this.updateStageTimer(deltaSeconds);
    this.updateHud();
    this.checkPitFall();
  }

  startStage(stageId: string): void {
    const stage = getStageDefinition(stageId);

    this.clearStage();
    this.currentStage = stage;
    this.stageTimeRemaining = stage.timerSeconds;
    this.stageActive = true;
    this.stageResolving = false;
    this.pausedByShell = false;
    this.playerInvulnerableUntil = 0;
    this.jumpsUsed = 0;
    this.wasPlayerGrounded = false;
    this.doubleJumpLandingArmed = false;
    this.coyoteJumpExpiresAt = 0;
    this.jumpBufferedUntil = 0;
    this.lastAirborneVelocityY = 0;
    this.ducking = false;

    this.buildStageDecor(stage);
    this.buildTerrain(stage);
    this.buildBlocks(stage);
    this.buildCoins(stage);
    this.buildCactusHazards(stage);
    this.buildPowerups();
    this.buildProjectileGroups();
    this.buildMovingPlatforms(stage);
    this.buildFallingBlocks(stage);
    this.buildGoal(stage);
    this.buildPlayer(stage);
    this.buildEnemies(stage);
    this.setupCollisions();

    this.cameras.main.setBounds(
      stage.cameraBounds.left,
      stage.cameraBounds.top,
      stage.cameraBounds.right,
      stage.cameraBounds.bottom,
    );
    this.physics.world.setBounds(0, 0, stage.width, stage.height + 180);

    if (this.player) {
      this.cameras.main.startFollow(this.player, true, 0.12, 0.04, 96, 0);
      this.cameras.main.setDeadzone(360, 220);
      this.syncPlayerAppearance();
    }

    this.updateHud();
  }

  showBackdrop(): void {
    this.clearStage();
    this.cameras.main.stopFollow();
    this.cameras.main.setScroll(0, 0);
    this.cameras.main.setBackgroundColor('#b8e7ff');

    const sky = this.add.image(480, 270, BACKDROP_VISUALS.sky.key).setDisplaySize(960, 540);
    const clouds = this.add.image(500, 160, BACKDROP_VISUALS.clouds.key).setDisplaySize(900, 140).setAlpha(0.5);
    const mountains = this.add.image(470, 370, BACKDROP_VISUALS.mountains.key).setDisplaySize(980, 180).setAlpha(0.9);
    const hills = this.add.image(480, 450, BACKDROP_VISUALS.hills.key).setDisplaySize(980, 170);
    const treeLeft = this.add.image(160, 380, BACKDROP_VISUALS.tree.key).setDisplaySize(120, 180);
    const treeRight = this.add.image(790, 390, BACKDROP_VISUALS.tree.key).setDisplaySize(100, 150);

    this.stageDecor.push(sky, clouds, mountains, hills, treeLeft, treeRight);
  }

  setPaused(paused: boolean): void {
    this.pausedByShell = paused;

    if (paused) {
      this.physics.world.pause();
    } else {
      this.physics.world.resume();
    }

    this.app.audio.setPaused(paused);
    this.updateHud();
  }

  private setupInput(): void {
    const keyboard = this.input.keyboard;

    if (!keyboard) {
      throw new Error('Keyboard input is required for this build.');
    }

    this.inputMap = createGameplayInputMap(keyboard);
  }

  private createTextures(): void {
    if (this.textures.exists('solid-body')) {
      return;
    }

    const graphics = this.add.graphics();

    const createRect = (key: string, width: number, height: number, fill: number, stroke = 0xffffff) => {
      graphics.clear();
      graphics.fillStyle(fill, 1);
      graphics.fillRect(0, 0, width, height);
      graphics.lineStyle(2, stroke, 0.35);
      graphics.strokeRect(1, 1, width - 2, height - 2);
      graphics.generateTexture(key, width, height);
    };

    createRect('solid-body', 8, 8, 0xffffff, 0xffffff);
    createRect('platform-body', 8, 8, 0xffffff, 0xffffff);
    createRect('falling-body', 8, 8, 0xffffff, 0xffffff);
    createRect('mystery-block', 48, 48, 0xf0b03f, 0x4e2e09);
    createRect('coin', 22, 22, 0xffd85f, 0xfff9cc);
    createRect('goal', 26, 86, 0xf2f4f8, 0x111111);
    createRect('hostile-projectile', 16, 12, 0xff7f61, 0xffefe9);
    this.createMushroomTexture(graphics);
    this.createFlowerTexture(graphics);
    this.createFireTexture(graphics);

    this.createDuckPoseTexture('player');
    this.createDuckPoseTexture('adventurer');
    this.createDuckPoseTexture('soldier');

    graphics.destroy();
  }

  private createDuckPoseTexture(variant: string): void {
    const duckTextureKey = `${variant}-duck`;

    if (this.textures.exists(duckTextureKey)) {
      return;
    }

    const source = this.textures.get(`${variant}-idle`).getSourceImage() as HTMLImageElement;
    const width = source.width;
    const height = source.height;
    const canvas = this.textures.createCanvas(duckTextureKey, width, height);

    if (!canvas) {
      return;
    }

    const context = canvas.context;
    const cropTop = Math.floor(height * 0.16);
    const sourceHeight = height - cropTop;
    const drawHeight = Math.floor(height * 0.66);

    context.imageSmoothingEnabled = false;
    context.clearRect(0, 0, width, height);
    context.drawImage(
      source,
      0,
      cropTop,
      width,
      sourceHeight,
      0,
      height - drawHeight,
      width,
      drawHeight,
    );
    canvas.refresh();
  }

  private createMushroomTexture(graphics: Phaser.GameObjects.Graphics): void {
    const key = 'powerup-mushroom';

    if (this.textures.exists(key)) {
      return;
    }

    graphics.clear();
    graphics.fillStyle(0xf6e7cf, 1);
    graphics.fillRoundedRect(10, 14, 12, 14, 3);
    graphics.fillStyle(0xe05555, 1);
    graphics.fillEllipse(16, 12, 26, 16);
    graphics.fillStyle(0xfff6e8, 1);
    graphics.fillCircle(10, 10, 3);
    graphics.fillCircle(16, 8, 2.5);
    graphics.fillCircle(22, 11, 3);
    graphics.lineStyle(2, 0x47230f, 0.9);
    graphics.strokeEllipse(16, 12, 26, 16);
    graphics.strokeRoundedRect(10, 14, 12, 14, 3);
    graphics.generateTexture(key, 32, 32);
  }

  private createFlowerTexture(graphics: Phaser.GameObjects.Graphics): void {
    const key = 'powerup-flower';

    if (this.textures.exists(key)) {
      return;
    }

    graphics.clear();
    graphics.fillStyle(0x2f9d58, 1);
    graphics.fillRect(14, 14, 4, 14);
    graphics.fillStyle(0x69d36d, 1);
    graphics.fillEllipse(10, 22, 8, 5);
    graphics.fillEllipse(22, 22, 8, 5);
    graphics.fillStyle(0xfff0f2, 1);
    graphics.fillEllipse(16, 8, 8, 14);
    graphics.fillEllipse(16, 16, 8, 14);
    graphics.fillEllipse(8, 12, 14, 8);
    graphics.fillEllipse(24, 12, 14, 8);
    graphics.fillStyle(0xffc74f, 1);
    graphics.fillCircle(16, 12, 4);
    graphics.lineStyle(2, 0x5b2d0b, 0.85);
    graphics.strokeCircle(16, 12, 4);
    graphics.generateTexture(key, 32, 32);
  }

  private createFireTexture(graphics: Phaser.GameObjects.Graphics): void {
    const key = 'fire-projectile';

    if (this.textures.exists(key)) {
      return;
    }

    graphics.clear();
    graphics.fillStyle(0xff6b2d, 1);
    graphics.fillTriangle(4, 14, 20, 3, 14, 18);
    graphics.fillStyle(0xffc84a, 1);
    graphics.fillTriangle(7, 13, 17, 6, 13, 16);
    graphics.fillStyle(0xfff4ae, 1);
    graphics.fillTriangle(10, 12, 15, 8, 12, 14);
    graphics.lineStyle(2, 0xa63a16, 0.85);
    graphics.strokeTriangle(4, 14, 20, 3, 14, 18);
    graphics.generateTexture(key, 24, 20);
  }

  private buildStageDecor(stage: StageDefinition): void {
    this.cameras.main.setBackgroundColor('#b8e7ff');

    const sky = this.add.tileSprite(stage.width / 2, stage.height / 2, stage.width, stage.height, BACKDROP_VISUALS.sky.key);
    const clouds = this.add.tileSprite(stage.width / 2, 120, stage.width + 320, 130, BACKDROP_VISUALS.clouds.key);
    const mountains = this.add.tileSprite(stage.width / 2, 370, stage.width + 240, 170, BACKDROP_VISUALS.mountains.key);
    const hills = this.add.tileSprite(stage.width / 2, 455, stage.width + 220, 150, BACKDROP_VISUALS.hills.key);

    clouds.setAlpha(0.55);
    sky.setScrollFactor(0, 0);
    clouds.setScrollFactor(0.1, 0.02);
    mountains.setScrollFactor(0.24, 0.08);
    hills.setScrollFactor(0.38, 0.16);

    this.stageDecor.push(sky, clouds, mountains, hills);

    for (let x = 180; x < stage.width + 220; x += 620) {
      const tree = this.add.image(x, 402, BACKDROP_VISUALS.tree.key).setDisplaySize(118, 176);
      tree.setScrollFactor(0.46, 0.2);
      this.stageDecor.push(tree);
    }
  }

  private buildTerrain(stage: StageDefinition): void {
    this.terrainGroup = this.physics.add.staticGroup();
    this.terrainCollisionRects = getStageCollisionRects(stage);

    this.terrainCollisionRects.forEach((rect) => {
      const fill = this.add.tileSprite(
        rect.x + rect.width / 2,
        rect.y + rect.height / 2,
        rect.width,
        rect.height,
        TERRAIN_VISUALS.fill.key,
      );
      const topHeight = Math.min(18, rect.height);
      const top = this.add.tileSprite(
        rect.x + rect.width / 2,
        rect.y + topHeight / 2,
        rect.width,
        topHeight,
        TERRAIN_VISUALS.top.key,
      );
      fill.setOrigin(0.5, 0.5);
      top.setOrigin(0.5, 0.5);
      this.stageDecor.push(fill, top);

      const segment = this.terrainGroup?.create(
        rect.x + rect.width / 2,
        rect.y + rect.height / 2,
        'solid-body',
      ) as Phaser.Physics.Arcade.Sprite;

      segment.setDisplaySize(rect.width, rect.height);
      segment.setAlpha(0.01);
      segment.refreshBody();
    });
  }

  private buildBlocks(stage: StageDefinition): void {
    this.blockGroup = this.physics.add.staticGroup();

    stage.blocks.forEach((block) => {
      const blockSprite = this.blockGroup?.create(block.x, block.y, 'mystery-block') as DynamicSprite;
      blockSprite.setDataEnabled();
      blockSprite.setData('spent', false);
      blockSprite.setData('reward', block.reward);
      blockSprite.refreshBody();
    });
  }

  private buildCoins(stage: StageDefinition): void {
    this.coinGroup = this.physics.add.group({
      allowGravity: false,
      immovable: true,
    });

    stage.coins.forEach((coin) => {
      const coinSprite = this.coinGroup?.create(coin.x, coin.y, 'coin') as DynamicSprite;
      const body = coinSprite.body as ArcadeBody;
      body.setAllowGravity(false);
      body.setImmovable(true);
    });
  }

  private buildCactusHazards(stage: StageDefinition): void {
    this.cactusGroup = this.physics.add.staticGroup();

    stage.cactusHazards.forEach((hazard) => {
      const supportTop = this.findSupportTopForStaticHazard(hazard.x, hazard.y + 48);
      const resolvedY = supportTop ?? hazard.y + 28;
      const cactus = this.cactusGroup?.create(hazard.x, resolvedY, TERRAIN_VISUALS.cactus.key) as DynamicSprite;

      cactus.setOrigin(0.5, 1);
      cactus.setDisplaySize(48, 68);
      cactus.setDataEnabled();
      cactus.setData('hazardType', 'cactus');
      const body = cactus.body as Phaser.Physics.Arcade.StaticBody;
      body.setSize(34, 40);
      body.setOffset(7, 28);
      cactus.refreshBody();
    });
  }

  private buildPowerups(): void {
    this.powerupGroup = this.physics.add.group({
      allowGravity: false,
      immovable: true,
    });
  }

  private buildProjectileGroups(): void {
    this.playerProjectiles = this.physics.add.group({
      allowGravity: false,
    });
    this.enemyProjectiles = this.physics.add.group({
      allowGravity: false,
    });
  }

  private buildMovingPlatforms(stage: StageDefinition): void {
    stage.movingPlatforms.forEach((platformDef) => {
      const platform = this.physics.add.sprite(platformDef.x, platformDef.y, TERRAIN_VISUALS.top.key);
      platform.setDisplaySize(platformDef.width, platformDef.height);
      platform.setImmovable(true);
      platform.body.setAllowGravity(false);
      platform.body.setSize(platformDef.width, platformDef.height);
      platform.setTint(0xe7f7ff);
      platform.setDataEnabled();
      platform.setData('minX', platformDef.minX);
      platform.setData('maxX', platformDef.maxX);
      platform.setData('speed', platformDef.speed);
      platform.setData('direction', 1);
      platform.setData('lastX', platform.x);
      this.movingPlatforms.push(platform);
    });
  }

  private buildFallingBlocks(stage: StageDefinition): void {
    stage.fallingBlocks.forEach((blockDef: FallingBlockDef) => {
      const block = this.physics.add.sprite(blockDef.x, blockDef.y, TERRAIN_VISUALS.hazard.key);
      block.setDisplaySize(blockDef.width, blockDef.height);
      block.setImmovable(true);
      block.body.setAllowGravity(false);
      block.body.setSize(blockDef.width, blockDef.height);
      block.setDataEnabled();
      block.setData('contactMs', 0);
      block.setData('triggered', false);
      block.setData('fallSpeed', 0);
      this.fallingBlocks.push(block);
    });
  }

  private buildGoal(stage: StageDefinition): void {
    this.goal = this.physics.add.staticImage(stage.goal.x, stage.goal.y, 'goal');
    this.goal.setOrigin(0.5, 1);
    this.goal.refreshBody();
  }

  private buildPlayer(stage: StageDefinition): void {
    this.player = this.physics.add.sprite(stage.spawn.x, stage.spawn.y, this.playerPoseKey('idle'));
    const body = this.player.body as ArcadeBody;

    this.player.setScale(PLAYER_SCALE);
    body.setSize(42, 72);
    body.setOffset(19, 28);
    this.player.setCollideWorldBounds(false);
    this.player.setDragX(1700);
    this.player.setMaxVelocity(360, 900);
    this.player.setBounce(0);
    this.player.setDataEnabled();
  }

  private buildEnemies(stage: StageDefinition): void {
    this.enemyGroup = this.physics.add.group();

    getSpawnableEnemies(stage, this.app.session.difficulty).forEach((enemyDef) => {
      const enemy = this.enemyGroup?.create(enemyDef.x, enemyDef.y, this.textureForEnemy(enemyDef.kind)) as DynamicSprite;

      enemy.setDataEnabled();
      enemy.setData('kind', enemyDef.kind);
      enemy.setData('minX', enemyDef.patrolMin ?? enemyDef.x - 160);
      enemy.setData('maxX', enemyDef.patrolMax ?? enemyDef.x + 160);
      enemy.setData('baseY', enemyDef.y);
      enemy.setData('amplitude', enemyDef.amplitude ?? 24);
      enemy.setData('frequency', enemyDef.frequency ?? 0.004);
      enemy.setData('direction', -1);
      enemy.setData('fireDelayMs', enemyDef.fireDelayMs ?? 2200);
      enemy.setData('nextShotAt', 0);
      enemy.setData('defeated', false);
      const body = enemy.body as ArcadeBody;

      if (enemyDef.kind === 'Ground') {
        enemy.setScale(0.45);
        body.setSize(42, 70);
        body.setOffset(18, 30);
      } else {
        enemy.setScale(enemyDef.kind === 'Flying' ? 2.1 : 2.3);
        body.setSize(14, 14);
        body.setOffset(2, 2);
      }

      if (enemyDef.kind === 'Flying' || enemyDef.kind === 'Shooter') {
        body.setAllowGravity(false);
      } else {
        enemy.setGravityY(1180);
      }
    });
  }

  private setupCollisions(): void {
    if (!this.player) {
      return;
    }

    if (this.terrainGroup) {
      this.trackCollider(this.physics.add.collider(this.player, this.terrainGroup));
    }

    if (this.blockGroup) {
      this.trackCollider(
        this.physics.add.collider(
          this.player,
          this.blockGroup,
          (object1, object2) => this.handlePlayerBlockCollision(object1, object2),
          undefined,
          this,
        ),
      );
    }

    this.movingPlatforms.forEach((platform) => {
      this.trackCollider(this.physics.add.collider(this.player!, platform));
    });

    this.fallingBlocks.forEach((block) => {
      this.trackCollider(this.physics.add.collider(this.player!, block));
    });

    if (this.enemyGroup && this.terrainGroup) {
      this.trackCollider(this.physics.add.collider(this.enemyGroup, this.terrainGroup));
    }

    if (this.enemyGroup && this.blockGroup) {
      this.trackCollider(this.physics.add.collider(this.enemyGroup, this.blockGroup));
    }

    if (this.coinGroup) {
      this.trackCollider(
        this.physics.add.overlap(
          this.player,
          this.coinGroup,
          (object1, object2) => this.handleCoinPickup(object1, object2),
          undefined,
          this,
        ),
      );
    }

    if (this.cactusGroup) {
      this.trackCollider(
        this.physics.add.overlap(
          this.player,
          this.cactusGroup,
          (object1, object2) => this.handlePlayerCactusCollision(object1, object2),
          undefined,
          this,
        ),
      );
    }

    if (this.powerupGroup) {
      this.trackCollider(
        this.physics.add.overlap(
          this.player,
          this.powerupGroup,
          (object1, object2) => this.handlePowerupPickup(object1, object2),
          undefined,
          this,
        ),
      );
    }

    if (this.enemyGroup) {
      this.trackCollider(
        this.physics.add.overlap(
          this.player,
          this.enemyGroup,
          (object1, object2) => this.handlePlayerEnemyCollision(object1, object2),
          undefined,
          this,
        ),
      );
    }

    if (this.playerProjectiles && this.enemyGroup) {
      this.trackCollider(
        this.physics.add.overlap(
          this.playerProjectiles,
          this.enemyGroup,
          (object1, object2) => this.handlePlayerProjectileHitEnemy(object1, object2),
          undefined,
          this,
        ),
      );
    }

    if (this.enemyProjectiles) {
      this.trackCollider(
        this.physics.add.overlap(
          this.player,
          this.enemyProjectiles,
          (object1, object2) => this.handleHostileProjectileHitPlayer(object1, object2),
          undefined,
          this,
        ),
      );
    }

    if (this.goal) {
      this.trackCollider(
        this.physics.add.overlap(this.player, this.goal, this.handleGoalReached, undefined, this),
      );
    }
  }

  private handlePauseInput(): void {
    if (!this.stageActive || this.stageResolving) {
      return;
    }

    if (isActionJustPressed(this.inputMap, 'pause')) {
      this.app.onGameplayPauseToggle();
    }
  }

  private updatePlayer(time: number): void {
    if (!this.player) {
      return;
    }

    const body = this.player.body as ArcadeBody;
    const movingLeft = isActionDown(this.inputMap, 'moveLeft');
    const movingRight = isActionDown(this.inputMap, 'moveRight');
    const movingDown = isActionDown(this.inputMap, 'moveDown');
    const running = isActionDown(this.inputMap, 'action');
    const speed = running ? PLAYER_MOVEMENT.runSpeed : PLAYER_MOVEMENT.walkSpeed;
    const onFloor = body.blocked.down || body.touching.down;
    const jumpPressed = isActionJustPressed(this.inputMap, 'jump');

    if (!this.wasPlayerGrounded && onFloor && this.doubleJumpLandingArmed) {
      this.app.audio.playLand();
      this.doubleJumpLandingArmed = false;
    }

    if (
      !this.wasPlayerGrounded &&
      onFloor &&
      this.lastAirborneVelocityY >= PLAYER_MOVEMENT.landingShakeThreshold
    ) {
      const excessImpact = this.lastAirborneVelocityY - PLAYER_MOVEMENT.landingShakeThreshold;
      this.triggerImpactShake(
        110,
        Phaser.Math.Clamp(0.002 + excessImpact / 180000, 0.002, 0.006),
      );
    }

    if (onFloor) {
      this.jumpsUsed = 0;
      this.coyoteJumpExpiresAt = time + PLAYER_MOVEMENT.coyoteTimeMs;
      this.lastAirborneVelocityY = 0;
    }

    if (movingLeft === movingRight) {
      this.player.setAccelerationX(0);
      this.player.setVelocityX(
        body.velocity.x * (onFloor ? PLAYER_MOVEMENT.groundIdleDamping : PLAYER_MOVEMENT.airIdleDamping),
      );
    } else if (movingLeft) {
      this.player.setAccelerationX(onFloor ? -PLAYER_MOVEMENT.groundAcceleration : -PLAYER_MOVEMENT.airAcceleration);
      this.player.setMaxVelocity(speed, PLAYER_MOVEMENT.maxVerticalSpeed);
      this.facing = -1;
    } else if (movingRight) {
      this.player.setAccelerationX(onFloor ? PLAYER_MOVEMENT.groundAcceleration : PLAYER_MOVEMENT.airAcceleration);
      this.player.setMaxVelocity(speed, PLAYER_MOVEMENT.maxVerticalSpeed);
      this.facing = 1;
    }

    if (body.velocity.x > speed) {
      this.player.setVelocityX(speed);
    }

    if (body.velocity.x < -speed) {
      this.player.setVelocityX(-speed);
    }

    this.player.setFlipX(this.facing < 0);

    if (jumpPressed) {
      this.jumpBufferedUntil = time + PLAYER_MOVEMENT.jumpBufferMs;
    }

    const jumpBuffered = time <= this.jumpBufferedUntil;
    const canGroundJump = this.jumpsUsed === 0 && (onFloor || time <= this.coyoteJumpExpiresAt);
    const canDoubleJump = this.jumpsUsed === 1 && !onFloor;

    if (jumpBuffered && (canGroundJump || canDoubleJump)) {
      const doubleJump = canDoubleJump;

      this.player.setVelocityY(
        doubleJump ? PLAYER_MOVEMENT.doubleJumpVelocity : PLAYER_MOVEMENT.groundJumpVelocity,
      );
      this.jumpsUsed = doubleJump ? 2 : 1;
      this.jumpBufferedUntil = 0;
      this.coyoteJumpExpiresAt = 0;

      if (doubleJump) {
        this.doubleJumpLandingArmed = true;
        this.app.audio.playDoubleJump();
      } else {
        this.app.audio.playJump();
      }
    }

    if (body.velocity.y < 0 && !this.isJumpHeld()) {
      this.player.setVelocityY(
        Math.max(body.velocity.y, PLAYER_MOVEMENT.releasedJumpFloorVelocity),
      );
    }

    if (
      this.app.session.form === 'Enhanced' &&
      isActionJustPressed(this.inputMap, 'action')
    ) {
      this.firePlayerProjectile();
    }

    this.applyMovingPlatformCarry();

    if (time < this.playerInvulnerableUntil) {
      const pulse = Math.floor(time / 90) % 2 === 0;
      this.player.setAlpha(pulse ? 0.45 : 1);
    } else {
      this.player.setAlpha(1);
    }

    if (!onFloor) {
      this.lastAirborneVelocityY = Math.max(this.lastAirborneVelocityY, body.velocity.y);
    }

    this.ducking =
      onFloor &&
      movingDown &&
      movingLeft === movingRight &&
      Math.abs(body.velocity.x) < PLAYER_MOVEMENT.platformCarrySpeedThreshold &&
      time >= this.playerInvulnerableUntil;

    this.updatePlayerPose(time, movingLeft, movingRight, onFloor, body.velocity.y, this.ducking);
    this.wasPlayerGrounded = onFloor;
  }

  private updateMovingPlatforms(deltaSeconds: number): void {
    this.movingPlatforms.forEach((platform) => {
      const lastX = platform.x;
      const direction = Number(platform.getData('direction')) || 1;
      const speed = Number(platform.getData('speed')) || 0;
      const minX = Number(platform.getData('minX'));
      const maxX = Number(platform.getData('maxX'));
      let nextDirection = direction;
      let nextX = platform.x + direction * speed * deltaSeconds;

      if (nextX <= minX) {
        nextX = minX;
        nextDirection = 1;
      } else if (nextX >= maxX) {
        nextX = maxX;
        nextDirection = -1;
      }

      platform.x = nextX;
      platform.setData('direction', nextDirection);
      platform.setData('lastX', lastX);
      (platform.body as ArcadeBody).updateFromGameObject();
    });
  }

  private updateFallingBlocks(deltaSeconds: number): void {
    this.fallingBlocks.forEach((block) => {
      const triggered = Boolean(block.getData('triggered'));

      if (!triggered) {
        const standing = this.isStandingOn(block);
        const contactMs = standing ? Number(block.getData('contactMs')) + deltaSeconds * 1000 : 0;

        block.setData('contactMs', contactMs);

        if (contactMs >= 500) {
          block.setData('triggered', true);
          block.setData('fallSpeed', 90);
        }

        return;
      }

      const nextSpeed = Number(block.getData('fallSpeed')) + 820 * deltaSeconds;
      block.setData('fallSpeed', nextSpeed);
      block.y += nextSpeed * deltaSeconds;
      (block.body as ArcadeBody).updateFromGameObject();
    });
  }

  private updateEnemies(time: number, deltaSeconds: number): void {
    const player = this.player;

    if (!this.enemyGroup || !player) {
      return;
    }

    this.enemyGroup.getChildren().forEach((child) => {
      const enemy = child as DynamicSprite;

      if (!enemy.active || enemy.getData('defeated')) {
        return;
      }

      const kind = enemy.getData('kind') as EnemyKind;
      const minX = Number(enemy.getData('minX'));
      const maxX = Number(enemy.getData('maxX'));
      let direction = Number(enemy.getData('direction')) || -1;

      if (kind === 'Ground' || kind === 'Armored' || kind === 'ProtectedHead') {
        const walkSpeed = kind === 'Armored' ? 54 : kind === 'ProtectedHead' ? 70 : 82;
        const body = enemy.body as ArcadeBody;
        const aheadX = enemy.x + direction * 22;
        const footY = body.bottom + 8;

        if (body.blocked.left || body.blocked.right || !this.hasSupportBelow(aheadX, footY)) {
          direction *= -1;
        }

        if (enemy.x <= minX) {
          direction = 1;
        }

        if (enemy.x >= maxX) {
          direction = -1;
        }

        enemy.setData('direction', direction);
        enemy.setVelocityX(direction * walkSpeed);
        enemy.setFlipX(direction > 0);

        if (kind === 'Ground') {
          enemy.setTexture(Math.floor(time / 180) % 2 === 0 ? 'zombie-walk1' : 'zombie-walk2');
        }
      }

      if (kind === 'Flying') {
        const amplitude = Number(enemy.getData('amplitude'));
        const frequency = Number(enemy.getData('frequency'));
        const baseY = Number(enemy.getData('baseY'));
        const nextX = enemy.x + direction * 92 * deltaSeconds;

        if (nextX <= minX) {
          direction = 1;
        } else if (nextX >= maxX) {
          direction = -1;
        }

        enemy.setData('direction', direction);
        enemy.x += direction * 92 * deltaSeconds;
        enemy.y = baseY + Math.sin(time * frequency) * amplitude;
        (enemy.body as ArcadeBody).updateFromGameObject();
      }

      if (kind === 'Shooter') {
        const nextShotAt = Number(enemy.getData('nextShotAt')) || 0;
        const fireDelayMs = Number(enemy.getData('fireDelayMs')) || 2200;
        const deltaToPlayer = player.x - enemy.x;

        if (Math.abs(deltaToPlayer) < 430 && time >= nextShotAt) {
          this.fireEnemyProjectile(enemy, deltaToPlayer >= 0 ? 1 : -1);
          enemy.setData('nextShotAt', time + fireDelayMs);
        }
      }
    });
  }

  private updateProjectiles(): void {
    this.playerProjectiles?.getChildren().forEach((child) => {
      const projectile = child as DynamicSprite;

      if (!this.currentStage || projectile.x < -40 || projectile.x > this.currentStage.width + 40) {
        projectile.destroy();
      }
    });

    this.enemyProjectiles?.getChildren().forEach((child) => {
      const projectile = child as DynamicSprite;

      if (
        !this.currentStage ||
        projectile.x < -40 ||
        projectile.x > this.currentStage.width + 40 ||
        projectile.y > this.currentStage.height + 200
      ) {
        projectile.destroy();
      }
    });
  }

  private updateStageTimer(deltaSeconds: number): void {
    if (this.stageResolving) {
      return;
    }

    this.stageTimeRemaining = Math.max(0, this.stageTimeRemaining - deltaSeconds);

    if (this.stageTimeRemaining <= 0) {
      this.resolveLifeLoss(true);
    }
  }

  private updateHud(): void {
    if (!this.currentStage) {
      return;
    }

    this.app.updateHud({
      stageId: this.currentStage.id,
      score: this.app.session.score,
      coins: this.app.session.coins,
      lives: this.app.session.lives,
      timeRemaining: Math.ceil(this.stageTimeRemaining),
      difficulty: this.app.session.difficulty,
      paused: this.pausedByShell,
    });
  }

  private checkPitFall(): void {
    if (!this.player || !this.currentStage) {
      return;
    }

    if (this.player.y > this.currentStage.height + 120) {
      this.resolveLifeLoss(true);
    }
  }

  private applyMovingPlatformCarry(): void {
    if (!this.player) {
      return;
    }

    this.movingPlatforms.forEach((platform) => {
      if (this.isStandingOn(platform)) {
        const lastX = Number(platform.getData('lastX')) || platform.x;
        this.player!.x += platform.x - lastX;
      }
    });
  }

  private isStandingOn(target: DynamicSprite): boolean {
    if (!this.player) {
      return false;
    }

    const playerBody = this.player.body as ArcadeBody;
    const targetBody = target.body as ArcadeBody;
    const verticallyAligned = Math.abs(playerBody.bottom - targetBody.top) <= 8;
    const horizontallyAligned =
      playerBody.right > targetBody.left + 6 && playerBody.left < targetBody.right - 6;

    return verticallyAligned && horizontallyAligned && playerBody.velocity.y >= 0;
  }

  private hasSupportBelow(x: number, y: number): boolean {
    if (this.terrainCollisionRects.length === 0) {
      return false;
    }

    return this.terrainCollisionRects.some((rect) => {
      const withinX = x >= rect.x && x <= rect.x + rect.width;
      const nearTop = y >= rect.y - 14 && y <= rect.y + 8;

      return withinX && nearTop;
    });
  }

  private findSupportTopForStaticHazard(x: number, sampleY: number): number | null {
    let nearestTop: number | null = null;

    this.terrainCollisionRects.forEach((rect) => {
      const withinX = x >= rect.x + 4 && x <= rect.x + rect.width - 4;
      const reachableY = rect.y >= sampleY - 24 && rect.y <= sampleY + 80;

      if (!withinX || !reachableY) {
        return;
      }

      if (nearestTop === null || rect.y < nearestTop) {
        nearestTop = rect.y;
      }
    });

    return nearestTop;
  }

  private firePlayerProjectile(): void {
    if (!this.playerProjectiles || !this.player) {
      return;
    }

    if (this.playerProjectiles.countActive(true) >= 2) {
      return;
    }

    const projectile = this.playerProjectiles.create(
      this.player.x + this.facing * 20,
      this.player.y - 6,
      'fire-projectile',
    ) as DynamicSprite;

    const body = projectile.body as ArcadeBody;
    body.setAllowGravity(false);
    projectile.setVelocity(this.facing * 310, 0);
    projectile.setDataEnabled();
    projectile.setData('hostile', false);
    this.app.audio.playShoot();
  }

  private fireEnemyProjectile(enemy: DynamicSprite, direction: number): void {
    if (!this.enemyProjectiles) {
      return;
    }

    const projectile = this.enemyProjectiles.create(
      enemy.x + direction * 20,
      enemy.y - 4,
      'hostile-projectile',
    ) as DynamicSprite;

    const body = projectile.body as ArcadeBody;
    body.setAllowGravity(false);
    projectile.setVelocity(direction * 210, 0);
    projectile.setDataEnabled();
    projectile.setData('hostile', true);
    this.app.audio.playShoot();
  }

  private handleCoinPickup(
    playerObject: unknown,
    coinObject: unknown,
  ): void {
    void playerObject;
    const coin = coinObject as DynamicSprite;
    const awardedLives = this.app.session.collectCoins(1);

    coin.destroy();
    this.app.audio.playCoin();

    if (awardedLives > 0) {
      this.app.audio.playExtraLife();
    }
  }

  private handlePowerupPickup(
    playerObject: unknown,
    powerupObject: unknown,
  ): void {
    void playerObject;
    const powerup = powerupObject as DynamicSprite;
    const reward = powerup.getData('reward') as RewardKind;

    this.app.session.applyReward(reward);
    this.syncPlayerAppearance();
    powerup.destroy();
    this.app.audio.playPowerup();
  }

  private handlePlayerBlockCollision(
    playerObject: unknown,
    blockObject: unknown,
  ): void {
    const player = playerObject as DynamicSprite;
    const block = blockObject as DynamicSprite;
    const playerBody = player.body as ArcadeBody;
    const blockBody = block.body as ArcadeBody;

    if (block.getData('spent')) {
      return;
    }

    const headHit = playerBody.touching.up && playerBody.velocity.y <= 0 && player.y > block.y;
    const centeredEnough =
      playerBody.right > blockBody.left + 4 && playerBody.left < blockBody.right - 4;

    if (!headHit || !centeredEnough) {
      return;
    }

    block.setData('spent', true);
    block.setTint(0x8a7348);
    this.app.audio.playBlockHit();
    this.spawnBlockReward(block);
  }

  private spawnBlockReward(block: DynamicSprite): void {
    if (!this.powerupGroup) {
      return;
    }

    const reward = block.getData('reward') as RewardKind;

    if (reward === 'coin') {
      const awardedLives = this.app.session.collectCoins(1);
      this.app.audio.playCoin();

      if (awardedLives > 0) {
        this.app.audio.playExtraLife();
      }

      const flare = this.add.rectangle(block.x, block.y - 36, 18, 18, 0xffd85f, 0.9);
      this.stageDecor.push(flare);
      this.tweens.add({
        targets: flare,
        y: flare.y - 28,
        alpha: 0,
        duration: 360,
        onComplete: () => {
          flare.destroy();
        },
      });
      return;
    }

    const texture = reward === 'mushroom' ? 'powerup-mushroom' : 'powerup-flower';
    const powerup = this.powerupGroup.create(block.x, block.y - 34, texture) as DynamicSprite;
    const body = powerup.body as ArcadeBody;
    body.setAllowGravity(false);
    powerup.setDataEnabled();
    powerup.setData('reward', reward);
    powerup.setVelocityY(-36);

    this.tweens.add({
      targets: powerup,
      y: powerup.y - 10,
      yoyo: true,
      repeat: 1,
      duration: 180,
      onComplete: () => {
        powerup.setVelocity(0, 0);
      },
    });
  }

  private handlePlayerEnemyCollision(
    playerObject: unknown,
    enemyObject: unknown,
  ): void {
    const player = playerObject as DynamicSprite;
    const enemy = enemyObject as DynamicSprite;

    if (!enemy.active || enemy.getData('defeated')) {
      return;
    }

    const playerBody = player.body as ArcadeBody;
    const enemyBody = enemy.body as ArcadeBody;
    const kind = enemy.getData('kind') as EnemyKind;
    const stomped = playerBody.velocity.y > 120 && playerBody.bottom <= enemyBody.top + 14;

    if (stomped && (kind === 'Ground' || kind === 'Flying' || kind === 'Shooter')) {
      this.defeatEnemy(enemy);
      player.setVelocityY(-350);
      this.triggerImpactShake(90, 0.0025);
      this.app.audio.playEnemyDefeat();
      this.app.session.addScore(200);
      return;
    }

    if (stomped) {
      player.setVelocityY(-220);
      this.triggerImpactShake(80, 0.0018);
    }

    this.damagePlayer();
  }

  private handlePlayerCactusCollision(
    playerObject: unknown,
    cactusObject: unknown,
  ): void {
    void playerObject;
    void cactusObject;
    this.damagePlayer();
  }

  private handlePlayerProjectileHitEnemy(
    projectileObject: unknown,
    enemyObject: unknown,
  ): void {
    const projectile = projectileObject as DynamicSprite;
    const enemy = enemyObject as DynamicSprite;

    if (!enemy.active || enemy.getData('defeated')) {
      projectile.destroy();
      return;
    }

    const kind = enemy.getData('kind') as EnemyKind;

    if (kind === 'Armored') {
      const projectileBody = projectile.body as ArcadeBody;

      projectile.setTexture('hostile-projectile');
      projectile.setVelocityX(-projectileBody.velocity.x);
      this.playerProjectiles?.remove(projectile);
      this.enemyProjectiles?.add(projectile);
      return;
    }

    projectile.destroy();
    this.defeatEnemy(enemy);
    this.app.session.addScore(250);
    this.app.audio.playEnemyDefeat();
  }

  private handleHostileProjectileHitPlayer(
    playerObject: unknown,
    projectileObject: unknown,
  ): void {
    void playerObject;
    const projectile = projectileObject as DynamicSprite;

    projectile.destroy();
    this.damagePlayer();
  }

  private damagePlayer(): void {
    if (!this.player || this.stageResolving) {
      return;
    }

    if (this.time.now < this.playerInvulnerableUntil) {
      return;
    }

    const outcome = this.app.session.takeDamage();

    if (outcome === 'life_lost') {
      this.app.audio.playDeath();
      this.resolveLifeLoss(false);
      return;
    }

    this.app.audio.playPowerDown();
    this.playerInvulnerableUntil = this.time.now + 1500;
    this.syncPlayerAppearance();
  }

  private resolveLifeLoss(isHazardLoss: boolean): void {
    if (this.stageResolving) {
      return;
    }

    this.stageResolving = true;
    this.stageActive = false;
    this.doubleJumpLandingArmed = false;

    if (isHazardLoss) {
      this.app.audio.playDeath();
      this.app.session.loseLife();
    }

    this.time.delayedCall(180, () => {
      this.app.onStageLifeLost();
    });
  }

  private handleGoalReached(): void {
    if (this.stageResolving || !this.currentStage) {
      return;
    }

    this.stageResolving = true;
    this.stageActive = false;
    this.app.audio.playStageClear();

    const summary: StageSummary = {
      stageId: this.currentStage.id,
      score: this.app.session.score,
      coins: this.app.session.coins,
      lives: this.app.session.lives,
      timeRemaining: Math.ceil(this.stageTimeRemaining),
      form: this.app.session.form,
      bonus: 0,
      difficulty: this.app.session.difficulty,
    };

    this.time.delayedCall(200, () => {
      this.app.onStageCleared(summary);
    });
  }

  private defeatEnemy(enemy: DynamicSprite): void {
    enemy.setData('defeated', true);
    (enemy.body as ArcadeBody).enable = false;
    enemy.setFlipY(true);

    this.tweens.add({
      targets: enemy,
      y: enemy.y + 180,
      alpha: 0,
      duration: 520,
      onComplete: () => {
        enemy.destroy();
      },
    });
  }

  private syncPlayerAppearance(): void {
    if (!this.player) {
      return;
    }

    this.player.setTexture(this.playerPoseKey('idle'));
  }

  private textureForEnemy(kind: EnemyKind): string {
    return getEnemyTextureKey(kind);
  }

  private updatePlayerPose(
    time: number,
    movingLeft: boolean,
    movingRight: boolean,
    onFloor: boolean,
    verticalVelocity: number,
    ducking: boolean,
  ): void {
    if (!this.player) {
      return;
    }

    const pose = resolvePlayerPose({
      invulnerable: time < this.playerInvulnerableUntil,
      grounded: onFloor,
      verticalVelocity,
      ducking,
      movingHorizontally: movingLeft !== movingRight,
      animationTimeMs: time,
    });

    this.player.setTexture(this.playerPoseKey(pose));
  }

  private playerPoseKey(
    pose: 'idle' | 'walk1' | 'walk2' | 'jump' | 'fall' | 'hurt' | 'duck',
  ): string {
    return getPlayerTextureKey(this.app.session.form, pose);
  }

  private isJumpHeld(): boolean {
    return isActionDown(this.inputMap, 'jump');
  }

  private triggerImpactShake(duration: number, intensity: number): void {
    this.cameras.main.shake(duration, intensity, false);
  }

  private trackCollider(collider: Phaser.Physics.Arcade.Collider): void {
    this.stageColliders.push(collider);
  }

  private clearStage(): void {
    this.stageActive = false;
    this.stageResolving = false;
    this.pausedByShell = false;
    this.wasPlayerGrounded = false;
    this.doubleJumpLandingArmed = false;
    this.coyoteJumpExpiresAt = 0;
    this.jumpBufferedUntil = 0;
    this.lastAirborneVelocityY = 0;
    this.ducking = false;
    this.terrainCollisionRects = [];
    this.physics.world.resume();

    this.stageColliders.forEach((collider) => collider.destroy());
    this.stageColliders.length = 0;

    this.terrainGroup?.clear(true, true);
    this.terrainGroup = null;
    this.blockGroup?.clear(true, true);
    this.blockGroup = null;
    this.coinGroup?.clear(true, true);
    this.coinGroup = null;
    this.cactusGroup?.clear(true, true);
    this.cactusGroup = null;
    this.powerupGroup?.clear(true, true);
    this.powerupGroup = null;
    this.enemyGroup?.clear(true, true);
    this.enemyGroup = null;
    this.playerProjectiles?.clear(true, true);
    this.playerProjectiles = null;
    this.enemyProjectiles?.clear(true, true);
    this.enemyProjectiles = null;
    this.goal?.destroy();
    this.goal = null;
    this.player?.destroy();
    this.player = null;

    this.movingPlatforms.forEach((platform) => platform.destroy());
    this.movingPlatforms.length = 0;
    this.fallingBlocks.forEach((block) => block.destroy());
    this.fallingBlocks.length = 0;
    this.stageDecor.forEach((decor) => decor.destroy());
    this.stageDecor.length = 0;
  }
}
