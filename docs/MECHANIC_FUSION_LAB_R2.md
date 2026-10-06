# Mechanic Fusion Lab R2

Status: ACTIVE IDEATION
Branch: new-game/bootstrap
Source drift checked against head 6df632e11a78bab7e7215d4a3e583f2f4393c1af.

## R2 rule

R1 survivors are not polished as-is.
For each candidate:
1. remove the obvious metaphor,
2. replace one mechanic atom,
3. change the player's main verb,
4. define a 30-second loop,
5. search for close current games,
6. reject if the core can be described by naming an existing title.

## Search findings affecting R2

### Hidden-rule space is crowded
Current/known examples include:
- Samewise: infer a secret transformation from examples, then apply it.
- Understand: infer changing rules by trial and error.
- IFO: experiment with an alien interface and discover its operating rules.
- HOX / cellular-automata puzzles: design or infer rules that evolve a field.

Therefore R1 "Hidden Physics" is rejected in its original form.
"Discover a hidden law, then predict a simulation" is not enough novelty by itself.

### Orbit / synchronization space is also crowded
Current examples include:
- Orbit Puzzle
- Planet Orbit Lock
- RoboSync
- timing/synchronization puzzle variants

Therefore R1 "Echo Orbit" loses orbit, ring, collision, billiard, and synchronized-robot metaphors.

### Existing guardrail remains
AfterMove already uses one player move causing all other pieces to react according to fixed rules.
Reject any global-reaction design that converges on that loop.

---

# R2-A — Memory Forecast

Derived from Echo Orbit.

## Atoms
- Into the Breach: future state is previewed before commitment
- Simon: information is briefly shown, then must be retained
- Snake: successful play creates future constraint
- Sokoban: a decision permanently removes future options

## Main verb
LOCK

The player does not move an avatar or projectile.
The player locks one future slot/state.

## 30-second loop
1. The field displays four future snapshots for ~2 seconds.
2. The snapshots disappear.
3. The present field remains visible.
4. The player locks exactly one region/state that must remain unchanged through the next resolution.
5. The system advances one step.
6. If the lock was strategically correct, the immediate crisis resolves.
7. The locked state becomes permanent, reducing future freedom.
8. A new preview appears, now with more interacting elements.

## Why it may work
- memory and prediction happen before the same decision
- success itself creates difficulty
- simple input: select + lock
- no falling blocks, path drawing, line clear, orbit, or chain-reaction tap

## Risks
- language ability is weak
- future-preview tactics can still remind players of Into the Breach
- permanent locking can drift toward wall-placement games

## R2 verdict
SURVIVE, but not prototype-ready.

---

# R2-B — Causal Seal

Derived from Forecast Trap.

## Atoms
- Minesweeper: local clues imply hidden global state
- Into the Breach: future danger is shown before it resolves
- Wordle: each result narrows remaining hypotheses
- Sokoban: commitment is irreversible

## Main verb
SEAL

The player does not attack a visible hazard.
The player seals one suspected cause.

## 30-second loop
1. Two or three future disturbances are telegraphed.
2. Small local clues appear around several possible causes.
3. The player infers which cause is generating which future disturbance.
4. One cause is sealed permanently.
5. The next state resolves.
6. The result provides structured feedback that narrows the remaining hidden mapping.
7. The permanent seal changes the topology of the next decision.
8. New disturbances appear.

## Why it may work
- the target is a cause, not a visible enemy
- deduction, prediction, judgment and insight are tightly coupled
- mistakes can be understood rather than feeling random
- supports very short rounds with deep thought

## Risks
- can feel like a static logic exercise if execution feedback is weak
- language ability is not intrinsic yet
- "clue -> accuse cause" can drift toward detective/deduction interfaces

## R2 verdict
SURVIVE, but not prototype-ready.

---

# R2-C — Hidden Physics

Original R1 candidate.

## Reason for rejection
Its core was:
- observe hidden rules,
- remember/infer them,
- adjust one parameter,
- predict system evolution.

This space overlaps strongly with Samewise, Understand, IFO, HOX and cellular-automata puzzle experiments.

## R2 verdict
REJECT.

Keep only isolated atoms for later combinations.

---

# Language requirement audit

The project goal includes language ability.
R2-A and R2-B currently do not use language intrinsically.

Important constraint:
Do NOT "add words" cosmetically.
That would repeat the failed Wordtris-style mistake.

Language must affect the decision itself, for example through:
- semantic ambiguity,
- category relation,
- synonym/antonym relation,
- contextual meaning,
- sentence-role logic,
without turning the whole game into a word quiz.

This requirement becomes the main gate for R3.

---

# R2 scoring

Scale: 1-5.

| Candidate | Novelty | Deep thought | Simplicity | Replayability | Memory | Attention | Spatial | Judgment | Insight | Prediction | Language | Result |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| Memory Forecast | 4 | 4 | 5 | 4 | 5 | 4 | 4 | 4 | 3 | 5 | 1 | SURVIVE |
| Causal Seal | 4 | 5 | 4 | 4 | 3 | 4 | 3 | 5 | 5 | 5 | 1 | SURVIVE |
| Hidden Physics | 2 | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 5 | 5 | 1 | REJECT |

## Current leader
Causal Seal has the strongest "think hard -> infer -> commit -> see if the model was right" loop.

Memory Forecast has the cleanest controls and strongest memory component.

Neither is accepted as the final game yet.

---

# R3 instruction

Create new combinations by injecting exactly one language atom into each survivor without adding a separate word mini-game.

Candidate language atoms to test:
- Connections: semantic grouping / multiple plausible meanings
- Scrabble: lexical choice has spatial opportunity cost
- Wordle: lexical hypotheses narrowed by feedback
- crosswords: intersecting constraints let one answer reveal another
- Papers, Please: language/rule interpretation under accumulating constraints

For each survivor:
1. add one language atom,
2. remove one weaker atom,
3. define one-sentence rule,
4. ensure control count stays <= 2,
5. search for direct analogues,
6. reject if it feels like a quiz or an existing word puzzle,
7. keep only candidates where language, space, prediction and judgment happen in the same action.
