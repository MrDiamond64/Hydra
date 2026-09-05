using LunarMenu.modules;
using LunarMenu.network;
using System.Collections.Generic;
using UnityEngine;

namespace LunarMenu.routines
{
	internal class VoteSpammer : Routine
	{
		public VoteSpammer() : base("VoteSpammer") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		public readonly float VOTE_DELAY = 1.0f;
		private float timeElapsed = 0.0f;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < VOTE_DELAY) return;
			timeElapsed = 0.0f;

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

				batch.QueueSendChatNote(PlayerControl.LocalPlayer, player.PlayerId, ChatNoteTypes.DidVote);
			}

			batch.FinishBatch();
		}

		private bool IsGlobal
		{
			get { return targets.Count == 1 && targets.Contains(int.MaxValue); }
		}

		private void OnMeetingEnd()
		{
			Lunar.notifications.Send("Vote Spammer", "Vote Spammer was disabled as the current meeting has ended.");
			Enabled = false;
		}

		private void OnDisconnect()
		{
			Lunar.notifications.Send("Vote Spammer", "Vote Spammer was disabled as you left the game.");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(MeetingHud.Instance == null)
			{
				Lunar.notifications.Send("Vote Spammer", "There must be an active meeting for this feature to work.");
				Enabled = false;
				return;
			}

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Lunar.notifications.Send("Vote Spammer", "Vote Spammer can only be used if you are the host of the lobby.");
				Enabled = false;
				return;
			}

			EventCoordinator.OnMeetingEnd += OnMeetingEnd;
			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			targets.Clear();

			EventCoordinator.OnMeetingEnd -= OnMeetingEnd;
			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}