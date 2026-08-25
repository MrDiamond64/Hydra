using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class NoKillChecks : Module
	{
		public NoKillChecks() : base("NoKillChecks") { }

		private static NoKillChecks Instance
		{
			get { return ModuleManager.noKillChecks; }
		}

		[HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.IsValidTarget))]
		class NoNormalKillChecks
		{
			static bool Prefix(NetworkedPlayerInfo target, ref bool __result)
			{
				if(!Instance.Enabled || target == PlayerControl.LocalPlayer.Data) return true;

				__result = true;
				return false;
			}
		}

		[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
		class NoImpKillChecks
		{
			static bool Prefix(NetworkedPlayerInfo target, ref bool __result)
			{
				if(!Instance.Enabled || target == PlayerControl.LocalPlayer.Data) return true;

				__result = true;
				return false;
			}
		}
	}
}