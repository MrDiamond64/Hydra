using HarmonyLib;
using Hazel;
using UnityEngine;

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
			static void Prefix(PlayerControl __instance, byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;
				RpcCalls RpcId = (RpcCalls)callId;

				switch(RpcId)
				{
					case RpcCalls.PlayAnimation:
						TaskTypes task = (TaskTypes)reader.ReadByte();

						InvalidPlayAnimation.OnPlayAnimation(__instance, task);
						break;

					case RpcCalls.CompleteTask:
						uint taskIndex = reader.ReadUInt32();

						InvalidCompleteTask.OnCompleteTask(__instance, taskIndex);
						break;

					case RpcCalls.SetLevel:
						uint level = reader.ReadPackedUInt32();

						InvalidSetLevel.OnSetLevel(__instance, level);
						break;

					case RpcCalls.SetScanner:
						bool scanning = reader.ReadBoolean();
						byte seqId = reader.ReadByte();

						InvalidScanner.OnSetScanner(__instance, scanning, seqId);
						break;

					case RpcCalls.SetStartCounter:
						int seqId2 = reader.ReadPackedInt32();
						sbyte counter = reader.ReadSByte();

						InvalidStartCounter.OnSetStartCounter(__instance, counter, seqId2);
						break;
				}

				// Put the read position back to its previous spot to not mess up the HandleRpc function
				reader.Position = oldReadPosition;
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
		class OnPlayerPhysicsRPC
		{
			static void Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;
				RpcCalls RpcId = (RpcCalls)callId;
				PlayerControl player = __instance.myPlayer;

				switch(RpcId)
				{
					case RpcCalls.EnterVent:
						InvalidVent.OnPlayerEnterVent(player);
						break;

					case RpcCalls.ExitVent:
						InvalidVent.OnPlayerExitVent(player);
						break;
				}

				// Put the read position back to its previous spot to not mess up the HandleRpc function
				reader.Position = oldReadPosition;
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		class OnNetTransformRPC
		{
			static void Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;
				RpcCalls RpcId = (RpcCalls)callId;
				PlayerControl player = __instance.myPlayer;

				switch(RpcId)
				{
					case RpcCalls.SnapTo:
						Vector2 position = NetHelpers.ReadVector2(reader);
						ushort seqId = reader.ReadUInt16();

						InvalidSnapTo.OnSnapTo(player, position, seqId);
						break;
				}

				// Put the read position back to its previous spot to not mess up the HandleRpc function
				reader.Position = oldReadPosition;
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static void Prefix(ShipStatus __instance, byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;
				RpcCalls RpcId = (RpcCalls)callId;

				// It would be nice if we could also add checks for CloseDoorOfType RPCs, however we are not able to determine who is sending that RPC
				/*
				switch(RpcId)
				{
					case RpcCalls.CloseDoorOfType:
						break;
				}
				*/

				// Put the read position back to its previous spot to not mess up the HandleRpc function
				reader.Position = oldReadPosition;
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