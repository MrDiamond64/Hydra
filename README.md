# Hydra
<div align="center">
  <img src="https://github.com/MrDiamond64/Hydra/blob/main/img/main.png?raw=true" alt="Hydra UI"/>
</div>

Hydra is a [BepInEx](https://github.com/BepInEx/BepInEx) Among Us mod built with the intention of enhancing the Among Us playing experience. Hydra adds quality of life features, fun trolling features, and an anticheat to detect players hacking in your lobbies.

# Features
> [!NOTE]
> This repo is a heavily stripped-down version with features I deem too powerful removed to prevent abuse, such as [being able to ban players from lobbies without being host](https://streamable.com/wtb7jl) and [becoming immortal without being host](https://streamable.com/k1b0m0).
>
> Being able to ban players from lobbies without being host is a great way to deal with annoying players who call meetings as soon as the game starts, or ban hackers from lobbies I am inside without having to reach a unanimous votekick, however this opens the door for abuse where someone could ban all the imposters from the lobby to insta-win or banning players who suspect you being an imposter.

- In-game notifications
- Show chat messages by ghosts (Useful for moderation to determine if players are acting fair)
- Always visible chat button
- Be able to flip the Skeld map as host
- Player color randomizer
- Teleporter
- Sabotage and close doors as crewmate
- Device and version spoofer
- See other player's roles
- [Configurable anticheat to detect common hacks and exploits](https://github.com/MrDiamond64/Hydra?tab=readme-ov-file#hydra-anticheat)
- And more!

# Hydra Anticheat
Hydra Anticheat is quite possibly the heart of this mod. It is able to detect when players attempt to cheat, such as if they enter vents without the proper roles, or try to teleport around the map. Upon detecting a cheater, Hydra is able to automatically ban the player from the lobby. You do not have to be the host of the lobby to be able to use Hydra Anticheat, you can do so without being host which will just send you a notification and not ban the player. Hydra Anticheat is meant to extend the vanilla Among Us anticheat by adding checks for cheats it does currently not detect, though it can be used in custom Among Us servers with a less strict anticheat as long as it follows a baseline.

Hydra Anticheat comes with a basic baseline: the backend server must be able to prevent player impersonation. If cheaters are able to send RPCs on the behalf of other players, then Hydra Anticheat will not be able to accurately determine who is cheating or not and flag the wrong players. The vanilla Among Us servers already come with impersonation checks, so this should not be much of a concern in those servers.

# Installation and Usage
> [!WARNING]
> Before using Hydra, please make sure to understand and fully consent to the warnings provided in the [Disclaimer](https://github.com/MrDiamond64/Hydra?tab=readme-ov-file#disclaimer) section.

As Hydra is a BepInEx mod, you will need to download BepInEx. BepInEx binaries are included in the Releases tab, however you can download the required build yourself at https://builds.bepinex.dev/projects/bepinex_be, and find version 735 (commit `5fef357`). As Among Us uses Il2cpp, you will need to download the IL2CPP variant of BepInEx for your operating system. The specific architecture of BepInEx will depend on where you downloaded Among Us from. If Among Us was installed from the Microsoft Store or Epic Games Store, then you will need the x64 variant, otherwise you will need the x86 variant.

Once you downloaded BepInEx, you can open the installation directory of Among Us (where `Among Us.exe` and `GameAssembly.dll` are located), and drag and drop all the files from BepInEx into that directory. Then, you can download the `Hydra.dll` file from the releases tab to `./BepInEx/plugins`. Once you have downloaded all the required files, you are free to open Among Us in your game launcher. The first launch of Among Us will take significantly longer, this is normal. Any subsequent launches will not have this delay. Once the BepInEx preflight is done, Among Us will open. You can access the Hydra UI by pressing the `Insert` button on your keyboard. This will show you the Hydra UI.

Hydra UI has multiple parts: the sections pane, and the features panes. The sections pane will have a list of buttons such as `Self`, `Host`, and `Anticheat`. Pressing any of these section buttons will show the features for this section in the Features Pane. The Features Pane will have sliders, buttons, and checkboxes which can be used to configure Hydra.

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
