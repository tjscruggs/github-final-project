# Trebuchet Duel: Mobile Strategy Prototype Spec

## Goal
Create a fast, replayable 2-player abstract strategy game for mobile where rival castles race to complete and fire a trebuchet.

## Product Pillars
- 3–5 minute matches
- High tactical tension from movement + disruption
- Clear readability on phone screens
- Local pass-and-play first
- Historical commanders with distinct abilities and visual identity

## Board and Pieces
- Board: 5x5 grid
- Players: 2
- Units: 2 engineers per player
- Castle zones: 1 castle tile per side on opposite board edges

## Commander System
At match start, each player picks one historical commander.

### Prototype commander roster
- **Alexander the Great**: Build/Repair can be done from up to 2 tiles from your castle.
- **Hannibal Barca**: Sabotage range to enemy trebuchet is increased to 2 tiles.
- **Sun Tzu**: Siege cooldown is reduced (1 turn in prototype).
- **Joan of Arc**: First sabotage against your trebuchet is negated.
- **Napoleon Bonaparte**: Siege can only target enemy structures.

### Era-based engineer aesthetics
Each commander changes the engineer piece look to match region/era flavor:
- Glyph markers (e.g., Λ, 兵, ✚, N)
- Different marker border motifs (classical, medieval, east-asia, early-modern)

## Turn Flow
Each turn:
1. Select 1 engineer
2. Move it to an empty tile within movement allowance
3. Perform 1 action (Build, Sabotage, Repair, Siege, or Fire)

If a player has a forfeit penalty queued, their full next turn is skipped.

## Actions
### 1) Build
- Trebuchet components must be completed in order:
  1. Base
  2. Frame
  3. Counterweight
  4. Sling
- Must be in valid build range of your castle (range depends on commander).

### 2) Sabotage
- Removes 1 highest completed component.
- Base is permanent and cannot be removed.
- Requires valid sabotage range to enemy castle zone.

### 3) Repair
- Restores 1 missing component in normal order.
- Requires valid repair/build range.

### 4) Siege Action (Global)
- Removes 1 removable component from any valid trebuchet on map.
- Does not require adjacency.
- Cannot remove Base.
- Cost: player forfeits their next turn.
- Anti-abuse: cooldown blocks repeated use.

### 5) Fire
- Requires all 4 components complete.
- Must be performed on a later turn after completion.
- If sabotaged before Fire resolves, player must rebuild missing parts first.

## Victory
A player wins by successfully using **Fire** after completing all trebuchet components.

## Prototype Scope
### Included
- Local 2-player gameplay loop
- Commander selection before match
- Era-themed engineer visuals tied to commander
- Full turn logic, penalties, and action validation
- Mobile-optimized board UI

### Stretch
- Basic AI opponent
- Turn history + undo in local mode
- Online async mode
