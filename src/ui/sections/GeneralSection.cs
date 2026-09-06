using LunarMenu.modules;
using UnityEngine;

namespace LunarMenu.ui.sections
{
	internal class GeneralSection : Section
	{
		public GeneralSection() : base("General") { }

		public override void Render() {
			GUILayout.Label("Welcome to Lunar! Lunar is a utility, moderation, and a trolling menu made to enhance the Among Us playing experience. We offer quality of life features, features to goof off with a closed group of friends and have a laugh, and features to help defend your lobbies from malicious players. Much of the feature-set included falls in the trolling group, as I personally made this to use in private lobbies with my friends. I hope you (and the people you play with) too are able to have fun using Lunar to play, or to protect yourself from hackers.\n\nSince some of Lunar's features can be used for cheating, I must make this explicity clear: Lunar should not be used to impair other player's experiences. Some people may have gotten off from a long day of work or school and just want to play a chill game of Among Us. By using Lunar to destroy lobbies, you are ruining people's day and robbing them out of enjoyment. If that does not convince you enough, then you should be aware that abusing mods for malicious purposes may result in a sanction being placed on your account.");

            ModuleManager.autoCopyLobbyCode.Enabled = GUILayout.Toggle(ModuleManager.autoCopyLobbyCode.Enabled, "Copy Room Code on Disconnect");
            ModuleManager.unlockCosmetics.Enabled = GUILayout.Toggle(ModuleManager.unlockCosmetics.Enabled, "Unlock Cosmetics");

            if (GUILayout.Button("Clear Notifications"))
			{
				Lunar.notifications.ClearNotifications();
				Lunar.notifications.Send("Notifications", "All notifications have been cleared.", 5);
			}
        }
	}
}