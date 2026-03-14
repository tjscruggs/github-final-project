namespace TrebuchetDuel.Core
{
    public enum PlayerId { Player1, Player2 }
    public enum TurnPhase { SelectUnit, MoveUnit, ChooseAction, ResolveAction, EndTurn }
    public enum ActionType { None, Build, Sabotage, Repair, Siege, Fire }
    public enum TrebuchetPart { None = 0, Base = 1, Frame = 2, Counterweight = 3, Sling = 4 }
}
