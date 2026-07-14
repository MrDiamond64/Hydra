using UnityEngine;

namespace HydraMenu.routines
{
	public class DoorTrollerRoutine : IRoutine
	{
		public DoorTrollerRoutine() : base("Door Troller") { }

		public float lockAndUnlockDelay = 0.5f;
		private float timeElapsed = 0f;
		private bool doorsLocked = false;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

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

		public override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Door Troller", "Door Troller can only be used if the game has started.", 10);
				Enabled = false;
				return;
			}

			if(ShipStatus.Instance.AllDoors.Count == 0)
			{
				Hydra.notifications.Send("Door Troller", "Door Troller can not be used as this map does not have any doors.", 10);
				Enabled = false;
				return;
			}

			if(!Sabotage.CanUnlockDoors())
			{
				Hydra.notifications.Send("Door Troller", "Door Troller can only be used if you are the host, or if the current map supports unlocking doors.", 10);
				Enabled = false;
				return;
			}
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Door Troller", "Door Troller was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}