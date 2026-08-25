using HarmonyLib;

namespace HydraMenu.modules.host
{
	internal class DisableCloseDoors : Module
	{
		public DisableCloseDoors() : base("DisableCloseDoors") { }

		private static DisableCloseDoors Instance
		{
			get { return ModuleManager.disableCloseDoors; }
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
		class OnCloseDoor
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}
	}
}