using HydraMenu.modules.host;
using HydraMenu.modules.protections;
using HydraMenu.modules.roles;
using HydraMenu.modules.self;
using HydraMenu.modules.spoofer;
using HydraMenu.modules.troll;
using HydraMenu.modules.visuals;
using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace HydraMenu.modules
{
	internal class ModuleManager : MonoBehaviour
	{
		// Host
		public static AssignRoles assignRoles = new AssignRoles();
		public static BanMidGame banMidGame = new BanMidGame();
		public static BlockLowLevels blockLowLevels = new BlockLowLevels();
		public static DisableCloseDoors disableCloseDoors = new DisableCloseDoors();
		public static DisableGameEnd disableGameEnd = new DisableGameEnd();
		public static DisableMeetings disableMeetings = new DisableMeetings();
		public static FlipSkeld flipSkeld = new FlipSkeld();
		public static NoKillCooldown noKillCooldown = new NoKillCooldown();

		// Protections
		public static AntiKick antiKick = new AntiKick();
		public static AntiOverload antiOverload = new AntiOverload();
		public static BlockServerTeleports blockServerTeleports = new BlockServerTeleports();
		public static BlockUnauthorizedUpdates blockUnauthorizedUpdates = new BlockUnauthorizedUpdates();
		public static BypassShapeshiftRatelimits bypassShapeshiftRatelimits = new BypassShapeshiftRatelimits();
		public static ForceDTLs forceDtls = new ForceDTLs();

		// Roles
		public static MoveInVents moveInVents = new MoveInVents();
		public static NoKillChecks noKillChecks = new NoKillChecks();
		public static NoShapeshiftAnimation noShapeshiftAnimation = new NoShapeshiftAnimation();
		public static UnlockSabotageButton unlockSabotageButton = new UnlockSabotageButton();
		public static VentAsCrewmate ventAsCrewmate = new VentAsCrewmate();

		// Self
		public static AlwaysShowTaskAnimations alwaysShowTaskAnimations = new AlwaysShowTaskAnimations();
		public static Immortality immortality = new Immortality();
		public static NoLadderCooldown noLadderCooldown = new NoLadderCooldown();
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
		public static DisableCameras disableCameras = new DisableCameras();
		public static DisableSabotages disableSabotages = new DisableSabotages();
		public static DisableVents disableVents = new DisableVents();

		// Visual
		public static AccurateDisconnectReason accurateDisconnectReason = new AccurateDisconnectReason();
		public static AlwaysVisibleChat alwaysVisibleChat = new AlwaysVisibleChat();
		public static Fullbright fullbright = new Fullbright();
		public static NoSeekerAnimation noSeekerAnimation = new NoSeekerAnimation();
		public static ShowGhostMessages showGhostMessages = new ShowGhostMessages();
		public static ShowGhosts showGhosts = new ShowGhosts();
		public static ShowProtections showProtections = new ShowProtections();
		public static SkipShhhAnimation skipShhhAnimation = new SkipShhhAnimation();
		public static SpectatePlayer spectatePlayer = new SpectatePlayer();

		public static readonly Module[] moduleList;

		static ModuleManager()
		{
			moduleList = [
				assignRoles,
				banMidGame,
				blockLowLevels,
				disableCloseDoors,
				disableGameEnd,
				disableMeetings,
				flipSkeld,
				noKillCooldown,

				antiKick,
				antiOverload,
				blockServerTeleports,
				blockUnauthorizedUpdates,
				bypassShapeshiftRatelimits,
				forceDtls,

				moveInVents,
				noKillChecks,
				noShapeshiftAnimation,
				unlockSabotageButton,
				ventAsCrewmate,

				alwaysShowTaskAnimations,
				immortality,
				noLadderCooldown,
				speedModifier,
				unlimitedMeetings,
				updateStatsFreeplay,

				spoofDevice,
				spoofLevel,
				spoofVersion,

				autoExposeImpostors,
				autoReportBodies,
				disableCameras,
				disableSabotages,
				disableVents,

				accurateDisconnectReason,
				alwaysVisibleChat,
				fullbright,
				noSeekerAnimation,
				showGhostMessages,
				showGhosts,
				showProtections,
				skipShhhAnimation,
				spectatePlayer
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
					Hydra.Log.LogWarning($"Config has entry for module {moduleName} when there is no such module");
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
			if(alwaysVisibleChat.Enabled)
			{
				HudManager.Instance.MatchInfoButton.gameObject.SetActive(MeetingHud.Instance != null);
			}
		}
	}
}