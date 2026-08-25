using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class SkipShhhAnimation : Module
	{
		public SkipShhhAnimation() : base("SkipShhhAnimation")
		{
			Enabled = true;
		}

		private static SkipShhhAnimation Instance
		{
			get { return ModuleManager.skipShhhAnimation; }
		}

		[HarmonyPatch(typeof(ShhhBehaviour), nameof(ShhhBehaviour.PlayAnimation))]
		class PlayShhhAnimation
		{
			static bool Prefix()
			{
				if(!Instance.Enabled) return true;

				HudManager.Instance.shhhEmblem.gameObject.SetActive(false);
				return false;
			}
		}
	}
}