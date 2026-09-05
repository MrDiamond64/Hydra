using System;
using System.Diagnostics;
using UnityEngine;

namespace LunarMenu.ui.sections
{
	internal class MenuSection : Section
	{
		public MenuSection() : base("Menu") { }

		private byte configIndex = 0;

		public override void Render()
		{
			// GUILayout.Label($"Texture 2D memory usage: {Texture2D.currentTextureMemory}");
			Lunar.notifications.disableNotifications = GUILayout.Toggle(Lunar.notifications.disableNotifications, "Disable Notifications");

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
				Lunar.Eject();
			}

			GUILayout.Space(5);
			GUILayout.Label($"Config:\nCurrent Config: {Lunar.config.currentConfig}");

			GUILayout.Label($"Selected Config: {Lunar.config.configList[configIndex]}");
			configIndex = (byte)GUILayout.HorizontalSlider(configIndex, 0, Lunar.config.configList.Count - 1);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Save"))
			{
				Lunar.config.SaveConfig(Lunar.config.configList[configIndex]);
			}

			if(GUILayout.Button("Load"))
			{
				Lunar.config.LoadConfig(Lunar.config.configList[configIndex]);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("New Config"))
			{
				string configName = Lunar.config.GetUnusedConfigName();
				// I doubt anyone will actually have 255 configs with the pattern of "Lunar [1-255]", but just in case...
				if(configName == null)
				{
					Lunar.notifications.Send("Config", "Failed to find an unused config name.");
					return;
				}

				Lunar.config.CreateNewConfig(configName);
			}

			if(GUILayout.Button("Open Config Folder"))
			{
				Process.Start("explorer.exe", Lunar.config.CONFIG_PATH);
			}
			GUILayout.EndHorizontal();
		}
	}
}