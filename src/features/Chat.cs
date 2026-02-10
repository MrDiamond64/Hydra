using HarmonyLib;

namespace HydraMenu.features
{
    internal class Chat
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public static class OnChat
        {
            public static bool LogChatMessages { get; set; } = true;
            public static bool ShowMessagesByGhosts { get; set; } = true;

            static void Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
            {
                if(sourcePlayer == null) return;

                if(LogChatMessages) Hydra.Log.LogMessage($"[ChatLogger] {sourcePlayer.Data.PlayerName}: {chatText}");

                // This is kind of a hacky workaround to be able to see messages by ghosts
                // The game has no easy way to show messages by ghosts, so we would have to completely reimplement the ChatController::AddChat function
                // I don't really like reimplementing large functions as it makes backwards compatability harder and requires more effort when updating the mod to newer versions of AU
                // Instead of having to reimplement the function, we can just use ChatController::AddChatWarning to add a chat bubble and include the player's name and message contents to the warning
                if(ShowMessagesByGhosts && !PlayerControl.LocalPlayer.Data.IsDead && sourcePlayer.Data.IsDead)
                {
                    __instance.AddChatWarning($"{sourcePlayer.Data.PlayerName}\n{chatText}");
                }
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SetVisible))]
        public static class AlwaysVisibleChat
        {
            public static bool Enabled { get; set; } = true;

            static void Prefix(ref bool visible)
            {
                if(Enabled) visible = true;
            }
        }
    }
}