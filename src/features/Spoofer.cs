using HarmonyLib;

namespace HydraMenu.features
{
    internal class Spoofer
    {
        public static bool shouldSpoofVersion = false;
		public static int spoofedVersion = Constants.GetBroadcastVersion();
        public static bool useModdedProtocol = false;
		public static Platforms spoofedPlatform = Constants.GetPlatformType();

		[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
		class SpoofVersion
		{
			static bool Prefix(ref int __result)
			{
                // Starting a local lobby or entering freeplay will bug out if we are using a spoofed version
                if(!shouldSpoofVersion || !AmongUsClient.Instance || AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return true;

                __result = spoofedVersion;
                if(useModdedProtocol) __result += 25;

                return false;
			}
		}

		[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
		class MarkVersionModded
        {
			static bool Prefix(ref bool __result)
			{
				if(useModdedProtocol)
                {
                    __result = true;
                    return false;
                } else
                {
                    return false;
                }
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