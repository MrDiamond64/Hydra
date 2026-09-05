using LunarMenu.modules;
using LunarMenu.network;
using UnityEngine;

namespace LunarMenu.routines
{
	public class AutoTriggerSporesRoutine : Routine
	{
		public AutoTriggerSporesRoutine() : base("AutoTriggerSpores") { }

		public readonly float SPORE_TRIGGER_DURATION = 5.0f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < SPORE_TRIGGER_DURATION) return;
			timeElapsed = 0f;

			FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

			BatchedMessage batch = new BatchedMessage();

			foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
			{
				batch.QueueTriggerSpore(PlayerControl.LocalPlayer, mushroom);
			}

			batch.FinishBatch();
		}

		private void OnDisconnect()
		{
			Lunar.notifications.Send("Trigger Spores", "Auto-Trigger Spores was disabled as you left the game.", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(ShipStatus.Instance == null)
			{
				Lunar.notifications.Send("Trigger Spores", "Auto-Trigger Spores can only be used if the game has started.", 10);
				Enabled = false;
				return;
			}

			if(Utilities.GetCurrentMap() != MapNames.Fungle)
			{
				Lunar.notifications.Send("Trigger Spores", "Auto-Trigger Spores can only be used in The Fungle.", 10);
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