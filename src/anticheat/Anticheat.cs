using HarmonyLib;
using Hazel;
using HydraMenu.anticheat.rpc;
using System.Collections.Generic;

namespace HydraMenu.anticheat
{
	internal class Anticheat
	{
		public static bool Enabled { get; set; } = true;

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
			{ RpcCalls.CloseDoorsOfType, new CloseDoorsOfType() },
			{ RpcCalls.UpdateSystem, new UpdateSystem() },
			{ RpcCalls.SetLevel, new SetLevel() },
		};

		public static bool CheckSpoofedPlatforms { get; set; } = true;

		public enum Punishments
		{
			None,
			Kick,
			ErrorKick,
			Ban
		}

		public static float NotificationDuration = 10.0f;

		public static Punishments punishment = Punishments.None;
		public static bool DiscardRPC = true;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
		class OnPlayerControlRPC
		{
			static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
			{
				RpcCalls RpcId = (RpcCalls)callId;

				RpcHandlers.TryGetValue(RpcId, out RpcCheck rpcCheck);
				if(!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

				if(rpcCheck.GetExpectedNetObject() != typeof(PlayerControl))
				{
					// Recieved a RPC that should've been sent for a different net object, some sort of exploit attempt?
					return false;
				}

				// Only we, the host, should be sending host-only RPCs
				if(AmongUsClient.Instance.AmHost && rpcCheck.IsHostOnly())
				{
					Flag(__instance, $"Sending RPC {RpcId} while non-host");
					return false;
				}

				int oldReadPosition = reader.Position;
				bool blockRpc = false;

				rpcCheck.Validate(__instance, reader, ref blockRpc);

				if(DiscardRPC && !blockRpc)
				{
					// Put the read position back to its previous spot to not mess up the HandleRpc function
					reader.Position = oldReadPosition;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
		class OnPlayerPhysicsRPC
		{
			static bool Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
			{
				PlayerControl player = __instance.myPlayer;
				RpcCalls RpcId = (RpcCalls)callId;

				RpcHandlers.TryGetValue(RpcId, out RpcCheck rpcCheck);
				if(!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

				if(rpcCheck.GetExpectedNetObject() != typeof(PlayerPhysics))
				{
					// Recieved a RPC that should've been sent for a different net object, some sort of exploit attempt?
					return false;
				}

				int oldReadPosition = reader.Position;
				bool blockRpc = false;

				rpcCheck.Validate(player, reader, ref blockRpc);

				if(DiscardRPC && !blockRpc)
				{
					// Put the read position back to its previous spot to not mess up the HandleRpc function
					reader.Position = oldReadPosition;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		class OnNetTransformRPC
		{
			static bool Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
			{
				PlayerControl player = __instance.myPlayer;
				RpcCalls RpcId = (RpcCalls)callId;

				RpcHandlers.TryGetValue(RpcId, out RpcCheck rpcCheck);
				if(!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

				if(rpcCheck.GetExpectedNetObject() != typeof(CustomNetworkTransform))
				{
					// Recieved a RPC that should've been sent for a different net object, some sort of exploit attempt?
					return false;
				}

				int oldReadPosition = reader.Position;
				bool blockRpc = false;

				rpcCheck.Validate(player, reader, ref blockRpc);

				if(DiscardRPC && !blockRpc)
				{
					// Put the read position back to its previous spot to not mess up the HandleRpc function
					reader.Position = oldReadPosition;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix(ShipStatus __instance, byte callId, MessageReader reader)
			{
				RpcCalls RpcId = (RpcCalls)callId;

				RpcHandlers.TryGetValue(RpcId, out RpcCheck rpcCheck);
				if(!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

				if(rpcCheck.GetExpectedNetObject() != typeof(ShipStatus))
				{
					// Recieved a RPC that should've been sent for a different net object, some sort of exploit attempt?
					return false;
				}

				int oldReadPosition = reader.Position;
				bool blockRpc = false;

				rpcCheck.Validate(null, reader, ref blockRpc);

				if(DiscardRPC && !blockRpc)
				{
					// Put the read position back to its previous spot to not mess up the HandleRpc function
					reader.Position = oldReadPosition;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static void Flag(PlayerControl player, string reason, bool shouldPunish = true)
		{
			Hydra.notifications.Send("Anticheat", reason, NotificationDuration);

			if(!AmongUsClient.Instance.AmHost || !shouldPunish) return;

			switch(punishment)
			{
				case Punishments.None:
					break;

				case Punishments.Kick:
				case Punishments.ErrorKick:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} was kicked by Hydra Anticheat for hacking");

					// The vanilla anticheat prevents using the ErrorKick method if the game has not started yet
					if(punishment == Punishments.Kick || LobbyBehaviour.Instance != null)
					{
						AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
					}
					else
					{
						// When a game starts, the host waits around ten seconds to wait for all clients to send the ClientReady game message
						// If the ten second timer is reached without a ClientReady game message being received by the host, the host will kick the player due to timeout
						// The kick message shown to the player will explain that the player has a poor internet connection or that their device is too old
						// and in-game, players will be shown that the player left due to an error instead of being kicked
						// Any other disconnection messages other than ClientTimeout will result in the vanilla anticheat banning us from the lobby
						AmongUsClient.Instance.SendLateRejection(player.OwnerId, DisconnectReasons.ClientTimeout);
					}
					break;

				case Punishments.Ban:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} was automatically banned by Hydra Anticheat for hacking");
					AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
					break;
			}
		}
	}
}