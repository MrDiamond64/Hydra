using HarmonyLib;

namespace HydraMenu.modules.host
{
	internal class NoKillCooldown : Module
	{
		public NoKillCooldown() : base("NoKillCooldown") { }

		private static NoKillCooldown Instance
		{
			get { return ModuleManager.noKillCooldown; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
		class KillTimer
		{
			static void Prefix(PlayerControl __instance, ref float time)
			{
				if(!Instance.Enabled || __instance != PlayerControl.LocalPlayer) return;

				time = 0.0f;
			}
		}
	}
}