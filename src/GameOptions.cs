using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;

namespace HydraMenu
{
	internal class GameOptions
	{
		// If we want to freely modify IGameOptions without it applying to ourselves, we will need to clone it
		// There might be a better way of doing this, but I just serialize the game options into a byte array and serialize it back into IGameOptions
		// which gives us a new instance of IGameOptions based off our pre-existing options
		public static IGameOptions CreateCloneOptions(IGameOptions options)
		{
			LogicOptions logicOptions = GameManager.Instance.LogicOptions;

			byte[] byteArray = logicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn);
			return logicOptions.gameOptionsFactory.FromBytes(byteArray);
		}

		// Only send the game options update to one specific player
		public static void SendGameOptionsToClient(IGameOptions options, int targetClientId)
		{
			// We have the manually apply game options in Freeplay as there is no networking layer there
			// Freeplay has some settings that cannot be changed, such as player vision, so the Blind Player feature wont work there
			if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay && targetClientId == PlayerControl.LocalPlayer.OwnerId)
			{
				GameManager.Instance.LogicOptions.SetGameOptions(options);
				return;
			}

			MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
			writer.StartMessage(InnerNet.Tags.GameDataTo);
			writer.Write(AmongUsClient.Instance.GameId);
			writer.WritePacked(targetClientId);

			writer.StartMessage((byte)GameDataTypes.DataFlag);
			writer.WritePacked(GameManager.Instance.NetId);

			writer.StartMessage((byte)FindLogicOptionsIndex());
			writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
			writer.EndMessage();

			writer.EndMessage();
			writer.EndMessage();

			AmongUsClient.Instance.SendOrDisconnect(writer);
			writer.Recycle();
		}

		private static int FindLogicOptionsIndex()
		{
			int logicIndex = -1;
			for(int i = 0; i < GameManager.Instance.LogicComponents.Count; i++)
			{
				GameLogicComponent compontent = GameManager.Instance.LogicComponents[i];

				Hydra.Log.LogMessage($"Found compontent {compontent.GetType()} at index {i}");
				if(compontent.GetType() != typeof(LogicOptions)) continue;

				logicIndex = i;
				break;
			}

			Hydra.Log.LogMessage($"Found LogicOptions at index {logicIndex}");
			return logicIndex;
		}
	}
}
