using AmongUs.GameOptions;
using UnityEngine;

namespace HydraMenu.routines
{
	public class NoMeetingPolusRoutine : IRoutine
	{
		public NoMeetingPolusRoutine() : base("NoMeetingPolus") { }

		public float extraMargin = 0.0f;

		// Bounding area coordinates for Polus Office & Emergency Button:
		// Top-Right: (21.90, -15.58), Bottom-Right: (21.90, -18.01), Bottom-Left: (17.49, -18.08), Top-Left: (17.49, -15.84)
		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null) return;
			if(Utilities.GetCurrentMap() != MapNames.Polus) return;
			if(PlayerControl.AllPlayerControls == null) return;

			// Do not activate anti-meeting vent TP until emergency cooldown drops to 2.0s or lower
			if(ShipStatus.Instance.EmergencyCooldown > 2.0f) return;

			float minX = 17.49f - extraMargin;
			float maxX = 21.90f + extraMargin;
			float minY = -18.08f - extraMargin;
			float maxY = -15.58f + extraMargin;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == null || player.Data == null || player.Data.IsDead) continue;

				Vector2 pos = player.transform.position;
				if(pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY)
				{
					Teleporter.TeleportToVent(player, 0);
				}
			}
		}

		protected override void OnEnable()
		{
			if(Utilities.GetCurrentMap() != MapNames.Polus)
			{
				Hydra.notifications.Send("No Meeting", "No Meeting (Non-host) (Polus) only works on the Polus map.", 10);
				Enabled = false;
			}
		}

		public override void OnDisconnect()
		{
			Enabled = false;
		}
	}
}
