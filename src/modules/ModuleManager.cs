using HarmonyLib;
using LunarMenu.modules.general;
using LunarMenu.modules.host;
using LunarMenu.modules.protections;
using LunarMenu.modules.roles;
using LunarMenu.modules.self;
using LunarMenu.modules.spoofer;
using LunarMenu.modules.troll;
using LunarMenu.modules.visuals;
using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace LunarMenu.modules
{
	internal class ModuleManager : MonoBehaviour
	{
		// General
		public static AutoCopyLobbyCode autoCopyLobbyCode = new AutoCopyLobbyCode();
		public static UnlockCosmetics unlockCosmetics = new UnlockCosmetics();

		// Host
		public static AssignRoles assignRoles = new AssignRoles();
		public static BanMidGame banMidGame = new BanMidGame();
		public static BlockLowLevels blockLowLevels = new BlockLowLevels();
		public static DisableGameEnd disableGameEnd = new DisableGameEnd();
		public static DisableMeetings disableMeetings = new DisableMeetings();
		public static DisableVentClean disableVentClean = new DisableVentClean();
		public static FakeShapeshiftBubble fakeShapeshiftBubble = new FakeShapeshiftBubble();
		public static FlipSkeld flipSkeld = new FlipSkeld();
		public static VoteImmune voteImmune = new VoteImmune();

		// Protections
		public static AntiCrash antiCrash = new AntiCrash();
		public static AntiKick antiKick = new AntiKick();
		public static AntiOverload antiOverload = new AntiOverload();
		public static BlockServerTeleports blockServerTeleports = new BlockServerTeleports();
		public static BlockUnauthorizedUpdates blockUnauthorizedUpdates = new BlockUnauthorizedUpdates();
		public static BypassDisconnectPenalty bypassDisconnectPenalty = new BypassDisconnectPenalty();
		public static BypassShapeshiftRatelimits bypassShapeshiftRatelimits = new BypassShapeshiftRatelimits();
		public static ForceDTLs forceDtls = new ForceDTLs();

		// Roles
		public static BypassComms bypassComms = new BypassComms();
		public static MoveInVents moveInVents = new MoveInVents();
		public static NoKillChecks noKillChecks = new NoKillChecks();
		public static NoSabotageCooldown noSabotageCooldown = new NoSabotageCooldown();
		public static NoShapeshiftAnimation noShapeshiftAnimation = new NoShapeshiftAnimation();
		public static UnlockSabotageButton unlockSabotageButton = new UnlockSabotageButton();
		public static VentAsCrewmate ventAsCrewmate = new VentAsCrewmate();

		// Self
		public static AlwaysShowTaskAnimations alwaysShowTaskAnimations = new AlwaysShowTaskAnimations();
		public static Immortality immortality = new Immortality();
		public static NoLadderCooldown noLadderCooldown = new NoLadderCooldown();
		public static NoZiplineCooldown noZiplineCooldown = new NoZiplineCooldown();
		public static SpeedModifier speedModifier = new SpeedModifier();
		public static UnlimitedMeetings unlimitedMeetings = new UnlimitedMeetings();
		public static UpdateStatsFreeplay updateStatsFreeplay = new UpdateStatsFreeplay();

		// Spoofer
		public static SpoofDevice spoofDevice = new SpoofDevice();
		public static SpoofLevel spoofLevel = new SpoofLevel();
		public static SpoofVersion spoofVersion = new SpoofVersion();

		// Troll
		public static AutoExposeImpostors autoExposeImpostors = new AutoExposeImpostors();
		public static AutoReportBodies autoReportBodies = new AutoReportBodies();
		public static CrashLobby crashLobby = new CrashLobby();
		public static DisableCameras disableCameras = new DisableCameras();
		public static DisableCloseDoors disableCloseDoors = new DisableCloseDoors();
		public static DisableSabotages disableSabotages = new DisableSabotages();
		public static DisableVents disableVents = new DisableVents();

		// Visual
		public static AccurateDisconnectReason accurateDisconnectReason = new AccurateDisconnectReason();
		public static AlwaysVisibleChat alwaysVisibleChat = new AlwaysVisibleChat();
		public static Fullbright fullbright = new Fullbright();
		public static NoSeekerAnimation noSeekerAnimation = new NoSeekerAnimation();
        public static RevealRoles revealRoles = new RevealRoles();
		public static RevealVotes revealVotes = new RevealVotes();
        public static ShowFPS showFPS = new ShowFPS();
        public static ShowGhostMessages showGhostMessages = new ShowGhostMessages();
		public static ShowGhosts showGhosts = new ShowGhosts();
		public static ShowLobbyInfo showLobbyInfo = new ShowLobbyInfo();
        public static ShowPhantoms showPhantoms = new ShowPhantoms();
		public static ShowPlayersInVents showPlayersInVents = new ShowPlayersInVents();
        public static ShowProtections showProtections = new ShowProtections();
		public static SkipShhhAnimation skipShhhAnimation = new SkipShhhAnimation();
		public static SpectatePlayer spectatePlayer = new SpectatePlayer();

        public static readonly Module[] moduleList;

		static ModuleManager()
		{
			moduleList = [
				autoCopyLobbyCode,
				unlockCosmetics,

				assignRoles,
				banMidGame,
				blockLowLevels,
				disableGameEnd,
				disableMeetings,
				disableVentClean,
				fakeShapeshiftBubble,
				flipSkeld,
				voteImmune,

				antiCrash,
				antiKick,
				antiOverload,
				blockServerTeleports,
				blockUnauthorizedUpdates,
				bypassDisconnectPenalty,
				bypassShapeshiftRatelimits,
				forceDtls,

				bypassComms,
				moveInVents,
				noKillChecks,
				noSabotageCooldown,
				noShapeshiftAnimation,
				unlockSabotageButton,
				ventAsCrewmate,

				alwaysShowTaskAnimations,
				immortality,
				noLadderCooldown,
				noZiplineCooldown,
				speedModifier,
				unlimitedMeetings,
				updateStatsFreeplay,

				spoofDevice,
				spoofLevel,
				spoofVersion,

				autoExposeImpostors,
				autoReportBodies,
				crashLobby,
				disableCameras,
				disableCloseDoors,
				disableSabotages,
				disableVents,

				accurateDisconnectReason,
				alwaysVisibleChat,
				fullbright,
				noSeekerAnimation,
				revealRoles,
				revealVotes,
				showFPS,
				showGhostMessages,
				showGhosts,
				showLobbyInfo,
				showPhantoms,
				showPlayersInVents,
				showProtections,
				skipShhhAnimation,
				spectatePlayer,
			];
		}

		// Return a dictionary of each module with its name, and another dictionary with names and values of each property
		public Dictionary<string, Dictionary<string, JsonElement>> GetConfigData()
		{
			Dictionary<string, Dictionary<string, JsonElement>> moduleConfig = new Dictionary<string, Dictionary<string, JsonElement>>();

			foreach(Module module in moduleList)
			{
				moduleConfig.Add(module.name, module.GetConfigData());
			}

			return moduleConfig;
		}

		public void LoadConfigData(Dictionary<string, Dictionary<string, JsonElement>> moduleConfig)
		{
			foreach((string moduleName, Dictionary<string, JsonElement> configData) in moduleConfig)
			{
				int moduleIndex = Array.FindIndex(moduleList, r => r.name == moduleName);
				if(moduleIndex == -1)
				{
					Lunar.Log.LogWarning($"Config has entry for module {moduleName} when there is no such module");
					continue;
				}

				Module module = moduleList[moduleIndex];
				module.LoadConfigData(configData);
			}
		}

		// No idea where else to put this
		public void Update()
		{
			// If PlayerControl::Data isn't null, then we know the player has fully loaded into the game
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;

			if(unlockSabotageButton.SabotageAsCrewmate) HudManager.Instance.SabotageButton.gameObject.SetActive(true);
			if(ventAsCrewmate.Enabled) HudManager.Instance.ImpostorVentButton.gameObject.SetActive(true);

			// The Chat button and Match Info buttons will overlap if both are active in-game (but not in meetings)
			// I tried modifying `MatchInfoButton.transform.position` and the likes to try and shift the button towards the left
			// however that only moved the collider of the button, not the icon
			// So we just use this workaround to hide the Match Info Button in situations where it will not overlap with the Chat button
			/* if(alwaysVisibleChat.Enabled)
			{
				HudManager.Instance.MatchInfoButton.gameObject.SetActive(MeetingHud.Instance != null);
			} */
		}

		[HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
		class EnableNameObject
		{
			static void Prefix(GameObject __instance, ref bool value)
			{
				try
				{
					if ((Utilities.InGame() || Utilities.InLobby()) && !value)
					{
						foreach (var player in PlayerControl.AllPlayerControls)
						{
							var playerInfo = player?.Data;
							if (!playerInfo || !player.cosmetics || player.visibilityItems != null) break;

                            var nameObject = player.cosmetics.nameText.gameObject;
                            if (((playerInfo.IsDead && showGhosts.Enabled) || (!playerInfo.IsDead && player.shouldAppearInvisible && showPhantoms.Enabled)) || (!playerInfo.IsDead && revealRoles.Enabled) && nameObject == __instance)
							{
								value = true;
								break;
							}
							else if ((playerInfo.IsDead || (!playerInfo.IsDead && player.shouldAppearInvisible)) && !revealRoles.Enabled && nameObject == __instance)
							{
                                value = false;
                                break;
                            }
						}
					}
				} catch { }
			}
		}
	}
}