using HarmonyLib;
using InnerNet;

namespace HydraMenu.features
{
	internal class PlayerLogger
	{
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
		class OnJoin
		{
			static void Postfix(PlayerControl __instance)
			{
				if(__instance == null || __instance == PlayerControl.LocalPlayer || AmongUsClient.Instance == null || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;

				ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
				if(clientData == null) return;

				PlatformSpecificData platformData = clientData.PlatformData;
				if (platformData == null) return;

				Hydra.Log.LogMessage($"[PlayerLogger] {clientData.PlayerName} ({__instance.NetId}) joined on {platformData.Platform}. friendcode {clientData.FriendCode}, puid {clientData.ProductUserId}");

			}
		}
	}
}