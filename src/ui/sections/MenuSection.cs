using System;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class MenuSection : Section
	{
		public MenuSection() : base("Menu") { }

		private byte configIndex = 0;

		public override void Render()
		{
			// GUILayout.Label($"Texture 2D memory usage: {Texture2D.currentTextureMemory}");
			Hydra.notifications.DisableNotifications = GUILayout.Toggle(Hydra.notifications.DisableNotifications, "Disable Notifications");

			GUILayout.Label($"Primary Color: {Styles.primaryColor}");
			Styles.primaryColor = (Styles.UIColors)GUILayout.HorizontalSlider((float)Styles.primaryColor, 0, Styles.ColorValues.Count - 1);

			GUILayout.Label($"Menu Opacity: {Styles.menuOpacity * 100:F0}%");
			Styles.menuOpacity = (float)Math.Round(GUILayout.HorizontalSlider(Styles.menuOpacity, 0, 1), 4);

			GUILayout.Label($"UI Scale: {MainUI.scale:F2}x");
			MainUI.scale = (float)Math.Round(GUILayout.HorizontalSlider(MainUI.scale, 0.5f, 2.0f), 2);

			if(GUILayout.Button("Apply Changes"))
			{
				Styles.ClearCache();
			}

			if(GUILayout.Button("Eject"))
			{
				Hydra.Eject();
			}

			GUILayout.Space(5);
			GUILayout.Label($"Config:\nCurrent Config: {Hydra.config.currentConfig}");

			GUILayout.Label($"Selected Config: {Hydra.config.configList[configIndex]}");
			configIndex = (byte)GUILayout.HorizontalSlider(configIndex, 0, Hydra.config.configList.Length - 1);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Load"))
			{
				Hydra.config.LoadConfig(Hydra.config.configList[configIndex]);
			}

			if(GUILayout.Button("Save"))
			{
				Hydra.config.SaveConfig(Hydra.config.configList[configIndex]);
			}
			GUILayout.EndHorizontal();
		}
	}
}