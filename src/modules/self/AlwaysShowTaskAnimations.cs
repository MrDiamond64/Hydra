using HarmonyLib;
using HydraMenu.network;

namespace HydraMenu.modules.self
{
	internal class AlwaysShowTaskAnimations : Module
	{
		// When PlayerControl::RpcPlayAnimation or PlayerControl::RpcSetScanner is called, they check if visual tasks are on before sending the RPC
		// If we want to be able to send those RPCs even with visual tasks are off, then we will need to reimplement those functions
		// We could just patch LogicOptionsNormal::GetVisualTasks and LogicOptionsHnS::GetVisualTasks, however the latter is optimized out by the Il2Cpp compiler so our patch won't actually get applied
		// meaning this will only show task animations on normal games and not hide and seek as well
		public AlwaysShowTaskAnimations() : base("AlwaysShowTaskAnimations") { }

		private static AlwaysShowTaskAnimations Instance
		{
			get { return ModuleManager.alwaysShowTaskAnimations; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetScanner))]
		class SetScanner
		{
			static bool Prefix(PlayerControl __instance, bool value)
			{
				if(!Instance.Enabled || __instance != PlayerControl.LocalPlayer) return true;

				RPCEmitter.SendSetScanner(value);
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcPlayAnimation))]
		class PlayAnimation
		{
			static bool Prefix(PlayerControl __instance, byte animType)
			{
				if(!Instance.Enabled || __instance != PlayerControl.LocalPlayer) return true;

				RPCEmitter.SendPlayAnimation(animType);
				return false;
			}
		}
	}
}