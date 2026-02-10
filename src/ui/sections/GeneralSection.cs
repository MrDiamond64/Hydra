using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
    internal class GeneralSection : ISection
    {
        public GeneralSection()
        {
            sectionName = "General";
        }

        public override void Render() {
            GUILayout.Label("Welcome to Hydra! Hydra is a utility, moderation, and a trolling menu made to enhance the Among Us playing experience. We offer quality of life features, features to goof off with a closed group of friends and have a laugh, and features to help defend your lobbies from malicious players. Much of the feature-set included falls in the trolling group, as I personally made this to use in private lobbies with my friends. I hope you (and the people you play with) too are able to have fun using Hydra to play, or to protect yourself from hackers.\n\nSince some of Hydra's features can be used for cheating, I must make this explicity clear: Hydra should not be used to impair other player's experiences. Some people may have gotten off from a long day of work or school and just want to play a chill game of Among Us. By using Hydra to destroy lobbies, you are ruining people's day and robbing them out of enjoyment. If that does not convince you enough, then you should be aware that abusing mods for malicious purposes may result in a sanction being placed on your account.");

            Chat.OnChat.LogChatMessages = GUILayout.Toggle(Chat.OnChat.LogChatMessages, "Log chat messages to console");

            if(GUILayout.Button("Clear Notifications"))
            {
                Hydra.notifications.ClearNotifications();
                Hydra.notifications.Send("Notifications", "All notifications have been cleared.", 5);
            }
        }
    }
}