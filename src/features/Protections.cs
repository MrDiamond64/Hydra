using HarmonyLib;
using InnerNet;

namespace HydraMenu.features
{
	internal class Protections
	{
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
				if(!Enabled || callId != (byte)RpcCalls.SnapTo || __instance.myPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;

				Hydra.Log.LogMessage($"Recived SnapTo RPC for our player, since block server teleports is enabled we will disregard the RPC");
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

		[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
		public static class Votekicks
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(int srcClient, int clientId)
			{
				Hydra.Log.LogInfo($"[VotekickLogger] {srcClient} voted to kick out {clientId}");
				if(clientId != PlayerControl.LocalPlayer.OwnerId) return true;

				ClientData player = AmongUsClient.Instance.GetClient(srcClient);

				Hydra.notifications.Send("Votekick Logger", $"{player.PlayerName} has voted to kick you out.");

				// Prevent players from being able to votekick you as host
				return !(Enabled && AmongUsClient.Instance.AmHost);
			}
		}
	}
}