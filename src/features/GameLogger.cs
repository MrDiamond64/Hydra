using System;
using System.Collections.Generic;
using System.Linq;

namespace HydraMenu.features
{
	public enum LogType
	{
		RPC,
		Warning,
		System,
		Chat
	}

	public class LogEntry
	{
		public DateTime Timestamp;
		public string PlayerName;
		public string Message;
		public LogType Type;

		public string GetFormattedString()
		{
			string color = "white";
			switch(Type)
			{
				case LogType.RPC:
					color = "#00FFFF"; // Cyan
					break;
				case LogType.Warning:
					color = "#FF4444"; // Red
					break;
				case LogType.System:
					color = "#44FF44"; // Green
					break;
				case LogType.Chat:
					color = "#FFFFFF"; // White
					break;
			}
			return $"<color={color}>[{Timestamp:HH:mm:ss}] [{Type}] {PlayerName}: {Message}</color>";
		}
	}

	public static class GameLogger
	{
		public static readonly List<LogEntry> Entries = new List<LogEntry>();
		private static readonly object lockObj = new object();
		public static int MaxEntries = 500;

		public static bool ShowRpc = true;
		public static bool ShowWarning = true;
		public static bool ShowSystem = true;
		public static bool ShowChat = true;
		public static string SearchFilter = "";
		public static string PlayerFilter = "";

		public static void Log(LogType type, string playerName, string message)
		{
			lock(lockObj)
			{
				try
				{
					string finalPlayer = string.IsNullOrEmpty(playerName) ? "System" : playerName;
					string finalMsg = string.IsNullOrEmpty(message) ? "" : message;

					// Prevent rapid duplicate logs within 200ms
					if(Entries.Count > 0)
					{
						var last = Entries[Entries.Count - 1];
						if(last.Type == type && last.PlayerName == finalPlayer && last.Message == finalMsg && (DateTime.Now - last.Timestamp).TotalMilliseconds < 200)
						{
							return;
						}
					}

					Entries.Add(new LogEntry
					{
						Timestamp = DateTime.Now,
						PlayerName = finalPlayer,
						Message = finalMsg,
						Type = type
					});

					if(Entries.Count > MaxEntries)
					{
						Entries.RemoveAt(0);
					}

					Hydra.Log.LogInfo($"[GameLogger] [{type}] {playerName}: {message}");
				}
				catch(Exception ex)
				{
					Hydra.Log.LogError($"Error in GameLogger.Log: {ex}");
				}
			}
		}

		public static void LogRpc(string playerName, string rpcName, string details = "")
		{
			string msg = string.IsNullOrEmpty(details) ? rpcName : $"{rpcName} ({details})";
			Log(LogType.RPC, playerName, msg);
		}

		public static void LogWarning(string playerName, string reason)
		{
			Log(LogType.Warning, playerName, reason);
		}

		public static void LogSystem(string playerName, string message)
		{
			Log(LogType.System, playerName, message);
		}

		public static void LogChat(string playerName, string text)
		{
			Log(LogType.Chat, playerName, text);
		}

		public static void Clear()
		{
			lock(lockObj)
			{
				Entries.Clear();
			}
		}

		public static List<LogEntry> GetFilteredEntries()
		{
			lock(lockObj)
			{
				try
				{
					var result = Entries.AsEnumerable();

					if(!ShowRpc) result = result.Where(e => e.Type != LogType.RPC);
					if(!ShowWarning) result = result.Where(e => e.Type != LogType.Warning);
					if(!ShowSystem) result = result.Where(e => e.Type != LogType.System);
					if(!ShowChat) result = result.Where(e => e.Type != LogType.Chat);

					if(!string.IsNullOrEmpty(PlayerFilter))
					{
						result = result.Where(e => e.PlayerName.IndexOf(PlayerFilter, StringComparison.OrdinalIgnoreCase) >= 0);
					}

					if(!string.IsNullOrEmpty(SearchFilter))
					{
						result = result.Where(e => e.Message.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) >= 0
							|| e.PlayerName.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) >= 0);
					}

					return result.ToList();
				}
				catch(Exception ex)
				{
					Hydra.Log.LogError($"Error in GameLogger.GetFilteredEntries: {ex}");
					return new List<LogEntry>();
				}
			}
		}
	}
}
