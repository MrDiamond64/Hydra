using UnityEngine;

namespace HydraMenu.routines
{
	public class ReportBodySpam : IRoutine
	{
		public ReportBodySpam() : base("ReportBodySpam") { }

		public float reportDelay = 2.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < reportDelay) return;
			timeElapsed = 0f;

			PlayerControl player = Utilities.GetRandomPlayer(false, false, false, false);

			if(MeetingHud.Instance == null)
			{
				Utilities.OpenMeeting(PlayerControl.LocalPlayer, player.Data);
			}

			PlayerControl.LocalPlayer.RpcStartMeeting(player.Data);
		}

		public override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Report Body Spam", "Report Body Spam can only be used once the game has started.", 10);
				Enabled = false;
				return;
			}

			if(!AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Report Body Spam", "Report Body Spam can only be used if you are the host of the lobby.", 10);
				Enabled = false;
				return;
			}
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Report Body Spam", "Report Body Spam was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}