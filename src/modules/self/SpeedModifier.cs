using HarmonyLib;

namespace HydraMenu.modules.self
{
	internal class SpeedModifier : Module
	{
		public SpeedModifier() : base("SpeedModifier")
		{
			// This module can be enabled at all times
			base.Enabled = true;
		}

		public float Multiplier { get; set; } = 1.0f;

		private static SpeedModifier Instance
		{
			get { return ModuleManager.speedModifier; }
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]
		class PlayerSpeedModifier
		{
			static void Postfix(ref float __result)
			{
				__result *= Instance.Multiplier;
			}
		}
	}
}