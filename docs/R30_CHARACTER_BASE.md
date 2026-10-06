# R30 Character Base — Novice Human

## Locked character identity
- Adult human woman.
- No class/job identity.
- Represents the default novice / ordinary citizen body.
- No weapon, shield, armor, cape, helmet, magical staff, or class ornament.
- This body is the visual base for future clothing, armor, class, and equipment layers.

## Base appearance
- Full human silhouette: head, face landmarks, neck, torso, shoulders, arms, hands, pelvis, thighs, knees, shins, and feet.
- Neutral mobile-readable proportions rather than exaggerated heroic armor proportions.
- Short dark-brown hair is included only to prevent the runtime base from reading as an unfinished mannequin.
- Warm medium skin tone.
- Neutral face with simple eyes, nose, and mouth.

## Base garment
- Opaque dark charcoal sports-bra-style top.
- Opaque dark charcoal briefs.
- No transparent material.
- No exposed nipples or genital detail.
- Garments are intentionally simple so future equipment can fully replace or cover them.

## Runtime rules
- Content id: `player.novice.base`.
- Runtime prefab override path: `Resources/KaiChe/CombatVisuals/PlayerNovice`.
- Current R30 implementation is a self-contained runtime 3D base so Android builds do not depend on an external art package.
- If a production skinned `PlayerNovice` prefab is added later, the existing resolver can replace the runtime body without changing combat logic.
- R30 removes the old sword/shield/armor/cape placeholder from the primary player visual path.

## Acceptance gate
R30 is accepted only when:
1. Unity preflight/build passes.
2. Android APK installs and launches on the paired device.
3. Runtime logs contain `KAI_CHE_R30_NOVICE_BASE_BODY_ACTIVE` and `KAI_CHE_R30_NOVICE_BASE_BODY_READY`.
4. Device screenshot evidence is captured.
5. The user visually approves the phone result.

Until item 5, treat R30 as a deployed visual candidate rather than a final locked art asset.
