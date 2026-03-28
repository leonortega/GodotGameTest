import { type Difficulty, type SaveData } from './types';

const SAVE_KEY = 'super-pixel-quest-web-save';
const MIN_DB = -24;
const MAX_DB = 0;

export function createDefaultSaveData(): SaveData {
  return {
    slot: {
      highestClearedStage: null,
      bestScore: 0,
      settings: {
        musicVolumeDb: -8,
        sfxVolumeDb: -6,
        difficulty: 'Normal',
      },
    },
  };
}

function clampDb(value: unknown, fallback: number): number {
  const numeric = Number(value);

  if (!Number.isFinite(numeric)) {
    return fallback;
  }

  return Math.max(MIN_DB, Math.min(MAX_DB, numeric));
}

function normalizeDifficulty(value: unknown): Difficulty {
  if (value === 'Easy' || value === 'Hard') {
    return value;
  }

  return 'Normal';
}

function normalizeStageId(value: unknown): string | null {
  if (typeof value !== 'string') {
    return null;
  }

  return /^[1-9]-[1-9]$/.test(value) ? value : null;
}

function isLegacySaveData(raw: unknown): raw is {
  highestClearedStage?: unknown;
  bestScore?: unknown;
  musicVolumeDb?: unknown;
  sfxVolumeDb?: unknown;
  difficulty?: unknown;
} {
  return Boolean(raw) && typeof raw === 'object' && !('slot' in (raw as Record<string, unknown>));
}

function sanitizeLegacySaveData(raw: {
  highestClearedStage?: unknown;
  bestScore?: unknown;
  musicVolumeDb?: unknown;
  sfxVolumeDb?: unknown;
  difficulty?: unknown;
}): SaveData {
  const defaults = createDefaultSaveData();

  return {
    slot: {
      highestClearedStage: normalizeStageId(raw.highestClearedStage),
      bestScore: Math.max(
        0,
        Number.isFinite(raw.bestScore) ? Math.floor(raw.bestScore as number) : 0,
      ),
      settings: {
        musicVolumeDb: clampDb(raw.musicVolumeDb, defaults.slot.settings.musicVolumeDb),
        sfxVolumeDb: clampDb(raw.sfxVolumeDb, defaults.slot.settings.sfxVolumeDb),
        difficulty: normalizeDifficulty(raw.difficulty),
      },
    },
  };
}

export function sanitizeSaveData(raw: unknown): SaveData {
  const defaults = createDefaultSaveData();

  if (!raw || typeof raw !== 'object') {
    return defaults;
  }

  if (isLegacySaveData(raw)) {
    return sanitizeLegacySaveData(raw);
  }

  const candidate = raw as Partial<SaveData>;
  const slot = candidate.slot;

  if (!slot || typeof slot !== 'object') {
    return defaults;
  }

  return {
    slot: {
      highestClearedStage: normalizeStageId(slot.highestClearedStage),
      bestScore: Math.max(
        0,
        Number.isFinite(slot.bestScore) ? Math.floor(slot.bestScore as number) : 0,
      ),
      settings: {
        musicVolumeDb: clampDb(
          slot.settings?.musicVolumeDb,
          defaults.slot.settings.musicVolumeDb,
        ),
        sfxVolumeDb: clampDb(
          slot.settings?.sfxVolumeDb,
          defaults.slot.settings.sfxVolumeDb,
        ),
        difficulty: normalizeDifficulty(slot.settings?.difficulty),
      },
    },
  };
}

export function loadSaveData(): SaveData {
  const defaults = createDefaultSaveData();

  try {
    const rawText = window.localStorage.getItem(SAVE_KEY);

    if (!rawText) {
      return defaults;
    }

    return sanitizeSaveData(JSON.parse(rawText));
  } catch {
    return defaults;
  }
}

export function persistSaveData(saveData: SaveData): void {
  window.localStorage.setItem(SAVE_KEY, JSON.stringify(saveData));
}

export function formatSaveData(saveData: SaveData): string {
  return JSON.stringify(saveData, null, 2);
}
