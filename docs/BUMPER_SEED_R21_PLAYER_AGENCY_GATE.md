# Bumper Seed R21 — Player Agency Gate

Status: IMPLEMENTATION
Date: 2026-10-07
Base: R20 validated feedback build

## Why R21 exists

R20 improved impact feel, but the player could still feel that target hits happened by luck because tap position did not affect the rebound direction.

R21 changes that relationship directly:

- Left third tap = left-biased rebound.
- Center third tap = straight inward rebound.
- Right third tap = right-biased rebound.
- Three short trajectory guides appear at wall impact.
- The selected trajectory is highlighted.
- The new bumper keeps the three-bumper FIFO rule.
- The new bumper collider is armed after a short delay so the chosen rebound is not immediately overwritten.

## Locked systems retained

- One-touch input.
- Automatic ball motion.
- Maximum three bumpers.
- Oldest-bumper deletion rule.
- Sequential targets.
- R20 audio, flash, pulse, and camera shake.

## Acceptance gate

R21 passes only when a real Android device confirms:

- R19 core marker is active.
- R20 feedback marker is active.
- R21 player-agency marker is active.
- APK builds, installs, and launches.
- Left / center / right input creates visibly different rebound paths.
- Selected trajectory feedback is readable during motion.
- The ball remains controllable and does not stick or tunnel.
- Three-bumper FIFO remains intact.
- The player can intentionally aim toward a target more reliably than R20.
