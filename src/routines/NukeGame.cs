using UnityEngine;

namespace HydraMenu.routines
{
	public class NukeGameRoutine : IRoutine
	{
		public NukeGameRoutine() : base("NukeGame") { }

		public float clickDelay = 0.15f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;
			if(PlayerControl.LocalPlayer.Data.IsDead) return;
			if(MeetingHud.Instance != null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < clickDelay) return;
			timeElapsed = 0f;

			Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, null, true);
		}

		public override void OnDisconnect()
		{
			Enabled = false;
		}
	}
}
