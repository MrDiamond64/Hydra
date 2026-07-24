using Hazel;
using HarmonyLib;
using HydraMenu.features;
using HydraMenu.network;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class TrollSection : ISection
	{
		public TrollSection() : base("Troll") { }

		public int selectedVent = 0;
		public System.Random rnd = new System.Random();

		public static byte size = 1;
		public static List<string> colors = new List<string> { "black", "blue", "green", "orange", "purple", "red", "white", "yellow" };
		public static byte colorIndex = 0;
		public static bool Enabled { get; set; } = false;

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			}

			Troll.AutoReportBodies.Enabled = Controls.PlayerSpecificToggle("Auto Report Bodies", PlayerControl.LocalPlayer, ref Troll.AutoReportBodies.source);
			Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
			Troll.BlockSabotages.Enabled = GUILayout.Toggle(Troll.BlockSabotages.Enabled, "Block Sabotages");
			Troll.BlockVenting.Enabled = GUILayout.Toggle(Troll.BlockVenting.Enabled, "Disable Vents");

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

			if(GUILayout.Button("Copy Random Player"))
			{
				PlayerControl randomPl = Utilities.GetRandomPlayer();
				Utilities.CopyPlayer(randomPl);
			}

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

			GUILayout.Space(5);
			GUILayout.Label($"Vent TP:");
			Hydra.routines.teleportSpammer.Enabled = GUILayout.Toggle(Hydra.routines.teleportSpammer.Enabled, "Teleport Flooder");

			GUILayout.Label($"Teleport everyone to vent: {selectedVent}");
			selectedVent = (int)GUILayout.HorizontalSlider(selectedVent, 0, ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 10);

			if(GUILayout.Button("Teleport to Vent"))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					Teleporter.TeleportToVent(player, selectedVent);
				}
			}

			if(GUILayout.Button("Teleport to Random Vent"))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player == PlayerControl.LocalPlayer) continue;

					int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);

					Teleporter.TeleportToVent(player, ventId);
				}
			}

			GUILayout.Space(5);
			// Automatically close and open all doors at a set interval
			GUILayout.Label("Door Troller:");
			Hydra.routines.doorTroller.Enabled = GUILayout.Toggle(Hydra.routines.doorTroller.Enabled, "Enabled");

			GUILayout.Label($"Lock and Unlock Delay: {Hydra.routines.doorTroller.lockAndUnlockDelay:F2}s");
			Hydra.routines.doorTroller.lockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.lockAndUnlockDelay, 0.1f, 2.0f);

			if (!Utilities.IsAnticheatPresent())
			{
				GUILayout.Space(5);
				GUILayout.Label("Custom Chat:");
				Enabled = GUILayout.Toggle(Enabled, "Enabled");
				GUILayout.Label($"Size: {size}");
				size = (byte)GUILayout.HorizontalSlider((int)size, 0f, 10f);
				GUILayout.Label("Color: " + colors[colorIndex]);
				colorIndex = (byte)GUILayout.HorizontalSlider((int)colorIndex, 0f, colors.Count - 1);
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
	public static class CustomChatPatch
	{
		private static bool Prefix(PlayerControl __instance, string chatText)
		{
			if (Utilities.IsAnticheatPresent())
			{
				TrollSection.Enabled = false;
				return true;
			}

			if (!TrollSection.Enabled)
			{
				return true;
			}

			string text = $"<size={TrollSection.size}><color={TrollSection.colors[TrollSection.colorIndex]}>{chatText}";
			
			if (DestroyableSingleton<HudManager>.Instance != null)
			{
				DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, text, true);
			}

			BatchedMessage batchedMessage = new BatchedMessage();
			batchedMessage.writer.StartMessage(2);
			batchedMessage.writer.WritePacked(__instance.NetId);
			batchedMessage.writer.Write((byte)13);
			batchedMessage.writer.Write(text);
			batchedMessage.writer.EndMessage();
			batchedMessage.FinishBatch();

			return false;
		}
	}
}
