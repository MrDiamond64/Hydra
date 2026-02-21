namespace HydraMenu.anticheat
{
	internal class InvalidStartCounter : ICheck
	{
		public static void OnSetStartCounter(PlayerControl player, sbyte counter, int seqId, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidStartCounter) return;

			// When a non-host player sends the SetStartCounter RPC, the counter value must always be -1
			// I'm not sure why non-host players even need to send this RPC, it's more something only the host should be sending
			if(player.OwnerId != AmongUsClient.Instance.HostId && counter != -1)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent a SetStartCounter RPC with an invalid value: {counter}.");
				Anticheat.Punish(player);
				blockRpc = true;

				// Revert the invalid start counter
				if(AmongUsClient.Instance.AmHost)
				{
					PlayerControl.LocalPlayer.RpcSetStartCounter(-1);
				}
			}

			/*
			if(PlayerControl.LocalPlayer != null && LobbyBehaviour.Instance == null)
			{
				// The vanilla game already ignores SetStartCounter RPCs when the game has started, so we do not need to revert it
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} sent a SetStartCounter RPC when the lobby has despawned.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
			*/
		}
	}
}