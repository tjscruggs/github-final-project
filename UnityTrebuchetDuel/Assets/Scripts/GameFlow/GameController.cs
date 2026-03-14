using System.Collections.Generic;
using System.Linq;
using TrebuchetDuel.Board;
using TrebuchetDuel.Core;
using TrebuchetDuel.Rules;
using TrebuchetDuel.UI;
using UnityEngine;

namespace TrebuchetDuel.GameFlow
{
    /// <summary>
    /// Orchestrates state + rules + board + ui. Entry point for gameplay scene.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        public BoardManager board;
        public UIController ui;

        private readonly GameState _state = new();
        private readonly RulesEngine _rules = new();

        private readonly List<CommanderConfig> _commanderPool = new()
        {
            new CommanderConfig { Id = "alexander", Name = "Alexander", Era = "Classical Greece", Ability = "Build/Repair range +1", BuildRepairRange = 2 },
            new CommanderConfig { Id = "hannibal", Name = "Hannibal", Era = "Carthage", Ability = "Sabotage range +1", SabotageRange = 2 },
            new CommanderConfig { Id = "sun-tzu", Name = "Sun Tzu", Era = "Ancient China", Ability = "Siege cooldown 1", SiegeCooldown = 1 },
            new CommanderConfig { Id = "joan", Name = "Joan of Arc", Era = "Medieval France", Ability = "Block first sabotage", BlockFirstSabotage = true },
            new CommanderConfig { Id = "napoleon", Name = "Napoleon", Era = "Early Modern France", Ability = "Siege enemy-only", SiegeEnemyOnly = true }
        };

        private void Start()
        {
            ui.BindCommanderOptions(_commanderPool.Select(c => $"{c.Name} ({c.Era})").ToArray());
            ui.OnStartMatchPressed += StartMatch;
            ui.OnActionPressed += PerformAction;
            ui.OnEndTurnPressed += EndTurn;
            ui.OnRestartPressed += Restart;
            board.OnTilePressed += HandleTileTap;

            StartMatch();
        }

        private void StartMatch()
        {
            var picks = ui.CommanderIndexes();
            _state.Reset(CloneCommander(_commanderPool[picks.p1]), CloneCommander(_commanderPool[picks.p2]));
            board.BuildBoard(_state);
            ui.SetMessage("Select one engineer, move, then choose an action.");
            Refresh();
        }

        private CommanderConfig CloneCommander(CommanderConfig c)
            => new() { Id = c.Id, Name = c.Name, Ability = c.Ability, Era = c.Era, BuildRepairRange = c.BuildRepairRange, SabotageRange = c.SabotageRange, MoveRange = c.MoveRange, SiegeCooldown = c.SiegeCooldown, BlockFirstSabotage = c.BlockFirstSabotage, SiegeEnemyOnly = c.SiegeEnemyOnly };

        private void HandleTileTap(Vector2Int tile)
        {
            if (_state.MatchOver) return;
            var tappedUnit = _state.Units.FirstOrDefault(u => u.Grid == tile);
            if (tappedUnit != null && _rules.CanSelectUnit(_state, tappedUnit))
            {
                _state.SelectedUnitId = tappedUnit.Id;
                _state.Phase = TurnPhase.MoveUnit;
                Refresh();
                return;
            }

            var selected = _rules.GetSelected(_state);
            if (selected != null && _rules.CanMoveTo(_state, selected, tile))
            {
                selected.Grid = tile;
                _state.MovedThisTurn = true;
                _state.Phase = TurnPhase.ChooseAction;
                ui.SetMessage("Now choose Build, Sabotage, Repair, Siege, or Fire.");
                Refresh();
            }
        }

        private void PerformAction(ActionType action)
        {
            if (_state.MatchOver) return;
            var me = _state.CurrentPlayer;
            var enemy = _rules.Opponent(me);
            var selected = _rules.GetSelected(_state);

            switch (action)
            {
                case ActionType.Build:
                    if (!_rules.CanBuildOrRepair(_state, selected)) return;
                    if (_state.Trebuchets[me].CompletedParts < 4)
                    {
                        _state.Trebuchets[me].CompletedParts++;
                        if (_state.Trebuchets[me].CompletedParts == 4)
                        {
                            _state.Trebuchets[me].ReadyToFire = true;
                            ui.SetMessage("Trebuchet complete. Fire on a later turn to win.");
                        }
                    }
                    break;
                case ActionType.Repair:
                    if (!_rules.CanBuildOrRepair(_state, selected)) return;
                    if (_state.Trebuchets[me].DamagedParts > 0 && _state.Trebuchets[me].CompletedParts < 4)
                    {
                        _state.Trebuchets[me].DamagedParts--;
                        _state.Trebuchets[me].CompletedParts++;
                    }
                    break;
                case ActionType.Sabotage:
                    if (!_rules.CanSabotage(_state, selected)) return;
                    if (_rules.ShouldBlockFirstSabotage(_state, enemy))
                    {
                        ui.SetMessage("Defender commander blocked this sabotage.");
                    }
                    else
                    {
                        _state.Trebuchets[enemy].CompletedParts--;
                        _state.Trebuchets[enemy].DamagedParts++;
                        _state.Trebuchets[enemy].ReadyToFire = false;
                    }
                    break;
                case ActionType.Siege:
                    if (_state.SiegeCooldown[me] > 0 || !_state.MovedThisTurn || _state.ActionTaken) return;
                    var target = SelectSiegeTarget(me, enemy);
                    if (!target.HasValue) return;
                    if (_rules.ShouldBlockFirstSabotage(_state, target.Value))
                    {
                        ui.SetMessage("Defender commander blocked the siege hit.");
                    }
                    else
                    {
                        _state.Trebuchets[target.Value].CompletedParts--;
                        _state.Trebuchets[target.Value].DamagedParts++;
                        _state.Trebuchets[target.Value].ReadyToFire = false;
                    }
                    _state.ForfeitNextTurn[me] = true;
                    _state.SiegeCooldown[me] = _state.Commanders[me].SiegeCooldown;
                    break;
                case ActionType.Fire:
                    if (!_rules.CanFire(_state, selected)) return;
                    _state.MatchOver = true;
                    _state.Winner = me;
                    ui.SetMessage("Direct hit! Match won.");
                    break;
            }

            _state.ActionTaken = true;
            _state.Phase = TurnPhase.EndTurn;
            Refresh();
        }

        private PlayerId? SelectSiegeTarget(PlayerId me, PlayerId enemy)
        {
            bool enemyValid = _state.Trebuchets[enemy].CompletedParts > 1;
            bool meValid = _state.Trebuchets[me].CompletedParts > 1;
            if (_state.Commanders[me].SiegeEnemyOnly) return enemyValid ? enemy : null;
            if (enemyValid) return enemy;
            if (meValid) return me;
            return null;
        }

        private void EndTurn()
        {
            if (!_state.MovedThisTurn) return;
            _state.SelectedUnitId = null;
            _state.MovedThisTurn = false;
            _state.ActionTaken = false;
            _state.Phase = TurnPhase.SelectUnit;

            TickCooldowns();
            _state.CurrentPlayer = _rules.Opponent(_state.CurrentPlayer);

            if (_state.ForfeitNextTurn[_state.CurrentPlayer])
            {
                _state.ForfeitNextTurn[_state.CurrentPlayer] = false;
                _state.CurrentPlayer = _rules.Opponent(_state.CurrentPlayer);
                ui.SetMessage("A siege penalty skipped that player's turn.");
            }

            Refresh();
        }

        private void TickCooldowns()
        {
            foreach (var key in new[] { PlayerId.Player1, PlayerId.Player2 })
            {
                if (_state.SiegeCooldown[key] > 0) _state.SiegeCooldown[key]--;
            }
        }

        private void Restart() => StartMatch();

        private void Refresh()
        {
            board.HighlightState(_state, _rules);
            var selected = _rules.GetSelected(_state);
            ui.SetActionButtons(
                build: _rules.CanBuildOrRepair(_state, selected),
                sabotage: _rules.CanSabotage(_state, selected),
                repair: _rules.CanBuildOrRepair(_state, selected) && _state.Trebuchets[_state.CurrentPlayer].DamagedParts > 0,
                siege: _state.MovedThisTurn && !_state.ActionTaken && _state.SiegeCooldown[_state.CurrentPlayer] == 0,
                fire: _rules.CanFire(_state, selected),
                endTurn: _state.MovedThisTurn
            );
            ui.Refresh(_state);
        }
    }
}
