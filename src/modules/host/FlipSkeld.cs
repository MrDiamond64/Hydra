using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.network;
using HydraMenu.ui.sections;
using InnerNet;
using UnityEngine.AddressableAssets;

namespace HydraMenu.modules.host
{
	internal class FlipSkeld : Module
	{
		public FlipSkeld() : base("FlipSkeld") { }

		private void SwapMapAssets()
		{
			if(AmongUsClient.Instance == null)
			{
				Enabled = false;
				return;
			}

			// ShipPrefabs is a list corresponding map IDs to their map
			// ID 0 is Skeld, 1 is Mira, 2 is Polus, and 3 is Dleks
			// If we want to be able to spawn in Dleks (as this is normally inaccessible) we can swap the two elements
			// so that 0 is Dleks and 3 is Skeld, spawning in Dleks instead of Skeld
			AssetReference temp = AmongUsClient.Instance.ShipPrefabs[3];
			AmongUsClient.Instance.ShipPrefabs[3] = AmongUsClient.Instance.ShipPrefabs[0];
			AmongUsClient.Instance.ShipPrefabs[0] = temp;
		}

		// When the host spawns in an instance of The Skeld, despawn it and spawn in an instance of dlekS ehT
		// This only works on Local lobbies; +25 modded protocol lobbies blocks non-hosts from despawning or spawning in net objects
		// and Impostor custom servers block non-hosts from despawning net objects, even if the anticheat is disabled (https://github.com/Impostor/Impostor/blob/master/src/Impostor.Server/Net/State/Game.Data.cs#L218)
		// (meaning this will not work on Skeld.net either)
		// AmongUsClient::CoStartGameHost does not spawn in an instance of ShipStatus if one already exists, so it is theoretically possible to spawn in an instance of dlekS ehT as soon as the host sends a StartGame message
		// but the timing tolerance is so incredibly thin that we will not beat the host in spawning in an instance of ShipStatus
		// Another implementation I thought of is waiting for the host to send a SetStartCounter with a counter of two, and then spawning in an instance of dlekS ehT, but there's no guarantee that the game will actually start
		// and it will not work if the host uses a force start feature, such as the one in Hydra
		private void OnNetObjectSpawn(InnerNetObject netObject)
		{
			if(AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame || AmongUsClient.Instance.AmHost || netObject.SpawnId != (int)SpawnType.SkeldShipStatus) return;
			Hydra.Log.LogMessage($"Despawning ship and spawning in a new one");

			BatchedMessage batch = new BatchedMessage();
			batch.QueueDespawn(netObject);
			batch.FinishBatch();

			AmongUsClient.Instance.StartCoroutine(HostSection.SpawnMap(0).WrapToIl2Cpp());
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnNetObjectSpawn += OnNetObjectSpawn;

			SwapMapAssets();
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnNetObjectSpawn -= OnNetObjectSpawn;

			SwapMapAssets();
		}
	}
}