using AmongUs.GameOptions;
using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class Shapeshift : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent Shapeshift when there is no active ShipStatus.");
				return false;
			}

			// Validate role capability: only Shapeshifters can execute shapeshifting
			if(player.Data.RoleType != RoleTypes.Shapeshifter)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to shapeshift when their role ({player.Data.RoleType}) is not a Shapeshifter.");
				return false;
			}

			// Dead players cannot shapeshift
			if(player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to shapeshift while dead.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.Shapeshift;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(PlayerControl);
		}
	}
}
