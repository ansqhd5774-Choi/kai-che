# Bumper Seed R20 — Impact Feel & Feedback Gate

Status: IMPLEMENTATION
Date: 2026-10-07
Base: R19 validated playable core

## Locked rule

R20 must not change the R19 one-tap / three-bumper gameplay rules.

## R20 additions

- Wall impact sound.
- Bumper creation sound.
- Target success sound.
- Short camera shake with event-specific strength.
- Short screen flash/pulse with event-specific color.
- Spawn pulse on newly created bumpers.
- Success pulse on the active target.
- Runtime-generated audio so no external audio asset is required for this prototype gate.

## Acceptance gate

R20 passes only when a real Android device confirms:

- R19 core marker is still active.
- R20 feedback marker is active.
- APK builds and installs over the isolated Bumper Seed package.
- Runtime launches without crash.
- Impact, bumper creation, and target success are perceptibly distinct.
- Camera shake is brief and does not reduce play readability.
- Screen flashes are brief and do not obscure the arena.
- Ball speed and three-bumper FIFO rules remain unchanged.
