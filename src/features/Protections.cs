using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace HydraMenu.features
{
	internal class Protections
	{
		public static bool BlockLargeGameMessages { get; set; } = true;
		public static bool BlockInvalidGameDataMessages { get; set; } = true;
		public static bool BlockUnauthorizedSystemUpdates { get; set; } = true;
		public static bool ProtectAgainstNonHostKickExploit { get; set; } = true;

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SetEndpoint))]
		public static class ForceDTLS
		{
			public static bool Enabled { get; set; } = true;

			static void Prefix(ref bool dtls)
			{
				if(Enabled) dtls = true;
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		public static class BlockServerTeleports
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(CustomNetworkTransform __instance, byte callId)
			{
				if(!Enabled || callId != (byte)RpcCalls.SnapTo || __instance.myPlayer != PlayerControl.LocalPlayer) return true;

				Hydra.Log.LogMessage($"Received SnapTo RPC for our player, since block server teleports is enabled we will disregard the RPC");
				return false;
			}
		}

		// Among Us had this bug where if you reported the body of a player who has left, the anticheat would incorrectly ban you from the lobby
		// To prevent incorrect lobby bans, we block reporting bodies of players who left
		/*
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
		public static class PreventReportBan
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(PlayerControl __instance, NetworkedPlayerInfo target)
			{
				if(
					// Only make it so only our body reports run
					__instance.NetId == PlayerControl.LocalPlayer.NetId &&
					// Make sure its not an emergency meeting
					target != null &&
					target.Disconnected &&
					// Hosts are exempt from the anticheat detection
					!AmongUsClient.Instance.AmHost
				)
				{
					Hydra.notifications.Send("Protections Alert", $"Saved you from getting banned by {target.PlayerName}'s glitched body.");
					return false;
				}
				return true;
			}
		}
		*/

		[HarmonyPatch(typeof(MessageReader), nameof(MessageReader.ReadPackedUInt32))]
		public static class HardenedReadPackedUInt
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(MessageReader __instance, ref uint __result)
			{
				if(!Enabled) return true;

				bool readMore = true;
				int shift = 0;
				uint output = 0;

				while(readMore)
				{
					if(__instance.BytesRemaining < 1) break;

					byte b = __instance.ReadByte();
					if(b >= 0x80)
					{
						readMore = true;
						b ^= 0x80;
					}
					else
					{
						readMore = false;
					}

					output |= (uint)(b << shift);
					shift += 7;
				}

				__result = output;
				return false;
			}
		}

		[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
		public static class Votekicks
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(int srcClient, int clientId)
			{
				Hydra.Log.LogInfo($"[VotekickLogger] {srcClient} voted to kick out {clientId}");
				if(clientId != PlayerControl.LocalPlayer.OwnerId) return true;

				ClientData player = AmongUsClient.Instance.FindClientById(srcClient);
				if(player == null) return false;

				Hydra.notifications.Send("Votekick Logger", $"{player.PlayerName} has voted to kick you out.");

				// Prevent players from being able to votekick you as host
				return !(Enabled && AmongUsClient.Instance.AmHost);
			}
		}

		[HarmonyPatch(typeof(AmongUsClient), nameof(InnerNetClient.CoStartGame))]
		public static class BypassShapeshiftRatelimits
		{
			public static bool Enabled { get; set; } = true;

			static void Postfix()
			{
				if(!Enabled || !AmongUsClient.Instance.AmHost) return;

				PlayerControl player = Utilities.GetRandomPlayer();
				if(player == null) return;

				// Shapeshifting and reverting shapeshifts have strict ratelimits for the host, which can impact the Mass Shapeshift feature in Host options
				// We can bypass these ratelimits by sending a game options update and setting the shapeshift cooldown to zero seconds
				IGameOptions options = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				options.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0.0f);

				// Send the settings update to a random player, we don't want to mess up our saved lobby settings
				GameOptions.SendGameOptionsToClient(options, player.OwnerId);
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleRpc))]
		public static class MemoryAllocationOverload
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(byte callId, MessageReader reader)
			{
				if(!Enabled || callId != (byte)RpcCalls.VotingComplete) return true;

				int oldReadPosition = reader.Position;

				// The game creates an array with the size of the following value
				// If this value is very large, then the client will attempt to allocate several gigabytes of memory
				int arrayLength = reader.ReadPackedInt32();

				if(arrayLength > 1024 || arrayLength > reader.BytesRemaining)
				{
					return false;
				}

				reader.Position = oldReadPosition;
				return true;
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;

				switch((RpcCalls)callId)
				{
					case RpcCalls.CloseDoorsOfType:
						// Only the host should receive CloseDoorsOfType RPCs
						if(BlockUnauthorizedSystemUpdates && !AmongUsClient.Instance.AmHost) return false;
						break;

					case RpcCalls.UpdateSystem:
						SystemTypes system = (SystemTypes)reader.ReadByte();
						PlayerControl player = reader.ReadNetObject<PlayerControl>();

						if(ProtectAgainstNonHostKickExploit && system == SystemTypes.Ventilation && !AmongUsClient.Instance.AmHost)
						{
							Hydra.notifications.Send("Protections Alert", $"{player.Data.PlayerName} attempted to use the VentilationSystem kick exploit on you!");
							return false;
						}

						// Only the host should receive CloseDoorsOfType RPCs
						if(BlockUnauthorizedSystemUpdates && !AmongUsClient.Instance.AmHost) return false;
						break;
				}

				reader.Position = oldReadPosition;
				return true;
			}
		}
	}
}