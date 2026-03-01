using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace HydraMenu.features
{
	internal class Roles : MonoBehaviour
	{
		public static bool DisableShapeshiftAnimation { get; set; } = false;
		// public static bool DisablePhantomEndAnimation { get; set; } = false;
		public static bool AllowVentingForCrewmates { get; set; } = true;

		public void Update()
		{
			if(AmongUsClient.Instance == null || !AmongUsClient.Instance.AmConnected|| HudManager.Instance == null || ShipStatus.Instance == null) return;

			if(SkipSabotageChecks.SabotageAsCrewmate) HudManager.Instance.SabotageButton.gameObject.SetActive(true);
			if(AllowVentingForCrewmates) HudManager.Instance.ImpostorVentButton.gameObject.SetActive(true);
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
		class ShapeshiftStart
		{
			static void Prefix(ref bool shouldAnimate)
			{
				if(DisableShapeshiftAnimation) shouldAnimate = false;
			}
		}

		// PlayerControl::CmdCheckRevertShapeshift just runs the PlayerControl::CmdCheckShapeshift function which we patch above, however for some reason we are not able to set shouldAnimate to false
		// My guess to why this happens is that the CmdCheckShapeshift function is getting inlined here so it doesn't actually get ran
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
		class ShapeshiftEnd
		{
			static void Prefix(ref bool shouldAnimate)
			{
				if(DisableShapeshiftAnimation) shouldAnimate = false;
			}
		}

		/*
		// Buggy, kill and use buttons don't work after unvanishing
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
		class PhantomEnd
		{
			static void Prefix(ref bool shouldAnimate)
			{
				if(DisablePhantomEndAnimation) shouldAnimate = false;
			}
		}
		*/

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
		public static class NoKillCooldown
		{
			public static bool Enabled { get; set; } = false;

			static void Prefix(PlayerControl __instance, ref float time)
			{
				if(!Enabled || __instance.PlayerId != PlayerControl.LocalPlayer.PlayerId) return;

				time = 0;
			}
		}

		// Clicking the sabotage button has checks to make sure the current player is indeed an imposter, not in a vent, and that the current gamemode supports sabotages
		// This means setting the GameObject's sabotage button state to active wont allow crewmates to sabotage alone, we need to override the DoClick function to not have those checks
		[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
		public static class SkipSabotageChecks
		{
			public static bool SabotageAsCrewmate { get; set; } = false;
			public static bool SabotageInVents { get; set; } = false;

			static bool Prefix()
			{
				PlayerControl player = PlayerControl.LocalPlayer;

				// We have to limit this to Imposters as the crewmate exit vent button will be on the same position as the imposter sabotage button
				if(!SabotageInVents && player.inVent && !RoleManager.IsImpostorRole(player.Data.RoleType)) return true;

				HudManager.Instance.ToggleMapVisible(new MapOptions { Mode = MapOptions.Modes.Sabotage });
				return false;
			}
		}

		// Similiar to being able to use the sabotage button while crewmate, the vent button also has checks to make sure the current player can actually vent, so we have to reimplement the Vent::CanUse function
		// The normal function also has checks to make sure the vent isn't being cleaned, however that isn't important so we don't reimplement those checks
		[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
		class SkipVentChecks
		{
			static bool Prefix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
			{
				if(!AllowVentingForCrewmates) return true;

				PlayerControl player = pc.Object;
				if(pc.IsDead) return true;

				couldUse = true;
				__result = Vector2.Distance(player.Collider.bounds.center, __instance.transform.position);

				bool isObstructed = PhysicsHelpers.AnythingBetween(player.Collider, player.Collider.bounds.center, __instance.transform.position, Constants.ShipOnlyMask, false);
				if(__result <= __instance.UsableDistance && !isObstructed) canUse = true;

				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
		public static class MoveModifier
		{
			public static bool MoveInVents { get; set; } = true;

			static bool Prefix(PlayerControl __instance, ref bool __result)
			{
				if(HudManager.Instance.Chat.IsOpenOrOpening) return true;

				if(__instance.inVent && MoveInVents)
				{
					__result = true;
					return false;
				}

				return true;
			}
		}

		public static void UpdateRole(RoleTypes role)
		{
			bool isGhost = RoleManager.IsGhostRole(role);

			// When a player turns into the ghost, the PlayerControl::CoSetRole function hides the report button. This function then calls the RoleManager::SetRole function we call here
			// This means when we are changing between normal or ghost roles, the report button will not properly be added/removed, so we have to reimplement it here
			// We also cannot use PlayerControl::CoSetRole directly as it prevents in-game roles being overriden by non-ghosts ones (we could just patch it and disable overriding, however a blackout occurs when the game starts)
			HudManager.Instance.ReportButton.gameObject.SetActive(!isGhost);

			RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, role);
		}
	}

}