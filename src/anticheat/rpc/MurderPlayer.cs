using Hazel;
using System;
using UnityEngine;

namespace HydraMenu.anticheat.rpc
{
	internal class MurderPlayer : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent MurderPlayer when there is no active ShipStatus.");
				return false;
			}

			uint targetNetId = reader.ReadPackedUInt32();
			MurderResultFlags result = (MurderResultFlags)reader.ReadInt32();

			// Impostor check: Only players with an impostor role should be executing murders
			if(!RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to murder a player when their role ({player.Data.RoleType}) is not an impostor.");
				return false;
			}

			// Alive check: Dead players should never be murdering
			if(player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to murder a player while dead.");
				return false;
			}

			// Locate target player to evaluate distance bounds
			PlayerControl victim = null;
			foreach(PlayerControl p in PlayerControl.AllPlayerControls)
			{
				if(p.NetId == targetNetId)
				{
					victim = p;
					break;
				}
			}

			if(victim != null && !player.Data.IsDead && !victim.Data.IsDead)
			{
				float distance = Vector2.Distance(player.transform.position, victim.transform.position);

				// Max legitimate kill distance in settings is ~2.5 plus network interpolation margin
				// Allow a generous threshold (10.0 units) to avoid false positives on high-ping lobbies;
				// anything beyond 10 units is an unequivocal cross-map kill exploit
				if(distance > 10.0f)
				{
					Anticheat.Flag(player, $"{player.Data.PlayerName} attempted a cross-map murder ({distance:F1} units from victim).");
					return false;
				}
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.MurderPlayer;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(PlayerControl);
		}
	}
}
