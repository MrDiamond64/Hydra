using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;

namespace HydraMenu.features
{
	internal class Spoofer
	{
		public static Platforms spoofedPlatform = Constants.GetPlatformType();

		// PlayerControl::RpcSetLevel is inlined in PlayerControl::Start so we cannot patch that function directly
		[HarmonyPatch(typeof(RpcSetLevelMessage), nameof(RpcSetLevelMessage.SerializeRpcValues))]
		public static class SpoofLevel
		{
			public static bool Enabled { get; set; } = false;
			public static uint newLevel = 200;

			static bool Prefix(MessageWriter msg)
			{
				if(!Enabled) return true;

				msg.WritePacked(newLevel - 1);
				PlayerControl.LocalPlayer.SetLevel(newLevel - 1);
				return false;
			}
		}

		[HarmonyPatch(typeof(PlatformSpecificData), nameof(PlatformSpecificData.Serialize))]
		class SpoofPlatform
		{
			static void Prefix(PlatformSpecificData __instance)
			{
				__instance.Platform = spoofedPlatform;

				switch (spoofedPlatform)
				{
					case Platforms.StandaloneWin10:
						__instance.XboxPlatformId = 2584878536129841;
						break;

					case Platforms.Xbox:
						// You can find the proper XUID for an Xbox gamertag at https://www.cxkes.me/xbox/xuid
						__instance.PlatformName = "Major Nelson";
						__instance.XboxPlatformId = 2584878536129841;
						break;

					case Platforms.Playstation:
						__instance.PlatformName = "";
						__instance.PsnPlatformId = 0;
						break;

					case Platforms.Switch:
						__instance.PlatformName = "Sus";
						break;

					default:
						// Other platforms do not send additional platform specific data
						__instance.PlatformName = "TESTNAME";
						__instance.XboxPlatformId = 0;
						__instance.PsnPlatformId = 0;
						break;
				}
			}
		}
	}
}