using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;

namespace HydraMenu.anticheat.gamedata
{
	internal class ClientReady : GameDataCheck
	{
		public override bool Validate(MessageReader reader)
		{
			int clientId = reader.ReadPackedInt32();

			ClientData client = AmongUsClient.Instance.FindClientById(clientId);
			if(client == null)
			{
				Anticheat.Flag($"Received ClientReady message for unknown client: {clientId}.");
				return false;
			}

			if(client.IsReady)
			{
				Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} sent a ClientReady message while already ready.");
				return false;
			}

			return true;
		}

		public override GameDataTypes GetGameDataType()
		{
			return GameDataTypes.ReadyFlag;
		}
	}
}