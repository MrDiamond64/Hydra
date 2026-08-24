using BepInEx;
using HydraMenu.ui;
using System.IO;
using System.Text.Json;
using UnityEngine;

namespace HydraMenu.modules
{
	internal class ConfigManager
	{
		public string currentConfig = "Hydra";
		public string[] configList;

		private readonly string configPath = Path.Combine(Paths.ConfigPath, "Hydra");
		private string ConfigFile
		{
			get { return Path.Combine(configPath, currentConfig + ".json"); }
		}

		public class ConfigData
		{
			public MainUI.MainUIConfig Menu { get; set; }
		}

		public void Initialize()
		{
			if(!Directory.Exists(configPath))
			{
				Hydra.Log.LogInfo("No config folder was found, creating...");
				Directory.CreateDirectory(configPath);

				configList = [ currentConfig ];
				return;
			}

			string[] configFiles = Directory.GetFiles(configPath, "*.json");
			Hydra.Log.LogInfo($"Discovered {configFiles.Length} config files");

			// Load the default config
			LoadConfig(currentConfig);
		}

		public void LoadConfig(string configName)
		{
			currentConfig = configName;
			if(!File.Exists(ConfigFile))
			{
				Hydra.Log.LogWarning($"Tried to load config {configName} when no such config exists");
				// Let's just carry on with our current config
				return;
			}

			string configString = File.ReadAllText(ConfigFile);

			ConfigData configData = null;
			try
			{
				configData = JsonSerializer.Deserialize<ConfigData>(configString);
			}
			catch
			{
				Hydra.Log.LogError($"Failed to load {ConfigFile}");
				return;
			}

			Hydra.mainUI.LoadConfigData(configData.Menu);

			Hydra.Log.LogInfo($"Loaded config {configName}");
		}

		public void SaveConfig()
		{
			ConfigData configData = new ConfigData();
			configData.Menu = Hydra.mainUI.GetConfigData();

			JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
			serializerOptions.WriteIndented = true;

			string configString = JsonSerializer.Serialize(configData, serializerOptions);
			File.WriteAllText(ConfigFile,  configString);

			Hydra.Log.LogInfo($"Config {currentConfig} has been saved to {ConfigFile}");
		}
	}
}