using AmongUs.GameOptions;
using System.Collections.Generic;

namespace HydraMenu.assets
{
	internal class GameAssets
	{
		// The RoleTypes enum has some weird gaps, everything from Crewmate (0) to Tracker (10) is normal, but then Detective is 12, Viper is 18, and Spirit Guide is 21
		// https://www.innersloth.com/2026-roadmap-part-1/
		// The Among Us 2026 roadmap does state that there are currently 15 prototype roles in the works,
		// could these gaps be attributed to roles that have not been added to the retail version of the game?
		public static readonly List<RoleTypes> RolesList = new List<RoleTypes>()
		{
			RoleTypes.Crewmate,
			RoleTypes.Scientist,
			RoleTypes.Engineer,
			RoleTypes.Noisemaker,
			RoleTypes.Tracker,
			RoleTypes.Detective,
			RoleTypes.Judge,
			RoleTypes.Impostor,
			RoleTypes.Shapeshifter,
			RoleTypes.Phantom,
			RoleTypes.Viper,
			RoleTypes.CrewmateGhost,
			RoleTypes.GuardianAngel,
			RoleTypes.SpiritGuide,
			RoleTypes.ImpostorGhost
		};

		public static readonly SortedList<int, string> BroadcastVersions = new SortedList<int, string>()
		{
			{ 50632950, "16.1.0" },
			{ 50643450, "17.1.0" },
			{ 50647000, "17.1.2" },
			{ 50645050, "17.2.0" },
			{ 50652900, "17.2.1 (build 6714)" }, // iOS
			{ 50653700, "17.2.2 (build 6768)" }, // iOS
			{ 50652400, "17.3.0 (build 6803)" }, // iOS and PC
			{ 50656250, "17.3.1 (build 6841)"}, // iOS
			{ 50656300, "17.4.0 (build 7044)" }, // PC
			{ 50658350, "17.4.0 (build 7045)"}, // iOS
			{ 50663350, "18.0.0" }, // PC
			{ 50663400, "18.0.0 (build 7348)" }, // iOS
			{ 50663650, "19.0.0 (build 7488)" }, // iOS
			{ 50663600, "19.0.0 (build 7489)" } // PC
		};

		public static readonly SortedList<int, string> PlayerPlatforms = new SortedList<int, string>()
		{
			{ 0, "Unknown" },
			{ 1, "Epic Games" },
			{ 2, "Steam" },
			{ 3, "MacOS" },
			{ 4, "Microsoft Store" },
			{ 5, "Itch.io" },
			{ 6, "iOS" },
			{ 7, "Android" },
			{ 8, "Nintendo Switch" },
			{ 9, "Xbox" },
			{ 10, "PlayStation" },
			{ 112, "Starlight" }
		};

		public static readonly SortedList<string, string> ModRegistrationGuids = new SortedList<string, string>()
		{
			// A blank mod GUID is treated as no mod registered
			{ "", "None" },
			// https://github.com/Gurge44/EndlessHostRoles/blob/ce23f0d050f617a59507e5bb0fbfff751d4b5134/Modules/AmciRegistration.cs#L23
			{ "446420cc-ce35-4c2f-8f5d-b14f84354421", "Endless Host Roles"},
			// https://github.com/TownOfNext/TownOfNext/blob/f1c5185273091136fb5c6a2bdc4104a02d7eb916/src/main.cs#L17
			{ "789bf6f3-41e4-439f-82d3-ed6d2d2fb710", "Town of Next"},
			// https://github.com/SuperNewRoles/SuperNewRoles/blob/master/SuperNewRoles/statics.cs#L16
			{ "9f21dfdc-1224-4322-af7b-63db54cea706", "Super New Roles" }
			// https://github.com/Limeau/TownofHost-Optimized/blob/025b5c3b9205b7cd54fe97e6b75969071074078a/TOHO/Patches/HostGamePatch.cs#L9
			// { "995c2cea-a12e-418b-a500-6436eefaaf4d", "Town of Host Optimized" }
		};

		public static string GetPlatformName(Platforms platformId)
		{
			return PlayerPlatforms.GetValueOrDefault((int)platformId, $"Unknown {platformId}");
		}

		public static string GetPlatformName(int platformId)
		{
			return PlayerPlatforms.GetValueOrDefault(platformId, $"Unknown {platformId}");
		}
	}
}