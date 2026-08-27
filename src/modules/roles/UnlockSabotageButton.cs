using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class UnlockSabotageButton : Module
	{
		public UnlockSabotageButton() : base("UnlockSabotageButton")
		{
			base.Enabled = true;
		}

		private static UnlockSabotageButton Instance
		{
			get { return ModuleManager.unlockSabotageButton; }
		}

		public bool SabotageAsCrewmate { get; set; } = false;
		public bool SabotageInVents { get; set; } = false;

		// Clicking the sabotage button has checks to make sure the current player is indeed an imposter, not in a vent, and that the current gamemode supports sabotages
		// This means setting the GameObject's sabotage button state to active won't allow crewmates to sabotage alone, we need to override the DoClick function to not have those checks
		[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
		class SkipSabotageChecks
		{
			static bool Prefix()
			{
				PlayerControl player = PlayerControl.LocalPlayer;

				// We have to limit this to Imposters as the crewmate exit vent button will be on the same position as the imposter sabotage button
				if(!Instance.SabotageInVents && player.inVent && !RoleManager.IsImpostorRole(player.Data.RoleType)) return true;

				HudManager.Instance.ToggleMapVisible(new MapOptions { Mode = MapOptions.Modes.Sabotage });
				return false;
			}
		}
	}
}