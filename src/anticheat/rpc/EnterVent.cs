using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class EnterVent : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent when there is no instance of ShipStatus.");
				return false;
			}

			// Check if the player vents if their role does not support venting (if they are not engineer or non-ghost imposter)
			// We also want to make sure that the player is not dead to avoid false positives if the player vents as soon as they die
			// (Maybe we can store the time at which a player died and skip this check if an EnterVent RPC was sent within 500ms of dying?)
			if(!player.Data.IsDead && !player.Data.Role.CanVent)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent when their role ({player.Data.RoleType}) does not support venting.");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek() && RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent while being the seeker.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.EnterVent;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(PlayerPhysics);
		}
	}
}