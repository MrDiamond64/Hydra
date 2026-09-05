using LunarMenu.modules;
using LunarMenu.network;
using UnityEngine;

namespace LunarMenu.routines
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

		private void OnDisconnect()
		{
			Lunar.notifications.Send("Glitter Bomb", "Glitter Bomb was disabled as you left the game.", 10);
			Enabled = false;
		}

		private void OnPlayerMurder(PlayerControl murder, PlayerControl target, MurderResultFlags result)
		{
			if(target != PlayerControl.LocalPlayer) return;

			Lunar.notifications.Send("Glitter Bomb", "Glitter Bomb was disabled as you have been killed.", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
			{
				Lunar.notifications.Send("Glitter Bomb", "You must be inside of a game in order for this feature to work.", 10);
				Enabled = false;
				return;
			}

			if(PlayerControl.LocalPlayer.Data.RoleType != AmongUs.GameOptions.RoleTypes.Phantom)
			{
				Lunar.notifications.Send("Glitter Bomb", "You must be Phantom in order for this feature to work.", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;
			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnDisconnect -= OnDisconnect;
			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
		}
	}
}