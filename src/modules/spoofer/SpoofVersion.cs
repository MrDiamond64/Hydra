using HarmonyLib;

namespace HydraMenu.modules.spoofer
{
	internal class SpoofVersion : Module
	{
		public SpoofVersion() : base("SpoofVersion") { }

		public int spoofedVersion = Constants.GetBroadcastVersion();
		public bool useModdedProtocol = false;

		public static SpoofVersion Instance
		{
			get { return ModuleManager.spoofVersion; }
		}

		[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
		class BroadcastVersion
		{
			static bool Prefix(ref int __result)
			{
				// Starting a local lobby or entering Freeplay will bug out if we are using a spoofed version
				if(!Instance.Enabled || AmongUsClient.Instance == null || AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return true;

				__result = Instance.spoofedVersion;
				if(Instance.useModdedProtocol) __result += 25;

				return false;
			}
		}

		[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
		class MarkVersionModded
		{
			static bool Prefix(ref bool __result)
			{
				if(!Instance.Enabled && !Instance.useModdedProtocol) return true;

				__result = true;
				return false;
			}
		}
	}
}