using HarmonyLib;
using InnerNet;

namespace HydraMenu.features
{
	internal class Host
	{
		// When a player reports a body, their client sends a ReportDeadBody RPC to the host. The host then should validate the RPC and start a meeting
		// To block meetings, we can simply ignore any received ReportDeadBody RPCs
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
		public static class DisableMeetings
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix()
			{
				return !Enabled;
			}
		}

		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
		public static class DisableSabotages
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix()
			{
				return !Enabled;
			}
		}

		[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldFlipSkeld))]
		public static class FlippedSkeld
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix(ref bool __result)
			{
				__result = Enabled;
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetLevel))]
		public static class BlockLowLevels
		{
			public static bool Enabled { get; set; } = false;
			public static uint MinLevel { get; set; } = 20;

			static void Prefix(PlayerControl __instance, uint level)
			{
				if(!Enabled || !AmongUsClient.Instance.AmHost || __instance.PlayerId == PlayerControl.LocalPlayer.PlayerId|| level > MinLevel) return;

				Hydra.notifications.Send("Block Low Levels", $"{__instance.Data.PlayerName} is level {level}, which is below the level threshold. They will be kicked from the game.");
				AmongUsClient.Instance.KickPlayer(__instance.OwnerId, false);
			}
		}

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanBan))]
		public static class BanMidGame
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(InnerNetClient __instance, ref bool __result)
			{
				if(!Enabled) return true;

				__result = __instance.AmHost;
				return false;
			}
		}

		[HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
		public static class DisableGameEnd
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix()
			{
				return !Enabled;
			}
		}
	}
}