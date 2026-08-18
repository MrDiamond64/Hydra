using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	public class AlwaysVisibleChat : Module
	{
		public AlwaysVisibleChat() : base("AlwaysVisibleChat")
		{
			Enabled = true;
		}

		public static AlwaysVisibleChat Instance
		{
			get { return ModuleManager.alwaysVisibleChat; }
		}

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.SetVisible))]
		class SetChatVisibility
		{
			static void Prefix(ref bool visible)
			{
				if(Instance.Enabled) visible = true;
			}
		}
	}
}