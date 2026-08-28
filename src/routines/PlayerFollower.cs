using HydraMenu.modules;
using InnerNet;
using UnityEngine;

namespace HydraMenu.routines
{
	public class PlayerFollowerRoutine : Routine
	{
		public PlayerFollowerRoutine() : base("PlayerFollower") { }

		public PlayerControl target;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null) return;

			/*
			float distance = Vector3.Distance(target.transform.position, PlayerControl.LocalPlayer.transform.position);
			if(distance > 2)
			{
				Hydra.Log.LogInfo($"We drifted too far away from the player we are following, teleporting back to course. Distance: {distance}");
				Teleporter.TeleportTo(target.transform.position);
			}
			*/

			// We could probably see how haunting as a ghost makes the follower walks towards a player's position so we don't have to directly teleport, but this works fine for now
			PlayerControl.LocalPlayer.transform.position = target.transform.position;
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("Player Follower", "Player Follower was disabled as you left the game.", 10);
			Enabled = false;
		}

		private void OnPlayerDisconnect(ClientData client, DisconnectReasons reason)
		{
			if(client.Character != target) return;

			Hydra.notifications.Send("Follow Player", "Follow Player was disabled as the player you were following left the game.");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				_enabled = false;
				return;
			}

			PlayerControl.LocalPlayer.moveable = false;
			PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;

			EventCoordinator.OnDisconnect += OnDisconnect;
			EventCoordinator.OnPlayerDisconnect += OnPlayerDisconnect;
		}

		protected override void OnDisable()
		{
			target = null;

			if(PlayerControl.LocalPlayer != null)
			{
				PlayerControl.LocalPlayer.moveable = true;
			}

			EventCoordinator.OnDisconnect -= OnDisconnect;
			EventCoordinator.OnPlayerDisconnect -= OnPlayerDisconnect;
		}
	}
}