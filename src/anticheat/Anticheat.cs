using HarmonyLib;
using Hazel;

namespace HydraMenu.anticheat
{
	internal class Anticheat
	{
		public enum Punishments
		{
			None,
			Kick,
			ErrorKick,
			Ban
		}

		public static bool Enabled { get; set; } = true;

		public static bool CheckSpoofedPlatforms { get; set; } = true;
		public static bool CheckSpoofedLevels { get; set; } = true;
		public static bool CheckInvalidCloseDoors { get; set; } = true;
		public static bool CheckInvalidCompleteTask { get; set; } = true;
		public static bool CheckInvalidPlayAnimation { get; set; } = true;
		public static bool CheckInvalidScan { get; set; } = true;
		public static bool CheckInvalidSnapTo { get; set; } = true;
		public static bool CheckInvalidStartCounter { get; set; } = true;
		public static bool CheckInvalidSystemUpdates { get; set; } = true;
		public static bool CheckInvalidVent { get; set; } = true;

		public static float NotificationDuration = 10.0f;

		public static Punishments punishment = Punishments.None;
		public static bool DiscardRPC = true;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
		class OnPlayerControlRPC
		{
			static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;
				RpcCalls RpcId = (RpcCalls)callId;

				bool blockRpc = false;
				switch(RpcId)
				{
					case RpcCalls.PlayAnimation:
						InvalidPlayAnimation.OnPlayAnimation(__instance, reader, ref blockRpc);
						break;

					case RpcCalls.CompleteTask:
						InvalidCompleteTask.OnCompleteTask(__instance, reader, ref blockRpc);
						break;

					case RpcCalls.SetLevel:
						InvalidSetLevel.OnSetLevel(__instance, reader, ref blockRpc);
						break;

					case RpcCalls.SetScanner:
						InvalidScanner.OnSetScanner(__instance, reader, ref blockRpc);
						break;

					case RpcCalls.SetStartCounter:
						InvalidStartCounter.OnSetStartCounter(__instance, reader, ref blockRpc);
						break;
				}

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
				int oldReadPosition = reader.Position;
				RpcCalls RpcId = (RpcCalls)callId;
				PlayerControl player = __instance.myPlayer;

				bool blockRpc = false;
				switch(RpcId)
				{
					case RpcCalls.EnterVent:
						InvalidVent.OnPlayerEnterVent(player, reader, ref blockRpc);
						break;

					case RpcCalls.ExitVent:
						InvalidVent.OnPlayerExitVent(player, reader, ref blockRpc);
						break;
				}

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
				int oldReadPosition = reader.Position;
				RpcCalls RpcId = (RpcCalls)callId;
				PlayerControl player = __instance.myPlayer;

				bool blockRpc = false;
				switch(RpcId)
				{
					case RpcCalls.SnapTo:
						InvalidSnapTo.OnSnapTo(player, reader, ref blockRpc);
						break;
				}

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
				int oldReadPosition = reader.Position;
				RpcCalls RpcId = (RpcCalls)callId;

				bool blockRpc = false;
				switch(RpcId)
				{
					case RpcCalls.CloseDoorsOfType:
						InvalidCloseDoors.OnDoorClose(reader, ref blockRpc);
						break;

					case RpcCalls.UpdateSystem:
						InvalidSystemUpdates.OnSystemUpdate(reader, ref blockRpc);
						break;
				}

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