using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class NoInfluencerRefreshCooldown : Module
	{
		// influencer lets you refresh the images in the spirit guide panel, which also resets the ability cooldown
		// RefreshImages function applies the refresh cooldown
		// zeroing cooldown while panel is open and no image selected means the refresh button can be pressed as fast as possible without the cooldown
		public NoInfluencerRefreshCooldown() : base("NoInfluencerRefreshCooldown")
		{
			base.Enabled = true;
		}

		private static NoInfluencerRefreshCooldown Instance
		{
			get { return ModuleManager.noInfluencerRefreshCooldown; }
		}

		[HarmonyPatch(typeof(SpiritGuideRole), nameof(SpiritGuideRole.FixedUpdate))]
		class NoInfluencerRefreshCooldownPatch
		{
			static void Prefix(SpiritGuideRole __instance)
			{
				if(!Instance.Enabled || !__instance.spiritGuidePanel.activeSelf) return;
				// panel is open while picking images so maybe wait until the selection is empty again?
				if(__instance.selectedImageButtons.Count > 0) return;
				__instance.cooldownSecondsRemaining = 0.0f;
			}
		}
	}
}