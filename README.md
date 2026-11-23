# CMIDE Bridge

CMIDE Bridge connects [CMIDE](https://github.com/CodeMarketModding/CMIDE) and [Code Market Live for VS Code](https://github.com/CodeMarketModding/CMIDE-VSCode) to a running Code Market world. It synchronizes scripts, provides the game's upgrade-aware completion and reference data, and hides folders whose names begin with `_` in the game's script explorer while leaving them available to the editors. It also supports moving scripts between folders from VS Code.

## Requirements

- Code Market for Windows.
- BepInEx 5 for Unity Mono, installed in the Code Market game directory.

## Install

1. Install [BepInEx 5](https://github.com/bepinex/bepinex/releases) in the game directory and launch the game once so BepInEx creates its folders. The game directory should contain both `Code Market_Data` and `BepInEx/core`.
2. Download `CMIDE-Bridge.dll` from [Releases](https://github.com/CodeMarketModding/CMIDE-Bridge/releases/latest).
3. Close the game. Create `BepInEx/plugins/CodeMarketLiveIDE` in the game directory if necessary, then copy the downloaded DLL into it. Remove older copies of the CMIDE bridge DLL from the plugins directory to avoid duplicate versions.
4. Start Code Market and load a world, then open CMIDE. BepInEx should report `Code Market Live IDE Bridge 1.0.9` in its log.

### Build from source

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and clone this repository. In PowerShell, build the bridge, replacing the example path with your game directory:

   ```powershell
   dotnet build Bridge/Bridge.csproj -c Release '-p:GameRoot=C:\SteamLibrary\steamapps\common\Code Market'
   ```

The built DLL is `Bridge/bin/Release/netstandard2.1/CMIDE-Bridge.dll`.

The build uses assemblies from your own game and BepInEx installation; they are not included in this repository. Alternatively set the `GameManaged` and `BepCore` MSBuild properties separately.

## Behavior

The bridge keeps the game responsive while CMIDE has focus. Scripts can continue running in the background. The IDE communicates with the bridge using local files in `%LOCALAPPDATA%\CodeMarketLiveIDE\bridge`; there is no network listener. Folders beginning with `_` are only hidden from the in-game explorer: they and their scripts remain in the project.
