using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;
using HydraMenu.anticheat.gamedata;
using HydraMenu.anticheat.rpc;
using System;
using System.Collections.Generic;

namespace HydraMenu.anticheat
{
	internal class Anticheat
	{
		public static bool Enabled { get; set; } = true;

		public static Dictionary<GameDataTypes, GameDataCheck> GameDataHandlers = new Dictionary<GameDataTypes, GameDataCheck>()
		{
			{ GameDataTypes.ReadyFlag, new ClientReady() }
		};

		public static Dictionary<RpcCalls, RpcCheck> RpcHandlers = new Dictionary<RpcCalls, RpcCheck>()
		{
			// RPC handlers in this dictionary should be sorted by their RPC ID
			{ RpcCalls.PlayAnimation, new PlayAnimation() },
			{ RpcCalls.CompleteTask, new CompleteTask() },
			{ RpcCalls.Exiled, new Exiled() },
			{ RpcCalls.CheckName, new CheckName() },
			{ RpcCalls.SetName, new SetName() },
			{ RpcCalls.SetColor, new SetColor() },
			{ RpcCalls.ReportDeadBody, new ReportDeadBody() },
			{ RpcCalls.SetScanner, new SetScanner() },
			{ RpcCalls.SetStartCounter, new SetStartCounter() },
			{ RpcCalls.EnterVent, new EnterVent() },
			{ RpcCalls.ExitVent, new ExitVent() },
			{ RpcCalls.SnapTo, new SnapTo() },
			{ RpcCalls.AddVote, new AddVote() },
			{ RpcCalls.CloseDoorsOfType, new CloseDoorsOfType() },
			{ RpcCalls.ClimbLadder, new ClimbLadder() },
			{ RpcCalls.UsePlatform, new UsePlatform() },
			{ RpcCalls.UpdateSystem, new UpdateSystem() },
			{ RpcCalls.SetLevel, new SetLevel() }
		};

		public static bool CheckSpoofedPlatforms { get; set; } = true;

		public enum Punishments
		{
			None,
			Kick,
			ErrorKick,
			ExploitKick,
			Ban
		}

		public static float NotificationDuration = 10.0f;

		public static Punishments punishment = Punishments.None;
		public static bool sendNotification = true;
		public static bool discardRpc = true;

		// The amount of times a player must be flagged before Hydra Anticheat will actually punish them
		public static uint FlagThreshold { get; set; } = 1;

		// Tracks how many times each player (by owner id) has been flagged since their count was last reset
		private static readonly Dictionary<int, uint> flagCounts = new Dictionary<int, uint>();

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
		class OnPlayerControlRPC
		{
			static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(PlayerControl), __instance, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
		class OnPlayerPhysicsRPC
		{
			static bool Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(PlayerPhysics), __instance.myPlayer, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		class OnNetTransformRPC
		{
			static bool Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(CustomNetworkTransform), __instance.myPlayer, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(ShipStatus), null, (RpcCalls)callId, reader);
			}
		}

		private static bool HandleRpc(Type sourceNetObj, PlayerControl player, RpcCalls rpc, MessageReader reader)
		{
			RpcHandlers.TryGetValue(rpc, out RpcCheck rpcCheck);
			if(!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

			if(sourceNetObj != rpcCheck.GetExpectedNetObject())
			{
				// Received an RPC that should've been sent for a different net object, some sort of exploit attempt?
				return false;
			}

			// Only we, the host, should be sending host-only RPCs
			if(player != null && AmongUsClient.Instance.AmHost && rpcCheck.IsHostOnly())
			{
				Flag(player, $"{player.Data.PlayerName} sent the {rpc} RPC while non-host.");
				return false;
			}

			int oldReadPosition = reader.Position;

			bool isValid = rpcCheck.Validate(player, reader);
			if(!isValid && discardRpc) return false;

			// Put the read position back to its previous spot to not mess up the HandleRpc function
			reader.Position = oldReadPosition;
			return true;
		}

		public static bool HandleGameData(GameDataTypes type, MessageReader reader)
		{
			GameDataHandlers.TryGetValue(type, out GameDataCheck gameDataCheck);
			if(!Enabled || gameDataCheck == null || !gameDataCheck.Enabled) return true;

			int oldReadPosition = reader.Position;

			bool isValid = gameDataCheck.Validate(reader);
			if(!isValid && discardRpc) return false;

			// Put the read position back to its previous spot
			reader.Position = oldReadPosition;
			return true;
		}

		public static void Flag(PlayerControl player, string reason, bool shouldPunish = true)
		{
			// Sanity check, make sure that we are not flagging ourselves
			// On servers without net object impersonation checks, it may be possible to send an invalid RPC on the behalf of the host
			// which would result in Hydra Anticheat flagging ourselves and banning us from our own lobby
			if(player == PlayerControl.LocalPlayer) return;

			if(!shouldPunish)
			{
				NotifyFlag(reason);
				return;
			}

			uint flagCount = RegisterFlag(player);
			if(flagCount < FlagThreshold)
			{
				NotifyFlag(reason);
				return;
			}

			// The player has reached the flag threshold, reset their count so future violations need to build back up to the threshold again
			flagCounts.Remove(player.OwnerId);

			if(punishment == Punishments.None)
			{
				NotifyFlag(reason);
				return;
			}

			bool weAreHost = AmongUsClient.Instance.AmHost;
			bool targetIsHost = player.OwnerId == AmongUsClient.Instance.HostId;

			// Kick, ErrorKick, and Ban all rely on host-only APIs, so if we are not the host of the lobby we cannot actually carry them out
			// ExploitKick is the only punishment that can be used without being host, since Utilities::KickPlayer has its own non-host fallback via the ventilation system exploit
			if(!weAreHost && punishment != Punishments.ExploitKick)
			{
				NotifyFlag(reason);
				return;
			}

			// The ventilation exploit cannot be used to kick the host of the lobby, Utilities::KickPlayer already refuses to do so
			if(punishment == Punishments.ExploitKick && targetIsHost)
			{
				NotifyFlag(reason);
				return;
			}

			Punish(player);
		}

		// If we do not know which player caused the violation
		public static void Flag(string reason)
		{
			NotifyFlag(reason);
		}

		private static uint RegisterFlag(PlayerControl player)
		{
			flagCounts.TryGetValue(player.OwnerId, out uint count);
			count++;
			flagCounts[player.OwnerId] = count;
			return count;
		}

		private static void NotifyFlag(string reason)
		{
			if(sendNotification)
			{
				Hydra.notifications.Send("Anticheat", reason, NotificationDuration);
			}
		}

		private static void NotifyPunishment(string message)
		{
			if(sendNotification)
			{
				Hydra.notifications.Send("Anticheat", message, NotificationDuration);
			}
		}

		private static void Punish(PlayerControl player)
		{
			switch(punishment)
			{
				case Punishments.Kick:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} was kicked by Hydra Anticheat for hacking");

					AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
					NotifyPunishment($"{player.Data.PlayerName} has been kick from the game by the Hydra Anticheat!");
					break;

				case Punishments.ErrorKick:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} was kicked by Hydra Anticheat for hacking");

					// The vanilla anticheat prevents using the ErrorKick method if the game has not started yet
					if(AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
					{
						AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
					}
					else
					{
						// When a game starts, the host waits around ten seconds to wait for all clients to send the ClientReady game message
						// If the ten-second timer is reached without a ClientReady game message being received by the host, the host will kick the player due to timeout
						// The kick message shown to the player will explain that the player has a poor internet connection or that their device is too old
						// and in-game, players will be shown that the player left due to an error instead of being kicked
						// Any other disconnection messages other than ClientTimeout will result in the vanilla anticheat kicking us from the lobby
						AmongUsClient.Instance.SendLateRejection(player.OwnerId, DisconnectReasons.ClientTimeout);
					}

					NotifyPunishment($"{player.Data.PlayerName} has been ErrorKicked by the Hydra Anticheat !");
					break;

				case Punishments.ExploitKick:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} was kicked by Hydra Anticheat via the ventilation system exploit");

					Utilities.KickPlayer(player);
					NotifyPunishment($"{player.Data.PlayerName} has been kicked by the Hydra Anticheat using the Ventilation System exploit!");
					break;

				case Punishments.Ban:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} was automatically banned by Hydra Anticheat for hacking");

					AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
					NotifyPunishment($"{player.Data.PlayerName} has been banned by the Hydra Anticheat!");
					break;
			}
		}
	}
}
