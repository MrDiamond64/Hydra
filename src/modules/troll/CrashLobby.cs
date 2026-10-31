namespace HydraMenu.modules.troll
{
	internal class CrashLobby : Module
	{
		public CrashLobby() : base("CrashLobby") { }

		private void OnGameStart()
		{
			PlayerControl.LocalPlayer.CmdReportDeadBody(null);
			Hydra.notifications.Send("Lobby Crasher", "The lobby has been crashed.");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameStart += OnGameStart;

			Hydra.notifications.Send("Lobby Crasher", "Crash Lobby has been enabled. This will crash the lobby as soon as the game starts.");
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
		}
	}
}