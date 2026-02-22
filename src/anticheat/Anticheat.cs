using HarmonyLib;
using Hazel;

namespace HydraMenu.anticheat
{
	internal class Anticheat
	{
		public static bool Enabled { get; set; } = true;
		public static bool Autoban { get; set; } = false;

		public static bool CheckSpoofedPlatforms { get; set; } = true;
		public static bool CheckSpoofedLevels { get; set; } = true;
		public static bool CheckInvalidCompleteTask { get; set; } = true;
		public static bool CheckInvalidPlayAnimation { get; set; } = true;
		public static bool CheckInvalidScan { get; set; } = true;
		public static bool CheckInvalidSnapTo { get; set; } = true;
		public static bool CheckInvalidStartCounter { get; set; } = true;
		public static bool CheckInvalidSystemUpdates { get; set; } = true;
		public static bool CheckInvalidVent { get; set; } = true;

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

				if(!blockRpc)
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

				if(!blockRpc)
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

				if(!blockRpc)
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
						// It would be nice if we could also add checks for CloseDoorOfType RPCs, however we are not able to determine who is sending that RPC
						break;

					case RpcCalls.UpdateSystem:
						InvalidSystemUpdate.OnSystemUpdate(reader, ref blockRpc);
						break;
				}

				if(!blockRpc)
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

		public static void Punish(PlayerControl player)
		{
			if(!Autoban || !AmongUsClient.Instance.AmHost) return;

			AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
			Hydra.Log.LogMessage($"{player.Data.PlayerName} was automatically banned by Hydra Anticheat for hacking.");
		}
	}
}