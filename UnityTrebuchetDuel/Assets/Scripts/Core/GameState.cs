using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrebuchetDuel.Core
{
    [Serializable]
    public class TrebuchetState
    {
        public int CompletedParts = 0;
        public int DamagedParts = 0;
        public bool ReadyToFire = false;
    }

    [Serializable]
    public class UnitState
    {
        public string Id;
        public PlayerId Owner;
        public Vector2Int Grid;

        public UnitState(string id, PlayerId owner, Vector2Int grid)
        {
            Id = id;
            Owner = owner;
            Grid = grid;
        }
    }

    [Serializable]
    public class CommanderConfig
    {
        public string Id;
        public string Name;
        public string Ability;
        public string Era;
        public int BuildRepairRange = 1;
        public int SabotageRange = 1;
        public int MoveRange = 1;
        public int SiegeCooldown = 2;
        public bool BlockFirstSabotage;
        public bool SiegeEnemyOnly;

        public static CommanderConfig Default(PlayerId p)
        {
            return p == PlayerId.Player1
                ? new CommanderConfig { Id = "alexander", Name = "Alexander", Era = "Classical Greece", Ability = "Build/Repair range +1", BuildRepairRange = 2 }
                : new CommanderConfig { Id = "sun-tzu", Name = "Sun Tzu", Era = "Ancient China", Ability = "Siege cooldown reduced", SiegeCooldown = 1 };
        }
    }

    [Serializable]
    public class GameState
    {
        public const int BoardSize = 5;

        public PlayerId CurrentPlayer = PlayerId.Player1;
        public TurnPhase Phase = TurnPhase.SelectUnit;
        public bool MovedThisTurn;
        public bool ActionTaken;
        public bool MatchOver;
        public PlayerId? Winner;

        public readonly Dictionary<PlayerId, Vector2Int> CastleByPlayer = new()
        {
            { PlayerId.Player1, new Vector2Int(0, 2) },
            { PlayerId.Player2, new Vector2Int(4, 2) }
        };

        public readonly List<UnitState> Units = new();
        public readonly Dictionary<PlayerId, TrebuchetState> Trebuchets = new();
        public readonly Dictionary<PlayerId, CommanderConfig> Commanders = new();
        public readonly Dictionary<PlayerId, int> SiegeCooldown = new();
        public readonly Dictionary<PlayerId, bool> ForfeitNextTurn = new();
        public readonly Dictionary<PlayerId, bool> FirstBlockUsed = new();

        public string SelectedUnitId;

        public void Reset(CommanderConfig p1, CommanderConfig p2)
        {
            CurrentPlayer = PlayerId.Player1;
            Phase = TurnPhase.SelectUnit;
            MovedThisTurn = false;
            ActionTaken = false;
            MatchOver = false;
            Winner = null;
            SelectedUnitId = null;

            Units.Clear();
            Units.Add(new UnitState("P1-A", PlayerId.Player1, new Vector2Int(0, 1)));
            Units.Add(new UnitState("P1-B", PlayerId.Player1, new Vector2Int(0, 3)));
            Units.Add(new UnitState("P2-A", PlayerId.Player2, new Vector2Int(4, 1)));
            Units.Add(new UnitState("P2-B", PlayerId.Player2, new Vector2Int(4, 3)));

            Trebuchets.Clear();
            Trebuchets[PlayerId.Player1] = new TrebuchetState();
            Trebuchets[PlayerId.Player2] = new TrebuchetState();

            Commanders.Clear();
            Commanders[PlayerId.Player1] = p1;
            Commanders[PlayerId.Player2] = p2;

            SiegeCooldown[PlayerId.Player1] = 0;
            SiegeCooldown[PlayerId.Player2] = 0;
            ForfeitNextTurn[PlayerId.Player1] = false;
            ForfeitNextTurn[PlayerId.Player2] = false;
            FirstBlockUsed[PlayerId.Player1] = false;
            FirstBlockUsed[PlayerId.Player2] = false;
        }
    }
}
