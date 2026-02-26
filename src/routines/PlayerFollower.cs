using HydraMenu.ui.sections;

namespace HydraMenu.routines
{
	public class PlayerFollowerRoutine : IRoutine
	{
		public PlayerFollowerRoutine()
		{
			this.routineName = "PlayerFollower";
		}

		public bool _enabled = false;
		private PlayerControl following;

		// This routine can only be enabled for a specific player, so we want the Players UI to only show the routine as being enabled when selecting a certain player
		public override bool Enabled
		{
			get
			{
				// If the selected player in the Players UI is the person we are attached to then return the routine as being enabled
				return AmAttachedTo(PlayersSection.selectedPlayer);
			}
			set
			{
				// If we are attempting to enable Follow Player, and the routine isn't already enabled OR we are attempting to follow another player, set the target
				if(value && (_enabled != value || !AmAttachedTo(PlayersSection.selectedPlayer)))
				{
					following = PlayersSection.selectedPlayer;
					_enabled = true;
					PlayerControl.LocalPlayer.moveable = false;

					Hydra.notifications.Send("Player Follower", $"You are now attached to {following.Data.PlayerName}", 5);
					// If we are attempting to disable Follow Player, and the Player UI shows the player we are currently following, then disable the routine
				}
				else if(!value && AmAttachedTo(PlayersSection.selectedPlayer))
				{
					Disable();
				}
			}
		}

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Disable();
				Hydra.notifications.Send("Player Follower", "Player Follower was disabled as you left the game.");
				return;
			}

			if(following == null)
			{
				Disable();
				Hydra.notifications.Send("Player Follower", "Player Follower was disabled as the person you attached to left the game.");
				return;
			}

			/*
			float distance = Vector3.Distance(following.transform.position, PlayerControl.LocalPlayer.transform.position);
			if(distance > 2)
			{
				Hydra.Log.LogInfo($"We drifted too far away from the player we are following, teleporting back to course. Distance: {distance}");
				Teleporter.TeleportTo(following.transform.position);
			}
			*/

			// We could probably see how haunting as a ghost makes the follower walks towards a player's position so we don't have to directly teleport, but this works fine for now
			PlayerControl.LocalPlayer.transform.position = following.transform.position;
		}

		public bool AmAttachedTo(PlayerControl player)
		{
			return following != null && player.PlayerId == following.PlayerId;
		}

		private void Disable()
		{
			_enabled = false;
			following = null;
			if(PlayerControl.LocalPlayer) PlayerControl.LocalPlayer.moveable = true;
		}
	}
}