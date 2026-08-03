using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class CheckName : RpcCheck
	{
		public readonly int MAX_NAME_LENGTH = 10;

		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			string requestedName = reader.ReadString();

			/*
			ClientData clientData = AmongUsClient.Instance.GetClient(player.OwnerId);
			if(AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame && requestedName != clientData.PlayerName)
			{
				Anticheat.Flag(player, $"{clientData.PlayerName} requested a name that does not match their name in the login handshake.");
				player.SetName(clientData.PlayerName);
				return false;
			}
			*/

			if(requestedName.Length > MAX_NAME_LENGTH)
			{
				Anticheat.Flag(player, $"{requestedName} tried setting their name to something too long ({requestedName.Length}).");
				return false;
			}

			if(requestedName.Contains('<'))
			{
				Anticheat.Flag(player, $"{requestedName} requested a name with invalid characters.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CheckName;
		}
	}
}