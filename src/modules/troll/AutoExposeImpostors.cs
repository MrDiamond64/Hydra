using UnityEngine;

namespace HydraMenu.modules.troll
{
	internal class AutoExposeImpostors : Module
	{
		public AutoExposeImpostors() : base("AutoExposeImpostors") { }

		public float MIN_KILL_DISTANCE = 1.0f;
		public float MAX_DISTANCE = 5.0f;

		public bool ExposeOnMurder { get; set; } = true;
		public bool ExposeOnShapeshift { get; set; } = true;
		// Only triggers on vanish and not unvanish
		// The unvanish animation is much shorter so once everyone gets teleported they might not notice the cloud
		public bool ExposeOnPhantom { get; set; } = true;

		private void OnPlayerMurder(PlayerControl murderer, PlayerControl target, MurderResultFlags flags)
		{
			if(!ExposeOnMurder || ShipStatus.Instance == null || !flags.HasFlag(MurderResultFlags.Succeeded) || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(murderer, MIN_KILL_DISTANCE, MAX_DISTANCE);
			if(selectedVent == null)
			{
				Hydra.Log.LogMessage("Found no applicable vents to teleport player to");
				return;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == murderer || player == target || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Teleporter.TeleportToVent(player, selectedVent.Id);
			}
		}

		private void OnPlayerShapeshift(PlayerControl shapeshifter, PlayerControl target, bool shouldAnimate)
		{
			if(!ExposeOnShapeshift || ShipStatus.Instance == null || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(shapeshifter, MIN_KILL_DISTANCE, MAX_DISTANCE);
			if(selectedVent == null)
			{
				Hydra.Log.LogMessage("Found no applicable vents to teleport player to");
				return;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == shapeshifter || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Teleporter.TeleportToVent(player, selectedVent.Id);
			}
		}

		private void OnPlayerPhantom(PlayerControl phantom)
		{
			if(!ExposeOnPhantom || ShipStatus.Instance == null || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(phantom, 0.0f, MAX_DISTANCE);
			if(selectedVent == null)
			{
				Hydra.Log.LogMessage("Found no applicable vents to teleport player to");
				return;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == phantom || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Teleporter.TeleportToVent(player, selectedVent.Id);
			}
		}

		private Vent FindClosestVent(PlayerControl player, float minDistance, float maxDistance)
		{
			foreach(Vent vent in ShipStatus.Instance.AllVents) {
				float distance = Vector2.Distance(player.transform.position, vent.transform.position);
				Hydra.Log.LogMessage($"Vent ID {vent.Id} has a distance of {distance}");

				// If the kill is too far away from the vent, then the teleported players will not be able to see the kill
				// If the kill is too close, then players will not be able to determine who killed in the stack
				if(distance < minDistance || distance > maxDistance) continue;

				// We also want to make sure that there isn't an object that would block the teleported player's view to the kill
				// Not perfect, as a lot of objects allow you to see through them
				if(PhysicsHelpers.AnythingBetween(player.Collider, player.Collider.bounds.center, vent.transform.position, Constants.ShipOnlyMask, false)) continue;

				return vent;
			}

			return null;
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
			EventCoordinator.OnPlayerShapeshift += OnPlayerShapeshift;
			EventCoordinator.OnPlayerPhantom += OnPlayerPhantom;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
			EventCoordinator.OnPlayerShapeshift -= OnPlayerShapeshift;
			EventCoordinator.OnPlayerPhantom -= OnPlayerPhantom;
		}
	}
}