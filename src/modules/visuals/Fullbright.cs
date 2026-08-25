using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class Fullbright : Module
	{
		public Fullbright() : base("Fullbright") { }

		private static Fullbright Instance
		{
			get { return ModuleManager.fullbright; }
		}

		// Is there a better way of implementing Fullbright?
		// This current method does not allow you to see through walls due to shadows
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
		class LightRadius
		{
			static bool Prefix(ref float __result)
			{
				if(!Instance.Enabled) return true;

				__result = 1000f;
				return false;
			}
		}
	}
}