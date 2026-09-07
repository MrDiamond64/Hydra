using HarmonyLib;
using Hazel;
using HydraMenu.ui.sections;
using Il2CppInterop.Runtime;
using InnerNet;
using System;
using System.Collections.Generic;
using static HydraMenu.network.Constants;

namespace HydraMenu.modules
{
	internal class EventCoordinator
	{
		// Game Events
		public static event Action OnGameStart;
		public static event Action OnGameLoad;
		public static event Action OnDisconnect;

		public static event Action OnMeetingEnd;
		public static event Action<Minigame> OnOpenMinigame;
		public static event Action<Ladder> OnUseLadder;
		public static event Action<ZiplineConsole> OnUseZipline;

		// Player Events
		public static event Action<PlayerControl, ClientData> OnPlayerJoin;
		public static event Action<ClientData, DisconnectReasons> OnPlayerDisconnect;
		public static event Action<PlayerControl, string> OnPlayerChat;

		public static event Action<PlayerControl, byte> OnPlayerEnterVent;
		public static event Action<PlayerControl, byte> OnPlayerExitVent;
		public static event Action<PlayerControl, byte, byte> OnPlayerMoveVent;

		public static event Action<PlayerControl> OnPlayerEnterCameras;
		public static event Action<PlayerControl> OnPlayerExitCameras;

		public static event Action<PlayerControl, PlayerControl, MurderResultFlags> OnPlayerMurder;
		public static event Action<PlayerControl, PlayerControl, bool> OnPlayerShapeshift;
		public static event Action<PlayerControl> OnPlayerPhantom;

		public static event Action<ClientData, ClientData> OnPlayerVotekick;

		public static event Action<NetworkedPlayerInfo, NetworkedPlayerInfo> OnPlayerCastVote;

		// Sabotage Events
		public static event Action OnHudOverrideSabotage;
		public static event Action OnHudOverrideRepair;

		// Network Events
		public static event Action<InnerNetObject> OnNetObjectSpawn;

		private static readonly HashSet<Il2CppSystem.Type> ShipNetObjects = [Il2CppType.From(typeof(ShipStatus)), Il2CppType.From(typeof(SkeldShipStatus)), Il2CppType.From(typeof(MiraShipStatus)), Il2CppType.From(typeof(PolusShipStatus)), Il2CppType.From(typeof(AirshipStatus)), Il2CppType.From(typeof(FungleShipStatus))];

		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
		class GameStart
		{
			static void Prefix()
			{
				PublishEvent(OnGameStart);
			}
		}

		// This function is called when the role selection screen finishes and the game is ready to play
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
		class GameLoad
		{
			static void Prefix()
			{
				PublishEvent(OnGameLoad);
			}
		}

		[HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
		class Disconnect
		{
			static void Prefix()
			{
				Hydra.Log.LogInfo("[Disconnect Logger] Our player was disconnected from the lobby");

				HostSection.lobbyList.Clear();
				HostSection.shipList.Clear();

				PublishEvent(OnDisconnect);
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
				Hydra.Log.LogMessage($"Ladder {__instance.Id} was used");

				PublishEvent(OnUseLadder, __instance.Destination);
			}
		}

		// This function is late enough to allow us to modify the ladder cooldown without the game overriding it
		// There is a ZiplineConsole::SetDestinationCooldown method, but we cannot patch it as it is inlined
		[HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.ResetTarget))]
		class ZiplineUsed
		{
			static void Postfix(ZiplineBehaviour __instance)
			{
				ZiplineConsole console = __instance.lastUsedConsole;
				if(console == null) return;

				Hydra.Log.LogMessage("Zipline " + (__instance.lastUsedConsole.atTop ? "at top" : "at bottom") + " was used");

				PublishEvent(OnUseZipline, console);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
		class PlayerJoin
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

		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
		class PlayerDisconnect
		{
			static void Prefix(ClientData data, DisconnectReasons reason)
			{
				if(data.Character == null) return;

				Hydra.Log.LogInfo($"[Disconnect Logger] {data.Character.Data.PlayerName} was disconnected with reason {reason}");

				PublishEvent(OnPlayerDisconnect, data, reason);
			}
		}

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
		class PlayerChat
		{
			static void Prefix(PlayerControl sourcePlayer, string chatText)
			{
				PublishEvent(OnPlayerChat, sourcePlayer, chatText);
			}
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deserialize))]
		class PlayerVentNonHost
		{
			static void Prefix(VentilationSystem __instance, MessageReader reader) {
				int oldReadPosition = reader.Position;

				int ventCleans = reader.ReadPackedInt32();
				if(ventCleans > PlayerControl.AllPlayerControls.Count || ventCleans > reader.BytesRemaining) goto end;

				// Skip reading through vent clean data
				// 1 byte for player id, another byte for vent id, so we need to skip by 2 * vent clean count
				reader.Position += 2 * ventCleans;

				int ventedPlayers = reader.ReadPackedInt32();
				if(ventedPlayers > PlayerControl.AllPlayerControls.Count || ventedPlayers > reader.BytesRemaining) goto end;

				Dictionary<byte, byte> ventData = new Dictionary<byte, byte>();
				for(int i = 0; i < ventedPlayers; i++)
				{
					byte playerId = reader.ReadByte();
					byte ventId = reader.ReadByte();

					ventData[playerId] = ventId;
				}

				// Compare with what we have with new data to see vent changes
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					byte playerId = player.PlayerId;

					bool inOld = __instance.PlayersInsideVents.TryGetValue(playerId, out byte oldVent);
					bool inNew = ventData.TryGetValue(playerId, out byte newVent);

					if(!inOld && inNew)
					{
						Hydra.Log.LogMessage($"{player.Data.PlayerName} entered vent {newVent}");
						PublishEvent(OnPlayerEnterVent, player, newVent);
					}
					else if(inOld && !inNew)
					{
						Hydra.Log.LogMessage($"{player.Data.PlayerName} left vent {oldVent}");
						PublishEvent(OnPlayerExitVent, player, oldVent);
					}
					else if(oldVent != newVent)
					{
						Hydra.Log.LogMessage($"{player.Data.PlayerName} moved from vent {oldVent} to {newVent}");
						PublishEvent(OnPlayerMoveVent, player, oldVent, newVent);
					}
				}

				end:
				reader.Position = oldReadPosition;
			}
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.UpdateSystem))]
		class PlayerVentHost
		{
			static void Prefix(VentilationSystem __instance, PlayerControl player, MessageReader msgReader)
			{
				int oldReadPosition = msgReader.Position;

				msgReader.ReadUInt16(); // Sequence ID
				VentilationSystem.Operation operation = (VentilationSystem.Operation)msgReader.ReadByte();
				byte ventId = msgReader.ReadByte();

				switch(operation)
				{
					case VentilationSystem.Operation.Enter:
						PublishEvent(OnPlayerEnterVent, player, ventId);
						break;

					case VentilationSystem.Operation.Exit:
						PublishEvent(OnPlayerExitVent, player, ventId);
						break;

					case VentilationSystem.Operation.Move:
						byte oldVent = __instance.PlayersInsideVents[player.PlayerId];
						PublishEvent(OnPlayerMoveVent, player, oldVent, ventId);
						break;
				}

				msgReader.Position = oldReadPosition;
			}
		}

		[HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.UpdateSystem))]
		class UpdateCamerasHost
		{
			static void Postfix(PlayerControl player, MessageReader msgReader)
			{
				msgReader.Position--;
				// 1 = Player started to watch cameras, 2 (and every other value) = Player stopped watching cameras
				byte operation = msgReader.ReadByte();
				msgReader.Position++;

				if(operation == 1)
				{
					PublishEvent(OnPlayerEnterCameras, player);
				}
				else
				{
					PublishEvent(OnPlayerExitCameras, player);
				}
			}
		}

		[HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.Deserialize))]
		class UpdateCamerasNonHost
		{
			static void Prefix(SecurityCameraSystemType __instance, MessageReader reader)
			{
				int oldReadPosition = reader.Position;

				int playerCount = reader.ReadPackedInt32();
				if(playerCount > PlayerControl.AllPlayerControls.Count || playerCount > reader.BytesRemaining) goto end;

				HashSet<byte> players = new HashSet<byte>();

				for(int i = 0; i < playerCount; i++)
				{
					byte playerId = reader.ReadByte();
					players.Add(playerId);
				}

				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					bool inOld = __instance.PlayersUsing.Contains(player.PlayerId);
					bool inNew = players.Contains(player.PlayerId);

					if(!inOld && inNew)
					{
						PublishEvent(OnPlayerEnterCameras, player);
					}
					else if(inOld && !inNew)
					{
						PublishEvent(OnPlayerExitCameras, player);
					}
				}

				end:
				reader.Position = oldReadPosition;
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

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
		class PlayerShapeshift
		{
			static void Prefix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
			{
				PublishEvent(OnPlayerShapeshift, __instance, targetPlayer, animate);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleServerVanish))]
		class PlayerPhantom
		{
			static void Prefix(PlayerControl __instance)
			{
				PublishEvent(OnPlayerPhantom, __instance);
			}
		}

		[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
		class PlayerVotekick
		{
			static void Postfix(int srcClient, int clientId)
			{
				Hydra.Log.LogInfo($"[VotekickLogger] {srcClient} voted to kick out {clientId}");

				ClientData source = AmongUsClient.Instance.FindClientById(srcClient);
				ClientData target = AmongUsClient.Instance.FindClientById(clientId);
				if(source == null || target == null) return;

				if(clientId == AmongUsClient.Instance.ClientId)
				{
					Hydra.notifications.Send("Votekick Logger", $"{source.PlayerName} has voted to kick you out.");
				}

				PublishEvent(OnPlayerVotekick, source, target);
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
		class PlayerCastVote
		{
			static void Postfix(PlayerId srcPlayerId, PlayerId suspectPlayerId)
			{
				NetworkedPlayerInfo voter = GameData.Instance.GetPlayerById(srcPlayerId);
				NetworkedPlayerInfo votee = GameData.Instance.GetPlayerById(suspectPlayerId);
				if(voter == null || votee == null) return;

				PublishEvent(OnPlayerCastVote, voter, votee);
			}
		}

		[HarmonyPatch(typeof(HudOverrideSystemType), nameof(HudOverrideSystemType.UpdateSystem))]
		class UpdateHudOverrideHost
		{
			static void Postfix(MessageReader msgReader)
			{
				msgReader.Position--;
				HudOverrideSystemOperation operation = (HudOverrideSystemOperation)msgReader.ReadByte();
				msgReader.Position++;

				if(operation.HasFlag(HudOverrideSystemOperation.Sabotage))
				{
					Hydra.Log.LogMessage($"HudOverride system was sabotaged");
					PublishEvent(OnHudOverrideSabotage);
				}
				else
				{
					Hydra.Log.LogMessage($"HudOverride system was repaired");
					PublishEvent(OnHudOverrideRepair);
				}
			}
		}

		[HarmonyPatch(typeof(HudOverrideSystemType), nameof(HudOverrideSystemType.Deserialize))]
		class UpdateHudOverrideNonHost
		{
			static void Prefix(HudOverrideSystemType __instance, MessageReader reader)
			{
				bool isActive = reader.ReadBoolean();
				bool wasActive = __instance.IsActive;
				reader.Position--;

				if(!wasActive && isActive)
				{
					Hydra.Log.LogMessage($"HudOverride system was sabotaged");
					PublishEvent(OnHudOverrideSabotage);
				}
				else if(wasActive && !isActive)
				{
					Hydra.Log.LogMessage($"HudOverride system was repaired");
					PublishEvent(OnHudOverrideRepair);
				}
			}
		}

		[HarmonyPatch(typeof(InnerNetObjectCollection), nameof(InnerNetObjectCollection.TryAddNetObject))]
		class NetObjectAdd
		{
			static void Postfix(InnerNetObject obj)
			{
				if(obj == null) return;

				Il2CppSystem.Type type = obj.GetIl2CppType();

				if(type == Il2CppType.From(typeof(LobbyBehaviour)))
				{
					HostSection.lobbyList.Enqueue(obj);
				}
				else if(ShipNetObjects.Contains(type))
				{
					HostSection.shipList.Enqueue(obj);
				}

				PublishEvent(OnNetObjectSpawn, obj);
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