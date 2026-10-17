using BepInEx;
using HydraMenu.anticheat;
using System;
using System.IO;

namespace HydraMenu.modules.misc
{
	internal class AnticheatAuditLogger : Module
	{
		private static readonly string LogDirectory = Path.Combine(Paths.GameRootPath, "BepInEx", "Hydra");
		private static readonly string LogFilePath = Path.Combine(LogDirectory, "cheater_audit.log");
		private static readonly object FileLock = new object();

		public AnticheatAuditLogger() : base("AnticheatAuditLogger")
		{
			base.Enabled = true;
		}

		private void OnViolation(PlayerControl player, string reason, Anticheat.Punishments punishment)
		{
			if(!Enabled || player == null || player.Data == null) return;

			try
			{
				if(!Directory.Exists(LogDirectory))
				{
					Directory.CreateDirectory(LogDirectory);
				}

				string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
				string playerName = player.Data.PlayerName ?? "Unknown";
				string role = player.Data.RoleType.ToString();
				string friendCode = player.Data.FriendCode ?? "N/A";
				int clientId = player.OwnerId;

				string logLine = $"[{timestamp}] Cheater Flagged: '{playerName}' | Role: {role} | FriendCode: {friendCode} | ClientId: {clientId} | Reason: {reason} | Action: {punishment}";

				lock(FileLock)
				{
					File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
				}

				Hydra.Log.LogInfo($"[AuditLogger] Recorded anticheat violation for {playerName}");
			}
			catch(Exception ex)
			{
				Hydra.Log.LogWarning($"[AuditLogger] Failed to write cheater audit log: {ex.Message}");
			}
		}

		protected override void OnEnable()
		{
			Anticheat.OnAnticheatViolation += OnViolation;
		}

		protected override void OnDisable()
		{
			Anticheat.OnAnticheatViolation -= OnViolation;
		}
	}
}
