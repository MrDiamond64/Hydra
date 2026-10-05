using AmongUs.GameOptions;
using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class NoInfluencerRefreshCooldown : Module
	{
		private static NoInfluencerRefreshCooldown Instance
		{
			get { return ModuleManager.noInfluencerRefreshCooldown; }
		}

		public NoInfluencerRefreshCooldown() : base("NoInfluencerRefreshCooldown")
		{
			base.Enabled = true;
		}

		private void OnUseRoleAbility(bool isSecondary)
		{
			if(!isSecondary && Utilities.IsAnticheatPresent()) return;

			// have to use TryCast here
			SpiritGuideRole role = PlayerControl.LocalPlayer.Data.Role.TryCast<SpiritGuideRole>();
			if(role == null) return;
			ActionButton button = isSecondary ? HudManager.Instance.SecondaryAbilityButton : HudManager.Instance.AbilityButton;
			button.SetCoolDown(0.0f, GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.SpiritGuideCooldownSeconds));
		}
		
		[HarmonyPatch(typeof(SpiritGuideRole), nameof(SpiritGuideRole.SetCooldown))]
		class SkipCooldown
		{
			static void Postfix(SpiritGuideRole __instance)
			{
				if(!Instance.Enabled || Utilities.IsAnticheatPresent()) return;

				__instance.cooldownSecondsRemaining = 0.0f;

				float maxCooldown = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.SpiritGuideCooldownSeconds);
				HudManager.Instance.AbilityButton.SetCoolDown(0.0f, maxCooldown);
				HudManager.Instance.SecondaryAbilityButton.SetCoolDown(0.0f, maxCooldown);
			}
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnUseRoleAbility += OnUseRoleAbility;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnUseRoleAbility -= OnUseRoleAbility;
		}
	}
}