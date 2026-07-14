namespace HydraMenu.network
{
	internal class Constants
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
			VoteBanSysten = 12,
			FungleShipStatus = 13
		}
	}
}