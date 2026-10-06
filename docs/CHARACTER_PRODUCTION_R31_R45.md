# Character Production Track — R31 to R45

This track replaces ad-hoc primitive-body iteration with a professional character production sequence.

## Guiding rule

Do not advance to the next R because code compiles. Advance only when the art gate for the current R is visually accepted.

## R31 — Concept / Reference Lock
- Lock identity, style, proportions, anatomy targets and required reference views.
- Produce/approve concept sheet.
- No production mesh yet.

Gate: user approves the visual target.

## R32 — Blockout
- Build the full body as large primary masses.
- Verify head/body ratio, shoulder width, rib cage, pelvis, limb length and foot contact.
- No surface detail.

Gate: front/side/back silhouette reads as a believable adult human.

## R33 — Primary Forms
- Resolve skull, jaw, neck, rib cage, pelvis, upper/lower limbs, hands and feet as coherent volumes.
- Remove mannequin/primitive appearance.

Gate: human form works before muscles, skin or textures.

## R34 — Secondary Forms / Anatomy
- Add anatomical landmarks, major muscle groups and soft-tissue transitions.
- Resolve clavicle, deltoid, elbow, pelvis, glute, knee, calf and ankle structures.

Gate: anatomy holds from front, side and 3/4 views.

## R35 — Tertiary Character Detail
- Refine face, eyelids, nose, lips, ears, fingers, toes and subtle surface forms.
- Keep detail appropriate for a mobile game rather than cinematic close-up.

Gate: close inspection reads as a finished human sculpt.

## R36 — Retopology
- Build animation-friendly game topology.
- Preserve loops for face, shoulder, elbow, pelvis/groin, knee and ankle.
- Establish mobile polygon budget.

Gate: deformation topology is clean and production-ready.

## R37 — UV / Baking
- Finalize UV layout.
- Bake high-frequency sculpt data to game mesh.
- Validate normals/tangents and artifact-free seams.

Gate: low-poly mesh retains the intended form.

## R38 — Materials / Hair / Base Garment
- Skin, eyes, hair and base garment materials.
- Use mobile-appropriate texture resolution and shader features.

Gate: character reads correctly under the actual URP lighting setup.

## R39 — Rigging
- Create Humanoid-compatible skeleton.
- Validate bone orientation, hierarchy, hands, feet, spine, neck and head.
- Add only justified helper bones.

Gate: valid Unity Humanoid Avatar.

## R40 — Skinning / Weight Painting
- Bind the mesh to the rig.
- Resolve shoulder, elbow, wrist, pelvis, knee and ankle deformation.

Gate: standard test poses do not collapse or tear.

## R41 — Corrective Deformation
- Add corrective blend shapes / pose-dependent fixes where skinning alone is insufficient.
- Preserve muscle and soft-tissue volume during movement.
- Target shoulder/armpit, elbow, abdomen, glute/hip, knee and calf transitions.

Gate: walking poses retain believable human volume.

## R42 — Locomotion Animation
- Idle, walk, run, start, stop and turn.
- Pelvis/shoulder counter-rotation, foot roll, stride, arm swing and center-of-mass motion.

Gate: motion reads naturally without game logic assistance.

## R43 — Unity Runtime Integration
- Animator Controller / Blend Tree.
- Movement direction drives facing and locomotion.
- Replace R30 primitive runtime body with the approved character prefab.

Gate: gameplay uses the production character path.

## R44 — Mobile Optimization
- LODs.
- Skin-weight limits.
- texture/memory budget.
- material count/draw-call review.
- mobile lighting/shadow review.

Gate: target Android device meets visual/performance budget.

## R45 — Device QA / Character Lock
- Verify front/back/side direction changes.
- Inspect idle/walk/run on physical phone.
- Capture close and gameplay-distance evidence.
- Fix remaining clipping, foot sliding, deformation and readability defects.

Gate: user visually approves the base human character.

## Definition of "character locked"

After R45, the base body is no longer redesigned casually.
New clothing, armor, classes and equipment must be layered on or derived from the locked base without breaking its rig/deformation contract.
