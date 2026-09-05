using System.Collections.Generic;

namespace LunarMenu.modules.host
{
	internal class VoteImmune : Module
	{
		public VoteImmune() : base("VoteImmune") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		private void OnPlayerCastVote(NetworkedPlayerInfo voter, NetworkedPlayerInfo votee)
		{
			if(!targets.Contains(votee.Object.GetHashCode())) return;

			Lunar.Log.LogMessage($"{voter.PlayerName} voted for a vote immune player, changing their vote to Skip");

			// Find the player that voted for the vote immune player, and make them change their vote to Skip
			// Democracy at its finest :P
			foreach(PlayerVoteArea voteArea in MeetingHud.Instance.playerStates)
			{
				if(voteArea.PlayerId != voter.PlayerId) continue;

				voteArea.VotedForId = 253; // Skip
			}
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerCastVote += OnPlayerCastVote;
		}

		protected override void OnDisable()
		{
			targets.Clear();

			EventCoordinator.OnPlayerCastVote -= OnPlayerCastVote;
		}
	}
}