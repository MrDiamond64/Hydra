using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class UsePlatform : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			MapNames map = Utilities.GetCurrentMap();
			if(map != MapNames.Airship)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform outside of the proper map.");
				return false;
			}

			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform when there is no instance of ShipStatus.");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform while in Hide and Seek.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.UsePlatform;
		}
	}
}