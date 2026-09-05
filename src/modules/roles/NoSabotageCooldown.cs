using HarmonyLib;

namespace LunarMenu.modules.roles
{
	internal class NoSabotageCooldown : Module
	{
		public NoSabotageCooldown() : base("NoSabotageCooldown")
		{
			base.Enabled = true;
		}

		private static NoSabotageCooldown Instance
		{
			get { return ModuleManager.noSabotageCooldown; }
		}

		[HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.CanUseSabotage), MethodType.Getter)]
		class AllowSabotageButton
		{
			static bool Prefix(ref bool __result)
			{
				if(!Instance.Enabled) return true;

				__result = true;
				return false;
			}
		}

		// The SabotageSystemType has checks to ensure that sabotages are not on cooldown
		// To bypass this we need to forcefully call the sabotage
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), [ typeof(SystemTypes), typeof(byte) ])]
		class ForceSabotage
		{
			static bool Prefix(SystemTypes systemType, byte amount)
			{
				if(!Instance.Enabled || systemType != SystemTypes.Sabotage) return true;

				Sabotage.SabotageSystem((SystemTypes)amount);
				return false;
			}
		}
	}
}