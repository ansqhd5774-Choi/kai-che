# Mechanic Fusion Lab R3 — Global-First

Status: ACTIVE IDEATION
Branch: new-game/bootstrap
Source drift checked against head 8a6ce8815626d7fb4e8209db63cf25c1c097dff4.

## Hard constraints

- No mandatory natural language in the playfield.
- Core must work with icons, shapes, quantity, motion, timing and spatial relations only.
- Maximum 2 essential input types.
- Do not copy an existing game's full loop.
- One famous game contributes only one design advantage.
- If a candidate can be described as "basically X", reject it.

## Current web overlap notes

- Timelie already provides direct future-state preview plus editable/reversible timeline.
- Freeze! already uses world rotation plus a freeze control as a physics puzzle.
- Grid & Guilt (2026 demo) already turns successful actions into future permanent hazards.
- Undercell already uses logical deduction plus locking cells/groups.
- Generic global-toggle and queued-action puzzle spaces are crowded.
- Search did not surface a close abstract 2D puzzle whose core is "preview several future events, veto exactly one, then the remaining events resolve."

---

# R3-A — Future Veto

## Atoms
- Into the Breach: future danger is revealed before resolution
- Lemmings: indirect intervention instead of direct control
- Simon: briefly shown information must be retained
- Tetris: pressure steadily increases

## Main verb
VETO

The player never moves a piece.

## One-sentence rule
Preview the next three events, remember them, then veto exactly one; the others happen.

## 30-second loop
1. Three future event icons appear for ~2 seconds.
2. Their detailed paths/effects animate briefly.
3. Details disappear, leaving only the current field.
4. Player taps one of the three event icons to veto it.
5. The other two events execute in order.
6. The field changes.
7. One additional future event is added to the next cycle as pressure rises.
8. Repeat until no safe veto exists.

## Cognitive load
- memory: retain the hidden event details
- attention: compare multiple future events
- spatial cognition: predict where each event changes the field
- judgment: choose which future is most dangerous
- insight: detect interaction between two events
- prediction: simulate the two events that will remain

## Strength
The action itself is unusual: the player controls the future by prohibition, not movement.

## Risk
If future events behave like cards, it may feel like a card battler.
Visual treatment must remain spatial and systemic, not deck-based.

## Verdict
SURVIVE — strongest R3 novelty candidate.

---

# R3-B — Causal Seal Zero-Text

## Atoms
- Minesweeper: local clues reveal hidden global state
- Into the Breach: future disturbance is telegraphed
- Wordle: feedback narrows the hypothesis space
- Sokoban: commitment is irreversible

## Main verb
SEAL

## One-sentence rule
Read non-verbal clues, infer which hidden source will cause the shown future disturbance, and permanently seal one source.

## 30-second loop
1. Several abstract sources pulse.
2. Nearby shape/quantity clues indicate partial causal relationships.
3. Two future disturbances are shown.
4. Player seals one suspected source.
5. Resolution occurs.
6. Result feedback narrows the remaining mapping.
7. The seal stays permanently and changes future geometry.
8. New sources/disturbances enter.

## Cognitive load
- attention: local clues + future disturbance
- spatial cognition: source-to-effect geometry
- judgment: choose a source to seal
- insight: infer hidden causal mapping
- prediction: anticipate the next disturbance
- memory: retain prior mappings/feedback

## Search risk
Undercell already combines deduction and locking, but its objective is identifying safe/piece cells.
Proofloom explores causal reasoning but is text-heavy and chain-building.
No close match found for this exact non-verbal moving-future + permanent-cause-seal loop.

## Verdict
SURVIVE — deepest reasoning candidate, but must avoid feeling like a logic worksheet.

---

# R3-C — Temporal Pin

## Atoms
- Into the Breach: future preview
- Simon: transient memory
- Snake: success creates future constraints
- Sokoban: irreversible commitment

## Main verb
PIN

## One-sentence rule
Memorize the next state, then pin exactly one object so it refuses the next transformation.

## 30-second loop
1. Next transformation is previewed.
2. Preview disappears.
3. Player pins one object.
4. Transformation resolves everywhere except the pinned object.
5. The pin remains as a permanent obstacle/constraint.
6. Repeat with more objects.

## Overlap risk
Freeze! already uses a freeze control in physics puzzles.
Timelie already previews and manipulates deterministic future states.
The exact pin-one-object/permanent-constraint loop differs, but the conceptual neighborhood is crowded.

## Verdict
HOLD, below A/B.

---

# R3-D — Rule Toggle

## Atoms
- Baba Is You: rules can change
- Pac-Man: moving threats change route value
- Breakout: trajectory prediction
- Lemmings: indirect intervention

## Main verb
TOGGLE

## One-sentence rule
Toggle one global interaction law at the right moment so autonomous objects resolve safely.

## Overlap risk
Too close to broad global-toggle / physics-rule puzzle territory.

## Verdict
REJECT.

---

# R3-E — Global Reaction Memory

## Atoms
- 2048: one action changes the whole state
- Simon: transient memory
- Sokoban: future consequences
- Pac-Man: moving hazards

## Overlap risk
Converges toward existing global reaction games such as AfterMove and toggle-state puzzle designs.

## Verdict
REJECT.

---

# R3 ranking

| Candidate | Novelty | Deep thought | Simplicity | Global | Replayability | Memory | Attention | Spatial | Judgment | Insight | Prediction |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Future Veto | 5 | 4 | 5 | 5 | 5 | 4 | 5 | 4 | 5 | 4 | 5 |
| Causal Seal Zero-Text | 4 | 5 | 4 | 5 | 4 | 3 | 5 | 4 | 5 | 5 | 5 |
| Temporal Pin | 3 | 4 | 5 | 5 | 4 | 5 | 4 | 4 | 4 | 3 | 5 |

## Current R3 leader

Future Veto.

Why:
- one action is easy to explain globally,
- no direct movement/control language is needed,
- future reasoning is central,
- memory can be intrinsic without a separate memory mini-game,
- difficulty scales by interactions and preview count, not extra controls,
- current search did not reveal a close abstract 2D puzzle with the same core loop.

## Important caveat

Novelty is not proven.
Search can only reduce obvious overlap risk; it cannot prove that no similar game exists.

---

# R4 instruction

Build paper-prototype rules for Future Veto and Causal Seal Zero-Text only.

For each:
1. fixed 6x6 or non-grid spatial field,
2. exactly 3 event/source types,
3. one fail condition,
4. one scoring rule,
5. a 60-second example run,
6. difficulty levels 1-5,
7. verify that every playfield instruction can be communicated without words,
8. search again using the final concrete mechanic, not the abstract concept.

Do not code yet.
