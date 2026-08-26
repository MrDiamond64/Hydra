using HarmonyLib;

namespace HydraMenu.modules.spoofer
{
	internal class SpoofDevice : Module
	{
		public SpoofDevice() : base("SpoofDevice")
		{
			// This module can be enabled at all times
			base.Enabled = true;
		}

		private static SpoofDevice Instance
		{
			get { return ModuleManager.spoofDevice; }
		}

		public Platforms SpoofedPlatform { get; set; } = Constants.GetPlatformType();

		[HarmonyPatch(typeof(PlatformSpecificData), nameof(PlatformSpecificData.Serialize))]
		class SerializeDevice
		{
			static void Prefix(PlatformSpecificData __instance)
			{
				__instance.Platform = Instance.SpoofedPlatform;

				switch (Instance.SpoofedPlatform)
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