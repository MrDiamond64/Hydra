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

			Minimap.Enabled = GUILayout.Toggle(Minimap.Enabled, "Show Players on Map (Minimap)");
			if(Minimap.Enabled)
			{
				Minimap.ShowDeadBodies = GUILayout.Toggle(Minimap.ShowDeadBodies, "    Show Dead Bodies on Map");
				Minimap.ShowTracers = GUILayout.Toggle(Minimap.ShowTracers, "    Show Movement Tracers");
				if(Minimap.ShowTracers)
				{
					Minimap.BundleTracers = GUILayout.Toggle(Minimap.BundleTracers, "    Organize Tracers");

					GUILayout.Label($"    Tracer Length: {Minimap.TrailSeconds:F0}s");
					Minimap.TrailSeconds = Mathf.Round(GUILayout.HorizontalSlider(Minimap.TrailSeconds, 5f, 120f));
				}
			}

			Chat.AlwaysVisibleChat.Enabled = GUILayout.Toggle(Chat.AlwaysVisibleChat.Enabled, "Always Visible Chat");

			Visuals.ShowGhosts.Enabled = GUILayout.Toggle(Visuals.ShowGhosts.Enabled, "Show Ghosts");
			Chat.OnChat.ShowMessagesByGhosts = GUILayout.Toggle(Chat.OnChat.ShowMessagesByGhosts, "Show messages by ghosts");
		}
	}
}