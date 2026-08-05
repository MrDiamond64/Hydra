using HydraMenu.features;
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

			Chat.AlwaysVisibleChat.Enabled = GUILayout.Toggle(Chat.AlwaysVisibleChat.Enabled, "Always Visible Chat");

			Visuals.ShowGhosts.Enabled = GUILayout.Toggle(Visuals.ShowGhosts.Enabled, "Show Ghosts");
			Chat.OnChat.ShowMessagesByGhosts = GUILayout.Toggle(Chat.OnChat.ShowMessagesByGhosts, "Show messages by ghosts");

			GUILayout.Space(5);
			Visuals.CameraZoom.Enabled = GUILayout.Toggle(Visuals.CameraZoom.Enabled, "Camera Zoom-Out");
			if(Visuals.CameraZoom.Enabled)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Zoom Scale: {Visuals.CameraZoom.ZoomFactor:F1}x");
				Visuals.CameraZoom.ZoomFactor = GUILayout.HorizontalSlider(Visuals.CameraZoom.ZoomFactor, 0.5f, 5.0f);
				GUILayout.EndHorizontal();
			}
		}
	}
}