using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class ExitVent : RpcCheck
	{
		// Sending ExitVent RPCs can be used to make the player teleport to areas without having to send SnapTo RPCs
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent when there is no instance of ShipStatus.");
				return false;
			}

			if(!player.Data.IsDead && !player.Data.Role.CanVent)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent when their role ({player.Data.RoleType}) does not support venting.");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek() && RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent while being the seeker.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.ExitVent;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(PlayerPhysics);
		}
	}
}