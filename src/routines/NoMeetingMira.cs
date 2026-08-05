using AmongUs.GameOptions;
using UnityEngine;

namespace HydraMenu.routines
{
	public class NoMeetingMiraRoutine : IRoutine
	{
		public NoMeetingMiraRoutine() : base("NoMeetingMira") { }

		public float extraMargin = 0.0f;

		// Bounding area coordinates for MiraHQ Cafeteria & Meeting Table:
		// Top Left: (21.35, 5.56), Top Right: (28.97, 5.46), Down Right: (29.07, -0.27), Down Left: (20.46, -0.27)
		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null) return;
			if(Utilities.GetCurrentMap() != MapNames.MiraHQ) return;
			if(PlayerControl.AllPlayerControls == null) return;

			// Do not activate anti-meeting vent TP until emergency cooldown drops to 2.0s or lower
			if(ShipStatus.Instance.EmergencyCooldown > 2.0f) return;

			// Bounding area coordinates for MiraHQ Cafeteria & Meeting Table:
			// Bottom-Left: (21.95, -0.05), Top-Left: (21.93, 4.26), Top-Right: (25.92, 4.26), Bottom-Right: (25.82, 0.09)
			float minX = 21.93f - extraMargin;
			float maxX = 25.92f + extraMargin;
			float minY = -0.05f - extraMargin;
			float maxY = 4.26f + extraMargin;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == null || player.Data == null || player.Data.IsDead) continue;

				Vector2 pos = player.transform.position;
				if(pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY)
				{
					Teleporter.TeleportToVent(player, 3);
				}
			}
		}

		protected override void OnEnable()
		{
			if(Utilities.GetCurrentMap() != MapNames.MiraHQ)
			{
				Hydra.notifications.Send("No Meeting", "No Meeting (Non-host) (MiraHQ) only works on the MiraHQ map.", 10);
				Enabled = false;
			}
		}

		public override void OnDisconnect()
		{
			Enabled = false;
		}
	}
}
