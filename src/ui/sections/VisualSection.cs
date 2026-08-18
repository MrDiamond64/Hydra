using HydraMenu.features;
using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class VisualSection : ISection
	{
		public VisualSection() : base("Visual") { }

		public override void Render()
		{
			Visuals.SkipShhhAnimation.Enabled = GUILayout.Toggle(Visuals.SkipShhhAnimation.Enabled, "Skip Shhh Animation");
			Visuals.NoSeekerAnimationPatch.Enabled = GUILayout.Toggle(Visuals.NoSeekerAnimationPatch.Enabled, "Skip Seeker Animation");
			Visuals.AccurateDisconnectReasons.Enabled = GUILayout.Toggle(Visuals.AccurateDisconnectReasons.Enabled, "Use more accurate disconnection reasons");

			Visuals.Fullbright.Enabled = GUILayout.Toggle(Visuals.Fullbright.Enabled, "Fullbright");
			Visuals.ShowProtections.Enabled = GUILayout.Toggle(Visuals.ShowProtections.Enabled, "Show Guardian Angel Protections");

			ModuleManager.alwaysVisibleChat.Enabled = GUILayout.Toggle(ModuleManager.alwaysVisibleChat.Enabled, "Always Visible Chat");

			Visuals.ShowGhosts.Enabled = GUILayout.Toggle(Visuals.ShowGhosts.Enabled, "Show Ghosts");
			ModuleManager.showGhostMessages.Enabled = GUILayout.Toggle(ModuleManager.showGhostMessages.Enabled, "Show messages by ghosts");
		}
	}
}