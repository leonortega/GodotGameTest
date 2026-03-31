export type AudioSettings = {
  musicVolumeDb: number;
  sfxVolumeDb: number;
};

type SaveSlot = {
  slot: {
    settings: AudioSettings;
  };
};

const STORAGE_KEY = "super-pixel-quest-save";
const MIN_DB = -20;
const MAX_DB = 0;

export const DEFAULT_AUDIO_SETTINGS: AudioSettings = {
  musicVolumeDb: -4,
  sfxVolumeDb: -2,
};

function clampDb(value: number, fallback: number): number {
  if (!Number.isFinite(value)) {
    return fallback;
  }

  return Math.min(MAX_DB, Math.max(MIN_DB, Math.round(value)));
}

function getStorage(): Storage | undefined {
  if (typeof window === "undefined") {
    return undefined;
  }

  return window.localStorage;
}

function normalizeSave(raw: unknown): SaveSlot {
  if (raw && typeof raw === "object") {
    const current = raw as Partial<SaveSlot> & Partial<AudioSettings>;
    const slotSettings = current.slot?.settings;

    if (slotSettings) {
      return {
        slot: {
          settings: {
            musicVolumeDb: clampDb(slotSettings.musicVolumeDb, DEFAULT_AUDIO_SETTINGS.musicVolumeDb),
            sfxVolumeDb: clampDb(slotSettings.sfxVolumeDb, DEFAULT_AUDIO_SETTINGS.sfxVolumeDb),
          },
        },
      };
    }

    if (typeof current.musicVolumeDb === "number" || typeof current.sfxVolumeDb === "number") {
      return {
        slot: {
          settings: {
            musicVolumeDb: clampDb(current.musicVolumeDb ?? DEFAULT_AUDIO_SETTINGS.musicVolumeDb, DEFAULT_AUDIO_SETTINGS.musicVolumeDb),
            sfxVolumeDb: clampDb(current.sfxVolumeDb ?? DEFAULT_AUDIO_SETTINGS.sfxVolumeDb, DEFAULT_AUDIO_SETTINGS.sfxVolumeDb),
          },
        },
      };
    }
  }

  return {
    slot: {
      settings: { ...DEFAULT_AUDIO_SETTINGS },
    },
  };
}

function persistSave(saveSlot: SaveSlot): void {
  const storage = getStorage();
  if (!storage) {
    return;
  }

  storage.setItem(STORAGE_KEY, JSON.stringify(saveSlot));
}

function makeDefaultSave(): SaveSlot {
  return {
    slot: {
      settings: { ...DEFAULT_AUDIO_SETTINGS },
    },
  };
}

function loadSave(): SaveSlot {
  const storage = getStorage();
  if (!storage) {
    return makeDefaultSave();
  }

  const raw = storage.getItem(STORAGE_KEY);
  if (!raw) {
    const defaultSave = makeDefaultSave();
    persistSave(defaultSave);
    return defaultSave;
  }

  try {
    const normalized = normalizeSave(JSON.parse(raw));
    persistSave(normalized);
    return normalized;
  } catch {
    const defaultSave = makeDefaultSave();
    persistSave(defaultSave);
    return defaultSave;
  }
}

class SettingsStore {
  private saveSlot = loadSave();

  getAudioSettings(): AudioSettings {
    return { ...this.saveSlot.slot.settings };
  }

  updateAudioSettings(next: Partial<AudioSettings>): AudioSettings {
    this.saveSlot = {
      slot: {
        settings: {
          musicVolumeDb: clampDb(next.musicVolumeDb ?? this.saveSlot.slot.settings.musicVolumeDb, DEFAULT_AUDIO_SETTINGS.musicVolumeDb),
          sfxVolumeDb: clampDb(next.sfxVolumeDb ?? this.saveSlot.slot.settings.sfxVolumeDb, DEFAULT_AUDIO_SETTINGS.sfxVolumeDb),
        },
      },
    };

    persistSave(this.saveSlot);
    return this.getAudioSettings();
  }
}

export function dbToGain(db: number): number {
  return Math.pow(10, db / 20);
}

const Settings = new SettingsStore();

export default Settings;