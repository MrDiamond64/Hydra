namespace HydraMenu.modules.misc
{
	internal class ChatLogger : Module
	{
		public ChatLogger() : base("ChatLogger")
		{
			base.Enabled = true;
		}

		private void OnPlayerChat(PlayerControl sender, string text)
		{
			if(sender.Data == null) return;

			Hydra.Log.LogMessage($"[ChatLogger] {sender.Data.PlayerName}: {text}");
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