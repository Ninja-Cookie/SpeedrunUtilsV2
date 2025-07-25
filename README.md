###### <p align="center">This project is a rewrite of [SpeedrunUtils](https://github.com/Loomeh/SpeedrunUtils), with the goal to make it more stable and easier to work with, as well as more features and fixes.</p>

# <p align="center">SpeedrunUtilsV2: A Bomb Rush Cyberfunk Speedrun Utility</p>

<p align="center"><img width="256" height="256" alt="icon" src="https://github.com/user-attachments/assets/9dffcbbb-9244-4ea8-ac4c-25220fcaf660" /></p>

**SpeedrunUtilsV2** is a (non-required but very helpful for runners and verifiers) Speedrun Utility for [Bomb Rush Cyberfunk](https://store.steampowered.com/app/1353230/Bomb_Rush_Cyberfunk/), allowed to be used in speedruns of the game.
This utility aims to make speedrunning BRC a lot friendlier, with features such as:
- Automatic interaction with [LiveSplit](https://livesplit.org/)
- Accurate Auto-Splitting
- Pausing time for loading screens automatically
- Cutscene skipping (with time automatically added on)
- In-Game FPS limit toggler
- General FPS configuration for capping or uncapping
- In-Game FPS Display

## How to install:
0. Install [LiveSplit](https://livesplit.org/)
1. Either use [r2modman](https://thunderstore.io/c/bomb-rush-cyberfunk/p/ebkr/r2modman/) and install BepInEx and SpeedrunUtilsV2 from the manager for Bomb Rush Cyberfunk, and start modded, or, if not using r2modman; Manually install [BepInEx](https://github.com/BepInEx/BepInEx/releases) by dragging the archived files from their release archive into your "\BombRushCyberfunk" folder (where your game is installed), which should now have a "BepInEx" folder, and other files that came with it, in the same place as your "Bomb Rush Cyberfunk" executable.
2. Run the game to generate BepInEx files, then close it.
3. Get the release of [SpeedrunUtilsV2](https://github.com/Ninja-Cookie/SpeedrunUtilsV2/releases) and place the contents of "plugins" within the archive to the "plugins" folder made by BepInEx at "\BombRushCyberfunk\BepInEx\plugins"

## How to use:
1. In LiveSplit, make sure to disable any other form of auto-splitting, such as from the "Edit Splits" menu.
2. Right-Click LiveSplit, hover "Controls" and select "Start TCP Server", this will allow the plugin to communicate with LiveSplit. (This will need to be done each time you launch LiveSplit)
3. When launching the game, it should auto-connect to LiveSplit, you can check this by bringing up the menu using F2 (by default), or manually connect from there.

- Splits can be configured In-Game through the menu using F2 (by default), this does not require a restart, and can be changed at any time.
- Keybinds and Settings can be configured within "\BombRushCyberfunk\BepInEx\config\SpeedrunUtilsV2", and the game must be restarted.
- The game must be launched with the plugin and BepInEx to first generate the config.

---

###### Note: The plugin has no idea what splits you actually have, it will just do a generic split at any point configured, please make sure your splits on LiveSplit match the configured splits in the plugin.

---

###### Example:
<img width="542" height="461" alt="image" src="https://github.com/user-attachments/assets/970b96fe-30d2-45c0-a218-d5a1708805bd" />
