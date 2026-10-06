# R32 Character Blockout — Full Human Proportions

Status: IMPLEMENTED CANDIDATE — requires Android device visual approval.

## Goal

R32 is the first stage that changes the in-game body itself.

The R30/R31 stacked Unity primitive body is replaced by generated low-poly blockout meshes with human proportion targets. R32 is still a blockout: it establishes silhouette and primary body ratios before anatomy refinement in R33-R35.

## Proportion target

- Adult woman / novice / ordinary citizen.
- Approx. 7.3-7.5 head units from crown to sole.
- Head + torso to groin approximately 4 head units.
- Legs approximately 3.5 head units.
- Shoulder width near the realistic adult construction range rather than heroic exaggeration.
- Wrists reach approximately the upper-thigh / greater-trochanter region.
- Both legs must read separately from hip to foot.

## Runtime implementation

- New file: `RuntimeHumanBlockoutBuilder.cs`.
- Uses Unity `Mesh` geometry generated from custom elliptical body profiles.
- Torso, pelvis, limbs, head, hands and feet use purpose-built profile meshes rather than `GameObject.CreatePrimitive` character parts.
- Existing player root, movement, visual slot and future prefab replacement path are preserved.
- The ground shadow may remain a Unity primitive because it is not character anatomy.
- Existing R30 device-smoke log markers remain temporarily for backward-compatible CI; R32-specific markers are added.

## What R32 intentionally does not finish

- final facial anatomy
- fingers/toes
- muscle definition
- skin pores / tertiary detail
- production topology
- weight painting
- corrective muscle deformation
- final hair
- final locomotion

Those remain R33-R42 work.

## Device acceptance gate

R32 passes only if the physical phone capture shows:

1. Complete head-to-foot human silhouette.
2. Both thighs, knees, calves, ankles and feet are visible.
3. Arms have readable shoulder/elbow/wrist/hand proportions.
4. Torso transitions chest -> waist -> pelvis without the R30 stacked-block appearance.
5. Head size is believable relative to the body.
6. Gameplay-camera silhouette reads as an adult human rather than a mannequin made from capsules.
7. `BUILD R32` is visible on the installed build.
8. User approves the phone result.

## Research basis

- Realist figure construction commonly begins around 7-7.5 head units, with head/torso and limb landmarks used as approximate guides:
  https://www.artistsnetwork.com/art-techniques/drawing-proportions-human-body-ameral/
- Orthographic proportions are specifically useful for 3D modeling and sculpting:
  https://anatomy4sculptors.com/blog/about-human-proportions-calculator/
- Unity Mesh supports procedural runtime geometry through vertex and triangle data, with normals/bounds recalculation after creation:
  https://docs.unity3d.com/Manual/UsingtheMeshClass.html


## R32 proportion upgrade V2

The first deployed R32 candidate still read too short and childlike against the approved R31 reference sheet.

V2 corrections:
- smaller head mass to counter gameplay-camera perspective enlargement
- longer, slimmer neck
- longer chest/abdomen chain
- narrower waist and wider pelvis transition
- longer thighs and calves with a lower knee position
- longer arms with larger hand silhouette
- larger feet with a clearer forefoot/heel mass
- visual player scale increased moderately for phone readability
- model root raised so the longer legs retain the same ground-contact plane

The goal is not to add anatomy detail. It is to make the gameplay silhouette belong to the same adult-body proportion family as the R31 reference before R33 begins.

Additional runtime marker:
- `KAI_CHE_R32_PROPORTION_UPGRADE_V2_READY`


## R32 proportion upgrade V3 — 95% gate

R32 does not advance to R33 until the gameplay silhouette reaches the user's minimum 95% reference-similarity gate against the approved R31 concept sheet.

V3 calibration targets:
- projected crotch / lower-garment bottom near 52-53% of crown-to-sole screen height
- longer thighs and shins to counter perspective foreshortening
- narrower shoulder envelope
- smaller head/hair mass
- stronger chest -> waist -> pelvis transition
- longer arm reach with fingertips near/below the upper-thigh region
- briefs taper toward the leg openings instead of reading as shorts

V3 runtime marker:
- `KAI_CHE_R32_PROPORTION_UPGRADE_V3_95_GATE_READY`

Acceptance remains visual: APK install + physical-phone capture + direct comparison to the R31 reference. If the result is below 95%, R32 remains open and is revised again.


## R32 proportion upgrade V4 — landmark rebalance

V3 confirmed that total lower-body length and shoulder width were much closer, but the physical-device capture still showed:
- knee landmark too low relative to the R31 reference
- pelvis and upper thighs too narrow
- calf volume too weak
- neck too long
- head slightly too small
- briefs/lower-garment bottom slightly too low

V4 therefore preserves the V3 total crown-to-sole height while redistributing the lower limb:
- shorter femur block
- higher knee landmark
- longer shin/calf block
- fuller hip and upper-thigh volume
- slightly higher lower-garment edge
- larger feet
- shorter neck with a slightly larger head/hair envelope

Runtime marker:
- `KAI_CHE_R32_PROPORTION_UPGRADE_V4_95_GATE_READY`
