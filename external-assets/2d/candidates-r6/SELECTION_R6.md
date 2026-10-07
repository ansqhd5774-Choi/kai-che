# Kai-Che 2D Asset Selection R6 — Deployment Unblocked

Date: 2026-10-07

## Selected first deployment candidate

Pico Heroes Sprite Pack — Mage

## Mandatory checks
- 8 directions: PASS
- Mage class exists: PASS
- Idle: PASS
- Walk: PASS
- Attack: PASS
- Hit: PASS
- Death: PASS
- Commercial game use: PASS
- Same-style expansion database: PASS (24 archetypes)
- No purchase required: PASS

## Resolution exception
The previous R4 native-height preference was too rigid for pixel art.

For true pixel-art assets, native 24x32 or 32x32 is allowed when:
1. scaling uses an exact integer multiplier;
2. texture filtering is Point/Nearest;
3. compression does not blur edges;
4. aspect ratio remains unchanged;
5. 96 px-class phone readability passes;
6. no synthetic interpolation is used.

For Pico Heroes, target display scale is x4.

## Final gate before FINAL status
- actual archive integrity check;
- mage sprite visual inspection;
- exact frame count verification;
- Unity 8-direction controller test;
- phone screenshot inspection;
- 60 FPS target test.

This candidate replaces the paid-only candidates as the first no-cost deployment path.
