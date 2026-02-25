using Hazel;
using InnerNet;
using System.Linq;

namespace HydraMenu.anticheat
{
	internal class InvalidSystemUpdate : ICheck
	{
		// TODO: Maybe change the variable name to something shorter lol?
		private static readonly SystemTypes[] SystemsThatCanBeUpdatedWhenDead = { SystemTypes.MedBay, SystemTypes.Sabotage };

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

			if(player.Data.IsDead && !SystemsThatCanBeUpdatedWhenDead.Contains(system))
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried to update system {system} while dead.");
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
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} attempted to bulk-update switches: {switches}.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
			else if(switches > 5)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} attempted to toggle an invalid switch: {switches}.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}
	}
}