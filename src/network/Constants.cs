using System;

namespace HydraMenu.network
{
	public enum OwnerIds
	{
		// Technically not a real net object owner ID, only used by InnerNetClient::StartRpcImmediately to know if a GameData or a GameDataTo root message should be used
		Everyone = -1,
		Host = -2,
		// Also not a real net object owner ID, only used in AmongUsClient::Spawn to autopopulate the current client's ID
		Self = -3,
		Server = -4
	}

	public enum SpawnType
	{
		SkeldShipStatus = 0,
		MeetingHud = 1,
		LobbyBehavior = 2,
		PlayerControl = 4,
		MiraShipStatus = 5,
		PolusShipStatus = 6,
		DleksShipStatus = 7,
		AirshipShipStatus = 8,
		GameManager = 10,
		NetworkedPlayerInfo = 11,
		VoteBanSystem = 12,
		FungleShipStatus = 13
	}

	[Flags]
	public enum HudOverrideSystemOperation
	{
		// All other bits are treated as Fix
		Sabotage = 128
	}

	[Flags]
	public enum LifeSuppSystemOperation
	{
		Fix = 16,
		RepairConsole = 64,
		Sabotage = 128
	}

	[Flags]
	public enum ReactorSystemOperation
	{
		Fix = 16,
		UnrepairConsole = 32,
		RepairConsole = 64,
		Sabotage = 128
	}
}