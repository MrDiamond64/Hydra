using System;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class MenuSection : ISection
	{
		public MenuSection() : base("Menu") 
		{
			AddFeature("Disable Notifications", () => { });
			AddFeature("Primary Color", () => { });
			AddFeature("Menu Opacity", () => { });
			AddFeature("UI Scale", () => { });
		}

		public override void Render()
		{
			// GUILayout.Label($"Texture 2D memory usage: {Texture2D.currentTextureMemory}");
			Hydra.notifications.DisableNotifications = GUILayout.Toggle(Hydra.notifications.DisableNotifications, "Disable Notifications");

			GUILayout.Label($"Primary Color: {Styles.primaryColor}");
			float primaryColorVal = GUILayout.HorizontalSlider((float)Styles.primaryColor, 0, Styles.ColorValues.Count - 1);
			if (primaryColorVal != (float)Styles.primaryColor)
			{
				Styles.primaryColor = (Styles.UIColors)(int)Math.Round(primaryColorVal);
				HydraMenu.ui.Settings.Config.General.PrimaryColorIndex = (int)Styles.primaryColor;
				Styles.ClearCache();
			}

			GUILayout.Label($"Menu Opacity: {Styles.menuOpacity * 100:F0}%");
			float newOpacity = (float)Math.Round(GUILayout.HorizontalSlider(Styles.menuOpacity, 0, 1), 4);
			if (newOpacity != Styles.menuOpacity)
			{
				Styles.menuOpacity = newOpacity;
				Styles.ClearCache();
			}

			GUILayout.Label($"UI Scale: {MainUI.scale:F2}x");
			float newScale = (float)Math.Round(GUILayout.HorizontalSlider(MainUI.scale, 0.5f, 2.0f), 2);
			if (newScale != MainUI.scale)
			{
				MainUI.scale = newScale;
				HydraMenu.ui.Settings.Config.General.UIScale = newScale;
				Styles.ClearCache();
			}

			GUILayout.Space(10);
			HydraMenu.ui.Settings.Config.General.AltF9ToggleEnabled = GUILayout.Toggle(HydraMenu.ui.Settings.Config.General.AltF9ToggleEnabled, "Enable Alt+F9 Menu Toggle");

			if(GUILayout.Button("Save All Settings"))
			{
				HydraMenu.ui.Settings.Save();
				Hydra.notifications.Send("Settings", "All settings have been saved to hydra_config.cfg", 5);
			}

			if(GUILayout.Button("Apply Changes"))
			{
				Styles.ClearCache();
			}
		}
	}
}