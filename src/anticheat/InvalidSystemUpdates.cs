using Hazel;
using InnerNet;

namespace HydraMenu.anticheat
{
	internal class InvalidSystemUpdate : ICheck
	{
		public static void OnSystemUpdate(MessageReader reader, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidSystemUpdates) return;

			SystemTypes system = (SystemTypes)reader.ReadByte();
			PlayerControl player = reader.ReadNetObject<PlayerControl>();

			if(!ShipStatus.Instance.Systems.ContainsKey(system))
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried to update system {system} when the current map has no such system.");
				Anticheat.Punish(player);
				blockRpc = true;
				return;
			}
		}
	}
}