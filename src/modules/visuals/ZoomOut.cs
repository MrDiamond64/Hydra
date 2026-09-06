using HarmonyLib;
using UnityEngine;

namespace LunarMenu.modules.visuals
{
    internal class ZoomOut : Module
    {
        public ZoomOut() : base("ZoomOut") { }

        private static ZoomOut Instance
        {
            get { return ModuleManager.zoomOut; }
        }

        private static bool _resolutionChangeNeeded = false;

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class ZoomOutPatch
        {
            static void Postfix(HudManager __instance)
            {
                if (Instance.Enabled)
                {
                    if (__instance.Chat.IsOpenOrOpening || MatchInfoGuide.Instance.IsActive || PlayerCustomizationMenu.Instance ||
                        (Utilities.InLobby() && (FriendsListUI.Instance.IsOpen || GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.active || GameStartManager.Instance.RulesEditPanel))) return;

                    _resolutionChangeNeeded = true;

                    if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                        Camera.main.orthographicSize++;
                        __instance.UICamera.orthographicSize++;

                        Utilities.AdjustResolution();
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                        if (!(Camera.main.orthographicSize > 3f)) return;

                        Camera.main.orthographicSize--;
                        __instance.UICamera.orthographicSize--;

                        Utilities.AdjustResolution();
                    }
                }
                else
                {
                    Camera.main.orthographicSize = 3f;
                    __instance.UICamera.orthographicSize = 3f;

                    if (_resolutionChangeNeeded)
                    {
                        Utilities.AdjustResolution();
                        _resolutionChangeNeeded = false;
                    }
                }
            }
        }
    }
}
