using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace LunarMenu
{
    internal class GameState
    {
        public static string CurrentScene;

        public static bool GameLoaded = false;
        public static bool InMeeting = false;

        public static byte VoteKicks = 0;

        public static Dictionary<byte, byte> voteMonitor = [];
        public static List<byte> vanishedPlayers = [];

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

        static void onGameEnd()
        {
            try
            {
                vanishedPlayers.Clear();
                VoteKicks = 0;
                GameLoaded = false;
            } catch { }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        class GameEnd
        {
            static void Prefix()
            {
                try
                {
                    onGameEnd();
                } catch { }
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
        class DisconnectUnloadGame
        {
            static void Prefix(InnerNetClient __instance)
            {
                try
                {
                    if (__instance.GameState == InnerNetClient.GameStates.Started || __instance.GameState == InnerNetClient.GameStates.Joined || __instance.NetworkMode == NetworkModes.FreePlay)
                        onGameEnd();
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
                    onGameEnd();
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
        class AddVoteKick
        {
            static void Prefix(int clientId)
            {
                try
                {
                    if (clientId == PlayerControl.LocalPlayer.OwnerId)
                        VoteKicks++;
                } catch { }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
        class ResetVoteMonitor
        {
            static void Prefix()
            {
                try
                {
                    voteMonitor.Clear();
                    InMeeting = true;
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetRoleInvisibility))]
        class AddVanishedPlayer
        {
            static void Prefix(PlayerControl __instance, bool isActive)
            {
                var pData = __instance?.Data;
                if (pData != null && pData.RoleType == RoleTypes.Phantom && isActive)
                    vanishedPlayers.Add(__instance.PlayerId);
                else if (vanishedPlayers.Contains(__instance.PlayerId))
                    vanishedPlayers.Remove(__instance.PlayerId);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
        class ResetVanishedPlayers
        {
            static void Prefix()
            {
                vanishedPlayers.Clear();
                try
                {
                    InMeeting = false;
                } catch { }
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
        class ClientUpdate
        {
            static void Prefix()
            {
                try
                {
                    if (!Utilities.InGame())
                        InMeeting = false;
                } catch { }
            }
        }
    }
}
