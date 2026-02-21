namespace HydraMenu.anticheat
{
	internal class InvalidCompleteTask : ICheck
	{
		public static void OnCompleteTask(PlayerControl player, uint taskIndex, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidCompleteTask) return;

			// If there is no instance of ShipStatus (such as if the game has not started yet or the map was despawned), then it is not possible to complete tasks (
			// Technically we don't need this to detect if someone completes a task in the lobby, as the task ID being greater than the total amount of tasks check should detect it
			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried completing task {taskIndex} when there was no valid instance of ShipStatus.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			if(RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried completing task {taskIndex} while being an imposter.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			// Task IDs are zero-indexed
			if((taskIndex + 1) > player.Data.Tasks.Count)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried completing task {taskIndex} when they only have {player.Data.Tasks.Count} tasks.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}
	}
}