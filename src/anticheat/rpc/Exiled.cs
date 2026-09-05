using Hazel;

namespace LunarMenu.anticheat.rpc
{
	internal class Exiled : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// The Exiled RPC is unused and is never sent in-game
			Anticheat.Flag(player, $"{player.Data.PlayerName} sent an invalid Exiled RPC.");
			return false;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.Exiled;
		}
	}
}