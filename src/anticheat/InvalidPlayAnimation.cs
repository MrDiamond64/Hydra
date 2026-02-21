namespace HydraMenu.anticheat
{
	internal class InvalidPlayAnimation : ICheck
	{
		public static void OnPlayAnimation(PlayerControl player, TaskTypes animation, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidPlayAnimation) return;

			if(LobbyBehaviour.Instance)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} inside the lobby.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			if(RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when they are an imposter.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			if(!GameManager.Instance.LogicOptions.GetVisualTasks())
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when visual tasks are off.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}
	}
}