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

namespace HydraMenu.modules
{
	internal class ModuleManager
	{
		// Host
		public static AssignRoles assignRoles = new AssignRoles();
		public static BanMidGame banMidGame = new BanMidGame();
		public static BlockLowLevels blockLowLevels = new BlockLowLevels();
		public static DisableCameras disableCameras = new DisableCameras();
		public static DisableCloseDoors disableCloseDoors = new DisableCloseDoors();
		public static DisableGameEnd disableGameEnd = new DisableGameEnd();
		public static DisableMeetings disableMeetings = new DisableMeetings();
		public static DisableSabotages disableSabotages = new DisableSabotages();
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
		public static BlockSabotages blockSabotages = new BlockSabotages();
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
				disableCameras,
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
				blockSabotages,
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
		public static Dictionary<string, Dictionary<string, JsonElement>> GetConfigData()
		{
			Dictionary<string, Dictionary<string, JsonElement>> moduleConfig = new Dictionary<string, Dictionary<string, JsonElement>>();

			foreach(Module module in moduleList)
			{
				moduleConfig.Add(module.name, module.GetConfigData());
			}

			return moduleConfig;
		}

		public static void LoadConfigData(Dictionary<string, Dictionary<string, JsonElement>> moduleConfig)
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
	}
}