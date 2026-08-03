using Hazel;
using System;
using UnityEngine;

namespace HydraMenu.anticheat.rpc
{
	internal class SnapTo : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// Vector2 position = NetHelpers.ReadVector2(reader);
			// ushort seqId = reader.ReadUInt16();

			if(LobbyBehaviour.Instance != null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SnapTo RPC while inside the lobby.");

				// We are not able to send SnapTo RPCs with other player's NetTransform net ids on Vanilla servers
				if(AmongUsClient.Instance.AmHost && !Utilities.IsAnticheatPresent())
				{
					player.NetTransform.RpcSnapTo(player.transform.position);
				}

				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SnapTo;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(CustomNetworkTransform);
		}
	}
}