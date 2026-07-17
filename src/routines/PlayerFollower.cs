namespace HydraMenu.routines
{
	public class PlayerFollowerRoutine : IRoutine
	{
		public PlayerFollowerRoutine() : base("PlayerFollower") { }

		public PlayerControl following;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null) return;

			if(following == null)
			{
				Enabled = false;
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

		protected override void OnEnable()
		{
			PlayerControl.LocalPlayer.moveable = false;
		}

		protected override void OnDisable()
		{
			following = null;
			if(PlayerControl.LocalPlayer != null) PlayerControl.LocalPlayer.moveable = true;
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Player Follower", "Player Follower was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}