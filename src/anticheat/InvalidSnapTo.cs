using Hazel;
using UnityEngine;

namespace HydraMenu.anticheat
{
	internal class InvalidSnapTo : ICheck
	{
		public static void OnSnapTo(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidSnapTo || !AmongUsClient.Instance.AmHost) return;

			Vector2 position = NetHelpers.ReadVector2(reader);
			// ushort seqId = reader.ReadUInt16();

			if(LobbyBehaviour.Instance != null)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent the SnapTo RPC while inside the lobby.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}
	}
}