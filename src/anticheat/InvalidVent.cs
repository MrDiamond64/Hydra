namespace HydraMenu.anticheat
{
	internal class InvalidVent
	{
		public static void OnPlayerEnterVent(PlayerControl player, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidVent) return;

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried to vent when there is no instance of ShipStatus.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			// Check if the player vents if their role does not support venting (if they are not engineer or non-ghost imposter)
			// We also want to make sure that the player is not dead to avoid false positives if the player vents as soon as they die
			// (Maybe we can store the time at which a player died and skip this check if an EnterVent RPC was sent within 500ms of dying?)
			if(!player.Data.IsDead && !player.Data.Role.CanVent)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried to vent when their role ({player.Data.RoleType}) does not support venting.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}

		// Sending ExitVent RPCs can be used to make the player teleport to areas without having to send SnapTo RPCs
		public static void OnPlayerExitVent(PlayerControl player, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidVent) return;

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried to exit a vent when there is no instance of ShipStatus.");
				Anticheat.Punish(player);
				blockRpc = true;
			}

			if(!player.Data.IsDead && !player.Data.Role.CanVent)
			{
				Hydra.notifications.Send("Anticheat", $"{player.Data.PlayerName} tried to exit a vent when their role ({player.Data.RoleType}) does not support venting.");
				Anticheat.Punish(player);
				blockRpc = true;
			}
		}
	}
}