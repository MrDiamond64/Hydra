using HarmonyLib;

namespace LunarMenu.modules.general
{
    internal class AutoCopyLobbyCode : Module
    {
        public AutoCopyLobbyCode() : base("AutoCopyLobbyCode") { }

        private static AutoCopyLobbyCode Instance
        {
            get { return ModuleManager.autoCopyLobbyCode; }
        }

        public static string LastLobbyJoined = "";

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
        class SaveLastLobby
        {
            static void Prefix(string gameIdString)
            {
                try
                {
                    LastLobbyJoined = gameIdString;
                } catch { }
            }
        }

        [HarmonyPatch(typeof(DisconnectPopup), nameof(DisconnectPopup.DoShow))]
        class CopyRoomCode
        {
            static void Postfix(DisconnectPopup __instance)
            {
                bool shouldCopyCode = Instance.Enabled && LastLobbyJoined != "";
                switch (AmongUsClient.Instance.LastDisconnectReason)
                {
                    case DisconnectReasons.Hacking:
                        __instance._textArea.text = $"You were banned for hacking.\n\n{(shouldCopyCode ? "Room code copied to clipboard" : "Please stop.")}";
                        break;
                    default:
                        __instance._textArea.text += shouldCopyCode ? "\n\nRoom code copied to clipboard" : "";
                        break;
                }
                if (shouldCopyCode) ClipboardHelper.PutClipboardString(LastLobbyJoined);
            }
        }
    }
}
