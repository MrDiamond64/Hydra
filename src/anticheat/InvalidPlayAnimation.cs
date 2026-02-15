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
		}
	}
}