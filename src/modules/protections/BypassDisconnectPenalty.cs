using AmongUs.Data.Player;
using HarmonyLib;

namespace LunarMenu.modules.protections
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
			static void Prefix(PlayerBanData __instance)
			{
				if(!Instance.Enabled) return;
				__instance.banPoints = 0.0f;
			}
		}
	}
}