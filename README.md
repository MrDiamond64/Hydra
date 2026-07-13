# Hydra
<div align="center">
  <img src="https://github.com/MrDiamond64/Hydra/blob/main/img/main.png?raw=true" alt="A screenshot showing the Hydra Players UI"/>
</div>

Hydra is a [BepInEx](https://github.com/BepInEx/BepInEx) Among Us mod built with the intention of enhancing the Among Us playing experience. Hydra adds quality of life features, fun trolling features, and an anticheat to detect players hacking in your lobbies.

We have a Discord server, feel free to join and talk, ask for help, or offer suggestions: https://discord.gg/N7azGPHm5F

# Features
- In-game notifications
- Show chat messages by ghosts (Useful for moderation to determine if players are acting fair)
- Always visible chat button
- Be able to flip the Skeld map as host
- Always impostor as host
- Player color randomizer
- Teleporter
- Sabotage and close doors as crewmate
- Device and version spoofer
- See other player's roles
- [Ban players from lobbies without host](https://streamable.com/wtb7jl)
- [Become immortal](https://streamable.com/k1b0m0)
- [Configurable anticheat to detect common hacks and exploits](https://github.com/MrDiamond64/Hydra?tab=readme-ov-file#hydra-anticheat)
- And more!

# Hydra Anticheat
Hydra Anticheat is quite possibly the heart of this mod. It is able to detect when players attempt to cheat, such as if they enter vents without the proper roles, or try to teleport around the map. Upon detecting a cheater, Hydra is able to automatically ban the player from the lobby. You do not have to be the host of the lobby to be able to use Hydra Anticheat, you can do so without being host which will just send you a notification and not ban the player. Hydra Anticheat is meant to extend the vanilla Among Us anticheat by adding checks for cheats it does currently not detect, though it can be used in custom Among Us servers with a less strict anticheat as long as it follows a baseline.

Hydra Anticheat comes with a basic baseline: the backend server must be able to prevent player impersonation. If cheaters are able to send RPCs on the behalf of other players, then Hydra Anticheat will not be able to accurately determine who is cheating or not and flag the wrong players. The vanilla Among Us servers already come with impersonation checks, so this should not be much of a concern in those servers.

# Installation and Usage
> [!WARNING]
> Before using Hydra, please make sure to understand and fully consent to the warnings provided in the [Disclaimer](https://github.com/MrDiamond64/Hydra?tab=readme-ov-file#disclaimer) section.

## Installing BepInEx
[BepInEx](https://github.com/bepinex/bepinex) is a modding framework that allows mods to be created for Unity games. Among Us uses Unity as their game engine, so Hydra uses the BepInEx modding framework in order to modify the game. Before you get to downloading Hydra, you first need to install BepInEx. There are many variants of BepInEx, we will need the il2cpp version specifically as Among Us uses the il2cpp compiler for cross-platform compatability. You can download BepInEx from the [Releases](https://github.com/MrDiamond64/Hydra/releases) tab, or you can alternatively get BepInEx binaries from [the BepInEx site](https://builds.bepinex.dev/projects/bepinex_be) if you would like. You may notice that BepInEx Il2Cpp has two architectures: x86 and x64, this is important as Among Us can be 32-bit or 64-bit depending on where you downloaded it from. The architecture of BepInEx must match the architecture of your Among Us installation. As a general rule of thumb, Microsoft Store and Epic Games provide x64 builds of Among Us, and Steam and Itch.io provide x86 builds of the game. If you are still unsure, you can open Task Manager by pressing `ctrl` + `shift` + `esc` on your keyboard, finding Among Us in the running processes list, and see if it says "Among Us.exe (32-bit)", if it does then you are using a 32-bit version of Among Us, if not then youare using a 64-bit version of Among Us.

Once you have downloaded the proper BepInEx for your Among Us build, you want to open your Among Us installation directory (where `Among Us.exe` and `GameAssembly.dll` are located), and extract the contents of BepInEx into the Among Us directory. Alongside the `Among Us.exe` file, you should see new files and folders such as `winhttp.dll`, `BepInEx`, and `dotnet`. If you do not see those files then your archival program may have extracted them into its own seperate folder than the current directory, if that is the case then open the newly created folder, and drag and drop all the contents into the same directory that `Among Us.exe` is located at. Assuming everything was done correctly, BepInEx should be installed and you are ready to download Hydra.

## Installing Hydra
To download Hydra, you simply need to go to the [Releases](https://github.com/MrDiamond64/Hydra/releases) tab, download the `HydraMenu.dll` file, and copy and paste the `HydraMenu.dll` file into the `./BepInEx/plugins/` directory. After you have done that, you can open Among Us. Opening Among Us for the first time after installing BepInEx will be different than normal, it may take much longer to start and you will see a terminal window. This is completely normal, the start delay is part of the BepInEx preflight process. Any subsequent launches of Among Us will not have this delay. After waiting a bit, Among Us should open and a mod icon should show up at the top right of your screen, at this point Hydra is ready and you can now get straight into having fun with Hydra!

## Using Hydra
You can access the Hydra UI by pressing `Insert` on your keyboard. Depending on your keyboard, you may have to toggle Num Lock or press the function key alongside the Insert key to get the menu to show up. After pressing Insert, you should see the Hydra UI. The Hydra UI has multiple parts: the sections pane, and the features panes. The sections pane will have a list of tabs such as `Self`, `Host`, and `Anticheat`. Pressing any of these tabs will show the features for this section in the Features Pane. The Features Pane will have sliders, buttons, and checkboxes which can be used to configure Hydra.

# TODO
- [ ] Improve anticheat with more checks (such as sabotaging as crewmate)
- [x] Add scrollbars to UI sections
- [x] Show player role and colors in Players UI section
- [ ] Explore the modded vanilla protocol which seems to have a much more lenient anticheat
- [ ] Saveable configs

# Disclaimer
> [!CAUTION]
> **Hydra should NEVER, under any circumstances, be used to impair the experiences of other players. If you use some of the trolling features, please make sure you are doing so in a private lobby with consenting players. You are free to join public lobbies with Hydra enabled as long as you use it with the intention of improving your Among Us game. With great power comes great responsibility!**

Something I recognize with utility mods like Hydra is that it opens the door for malicious users to cause destruction in lobbies. I have tried to limit the potential of abuse by removing powerful and abuse-prone features from the public version and adding safeguards to limit abuse. Even with these protections, there is always a chance for abuse and malicious activities. All I can do is to ask you, the person using Hydra, to please do not use Hydra for malicious purposes and follow the [Innersloth Code of Conduct](https://www.innersloth.com/code-of-conduct/) and rules set by the lobby you are playing on. Only use it to detect cheaters in public lobbies, or in use where everyone else consents to the usage of the Hydra's more advanced features.

If you fail to follow my suggestion, then do not expect to receive any kind of support or liability by me. Your account may also be placed in a sanction by Innersloth and you will lose your Among Us account, along with any data associated with it, such as your friends list, unlocked cosmetics, purchases, beans and coins, etc.

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
