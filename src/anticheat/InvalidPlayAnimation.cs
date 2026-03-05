using Hazel;

namespace HydraMenu.anticheat
{
	internal class InvalidPlayAnimation : ICheck
	{
		public static void OnPlayAnimation(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidPlayAnimation) return;

			TaskTypes animation = (TaskTypes)reader.ReadByte();

			if(LobbyBehaviour.Instance)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} inside the lobby.");
				blockRpc = true;
			}

			if(RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when they are an imposter.");
				blockRpc = true;
			}

			if(!GameManager.Instance.LogicOptions.GetVisualTasks())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when visual tasks are off.");
				blockRpc = true;
			}
		}
	}
}