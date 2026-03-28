import Phaser from 'phaser';

export const GAMEPLAY_BINDINGS = {
  moveLeft: [Phaser.Input.Keyboard.KeyCodes.LEFT, Phaser.Input.Keyboard.KeyCodes.A],
  moveRight: [Phaser.Input.Keyboard.KeyCodes.RIGHT, Phaser.Input.Keyboard.KeyCodes.D],
  moveDown: [Phaser.Input.Keyboard.KeyCodes.DOWN, Phaser.Input.Keyboard.KeyCodes.S],
  jump: [
    Phaser.Input.Keyboard.KeyCodes.UP,
    Phaser.Input.Keyboard.KeyCodes.W,
    Phaser.Input.Keyboard.KeyCodes.SPACE,
    Phaser.Input.Keyboard.KeyCodes.K,
  ],
  action: [Phaser.Input.Keyboard.KeyCodes.SHIFT, Phaser.Input.Keyboard.KeyCodes.J],
  pause: [Phaser.Input.Keyboard.KeyCodes.ESC, Phaser.Input.Keyboard.KeyCodes.P],
} as const;

export type GameplayAction = keyof typeof GAMEPLAY_BINDINGS;
export type GameplayInputMap = Record<GameplayAction, Phaser.Input.Keyboard.Key[]>;

export const SHELL_SHORTCUTS = {
  start: 'Enter',
  controls: 'KeyC',
  enemies: 'KeyE',
  config: 'KeyO',
  difficulty: 'KeyD',
  back: 'Escape',
} as const;

export function createGameplayInputMap(
  keyboard: Phaser.Input.Keyboard.KeyboardPlugin,
): GameplayInputMap {
  return {
    moveLeft: GAMEPLAY_BINDINGS.moveLeft.map((keyCode) => keyboard.addKey(keyCode)),
    moveRight: GAMEPLAY_BINDINGS.moveRight.map((keyCode) => keyboard.addKey(keyCode)),
    moveDown: GAMEPLAY_BINDINGS.moveDown.map((keyCode) => keyboard.addKey(keyCode)),
    jump: GAMEPLAY_BINDINGS.jump.map((keyCode) => keyboard.addKey(keyCode)),
    action: GAMEPLAY_BINDINGS.action.map((keyCode) => keyboard.addKey(keyCode)),
    pause: GAMEPLAY_BINDINGS.pause.map((keyCode) => keyboard.addKey(keyCode)),
  };
}

export function isActionDown(inputMap: GameplayInputMap, action: GameplayAction): boolean {
  return inputMap[action].some((key) => key.isDown);
}

export function isActionJustPressed(inputMap: GameplayInputMap, action: GameplayAction): boolean {
  return inputMap[action].some((key) => Phaser.Input.Keyboard.JustDown(key));
}
