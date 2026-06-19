using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.features;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SelfSection : ISection
	{
		public SelfSection() : base("Self")
		{
			usernameInput = new("Edit Username");
		}

		private uint level = 199;
		private Controls.TextInput usernameInput;
		private bool isNameReady = false;

		public override void Render()
		{
			if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
				isNameReady = false;
			}
			else
			{
				if (!isNameReady)
				{
					usernameInput.contents = PlayerControl.LocalPlayer.Data.PlayerName;
					isNameReady = true;
				}
				GUILayout.Label($"Role: {PlayerControl.LocalPlayer.Data.RoleType}");
			}

			// Self.BypassIntentionalDisconnectionBlocks.Enabled = GUILayout.Toggle(Self.BypassIntentionalDisconnectionBlocks.Enabled, "Bypass intentional disconnection temp bans");
			Self.UpdateStatsFreeplay.Enabled = GUILayout.Toggle(Self.UpdateStatsFreeplay.Enabled, "Update Stats in Freeplay");
			Immortality.Enabled = GUILayout.Toggle(Immortality.Enabled, "Become Immortal");
			Self.AlwaysShowTaskAnimations = GUILayout.Toggle(Self.AlwaysShowTaskAnimations, "Always Show Task Animations");
			Self.NoLadderCooldown.Enabled = GUILayout.Toggle(Self.NoLadderCooldown.Enabled, "No Ladder Cooldown");
			Self.UnlimitedMeetings.enabled = GUILayout.Toggle(Self.UnlimitedMeetings.enabled, "Unlimited Meetings");
			Self.UseBypassRpc = GUILayout.Toggle(Self.UseBypassRpc, "Bypass RPC");

			if (GUILayout.Button("Call Meeting"))
			{
				Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, null);
			}

			if (GUILayout.Button("Complete All Tasks"))
			{
				PlayerControl.LocalPlayer.StartCoroutine(CompleteAllTasks().WrapToIl2Cpp());
			}

			GUILayout.Label("Task Animations:");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Start Medbay Scan"))
			{
				Network.SendSetScanner(true);
			}

			if (GUILayout.Button("Finish Medbay Scan"))
			{
				Network.SendSetScanner(false);
			}
			GUILayout.EndHorizontal();

			Dictionary<string, TaskTypes> animations = MapAssets.GetAnimations();
			Controls.DrawButtonCell(animations, PlayAnimation, 2);

			GUILayout.Space(5);
			GUILayout.Label("Avatar Controls:");
			if (GUILayout.Button("Randomize Avatar"))
			{
				if (AmongUsClient.Instance.AmConnected)
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

			if (GUILayout.Button("Randomize Color"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());
			}

			if (GUILayout.Button("Restore Avatar"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor(DataManager.Player.Customization.Color);
				PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
				PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
				PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
				PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
			}

			GUILayout.Space(5);
			GUILayout.Label($"Update level to: {level + 1}");
			level = (uint)GUILayout.HorizontalSlider(level, 0, 199);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("-100"))
			{
				ClampSelectedLevel(level - 100);
			}

			if (GUILayout.Button("-10"))
			{
				ClampSelectedLevel(level - 10);
			}

			if (GUILayout.Button("+10"))
			{
				ClampSelectedLevel(level + 10);
			}

			if (GUILayout.Button("+100"))
			{
				ClampSelectedLevel(level + 100);
			}
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Send Level Update"))
			{
				PlayerControl.LocalPlayer.RpcSetLevel(level);
				Hydra.notifications.Send("Level Updater", $"Your level has been changed to {level + 1}", 5);
			}

			GUILayout.Space(5);
			GUILayout.BeginHorizontal();

			usernameInput.Render();

			if (GUILayout.Button("Set Username"))
			{
				Network.BatchedMessage bm = new();
				bm.UseAnticheatBypass();
				bm.QueueSetName(PlayerControl.LocalPlayer, usernameInput.contents);
				bm.FinishBatch();
			}
		}

		public IEnumerator CompleteAllTasks()
		{
			Il2CppSystem.Collections.Generic.List<PlayerTask> allTasks = PlayerControl.LocalPlayer.myTasks;

			Hydra.Log.LogInfo("Completing all tasks...");
			foreach (PlayerTask task in allTasks)
			{
				if (task.IsComplete)
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
			if (PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("Play Animation", "This option can only be used inside of a game.");
				return;
			}

			if (ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Play Animation", "There must be an instance of ShipStatus for this feature to work.");
				return;
			}

			Network.SendPlayAnimation((byte)task);
		}

		private void ClampSelectedLevel(uint newLevel)
		{
			// Do we really need to have an upper bounds on the level value?
			// I doubt anyone will press the +100 that much anyway
			uint maxLevel = Utilities.IsAnticheatPresent() ? 100001 : uint.MaxValue - 1;

			level = Math.Clamp(newLevel, 0, maxLevel);
		}
	}
}