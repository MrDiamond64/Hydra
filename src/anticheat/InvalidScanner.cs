using Hazel;

namespace HydraMenu.anticheat
{
	internal class InvalidScanner : ICheck
	{
		public static void OnSetScanner(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidScan) return;

			bool scanning = reader.ReadBoolean();
			// byte seqId = reader.ReadByte();

			// The medbay scan task can only be done if the map itself exists
			// If the game has not started yet (which the vanilla anticheat should already check), or if we're in the lobby to actual game transition, or the map was despawned,
			// then we know that SetScanner RPC was sent illegitimately
			if(ShipStatus.Instance == null && scanning)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the SetScanner RPC while the map has not spawned in yet.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			// When a player gets killed, a SetScanner RPC with the scanning value sent to false is sent
			// This applies to Imposters too if they were to somehow die, so we need to account for this false flag
			if(RoleManager.IsImpostorRole(player.Data.RoleType) && scanning)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the SetScanner RPC when they are an imposter {scanning}.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			if(!GameManager.Instance.LogicOptions.GetVisualTasks())
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the SetScanner RPC while visual tasks were disabled.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			bool hasMedbayScanTask = false;
			foreach(NetworkedPlayerInfo.TaskInfo task in player.Data.Tasks)
			{
				if(task.TypeId != (byte)TaskTypes.SubmitScan) continue;

				hasMedbayScanTask = true;
				break;
			}

			// SetScanner RPC is sent upon player death, so we have to make sure the scanning value is set to true to avoid false positives
			if(!hasMedbayScanTask && scanning)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the SetScanner RPC without being assigned the medbay scan task.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}
	}
}