using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class ShowProtections : Module
	{
		public ShowProtections() : base("ShowProtections") { }

		public static ShowProtections Instance
		{
			get { return ModuleManager.showProtections; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
		class VisibleProtect
		{
			static void Prefix(ref bool visible)
			{
				if(Instance.Enabled) visible = true;
			}
		}
	}
}