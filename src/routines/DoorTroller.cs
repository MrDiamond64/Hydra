using UnityEngine;

namespace HydraMenu.routines
{
	public class DoorTrollerRoutine : IRoutine
	{
		public DoorTrollerRoutine()
		{
			this.routineName = "DoorTroller";
		}

		public float lockAndUnlockDelay = 0.5f;
		private float timeElapsed = 0f;
		private bool doorsLocked = false;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Door Troller", "Door troller has been disabled as you either left the game or the current map does not support unlocking doors.", 5);

				this.Enabled = false;
				return;
			}

			if(ShipStatus.Instance.AllDoors.Count == 0)
			{
				Hydra.notifications.Send("Door Troller", "Door troller was disabled as this map does not have any doors.", 5);

				this.Enabled = false;
				return;
			}

			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Door Troller", "Door troller can only work if you are the host, or if the current map supports unlocking doors.", 5);

				this.Enabled = false;
				return;
			}

			timeElapsed += Time.deltaTime;
			if(timeElapsed < lockAndUnlockDelay) return;

			if(doorsLocked)
			{
				Sabotage.UnlockAll();
			}
			else
			{
				Sabotage.LockAll();
			}

			doorsLocked = !doorsLocked;
			timeElapsed = 0;
		}
	}
}