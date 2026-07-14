using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class VisualSection : ISection
	{
		public VisualSection() : base("Visual") 
		{
			AddFeature("Skip Shhh Animation", () => {
				Visuals.SkipShhhAnimation.Enabled = GUILayout.Toggle(Visuals.SkipShhhAnimation.Enabled, "Skip Shhh Animation");
			});
			AddFeature("Accurate Disconnect Reasons", () => {
				Visuals.AccurateDisconnectReasons.Enabled = GUILayout.Toggle(Visuals.AccurateDisconnectReasons.Enabled, "Use more accurate disconnection reasons");
			});
			AddFeature("Fullbright", () => {
				Visuals.Fullbright.Enabled = GUILayout.Toggle(Visuals.Fullbright.Enabled, "Fullbright");
			});
			AddFeature("Show Guardian Angel Protections", () => {
				Visuals.ShowProtections.Enabled = GUILayout.Toggle(Visuals.ShowProtections.Enabled, "Show Guardian Angel Protections");
			});
			AddFeature("Always Visible Chat", () => {
				Chat.AlwaysVisibleChat.Enabled = GUILayout.Toggle(Chat.AlwaysVisibleChat.Enabled, "Always Visible Chat");
			});
			AddFeature("Show Ghosts", () => {
				Visuals.ShowGhosts.Enabled = GUILayout.Toggle(Visuals.ShowGhosts.Enabled, "Show Ghosts");
			});
			AddFeature("Show messages by ghosts", () => {
				Chat.OnChat.ShowMessagesByGhosts = GUILayout.Toggle(Chat.OnChat.ShowMessagesByGhosts, "Show messages by ghosts");
			});
		}

		public override void Render()
		{
			foreach (var feature in Features)
			{
				feature.RenderAction();
			}
		}
	}
}