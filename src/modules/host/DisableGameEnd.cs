using HarmonyLib;

namespace HydraMenu.modules.host
{
	internal class DisableGameEnd : Module
	{
		public DisableGameEnd() : base("DisableGameEnd") { }

		private static DisableGameEnd Instance
		{
			get { return ModuleManager.disableGameEnd; }
		}

		[HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
		class OnEndGame
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}
	}
}