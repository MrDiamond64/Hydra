using HydraMenu.network;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.routines
{
	public class DiscoHostRoutine : IRoutine
	{
		public DiscoHostRoutine() : base("DiscoHost") { }
		public HashSet<int> targets = new HashSet<int>();

		public float randomizationDelay = 0.5f;
		private float timeElapsed = 0f;

		private System.Random rnd = new System.Random();

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < randomizationDelay) return;
			timeElapsed = 0f;

			List<int> colors = Enumerable.Range(0, 18).ToList();

			BatchedMessage batch = new BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(!IsGlobal && !targets.Contains(player.GetHashCode())) continue;

				// Assign each player a unique color
				int color;
				if(colors.Count != 0)
				{
					color = colors[rnd.Next(0, colors.Count)];
					colors.Remove(color);
				}
				else
				{
					// To ensure compatability for lobbies with more than 18 players
					color = rnd.Next(0, 18);
				}

				batch.QueueSetColor(player, (byte)color);
			}

			batch.FinishBatch();
		}

		public bool IsGlobal
		{
			get { return targets.Count == 1 && targets.Contains(int.MaxValue); }
		}

		public override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("Disco Party", "Disco Party can only be used inside of a game.", 10);
				Enabled = false;
				return;
			}

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Disco Party", "Disco Party can only be used if you are the host of the lobby.", 10);
				Enabled = false;
				return;
			}
		}

		public override void OnDisable()
		{
			targets.Clear();
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Disco Party", "Disco Party was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}