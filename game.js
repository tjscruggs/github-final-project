const BOARD_SIZE = 5;
const COMPONENTS = ["Base", "Frame", "Counterweight", "Sling"];

const COMMANDERS = [
  {
    id: "alexander",
    name: "Alexander the Great",
    regionEra: "Classical Greece (4th c. BCE)",
    unitGlyph: "Λ",
    engineerTheme: "theme-classical",
    abilityText: "Rapid Logistics: Build and Repair can be performed from up to 2 tiles from your castle.",
    applyAbilityContext: { extendedCastleRange: 2 }
  },
  {
    id: "hannibal",
    name: "Hannibal Barca",
    regionEra: "Carthage (3rd c. BCE)",
    unitGlyph: "𐤇",
    engineerTheme: "theme-classical",
    abilityText: "Encirclement: Sabotage works from up to 2 tiles from the enemy castle.",
    applyAbilityContext: { sabotageRange: 2 }
  },
  {
    id: "sun-tzu",
    name: "Sun Tzu",
    regionEra: "Ancient China",
    unitGlyph: "兵",
    engineerTheme: "theme-east-asia",
    abilityText: "Art of War: Siege cooldown reduced by 1 (effective cooldown = 1 turn).",
    applyAbilityContext: { siegeCooldown: 1 }
  },
  {
    id: "joan",
    name: "Joan of Arc",
    regionEra: "Medieval France (15th c.)",
    unitGlyph: "✚",
    engineerTheme: "theme-medieval",
    abilityText: "Inspiration: The first sabotage against your trebuchet each match is prevented.",
    applyAbilityContext: { blockFirstSabotage: true }
  },
  {
    id: "napoleon",
    name: "Napoleon Bonaparte",
    regionEra: "Early Modern France (19th c.)",
    unitGlyph: "N",
    engineerTheme: "theme-early-modern",
    abilityText: "Grand Battery: Your Siege Action can only target enemy structures (never your own).",
    applyAbilityContext: { siegeEnemyOnly: true }
  }
];

const defaultCommander = {
  P1: COMMANDERS[0].id,
  P2: COMMANDERS[2].id
};

const state = {
  currentPlayer: "P1",
  selectedUnitId: null,
  movedThisTurn: false,
  actionTaken: false,
  winner: null,
  pendingSkip: { P1: false, P2: false },
  siegeCooldown: { P1: 0, P2: 0 },
  needsFire: { P1: false, P2: false },
  commanderBlockUsed: { P1: false, P2: false },
  commanders: { ...defaultCommander },
  units: [],
  trebuchets: {
    P1: { level: 0, sabotaged: 0 },
    P2: { level: 0, sabotaged: 0 }
  }
};

const castleZones = {
  P1: [{ x: 0, y: 2 }],
  P2: [{ x: 4, y: 2 }]
};

const boardEl = document.getElementById("board");
const turnLabel = document.getElementById("turnLabel");
const selectedLabel = document.getElementById("selectedLabel");
const penaltyLabel = document.getElementById("penaltyLabel");
const cooldownLabel = document.getElementById("cooldownLabel");
const messageBox = document.getElementById("messageBox");
const commanderAbilityText = document.getElementById("commanderAbilityText");
const p1CommanderSelect = document.getElementById("p1Commander");
const p2CommanderSelect = document.getElementById("p2Commander");

const buttons = {
  build: document.getElementById("buildBtn"),
  sabotage: document.getElementById("sabotageBtn"),
  repair: document.getElementById("repairBtn"),
  siege: document.getElementById("siegeBtn"),
  fire: document.getElementById("fireBtn"),
  endTurn: document.getElementById("endTurnBtn"),
  reset: document.getElementById("resetBtn"),
  applyCommanders: document.getElementById("applyCommandersBtn")
};

function getCommanderById(id) {
  return COMMANDERS.find((c) => c.id === id) || COMMANDERS[0];
}

function commanderFor(player) {
  return getCommanderById(state.commanders[player]);
}

function getOpponent(player) {
  return player === "P1" ? "P2" : "P1";
}

function isCastleTile(player, x, y) {
  return castleZones[player].some((t) => t.x === x && t.y === y);
}

function unitAt(x, y) {
  return state.units.find((u) => u.x === x && u.y === y) || null;
}

function adjacent(a, b) {
  return Math.abs(a.x - b.x) <= 1 && Math.abs(a.y - b.y) <= 1 && !(a.x === b.x && a.y === b.y);
}

function withinRangeToCastle(player, x, y, range) {
  return castleZones[player].some((tile) => Math.abs(tile.x - x) <= range && Math.abs(tile.y - y) <= range && !(tile.x === x && tile.y === y));
}

function selectedUnit() {
  return state.units.find((u) => u.id === state.selectedUnitId) || null;
}

function maxMoveDistance(player) {
  const cmd = commanderFor(player);
  return cmd.id === "alexander" ? 2 : 1;
}

function canMove(unit, tx, ty) {
  if (state.movedThisTurn || !unit) return false;
  if (tx < 0 || tx >= BOARD_SIZE || ty < 0 || ty >= BOARD_SIZE) return false;
  if (unitAt(tx, ty)) return false;
  const range = maxMoveDistance(unit.owner);
  return Math.abs(unit.x - tx) <= range && Math.abs(unit.y - ty) <= range && !(unit.x === tx && unit.y === ty);
}

function setMessage(msg) {
  messageBox.textContent = msg;
}

function getBuildRepairRange(player) {
  const cmd = commanderFor(player);
  return cmd.applyAbilityContext.extendedCastleRange || 1;
}

function getSabotageRange(player) {
  const cmd = commanderFor(player);
  return cmd.applyAbilityContext.sabotageRange || 1;
}

function getSiegeCooldownValue(player) {
  const cmd = commanderFor(player);
  return cmd.applyAbilityContext.siegeCooldown || 2;
}

function commanderInfoLine(player) {
  const c = commanderFor(player);
  return `${player === "P1" ? "Red" : "Blue"}: ${c.name} [${c.regionEra}] — ${c.abilityText}`;
}

function updateHud() {
  const current = commanderFor(state.currentPlayer);
  turnLabel.textContent = `${state.currentPlayer === "P1" ? "Red" : "Blue"} (${current.name})`;
  selectedLabel.textContent = state.selectedUnitId || "None";
  const penalties = Object.entries(state.pendingSkip)
    .filter(([, v]) => v)
    .map(([k]) => (k === "P1" ? "Red skips next" : "Blue skips next"));
  penaltyLabel.textContent = penalties.length ? penalties.join("; ") : "None";
  cooldownLabel.textContent = `Red:${state.siegeCooldown.P1} / Blue:${state.siegeCooldown.P2}`;
  commanderAbilityText.textContent = `${commanderInfoLine("P1")} | ${commanderInfoLine("P2")}`;

  const player = state.currentPlayer;
  const unit = selectedUnit();
  const opp = getOpponent(player);
  const buildRange = getBuildRepairRange(player);
  const sabotageRange = getSabotageRange(player);

  buttons.build.disabled = !(state.movedThisTurn && !state.actionTaken && unit && withinRangeToCastle(player, unit.x, unit.y, buildRange));
  buttons.sabotage.disabled = !(state.movedThisTurn && !state.actionTaken && unit && withinRangeToCastle(opp, unit.x, unit.y, sabotageRange) && state.trebuchets[opp].level > 1);
  buttons.repair.disabled = !(state.movedThisTurn && !state.actionTaken && unit && withinRangeToCastle(player, unit.x, unit.y, buildRange) && state.trebuchets[player].sabotaged > 0);
  buttons.siege.disabled = !(state.movedThisTurn && !state.actionTaken && state.siegeCooldown[player] === 0);
  buttons.fire.disabled = !(state.movedThisTurn && !state.actionTaken && state.needsFire[player] && unit && withinRangeToCastle(player, unit.x, unit.y, 1));
  buttons.endTurn.disabled = !state.movedThisTurn;

  renderBoard();
}

function componentName(level) {
  return level <= 0 ? "None" : COMPONENTS[Math.min(level, 4) - 1];
}

function renderBoard() {
  boardEl.innerHTML = "";
  for (let x = 0; x < BOARD_SIZE; x++) {
    for (let y = 0; y < BOARD_SIZE; y++) {
      const tile = document.createElement("button");
      tile.className = "tile";
      tile.type = "button";

      if (isCastleTile("P1", x, y)) tile.classList.add("castle-p1");
      if (isCastleTile("P2", x, y)) tile.classList.add("castle-p2");

      const unit = unitAt(x, y);
      if (unit && unit.id === state.selectedUnitId) tile.classList.add("highlight");
      if (state.selectedUnitId && canMove(selectedUnit(), x, y)) tile.classList.add("move-target");

      if (isCastleTile("P1", x, y)) {
        const t = state.trebuchets.P1;
        const tag = document.createElement("div");
        tag.className = "treb";
        tag.textContent = `R: ${componentName(t.level)}${t.sabotaged ? ` (-${t.sabotaged})` : ""}`;
        tile.appendChild(tag);
        const mark = document.createElement("div");
        mark.className = "castle-mark";
        mark.textContent = "Red Castle";
        tile.appendChild(mark);
      }

      if (isCastleTile("P2", x, y)) {
        const t = state.trebuchets.P2;
        const tag = document.createElement("div");
        tag.className = "treb";
        tag.textContent = `B: ${componentName(t.level)}${t.sabotaged ? ` (-${t.sabotaged})` : ""}`;
        tile.appendChild(tag);
        const mark = document.createElement("div");
        mark.className = "castle-mark";
        mark.textContent = "Blue Castle";
        tile.appendChild(mark);
      }

      if (unit) {
        const commander = commanderFor(unit.owner);
        const mark = document.createElement("div");
        mark.className = `unit-mark ${unit.owner === "P1" ? "p1" : "p2"} ${commander.engineerTheme}`;
        mark.textContent = commander.unitGlyph;
        mark.title = `${commander.name} engineer style`;
        tile.appendChild(mark);
      }

      tile.addEventListener("click", () => onTileClick(x, y));
      boardEl.appendChild(tile);
    }
  }
}

function onTileClick(x, y) {
  if (state.winner) return;
  const maybe = unitAt(x, y);
  if (maybe && maybe.owner === state.currentPlayer && !state.movedThisTurn) {
    state.selectedUnitId = maybe.id;
    setMessage(`${maybe.id} selected. Move to an adjacent empty tile.`);
    updateHud();
    return;
  }

  const unit = selectedUnit();
  if (unit && canMove(unit, x, y)) {
    unit.x = x;
    unit.y = y;
    state.movedThisTurn = true;
    setMessage(`${unit.id} moved. Choose one action: Build, Sabotage, Repair, Siege, or Fire.`);
    updateHud();
  }
}

function incrementCooldownsAndResolveSkips(nextPlayer) {
  for (const p of ["P1", "P2"]) {
    if (state.siegeCooldown[p] > 0) state.siegeCooldown[p] -= 1;
  }

  if (state.pendingSkip[nextPlayer]) {
    state.pendingSkip[nextPlayer] = false;
    state.currentPlayer = getOpponent(nextPlayer);
    setMessage(`${nextPlayer === "P1" ? "Red" : "Blue"} forfeits this turn due to Siege penalty.`);
  } else {
    state.currentPlayer = nextPlayer;
  }
}

function endTurn() {
  if (!state.movedThisTurn) {
    setMessage("You must move before ending turn.");
    return;
  }
  state.selectedUnitId = null;
  state.movedThisTurn = false;
  state.actionTaken = false;
  incrementCooldownsAndResolveSkips(getOpponent(state.currentPlayer));
  updateHud();
}

function performBuild() {
  const player = state.currentPlayer;
  const unit = selectedUnit();
  const buildRange = getBuildRepairRange(player);
  if (!unit || !withinRangeToCastle(player, unit.x, unit.y, buildRange) || !state.movedThisTurn || state.actionTaken) return;

  const treb = state.trebuchets[player];
  if (treb.level < 4) {
    treb.level += 1;
    setMessage(`${player === "P1" ? "Red" : "Blue"} built: ${componentName(treb.level)}.`);
    if (treb.level === 4) {
      state.needsFire[player] = true;
      setMessage(`${player === "P1" ? "Red" : "Blue"} completed Sling. Survive until a future turn to FIRE.`);
    }
  } else {
    setMessage("Trebuchet already complete. Use Fire on a later turn to win.");
  }

  if (treb.sabotaged > 0 && treb.level === 4) treb.sabotaged = 0;
  state.actionTaken = true;
  updateHud();
}

function sabotageBlockedByJoan(targetPlayer) {
  const targetCommander = commanderFor(targetPlayer);
  if (targetCommander.applyAbilityContext.blockFirstSabotage && !state.commanderBlockUsed[targetPlayer]) {
    state.commanderBlockUsed[targetPlayer] = true;
    return true;
  }
  return false;
}

function performSabotage() {
  const player = state.currentPlayer;
  const unit = selectedUnit();
  const enemy = getOpponent(player);
  const range = getSabotageRange(player);
  if (!unit || !state.movedThisTurn || state.actionTaken) return;
  if (!withinRangeToCastle(enemy, unit.x, unit.y, range)) {
    setMessage("Sabotage requires range to enemy trebuchet zone.");
    return;
  }

  const treb = state.trebuchets[enemy];
  if (treb.level <= 1) {
    setMessage("Cannot sabotage Base. No sabotagable component exists.");
    return;
  }

  if (sabotageBlockedByJoan(enemy)) {
    setMessage(`${enemy === "P1" ? "Red" : "Blue"} blocked this sabotage via commander ability.`);
    state.actionTaken = true;
    updateHud();
    return;
  }

  const removed = componentName(treb.level);
  treb.level -= 1;
  treb.sabotaged += 1;
  state.needsFire[enemy] = false;
  setMessage(`${enemy === "P1" ? "Red" : "Blue"} trebuchet sabotaged! Removed ${removed}.`);
  state.actionTaken = true;
  updateHud();
}

function performRepair() {
  const player = state.currentPlayer;
  const unit = selectedUnit();
  const buildRange = getBuildRepairRange(player);
  if (!unit || !state.movedThisTurn || state.actionTaken) return;
  if (!withinRangeToCastle(player, unit.x, unit.y, buildRange)) {
    setMessage("Repair requires range to your castle trebuchet.");
    return;
  }

  const treb = state.trebuchets[player];
  if (treb.sabotaged <= 0 || treb.level >= 4) {
    setMessage("No damaged component to repair.");
    return;
  }

  treb.level += 1;
  treb.sabotaged -= 1;
  setMessage(`${player === "P1" ? "Red" : "Blue"} repaired ${componentName(treb.level)}.`);
  if (treb.level === 4) {
    state.needsFire[player] = true;
    setMessage(`${player === "P1" ? "Red" : "Blue"} restored Sling. Fire on a later turn to win.`);
  }
  state.actionTaken = true;
  updateHud();
}

function performSiege() {
  const player = state.currentPlayer;
  const enemy = getOpponent(player);
  if (!state.movedThisTurn || state.actionTaken) return;
  if (state.siegeCooldown[player] > 0) {
    setMessage("Siege Action is on cooldown.");
    return;
  }

  const cmd = commanderFor(player);
  const enemyTarget = state.trebuchets[enemy].level > 1 ? enemy : null;
  const myTarget = state.trebuchets[player].level > 1 ? player : null;
  const target = cmd.applyAbilityContext.siegeEnemyOnly ? enemyTarget : (enemyTarget || myTarget);

  if (!target) {
    setMessage("No valid siege target with removable components.");
    return;
  }

  if (sabotageBlockedByJoan(target)) {
    setMessage(`${target === "P1" ? "Red" : "Blue"} blocked this Siege hit via commander ability.`);
    state.pendingSkip[player] = true;
    state.siegeCooldown[player] = getSiegeCooldownValue(player);
    state.actionTaken = true;
    updateHud();
    return;
  }

  const treb = state.trebuchets[target];
  const removed = componentName(treb.level);
  treb.level -= 1;
  treb.sabotaged += 1;
  state.needsFire[target] = false;

  state.pendingSkip[player] = true;
  state.siegeCooldown[player] = getSiegeCooldownValue(player);
  setMessage(`Siege strikes ${target === "P1" ? "Red" : "Blue"} trebuchet: removed ${removed}. You forfeit your next turn.`);
  state.actionTaken = true;
  updateHud();
}

function performFire() {
  const player = state.currentPlayer;
  const unit = selectedUnit();
  if (!unit || !state.movedThisTurn || state.actionTaken) return;
  if (!withinRangeToCastle(player, unit.x, unit.y, 1)) {
    setMessage("Fire requires adjacency to your castle trebuchet.");
    return;
  }
  if (!state.needsFire[player] || state.trebuchets[player].level < 4) {
    setMessage("Trebuchet not ready to fire.");
    return;
  }

  state.winner = player;
  state.actionTaken = true;
  setMessage(`${player === "P1" ? "Red" : "Blue"} fires the trebuchet and wins!`);
  updateHud();
}

function configureCommanderSelects() {
  const options = COMMANDERS.map((c) => `<option value="${c.id}">${c.name}</option>`).join("");
  p1CommanderSelect.innerHTML = options;
  p2CommanderSelect.innerHTML = options;
  p1CommanderSelect.value = state.commanders.P1;
  p2CommanderSelect.value = state.commanders.P2;
}

function applyCommanderPicks() {
  state.commanders.P1 = p1CommanderSelect.value;
  state.commanders.P2 = p2CommanderSelect.value;
  resetGame(true);
}

function resetGame(useCurrentCommanders = false) {
  if (!useCurrentCommanders) {
    state.commanders.P1 = p1CommanderSelect.value || state.commanders.P1;
    state.commanders.P2 = p2CommanderSelect.value || state.commanders.P2;
  }

  state.currentPlayer = "P1";
  state.selectedUnitId = null;
  state.movedThisTurn = false;
  state.actionTaken = false;
  state.winner = null;
  state.pendingSkip = { P1: false, P2: false };
  state.siegeCooldown = { P1: 0, P2: 0 };
  state.needsFire = { P1: false, P2: false };
  state.commanderBlockUsed = { P1: false, P2: false };
  state.units = [
    { id: "P1-A", owner: "P1", x: 0, y: 1 },
    { id: "P1-B", owner: "P1", x: 0, y: 3 },
    { id: "P2-A", owner: "P2", x: 4, y: 1 },
    { id: "P2-B", owner: "P2", x: 4, y: 3 }
  ];
  state.trebuchets = {
    P1: { level: 0, sabotaged: 0 },
    P2: { level: 0, sabotaged: 0 }
  };

  setMessage("Red starts. Select an engineer.");
  updateHud();
}

buttons.build.addEventListener("click", performBuild);
buttons.sabotage.addEventListener("click", performSabotage);
buttons.repair.addEventListener("click", performRepair);
buttons.siege.addEventListener("click", performSiege);
buttons.fire.addEventListener("click", performFire);
buttons.endTurn.addEventListener("click", endTurn);
buttons.reset.addEventListener("click", () => resetGame(false));
buttons.applyCommanders.addEventListener("click", applyCommanderPicks);

configureCommanderSelects();
resetGame(false);
