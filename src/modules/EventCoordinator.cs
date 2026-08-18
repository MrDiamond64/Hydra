using HarmonyLib;
using System;

namespace HydraMenu.modules
{
	internal class EventCoordinator
	{
		// Game Events
		public static event Action OnMeetingEnd;
		public static event Action OnGameLoad;
		public static event Action<Minigame> OnOpenMinigame;
		public static event Action<Ladder> OnUseLadder;

		// Player Events
		public static event Action<PlayerControl, string> OnPlayerChat;
		public static event Action<PlayerControl, PlayerControl, MurderResultFlags> OnPlayerMurder;

		[HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
		class GameLoad
		{
			static void Prefix()
			{
				if(OnGameLoad != null)
				{
					OnGameLoad();
				}
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
		class MeetingEnd
		{
			static void Prefix()
			{
				if(OnMeetingEnd != null)
				{
					OnMeetingEnd();
				}
			}
		}

		[HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
		class MinigameOpen
		{
			static void Prefix(Minigame __instance)
			{
				Hydra.Log.LogMessage($"Minigame of type {__instance.GetIl2CppType().Name} was opened");

				if(OnOpenMinigame != null)
				{
					OnOpenMinigame(__instance);
				}
			}
		}

		// This function is late enough to allow us to modify the ladder cooldown without the game overriding it
		[HarmonyPatch(typeof(Ladder), nameof(Ladder.SetDestinationCooldown))]
		class LadderUsed
		{
			static void Postfix(Ladder __instance)
			{
				Hydra.Log.LogMessage($"Used ladder");

				if(OnUseLadder != null)
				{
					OnUseLadder(__instance.Destination);
				}
			}
		}

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

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		class PlayerMurder
		{
			static void Prefix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
			{
				if(OnPlayerMurder != null)
				{
					OnPlayerMurder(__instance, target, resultFlags);
				}
			}
		}
	}
}