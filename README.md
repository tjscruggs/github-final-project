# Trebuchet Duel Prototype

A mobile-first, local 2-player strategy game prototype inspired by positional tactical board tension, with an original castle/trebuchet objective.

## Concept
- 5x5 board
- Two rival castles (one on each side)
- Two engineers per player
- Historical commander selection (unique ability + era-themed engineer visuals)
- Move one engineer, then take one action (Build, Sabotage, Repair, Siege, or Fire)
- Win by completing a 4-part trebuchet and successfully firing it on a later turn

## Run locally
```bash
python3 -m http.server 8000
```
Then open: `http://localhost:8000`

## Files
- `index.html`: UI structure and commander selection controls
- `styles.css`: mobile-optimized board/controls and era-themed engineer styling
- `game.js`: game state, commander abilities, and turn/action rules
- `docs/android-strategy-game-concept.md`: gameplay/design specification
