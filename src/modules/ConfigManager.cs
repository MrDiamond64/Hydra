using System.IO;
using System.Text.Json;
using UnityEngine;

namespace HydraMenu.modules
{
	internal class ConfigManager
	{
		public string currentConfig = "Hydra";
		public string[] configList;

		// Base path is the location of GameAssembly.dll
		private readonly string configPath = "./BepInEx/config/Hydra/";
		private string ConfigFile
		{
			get { return  configPath + currentConfig + ".json"; }
		}

		public class ConfigData
		{
			public KeyCode MenuKey { get; set; }
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

			// If our config file has no MenuKey property then JsonSerializer will default to None (0)
			if(configData.MenuKey != KeyCode.None)
			{
				Hydra.mainUI.menuKey = configData.MenuKey;
			}

			Hydra.Log.LogInfo($"Loaded config {configName}");
		}

		public void SaveConfig()
		{
			ConfigData configData = new ConfigData();
			configData.MenuKey = Hydra.mainUI.menuKey;

			JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
			serializerOptions.WriteIndented = true;

			string configString = JsonSerializer.Serialize(configData, serializerOptions);
			File.WriteAllText(ConfigFile,  configString);

			Hydra.Log.LogInfo($"Config {currentConfig} has been saved to {ConfigFile}");
		}
	}
}