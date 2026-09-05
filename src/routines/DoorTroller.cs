using LunarMenu.modules;
using UnityEngine;

namespace LunarMenu.routines
{
	public class DoorTrollerRoutine : Routine
	{
		public DoorTrollerRoutine() : base("DoorTroller") { }

		public float LockAndUnlockDelay { get; set; } = 0.5f;
		private float timeElapsed = 0f;
		private bool doorsLocked = false;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < LockAndUnlockDelay) return;

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

		private void OnDisconnect()
		{
			Lunar.notifications.Send("Door Troller", "Door Troller was disabled as you left the game.", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Lunar.notifications.Send("Door Troller", "Door Troller can only be used if the game has started.", 10);
				Enabled = false;
				return;
			}

			if(ShipStatus.Instance.AllDoors.Count == 0)
			{
				Lunar.notifications.Send("Door Troller", "Door Troller can not be used as this map does not have any doors.", 10);
				Enabled = false;
				return;
			}

			if(!Sabotage.CanUnlockDoors())
			{
				Lunar.notifications.Send("Door Troller", "Door Troller can only be used if you are the host, or if the current map supports unlocking doors.", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}