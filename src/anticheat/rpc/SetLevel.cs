using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetLevel : RpcCheck
	{
		public readonly uint MAX_PLAYER_LEVEL = 10000;

		// We should not block SetLevel RPCs
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			uint level = reader.ReadPackedUInt32();

			// The vanilla Among Us anticheat bans players if they send a SetLevel RPC with a lever greater than 100k
			// This is rather generous, we just check if the requested player level is greater than 10k
			if(level > MAX_PLAYER_LEVEL)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC with a level that is too high ({level}).");
				player.SetLevel(MAX_PLAYER_LEVEL);
				return false;
			}

			// The SetLevel RPC should only be sent when a player joins the game in the lobby
			if(ShipStatus.Instance)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC when the game has already started.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SetLevel;
		}
	}
}