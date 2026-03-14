# Unity Implementation Notes (Android-first)

This project has been migrated away from a web prototype to a Unity Android-first architecture.

## Why portrait mode
Portrait is selected for first implementation because:
- Better one-handed testing on phones.
- Clear vertical stack for turn status + action buttons.
- 5x5 board still remains readable with highlighted tiles.

## Gameplay systems split
- Game state: `Assets/Scripts/Core/GameState.cs`
- Rules engine: `Assets/Scripts/Rules/RulesEngine.cs`
- Board/tile system: `Assets/Scripts/Board/BoardManager.cs`, `TileView.cs`
- Unit logic: `Assets/Scripts/Units/UnitController.cs`
- UI layer: `Assets/Scripts/UI/UIController.cs`
- Flow/orchestration: `Assets/Scripts/GameFlow/GameController.cs`
- Main menu flow: `Assets/Scripts/Menu/MainMenuController.cs`

## Android goal
The Unity project is structured so it can be opened in Unity Hub, switched to Android platform, built as APK, and installed in Android Emulator.
