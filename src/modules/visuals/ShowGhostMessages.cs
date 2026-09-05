using UnityEngine;

namespace LunarMenu.modules.visuals
{
	internal class ShowGhostMessages : Module
	{
		public ShowGhostMessages() : base("ShowGhostMessages")
		{
			base.Enabled = true;
		}

		private void OnPlayerChat(PlayerControl player, string text)
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || PlayerControl.LocalPlayer.Data.IsDead || !player.Data.IsDead) return;

			// There's no quick and easy way to get messages by ghosts to show up in chat if we are still alive
			// We have to reimplement the ChatController::AddChat method and build the chat bubble ourself
			ChatController chatController = HudManager.Instance.Chat;
			ChatBubble bubble = chatController.GetPooledBubble();
			try
			{
				bubble.transform.SetParent(chatController.scroller.Inner);
				bubble.transform.localScale = Vector3.one;

				if(player == PlayerControl.LocalPlayer)
				{
					bubble.SetRight();
				}
				else
				{
					bubble.SetLeft();
				}

				bubble.SetCosmetics(player.Data);
				chatController.SetChatBubbleName(bubble, player.Data, player.Data.IsDead, false, PlayerNameColor.Get(player.Data), null);
				bubble.SetText(text);
				bubble.AlignChildren();
				chatController.AlignAllBubbles();
				chatController.chatNotification.SetUp(player, text);

				if(!chatController.IsOpenOrOpening && chatController.notificationRoutine == null)
				{
					chatController.notificationRoutine = chatController.StartCoroutine(chatController.BounceDot());
				}

				if(player != PlayerControl.LocalPlayer && !chatController.IsOpenOrOpening)
				{
					SoundManager.Instance.PlaySound(chatController.messageSound, false, 1f, null).pitch = 0.5f + player.PlayerId / 15f;
					chatController.chatNotification.SetUp(player, text);
				}
			}
			catch
			{
				HudManager.Instance.Chat.chatBubblePool.Reclaim(bubble);
			}
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerChat += OnPlayerChat;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerChat -= OnPlayerChat;
		}
	}
}