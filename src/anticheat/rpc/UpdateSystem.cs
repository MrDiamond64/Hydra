using Hazel;
using Il2CppInterop.Runtime;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HydraMenu.anticheat.rpc
{
	internal class UpdateSystem : RpcCheck
	{
		// TODO: Maybe change the variable name to something shorter lol?
		private static readonly SystemTypes[] SystemsThatCanBeUpdatedWhenDead = {
			SystemTypes.MedBay,
			SystemTypes.Sabotage,
			// Ghosts update the Security system when closing cameras, but not opening them
			SystemTypes.Security,
			SystemTypes.Ventilation
		};

		private static readonly Dictionary<Il2CppSystem.Type, Func<PlayerControl, MessageReader, bool>> systemHandlers = new Dictionary<Il2CppSystem.Type, Func<PlayerControl, MessageReader, bool>>()
		{
			{ Il2CppType.From(typeof(SwitchSystem)), ValidateSwitchSystem },
			{ Il2CppType.From(typeof(MushroomMixupSabotageSystem)), ValidateMushroomMixupSystem },
			{ Il2CppType.From(typeof(ReactorSystemType)), ValidateReactorSystem},
			{ Il2CppType.From(typeof(SabotageSystemType)), ValidateSabotageSystem }
		};

		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			SystemTypes system = (SystemTypes)reader.ReadByte();
			player = reader.ReadNetObject<PlayerControl>();

			ShipStatus.Instance.Systems.TryGetValue(system, out ISystemType systemInterface);
			if(systemInterface == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to update system {system} when the current map has no such system.");
				return false;
			}

			if(player.Data.IsDead && !SystemsThatCanBeUpdatedWhenDead.Contains(system))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to update system {system} while dead.");
				return false;
			}

			// thanks https://discord.com/channels/623153565053222947/754333645199900723/1292612730418757785
			Il2CppSystem.Type systemType = Il2CppType.TypeFromPointer(systemInterface.ObjectClass);

			systemHandlers.TryGetValue(systemType, out Func<PlayerControl, MessageReader, bool> handler);
			if(handler == null) return true;

			return handler(player, reader);
		}

		// The Mushroom Mixup system is only updated in the SabotageSystemType::Update function by the host. It should never be sent by a player
		private static bool ValidateMushroomMixupSystem(PlayerControl player, MessageReader reader)
		{
			MushroomMixupSabotageSystem.Operation operation = (MushroomMixupSabotageSystem.Operation)reader.ReadByte();

			Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to update Mushroom Mixup system with operation {operation}.");
			return false;
		}

		private static bool ValidateReactorSystem(PlayerControl player, MessageReader reader)
		{
			byte operation = reader.ReadByte();

			switch(operation)
			{
				case 16:
					Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to forcefully fix the Reactor sabotage");
					return false;

				case 128:
					Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force call the Reactor sabotage");
					return false;

				default:
					return true;
			}
		}

		private static bool ValidateSabotageSystem(PlayerControl player, MessageReader reader)
		{
			SystemTypes system = (SystemTypes)reader.ReadByte();

			Dictionary<string, SystemTypes> validSabotages = Sabotage.GetSabotages();
			if(!validSabotages.ContainsValue(system))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage an invalid system: {system}.");
				return false;
			}

			if(!RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} when they are not an imposter.");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} while in Hide and Seek.");
				return false;
			}

			return true;
		}

		private static bool ValidateSwitchSystem(PlayerControl player, MessageReader reader)
		{
			byte switches = reader.ReadByte();

			if(switches.HasBit(128))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to bulk-update switches: {switches}.");
				return false;
			}

			if(switches > 5)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to toggle an invalid switch: {switches}.");
				return false;
			}

			// Block light switch updates when lights are currently not sabotaged
			// It is possible for this check to false flag if a player is attempting to fix lights when they have not received the message about the sabotage being fixed
			// This is also why you may experience the bug where lights get unfixed right after they get fixed
			// So to avoid wrongly banning players, we just silent flag and block the RPC to prevent hackers from being able to force sabotage lights
			SwitchSystem system = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
			if(system.ExpectedSwitches == system.ActualSwitches)
			{
				Hydra.Log.LogInfo($"Blocked switch update from {player.Data.PlayerName} as lights are not currently sabotaged");
				return false;
			}

			// False positives may be possible if a player is toggling light switches before their client receives the StartMeeting RPC so we silent flag
			// Maybe we can check to see what state the meeting is in, and if it is after the meeting was animated then flag the player?
			if(MeetingHud.Instance)
			{
				Hydra.Log.LogInfo($"Blocked switch update from {player.Data.PlayerName} as there is a currently active meeting");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.UpdateSystem;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(ShipStatus);
		}
	}
}