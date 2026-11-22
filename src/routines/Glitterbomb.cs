using HydraMenu.modules;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.routines
{
	public class GlitterBomb : Routine
	{
		public GlitterBomb() : base("Glitterbomb") { }

		private readonly float PHANTOM_DELAY = 0.05f;
		private float timeElapsed = 0.0f;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < PHANTOM_DELAY) return;
			timeElapsed = 0.0f;

			PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());

			BatchedMessage batch = new BatchedMessage();
			batch.QueueAppear(PlayerControl.LocalPlayer);
			batch.QueueVanish(PlayerControl.LocalPlayer);
			batch.FinishBatch();
		}

		private void OnPlayerMurder(PlayerControl murder, PlayerControl target, MurderResultFlags result)
		{
			if(target != PlayerControl.LocalPlayer) return;

			Hydra.notifications.Send("Glitter Bomb", "Glitter Bomb was disabled as you have been killed.", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
			{
				Hydra.notifications.Send("Glitter Bomb", "You must be inside of a game in order for this feature to work.", 10);
				Enabled = false;
				return;
			}

			if(PlayerControl.LocalPlayer.Data.RoleType != AmongUs.GameOptions.RoleTypes.Phantom)
			{
				Hydra.notifications.Send("Glitter Bomb", "You must be Phantom in order for this feature to work.", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
		}
	}
}