namespace HydraMenu.anticheat
{
	internal class InvalidSetLevel : ICheck
	{
		public static readonly uint MAX_PLAYER_LEVEL = 10000;
		// We should not block SetLevel RPCs
		public static void OnSetLevel(PlayerControl player, uint level, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckSpoofedLevels) return;

			// The vanilla Among Us anticheat bans players if they send a SetLevel RPC with a lever greater than 100k
			// This is rather generous, we just check if the requested player level is greater than 10k
			if(level > MAX_PLAYER_LEVEL)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent SetLevel RPC with a level that is too high ({level}).");
				Anticheat.Punish(player);
			}

			// The SetLevel RPC should only be sent when a player joins the game in the lobby
			if(ShipStatus.Instance)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent SetLevel RPC when the game has started.");
				Anticheat.Punish(player);
			}
		}
	}
}