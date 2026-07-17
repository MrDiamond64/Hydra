using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.features;
using HydraMenu.network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SelfSection : ISection
	{
		public SelfSection() : base("Self") 
		{
			AddFeature("Update Stats in Freeplay", () => {
				Self.UpdateStatsFreeplay.Enabled = GUILayout.Toggle(Self.UpdateStatsFreeplay.Enabled, "Update Stats in Freeplay");
			});
			AddFeature("Become Immortal", () => {
				Immortality.Enabled = GUILayout.Toggle(Immortality.Enabled, "Become Immortal");
			});
			AddFeature("Always Show Task Animations", () => {
				Self.AlwaysShowTaskAnimations = GUILayout.Toggle(Self.AlwaysShowTaskAnimations, "Always Show Task Animations");
			});
			AddFeature("No Ladder Cooldown", () => {
				Self.NoLadderCooldown.Enabled = GUILayout.Toggle(Self.NoLadderCooldown.Enabled, "No Ladder Cooldown");
			});
			AddFeature("Unlimited Meetings", () => {
				Self.UnlimitedMeetings.enabled = GUILayout.Toggle(Self.UnlimitedMeetings.enabled, "Unlimited Meetings");
			});
			AddFeature("Call Meeting", () => {
				if(GUILayout.Button("Call Meeting"))
				{
					Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, null);
				}
			});
			AddFeature("Complete All Tasks", () => {
				if(GUILayout.Button("Complete All Tasks"))
				{
					PlayerControl.LocalPlayer.StartCoroutine(CompleteAllTasks().WrapToIl2Cpp());
				}
			});
			AddFeature("Start Medbay Scan", () => {
				if(GUILayout.Button("Start Medbay Scan"))
				{
					RPCEmitter.SendSetScanner(true);
				}
			});
			AddFeature("Finish Medbay Scan", () => {
				if(GUILayout.Button("Finish Medbay Scan"))
				{
					RPCEmitter.SendSetScanner(false);
				}
			});
			AddFeature("Randomize Avatar", () => {
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
			});
			AddFeature("Randomize Color", () => {
				if(GUILayout.Button("Randomize Color"))
				{
					PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());
				}
			});
			AddFeature("Restore Avatar", () => {
				if(GUILayout.Button("Restore Avatar"))
				{
					PlayerControl.LocalPlayer.CmdCheckColor(DataManager.Player.Customization.Color);
					PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
					PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
					PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
					PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
				}
			});
		}

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

			RenderOrderedFeatures();
		}

		private void RenderOrderedFeatures()
		{
			Features[0].RenderAction();
			Features[1].RenderAction();
			Features[2].RenderAction();
			Features[3].RenderAction();
			Features[4].RenderAction();
			Features[5].RenderAction();
			Features[6].RenderAction();

			GUILayout.Label("Task Animations:");
			GUILayout.BeginHorizontal();
			Features[7].RenderAction();
			Features[8].RenderAction();
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Label("Avatar Controls:");
			Features[9].RenderAction();
			Features[10].RenderAction();
			Features[11].RenderAction();
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