using HarmonyLib;
using InnerNet;

namespace HydraMenu.modules.host
{
	internal class BanMidGame : Module
	{
		public BanMidGame() : base("BanMidGame") { }

		public static BanMidGame Instance
		{
			get { return ModuleManager.banMidGame; }
		}

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanBan))]
		class CanBan
		{
			static bool Prefix(InnerNetClient __instance, ref bool __result)
			{
				if(!Instance.Enabled) return true;

				__result = __instance.AmHost;
				return false;
			}
		}
	}
}