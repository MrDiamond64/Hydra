using AmongUs.GameOptions;
using UnityEngine;

namespace HydraMenu.routines
{
	public class NoMeetingAirshipRoutine : IRoutine
	{
		public NoMeetingAirshipRoutine() : base("NoMeetingAirship") { }

		public float extraMargin = 0.0f;

		// Bounding area coordinates for Airship Meeting Room & Button:
		// Bottom-Right: (8.97, 13.38), Top-Right: (8.15, 17.22), Top-Left: (14.45, 17.49), Bottom-Left: (14.37, 13.74)
		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null) return;
			if(Utilities.GetCurrentMap() != MapNames.Airship) return;
			if(PlayerControl.AllPlayerControls == null) return;

			// Do not activate anti-meeting vent TP until emergency cooldown drops to 2.0s or lower
			if(ShipStatus.Instance.EmergencyCooldown > 2.0f) return;

			float minX = 8.15f - extraMargin;
			float maxX = 14.45f + extraMargin;
			float minY = 13.38f - extraMargin;
			float maxY = 17.49f + extraMargin;

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
			if(Utilities.GetCurrentMap() != MapNames.Airship)
			{
				Hydra.notifications.Send("No Meeting", "No Meeting (Non-host) (Airship) only works on the Airship map.", 10);
				Enabled = false;
			}
		}

		public override void OnDisconnect()
		{
			Enabled = false;
		}
	}
}
