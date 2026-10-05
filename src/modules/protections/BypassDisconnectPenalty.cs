using AmongUs.Data.Player;
using HarmonyLib;

namespace HydraMenu.modules.protections
{
	internal class BypassDisconnectPenalty : Module
	{
		public BypassDisconnectPenalty() : base("BypassDisconnectPenalty")
		{
			base.Enabled = true;
		}

		private static BypassDisconnectPenalty Instance
		{
			get { return ModuleManager.bypassDisconnectPenalty; }
		}

		// Developing this module was slightly difficult, as a lot of the ban points handling are in small getter functions
		// which mostly all get inlined by the Il2Cpp compiler
		// I looked through the GameAssembly.dll file in IDA and PlayerBanData::get_BanMinutesLeft was the few functions not inlined
		[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.BanMinutesLeft), MethodType.Getter)]
		class GetBanMinutes
		{
			static bool Prefix(PlayerBanData __instance, ref int __result)
			{
				if(!Instance.Enabled) return true;

				__instance.banPoints = 0.0f;
				__result = 0;
				return false;
			}
		}
	}
}