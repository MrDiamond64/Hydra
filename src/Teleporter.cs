using Hazel;
using HydraMenu.features;
using HydraMenu.network;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu
{
	internal class Teleporter
	{
		// This exists for the same reason as why the UpdateSystemsDirectly option for sabotages exist
		public static bool UseSnapToRPC { get; set; } = true;

		public static Dictionary<string, Vector2> skeldTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "Cafeteria", new Vector2(-0.78f, 2.48f) },
			{ "Weapons", new Vector2(8.04f, 1.24f) },
			{ "Medbay", new Vector2(-8.61f, -4.30f) },
			{ "Admin", new Vector2(2.79f, -7.69f) },
			{ "Oxygen", new Vector2(6.50f, -3.50f) },
			{ "Navigation", new Vector2(16.77f, -4.67f) },
			{ "Shields", new Vector2(9.26f, -12.19f) },
			{ "Communications", new Vector2(5.10f, -15.24f ) },
			{ "Storage", new Vector2(-3.72f, -14.61f ) },
			{ "Electrical", new Vector2(-6.91f, -8.60f ) },
			{ "Upper Engine", new Vector2(-17.61f, -0.72f) },
			{ "Lower Engine", new Vector2(-17.33f, -13.10f ) },
			{ "Security", new Vector2(-12.81f, -3.01f) },
			{ "Reactor", new Vector2(-20.53f, -5.39f) },
		};

		public static Dictionary<string, Vector2> miraTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "Launchpad", new Vector2(-4.43f, 1.98f) },
			{ "Medbay", new Vector2(14.58f, 0.33f) },
			{ "Communications", new Vector2(15.60f, 4.96f) },
			{ "Locker Room", new Vector2(9.68f, 3.71f) },
			{ "Decontamination", new Vector2(6.12f, 6.34f) },
			{ "Laboratory", new Vector2(9.43f, 13.98f) },
			{ "Reactor", new Vector2(2.55f, 11.71f) },
			{ "Office", new Vector2(14.68f, 20.63f) },
			{ "Admin", new Vector2(19.41f, 19.01f) },
			{ "Greenhouse", new Vector2(17.92f, 23.86f) },
			{ "Cafeteria", new Vector2(25.44f, 2.77f) },
			{ "Storage", new Vector2(19.59f, 4.79f) },
			{ "Weapons", new Vector2(19.94f, -1.96f) },
		};

		public static Dictionary<string, Vector2> polusTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "Dropship", new Vector2(16.61f, -1.17f) },
			{ "Storage", new Vector2(20.35f, -11.46f) },
			{ "Electrical", new Vector2(7.51f, -9.86f) },
			{ "Security", new Vector2(2.98f, -12.18f) },
			{ "Oxygen", new Vector2(1.55f, -16.81f ) },
			{ "Boiler Room", new Vector2(2.14f, -23.55f) },
			{ "Communications", new Vector2(11.70f, -15.87f ) },
			{ "Weapons", new Vector2(10.71f, -22.90f) },
			{ "Office", new Vector2(25.00f, -16.99f) },
			{ "Admin", new Vector2(22.76f, -22.32f) },
			{ "Laboratory", new Vector2(33.48f, -7.41f) },
			{ "Specimen", new Vector2(36.78f, -19.28f) }
		};

		public static Dictionary<string, Vector2> airshipTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "Engine Room", new Vector2(-0.73f, 0.60f) },
			{ "Communications", new Vector2(-13.03f, 1.31f ) },
			{ "Cockpit", new Vector2(-20.98f, -1.15f ) },
			{ "Armory", new Vector2(-10.28f, -6.44f ) },
			{ "Kitchen", new Vector2(-4.31f, -11.08f ) },
			{ "Viewing Deck", new Vector2(-13.62f, -12.68f ) },
			{ "Security", new Vector2(7.13f, -11.51f ) },
			{ "Electrical", new Vector2(16.29f, -6.33f ) },
			{ "Medical", new Vector2(22.85f, -7.40f ) },
			{ "Cargo Bay", new Vector2(33.74f, -0.76f ) },
			{ "Lounge", new Vector2(25.82f, 7.24f ) },
			{ "Records", new Vector2(19.86f, 10.07f ) },
			{ "Showers", new Vector2(20.88f, 0.48f ) },
			{ "Main Hall", new Vector2(10.77f, -0.13f ) },
			{ "Brig", new Vector2(-0.94f, 8.66f ) },
			{ "Vault", new Vector2(-8.78f, 8.13f ) },
			{ "Gap Room", new Vector2(4.15f, 8.73f ) },
			{ "Meeting Room", new Vector2(11.03f, 16.06f ) },
		};

		public static Dictionary<string, Vector2> fungleTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "Meeting Room", new Vector2(-3.08f, -0.41f) },
			{ "The Dorm", new Vector2(1.66f, -1.53f) },
			{ "Laboratory", new Vector2(-4.31f, -8.56f) },
			{ "Greenhouse", new Vector2(9.34f, -9.92f) },
			{ "Reactor", new Vector2(21.38f, -7.48f) },
			{ "Upper Engine", new Vector2(21.54f, 2.97f) },
			{ "Lookout", new Vector2(7.79f, 1.53f) },
			{ "Mining Pit", new Vector2(12.56f, 9.28f) },
			{ "Communications", new Vector2(21.17f, 13.70f) },
			{ "Storage", new Vector2(-0.44f, 5.69f) },
			{ "Dropship", new Vector2(-7.59f, 10.76f) },
			{ "Cafeteria", new Vector2(-16.49f, 7.14f) },
			{ "Splash Zone", new Vector2(-17.89f, -0.08f) },
			{ "Kitchen", new Vector2(-15.55f, -7.52f) },
			{ "Dock", new Vector2(-23.00f, -7.16f) },
		};

		public static Dictionary<string, Vector2> dleksTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "Cafeteria", new Vector2(0.78f, 2.48f) },
			{ "Weapons", new Vector2(-8.04f, 1.24f) },
			{ "Medbay", new Vector2(8.61f, -4.30f) },
			{ "Admin", new Vector2(-2.79f, -7.69f) },
			{ "Oxygen", new Vector2(-6.50f, -3.50f) },
			{ "Navigation", new Vector2(-16.77f, -4.67f) },
			{ "Shields", new Vector2(-9.26f, -12.19f) },
			{ "Communications", new Vector2(-5.10f, -15.24f ) },
			{ "Storage", new Vector2(3.72f, -14.61f ) },
			{ "Electrical", new Vector2(6.91f, -8.60f ) },
			{ "Upper Engine", new Vector2(17.61f, -0.72f) },
			{ "Lower Engine", new Vector2(17.33f, -13.10f ) },
			{ "Security", new Vector2(12.81f, -3.01f) },
			{ "Reactor", new Vector2(20.53f, -5.39f) },
		};

		public static Dictionary<string, Vector2> GetTeleportLocations()
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			return currentMap switch
			{
				MapNames.Skeld => skeldTeleportLocations,
				MapNames.MiraHQ => miraTeleportLocations,
				MapNames.Polus => polusTeleportLocations,
				MapNames.Airship => airshipTeleportLocations,
				MapNames.Fungle => fungleTeleportLocations,
				MapNames.Dleks => dleksTeleportLocations,
				// If we don't have any teleport locations for the current map then just default to the Skeld ones
				_ => skeldTeleportLocations,
			};
		}

		public static void TeleportTo(Vector2 position)
		{
			if(UseSnapToRPC)
			{
				PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
			}
			else
			{
				PlayerControl.LocalPlayer.NetTransform.SnapTo(position);
			}
		}

		public static void TeleportPlayerTo(PlayerControl player, Vector2 position)
		{
			BatchedMessage batch = new BatchedMessage();
			batch.QueueSnapTo(player, position);
			batch.FinishBatch();
		}

		public static void TeleportAllTo(Vector2 position)
		{
			BatchedMessage batch = new BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				batch.QueueSnapTo(player, position);
			}

			batch.FinishBatch();
		}

		public static void TeleportToVent(PlayerControl player, int ventId)
		{
			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Vent TP", "The game must have started in order for this feature to work");
				return;
			}

			if(AmongUsClient.Instance.AmHost)
			{
				player.MyPhysics.RpcBootFromVent(ventId);
				return;
			}

			if(!Troll.VentSeqIds.ContainsKey(player))
			{
				// High enough value to supersede the actual sequence ID
				Troll.VentSeqIds.Add(player, 10000);
			}

			MessageWriter enterVent = MessageWriter.Get(SendOption.None);
			enterVent.Write(++Troll.VentSeqIds[player]);
			enterVent.Write((byte)VentilationSystem.Operation.Enter);
			enterVent.Write((byte)ventId);

			MessageWriter bootFromVent = MessageWriter.Get(SendOption.None);
			bootFromVent.Write(++Troll.VentSeqIds[player]);
			bootFromVent.Write((byte)VentilationSystem.Operation.BootImpostors);
			bootFromVent.Write((byte)ventId);

			BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);
			batch.QueueUpdateSystem(player, SystemTypes.Ventilation, enterVent);
			batch.QueueUpdateSystem(player, SystemTypes.Ventilation, bootFromVent);
			batch.FinishBatch();

			enterVent.Recycle();
			bootFromVent.Recycle();
		}
	}
}