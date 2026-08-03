using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class ReportDeadBody : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to call a meeting in Hide and Seek");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.ReportDeadBody;
		}
	}
}