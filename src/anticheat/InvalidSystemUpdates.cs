using Hazel;
using InnerNet;
using System.Collections.Generic;
using System.Linq;

namespace HydraMenu.anticheat
{
	internal class InvalidSystemUpdates : ICheck
	{
		// TODO: Maybe change the variable name to something shorter lol?
		private static readonly SystemTypes[] SystemsThatCanBeUpdatedWhenDead = {
			SystemTypes.MedBay,
			SystemTypes.Sabotage,
			// Ghosts update the Security system when closing cameras, but not opening them
			SystemTypes.Security
		};

		public static void OnSystemUpdate(MessageReader reader, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidSystemUpdates) return;

			SystemTypes system = (SystemTypes)reader.ReadByte();
			PlayerControl player = reader.ReadNetObject<PlayerControl>();

			if(!ShipStatus.Instance.Systems.ContainsKey(system))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to update system {system} when the current map has no such system.");
				blockRpc = true;
				return;
			}

			if(player.Data.IsDead && !SystemsThatCanBeUpdatedWhenDead.Contains(system))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to update system {system} while dead.");
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

			Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to update Mushroom Mixup system with operation {operation}.");
			blockRpc = true;
		}

		private static void ValidateSabotageSystem(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			SystemTypes system = (SystemTypes)reader.ReadByte();

			Dictionary<string, SystemTypes> validSabotages = Sabotage.GetSabotages();
			if(!validSabotages.ContainsValue(system))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage an invalid system: {system}.");
				blockRpc = true;
			}

			if(!RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} when they are not an imposter.");
				blockRpc = true;
			}

			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} while in Hide and Seek.");
				blockRpc = true;
			}
		}

		private static void ValidateSwitchSystem(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			byte switches = reader.ReadByte();

			if(switches.HasBit(128))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to bulk-update switches: {switches}.");
				blockRpc = true;
			}
			else if(switches > 5)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to toggle an invalid switch: {switches}.");
				blockRpc = true;
			}

			// Block light switch updates when lights are currently not sabotaged
			// It is possible for this check to false flag if a player is attempting to fix lights when they have not received the message about the sabotage being fixed
			// This is also why you may experience the bug where lights get unfixed right after they get fixed
			// So to avoid wrongly banning players, we just silent flag and block the RPC to prevent hackers from being able to force sabotage lights
			SwitchSystem system = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
			if(system.ExpectedSwitches == system.ActualSwitches)
			{
				Hydra.Log.LogInfo($"Blocked switch update from {player.Data.PlayerName} as lights are not currently sabotaged");
				blockRpc = true;
			}
		}
	}
}