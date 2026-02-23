using System.Collections.Generic;

namespace HydraMenu
{
	internal class Sabotage
	{
		/*
		When you sabotage as imposter, your game updates SystemTypes.Sabotage with the amount field being the system ID of the sabotage and sends it to the host
		Upon receiving the system update by the host, the host checks if the game is currently in a meeting or if sabotages are on cooldown, and blocks the sabotage if either of the conditiions are met
		If all checks go well, then the host updates the systems directly (so if you sabotaged Reactor, the host would update SystemTypes.Reactor) and broadcast the update to all online clients
		To get around the meeting and cooldown checks, we can just update the systems ourselves, which the host will relay to all online clients
		*/
		public static bool UpdateSystemsDirectly { get; set; } = true;

		public static Dictionary<string, SystemTypes> skeldSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "Reactor", SystemTypes.Reactor },
			{ "Oxygen", SystemTypes.LifeSupp },
			{ "Lights", SystemTypes.Electrical },
			{ "Communications", SystemTypes.Comms }
		};

		public static Dictionary<string, SystemTypes> skeldDoors = new Dictionary<string, SystemTypes>()
		{
			{ "Cafeteria", SystemTypes.Cafeteria },
			{ "Storage", SystemTypes.Storage },
			{ "Medbay", SystemTypes.MedBay },
			{ "Security", SystemTypes.Security },
			{ "Upper Engine", SystemTypes.UpperEngine },
			{ "Lower Engine", SystemTypes.LowerEngine },
			{ "Electrical", SystemTypes.Electrical }
		};

		public static Dictionary<string, SystemTypes> miraSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "Reactor", SystemTypes.Reactor },
			{ "Oxygen", SystemTypes.LifeSupp },
			{ "Lights", SystemTypes.Electrical },
			{ "Communications", SystemTypes.Comms }
		};

		public static Dictionary<string, SystemTypes> polusSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "Reactor", SystemTypes.Laboratory },
			{ "Lights", SystemTypes.Electrical },
			{ "Communications", SystemTypes.Comms }
		};

		public static Dictionary<string, SystemTypes> polusDoors = new Dictionary<string, SystemTypes>()
		{
			{ "Office", SystemTypes.Office },
			{ "Laboratory", SystemTypes.Laboratory },
			{ "Electrical", SystemTypes.Electrical },
			{ "Oxygen", SystemTypes.LifeSupp },
			{ "Communications", SystemTypes.Comms },
			{ "Weapons", SystemTypes.Weapons },
			{ "Storage", SystemTypes.Storage }
		};

		public static Dictionary<string, SystemTypes> airshipSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "Reactor", SystemTypes.HeliSabotage },
			{ "Lights", SystemTypes.Electrical },
			{ "Communications", SystemTypes.Comms }
		};

		public static Dictionary<string, SystemTypes> airshipDoors = new Dictionary<string, SystemTypes>()
		{
			{ "Brig", SystemTypes.Brig },
			{ "Records", SystemTypes.Records },
			{ "Communications", SystemTypes.Comms },
			{ "Main Hall", SystemTypes.MainHall },
			{ "Kitchen", SystemTypes.Kitchen },
			{ "Medical", SystemTypes.Medical }
		};

		public static Dictionary<string, SystemTypes> fungleSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "Reactor", SystemTypes.Reactor },
			{ "Communications", SystemTypes.Comms },
			{ "Mushroom Mixup", SystemTypes.MushroomMixupSabotage }
		};

		public static Dictionary<string, SystemTypes> GetSabotages()
		{
			MapNames map = Utilities.GetCurrentMap();
			switch(map)
			{
				case MapNames.Skeld:
				case MapNames.Dleks:
					return skeldSabotages;

				case MapNames.MiraHQ:
					return miraSabotages;

				case MapNames.Polus:
					return polusSabotages;

				case MapNames.Airship:
					return airshipSabotages;

				case MapNames.Fungle:
					return fungleSabotages;

				// If we don't have any sabotages for the current map then just default to the Skeld ones
				default:
					return skeldSabotages;
			}
		}

		public static Dictionary<string, SystemTypes> GetDoors()
		{
			MapNames map = Utilities.GetCurrentMap();
			switch(map)
			{
				case MapNames.Skeld:
				case MapNames.Dleks:
					return skeldDoors;

				// Mira has no closable doors
				case MapNames.MiraHQ:
					return [];

				case MapNames.Polus:
					return polusDoors;

				case MapNames.Airship:
					return airshipDoors;

				// If we don't have any doors for the current map then just default to the Skeld ones
				default:
					return skeldDoors;
			}
		}

		// I thought that maybe we could check if ShipStatus::Systems included an entry for the doors system type
		// however it turns out that Skeld has the doors system type even when it doesn't have unlockable doors
		public static bool CanUnlockDoors()
		{
			MapNames map = Utilities.GetCurrentMap();
			return AmongUsClient.Instance.AmHost || map == MapNames.Polus || map == MapNames.Airship || map == MapNames.Fungle;
		}

		public static void SabotageSystem(SystemTypes system)
		{
			if(!UpdateSystemsDirectly)
			{
				ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)system);
				return;
			}

			switch(system)
			{
				case SystemTypes.Reactor:
				case SystemTypes.Laboratory:
				case SystemTypes.HeliSabotage:
				case SystemTypes.LifeSupp:
				case SystemTypes.Comms:
					ShipStatus.Instance.RpcUpdateSystem(system, 128);
					break;

				// Eletrical sabotage requires us to update each individual light switch
				// The following code comes from SabotageSystemType::UpdateSystem
				case SystemTypes.Electrical:
					byte amount = 4;

					for(byte i = 0; i < 5; i++)
					{
						if(BoolRange.Next(0.5f))
						{
							amount |= (byte)(1 << i);
						}
					}

					ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)(amount | 128));
					break;

				case SystemTypes.MushroomMixupSabotage:
					ShipStatus.Instance.RpcUpdateSystem(system, 1);
					break;
			}
		}

		public static void FixSabotage(SystemTypes system)
		{
			switch(system)
			{
				// ShipStatus::RepairCriticalSabotages uses amount value of 16 to insta fix sabotages
				// This amount value should only be send by the host, so this can be detected by anticheats
				case SystemTypes.Reactor:
				case SystemTypes.Laboratory:
				case SystemTypes.LifeSupp:
				case SystemTypes.Comms:
					ShipStatus.Instance.RpcUpdateSystem(system, 16);
					break;

				case SystemTypes.Electrical:
					SwitchSystem switches = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();

					// Hydra.Log.LogMessage($"Actual: {switches.ActualSwitches}, expected: {switches.ExpectedSwitches}");
					int amount = switches.ActualSwitches ^ switches.ExpectedSwitches;

					if(amount == 0)
					{
						Hydra.Log.LogInfo($"Attempted to fix lights, XOR operation is 0 so that means we have nothing to fix.");
						break;
					}

					// If the 8th bit is off, then the amount value is the index of the light switch (so 0, 1, 2, 3, or 4, and potentially 5 or 6 if those were to ever get added) that should get toggled
					// If it is on, then the amount value is a binary representation of what switches should be toggled
					// So if we had an amount value of 172 (which in binary is 1000 1101), that would mean light switches 0, 2, and 3 would be toggled
					// I don't think the 8th bit is actually ever used outside of SabotageSystemType, so anticheats can detect this
					ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)(amount | 128));
					break;

				case SystemTypes.MushroomMixupSabotage:
					if(!AmongUsClient.Instance.AmHost)
					{
						Hydra.Log.LogInfo("Attempted to fix Mushroom Mixup, we are not the host so nothing can be done");
						break;
					}

					Hydra.Log.LogInfo("Attempted to fix Mushroom Mixup, we are the host so it can be fixed");

					MushroomMixupSabotageSystem mixupSystem = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage].Cast<MushroomMixupSabotageSystem>();
					mixupSystem.currentSecondsUntilHeal = 0.1f;
					mixupSystem.IsDirty = true;
					break;
			}
		}

		public static void LockDoor(SystemTypes door)
		{
			ShipStatus.Instance.RpcCloseDoorsOfType(door);
		}

		public static void UnlockDoor(byte id)
		{
			if(AmongUsClient.Instance.AmHost)
			{
				MapNames currentMap = Utilities.GetCurrentMap();

				// On Skeld, all doors have an id of 0, so unfourtunately getting a door by its ID by using ShipStatus.Instance.AllDoors[id] wont work
				for(byte i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
				{
					OpenableDoor door = ShipStatus.Instance.AllDoors[i];
					if(door.Id != id) continue;
					door.SetDoorway(true);

					if(currentMap == MapNames.Skeld)
					{
						AutoDoorsSystemType doorSystem = ShipStatus.Instance.Systems[SystemTypes.Doors].Cast<AutoDoorsSystemType>();
						doorSystem.dirtyBits |= 1U << i;
					}
					else
					{
						DoorsSystemType doorSystem = ShipStatus.Instance.Systems[SystemTypes.Doors].Cast<DoorsSystemType>();
						doorSystem.IsDirty = true;
					}
				}
				return;
			}

			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(id | 64));
		}

		public static void SabotageAll()
		{
			Dictionary<string, SystemTypes> sabotages = GetSabotages();
			foreach(var (_, value) in sabotages)
			{
				SabotageSystem(value);
			}
		}

		public static void FixAllSabotages()
		{
			Dictionary<string, SystemTypes> sabotages = GetSabotages();
			foreach(var (_, value) in sabotages)
			{
				FixSabotage(value);
			}
		}

		public static void LockAll()
		{
			Dictionary<string, SystemTypes> doors = GetDoors();
			foreach(var (_, value) in doors)
			{
				LockDoor(value);
			}
		}

		public static void UnlockAll()
		{
			// 'AllDoors' also includes entries for Polus' decontamination doors, funnily enough
			foreach(OpenableDoor door in ShipStatus.Instance.AllDoors)
			{
				UnlockDoor((byte)door.Id);
			}
		}
	}
}