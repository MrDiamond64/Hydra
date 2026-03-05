using Hazel;

namespace HydraMenu.anticheat
{
	internal class InvalidSetLevel : ICheck
	{
		public static readonly uint MAX_PLAYER_LEVEL = 10000;

		// We should not block SetLevel RPCs
		public static void OnSetLevel(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckSpoofedLevels) return;

			uint level = reader.ReadPackedUInt32();

			// The vanilla Among Us anticheat bans players if they send a SetLevel RPC with a lever greater than 100k
			// This is rather generous, we just check if the requested player level is greater than 10k
			if(level > MAX_PLAYER_LEVEL)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC with a level that is too high ({level}).");
				blockRpc = true;

				player.SetLevel(MAX_PLAYER_LEVEL);
			}

			// The SetLevel RPC should only be sent when a player joins the game in the lobby
			if(ShipStatus.Instance)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC when the game has started.");
			}
		}
	}
}