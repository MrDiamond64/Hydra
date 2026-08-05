using Hazel;
using HydraMenu.features;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.routines
{
	public class NoMeetingSkeldRoutine : IRoutine
	{
		public NoMeetingSkeldRoutine() : base("NoMeetingSkeld") { }

		// Default margin set to 0.0
		public float extraMargin = 0.0f;

		// Right Passage Exclusion Polygon (Intersecting area is allowed)
		private static readonly Vector2[] rightPassage = new Vector2[]
		{
			new Vector2(4.59f, 0.62f),
			new Vector2(3.00f, 5.05f),
			new Vector2(5.68f, 7.09f),
			new Vector2(7.63f, 0.84f)
		};

		// Left Passage Exclusion Polygon (Intersecting area is allowed)
		private static readonly Vector2[] leftPassage = new Vector2[]
		{
			new Vector2(-6.07f, 0.63f),
			new Vector2(-4.48f, 5.00f),
			new Vector2(-7.80f, 5.85f),
			new Vector2(-8.15f, 0.43f)
		};

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null) return;
			if(Utilities.GetCurrentMap() != MapNames.Skeld) return;
			if(PlayerControl.AllPlayerControls == null) return;

			// Do not activate anti-meeting vent TP until emergency cooldown drops to 2.0s or lower
			if(ShipStatus.Instance.EmergencyCooldown > 2.0f) return;

			float minX = -4.26f - extraMargin;
			float maxX = 2.57f + extraMargin;
			float minY = -1.48f - extraMargin;
			float maxY = 3.91f + extraMargin;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == null || player.Data == null || player.Data.IsDead) continue;

				Vector2 pos = player.transform.position;
				bool inMeetingZone = (pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY);
				if(!inMeetingZone) continue;

				bool inAllowedRight = IsPointInPolygon(pos, rightPassage);
				bool inAllowedLeft = IsPointInPolygon(pos, leftPassage);

				// Only boot player if inside meeting zone AND NOT inside allowed passage intersections
				if(!inAllowedRight && !inAllowedLeft)
				{
					Teleporter.TeleportToVent(player, 0);
				}
			}
		}

		private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
		{
			bool inside = false;
			int j = polygon.Length - 1;
			for(int i = 0; i < polygon.Length; i++)
			{
				if((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
				   (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
				{
					inside = !inside;
				}
				j = i;
			}
			return inside;
		}

		protected override void OnEnable()
		{
			if(Utilities.GetCurrentMap() != MapNames.Skeld)
			{
				Hydra.notifications.Send("No Meeting", "No Meeting (Non-host) (The Skeld) only works on The Skeld map.", 10);
				Enabled = false;
			}
		}

		public override void OnDisconnect()
		{
			Enabled = false;
		}
	}
}
