using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class VisualSection : ISection
	{
		public VisualSection()
		{
			name = "Visual";
		}

		public override void Render()
		{
			Visuals.SkipShhhAnimation.Enabled = GUILayout.Toggle(Visuals.SkipShhhAnimation.Enabled, "Skip Shhh Animation");
			Visuals.AccurateDisconnectReasons.Enabled = GUILayout.Toggle(Visuals.AccurateDisconnectReasons.Enabled, "Use more accurate disconnection reasons");

			Visuals.Fullbright.Enabled = GUILayout.Toggle(Visuals.Fullbright.Enabled, "Fullbright");
			Visuals.ShowProtections.Enabled = GUILayout.Toggle(Visuals.ShowProtections.Enabled, "Show guardian angel protections");

			Chat.AlwaysVisibleChat.Enabled = GUILayout.Toggle(Chat.AlwaysVisibleChat.Enabled, "Chat is always visible");
			Chat.OnChat.ShowMessagesByGhosts = GUILayout.Toggle(Chat.OnChat.ShowMessagesByGhosts, "Show messages by ghosts");
		}
	}
}