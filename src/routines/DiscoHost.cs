using UnityEngine;

namespace HydraMenu.routines
{
	public class DiscoHostRoutine : IRoutine
	{
		public DiscoHostRoutine()
		{
			name = "DiscoHost";
		}

		public float randomizationDelay = 0.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < randomizationDelay) return;

			System.Random rnd = new System.Random();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				player.RpcSetColor((byte)rnd.Next(0, 17));
			}

			timeElapsed = 0f;
		}

		public override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("Disco Party", "Disco Party can only be used inside of a game.", 10);
				Enabled = false;
				return;
			}

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Disco Party", "Disco Party can only be used if you are the host of the lobby.", 10);
				Enabled = false;
				return;
			}
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Disco Party", "Disco Party was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}