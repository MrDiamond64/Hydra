using AmongUs.GameOptions;
using UnityEngine;

namespace HydraMenu.routines
{
	public class NoMeetingFungleRoutine : IRoutine
	{
		public NoMeetingFungleRoutine() : base("NoMeetingFungle") { }

		public float extraMargin = 0.0f;

		// Bounding area coordinates for Fungle Emergency Meeting Area:
		// Top-Left: (-5.80, 0.45), Bottom-Left: (-5.80, -3.95), Bottom-Right: (-0.07, -3.95), Top-Right: (-0.07, 0.97)
		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null) return;
			if(Utilities.GetCurrentMap() != MapNames.Fungle) return;
			if(PlayerControl.AllPlayerControls == null) return;

			// Do not activate anti-meeting vent TP until emergency cooldown drops to 2.0s or lower
			if(ShipStatus.Instance.EmergencyCooldown > 2.0f) return;

			float minX = -5.80f - extraMargin;
			float maxX = -0.07f + extraMargin;
			float minY = -3.95f - extraMargin;
			float maxY = 0.97f + extraMargin;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == null || player.Data == null || player.Data.IsDead) continue;

				Vector2 pos = player.transform.position;
				if(pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY)
				{
					Teleporter.TeleportToVent(player, 5);
				}
			}
		}

		protected override void OnEnable()
		{
			if(Utilities.GetCurrentMap() != MapNames.Fungle)
			{
				Hydra.notifications.Send("No Meeting", "No Meeting (Non-host) (Fungle) only works on the Fungle map.", 10);
				Enabled = false;
			}
		}

		public override void OnDisconnect()
		{
			Enabled = false;
		}
	}
}
