using UnityEngine;

namespace HydraMenu.anticheat
{
	internal class InvalidSnapTo : ICheck
	{
		public static void OnSnapTo(PlayerControl player, Vector2 position, ushort minSid, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidStartCounter || !AmongUsClient.Instance.AmHost) return;

			if(LobbyBehaviour.Instance != null)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the SnapTo RPC while inside the lobby.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}
	}
}