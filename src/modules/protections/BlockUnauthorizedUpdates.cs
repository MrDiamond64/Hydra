using HarmonyLib;

namespace HydraMenu.modules.protections
{
	internal class BlockUnauthorizedUpdates : Module
	{
		// All ShipStatus RPCs (CloseDoorsOfType and UpdateSystem) should only ever be sent to the host
		// It is possible for a non-host to send system updates to anyone they want
		// and cause a desync between the actual game state and their game state
		public BlockUnauthorizedUpdates() : base("BlockUnauthorizedUpdates")
		{
			base.Enabled = true;
		}

		private static BlockUnauthorizedUpdates Instance
		{
			get { return ModuleManager.blockUnauthorizedUpdates; }
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix()
			{
				return !Instance.Enabled || AmongUsClient.Instance.AmHost;
			}
		}
	}
}