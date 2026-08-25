using System;
using System.IO;
using System.Text.Json;
using UnityEngine;
using AmongUs.GameOptions;

namespace HydraMenu
{
    public class ConfigData
    {
        public float Scale { get; set; } = 1.0f;
        public float MenuOpacity { get; set; } = 1.0f;
        public int PrimaryColor { get; set; } = 0;
        public bool DisableNotifications { get; set; } = false;

        public bool AlwaysShowTaskAnimations { get; set; } = true;
        public bool UpdateStatsFreeplay { get; set; } = false;
        public bool NoLadderCooldown { get; set; } = true;
        public bool UnlimitedMeetings { get; set; } = true;
        public float PlayerSpeedModifier { get; set; } = 1.0f;

        public bool Fullbright { get; set; } = false;
        public bool ShowProtections { get; set; } = true;
        public bool SkipShhhAnimation { get; set; } = true;
        public bool SkipSeekerAnimation { get; set; } = true;
        public bool AccurateDisconnectReasons { get; set; } = true;
        public bool ShowGhosts { get; set; } = true;
        public bool AlwaysVisibleChat { get; set; } = false;
        public bool LogChatMessages { get; set; } = false;
        public bool ShowMessagesByGhosts { get; set; } = false;

        public bool BanMidGame { get; set; } = false;
        public bool FlippedSkeld { get; set; } = false;
        public bool DisableSabotages { get; set; } = false;
        public bool DisableCloseDoors { get; set; } = false;
        public bool DisableCameras { get; set; } = false;
        public bool DisableGameEnd { get; set; } = false;
        public bool NoKillCooldown { get; set; } = false;
        public bool BlockLowLevels { get; set; } = false;
        public uint BlockLowLevelsMinLevel { get; set; } = 0;
        public bool AlwaysImposter { get; set; } = false;
        public bool DisableMeetings { get; set; } = false;
        public bool ReportBodySpam { get; set; } = false;

        public bool Immortality { get; set; } = false;

        public bool TeleporterUseSnapToRPC { get; set; } = true;

        public bool TrollAutoReportBodies { get; set; } = false;
        public bool TrollBlockVenting { get; set; } = false;
        public bool TrollBlockSabotages { get; set; } = false;

        public bool SpooferShouldSpoofVersion { get; set; } = false;
        public int SpooferSpoofedVersion { get; set; } = 0;
        public bool SpooferUseModdedProtocol { get; set; } = false;
        public int SpooferSpoofedPlatform { get; set; } = 0;
        public bool SpooferSpoofLevelEnabled { get; set; } = false;
        public uint SpooferNewLevel { get; set; } = 200;

        public bool ProtectionsForceDTLS { get; set; } = true;
        public bool ProtectionsBlockServerTeleports { get; set; } = true;
        public bool ProtectionsBlockUnauthorizedSystemUpdates { get; set; } = true;
        public bool ProtectionsBlockLargeGameMessages { get; set; } = true;
        public bool ProtectionsBlockInvalidGameDataMessages { get; set; } = true;
        public bool ProtectionsHardenedReadPackedUInt { get; set; } = true;
        public bool ProtectionsMemoryAllocationOverload { get; set; } = true;
        public bool ProtectionsBypassShapeshiftRatelimits { get; set; } = true;
        public bool ProtectionsVotekicks { get; set; } = true;
        public bool ProtectionsProtectAgainstNonHostKickExploit { get; set; } = true;

        public bool AnticheatEnabled { get; set; } = true;
        public bool AnticheatCheckSpoofedPlatforms { get; set; } = true;
        public bool AnticheatSendNotification { get; set; } = true;
        public bool AnticheatDiscardRpc { get; set; } = true;
        public int AnticheatPunishment { get; set; } = 0;

        public bool RolesDisableShapeshiftAnimation { get; set; } = false;
        public bool RolesAllowVentingForCrewmates { get; set; } = true;
        public bool RolesNoKillChecks { get; set; } = false;
        public bool RolesSabotageAsCrewmate { get; set; } = false;
        public bool RolesSabotageInVents { get; set; } = false;
        public bool RolesMoveInVents { get; set; } = true;

        public bool SabotageUpdateSystemsDirectly { get; set; } = true;
        public bool SpectatePlayerEnabled { get; set; } = false;
        public int AlwaysImposterAssignedRole { get; set; } = 0;
        public bool SelfUnlimitedMeetingsEnabled { get; set; } = true;
    }

    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(BepInEx.Paths.ConfigPath, "HydraMenu.json");

        public static void Save()
        {
            try
            {
                ConfigData data = new ConfigData
                {
                    Scale = ui.MainUI.scale,
                    MenuOpacity = ui.Styles.menuOpacity,
                    PrimaryColor = (int)ui.Styles.primaryColor,
                    DisableNotifications = Hydra.notifications.DisableNotifications,

                    AlwaysShowTaskAnimations = features.Self.AlwaysShowTaskAnimations,
                    UpdateStatsFreeplay = features.Self.UpdateStatsFreeplay.Enabled,
                    NoLadderCooldown = features.Self.NoLadderCooldown.Enabled,
                    UnlimitedMeetings = features.Self.UnlimitedMeetings.enabled,
                    PlayerSpeedModifier = features.Self.PlayerSpeedModifier.Multiplier,

                    Fullbright = features.Visuals.Fullbright.Enabled,
                    ShowProtections = features.Visuals.ShowProtections.Enabled,
                    SkipShhhAnimation = features.Visuals.SkipShhhAnimation.Enabled,
                    SkipSeekerAnimation = features.Visuals.NoSeekerAnimationPatch.Enabled,
                    AccurateDisconnectReasons = features.Visuals.AccurateDisconnectReasons.Enabled,
                    ShowGhosts = features.Visuals.ShowGhosts.Enabled,
                    AlwaysVisibleChat = features.Chat.AlwaysVisibleChat.Enabled,
                    LogChatMessages = features.Chat.OnChat.LogChatMessages,
                    ShowMessagesByGhosts = features.Chat.OnChat.ShowMessagesByGhosts,

                    BanMidGame = features.Host.BanMidGame.Enabled,
                    FlippedSkeld = features.Host.FlippedSkeld,
                    DisableSabotages = features.Host.DisableSabotages.Enabled,
                    DisableCloseDoors = features.Host.DisableCloseDoors.Enabled,
                    DisableCameras = features.Host.DisableCameras.Enabled,
                    DisableGameEnd = features.Host.DisableGameEnd.Enabled,
                    NoKillCooldown = features.Host.NoKillCooldown.Enabled,
                    BlockLowLevels = features.Host.BlockLowLevels.Enabled,
                    BlockLowLevelsMinLevel = features.Host.BlockLowLevels.MinLevel,
                    AlwaysImposter = features.Host.AlwaysImposter.Enabled,
                    DisableMeetings = features.Host.DisableMeetings.Enabled,
                    ReportBodySpam = Hydra.routines.reportBodySpam.Enabled,

                    Immortality = features.Immortality.Enabled,

                    TeleporterUseSnapToRPC = Teleporter.UseSnapToRPC,

                    TrollAutoReportBodies = features.Troll.AutoReportBodies.Enabled,
                    TrollBlockVenting = features.Troll.BlockVenting.Enabled,
                    TrollBlockSabotages = features.Troll.BlockSabotages.Enabled,

                    SpooferShouldSpoofVersion = features.Spoofer.shouldSpoofVersion,
                    SpooferSpoofedVersion = features.Spoofer.spoofedVersion,
                    SpooferUseModdedProtocol = features.Spoofer.useModdedProtocol,
                    SpooferSpoofedPlatform = (int)features.Spoofer.spoofedPlatform,
                    SpooferSpoofLevelEnabled = features.Spoofer.SpoofLevel.Enabled,
                    SpooferNewLevel = features.Spoofer.SpoofLevel.newLevel,

                    ProtectionsForceDTLS = features.Protections.ForceDTLS.Enabled,
                    ProtectionsBlockServerTeleports = features.Protections.BlockServerTeleports.Enabled,
                    ProtectionsBlockUnauthorizedSystemUpdates = features.Protections.BlockUnauthorizedSystemUpdates,
                    ProtectionsBlockLargeGameMessages = features.Protections.BlockLargeGameMessages,
                    ProtectionsBlockInvalidGameDataMessages = features.Protections.BlockInvalidGameDataMessages,
                    ProtectionsHardenedReadPackedUInt = features.Protections.HardenedReadPackedUInt.Enabled,
                    ProtectionsMemoryAllocationOverload = features.Protections.MemoryAllocationOverload.Enabled,
                    ProtectionsBypassShapeshiftRatelimits = features.Protections.BypassShapeshiftRatelimits.Enabled,
                    ProtectionsVotekicks = features.Protections.Votekicks.Enabled,
                    ProtectionsProtectAgainstNonHostKickExploit = features.Protections.ProtectAgainstNonHostKickExploit,

                    AnticheatEnabled = anticheat.Anticheat.Enabled,
                    AnticheatCheckSpoofedPlatforms = anticheat.Anticheat.CheckSpoofedPlatforms,
                    AnticheatSendNotification = anticheat.Anticheat.sendNotification,
                    AnticheatDiscardRpc = anticheat.Anticheat.discardRpc,
                    AnticheatPunishment = (int)anticheat.Anticheat.punishment,

                    RolesDisableShapeshiftAnimation = features.Roles.DisableShapeshiftAnimation,
                    RolesAllowVentingForCrewmates = features.Roles.AllowVentingForCrewmates,
                    RolesNoKillChecks = features.Roles.NoKillChecks,
                    RolesSabotageAsCrewmate = features.Roles.SkipSabotageChecks.SabotageAsCrewmate,
                    RolesSabotageInVents = features.Roles.SkipSabotageChecks.SabotageInVents,
                    RolesMoveInVents = features.Roles.MoveModifier.MoveInVents,

                    SabotageUpdateSystemsDirectly = Sabotage.UpdateSystemsDirectly,
                    SpectatePlayerEnabled = features.Visuals.SpectatePlayer.Enabled,
                    AlwaysImposterAssignedRole = (int)features.Host.AlwaysImposter.assignedRole,
                    SelfUnlimitedMeetingsEnabled = features.Self.UnlimitedMeetings.enabled,
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
                Hydra.Log.LogInfo("Config saved to " + ConfigPath);
            }
            catch (Exception ex)
            {
                Hydra.Log.LogError("Failed to save config: " + ex.Message);
            }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    Hydra.Log.LogInfo("No config file found at " + ConfigPath);
                    return;
                }

                string json = File.ReadAllText(ConfigPath);
                ConfigData data = JsonSerializer.Deserialize<ConfigData>(json);

                if (data == null) return;

                ui.MainUI.scale = data.Scale;
                ui.Styles.menuOpacity = data.MenuOpacity;
                ui.Styles.primaryColor = (ui.Styles.UIColors)data.PrimaryColor;
                Hydra.notifications.DisableNotifications = data.DisableNotifications;

                features.Self.AlwaysShowTaskAnimations = data.AlwaysShowTaskAnimations;
                features.Self.UpdateStatsFreeplay.Enabled = data.UpdateStatsFreeplay;
                features.Self.NoLadderCooldown.Enabled = data.NoLadderCooldown;
                features.Self.UnlimitedMeetings.enabled = data.UnlimitedMeetings;
                features.Self.PlayerSpeedModifier.Multiplier = data.PlayerSpeedModifier;

                features.Visuals.Fullbright.Enabled = data.Fullbright;
                features.Visuals.ShowProtections.Enabled = data.ShowProtections;
                features.Visuals.SkipShhhAnimation.Enabled = data.SkipShhhAnimation;
                features.Visuals.NoSeekerAnimationPatch.Enabled = data.SkipSeekerAnimation;
                features.Visuals.AccurateDisconnectReasons.Enabled = data.AccurateDisconnectReasons;
                features.Visuals.ShowGhosts.Enabled = data.ShowGhosts;
                features.Chat.AlwaysVisibleChat.Enabled = data.AlwaysVisibleChat;
                features.Chat.OnChat.LogChatMessages = data.LogChatMessages;
                features.Chat.OnChat.ShowMessagesByGhosts = data.ShowMessagesByGhosts;

                features.Host.BanMidGame.Enabled = data.BanMidGame;
                features.Host.FlippedSkeld = data.FlippedSkeld;
                features.Host.DisableSabotages.Enabled = data.DisableSabotages;
                features.Host.DisableCloseDoors.Enabled = data.DisableCloseDoors;
                features.Host.DisableCameras.Enabled = data.DisableCameras;
                features.Host.DisableGameEnd.Enabled = data.DisableGameEnd;
                features.Host.NoKillCooldown.Enabled = data.NoKillCooldown;
                features.Host.BlockLowLevels.Enabled = data.BlockLowLevels;
                features.Host.BlockLowLevels.MinLevel = data.BlockLowLevelsMinLevel;
                features.Host.AlwaysImposter.Enabled = data.AlwaysImposter;
                features.Host.DisableMeetings.Enabled = data.DisableMeetings;
                Hydra.routines.reportBodySpam.Enabled = data.ReportBodySpam;

                features.Immortality.Enabled = data.Immortality;

                Teleporter.UseSnapToRPC = data.TeleporterUseSnapToRPC;

                features.Troll.AutoReportBodies.Enabled = data.TrollAutoReportBodies;
                features.Troll.BlockVenting.Enabled = data.TrollBlockVenting;
                features.Troll.BlockSabotages.Enabled = data.TrollBlockSabotages;

                features.Spoofer.shouldSpoofVersion = data.SpooferShouldSpoofVersion;
                features.Spoofer.spoofedVersion = data.SpooferSpoofedVersion;
                features.Spoofer.useModdedProtocol = data.SpooferUseModdedProtocol;
                features.Spoofer.spoofedPlatform = (Platforms)data.SpooferSpoofedPlatform;
                features.Spoofer.SpoofLevel.Enabled = data.SpooferSpoofLevelEnabled;
                features.Spoofer.SpoofLevel.newLevel = data.SpooferNewLevel;

                features.Protections.ForceDTLS.Enabled = data.ProtectionsForceDTLS;
                features.Protections.BlockServerTeleports.Enabled = data.ProtectionsBlockServerTeleports;
                features.Protections.BlockUnauthorizedSystemUpdates = data.ProtectionsBlockUnauthorizedSystemUpdates;
                features.Protections.BlockLargeGameMessages = data.ProtectionsBlockLargeGameMessages;
                features.Protections.BlockInvalidGameDataMessages = data.ProtectionsBlockInvalidGameDataMessages;
                features.Protections.HardenedReadPackedUInt.Enabled = data.ProtectionsHardenedReadPackedUInt;
                features.Protections.MemoryAllocationOverload.Enabled = data.ProtectionsMemoryAllocationOverload;
                features.Protections.BypassShapeshiftRatelimits.Enabled = data.ProtectionsBypassShapeshiftRatelimits;
                features.Protections.Votekicks.Enabled = data.ProtectionsVotekicks;
                features.Protections.ProtectAgainstNonHostKickExploit = data.ProtectionsProtectAgainstNonHostKickExploit;

                anticheat.Anticheat.Enabled = data.AnticheatEnabled;
                anticheat.Anticheat.CheckSpoofedPlatforms = data.AnticheatCheckSpoofedPlatforms;
                anticheat.Anticheat.sendNotification = data.AnticheatSendNotification;
                anticheat.Anticheat.discardRpc = data.AnticheatDiscardRpc;
                anticheat.Anticheat.punishment = (anticheat.Anticheat.Punishments)data.AnticheatPunishment;

                features.Roles.DisableShapeshiftAnimation = data.RolesDisableShapeshiftAnimation;
                features.Roles.AllowVentingForCrewmates = data.RolesAllowVentingForCrewmates;
                features.Roles.NoKillChecks = data.RolesNoKillChecks;
                features.Roles.SkipSabotageChecks.SabotageAsCrewmate = data.RolesSabotageAsCrewmate;
                features.Roles.SkipSabotageChecks.SabotageInVents = data.RolesSabotageInVents;
                features.Roles.MoveModifier.MoveInVents = data.RolesMoveInVents;

                Sabotage.UpdateSystemsDirectly = data.SabotageUpdateSystemsDirectly;
                features.Visuals.SpectatePlayer.Enabled = data.SpectatePlayerEnabled;
                features.Host.AlwaysImposter.Enabled = data.AlwaysImposter;
                features.Host.AlwaysImposter.assignedRole = (RoleTypes)data.AlwaysImposterAssignedRole;
                features.Self.UnlimitedMeetings.enabled = data.UnlimitedMeetings;

                ui.Styles.ClearCache();
                Hydra.Log.LogInfo("Config loaded from " + ConfigPath);
            }
            catch (Exception ex)
            {
                Hydra.Log.LogError("Failed to load config: " + ex.Message);
            }
        }
    }
}
