using HarmonyLib;
using InnerNet;
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
		public static event Action<PlayerControl, ClientData> OnPlayerJoin;
		public static event Action<PlayerControl, string> OnPlayerChat;
		public static event Action<PlayerControl, PlayerControl, MurderResultFlags> OnPlayerMurder;

		// This function is called when the role selection screen finishes and the game is ready to play
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
		class GameLoad
		{
			static void Prefix()
			{
				PublishEvent(OnGameLoad);
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
		class MeetingEnd
		{
			static void Prefix()
			{
				PublishEvent(OnMeetingEnd);
			}
		}

		[HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
		class MinigameOpen
		{
			static void Prefix(Minigame __instance)
			{
				Hydra.Log.LogMessage($"Minigame of type {__instance.GetIl2CppType().Name} was opened");

				PublishEvent(OnOpenMinigame, __instance);
			}
		}

		// This function is late enough to allow us to modify the ladder cooldown without the game overriding it
		[HarmonyPatch(typeof(Ladder), nameof(Ladder.SetDestinationCooldown))]
		class LadderUsed
		{
			static void Postfix(Ladder __instance)
			{
				Hydra.Log.LogMessage($"Used ladder");

				PublishEvent(OnUseLadder, __instance.Destination);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
		class OnJoin
		{
			static void Postfix(PlayerControl __instance)
			{
				if(__instance == PlayerControl.LocalPlayer || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;

				ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
				if(clientData != null)
				{
					PlatformSpecificData platformData = clientData.PlatformData;
					Hydra.Log.LogMessage($"[PlayerLogger] {clientData.PlayerName} ({__instance.NetId}) joined on {platformData.Platform}. Friendcode {clientData.FriendCode}, PUID {clientData.ProductUserId}");
				}
				else
				{
					// We should use NetworkedPlayerInfo::PlayerName instead of PlayerControl::name whenever possible to get the player's name
					// however if the PlayerControl object has just spawned, then it is unlikely that a NetworkedPlayerInfo object has spawned yet
					Hydra.Log.LogMessage($"[PlayerLogger] {__instance.name} ({__instance.NetId}) joined.");
				}

				PublishEvent(OnPlayerJoin, __instance, clientData);
			}
		}

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
		class PlayerChat
		{
			static void Prefix(PlayerControl sourcePlayer, string chatText)
			{
				Hydra.Log.LogMessage($"[ChatLogger] {sourcePlayer.Data.PlayerName}: {chatText}");

				PublishEvent(OnPlayerChat, sourcePlayer, chatText);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		class PlayerMurder
		{
			static void Prefix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
			{
				PublishEvent(OnPlayerMurder, __instance, target, resultFlags);
			}
		}

		// These functions are to simplify having null-checks everywhere
		// Yes I know we could use evt?.Invoke to avoid having to check if the event is null, I just don't like that code style
		private static void PublishEvent(Action evt)
		{
			if(evt == null) return;
			evt();
		}

		private static void PublishEvent<T1>(Action<T1> evt, T1 arg1)
		{
			if(evt == null) return;
			evt(arg1);
		}

		private static void PublishEvent<T1, T2>(Action<T1, T2> evt, T1 arg1, T2 arg2)
		{
			if(evt == null) return;
			evt(arg1, arg2);
		}

		private static void PublishEvent<T1, T2, T3>(Action<T1, T2, T3> evt, T1 arg1, T2 arg2, T3 arg3)
		{
			if(evt == null) return;
			evt(arg1, arg2, arg3);
		}
	}
}