using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;

namespace HydraMenu.anticheat.gamedata
{
	internal class SceneChange : GameDataCheck
	{
		public override bool Validate(MessageReader reader)
		{
			int clientId = reader.ReadPackedInt32();
			string scene = reader.ReadString();

			ClientData client = AmongUsClient.Instance.FindClientById(clientId);
			if(client == null)
			{
				Anticheat.Flag($"Received SceneChange message for unknown client: {clientId}.");
				return false;
			}

			// If the host receives a scene change of Tutorial, it will spawn in an instance of The Skeld map
			if(scene == "Tutorial")
			{
				Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} sent a scene change of Tutorial.");
				return false;
			}

			return true;
		}

		public override GameDataTypes GetGameDataType()
		{
			return GameDataTypes.SceneChangeFlag;
		}
	}
}