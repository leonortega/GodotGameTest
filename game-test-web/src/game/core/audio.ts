import type { SaveData } from './types';
import { MUSIC_ASSETS, SFX_ASSETS } from './assets';

function dbToGain(db: number): number {
  return Math.max(0, Math.min(1, Math.pow(10, db / 20)));
}

export class RetroAudio {
  private currentMusic: HTMLAudioElement | null = null;
  private audioContext: AudioContext | null = null;
  private baseMusicDb: number;
  private baseSfxDb: number;
  private paused = false;
  private preloadStarted = false;
  private readonly musicCache = new Map<string, HTMLAudioElement>();
  private readonly sfxBuffers = new Map<string, AudioBuffer>();
  private sfxPreloadPromise: Promise<void> | null = null;
  private pendingThemeId: string | null = null;

  constructor(saveData: SaveData) {
    this.baseMusicDb = saveData.slot.settings.musicVolumeDb;
    this.baseSfxDb = saveData.slot.settings.sfxVolumeDb;
  }

  async unlock(): Promise<void> {
    this.preloadAudioAssets();

    const context = this.ensureAudioContext();

    if (context.state !== 'running') {
      try {
        await context.resume();
      } catch {
        return;
      }
    }

    await this.preloadSfxBuffers();

    if (this.currentMusic && this.currentMusic.paused) {
      void this.currentMusic.play().catch(() => undefined);
    } else if (this.pendingThemeId) {
      this.startTheme(this.pendingThemeId);
    }
  }

  setMusicVolumeDb(nextValue: number): void {
    this.baseMusicDb = nextValue;
    this.updateMusicVolume();
  }

  setSfxVolumeDb(nextValue: number): void {
    this.baseSfxDb = nextValue;
  }

  setPaused(paused: boolean): void {
    this.paused = paused;
    this.updateMusicVolume();
  }

  startTheme(themeId: string): void {
    this.preloadAudioAssets();
    this.pendingThemeId = themeId;
    const src = MUSIC_ASSETS[themeId as keyof typeof MUSIC_ASSETS] ?? MUSIC_ASSETS.title;
    const nextMusic = this.getMusicElement(src);

    nextMusic.loop = true;
    nextMusic.volume = this.getMusicVolume();

    this.stopMusic();
    this.currentMusic = nextMusic;
    this.currentMusic.currentTime = 0;

    void nextMusic.play().catch(() => undefined);
  }

  stopMusic(): void {
    if (!this.currentMusic) {
      return;
    }

    this.currentMusic.pause();
    this.currentMusic.currentTime = 0;
    this.currentMusic = null;
  }

  playUiTick(): void {
    this.playSfx(SFX_ASSETS.uiTick, true);
  }

  playUiHover(): void {
    this.playSfx(SFX_ASSETS.uiHover, true);
  }

  playUiBack(): void {
    this.playSfx(SFX_ASSETS.uiBack, true);
  }

  playJump(): void {
    this.playSfx(SFX_ASSETS.jump, false);
  }

  playDoubleJump(): void {
    this.playSfx(SFX_ASSETS.doubleJump, false);
  }

  playLand(): void {
    this.playSfx(SFX_ASSETS.land, false);
  }

  playCoin(): void {
    this.playSfx(SFX_ASSETS.coin, false);
  }

  playPowerup(): void {
    this.playSfx(SFX_ASSETS.powerup, false);
  }

  playPowerDown(): void {
    this.playSfx(SFX_ASSETS.powerDown, false);
  }

  playEnemyDefeat(): void {
    this.playSfx(SFX_ASSETS.enemyDefeat, false);
  }

  playDamage(): void {
    this.playSfx(SFX_ASSETS.damage, false);
  }

  playDeath(): void {
    this.playSfx(SFX_ASSETS.death, false);
  }

  playShoot(): void {
    this.playSfx(SFX_ASSETS.shoot, false);
  }

  playPause(): void {
    this.playSfx(SFX_ASSETS.pause, true);
  }

  playStageClear(): void {
    this.playSfx(SFX_ASSETS.stageClear, false);
  }

  playExtraLife(): void {
    this.playSfx(SFX_ASSETS.extraLife, false);
  }

  playBlockHit(): void {
    this.playSfx(SFX_ASSETS.blockHit, false);
  }

  private playSfx(src: string, ui: boolean): void {
    void this.playSfxAsync(src, ui);
  }

  private updateMusicVolume(): void {
    if (!this.currentMusic) {
      return;
    }

    this.currentMusic.volume = this.getMusicVolume();
  }

  private getMusicVolume(): number {
    const pausedOffset = this.paused ? -10 : 0;

    return dbToGain(this.baseMusicDb + pausedOffset);
  }

  private getSfxVolume(): number {
    return dbToGain(this.baseSfxDb);
  }

  private getUiVolume(): number {
    return dbToGain(this.baseSfxDb - 3);
  }

  private ensureAudioContext(): AudioContext {
    if (!this.audioContext) {
      this.audioContext = new AudioContext();
    }

    return this.audioContext;
  }

  private preloadAudioAssets(): void {
    if (this.preloadStarted) {
      return;
    }

    this.preloadStarted = true;

    Object.values(MUSIC_ASSETS).forEach((src) => {
      this.getMusicElement(src).load();
    });

    void this.preloadSfxBuffers();
  }

  private getMusicElement(src: string): HTMLAudioElement {
    const cached = this.musicCache.get(src);

    if (cached) {
      return cached;
    }

    const audio = new Audio(src);
    audio.preload = 'auto';
    this.musicCache.set(src, audio);
    return audio;
  }

  private preloadSfxBuffers(): Promise<void> {
    if (this.sfxPreloadPromise) {
      return this.sfxPreloadPromise;
    }

    const context = this.ensureAudioContext();

    this.sfxPreloadPromise = Promise.all(
      Object.values(SFX_ASSETS).map(async (src) => {
        if (this.sfxBuffers.has(src)) {
          return;
        }

        try {
          const response = await fetch(src);

          if (!response.ok) {
            return;
          }

          const encoded = await response.arrayBuffer();
          const decoded = await context.decodeAudioData(encoded.slice(0));
          this.sfxBuffers.set(src, decoded);
        } catch {
          return;
        }
      }),
    ).then(() => undefined);

    return this.sfxPreloadPromise;
  }

  private async playSfxAsync(src: string, ui: boolean): Promise<void> {
    const context = this.ensureAudioContext();

    if (context.state !== 'running') {
      try {
        await context.resume();
      } catch {
        return;
      }
    }

    await this.preloadSfxBuffers();

    const buffer = this.sfxBuffers.get(src);

    if (!buffer) {
      return;
    }

    const source = context.createBufferSource();
    const gain = context.createGain();

    source.buffer = buffer;
    gain.gain.value = ui ? this.getUiVolume() : this.getSfxVolume();
    source.connect(gain);
    gain.connect(context.destination);
    source.start();
    source.onended = () => {
      source.disconnect();
      gain.disconnect();
    };
  }
}
