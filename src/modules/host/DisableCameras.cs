using Hazel;
using HydraMenu.network;

namespace HydraMenu.modules.host
{
	internal class DisableCameras : Module
	{
		// It is not possible to watch security cameras when the comms sabotage is active. We can abuse this to disable security cameras
		// When a player starts to watch security cameras, sabotage comms for that player, when the player stops watching cameras, fix comms sabotage for that player
		public DisableCameras() : base("DisableCameras") { }

		private void OnPlayerEnterCameras(PlayerControl player)
		{
			UpdateCommsState(player, 1);
		}

		private void OnPlayerExitCameras(PlayerControl player)
		{
			UpdateCommsState(player, 0);
		}

		private void UpdateCommsState(PlayerControl player, byte operation)
		{
			if(!AmongUsClient.Instance.AmHost || player == PlayerControl.LocalPlayer) return;

			// Prevent an exploit where if the comms sabotage is active, someone could enter and leave the security cameras to remove the comms effect from themselves
			if(Sabotage.IsSabotageActive(SystemTypes.Comms))
			{
				// There is an edge case where if someone is on the security cameras panel when comms are actively sabotaged, and the sabotage is fixed,
				// then the player will be able to watch the security cameras
				// I don't think it is worthwhile to fix this edge case considering this feature is unlikely to even be used by anyone
				Hydra.Log.LogMessage($"{player.Data.name} updated security cameras, we do not need to do anything as the Comms sabotage is already active");
				return;
			}

			Hydra.Log.LogMessage($"{player.Data.PlayerName} updated security cameras, sending Comms system update");

			MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
			systemUpdate.StartMessage((byte)SystemTypes.Comms);
			// 1 = Comms sabotage is active, 0 = Comms sabotage is inactive
			systemUpdate.Write(operation == 1);
			systemUpdate.EndMessage();

			BatchedMessage batch = new BatchedMessage(player.OwnerId);
			batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);
			batch.FinishBatch();
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerEnterCameras += OnPlayerEnterCameras;
			EventCoordinator.OnPlayerExitCameras += OnPlayerExitCameras;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerEnterCameras -= OnPlayerEnterCameras;
			EventCoordinator.OnPlayerExitCameras -= OnPlayerExitCameras;
		}
	}
}