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
				if(__instance == PlayerControl.LocalPlayer || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;

				ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
				if(clientData == null) return;

				PlatformSpecificData platformData = clientData.PlatformData;

				Hydra.Log.LogMessage($"[PlayerLogger] {clientData.PlayerName} ({__instance.NetId}) joined on {platformData.Platform}. Friendcode {clientData.FriendCode}, PUID {clientData.ProductUserId}");
			}
		}
	}
}