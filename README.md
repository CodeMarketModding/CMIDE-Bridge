# CMIDE Bridge

CMIDE Bridge connects [CMIDE](https://github.com/CodeMarketModding/CMIDE) to a running Code Market world. It synchronizes scripts, provides the game's upgrade-aware completion and reference data, and hides folders whose names begin with `_` in the game's script explorer while leaving them available to the IDE.

## Requirements

- Code Market for Windows.
- BepInEx 5 for Unity Mono, installed in the Code Market game directory.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) to build the bridge.

## Install

1. Install BepInEx 5 in the game directory and launch the game once so BepInEx creates its folders. The game directory should contain both `Code Market_Data` and `BepInEx/core`.
2. Clone this repository. In PowerShell, build the bridge, replacing the example path with your game directory:

   ```powershell
   dotnet build Bridge/Bridge.csproj -c Release '-p:GameRoot=C:\SteamLibrary\steamapps\common\Code Market'
   ```

3. Close the game. Create `BepInEx/plugins/CodeMarketLiveIDE` in the game directory if necessary, then copy `Bridge/bin/Release/netstandard2.1/CodeMarket.LiveIDE.HiddenFolders.dll` into it.
4. Start Code Market and load a world, then open CMIDE. BepInEx should report `Code Market Live IDE Bridge 1.0.7` in its log.

The build uses assemblies from your own game and BepInEx installation; they are not included in this repository. Alternatively set the `GameManaged` and `BepCore` MSBuild properties separately.

## Behavior

The bridge keeps the game responsive while CMIDE has focus. Scripts can continue running in the background. The IDE communicates with the bridge using local files in `%LOCALAPPDATA%\CodeMarketLiveIDE\bridge`; there is no network listener. Folders beginning with `_` are only hidden from the in-game explorer: they and their scripts remain in the project.
