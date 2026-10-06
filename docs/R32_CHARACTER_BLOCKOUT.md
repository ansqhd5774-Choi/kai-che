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
