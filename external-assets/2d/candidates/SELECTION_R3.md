# 2D Wizard Asset Selection R4 — 8 Direction Gate

Selection date: 2026-10-07
Status: ACTIVE

## Core correction

For Kai-Che's new 2D free-movement RPG, 8-direction character support is a HARD REQUIREMENT.

The previous three side-view candidates are INVALID for the player-character role:
1. Evil Wizard 3 — REJECTED: side-view only
2. EVil Wizard 2 — REJECTED: side-view only
3. Wizard Pack — REJECTED: side-view only

They may remain archived only as style/reference material. They must not be used as the controllable player character.

## Gate 1 — mandatory requirements

Any candidate fails immediately if any item below is missing.

### Direction
- Minimum movement directions: 8
- Required direction set: N, NE, E, SE, S, SW, W, NW
- Direction changes must swap to distinct directional sprites/animations.
- Mirroring may be used only if visual asymmetry is not broken; mirrored-only 4-direction packs do not qualify as native 8-direction.
- Character must visibly face the input direction within 1 animation update.

### Core animation coverage per direction
Required for all 8 directions unless noted:
- Idle: >= 4 frames
- Walk or Run: >= 6 frames
- Attack or Cast: >= 6 frames
- Hit: >= 2 frames
- Death: >= 4 frames

Preferred:
- Idle: >= 6
- Walk: >= 8
- Run: >= 8
- Attack/Cast: >= 8
- Hit: >= 3
- Death: >= 6

### Sprite technical quality
- Transparent PNG: required
- Consistent frame box size: 100%
- Pivot/feet alignment deviation: <= 2 px target, <= 3 px absolute maximum
- No clipping in any required frame
- Native character visible height: >= 64 px preferred
- Minimum acceptable native visible height: 48 px only if 96 px mobile readability test passes
- Sprite-sheet structure must be deterministic enough for automated slicing or explicit frame metadata

### Mobile readability
Must pass all:
- 100% scale: face/body/weapon identifiable
- 50% scale: class silhouette identifiable
- 25% scale: facing direction identifiable
- 96 px displayed height: weapon, head direction, and body orientation distinguishable
- 10 simultaneous actors: player remains distinguishable from enemies/background

### Visual quality
- No broken outline pixels or frame artifacts
- No animation frame with obvious anatomy/gear discontinuity
- Directional consistency: costume, weapon hand, accessories and proportions remain consistent across all 8 views
- Player silhouette must remain recognizable in grayscale
- Main-character candidate must score >= 90/100 after visual, animation and expansion scoring

### License
Required:
- Commercial use allowed
- Modification allowed
- Original creator identifiable
- Original source URL saved
- License text/type saved
- Redistribution conditions understood before repository storage
- Ambiguous or missing license: automatic reject

### Expansion ecosystem
Minimum:
- Same or strongly compatible style must support at least 3 additional character archetypes
- >= 10 enemy/monster candidates in compatible style OR a practical recolor/variant path
- At least one compatible environment/tile set
- At least one compatible VFX set or clear ability to create matching VFX
- If none exist, candidate is prototype-only and cannot be selected as final main-character art direction

## Scoring after Gate 1 — 100 points

### A. Visual quality — 35
- First impression / hero appeal: 8
- Silhouette quality: 6
- 8-direction consistency: 8
- Mobile readability: 6
- Color/value separation: 3
- Detail quality: 4

### B. Animation quality — 35
- 8-direction movement quality: 10
- 8-direction attack/cast quality: 10
- Idle quality: 4
- Hit reaction: 3
- Death: 3
- Foot sliding / pivot stability: 3
- Loop transitions: 2

### C. Expansion / production suitability — 30
- Same-style player classes: 5
- Monsters: 5
- NPCs: 3
- Environment/tiles: 4
- VFX: 4
- Weapons/equipment: 3
- Boss expansion: 2
- UI/icon compatibility: 2
- Palette/variant editing: 2

## Final grades
- S+: 95–100 and Gate 1 PASS
- S: 90–94 and Gate 1 PASS
- A: 82–89 and Gate 1 PASS
- B: 72–81 and Gate 1 PASS
- REJECTED: Gate 1 fail or score <= 71

Kai-Che playable main character:
- S grade minimum
- Native 8-direction movement mandatory
- No side-view-only assets
- No 4-direction-only assets
- No "we can add the missing directions later" assumption during selection

## Real-game validation before adoption
Top candidates must pass:
1. Unity import and deterministic slicing
2. 8-direction input rotation/facing test
3. 30 seconds continuous free movement
4. 10 turns through all eight directions
5. 10 attack/cast actions from each direction
6. 10 hit reactions
7. 5 deaths
8. 10 simultaneous enemies
9. 96 px-class mobile display test
10. Real-device 60 FPS target test

Only after these tests may one asset be marked FINAL.
