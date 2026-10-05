using Hazel;
using HydraMenu.network;
using System.Collections.Generic;
using System.Linq;

namespace HydraMenu.modules.host
{
	internal class VoteImmune : Module
	{
		public VoteImmune() : base("VoteImmune") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		private void OnPlayerCastVote(NetworkedPlayerInfo voter, NetworkedPlayerInfo votee)
		{
			if(votee == null || !targets.Contains(votee.Object.GetHashCode())) return;

			Hydra.Log.LogMessage($"{voter.PlayerName} voted for a vote immune player, changing their vote to Skip");

			// Find the player that voted for the vote immune player, and make them change their vote to Skip
			// Democracy at its finest :P
			PlayerVoteArea voteArea = MeetingHud.Instance.playerStates.FirstOrDefault(area => area.PlayerId == voter.PlayerId);
			if(voteArea == null)
			{
				Hydra.Log.LogError($"Failed to find VoteState for {voter.PlayerName}");
				return;
			}

			voteArea.VotedForId = 253; // Skip

			// If we are not the host, and we are in a host-authoritative lobby
			// then make the host override their stored VoteStates
			if(!AmongUsClient.Instance.AmHost && !Utilities.IsAnticheatPresent())
			{
				Hydra.Log.LogMessage("Overriding the host's stored VoteState");

				MessageWriter writer = MessageWriter.Get(SendOption.None);
				writer.WritePacked(1); // VoteStates length
				writer.StartMessage(voter.PlayerId);
				voteArea.Serialize(writer);
				writer.EndMessage();

				BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);
				batch.QueueDataFlag(MeetingHud.Instance.NetId, writer);
				batch.FinishBatch();
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