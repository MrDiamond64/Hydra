using HarmonyLib;
using System;

namespace HydraMenu.modules
{
	public class EventCoordinator
	{
		public static event Action<PlayerControl, string> OnPlayerChat;

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
		class PlayerChat
		{
			static void Prefix(PlayerControl sourcePlayer, string chatText)
			{
				Hydra.Log.LogMessage($"[ChatLogger] {sourcePlayer.Data.PlayerName}: {chatText}");

				if(OnPlayerChat != null)
				{
					OnPlayerChat(sourcePlayer, chatText);
				}
			}
		}
	}
}