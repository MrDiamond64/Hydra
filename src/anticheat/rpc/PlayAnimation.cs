using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class PlayAnimation : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			TaskTypes animation = (TaskTypes)reader.ReadByte();

			if(LobbyBehaviour.Instance)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} inside the lobby.");
				return false;
			}

			if(RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when they are an imposter.");
				return false;
			}

			if(!GameManager.Instance.LogicOptions.GetVisualTasks())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when visual tasks are off.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.PlayAnimation;
		}
	}
}