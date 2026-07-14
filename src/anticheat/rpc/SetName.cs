using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetName : RpcCheck
	{
		// We increase the max name length to 12 instead of 10 as we need to account for the numbers at the end of player names when there are multiple players with the same name
		public readonly int MAX_NAME_LENGTH = 12;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			// On modded lobbies, it is common for players to have custom names
			if(!Utilities.IsAnticheatPresent()) return;

			uint netId = reader.ReadUInt32();
			string requestedName = reader.ReadString();

			if(netId != GetExpectedNetId(player))
			{
				blockRpc = true;
				Anticheat.Flag(player, $"SetName RPC sent for {requestedName} includes an invalid net id, received {netId}, expected {GetExpectedNetId(player)}.");
			}

			if(requestedName.Length > MAX_NAME_LENGTH)
			{
				blockRpc = true;
				Anticheat.Flag(player, $"{requestedName} tried setting their name to something too long ({requestedName.Length}).");
			}

			if(requestedName.Contains('<'))
			{
				blockRpc = true;
				Anticheat.Flag(player, $"{requestedName} requested a name with invalid characters.");
			}
		}

		private uint GetExpectedNetId(PlayerControl player)
		{
			// In host authority, the host sends the SetName RPC with the net id of the player's NetworkedPlayerInfo net object
			// In server authority, the server sends the SetName RPC with the net id of the player's PlayerControl net object
			// I don't know why this discrepancy exists, or why the SetName RPC even includes the net id field
			return Utilities.IsAnticheatPresent() ? player.NetId : player.Data.NetId;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SetName;
		}

		// On vanilla servers, the host is able to receive the SetName RPC due to how server-authority works
		// On modded servers, the host should never receive the SetName RPC
		public override bool IsHostOnly()
		{
			return !Utilities.IsAnticheatPresent();
		}
	}
}
