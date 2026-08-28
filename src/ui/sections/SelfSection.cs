using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SelfSection : Section
	{
		public SelfSection() : base("Self") { }

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			}
			else
			{
				GUILayout.Label($"Role: {PlayerControl.LocalPlayer.Data.RoleType}");
			}

			// Self.BypassIntentionalDisconnectionBlocks.Enabled = GUILayout.Toggle(Self.BypassIntentionalDisconnectionBlocks.Enabled, "Bypass intentional disconnection temp bans");
			ModuleManager.alwaysShowTaskAnimations.Enabled = GUILayout.Toggle(ModuleManager.alwaysShowTaskAnimations.Enabled, "Always Show Task Animations");
			ModuleManager.immortality.Enabled = GUILayout.Toggle(ModuleManager.immortality.Enabled, "Become Immortal");
			ModuleManager.noLadderCooldown.Enabled = GUILayout.Toggle(ModuleManager.noLadderCooldown.Enabled, "No Ladder Cooldown");
			ModuleManager.unlimitedMeetings.Enabled = GUILayout.Toggle(ModuleManager.unlimitedMeetings.Enabled, "Unlimited Meetings");
			ModuleManager.updateStatsFreeplay.Enabled = GUILayout.Toggle(ModuleManager.updateStatsFreeplay.Enabled, "Update Stats in Freeplay");

			if(GUILayout.Button("Call Meeting"))
			{
				Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, null);
			}

			if(GUILayout.Button("Complete All Tasks"))
			{
				PlayerControl.LocalPlayer.StartCoroutine(CompleteAllTasks().WrapToIl2Cpp());
			}

			GUILayout.Label("Task Animations:");
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Start Medbay Scan"))
			{
				RPCEmitter.SendSetScanner(true);
			}

			if(GUILayout.Button("Finish Medbay Scan"))
			{
				RPCEmitter.SendSetScanner(false);
			}
			GUILayout.EndHorizontal();

			Dictionary<string, TaskTypes> animations = MapAssets.GetAnimations();
			Controls.DrawButtonCell(animations, PlayAnimation, 2);

			GUILayout.Space(5);
			GUILayout.Label("Avatar Controls:");
			if(GUILayout.Button("Randomize Avatar"))
			{
				if(AmongUsClient.Instance.AmConnected)
				{
					Utilities.RandomizePlayer(true);

					Hydra.notifications.Send("Player Randomizer", "Your avatar has been randomized for this game.", 5);
				}
				else
				{
					Utilities.RandomizePlayer();

					Hydra.notifications.Send("Player Randomizer", "Your name and avatar has been randomized.", 5);
				}
			}

			if(GUILayout.Button("Randomize Color"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());
			}

			if(GUILayout.Button("Restore Avatar"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor(DataManager.Player.Customization.Color);
				PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
				PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
				PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
				PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
			}
		}

		public IEnumerator CompleteAllTasks()
		{
			Il2CppSystem.Collections.Generic.List<PlayerTask> allTasks = PlayerControl.LocalPlayer.myTasks;

			Hydra.Log.LogInfo("Completing all tasks...");
			foreach(PlayerTask task in allTasks)
			{
				if(task.IsComplete)
				{
					Hydra.Log.LogInfo($"Task {task.Id} has already been completed, skipping");
					continue;
				}

				Hydra.Log.LogInfo($"Sent CompleteTask RPC for task {task.Id}");
				PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);

				// If we want to complete more than six tasks then a delay needs to be implemented
				// otherwise the vanilla anticheat will kick us for violating ratelimits
				yield return Effects.Wait(0.05f);
			}

			Hydra.notifications.Send("Task Finisher", "All your tasks have been finished.", 5);
		}

		public void PlayAnimation(TaskTypes task)
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("Play Animation", "This option can only be used inside of a game.");
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Play Animation", "There must be an instance of ShipStatus for this feature to work.");
				return;
			}

			RPCEmitter.SendPlayAnimation((byte)task);
		}
	}
}