using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class ShowGhostMessages : Module
	{
		public ShowGhostMessages() : base("ShowGhostMessages")
		{
			Enabled = true;
		}

		public static ShowGhostMessages Instance
		{
			get { return ModuleManager.showGhostMessages; }
		}

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
		public static class OnChat
		{
			static void Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
			{
				if(sourcePlayer == null) return;

				// This is kind of a hacky workaround to be able to see messages by ghosts
				// The game has no easy way to show messages by ghosts, so we would have to completely reimplement the ChatController::AddChat function
				// I don't really like reimplementing large functions as it makes backwards compatability harder and requires more effort when updating the mod to newer versions of AU
				// Instead of having to reimplement the function, we can just use ChatController::AddChatWarning to add a chat bubble and include the player's name and message contents to the warning
				if(Instance.Enabled && !PlayerControl.LocalPlayer.Data.IsDead && sourcePlayer.Data.IsDead)
				{
					__instance.AddChatWarning($"{sourcePlayer.Data.PlayerName}\n{chatText}");
				}
			}
		}
	}
}