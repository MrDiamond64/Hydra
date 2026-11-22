using HarmonyLib;
using Hazel;
using HydraMenu.network;

namespace HydraMenu.modules.host
{
	internal class ExtendLobbyTimer : Module
	{
		// Changes the expiration timer of lobbies from ten minutes to thirteen minutes
		public ExtendLobbyTimer() : base("ExtendLobbyTimer")
		{
			base.Enabled = true;
		}

		private static ExtendLobbyTimer Instance
		{
			get { return ModuleManager.extendLobbyTimer; }
		}

		[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.HandleRpc))]
		class HandleRpc
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				if(!Instance.Enabled || !AmongUsClient.Instance.AmHost || callId != (byte)RpcCalls.LobbyTimeExpiring) return true;

				// In the PC and iOS clients, handling of lobby time extension requests appear to be unimplemented
				// I have a theory that lobby time extension requests are used by PlayStation versions of the game
				// In the PlayStation games, you have additional regions to play on. These regions are hosted by Sony Interactive Entertainment
				// These regions do not get as many players as the main regions, simply because they lack cross-play between platforms
				// It would make more sense for SIE hosted lobbies to have increased lobby timers to accomodate for fewer players
				// Though I am not sure why even normal regions would handle lobby expiration timers, and I do not have a PlayStation to confirm this theory
				int timeRemaining = reader.ReadPackedInt32();
				bool extensionAvailable = reader.ReadBoolean();
				int hostId = 255;
				// Each extension ID starts from zero and is incremented by one for every extension
				// I thought that depending on how much lobbies exist on a region, the backend servers will allow for more extension requests
				// but no matter what, each lobby is only given one extension that is three minutes long
				int extensionId = 0;
				int extensionTime = 0;

				if(extensionAvailable)
				{
					hostId = reader.ReadPackedInt32();
					extensionId = reader.ReadPackedInt32();
					extensionTime = reader.ReadPackedInt32();
				}

				Hydra.Log.LogMessage($"Time remaining: {timeRemaining}, Extension Available: {extensionAvailable}, Host ID: {hostId}, Extension ID: {extensionId}, Extension Time: {extensionTime}");
				if(!extensionAvailable) return true;

				BatchedMessage batch = new BatchedMessage();
				batch.QueueExtendLobbyTimer(extensionId);
				batch.FinishBatch();

				return false;
			}
		}
	}
}