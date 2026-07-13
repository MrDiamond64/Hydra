using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.routines
{
	// This could be implemented as a postix patch of CustomNetworkTransform::Deserialize or as a routine
	// It took me a while, but I concluded that using a routine is a more elegant design choice
	public class JailPlayerRoutine : IRoutine
	{
		public JailPlayerRoutine() : base("JailPlayer") { }

		public HashSet<int> targets = new HashSet<int>();

		// For the sake of performance, only check if players are outside of the jail every 500ms
		public float delay = 0.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < delay) return;
			timeElapsed = 0f;

			GetMapData(out SystemTypes jailRoom, out int ventId);

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(!targets.Contains(player.GetHashCode())) continue;

				SystemTypes room = GetRoomForPlayer(player);
				if(room != jailRoom)
				{
					Teleporter.TeleportToVent(player, ventId);
				}
			}
		}

		// The RoomTracker::GetRoomForPlayer function returns a string with the player's current whereabouts
		// however it will throw an error if you are not the detective and the player not inside a room but rather near it
		private SystemTypes GetRoomForPlayer(PlayerControl player)
		{
			foreach(PlainShipRoom room in ShipStatus.Instance.AllRooms)
			{
				if(room.roomArea == null) continue;

				int collisions = room.roomArea.OverlapCollider(HudManager.Instance.roomTracker.filter, HudManager.Instance.roomTracker.detectiveBuffer);
				if(RoomTracker.CheckHitsForPlayer(HudManager.Instance.roomTracker.detectiveBuffer, collisions, player))
				{
					return room.RoomId;
				}
			}

			return (SystemTypes)255;
		}

		private void GetMapData(out SystemTypes room, out int ventId)
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			switch(currentMap)
			{
				case MapNames.Skeld:
				case MapNames.Dleks:
					room = SystemTypes.Nav;
					ventId = 12;
					break;

				case MapNames.MiraHQ:
					room = SystemTypes.Decontamination;
					ventId = 9;
					break;

				case MapNames.Polus:
					room = SystemTypes.Storage;
					ventId = 8;
					break;

				case MapNames.Airship:
					room = SystemTypes.GapRoom;
					ventId = 7;
					break;

				case MapNames.Fungle:
					room = SystemTypes.Laboratory;
					ventId = 4;
					break;

				// Default to the Skeld values
				default:
					room = SystemTypes.Nav;
					ventId = 12;
					break;
			}
		}

		public override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Jail Player", "Jail Player can only be used inside of a game.", 10);
				Enabled = false;
				return;
			}
		}

		public override void OnDisable()
		{
			targets.Clear();
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Jail Player", "Jail Player has been disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}