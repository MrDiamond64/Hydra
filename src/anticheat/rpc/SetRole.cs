using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class SetRole : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// In Among Us, SetRole is exclusively a host-authoritative RPC
			// Any client attempting to dispatch SetRole is attempting to force self-promotion or reassign player roles
			Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to set or modify player roles as non-host.");
			return false;
		}

		public override bool IsHostOnly()
		{
			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.SetRole;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(PlayerControl);
		}
	}
}
