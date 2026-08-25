using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class ShowGhosts : Module
	{
		public ShowGhosts() : base("ShowGhosts")
		{
			Enabled = true;
		}

		private static ShowGhosts Instance
		{
			get { return ModuleManager.showGhosts; }
		}

		// PlayerControl::FixedUpdate sets PlayerControl::set_Visible to false if the player is dead, or true if the player is alive
		// The set_Visible function runs CosmeticsLayer::set_Visible in order to hide or show the player's cosmetics
		// If we want to show ghosts even if we are alive, then we can reimplement PlayerControl::set_Visible and make it so player cosmetics are always visible
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Visible), MethodType.Setter)]
		class SetVisible
		{
			static bool Prefix(PlayerControl __instance)
			{
				if(!Instance.Enabled || !__instance.Data.IsDead) return true;

				__instance.cosmetics.Visible = true;
				return false;
			}
		}
	}
}