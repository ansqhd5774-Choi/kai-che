# New Game Fusion Concept V0.1

Status: DRAFT  
Branch: `new-game/bootstrap`

## Goal

Create an original 2D game with Tetris-level input simplicity while making each placement demand multiple cognitive processes at once.

The design must not become a collection of separate mini-games. The mechanics must collapse into one continuous core loop.

## Confirmed cognitive targets

1. 기억력
2. 주의력
3. 언어 능력
4. 공간 인지력
5. 판단력
6. 통찰력
7. 예측력
8. TBD
9. TBD

Do not invent targets 8 and 9 until explicitly defined.

## Source mechanics retained

### Tetris
Retain:
- Continuous spatial pressure
- Rotate / move / place
- Board state becomes the main source of difficulty

Discard:
- Tetromino identity as a requirement
- Exact Tetris line-clear rules if they make the game derivative

### Simon
Retain:
- Briefly shown information
- Information must be kept in memory while acting
- Difficulty can rise by increasing sequence/rule length

Discard:
- Separate repeat-the-sequence phase
- Four-button imitation loop

### Wordle
Retain:
- Feedback after a decision
- Correct / partially useful / wrong information can drive inference
- Language information can be part of the same board

Discard:
- Six-guess structure
- Standalone word-guess screen

### Sokoban
Retain:
- Placement should have consequences
- Good play requires thinking ahead
- Some mistakes should create future constraints rather than immediate failure

Discard:
- Walking character
- Box-pushing map structure

### 2048 / merge puzzles
Retain:
- One simple action can change several future possibilities
- Successful patterns can trigger compact chain reactions

Discard:
- Whole-board swipe control

## Fusion rule V0.1

**Remember a briefly shown rule, then rotate and place falling information tiles so that both the spatial pattern and the information rule are satisfied.**

The player does not switch into separate memory, word, or logic mini-games. Every falling tile is part of the same board and the same decision.

## Board object

Use an original `information tile`, not a Tetromino.

Initial prototype candidate:
- 1x2 tile
- each half contains one symbol/letter/value
- tile can rotate
- player moves left/right and drops

Exact tile geometry is not final.

## Core loop

1. A short rule or sequence is shown briefly.
2. The rule disappears or becomes partially hidden.
3. An information tile enters the board.
4. Player rotates / moves / places it.
5. Placement receives lightweight feedback.
6. Correct relationships form a chain.
7. Completed chains clear or compress the board.
8. New information arrives while the previous board state remains relevant.
9. Speed / ambiguity / rule length gradually increase.

## Cognitive mapping

| Target | How the same placement uses it |
|---|---|
| 기억력 | hold the temporarily shown rule/sequence |
| 주의력 | track current tile, board, cue and danger simultaneously |
| 언어 능력 | interpret letters, syllables, words or semantic relations |
| 공간 인지력 | rotate and position the tile |
| 판단력 | choose the best valid placement now |
| 통찰력 | infer the hidden relation/pattern |
| 예측력 | anticipate future tiles and board consequences |
| TBD | not assigned |
| TBD | not assigned |

## Example prototype rule

A three-element relation is flashed briefly, for example:

`A → C → B`

The cue then disappears.

Falling tiles contain letters/symbols. The player must place them so a valid spatial chain also follows the remembered relation. A spatially neat placement can therefore still be strategically wrong.

Later versions may replace literal A/B/C with:
- Korean syllables
- semantic categories
- shape relations
- number relations

The control scheme should remain the same.

## Design constraints

- Core rule explainable in one sentence
- No separate mini-games
- Mobile-friendly
- Maximum 3 essential input types
- Difficulty should come from relationships, not from extra buttons
- A single placement should affect at least 4 cognitive targets
- Avoid direct visual/mechanical cloning of any reference game

## V0.1 validation questions

1. Can a new player understand the physical action within 30 seconds?
2. Does the board remain readable on a phone?
3. Does every placement have a meaningful future consequence?
4. Is the memory layer useful without becoming annoying?
5. Can language be present without turning the game into a pure word puzzle?
6. Can the rule generate endless variation procedurally?
7. Do the unknown targets 8 and 9 fit the same core loop once defined?

## Next design pass

V0.2 should produce:
- one fixed board layout
- one fixed tile shape
- one fixed scoring/clear rule
- one fixed fail condition
- a 60-second sample play sequence
- difficulty levels 1–5
