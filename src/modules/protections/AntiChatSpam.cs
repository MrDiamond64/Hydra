using HarmonyLib;
using System;
using System.Collections.Generic;

namespace HydraMenu.modules.protections
{
	internal class AntiChatSpam : Module
	{
		public AntiChatSpam() : base("AntiChatSpam")
		{
			base.Enabled = true;
		}

		private static AntiChatSpam Instance
		{
			get { return ModuleManager.antiChatSpam; }
		}

		public int MaxMessagesPerWindow { get; set; } = 4;
		public float WindowSeconds { get; set; } = 2.0f;
		public bool BlockRepetitiveMessages { get; set; } = true;

		private static readonly Dictionary<int, List<float>> PlayerMessageTimestamps = new Dictionary<int, List<float>>();
		private static readonly Dictionary<int, string> LastPlayerMessage = new Dictionary<int, string>();

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
		class OnAddChat
		{
			static bool Prefix(PlayerControl sourcePlayer, string chatText)
			{
				if(!Instance.Enabled || sourcePlayer == null || sourcePlayer == PlayerControl.LocalPlayer) return true;

				int playerId = sourcePlayer.PlayerId;
				float currentTime = UnityEngine.Time.time;

				// Rate limiting sliding window
				if(!PlayerMessageTimestamps.TryGetValue(playerId, out List<float> timestamps))
				{
					timestamps = new List<float>();
					PlayerMessageTimestamps[playerId] = timestamps;
				}

				// Purge old timestamps outside window
				timestamps.RemoveAll(t => (currentTime - t) > Instance.WindowSeconds);

				if(timestamps.Count >= Instance.MaxMessagesPerWindow)
				{
					Hydra.Log.LogWarning($"[AntiChatSpam] Suppressed chat flood from {sourcePlayer.Data?.PlayerName}");
					return false;
				}

				// Block exact repetitive message spamming
				if(Instance.BlockRepetitiveMessages && LastPlayerMessage.TryGetValue(playerId, out string lastMsg))
				{
					if(lastMsg == chatText && timestamps.Count >= 2)
					{
						Hydra.Log.LogWarning($"[AntiChatSpam] Suppressed repetitive message spam from {sourcePlayer.Data?.PlayerName}");
						return false;
					}
				}

				timestamps.Add(currentTime);
				LastPlayerMessage[playerId] = chatText;

				return true;
			}
		}

		protected override void OnEnable()
		{
			PlayerMessageTimestamps.Clear();
			LastPlayerMessage.Clear();
		}

		protected override void OnDisable()
		{
			PlayerMessageTimestamps.Clear();
			LastPlayerMessage.Clear();
		}
	}
}
