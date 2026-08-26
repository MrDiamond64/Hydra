namespace HydraMenu.modules.troll
{
	internal class AutoReportBodies : Module
	{
		public AutoReportBodies() : base("AutoReportBodies") { }

		// The player that should be framed for reporting the body
		public PlayerControl target;

		private void OnPlayerMurder(PlayerControl murderer, PlayerControl victim, MurderResultFlags flags)
		{
			if(!flags.HasFlag(MurderResultFlags.Succeeded)) return;

			if(AmongUsClient.Instance.AmHost)
			{
				Utilities.OpenMeeting(target ?? PlayerControl.LocalPlayer, victim.Data);
				return;
			}

			if(PlayerControl.LocalPlayer.Data.IsDead) return;

			Hydra.notifications.Send("Auto Report Bodies", $"{victim.Data.PlayerName} was killed by {murderer.Data.PlayerName} ({Utilities.GetPlayerColor(murderer.Data)}), their body has been automatically reported.");
			PlayerControl.LocalPlayer.CmdReportDeadBody(victim.Data);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
		}
	}
}