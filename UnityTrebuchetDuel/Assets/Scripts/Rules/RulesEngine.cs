using System.Linq;
using TrebuchetDuel.Core;
using UnityEngine;

namespace TrebuchetDuel.Rules
{
    /// <summary>
    /// Pure-ish rules and validation layer. Keeps gameplay logic out of UI.
    /// </summary>
    public class RulesEngine
    {
        public bool IsInsideBoard(Vector2Int p) => p.x >= 0 && p.y >= 0 && p.x < GameState.BoardSize && p.y < GameState.BoardSize;

        public bool IsOccupied(GameState state, Vector2Int p) => state.Units.Any(u => u.Grid == p);

        public UnitState GetSelected(GameState state)
            => state.Units.FirstOrDefault(u => u.Id == state.SelectedUnitId);

        public bool IsAdjacent(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) <= 1 && Mathf.Abs(a.y - b.y) <= 1 && a != b;

        public bool IsWithinRange(Vector2Int from, Vector2Int target, int range)
            => Mathf.Abs(from.x - target.x) <= range && Mathf.Abs(from.y - target.y) <= range && from != target;

        public bool CanSelectUnit(GameState state, UnitState unit)
            => !state.MatchOver && state.Phase == TurnPhase.SelectUnit && unit.Owner == state.CurrentPlayer;

        public bool CanMoveTo(GameState state, UnitState unit, Vector2Int target)
        {
            if (state.MatchOver || unit == null || state.MovedThisTurn) return false;
            if (!IsInsideBoard(target) || IsOccupied(state, target)) return false;
            var moveRange = state.Commanders[unit.Owner].MoveRange;
            return IsWithinRange(unit.Grid, target, moveRange);
        }

        public bool CanBuildOrRepair(GameState state, UnitState unit)
        {
            if (unit == null || !state.MovedThisTurn || state.ActionTaken) return false;
            int range = state.Commanders[state.CurrentPlayer].BuildRepairRange;
            return IsWithinRange(state.CastleByPlayer[state.CurrentPlayer], unit.Grid, range);
        }

        public bool CanSabotage(GameState state, UnitState unit)
        {
            if (unit == null || !state.MovedThisTurn || state.ActionTaken) return false;
            var enemy = Opponent(state.CurrentPlayer);
            int range = state.Commanders[state.CurrentPlayer].SabotageRange;
            return IsWithinRange(state.CastleByPlayer[enemy], unit.Grid, range) && state.Trebuchets[enemy].CompletedParts > 1;
        }

        public bool CanFire(GameState state, UnitState unit)
        {
            if (unit == null || !state.MovedThisTurn || state.ActionTaken) return false;
            var treb = state.Trebuchets[state.CurrentPlayer];
            return treb.ReadyToFire && IsAdjacent(state.CastleByPlayer[state.CurrentPlayer], unit.Grid);
        }

        public PlayerId Opponent(PlayerId p) => p == PlayerId.Player1 ? PlayerId.Player2 : PlayerId.Player1;

        public bool ShouldBlockFirstSabotage(GameState state, PlayerId defender)
        {
            var cmd = state.Commanders[defender];
            if (!cmd.BlockFirstSabotage || state.FirstBlockUsed[defender]) return false;
            state.FirstBlockUsed[defender] = true;
            return true;
        }
    }
}
