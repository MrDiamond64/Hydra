using Hazel;
using HydraMenu.features;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class TrollSection : ISection
	{
		private int selectedVent = 0;
		private System.Random rnd = new System.Random();

		public TrollSection() : base("Troll") 
		{
			AddFeature("Auto Report Bodies", () => {
				Troll.AutoReportBodies.Enabled = Controls.PlayerSpecificToggle("Auto Report Bodies", PlayerControl.LocalPlayer, ref Troll.AutoReportBodies.source);
			});
			AddFeature("Auto Trigger Spores", () => {
				Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
			});
			AddFeature("Block Sabotages", () => {
				Troll.BlockSabotages.Enabled = GUILayout.Toggle(Troll.BlockSabotages.Enabled, "Block Sabotages");
			});
			AddFeature("Disable Vents", () => {
				Troll.BlockVenting.Enabled = GUILayout.Toggle(Troll.BlockVenting.Enabled, "Disable Vents");
			});
			AddFeature("Kick All Players", () => {
				if(GUILayout.Button("Kick All Players"))
				{
					Hydra.Log.LogInfo($"Sending Enter ventilation system update to all players");

					MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
					writer.Write((ushort)0);
					writer.Write((byte)VentilationSystem.Operation.Enter);
					writer.Write((byte)0);

					BatchedMessage batch = new BatchedMessage();
					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer);
					batch.FinishBatch();

					writer.Recycle();

					foreach(PlayerControl player in PlayerControl.AllPlayerControls)
					{
						if(player == PlayerControl.LocalPlayer || player.OwnerId == AmongUsClient.Instance.HostId) continue;

						Utilities.KickPlayer(player, true);
					}
				}
			});
			AddFeature("Copy Random Player", () => {
				if(GUILayout.Button("Copy Random Player"))
				{
					PlayerControl randomPl = Utilities.GetRandomPlayer();
					Utilities.CopyPlayer(randomPl);
				}
			});
			AddFeature("Trigger All Spores", () => {
				if(GUILayout.Button("Trigger All Spores"))
				{
					if(Utilities.GetCurrentMap() != MapNames.Fungle)
					{
						Hydra.notifications.Send("Trigger Spores", "This option only works on the Fungle map.");
					}
					else
					{
						FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

						foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
						{
							PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
						}

						Hydra.notifications.Send("Trigger Spores", "All spores have been triggered.", 5);
					}
				}
			});
			AddFeature("Teleport Flooder", () => {
				GUILayout.Label($"Vent TP:");
				Hydra.routines.teleportSpammer.Enabled = GUILayout.Toggle(Hydra.routines.teleportSpammer.Enabled, "Teleport Flooder");
			});
			AddFeature("Teleport to Vent", () => {
				GUILayout.Label($"Teleport everyone to vent: {selectedVent}");
				selectedVent = (int)GUILayout.HorizontalSlider(selectedVent, 0, ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 10);

				if(GUILayout.Button("Teleport to Vent"))
				{
					foreach(PlayerControl player in PlayerControl.AllPlayerControls)
					{
						Teleporter.TeleportToVent(player, selectedVent);
					}
				}
			});
			AddFeature("Teleport to Random Vent", () => {
				if(GUILayout.Button("Teleport to Random Vent"))
				{
					foreach(PlayerControl player in PlayerControl.AllPlayerControls)
					{
						if(player == PlayerControl.LocalPlayer) continue;

						int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);

						Teleporter.TeleportToVent(player, ventId);
					}
				}
			});
			AddFeature("Door Troller", () => {
				GUILayout.Label("Door Troller:");
				Hydra.routines.doorTroller.Enabled = GUILayout.Toggle(Hydra.routines.doorTroller.Enabled, "Enabled");

				GUILayout.Label($"Lock and Unlock Delay: {Hydra.routines.doorTroller.lockAndUnlockDelay:F2}s");
				Hydra.routines.doorTroller.lockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.lockAndUnlockDelay, 0.1f, 2.0f);
			});
		}

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			}

			foreach (var feature in Features)
			{
				feature.RenderAction();
			}
		}
	}
}