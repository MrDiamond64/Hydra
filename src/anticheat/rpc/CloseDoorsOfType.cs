using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class CloseDoorsOfType : RpcCheck
	{
		// Player is null here as we cannot determine who sent this RPC
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// It would be nice if we could also add additional checks such as someone closing doors without being imposter
			// however we are not able to determine who sent the CloseDoorsOfType RPC
			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag("Someone attempted to close doors while in Hide and Seek.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CloseDoorsOfType;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(ShipStatus);
		}
	}
}