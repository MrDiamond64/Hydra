using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SelfSection : ISection
	{
		public SelfSection()
		{
			name = "Self";
		}

		private uint level = 199;

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			} else
			{
				GUILayout.Label($"Role: {PlayerControl.LocalPlayer.Data.RoleType}");
			}

			// Self.BypassIntentionalDisconnectionBlocks.Enabled = GUILayout.Toggle(Self.BypassIntentionalDisconnectionBlocks.Enabled, "Bypass intentional disconnection temp bans");
			Self.UpdateStatsFreeplay.Enabled = GUILayout.Toggle(Self.UpdateStatsFreeplay.Enabled, "Update Stats in Freeplay");
			Self.AlwaysShowTaskAnimations = GUILayout.Toggle(Self.AlwaysShowTaskAnimations, "Always Show Task Animations");
			// Self.NoLadderCooldown.enabled = GUILayout.Toggle(Self.NoLadderCooldown.enabled, "No Ladder Cooldown");
			Self.UnlimitedMeetings.enabled = GUILayout.Toggle(Self.UnlimitedMeetings.enabled, "Unlimited Meetings");

			if(GUILayout.Button("Call Meeting"))
			{
				if(AmongUsClient.Instance.AmHost)
				{
					Hydra.Log.LogInfo("We are the host, we can force a meeting");
					Utilities.OpenMeeting(PlayerControl.LocalPlayer, null);
				}
				else
				{
					PlayerControl.LocalPlayer.CmdReportDeadBody(null);
				}
			}

			if(GUILayout.Button("Randomize Avatar"))
			{
				if(AmongUsClient.Instance.AmConnected)
				{
					Utilities.RandomizePlayer(true);

					Hydra.notifications.Send("Player Randomizer", "Your avatar has been randomized for this game.", 5);
				} else
				{
					AccountManager.Instance.RandomizeName();
					Utilities.RandomizePlayer();

					Hydra.notifications.Send("Player Randomizer", "Your name and avatar has been randomized.", 5);
				}
			}

			GUILayout.Label("Task Animations:");
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Start Medbay Scan"))
			{
				Network.SendSetScanner(true);
			}

			if(GUILayout.Button("Finish Medbay Scan"))
			{
				Network.SendSetScanner(false);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Clear Asteroids"))
			{
				Network.SendPlayAnimation((byte)TaskTypes.ClearAsteroids);
			}

			if(GUILayout.Button("Empty Garbage"))
			{
				Network.SendPlayAnimation((byte)TaskTypes.EmptyGarbage);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Prime Shields"))
			{
				Network.SendPlayAnimation((byte)TaskTypes.PrimeShields);
			}

			GUILayout.Space(5);
			GUILayout.Label($"Update level to: {level + 1}");
			level = (uint)GUILayout.HorizontalSlider(level, 0, 199);

			if(GUILayout.Button("Send Level Update"))
			{
				PlayerControl.LocalPlayer.RpcSetLevel(level);
				Hydra.notifications.Send("Level Updater", $"Your level has been changed to {level + 1}", 5);
			}
		}
	}
}