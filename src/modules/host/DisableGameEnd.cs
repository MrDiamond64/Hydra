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
		public static class OnEndGame
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}
	}
}