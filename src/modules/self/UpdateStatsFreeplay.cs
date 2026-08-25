using AmongUs.Data.Player;
using HarmonyLib;

namespace HydraMenu.modules.self
{
	internal class UpdateStatsFreeplay : Module
	{
		public UpdateStatsFreeplay() : base("UpdateStatsFreeplay") { }

		private static UpdateStatsFreeplay Instance
		{
			get { return ModuleManager.updateStatsFreeplay; }
		}

		[HarmonyPatch(typeof(PlayerStatsData), nameof(PlayerStatsData.ValidateStat))]
		class ValidateStat
		{
			static void Prefix(PlayerStatsData __instance)
			{
				if(!Instance.Enabled) return;

				__instance.isTrackingStats = true;
			}
		}
	}
}