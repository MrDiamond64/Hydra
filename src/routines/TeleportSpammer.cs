using UnityEngine;

namespace HydraMenu.routines
{
	public class TeleportSpammer : IRoutine
	{
		public TeleportSpammer() : base("TeleportSpammer") { }

		private System.Random rnd = new System.Random();
		private float teleportDelay = 0.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < teleportDelay) return;
			timeElapsed = 0f;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == PlayerControl.LocalPlayer) continue;

				int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);

				Teleporter.TeleportToVent(player, ventId);
			}
		}

		public override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Teleport Spammer", "Teleport Spammer can only be used once the game has started.", 10);
				Enabled = false;
				return;
			}
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Teleport Spammerm", "Teleport Spammer was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}