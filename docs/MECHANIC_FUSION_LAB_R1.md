# Mechanic Fusion Lab R1

Status: ACTIVE IDEATION
Branch: new-game/bootstrap
Source drift checked against head 519c4f07745037da047d0deb667bf760ca5f5872.

## Method

Do not import a whole game loop.
Extract exactly one advantage from each reference game, combine 4 atoms, then reject if the result strongly resembles an existing game.

Known cognitive targets from current discussion:
- 기억력
- 주의력
- 언어 능력
- 공간 인지력
- 판단력
- 통찰력
- 예측력
- target 8: TBD
- target 9: TBD

## Atom pool

- Tetris: escalating pressure
- Minesweeper: local clues imply hidden global state
- Sokoban: choices can permanently reduce future options
- 2048: one input can alter the global state
- Snake: success itself creates future constraint
- Pac-Man: moving threats continuously change route value
- Breakout: prediction of future trajectories matters more than the current position
- Simon: briefly seen information must be retained
- Wordle: feedback progressively narrows hypotheses
- Baba Is You: rules themselves can change during play
- Into the Breach: future danger is shown before resolution
- Lemmings: indirect intervention rather than direct unit control

## Existing-overlap guardrail

AfterMove (2026) already uses: move one piece, all other pieces react by their own rules.
Reject any candidate whose core becomes "one move -> every piece automatically reacts according to its rule."

## Candidate A — Pressure Deduction Lock

Atoms:
- Tetris pressure
- Minesweeper clue inference
- Sokoban irreversibility
- Wordle narrowing feedback

Loop:
A field contains partially known relationships. Each turn the player permanently locks one relation. Locking reveals feedback that narrows the hidden solution, while pressure continuously rises.

Strengths:
- deep deduction
- irreversible commitment creates tension
- simple input

Risks:
- can feel like Mastermind/Minesweeper under a timer
- spatial cognition may be weak

Verdict: HOLD

## Candidate B — Echo Orbit

Atoms:
- Snake success increases constraint
- Into the Breach future preview
- Simon transient memory
- Breakout trajectory prediction

Loop:
Several objects move on intersecting orbits. Short future paths are previewed. A brief symbol rule is shown and then hidden. The player makes one small intervention to create a target encounter. Each successful encounter leaves a permanent object/rule trace, making later predictions harder.

Strengths:
- prediction-heavy
- memory and attention are naturally simultaneous
- success makes the game harder rather than merely faster

Risks:
- could resemble trajectory/billiards puzzle if intervention is too physical

Verdict: SURVIVE

## Candidate C — Rule Forecast

Atoms:
- Baba rule change
- Into the Breach future preview
- Pac-Man moving threat
- Minesweeper partial clues

Loop:
Autonomous threats move continuously. The next state is previewed, but the rule producing later states is only partially known through local clues. The player changes one abstract relationship before execution, then observes whether the forecast was correctly interpreted.

Strengths:
- strong insight + deduction + prediction
- dynamic board
- simple decision surface

Risks:
- rule editing can drift too close to Baba/IF programming games
- needs a non-textual rule manipulation method

Verdict: HOLD

## Candidate D — Signal Garden

Atoms:
- Lemmings indirect intervention
- Wordle narrowing feedback
- Sokoban irreversible commitment
- Tetris escalating pressure

Loop:
Autonomous signals enter a network. The player cannot move them directly and may alter only one junction per cycle. Each altered junction becomes locked. Outcome feedback narrows which routing principle is correct while more signals enter over time.

Strengths:
- indirect control
- planning under pressure
- memory/attention can scale naturally

Risks:
- risks resembling Mini Metro / train-routing games

Verdict: REJECT unless routing metaphor is removed

## Candidate E — Hidden Physics

Atoms:
- Breakout trajectory prediction
- Simon memory
- Baba changing rules
- Tetris pressure

Loop:
Moving elements follow a small set of physical-looking interaction rules. The current rule set is shown briefly, then hidden. The player changes only one field parameter before the next motion cycle. Pressure rises by adding interactions, not by increasing button complexity.

Strengths:
- spatial + memory + prediction simultaneous
- can remain visually simple
- high "I calculated that" payoff

Risks:
- can become a generic laser/reflection puzzle
- must avoid mirrors, balls, paddles, lasers

Verdict: SURVIVE

## Candidate F — Forecast Trap

Atoms:
- Minesweeper clue inference
- Into the Breach telegraphed future
- Snake success creates constraint
- Wordle narrowing feedback

Loop:
The board shows multiple future hazards and partial clues about the hidden cause behind them. The player neutralizes one cause per turn. A correct solve leaves behind a permanent constraint, reducing future safe options; feedback narrows the remaining hidden causes.

Strengths:
- high deduction
- perfect-information and hidden-information tension
- success naturally raises difficulty

Risks:
- needs a strong visual metaphor
- may be too turn-based/clinical without satisfying execution

Verdict: SURVIVE

## Candidate G — Global Reaction Board

Atoms:
- 2048 global effect
- Pac-Man dynamic threat
- Simon memory
- Sokoban consequence

Result resembles "one move -> many pieces react" designs, especially AfterMove.

Verdict: REJECT

## Candidate H — Rule Shepherd

Atoms:
- Lemmings indirect control
- Baba mutable rules
- Tetris pressure
- Wordle feedback

Result trends toward rule-programming / automation puzzle territory.

Verdict: REJECT

## R1 survivors

1. Echo Orbit
2. Hidden Physics
3. Forecast Trap

## R2 instruction

For each survivor:
1. Remove its obvious metaphor.
2. Replace the intervention verb with a different interaction.
3. Define a 30-second playable loop.
4. Check current games for close matches.
5. Score: novelty, fun, cognitive load, simplicity, replayability.
6. Only then choose one prototype candidate.
