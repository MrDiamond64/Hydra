using HarmonyLib;
using UnityEngine;

namespace HydraMenu.modules.roles
{
	internal class MoveInVents : Module
	{
		public MoveInVents() : base("MoveInVents")
		{
			base.Enabled = true;
		}

		private static MoveInVents Instance
		{
			get { return ModuleManager.moveInVents; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
		class MoveModifier
		{
			static bool Prefix(PlayerControl __instance, ref bool __result)
			{
				if(!Instance.Enabled || !__instance.inVent || HudManager.Instance.Chat.IsOpenOrOpening) return true;

				__result = true;
				return false;
			}
		}

		protected override void OnDisable()
		{
			if(PlayerControl.LocalPlayer == null || Vent.currentVent == null) return;

			PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;
			PlayerControl.LocalPlayer.NetTransform.SnapTo(Vent.currentVent.transform.position + Vent.currentVent.Offset);
		}
	}
}