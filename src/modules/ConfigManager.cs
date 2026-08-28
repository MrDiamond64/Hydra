using BepInEx;
using HydraMenu.anticheat;
using HydraMenu.ui;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HydraMenu.modules
{
	internal class ConfigManager
	{
		public readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "Hydra");

		public readonly List<string> configList = new List<string>();
		public string currentConfig = "Hydra";

		public class ConfigData
		{
			public MainUI.MainUIConfig Menu { get; set; }
			public Dictionary<string, Dictionary<string, JsonElement>> Modules { get; set; }
			public Dictionary<string, Dictionary<string, JsonElement>> Routines { get; set; }
			public Anticheat.AnticheatConfigData Anticheat { get; set; }
		}

		public void Initialize()
		{
			if(!Directory.Exists(CONFIG_PATH))
			{
				Hydra.Log.LogInfo("No config folder was found, creating...");
				Directory.CreateDirectory(CONFIG_PATH);

				configList.Add(currentConfig);
				return;
			}

			string[] configFiles = Directory.GetFiles(CONFIG_PATH, "*.json");
			Hydra.Log.LogInfo($"Discovered {configFiles.Length} config files");

			for(int i = 0; i < configFiles.Length; i++)
			{
				configList.Add(Path.GetFileNameWithoutExtension(configFiles[i]));
			}

			// There should always be a config named "Hydra" present
			if(!configList.Contains(currentConfig))
			{
				configList.Add(currentConfig);
			}

			// Load the default config
			LoadConfig(currentConfig);
		}

		public string GetConfigPath(string configName)
		{
			return Path.Combine(CONFIG_PATH, configName + ".json");
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

		public string GetUnusedConfigName()
		{
			HashSet<string> configHashList = configList.ToHashSet();
			string match = null;

			// https://stackoverflow.com/a/27289807
			char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
			string baseConfigName = currentConfig.TrimEnd(digits).Trim();

			for(int i = 1; i < 255; i++)
			{
				string configName = baseConfigName + " " + i;
				if(configHashList.Contains(configName)) continue;

				match = configName;
				break;
			}

			return match;
		}

		public void CreateNewConfig(string configName)
		{
			// First save our old config
			SaveConfig(currentConfig);

			// Then create our new config
			configList.Add(configName);
			SaveConfig(configName);
			currentConfig = configName;
		}
	}
}