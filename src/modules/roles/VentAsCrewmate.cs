using HarmonyLib;
using UnityEngine;

namespace HydraMenu.modules.roles
{
	internal class VentAsCrewmate : Module
	{
		public VentAsCrewmate() : base("VentAsCrewmate")
		{
			base.Enabled = true;
		}

		private static VentAsCrewmate Instance
		{
			get { return ModuleManager.ventAsCrewmate; }
		}

		// Similar to being able to use the sabotage button while crewmate, the vent button also has checks to make sure the current player can actually vent, so we have to reimplement the Vent::CanUse function
		// The normal function also has checks to make sure the vent isn't being cleaned, however that isn't important so we don't reimplement those checks
		[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
		class SkipVentChecks
		{
			static bool Prefix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
			{
				if(!Instance.Enabled) return true;

				PlayerControl player = pc.Object;
				if(pc.IsDead) return true;

				couldUse = true;
				__result = Vector2.Distance(player.Collider.bounds.center, __instance.transform.position);

				bool isObstructed = PhysicsHelpers.AnythingBetween(player.Collider, player.Collider.bounds.center, __instance.transform.position, Constants.ShipOnlyMask, false);
				if(__result <= __instance.UsableDistance && !isObstructed) canUse = true;

				return false;
			}
		}

		protected override void OnDisable()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType)) return;

			HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
			if(Vent.currentVent != null)
			{
				Vent.currentVent.SetButtons(false);
				PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
			}
		}
	}
}