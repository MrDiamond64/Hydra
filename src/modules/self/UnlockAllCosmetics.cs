using AmongUs.Data.Player;
using HarmonyLib;

namespace HydraMenu.modules.self
{
	internal class UnlockAllCosmetics : Module
	{
		public UnlockAllCosmetics() : base("UnlockAllCosmetics") { }

		private static UnlockAllCosmetics Instance
		{
			get { return ModuleManager.unlockAllCosmetics; }
		}

		[HarmonyPatch(typeof(PlayerPurchasesData), nameof(PlayerPurchasesData.GetPurchase))]
		class GetPurchase
		{
			static void Postfix(ref bool __result)
			{
				__result |= Instance.Enabled;
			}
		}
	}
}
