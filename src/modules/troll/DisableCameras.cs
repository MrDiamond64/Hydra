using Hazel;
using HydraMenu.network;

namespace HydraMenu.modules.troll
{
	internal class DisableCameras : Module
	{
		// It is not possible to watch security cameras when the comms sabotage is active. We can abuse this to disable security cameras
		// When a player starts to watch security cameras, sabotage comms for that player, when the player stops watching cameras, fix comms sabotage for that player
		public DisableCameras() : base("DisableCameras") { }

		private void OnPlayerEnterCameras(PlayerControl player)
		{
			// If we update Comms for the host, then everybody will be affected by Comms
			if(player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer) return;

			Hydra.Log.LogMessage($"{player.Data.PlayerName} started to watch cameras, sending Comms system update");

			EnableCommsFor(player);
		}

		private void OnPlayerExitCameras(PlayerControl player)
		{
			// If we update Comms for the host, then everybody will be affected by Comms
			if(player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer) return;

			// Prevent an exploit where if the comms sabotage is active, someone could enter and leave the security cameras to remove the comms effect from themselves
			if(Sabotage.IsSabotageActive(SystemTypes.Comms))
			{
				// There is an edge case where if someone is on the security cameras panel when comms are actively sabotaged, and the sabotage is fixed,
				// then the player will be able to watch the security cameras
				// I don't think it is worthwhile to fix this edge case considering this feature is unlikely to even be used by anyone
				Hydra.Log.LogMessage($"{player.Data.name} updated security cameras, we do not need to do anything as the Comms sabotage is already active");
				return;
			}

			Hydra.Log.LogMessage($"{player.Data.PlayerName} stopped watching cameras, sending Comms system update");

			DisableCommsFor(player);
		}

		private void EnableCommsFor(PlayerControl player)
		{
			BatchedMessage batch = new BatchedMessage(player.OwnerId);

			if(!AmongUsClient.Instance.AmHost)
			{
				batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Comms, 128);
			}
			else
			{
				MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
				systemUpdate.StartMessage((byte)SystemTypes.Comms);
				// 1 = Comms sabotage is active, 0 = Comms sabotage is inactive
				systemUpdate.Write(1);
				systemUpdate.EndMessage();

				batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);
			}

			batch.FinishBatch();
		}

		private void DisableCommsFor(PlayerControl player)
		{
			BatchedMessage batch = new BatchedMessage(player.OwnerId);

			if(!AmongUsClient.Instance.AmHost)
			{
				batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Comms, 16 | 0);
				batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Comms, 16 | 1);
			}
			else
			{
				MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
				systemUpdate.StartMessage((byte)SystemTypes.Comms);
				// 1 = Comms sabotage is active, 0 = Comms sabotage is inactive
				systemUpdate.Write(0);
				systemUpdate.EndMessage();

				batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);
			}

			batch.FinishBatch();
		}

		protected override void OnEnable()
		{
			if(ShipStatus.Instance != null)
			{
				ISystemType system = ShipStatus.Instance.Systems[SystemTypes.Security];
				if(system == null) return;

				SecurityCameraSystemType securitySystem = system.Cast<SecurityCameraSystemType>();

				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer || !securitySystem.PlayersUsing.Contains(player.PlayerId)) return;

					EnableCommsFor(player);
				}
			}

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