using HarmonyLib;
using InnerNet;
using UnityEngine.SceneManagement;

namespace LunarMenu
{
    internal class GameState
    {
        public static string CurrentScene;
        public static bool GameLoaded = false;

        [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Internal_ActiveSceneChanged))]
        class UpdateCurrentScene
        {
            static void Prefix(Scene newActiveScene)
            {
                CurrentScene = Scene.GetNameInternal(newActiveScene.m_Handle);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnGameStart))]
        class GameLoad
        {
            static void Prefix()
            {
                try
                {
                    GameLoaded = true;
                } catch { }
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        class GameEnd
        {
            static void Prefix()
            {
                try
                {
                    GameLoaded = false;
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
        class DisconnectUnloadGame
        {
            static void Prefix(InnerNetClient __instance)
            {
                try
                {
                    if (__instance.GameState == InnerNetClient.GameStates.Started || __instance.GameState == InnerNetClient.GameStates.Joined || __instance.NetworkMode == NetworkModes.FreePlay) GameLoaded = false;
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.EnqueueDisconnect))]
        class QueueUnloadGame
        {
            static void Prefix(DisconnectReasons reason, string stringReason)
            {
                try
                {
                    if (reason == DisconnectReasons.Error && (stringReason == "Timeout while waiting for player ID assignment" || stringReason == "Timeout while waiting for player data containers")) return;
                    GameLoaded = false;
                }
                catch { }
            }
        }
    }
}
