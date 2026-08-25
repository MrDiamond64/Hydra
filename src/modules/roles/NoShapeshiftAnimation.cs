using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class NoShapeshiftAnimation : Module
	{
		public NoShapeshiftAnimation() : base("NoShapeshiftAnimation") { }

		private static NoShapeshiftAnimation Instance
		{
			get { return ModuleManager.noShapeshiftAnimation; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
		class StartShapeshift
		{
			static void Prefix(ref bool shouldAnimate)
			{
				if(!Instance.Enabled) return;

				shouldAnimate = false;
			}
		}

		// PlayerControl::CmdCheckRevertShapeshift calls the PlayerControl::CmdCheckShapeshift function which we patch above, however the function is inlined
		// so we have to patch PlayerControl::CmdCheckRevertShapeshift as well
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
		class RevertShapeshift
		{
			static void Prefix(ref bool shouldAnimate)
			{
				if(!Instance.Enabled) return;

				shouldAnimate = false;
			}
		}
	}
}