using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class ReportDeadBody : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to call a meeting in Hide and Seek");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.ReportDeadBody;
		}
	}
}