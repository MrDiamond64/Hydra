using HarmonyLib;
using AmongUs.Data.Player;

namespace HydraMenu.features
{
	internal class Self
	{
		// When PlayerControl::RpcPlayAnimation or PlayerControl::RpcSetScanner is called, they check if visual tasks are on before sending the RPC
		// If we want to be able to send those RPCs even with visual tasks are off, then we will need to reimplement those functions
		// We could just patch LogicOptionsNormal::GetVisualTasks and LogicOptionsHnS::GetVisualTasks, however the latter is inlined so our patch won't actually get applied
		// meaning this will only show task animations on normal games and not hide and seek aswell
		public static bool AlwaysShowTaskAnimations { get; set; } = true;

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

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetScanner))]
		class AlwaysDoScanAnimation
		{
			static bool Prefix(PlayerControl __instance, bool value)
			{
				if(__instance.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;

				if(AlwaysShowTaskAnimations)
				{
					Network.SendSetScanner(value);
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcPlayAnimation))]
		class AlwaysDoTaskAnimaton
		{
			static bool Prefix(PlayerControl __instance, byte animType)
			{
				if(__instance.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;

				if(AlwaysShowTaskAnimations)
				{
					Network.SendPlayAnimation(animType);
					return false;
				}
				else
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
			public static float Multiplier { get; set; } = 1.0f;

			static void Postfix(ref float __result)
			{
				__result *= Multiplier;
			}
		}

		[HarmonyPatch(typeof(Ladder), nameof(Ladder.SetDestinationCooldown))]
		public static class NoLadderCooldown
		{
			public static bool Enabled { get; set; } = true;
			static void Postfix(Ladder __instance)
			{
				if(Enabled)
				{
					Hydra.Log.LogMessage($"Used ladder");
					__instance.CoolDown = 0.0f;
					__instance.Destination.CoolDown = 0.0f;
				}
			}
		}

		[HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Begin))]
		public static class UnlimitedMeetings
		{
			public static bool enabled = true;

			static void Prefix()
			{
				if(enabled) PlayerControl.LocalPlayer.RemainingEmergencies = 999999;
			}
		}
	}
}