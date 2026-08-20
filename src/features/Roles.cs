using HarmonyLib;
using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.features
{
	internal class Roles : MonoBehaviour
	{
		// public static bool DisablePhantomEndAnimation { get; set; } = false;
		public static bool AllowVentingForCrewmates { get; set; } = true;

		public void Update()
		{
			// If PlayerControl::Data isn't null, then we know the player has fully loaded into the game
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;

			if(SkipSabotageChecks.SabotageAsCrewmate) HudManager.Instance.SabotageButton.gameObject.SetActive(true);
			if(AllowVentingForCrewmates) HudManager.Instance.ImpostorVentButton.gameObject.SetActive(true);

			// The Chat button and Match Info buttons will overlap if both are active in-game (but not in meetings)
			// I tried modifying `MatchInfoButton.transform.position` and the likes to try and shift the button towards the left
			// however that only moved the collider of the button, not the icon
			// So we just use this workaround to hide the Match Info Button in situations where it will not overlap with the Chat button
			if(ModuleManager.alwaysVisibleChat.Enabled)
			{
				HudManager.Instance.MatchInfoButton.gameObject.SetActive(MeetingHud.Instance != null);
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

		// Clicking the sabotage button has checks to make sure the current player is indeed an imposter, not in a vent, and that the current gamemode supports sabotages
		// This means setting the GameObject's sabotage button state to active won't allow crewmates to sabotage alone, we need to override the DoClick function to not have those checks
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

		// Similar to being able to use the sabotage button while crewmate, the vent button also has checks to make sure the current player can actually vent, so we have to reimplement the Vent::CanUse function
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
	}
}