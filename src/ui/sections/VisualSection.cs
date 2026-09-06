using LunarMenu.modules;
using UnityEngine;

namespace LunarMenu.ui.sections
{
	internal class VisualSection : Section
	{
		public VisualSection() : base("Visual") { }

		public override void Render()
		{
			ModuleManager.skipShhhAnimation.Enabled = GUILayout.Toggle(ModuleManager.skipShhhAnimation.Enabled, "Skip Shhh Animation");
			ModuleManager.noSeekerAnimation.Enabled = GUILayout.Toggle(ModuleManager.noSeekerAnimation.Enabled, "Skip Seeker Animation");
			ModuleManager.accurateDisconnectReason.Enabled = GUILayout.Toggle(ModuleManager.accurateDisconnectReason.Enabled, "Use more accurate disconnection reasons");

			ModuleManager.fullbright.Enabled = GUILayout.Toggle(ModuleManager.fullbright.Enabled, "Fullbright");
			ModuleManager.showProtections.Enabled = GUILayout.Toggle(ModuleManager.showProtections.Enabled, "Show Guardian Angel Protections");

			ModuleManager.alwaysVisibleChat.Enabled = GUILayout.Toggle(ModuleManager.alwaysVisibleChat.Enabled, "Always Visible Chat");

			ModuleManager.showGhosts.Enabled = GUILayout.Toggle(ModuleManager.showGhosts.Enabled, "Show Ghosts");
			ModuleManager.showGhostMessages.Enabled = GUILayout.Toggle(ModuleManager.showGhostMessages.Enabled, "Show messages by ghosts");

            ModuleManager.revealRoles.Enabled = GUILayout.Toggle(ModuleManager.revealRoles.Enabled, "Show Player Roles");
            ModuleManager.revealRoles.ShowKillCD = GUILayout.Toggle(ModuleManager.revealRoles.ShowKillCD, "Show Kill Cooldown");
            ModuleManager.revealRoles.ShowPlayerInfo = GUILayout.Toggle(ModuleManager.revealRoles.ShowPlayerInfo, "Show Player Info");

            ModuleManager.revealVotes.Enabled = GUILayout.Toggle(ModuleManager.revealVotes.Enabled, "Reveal Votes");
            ModuleManager.revealVotes.RevealAnonymousVotes = GUILayout.Toggle(ModuleManager.revealVotes.RevealAnonymousVotes, "Reveal Anonymous Votes");

            ModuleManager.showFPS.Enabled = GUILayout.Toggle(ModuleManager.showFPS.Enabled, "Show FPS");
            ModuleManager.showFPS.ShowHost = GUILayout.Toggle(ModuleManager.showFPS.ShowHost, "Show Lobby Host");
            ModuleManager.showFPS.ShowVoteKicks = GUILayout.Toggle(ModuleManager.showFPS.ShowVoteKicks, "Show Votekicks");

            ModuleManager.showLobbyInfo.Enabled = GUILayout.Toggle(ModuleManager.showLobbyInfo.Enabled, "Show Lobby Info");

            ModuleManager.showPhantoms.Enabled = GUILayout.Toggle(ModuleManager.showPhantoms.Enabled, "Show Vanished Players");
            ModuleManager.showPlayersInVents.Enabled = GUILayout.Toggle(ModuleManager.showPlayersInVents.Enabled, "Show Players in Vents");

            ModuleManager.showLobbyInfo.Enabled = GUILayout.Toggle(ModuleManager.showLobbyInfo.Enabled, "Show Lobby Info");

			ModuleManager.zoomOut.Enabled = GUILayout.Toggle(ModuleManager.zoomOut.Enabled, "Zoom Out");
        }
	}
}