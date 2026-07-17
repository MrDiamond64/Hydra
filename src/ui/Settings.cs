using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BepInEx;
using System.Linq;

namespace HydraMenu.ui
{
    public static class Settings
    {
        public static ConfigData Config = new ConfigData();
        private static string configPath = Path.Combine(Paths.ConfigPath, "hydra_config.cfg");
        private static string presetsFolder = Path.Combine(Paths.ConfigPath, "Presets");

        public static void Save()
        {
            try 
            {
                string json = System.Text.Json.JsonSerializer.Serialize(Config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
                Hydra.Log.LogInfo($"[Settings] Successfully saved configuration to: {configPath}");
            }
            catch (Exception e)
            {
                Hydra.Log.LogError($"[Settings] Failed to save settings to {configPath}: {e.Message}");
            }
        }

        public static void Load()
        {
            if (!File.Exists(configPath)) 
            {
                Hydra.Log.LogInfo($"[Settings] No config file found at {configPath}, using defaults.");
                return;
            }
            try
            {
                string json = File.ReadAllText(configPath);
                Config = System.Text.Json.JsonSerializer.Deserialize<ConfigData>(json);
                Hydra.Log.LogInfo($"[Settings] Successfully loaded configuration from: {configPath}");
            }
            catch (Exception e)
            {
                Hydra.Log.LogError($"[Settings] Failed to load settings from {configPath}: {e.Message}");
            }
        }

        public static void SavePreset(string name)
        {
            try
            {
                if (!Directory.Exists(presetsFolder)) Directory.CreateDirectory(presetsFolder);
                string path = Path.Combine(presetsFolder, $"{name}.json");
                string json = System.Text.Json.JsonSerializer.Serialize(Config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[Hydra] Failed to save preset: {e.Message}");
            }
        }

        public static void LoadPreset(string name)
        {
            try
            {
                string path = Path.Combine(presetsFolder, $"{name}.json");
                if (!File.Exists(path)) return;
                string json = File.ReadAllText(path);
                Config = System.Text.Json.JsonSerializer.Deserialize<ConfigData>(json);
                Save(); // Persist the loaded preset to main config
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[Hydra] Failed to load preset: {e.Message}");
            }
        }

        public static List<string> GetPresets()
        {
            if (!Directory.Exists(presetsFolder)) return new List<string>();
            return Directory.GetFiles(presetsFolder, "*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .ToList();
        }
    }

    [Serializable]
    public class ConfigData
    {
        public KeyBinds Binds { get; set; } = new KeyBinds();
        public GeneralSettings General { get; set; } = new GeneralSettings();
        public FeatureSettings Features { get; set; } = new FeatureSettings();
    }

    [Serializable]
    public class KeyBinds
    {
        public KeyCode ToggleMenu { get; set; } = KeyCode.Insert;
        public KeyCode NoClip { get; set; } = KeyCode.None;
        public KeyCode KillAll { get; set; } = KeyCode.None;
        public KeyCode CloseAllDoors { get; set; } = KeyCode.None;
    }

    [Serializable]
    public class GeneralSettings
    {
        public bool AltF9ToggleEnabled { get; set; } = true;
        public bool ShowKillCooldown { get; set; } = false;
        public bool ShowImpostors { get; set; } = true;
        public bool RevealRoles { get; set; } = true;
        public bool FullbrightEnabled { get; set; } = false;
        public bool ShowProtectionsEnabled { get; set; } = true;
        public bool AccurateDisconnectReasonsEnabled { get; set; } = true;
        public bool SkipShhhAnimationEnabled { get; set; } = true;
        public float MenuOpacity { get; set; } = 0.85f;
        public int PrimaryColorIndex { get; set; } = 0;
        public float UIScale { get; set; } = 1.0f;
    }

    [Serializable]
    public class FeatureSettings
    {
        // Anticheat
        public bool AnticheatEnabled { get; set; } = true;
        public bool CheckSpoofedPlatforms { get; set; } = true;

        // Host
        public bool CustomImpostorAmountEnabled { get; set; } = false;
        public int CustomImpostorAmount { get; set; } = 3;
        public bool DisableMeetings { get; set; } = false;
        public bool DisableSabotages { get; set; } = false;
        public bool DisableCloseDoors { get; set; } = false;
        public bool DisableGameEnd { get; set; } = false;
        public bool NoKillCooldown { get; set; } = false;

        // Protections
        public bool BlockLargeGameMessages { get; set; } = true;
        public bool BlockInvalidGameDataMessages { get; set; } = true;
        public bool BlockUnauthorizedSystemUpdates { get; set; } = true;
        public bool ProtectAgainstNonHostKickExploit { get; set; } = true;
        public bool ForceDTLSEnabled { get; set; } = true;
        public bool BlockServerTeleportsEnabled { get; set; } = true;

        // Roles
        public bool DisableShapeshiftAnimation { get; set; } = false;
        public bool AllowVentingForCrewmates { get; set; } = true;
        public bool SabotageAsCrewmate { get; set; } = false;
        public bool SabotageInVents { get; set; } = false;
        public bool NoKillChecks { get; set; } = false;

        // Self
        public bool AlwaysShowTaskAnimations { get; set; } = true;
        public bool UpdateStatsFreeplay { get; set; } = false;

        // Spoofer
        public bool ShouldSpoofVersion { get; set; } = false;
        public int SpoofedVersion { get; set; } = 0;
        public bool UseModdedProtocol { get; set; } = false;
        public Platforms spoofedPlatform { get; set; } = Constants.GetPlatformType();
        public bool AvoidPenalties { get; set; } = true;

        // Troll
        public bool AutoReportBodiesEnabled { get; set; } = false;
        public bool BlockVentingEnabled { get; set; } = false;
        public bool BlockSabotagesEnabled { get; set; } = false;
    }
}
