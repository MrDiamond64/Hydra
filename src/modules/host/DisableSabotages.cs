using HarmonyLib;

namespace HydraMenu.modules.host
{
	internal class DisableSabotages : Module
	{
		public DisableSabotages() : base("DisableSabotages") { }

		private static DisableSabotages Instance
		{
			get { return ModuleManager.disableSabotages; }
		}

		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
		class OnSabotage
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}
	}
}