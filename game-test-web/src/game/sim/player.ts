import type { PlayerPose } from '../core/assets';

export const PLAYER_MOVEMENT = {
  walkSpeed: 170,
  runSpeed: 240,
  groundAcceleration: 1400,
  airAcceleration: 900,
  groundIdleDamping: 0.82,
  airIdleDamping: 0.97,
  maxVerticalSpeed: 900,
  groundJumpVelocity: -450,
  doubleJumpVelocity: -390,
  releasedJumpFloorVelocity: -230,
  coyoteTimeMs: 100,
  jumpBufferMs: 150,
  platformCarrySpeedThreshold: 42,
  landingShakeThreshold: 520,
} as const;

export interface PlayerPoseContext {
  invulnerable: boolean;
  grounded: boolean;
  verticalVelocity: number;
  ducking: boolean;
  movingHorizontally: boolean;
  animationTimeMs: number;
}

export function resolvePlayerPose(context: PlayerPoseContext): PlayerPose {
  if (context.invulnerable) {
    return 'hurt';
  }

  if (!context.grounded) {
    return context.verticalVelocity < 0 ? 'jump' : 'fall';
  }

  if (context.ducking) {
    return 'duck';
  }

  if (context.movingHorizontally) {
    return Math.floor(context.animationTimeMs / 140) % 2 === 0 ? 'walk1' : 'walk2';
  }

  return 'idle';
}
