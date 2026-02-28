using Hazel;
using InnerNet;
using System.Collections.Generic;
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

				case SystemTypes.MushroomMixupSabotage:
					ValidateMushroomMixupSystem(player, reader, ref blockRpc);
					break;

				case SystemTypes.Sabotage:
					ValidateSabotageSystem(player, reader, ref blockRpc);
					break;
			}
		}

		// The Mushroom Mixup system is only be updated in the SabotageSystemType::Update function by the host. It should never be sent by a player
		private static void ValidateMushroomMixupSystem(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			MushroomMixupSabotageSystem.Operation operation = (MushroomMixupSabotageSystem.Operation)reader.ReadByte();

			Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} attempted to update Mushroom Mixup system with operation {operation}.");
			Anticheat.Punish(player);
			blockRpc = true;
		}

		private static void ValidateSabotageSystem(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			SystemTypes system = (SystemTypes)reader.ReadByte();

			Dictionary<string, SystemTypes> validSabotages = Sabotage.GetSabotages();
			if(!validSabotages.ContainsValue(system))
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} attempted to sabotage an invalid system: {system}.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			if(!RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} attempted to sabotage {system} when they are not an imposter.");
				Anticheat.Punish(player);
				blockRpc = true;
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