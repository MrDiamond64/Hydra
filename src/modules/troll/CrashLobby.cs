namespace LunarMenu.modules.troll
{
	internal class CrashLobby : Module
	{
		public CrashLobby() : base("CrashLobby") { }

		private void OnGameStart()
		{
			PlayerControl.LocalPlayer.CmdReportDeadBody(null);
			Lunar.notifications.Send("Lobby Crasher", "The lobby has been crashed.");
		}

		protected override void OnEnable()
		{
			Lunar.notifications.Send("Lobby Crasher", "Crash Lobby has been enabled. This will crash the lobby as soon as the game starts.");

			EventCoordinator.OnGameStart += OnGameStart;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
		}
	}
}