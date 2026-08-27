using BepInEx;
using HydraMenu.anticheat;
using HydraMenu.ui;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HydraMenu.modules
{
	internal class ConfigManager
	{
		public readonly string configPath = Path.Combine(Paths.ConfigPath, "Hydra");

		public string currentConfig = "Hydra";
		public string[] configList = [];

		public class ConfigData
		{
			public MainUI.MainUIConfig Menu { get; set; }
			public Dictionary<string, Dictionary<string, JsonElement>> Modules { get; set; }
			public Dictionary<string, Dictionary<string, JsonElement>> Routines { get; set; }
			public Anticheat.AnticheatConfigData Anticheat { get; set; }
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

			configList = new string[configFiles.Length];

			for(byte i = 0; i < configFiles.Length; i++)
			{
				configList[i] = Path.GetFileNameWithoutExtension(configFiles[i]);
			}

			if(configList.Length == 0)
			{
				configList = ["Hydra"];
			}

			// Load the default config
			LoadConfig(currentConfig);
		}

		public string GetConfigPath(string configName)
		{
			return Path.Combine(configPath, configName + ".json");
		}

		public void LoadConfig(string configName)
		{
			string configLocation = GetConfigPath(configName);
			if(!File.Exists(configLocation))
			{
				Hydra.Log.LogWarning($"Tried to load config {configName} when no such config exists");
				// Let's just carry on with our current config
				return;
			}

			string configString = File.ReadAllText(configLocation);

			ConfigData configData = null;
			try
			{
				configData = JsonSerializer.Deserialize<ConfigData>(configString);
			}
			catch
			{
				Hydra.Log.LogError($"Failed to load config at {configLocation}");
				return;
			}

			Hydra.mainUI.LoadConfigData(configData.Menu);
			Hydra.modules.LoadConfigData(configData.Modules);
			Hydra.routines.LoadConfigData(configData.Routines);
			Anticheat.LoadConfigData(configData.Anticheat);

			currentConfig = configName;
			Hydra.Log.LogInfo($"Loaded config {configName}");
		}

		public void SaveConfig(string configName)
		{
			string configLocation = GetConfigPath(configName);

			ConfigData configData = new ConfigData();
			configData.Menu = Hydra.mainUI.GetConfigData();
			configData.Modules = Hydra.modules.GetConfigData();
			configData.Routines = Hydra.routines.GetConfigData();
			configData.Anticheat = Anticheat.GetConfigData();

			JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
			serializerOptions.WriteIndented = true;

			string configString = JsonSerializer.Serialize(configData, serializerOptions);
			File.WriteAllText(configLocation, configString);

			Hydra.Log.LogInfo($"Config {configName} has been saved to {configLocation}");
		}
	}
}