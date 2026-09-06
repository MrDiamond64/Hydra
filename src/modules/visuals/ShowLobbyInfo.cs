using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace LunarMenu.modules.visuals
{
    internal class ShowLobbyInfo : Module
    {
        public ShowLobbyInfo() : base("ShowLobbyInfo") { }

        private static ShowLobbyInfo Instance
        {
            get { return ModuleManager.showLobbyInfo; }
        }

        [HarmonyPatch(typeof(GameContainer), nameof(GameContainer.SetupGameInfo))]
        class LobbyInfo
        {
            static void Postfix(GameContainer __instance)
            {
                if (!Instance.Enabled) return;

                var platform = Utilities.GetPlatformString(__instance.gameListing.Platform);

                string lobbyCode = DataManager.Settings.Gameplay.StreamerMode ? "******" : GameCode.IntToGameName(__instance.gameListing.GameId);
                int age = Mathf.Max(0, (int)__instance.gameListing.Age);

                string playerCount = "<#0f0>";
                if (__instance.gameListing.PlayerCount == 4) playerCount = "<#ff0>";
                if (__instance.gameListing.PlayerCount < 4) playerCount = "<#f00>";

                playerCount += __instance.capacity.text;
                string trueHostName = __instance.gameListing.TrueHostName;

                string separator = "<#0000>000000000000000</color>";

                __instance.capacity.text = $"<size=40%>{separator}\n<#fb0>{trueHostName}</color>\n{playerCount}\n<#0bf>{lobbyCode}</color>\n<#b0f>{platform}</color>\n" +
                    $"<#0f0>Age: {age / 60}:{(age % 60 < 10 ? "0" : "")}{age % 60}</color>\n{separator}</size>";
            }
        }
    }
}
