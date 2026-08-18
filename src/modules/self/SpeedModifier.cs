using HarmonyLib;

namespace HydraMenu.modules.self
{
	internal class SpeedModifier : Module
	{
		public SpeedModifier() : base("SpeedModifier")
		{
			// This module can be enabled at all times
			Enabled = true;
		}

		public float multiplier = 1.0f;

		public static SpeedModifier Instance
		{
			get { return ModuleManager.speedModifier; }
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]
		public static class PlayerSpeedModifier
		{
			static void Postfix(ref float __result)
			{
				__result *= Instance.multiplier;
			}
		}
	}
}