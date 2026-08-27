using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.ui.sections
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
		}
	}
}