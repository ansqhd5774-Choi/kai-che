# R31 Character Concept & Reference Lock

Status: DESIGN GATE — no production mesh is accepted until this document and the visual concept sheet are approved.

## 1. R31 goal

R31 replaces the R30 "runtime primitive body" mindset with a professional character-art target.

The character must read as a believable adult human woman first, before clothing, class, equipment, combat style, or animation polish are layered on top.

R31 is a reference/decision stage. It does not attempt to fake detail by adding more Unity primitives.

## 2. Locked identity

- Adult human woman.
- Default novice / ordinary citizen.
- No class identity.
- No weapon, shield, armor, cape, helmet, staff, spell ornament, or heroic silhouette.
- Neutral, non-glamourized body designed to support later clothing and equipment.
- Short dark-brown hair for the base presentation.
- Warm medium skin tone.
- Neutral facial expression.
- Opaque dark-charcoal sports-bra-style top and opaque dark-charcoal briefs.
- No transparent material and no explicit anatomical detail.

## 3. Visual style target

Target style: believable semi-realistic game human.

Required:
- Human anatomy must be convincing at close inspection.
- Silhouette must remain readable at the current mobile gameplay camera distance.
- Body proportions must feel natural rather than heroic, chibi, mannequin-like, or fashion-illustration exaggerated.
- Face must read as an actual face, not as separate geometric pieces attached to a sphere.
- Hands and feet must have recognizable human structure.
- Muscle and fat volumes must be present but not bodybuilder-exaggerated.

Not the target:
- Photoreal cinematic human.
- Stylized anime/chibi proportions.
- Hyper-muscular fantasy hero.
- Doll/mannequin anatomy.
- Capsule/cube/sphere construction visible in the final model.

## 4. Proportion baseline

Use a neutral art baseline of approximately 7.5 head units for the standing adult figure.

R31 target scale:
- Character art scale: 1.65 m at neutral standing pose.
- Total figure: about 7.5 head units.
- Head + torso to crotch: about 4 head units.
- Legs: about 3.5-4 head units depending on final pelvis placement.
- Shoulder width: approximately 2 head widths as a starting reference, then adjusted to the chosen female body type.
- Arm span should remain close to total body height.
- Wrist level in neutral stance should fall near the upper-thigh / greater-trochanter region.
- Hand length should be roughly 0.7-0.8 head units.

These are construction guides, not rigid anatomical laws. The final model must be judged against coherent reference, not only numerical ratios.

## 5. Anatomy landmarks that must exist in the sculpt

The future R32-R35 model must preserve readable landmarks for:
- skull and jaw
- neck cylinder / sternocleidomastoid region
- clavicles
- shoulder cap / deltoid transition
- rib-cage volume
- waist transition
- pelvis / iliac crest region
- gluteal mass
- elbow
- wrist
- hand palm and fingers
- greater trochanter / hip transition
- patella / knee block
- calf volume
- ankle malleoli
- heel, arch, forefoot and toes silhouette

Soft tissue should connect these landmarks naturally. No body segment may look like an isolated primitive.

## 6. Face reference baseline

The face must be built as one coherent head form.

Reference landmarks:
- Eyes sit approximately around the vertical midpoint of the adult head.
- Inter-eye spacing starts around one eye width.
- Nose base sits roughly halfway between brow line and chin.
- Ears generally occupy the brow-to-nose vertical zone.
- Mouth sits between nose and chin and must be integrated into the muzzle/jaw volume.
- Eyelids wrap around the eyeballs; eyes must not read as dots pasted onto the face.
- Nose must project from the facial planes.
- Lips require upper/lower volume and mouth corners, not a flat line.

R31 does not lock a specific ethnicity or celebrity likeness. The target is a neutral original game character.

## 7. Body composition target

- Ordinary healthy adult body.
- Moderate subcutaneous softness.
- Subtle muscle definition only where it helps form readability.
- No bodybuilding striation.
- No extreme hourglass exaggeration.
- No unnaturally narrow waist.
- No oversized chest, hips, hands, head, or limbs.
- Left/right anatomy must be symmetrical in the base neutral pose before natural asymmetry is considered later.

## 8. Future deformation requirement

The model must be designed from the start for believable movement.

Areas requiring enough topology and volume for later deformation:
- neck
- shoulder / armpit
- elbow
- wrist
- spine / abdomen
- pelvis / groin
- glute / upper thigh
- knee
- ankle

R41 corrective deformation will later address pose-dependent muscle and joint shape changes. R31 therefore forbids a topology concept that cannot support these areas.

## 9. Required reference views

Before R32 blockout begins, the approved concept package must contain:

1. Front orthographic-style neutral view.
2. Side neutral view.
3. Back neutral view.
4. Front 3/4 view.
5. Face close-up: front.
6. Face close-up: side or 3/4.
7. Hand reference.
8. Foot reference.
9. Gameplay-scale silhouette preview.

Pose for modeling reference:
- relaxed A-pose, not rigid T-pose
- feet parallel and fully visible
- fingers separated enough to understand hand construction
- neutral head orientation

## 10. Mobile readability gate

The character will be judged at two distances:

### Close inspection
Must show:
- recognizable eyes, eyelids, nose, lips, ears and jaw
- continuous arm and leg anatomy
- hands and feet
- believable torso/pelvis transition

### Gameplay camera
Must show:
- head/body/arms/legs as one coherent human silhouette
- clear separation between both legs
- clear foot contact with ground
- shoulders and pelvis orientation
- no missing-face or amputated-leg appearance

Fine skin pores are not an R31 requirement.

## 11. Production rules

- R30 primitive geometry is reference for failure analysis only; do not refine it into the production body.
- The production character must use a continuous skinned character mesh.
- Unity runtime target is a Skinned Mesh Renderer / Humanoid-ready character.
- Blend Shapes may be used later for facial and corrective deformation.
- All imported assets must have clear commercial-use licensing.
- No unlicensed reference image or third-party mesh is to be committed as a game asset.
- External reference links may be documented, but copyrighted images are not copied into the repository unless their license permits it.

## 12. R31 rejection checklist

Reject the concept if any of the following is true:
- body still reads as stacked primitives
- face disappears at modest zoom
- thighs/shins/feet are not separately readable
- arms look tubular without elbow/wrist structure
- pelvis-to-leg connection is ambiguous
- proportions read as chibi, fashion-idealized, or superhero-like
- body cannot plausibly support Humanoid rigging
- reference views disagree with each other
- base design carries a warrior/mage/class identity

## 13. R31 acceptance gate

R31 is complete only when:

1. This specification is merged into the integration branch.
2. A front/side/back/3/4 concept sheet exists.
3. Face, hand and foot references exist.
4. Gameplay-scale silhouette has been reviewed.
5. No class/equipment identity remains.
6. The user visually approves the R31 concept.
7. Only then does R32 blockout start.

## Research basis

- Autodesk production example: concept sketches establish proportions, personality, silhouette and costume before modeling/rigging.
  https://blogs.autodesk.com/media-and-entertainment/2026/07/10/ai-motion-capture-for-beginners-autodesk-flow-studio/
- Artists Network: realist adult construction commonly uses about 7-7.5 head units and head-unit landmark checks.
  https://www.artistsnetwork.com/art-techniques/drawing-proportions-human-body-ameral/
- Proko: proportion systems are construction guides and must be combined with observation/reference.
  https://www.proko.com/course-lesson/human-proportions-average-figure
- Anatomy for Sculptors: orthographic proportions are especially relevant to 3D modeling/sculpting; bony landmarks are more reliable than soft-tissue landmarks.
  https://anatomy4sculptors.com/blog/about-human-proportions-calculator/
  https://anatomy4sculptors.com/blog/anatomy-for-artists/
- Unity 6: deformable humanoid production meshes are handled through Skinned Mesh Renderer; blend shapes and multiple bone influences are supported.
  https://docs.unity3d.com/kr/current/Manual/class-SkinnedMeshRenderer.html
