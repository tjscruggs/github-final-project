# Trebuchet Duel (Unity Android Prototype)

This is a Unity project scaffold for a mobile-first 2-player strategy prototype.

## Quick Start
1. Open **Unity Hub** → **Open** → select `UnityTrebuchetDuel`.
2. Open scene `Assets/Scenes/MainMenu.unity` (create it using setup instructions below if missing).
3. Press Play.

## Android Build (APK)
1. Unity: **File → Build Settings**.
2. Select **Android** and click **Switch Platform**.
3. In **Player Settings** set package name (example `com.yourname.trebuchetduel`).
4. Keep **IL2CPP** and ARM64 enabled for emulator/device compatibility.
5. Click **Build** to generate an APK.

## Run in Android Emulator (Android Studio)
1. Open Android Studio → **Device Manager**.
2. Create/start an emulator (Pixel + Android 13 recommended).
3. Install APK:
   - Drag/drop APK onto emulator, or
   - `adb install -r path/to/trebuchet-duel.apk`
4. Launch app and test touch interactions.

## Scene Setup (required)
Because scenes are not pre-authored binary assets in this repository, create these once:

### MainMenu Scene
- Create scene `Assets/Scenes/MainMenu.unity`.
- Add Canvas + EventSystem.
- Add title text and two buttons:
  - `Play` button → OnClick: `MainMenuController.StartGame`
  - `Quit` button → OnClick: `MainMenuController.QuitGame`
- Add empty object `MainMenuController` with script `MainMenuController`.

### Gameplay Scene
- Create scene `Assets/Scenes/Gameplay.unity`.
- Add Canvas (Screen Space - Overlay), EventSystem.
- Add `GameController`, `BoardManager`, `UIController` components to a root object `GameRoot`.
- Create a tile prefab:
  - UI Button with Image + `TileView` script.
  - Assign Image to `TileView.Background`, Button to `TileView.Button`.
- Add a `BoardRoot` RectTransform with GridLayoutGroup, anchor-stretch for portrait.
- Create buttons/text fields for HUD and action controls, wire references in `UIController`.
- Assign `BoardManager.boardRoot`, `BoardManager.tilePrefab`, and `GameController.board/ui`.
- Add both scenes to Build Settings (MainMenu index 0, Gameplay index 1).

## Architecture
- `Assets/Scripts/Core`: game data and enums
- `Assets/Scripts/Rules`: rules engine and validation
- `Assets/Scripts/Board`: board + tile views/highlighting
- `Assets/Scripts/Units`: unit view logic
- `Assets/Scripts/UI`: UI events and HUD binding
- `Assets/Scripts/GameFlow`: orchestration / turn flow
- `Assets/Scripts/Menu`: main menu scene logic

## Notes
- Designed for portrait mode first for one-handed phone usability.
- Placeholder visuals only; gameplay loop prioritized over polish.
