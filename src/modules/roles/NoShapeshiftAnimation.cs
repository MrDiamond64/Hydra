using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class NoShapeshiftAnimation : Module
	{
		public NoShapeshiftAnimation() : base("NoShapeshiftAnimation", [ typeof(FakeShapeshiftBubble) ]) { }

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

		protected override void OnEnable()
		{
			if(ModuleManager.fakeShapeshiftBubble.Enabled)
			{
				Hydra.notifications.Send("No Shapeshift Animation", "The Fake Shapeshift Bubble module is incompatible with this module, so it has been disabled.", 5);
				ModuleManager.fakeShapeshiftBubble.Enabled = false;
			}
		}
	}
}