using HydraMenu.network;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.routines
{
	internal class InfiniteRoleCooldown : Routine
	{
		public InfiniteRoleCooldown() : base("InfiniteRoleCooldown") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		private readonly float CHECK_DELAY = 4.0f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < CHECK_DELAY) return;
			timeElapsed = 0f;

			PlayerControl protectTarget = GetProtectTarget();
			int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();

			// The handler for the ProtectPlayer RPC calls RoleBehavior::SetCooldown which starts the cooldown for a player's ability, even if they are not the guardian angel
			// By spamming the ProtectPlayer RPC for a player, we can make it so they can never use their role's ability
			BatchedMessage batch = new BatchedMessage();
			batch.skipLocalHandling = true;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player.Data == null || !targets.Contains(player.GetHashCode())) continue;

				if(batch.msgCount >= packingLimit)
				{
					batch.FinishBatch();
					batch = new BatchedMessage();
				}

				player.Data.Role.SetCooldown();
				batch.QueueProtectPlayer(player, protectTarget);
			}

			batch.FinishBatch();
		}

		private PlayerControl GetProtectTarget()
		{
			// Impostors and ghosts cannot be killed, so sending ProectPlayer for them should not impact the game
			// If there are no impostors or ghosts then default to our player
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player.Data == null || (!player.Data.IsDead && RoleManager.IsImpostorRole(player.Data.RoleType))) continue;

				return player;
			}

			return PlayerControl.LocalPlayer;
		}

		protected override void OnEnable()
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();
			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Infinite Role Cooldown", "You must be the host of the lobby in order to use this feature.");
				Enabled = false;
				return;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("Infinite Role Cooldown", "The game must have started in order for this feature to work.");
				Enabled = false;
				return;
			}
		}

		protected override void OnDisable()
		{
			targets.Clear();
		}
	}
}