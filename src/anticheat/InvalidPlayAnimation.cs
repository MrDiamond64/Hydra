namespace HydraMenu.anticheat
{
	internal class InvalidPlayAnimation : ICheck
	{
		public static void OnPlayAnimation(PlayerControl player, TaskTypes animation)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidPlayAnimation) return;

			if(LobbyBehaviour.Instance)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} inside the lobby.");
				Anticheat.Punish(player);
			}

			if(RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when they are an imposter.");
				Anticheat.Punish(player);
			}

			if(!GameManager.Instance.LogicOptions.GetVisualTasks())
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when visual tasks are off.");
				Anticheat.Punish(player);
			}

			bool hasTask = false;
			foreach(NetworkedPlayerInfo.TaskInfo task in player.Data.Tasks)
			{
				if(task.TypeId != (byte)animation) continue;

				hasTask = true;
				break;
			}

			// SetScanner RPC is sent upon player death, so we have to make sure the scanning value is set to true to avoid false positives
			if(!hasTask)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when they do not currently have that task.");
				Anticheat.Punish(player);
			}
		}
	}
}