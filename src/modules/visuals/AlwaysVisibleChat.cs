using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class AlwaysVisibleChat : Module
	{
		public AlwaysVisibleChat() : base("AlwaysVisibleChat")
		{
			Enabled = true;
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