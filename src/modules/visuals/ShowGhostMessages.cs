namespace HydraMenu.modules.visuals
{
	internal class ShowGhostMessages : Module
	{
		public ShowGhostMessages() : base("ShowGhostMessages")
		{
			base.Enabled = true;
		}

		private void OnPlayerChat(PlayerControl player, string text)
		{
			if(player == null) return;

			// This is kind of a hacky workaround to be able to see messages by ghosts
			// The game has no easy way to show messages by ghosts, so we would have to completely reimplement the ChatController::AddChat function
			// I don't really like reimplementing large functions as it makes backwards compatability harder and requires more effort when updating the mod to newer versions of AU
			// Instead of having to reimplement the function, we can just use ChatController::AddChatWarning to add a chat bubble and include the player's name and message contents to the warning
			if(!PlayerControl.LocalPlayer.Data.IsDead && player.Data.IsDead)
			{
				HudManager._instance.Chat.AddChatWarning($"{player.Data.PlayerName}\n{text}");
			}
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerChat += OnPlayerChat;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerChat -= OnPlayerChat;
		}
	}
}