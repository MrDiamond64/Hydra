using HarmonyLib;
using InnerNet;

namespace HydraMenu.anticheat
{
	internal class PlatformSpoofer : ICheck
	{
		// Refer to HydraMenu.features.PlayerLogger to know why we hook PlayerControl::SetName for player joins
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetName))]
		class PlatformSpoof
		{
			static void Postfix(PlayerControl __instance)
			{
				if(!Anticheat.Enabled || !Anticheat.CheckSpoofedPlatforms) return;

				ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
				if(clientData == null) return;

				PlatformSpecificData platformData = clientData.PlatformData;

				if(!IsValidPlatform(platformData))
				{
					Hydra.notifications.Send("Anticheat", $"{__instance.Data.PlayerName} was detected with spoofed platform information. Platform: {platformData.Platform}, Platform name: {platformData.PlatformName}, XUID: {platformData.XboxPlatformId}, PSID: {platformData.PsnPlatformId}.");
					Anticheat.Punish(__instance);
				}
			}
		}

		public static bool IsValidPlatform(PlatformSpecificData platform)
		{
			string platformName = platform.PlatformName;
			ulong xuid = platform.XboxPlatformId;
			ulong psid = platform.PsnPlatformId;

			switch(platform.Platform)
			{
				case Platforms.StandaloneEpicPC:
				case Platforms.StandaloneSteamPC:
				case Platforms.StandaloneMac:
				case Platforms.StandaloneItch:
				case Platforms.IPhone:
				case Platforms.Android:
					if(IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
					break;

				case Platforms.StandaloneWin10:
					if(IsGenericPlatformName(platformName) && xuid != 0 && psid == 0) return true;
					break;

				case Platforms.Xbox:
					// Xbox Gamertags must be in the range of 3 to 16 characters
					// Other rules for gamertags: https://learn.microsoft.com/en-us/gaming/gdk/docs/store/policies/xr/xr046?view=gdk-2510
					// We could potentially resolve XUIDs into gamertags and see if it matches, but the Xbox live API endpoint for XUID->gamertag is
					// authentication locked
					if(!IsGenericPlatformName(platformName) && platformName.Length >= 3 && platformName.Length <= 16 && xuid != 0 && psid == 0) return true;
					break;

				case Platforms.Playstation:
					if(!IsGenericPlatformName(platformName) && xuid == 0 && psid != 0) return true;
					break;

				case Platforms.Switch:
					if(!IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
					break;

				// On Local lobbies, all players have a platform ID of 255
				case (Platforms)255:
					if(AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return true;
					break;
			}

			// If the Platform ID is invalid, or the platform specific data for each platform is invalid, then we know that the player's device is spoofed
			return false;
		}

		public static bool IsGenericPlatformName(string platformName)
		{
			return platformName == "TESTNAME";
		}
	}
}