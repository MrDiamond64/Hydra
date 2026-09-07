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
				Hydra.Log.LogMessage($"{player.Data.name} updated security cameras, we do not need to do anything as the Comms sabotage is already active");
				return;
			}

			Hydra.Log.LogMessage($"{player.Data.PlayerName} stopped watching cameras, sending Comms system update");

			DisableCommsFor(player);
		}

		// Fix an edge case where if a player is watching cameras while the comms sabotage is in effect
		// and someone fixes the comms sabotage, then the fake comms effect on the player will be removed
		// and the player will be able to watch security cameras
		private void OnCommsRepair()
		{
			Hydra.Log.LogMessage($"Comms sabotage was fixed, re-applying comms effect to all players watching security cameras");

			// If we are the host, then make sure to flush any queued net objects
			// It may take up to 100ms for SendAllStreamedObjects to be called by the game, so if we are re-applying the comms effect now
			// then clients will first receive our enable comms sabotage message which we are sending here
			// and then the game will later send a message with the comms sabotage fixed, overriding the enabled comms sabotage
			if(AmongUsClient.Instance.AmHost)
			{
				AmongUsClient.Instance.SendAllStreamedObjects();
			}

			ReapplyCommsEffect();
		}

		private void EnableCommsFor(PlayerControl player)
		{
			if(!AmongUsClient.Instance.AmHost)
			{
				Sabotage.SabotageSystem(SystemTypes.Comms, player.OwnerId);
				return;
			}

			BatchedMessage batch = new BatchedMessage(player.OwnerId);

			MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
			systemUpdate.StartMessage((byte)SystemTypes.Comms);
			// 1 = Comms sabotage is active, 0 = Comms sabotage is inactive
			systemUpdate.Write(1);
			systemUpdate.EndMessage();

			batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);

			batch.FinishBatch();
		}

		private void DisableCommsFor(PlayerControl player)
		{
			if(!AmongUsClient.Instance.AmHost)
			{
				Sabotage.FixSabotage(SystemTypes.Comms, player.OwnerId);
				return;
			}

			BatchedMessage batch = new BatchedMessage(player.OwnerId);

			MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
			systemUpdate.StartMessage((byte)SystemTypes.Comms);
			// 1 = Comms sabotage is active, 0 = Comms sabotage is inactive
			systemUpdate.Write(0);
			systemUpdate.EndMessage();

			batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);

			batch.FinishBatch();
		}

		private void ReapplyCommsEffect()
		{
			if(ShipStatus.Instance == null) return;

			ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Security, out ISystemType system);
			if(system == null) return;

			SecurityCameraSystemType securitySystem = system.Cast<SecurityCameraSystemType>();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer || !securitySystem.PlayersUsing.Contains(player.PlayerId)) continue;

				EnableCommsFor(player);
			}
		}

		protected override void OnEnable()
		{
			ReapplyCommsEffect();

			if(Utilities.GetCurrentMap() == MapNames.Fungle)
			{
				Hydra.notifications.Send("Disable Security Cameras", "This feature does not work on The Fungle.", 10);
			}

			EventCoordinator.OnPlayerEnterCameras += OnPlayerEnterCameras;
			EventCoordinator.OnPlayerExitCameras += OnPlayerExitCameras;
			EventCoordinator.OnHudOverrideRepair += OnCommsRepair;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerEnterCameras -= OnPlayerEnterCameras;
			EventCoordinator.OnPlayerExitCameras -= OnPlayerExitCameras;
			EventCoordinator.OnHudOverrideRepair -= OnCommsRepair;
		}
	}
}