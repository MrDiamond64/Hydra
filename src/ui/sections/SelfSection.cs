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
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			} else
			{
				GUILayout.Label($"Role: {PlayerControl.LocalPlayer.Data.RoleType}");
			}

			// Self.BypassIntentionalDisconnectionBlocks.Enabled = GUILayout.Toggle(Self.BypassIntentionalDisconnectionBlocks.Enabled, "Bypass intentional disconnection temp bans");
			Self.UpdateStatsFreeplay.Enabled = GUILayout.Toggle(Self.UpdateStatsFreeplay.Enabled, "Update Stats in Freeplay");
			Self.AlwaysDoScanAnimation.Enabled = GUILayout.Toggle(Self.AlwaysDoScanAnimation.Enabled, "Always Show Medbay Scan");

			if(GUILayout.Button("Call Meeting"))
			{
				PlayerControl.LocalPlayer.CmdReportDeadBody(null);
			}

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