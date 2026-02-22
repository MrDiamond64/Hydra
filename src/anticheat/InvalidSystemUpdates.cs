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

			switch(system)
			{
				case SystemTypes.Electrical:
					ValidateSwitchSystem(player, reader, ref blockRpc);
					break;
			}
		}

		public static void ValidateSwitchSystem(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			byte switches = reader.ReadByte();

			if(switches.HasBit(128))
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} attempted to update switch system with 8th bit active ({switches}).");
				Anticheat.Punish(player);
				blockRpc = true;
			}
			else if(switches > 5)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} attempted to toggle an invalid switch: {switches}.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			if(player.Data.IsDead)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} attempted toggle light switches while dead.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}
	}
}