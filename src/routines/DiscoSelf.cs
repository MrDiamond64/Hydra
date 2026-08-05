using UnityEngine;

namespace HydraMenu.routines
{
	public class DiscoSelfRoutine : IRoutine
	{
		public DiscoSelfRoutine() : base("DiscoSelf") { }

		public float delay = 0.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < delay) return;
			timeElapsed = 0f;

			PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("Disco Mode", "Disco Mode can only be used inside of a game.", 10);
				Enabled = false;
			}
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Disco Mode", "Disco Mode was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}
