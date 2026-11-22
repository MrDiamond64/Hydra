using HarmonyLib;
using HydraMenu.network;
using InnerNet;
using UnityEngine;

namespace HydraMenu.modules.misc
{
	internal class Whisper : Module
	{
		public Whisper() : base("Whisper") { }

		private static Whisper Instance
		{
			get { return ModuleManager.whisper; }
		}

		public PlayerControl target;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
		class OnChat
		{
			static bool Prefix(PlayerControl __instance, string chatText)
			{
				if(!Instance.Enabled) return true;

				// Avoid two chat bubbles from showing up if we are whispering to ourselves
				if(__instance != Instance.target)
				{
					BatchedMessage batch = new BatchedMessage(Instance.target.OwnerId);
					batch.QueueSendChat(__instance, chatText);
					batch.FinishBatch();
				}

				AddChatBubble(__instance, Instance.target, chatText);

				return false;
			}
		}

		private static void AddChatBubble(PlayerControl sender, PlayerControl target, string text)
		{
			ChatController chatController = HudManager.Instance.Chat;

			ChatBubble bubble = chatController.GetPooledBubble();
			try
			{
				bubble.transform.SetParent(chatController.scroller.Inner);
				bubble.transform.localScale = Vector3.one;

				bubble.SetRight();

				bubble.SetCosmetics(sender.Data);

				bool didVote = MeetingHud.Instance != null && MeetingHud.Instance.DidVote(sender.PlayerId);
				bubble.SetName($"{sender.Data.PlayerName} -> {target.Data.PlayerName}", sender.Data.IsDead, didVote, PlayerNameColor.Get(sender.Data));

				bubble.SetText(text);
				bubble.AlignChildren();
				chatController.AlignAllBubbles();
				chatController.chatNotification.SetUp(PlayerControl.LocalPlayer, text);
			}
			catch
			{
				HudManager.Instance.Chat.chatBubblePool.Reclaim(bubble);
			}
		}

		private void OnPlayerDisconnect(ClientData client, DisconnectReasons reason)
		{
			if(client.Character != target) return;

			Hydra.notifications.Send("Whisper", "Whisper was disabled as the player you were whispering to left the game.");
			Enabled = false;
		}

		private void OnGameEnd(EndGameResult result)
		{
			Hydra.notifications.Send("Whisper", "Whisper was disabled as the game has ended.", 10);
			Enabled = false;
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("Whisper", "Whisper was disabled as you left the game.");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(target == null)
			{
				_enabled = false;
				return;
			}

			EventCoordinator.OnPlayerDisconnect += OnPlayerDisconnect;
			EventCoordinator.OnDisconnect += OnDisconnect;
			EventCoordinator.OnGameEnd += OnGameEnd;
		}

		protected override void OnDisable()
		{
			target = null;

			EventCoordinator.OnPlayerDisconnect -= OnPlayerDisconnect;
			EventCoordinator.OnDisconnect -= OnDisconnect;
			EventCoordinator.OnGameEnd += OnGameEnd;
		}
	}
}