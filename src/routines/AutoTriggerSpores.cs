using UnityEngine;

namespace HydraMenu.routines
{
	public class AutoTriggerSporesRoutine : IRoutine
	{
		public AutoTriggerSporesRoutine() : base("AutoTriggerSpores") { }

		public readonly float SPORE_TRIGGER_LENGTH = 5.0f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < SPORE_TRIGGER_LENGTH) return;
			timeElapsed = 0f;

			FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();
			foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
			{
				PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
			}
		}

		public override void OnEnable()
		{
			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Trigger Spores", "Auto-Trigger Spores can only be used if the game has started.", 10);
				Enabled = false;
				return;
			}

			if(Utilities.GetCurrentMap() != MapNames.Fungle)
			{
				Hydra.notifications.Send("Trigger Spores", "Auto-Trigger Spores can only be used in The Fungle.", 10);
				Enabled = false;
				return;
			}
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Trigger Spores", "Auto-Trigger Spores was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}