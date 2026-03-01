using HarmonyLib;
using AmongUs.Data.Player;

namespace HydraMenu.features
{
	internal class Self
	{
		/*
		[HarmonyPatch(typeof(DataManager), nameof(DataManager.Player.Ban.IsBanned), MethodType.Getter)]
		public static class BypassIntentionalDisconnectionBlocks
		{
			public static bool Enabled { get; set; } = true;

			static void Postfix(ref bool __result)
			{
				if(Enabled) __result = false;
			}
		}
		*/

		// The PlayerControl::RpcSetScanner function has a check to see if visual tasks are disabled before sending the SetScanner RPCs
		// If we want to be able to do the medbay scan animation while visual tasks are off, we can instead run our own RpcSetScanner function which does not have that check
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetScanner))]
		public static class AlwaysDoScanAnimation
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(PlayerControl __instance, bool value)
			{
				if(__instance.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;

				if(Enabled)
				{
					Network.SendSetScanner(value);
					return false;
				} else
				{
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerStatsData), nameof(PlayerStatsData.ValidateStat))]
		public static class UpdateStatsFreeplay
		{
			public static bool Enabled { get; set; } = false;

			static void Prefix(PlayerStatsData __instance)
			{
				if(Enabled)
				{
					__instance.isTrackingStats = true;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]
		public static class PlayerSpeedModifier
		{
			public static float Multiplier { get; set; } = 1;

			static void Postfix(ref float __result)
			{
				__result *= Multiplier;
			}
		}
	}
}