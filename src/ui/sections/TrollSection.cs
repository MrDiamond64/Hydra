using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class TrollSection : Section
	{
		public TrollSection() : base("Troll") { }

		private int selectedVent = 0;
		private readonly System.Random rnd = new System.Random();

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			}

			ModuleManager.autoReportBodies.Enabled = Controls.PlayerSpecificToggle("Auto Report Bodies", PlayerControl.LocalPlayer, ref ModuleManager.autoReportBodies.target);
			Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
			ModuleManager.disableCameras.Enabled = GUILayout.Toggle(ModuleManager.disableCameras.Enabled, "Disable Security Cameras");
			ModuleManager.disableSabotages.Enabled = GUILayout.Toggle(ModuleManager.disableSabotages.Enabled, "Disable Sabotages");
			ModuleManager.disableVents.Enabled = GUILayout.Toggle(ModuleManager.disableVents.Enabled, "Disable Vents");

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

			if(GUILayout.Button("Deplete HnS Timer"))
			{
				AmongUsClient.Instance.StartCoroutine(DepleteSeekTimer().WrapToIl2Cpp());
			}

			Dictionary<int, string> vents = MapAssets.GetVents();

			GUILayout.Space(5);
			GUILayout.Label($"Vent TP:");
			Hydra.routines.teleportSpammer.Enabled = GUILayout.Toggle(Hydra.routines.teleportSpammer.Enabled, "Teleport Flooder");

			GUILayout.Label($"Teleport everyone to vent: {vents[selectedVent]}");
			selectedVent = (int)GUILayout.HorizontalSlider(selectedVent, 0, vents.Count - 1);

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

			GUILayout.Label($"Lock and Unlock Delay: {Hydra.routines.doorTroller.LockAndUnlockDelay:F2}s");
			Hydra.routines.doorTroller.LockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.LockAndUnlockDelay, 0.1f, 2.0f);

			GUILayout.Space(5);
			GUILayout.Label("Auto Expose Impostors:");
			ModuleManager.autoExposeImpostors.Enabled = GUILayout.Toggle(ModuleManager.autoExposeImpostors.Enabled, "Enabled");
			ModuleManager.autoExposeImpostors.ExposeOnMurder = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnMurder, "Expose On Murder");
			ModuleManager.autoExposeImpostors.ExposeOnShapeshift = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnShapeshift, "Expose On Shapeshift");
			ModuleManager.autoExposeImpostors.ExposeOnPhantom = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnPhantom, "Expose On Phantom");
		}

		// In Hide and Seek, completing a task will reduce the HnS hide timer depending on the length of the task (short, common, or long)
		// The problem is the game reduces the task timer even if we have already completed the task
		// so we can spam the CompleteTask RPC with the same task, and reduce the task timer to zero seconds
		private IEnumerator DepleteSeekTimer()
		{
			if(!GameManager.Instance.IsHideAndSeek())
			{
				Hydra.notifications.Send("Deplete HnS Timer", "This feature can only be used in Hide and Seek.");
				yield break;
			}

			LogicGameFlowHnS gameFlow = GameManager.Instance.LogicFlow.Cast<LogicGameFlowHnS>();
			LogicOptionsHnS logicOptions = GameManager.Instance.LogicOptions.Cast<LogicOptionsHnS>();

			if(AmongUsClient.Instance.AmHost)
			{
				gameFlow.AdjustEscapeTimer(gameFlow.currentHideTime, true);
				yield break;
			}

			PlayerTask task = PlayerControl.LocalPlayer.myTasks[0];
			if(task == null)
			{
				Hydra.notifications.Send("Deplete HnS Timer", "This feature requires you to have at least one task.");
				yield break;
			}

			// If the HnS hide timer is up, all our tasks are replaced with a task of ImportantTextTask
			NormalPlayerTask normalTask = task.TryCast<NormalPlayerTask>();
			if(normalTask == null)
			{
				Hydra.notifications.Send("Deplete HnS Timer", "This feature cannot be used during the final hide time.");
				yield break;
			}

			float completeDeduction;
			switch(normalTask.Length)
			{
				case NormalPlayerTask.TaskLength.None:
				case NormalPlayerTask.TaskLength.Common:
				default:
					completeDeduction = logicOptions.GetCommonTaskTimeValue();
					break;

				case NormalPlayerTask.TaskLength.Short:
					completeDeduction = logicOptions.GetShortTaskTimeValue();
					break;

				case NormalPlayerTask.TaskLength.Long:
					completeDeduction = logicOptions.GetLongTaskTimeValue();
					break;
			}

			int totalCompletions = 0;
			int requiredCompletions = (int)Math.Ceiling(gameFlow.currentHideTime / completeDeduction);

			Hydra.Log.LogInfo($"Current escape time is {gameFlow.currentHideTime} and each task completion reduces the timer by {completeDeduction}s. We need to send the CompleteTask RPC {requiredCompletions} times to deplete the HnS timer.");

			while(totalCompletions < requiredCompletions)
			{
				BatchedMessage batch = new BatchedMessage();

				// The message packing limit for non-hosts should be ten, but the Among Us anticheat disconnects us if we have more than six CompleteTask RPCs in a single batch
				for(byte i = 0; i < 6; i++)
				{
					batch.QueueCompleteTask(PlayerControl.LocalPlayer, (uint)task.Index);
				}

				batch.FinishBatch();

				// Each batch will contain exactly six CompleteTask RPCs, which may be more than the required task completions, but that is fine
				totalCompletions += 6;

				yield return Effects.Wait(0.05f);
			}
		}
	}
}