using AmongUs.GameOptions;
using HydraMenu.network;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace HydraMenu.modules.troll
{
	internal class Blackout : Module
	{
		public Blackout() : base("Blackout") { }

		public enum BlackoutMethods
		{
			DespawnPlayerPhysics
		}

		public BlackoutMethods method { get; set; } =  BlackoutMethods.DespawnPlayerPhysics;

		// In order for the intro cutscene to display, all players must have a role assigned to them
		// If we despawn our PlayerPhysics or CustomNetworkTransform, our PlayerControl will also be despawned as these objects are linked together
		// By despawning our PlayerPhysics as soon as the game starts for the host, the host will no longer assign a role for our player
		// and all other players in the lobby will be stuck on a black screen as we have not yet been assigned a role
		private void OnGameStart()
		{
			if(AmongUsClient.Instance.AmHost || method != BlackoutMethods.DespawnPlayerPhysics) return;
			Hydra.Log.LogMessage($"Using despawn PlayerPhysics blackout method");

			BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);
			batch.QueueDespawn(PlayerControl.LocalPlayer.MyPhysics);
			batch.QueueDespawn(PlayerControl.LocalPlayer.NetTransform);
			batch.FinishBatch();

			// Assign ourselves a role, tasks, and force the intro cutscene so we are not also stuck on a black screen
			PlayerControl.LocalPlayer.StartCoroutine(PlayerControl.LocalPlayer.CoSetRole(RoleTypes.Crewmate, false));
			PlayerControl.LocalPlayer.Data.SetTasks(new Il2CppStructArray<byte>(0));

			Enabled = false;
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameStart += OnGameStart;

			Hydra.notifications.Send("Blackout", "Blackout has been enabled. This will freeze all players on a black screen when the game starts.");
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
		}
	}
}