using HarmonyLib;
using UnityEngine;

namespace LunarMenu.modules.visuals
{
    internal class ShowFPS : Module
    {
        public ShowFPS() : base("ShowFPS") { }

        private static ShowFPS Instance
        {
            get { return ModuleManager.showFPS; }
        }

        public bool ShowHost { get; set; } = false;

        private static int fps;
        private static int fpsDelay = 0;

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        class ShowFPSPing
        {
            static void Prefix(PingTracker __instance)
            {
                __instance.gamePos = new Vector2(0f, 0f);
            }

            static void Postfix(PingTracker __instance)
            {
                bool isFreeplay = (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay);
                __instance.text.alignment = TMPro.TextAlignmentOptions.Top;
                if (isFreeplay)
                {
                    if (!__instance.gameObject.active) __instance.gameObject.SetActive(true);
                    __instance.aspectPosition.DistanceFromEdge = __instance.gamePos;
                }
                try
                {
                    string sep = " • "
                    int currentFps = (int)Mathf.Round(1f / Time.deltaTime);
                    if (fpsDelay <= 0 || currentFps <= 30)
                    {
                        fps = currentFps;
                        fpsDelay = (int)(0.5 * currentFps);
                    }
                    else fpsDelay--;
                    string fpsText = Instance.Enabled ? sep : "";
                    if (Instance.Enabled)
                    {
                        if (fps <= 20) fpsText += $"<#f00>FPS: {fps}</color>";
                        else if (fps <= 40) fpsText += $"<#ff0>FPS: {fps}</color>";
                        else fpsText += $"<#0f0>FPS: {fps}</color>";
                    }
                    string noClip = PlayerControl.LocalPlayer.Collider.enabled ? "" : (sep + "Noclip");
                    var host = AmongUsClient.Instance.GetHost();
                    string hostText = Instance.ShowHost && Utilities.inGame ? (AmongUsClient.Instance.AmHost ? (sep + "You are Host") : $"{sep}Host: <#{ColorUtility.ToHtmlStringRGB()}>")
                }
            }
        }
    }
}
