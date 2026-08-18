using HarmonyLib;
using UnityEngine;

namespace HydraMenu.features
{
    internal class Visuals
    {

		[HarmonyPatch(typeof(ShhhBehaviour), nameof(ShhhBehaviour.PlayAnimation))]
		public static class SkipShhhAnimation
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix()
			{
				if(Enabled)
				{
					HudManager.Instance.shhhEmblem.gameObject.SetActive(false);
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(LogicOptionsHnS), nameof(LogicOptionsHnS.GetCrewmateLeadTime))]
		public static class NoSeekerAnimationPatch
		{
			 public static bool Enabled { get; set; } = true;
			
			 public static bool Prefix(ref int __result)
			 {
				 if(Enabled)
				 {
					 __result = 0;
					 return false;
				 }
				 else
				 {
					 return true;
				 } 
			 }
		}

		// PlayerControl::FixedUpdate sets PlayerControl::set_Visible to false if the player is dead, or true if the player is alive
		// The set_Visible function runs CosmeticsLayer::set_Visible in order to hide or show the player's cosmetics
		// If we want to show ghosts even if we are alive, then we can reimplement PlayerControl::set_Visible and make it so player cosmetics are always visible
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Visible), MethodType.Setter)]
		public static class ShowGhosts
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(PlayerControl __instance)
			{
				if(Enabled && __instance.Data.IsDead)
				{
					__instance.cosmetics.Visible = true;
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public static class SpectatePlayer
		{
			private static bool _enabled = false;
			private static bool wasShadowsEnabled = false;

			public static PlayerControl target;

			public static bool Enabled
			{
				get { return _enabled; }
				set
				{
					if(_enabled == value) return;
					_enabled = value;

					FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();

					if(value)
					{
						camera.SetTarget(target);
						wasShadowsEnabled = HudManager._instance.ShadowQuad.gameObject.active;
						HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
					}
					else
					{
						camera.SetTarget(PlayerControl.LocalPlayer);

						if(wasShadowsEnabled) HudManager.Instance.ShadowQuad.gameObject.SetActive(true);
					}
				}
			}
		}
	}
}