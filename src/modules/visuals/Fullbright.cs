using HarmonyLib;
using UnityEngine;

namespace LunarMenu.modules.visuals
{
	internal class Fullbright : Module
	{
		public Fullbright() : base("Fullbright") { }

		private static Fullbright Instance
		{
			get { return ModuleManager.fullbright; }
		}

		[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
		class HideShadows
		{
			static void Postfix(HudManager __instance)
			{
				__instance.ShadowQuad.gameObject.SetActive(!ModuleManager.spectatePlayer.Enabled && !RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType) && !Instance.Enabled && !(Camera.main.orthographicSize > 3f));
			}
		}
	}
}