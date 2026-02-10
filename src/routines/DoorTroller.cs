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
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null || !Sabotage.CanUnlockDoors())
			{
				this.Enabled = false;
				Hydra.notifications.Send("Door Troller", "Door troller has been disabled as you either left the game or the current map does not support unlocking doors.", 5);

				return;
			}

			timeElapsed += Time.deltaTime;
			if(timeElapsed < lockAndUnlockDelay) return;

			if(doorsLocked)
			{
				Sabotage.UnlockAll();
			} else
			{
				Sabotage.LockAll();
			}

			doorsLocked = !doorsLocked;
			timeElapsed = 0;
		}
	}
}