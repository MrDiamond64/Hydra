using LunarMenu.modules;
using System.Collections.Generic;
using UnityEngine;

namespace LunarMenu.routines
{
	public class TeleportSpammer : Routine
	{
		public TeleportSpammer() : base("TeleportSpammer") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		private readonly System.Random rnd = new System.Random();
		private readonly float TELEPORT_DELAY = 0.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < TELEPORT_DELAY) return;
			timeElapsed = 0f;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == PlayerControl.LocalPlayer || (!IsGlobal && !targets.Contains(player.GetHashCode()))) continue;

				int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);

				Teleporter.TeleportToVent(player, ventId);
			}
		}

		private bool IsGlobal
		{
			get { return targets.Count == 1 && targets.Contains(int.MaxValue); }
		}

		private void OnDisconnect()
		{
			Lunar.notifications.Send("Teleport Spammer", "Teleport Spammer was disabled as you left the game.", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Lunar.notifications.Send("Teleport Spammer", "Teleport Spammer can only be used once the game has started.", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			targets.Clear();

			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}