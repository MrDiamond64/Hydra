using HarmonyLib;
using InnerNet;

namespace HydraMenu.features
{
    internal class PlayerLogger
    {
        // We could technically patch PlayerControl::Start in order to determine when a player join's a lobby, however at that point the player's name or cosmetics have not been loaded as they client sends them after the initial join
        // If we want to be able to determine when a player join's a lobby with their name loaded, we just hook the SetName RPC handler which is only sent on join
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetName))]
        class OnJoin
        {
            static void Postfix(PlayerControl __instance)
            {
                if(__instance.PlayerId == PlayerControl.LocalPlayer?.PlayerId || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;

                ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
                if(clientData == null) return;

				PlatformSpecificData platformData = clientData.PlatformData;

				Hydra.Log.LogMessage($"[PlayerLogger] {__instance.Data.PlayerName} ({__instance.NetId}) joined on {platformData.Platform}. friendcode {clientData.FriendCode}, puid {clientData.ProductUserId}");

            }
        }
    }
}