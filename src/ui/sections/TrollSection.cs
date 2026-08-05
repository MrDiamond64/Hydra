using Hazel;
using HydraMenu.features;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class TrollSection : ISection
	{
		public TrollSection() : base("Troll") { }

		public int selectedVent = 0;
		public System.Random rnd = new System.Random();

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			}

			Troll.AutoReportBodies.Enabled = Controls.PlayerSpecificToggle("Auto Report Bodies", PlayerControl.LocalPlayer, ref Troll.AutoReportBodies.source);
			Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
			Hydra.routines.discoSelf.Enabled = GUILayout.Toggle(Hydra.routines.discoSelf.Enabled, "Disco Mod (Non-Host)");
			if(Hydra.routines.discoSelf.Enabled)
			{
				GUILayout.Label($"Disco Delay: {Hydra.routines.discoSelf.delay:F2}s");
				Hydra.routines.discoSelf.delay = (float)System.Math.Round(GUILayout.HorizontalSlider(Hydra.routines.discoSelf.delay, 0.05f, 2.0f), 2);
			}

			bool prevHand = Hydra.routines.petPlayer.Enabled && Hydra.routines.petPlayer.manualControl;
			bool newHand = GUILayout.Toggle(prevHand, "Control Petting Hand");
			if(newHand != prevHand)
			{
				if(newHand)
				{
					Hydra.routines.petPlayer.target = null;
					Hydra.routines.petPlayer.manualControl = true;
					Hydra.routines.petPlayer.Enabled = true;
				}
				else
				{
					Hydra.routines.petPlayer.Enabled = false;
				}
			}
			if(Hydra.routines.petPlayer.Enabled && Hydra.routines.petPlayer.manualControl)
			{
				GUILayout.Label($"Hand Speed: {Hydra.routines.petPlayer.speed:F1}");
				Hydra.routines.petPlayer.speed = (float)System.Math.Round(GUILayout.HorizontalSlider(Hydra.routines.petPlayer.speed, 1.0f, 15.0f), 1);
			}

			if(ShipStatus.Instance != null && Utilities.GetCurrentMap() == MapNames.Skeld)
			{
				Hydra.routines.noMeetingSkeld.Enabled = GUILayout.Toggle(Hydra.routines.noMeetingSkeld.Enabled, "No meeting (Non-host) (The Skeld)");
				if(Hydra.routines.noMeetingSkeld.Enabled)
				{
					GUILayout.Label($"Area Margin: +{Hydra.routines.noMeetingSkeld.extraMargin:F1}");
					Hydra.routines.noMeetingSkeld.extraMargin = (float)System.Math.Round(GUILayout.HorizontalSlider(Hydra.routines.noMeetingSkeld.extraMargin, 0.0f, 2.5f), 1);
				}
			}
			else
			{
				Hydra.routines.noMeetingSkeld.Enabled = false;
			}

			if(ShipStatus.Instance != null && Utilities.GetCurrentMap() == MapNames.MiraHQ)
			{
				Hydra.routines.noMeetingMira.Enabled = GUILayout.Toggle(Hydra.routines.noMeetingMira.Enabled, "No meeting (Non-host) (MiraHQ)");
				if(Hydra.routines.noMeetingMira.Enabled)
				{
					GUILayout.Label($"Area Margin: +{Hydra.routines.noMeetingMira.extraMargin:F1}");
					Hydra.routines.noMeetingMira.extraMargin = (float)System.Math.Round(GUILayout.HorizontalSlider(Hydra.routines.noMeetingMira.extraMargin, 0.0f, 2.5f), 1);
				}
			}
			else
			{
				Hydra.routines.noMeetingMira.Enabled = false;
			}

			if(ShipStatus.Instance != null && Utilities.GetCurrentMap() == MapNames.Polus)
			{
				Hydra.routines.noMeetingPolus.Enabled = GUILayout.Toggle(Hydra.routines.noMeetingPolus.Enabled, "No meeting (Non-host) (Polus)");
				if(Hydra.routines.noMeetingPolus.Enabled)
				{
					GUILayout.Label($"Area Margin: +{Hydra.routines.noMeetingPolus.extraMargin:F1}");
					Hydra.routines.noMeetingPolus.extraMargin = (float)System.Math.Round(GUILayout.HorizontalSlider(Hydra.routines.noMeetingPolus.extraMargin, 0.0f, 2.5f), 1);
				}
			}
			else
			{
				Hydra.routines.noMeetingPolus.Enabled = false;
			}

			if(ShipStatus.Instance != null && Utilities.GetCurrentMap() == MapNames.Airship)
			{
				Hydra.routines.noMeetingAirship.Enabled = GUILayout.Toggle(Hydra.routines.noMeetingAirship.Enabled, "No meeting (Non-host) (Airship)");
				if(Hydra.routines.noMeetingAirship.Enabled)
				{
					GUILayout.Label($"Area Margin: +{Hydra.routines.noMeetingAirship.extraMargin:F1}");
					Hydra.routines.noMeetingAirship.extraMargin = (float)System.Math.Round(GUILayout.HorizontalSlider(Hydra.routines.noMeetingAirship.extraMargin, 0.0f, 2.5f), 1);
				}
			}
			else
			{
				Hydra.routines.noMeetingAirship.Enabled = false;
			}

			if(ShipStatus.Instance != null && Utilities.GetCurrentMap() == MapNames.Fungle)
			{
				Hydra.routines.noMeetingFungle.Enabled = GUILayout.Toggle(Hydra.routines.noMeetingFungle.Enabled, "No meeting (Non-host) (Fungle)");
				if(Hydra.routines.noMeetingFungle.Enabled)
				{
					GUILayout.Label($"Area Margin: +{Hydra.routines.noMeetingFungle.extraMargin:F1}");
					Hydra.routines.noMeetingFungle.extraMargin = (float)System.Math.Round(GUILayout.HorizontalSlider(Hydra.routines.noMeetingFungle.extraMargin, 0.0f, 2.5f), 1);
				}
			}
			else
			{
				Hydra.routines.noMeetingFungle.Enabled = false;
			}

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
		}
	}
}