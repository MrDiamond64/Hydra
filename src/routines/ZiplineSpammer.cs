using LunarMenu.modules;
using LunarMenu.network;
using System.Collections.Generic;
using UnityEngine;

namespace LunarMenu.routines
{
	internal class ZiplineSpammer : Routine
	{
		public ZiplineSpammer() : base("ZiplineSpammer") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		private readonly System.Random rnd = new System.Random();
		public readonly float ZIPLINE_DELAY = 5.0f;
		private float timeElapsed = 0.0f;
		private FungleShipStatus shipStatus = null;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < ZIPLINE_DELAY) return;
			timeElapsed = 0.0f;

			if(shipStatus == null)
			{
				return;
			}

			int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();
			BatchedMessage batch = new BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(!IsGlobal && !targets.Contains(player.GetHashCode())) continue;

				if(batch.msgCount >= packingLimit)
				{
					batch.FinishBatch();
					batch = new BatchedMessage();
				}

				bool fromTop = rnd.Next(0, 2) == 0;
				batch.QueueUseZipline(player, shipStatus.Zipline, fromTop);
			}

			batch.FinishBatch();
		}

		private bool IsGlobal
		{
			get { return targets.Count == 1 && targets.Contains(int.MaxValue); }
		}

		private void OnDisconnect()
		{
			Lunar.notifications.Send("Zipline Spammer", "Zipline Spammer was disabled as you left the game.", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Lunar.notifications.Send("Zipline Spammer", "Zipline Spammer can only be used once the game has started.", 10);
				Enabled = false;
				return;
			}

			if(Utilities.GetCurrentMap() != MapNames.Fungle)
			{
				Lunar.notifications.Send("Zipline Spammer", "Zipline Spammer can only be used on The Fungle.", 10);
				Enabled = false;
				return;
			}

			shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			targets.Clear();

			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}