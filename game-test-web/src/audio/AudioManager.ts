import Phaser from "phaser";
import Settings, { dbToGain } from "../core/Settings";

export type MusicKey =
  | "title"
  | "overworld"
  | "underground"
  | "athletic-sky"
  | "castle"
  | "invincibility"
  | "hurry"
  | "game-over"
  | "world-clear";
type ManagedSound = Phaser.Sound.BaseSound & {
  setVolume?: (value: number) => ManagedSound;
};

export default class AudioManager {
  private scene: Phaser.Scene;
  private currentMusic?: ManagedSound;
  private currentKey?: MusicKey;

  constructor(scene: Phaser.Scene) {
    this.scene = scene;
  }

  playMusic(key: MusicKey, loop = true): void {
    if (this.currentKey === key && this.currentMusic?.isPlaying) {
      return;
    }

    this.stopMusic();
    const music = this.scene.sound.add(key, { loop, volume: 0.35 * dbToGain(Settings.getAudioSettings().musicVolumeDb) }) as ManagedSound;
    music.play();
    this.currentMusic = music;
    this.currentKey = key;
  }

  refreshMusicVolume(): void {
    if (!this.currentMusic) {
      return;
    }

    this.currentMusic.setVolume?.(0.35 * dbToGain(Settings.getAudioSettings().musicVolumeDb));
  }

  stopMusic(): void {
    if (this.currentMusic) {
      this.currentMusic.stop();
      this.currentMusic.destroy();
      this.currentMusic = undefined;
      this.currentKey = undefined;
    }
  }

  pauseMusic(): void {
    if (this.currentMusic?.isPlaying) {
      this.currentMusic.pause();
    }
  }

  resumeMusic(): void {
    if (this.currentMusic && this.currentMusic.isPaused) {
      this.currentMusic.resume();
      this.refreshMusicVolume();
    }
  }

  playSfx(key: string, config?: Phaser.Types.Sound.SoundConfig): void {
    const baseVolume = config?.volume ?? 0.5;
    this.scene.sound.play(key, { ...config, volume: baseVolume * dbToGain(Settings.getAudioSettings().sfxVolumeDb) });
  }
}
