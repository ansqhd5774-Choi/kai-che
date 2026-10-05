# KaiChe R16 third-party visual candidates

R16 keeps external art optional. The runtime falls back to the R15 primitive visuals when no prefab exists.

## Candidate sources

### Quaternius RPG Character Pack
- Intended use: initial player-character prototype.
- Source: https://quaternius.com/packs/rpgcharacters.html
- Pack states 6 rigged, animated, textured fantasy characters.
- Formats include FBX, OBJ, Blend and glTF.
- License shown by the pack page: CC0.

### Quaternius Animated Monster Pack
- Intended use: initial monster prototype.
- Source: https://quaternius.com/packs/animatedmonster.html
- Pack states 4 animated monsters.
- Formats include FBX, OBJ and Blend.
- License shown by the pack page: CC0.

## Unity integration contract

Place approved prefabs at:

- Assets/Resources/KaiChe/CombatVisuals/PlayerWarrior.prefab
- Assets/Resources/KaiChe/CombatVisuals/StarterMonster.prefab
- Assets/Resources/KaiChe/CombatVisuals/RunnerMonster.prefab
- Assets/Resources/KaiChe/CombatVisuals/BruteMonster.prefab
- Assets/Resources/KaiChe/CombatVisuals/BossMonster.prefab

Missing prefabs are allowed and automatically fall back to the R15 runtime primitive visual.
