using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class AlwaysVisibleChat : Module
	{
		public AlwaysVisibleChat() : base("AlwaysVisibleChat")
		{
			base.Enabled = true;
		}

		private static AlwaysVisibleChat Instance
		{
			get { return ModuleManager.alwaysVisibleChat; }
		}

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.SetVisible))]
		class SetChatVisibility
		{
			static void Prefix(ref bool visible)
			{
				if(Instance.Enabled) visible = true;
			}
		}

		[HarmonyPatch(typeof(MatchInfoHudButton), nameof(MatchInfoHudButton.Update))]
		class FixMatchInfoOverlap
		{
			static bool Prefix(MatchInfoHudButton __instance)
			{
				if(!Instance.Enabled) return true;

				__instance.aspectPosition.DistanceFromEdge = MatchInfoHudButton.adjustedDistanceFromEdge;
				return false;
			}
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;

			HudManager.Instance.Chat.SetVisible(true);
		}

		protected override void OnDisable()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;

			bool shouldBeEnabled = RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType) || LobbyBehaviour.Instance != null || MeetingHud.Instance != null;
			HudManager.Instance.Chat.SetVisible(shouldBeEnabled);
		}
	}
}