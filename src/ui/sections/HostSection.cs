using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.features;
using InnerNet;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HydraMenu.ui.sections
{
	internal class HostSection : ISection
	{
		public HostSection() : base("Host") { }

		private byte selectedMap = 0;

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			}
			else if(!AmongUsClient.Instance.AmHost)
			{
				GUILayout.Label("You are not the host of the current lobby. Using these options will either do nothing or get you banned by the anticheat");
			}

			Host.BanMidGame.Enabled = GUILayout.Toggle(Host.BanMidGame.Enabled, "Be able to ban players mid-game");

			Host.FlippedSkeld = GUILayout.Toggle(Host.FlippedSkeld, "Use Flipped Skeld Map");

			Host.DisableSabotages.Enabled = GUILayout.Toggle(Host.DisableSabotages.Enabled, "Disable Sabotages");
			Host.DisableCloseDoors.Enabled = GUILayout.Toggle(Host.DisableCloseDoors.Enabled, "Disable Close Doors");
			Host.DisableCameras.Enabled = GUILayout.Toggle(Host.DisableCameras.Enabled, "Disable Security Cameras");
			Host.DisableGameEnd.Enabled = GUILayout.Toggle(Host.DisableGameEnd.Enabled, "Disable Game End");
			Host.NoKillCooldown.Enabled = GUILayout.Toggle(Host.NoKillCooldown.Enabled, "No Kill Cooldown");

			GUILayout.BeginHorizontal();
			Host.BlockLowLevels.Enabled = GUILayout.Toggle(Host.BlockLowLevels.Enabled, $"Kick players with less than {Host.BlockLowLevels.MinLevel} levels");
			Host.BlockLowLevels.MinLevel = (uint)GUILayout.HorizontalSlider(Host.BlockLowLevels.MinLevel, 0, 100);
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Force Start Game"))
			{
				AmongUsClient.Instance.StartGame();
			}

			if(GUILayout.Button("Kill Everyone"))
			{
				KillAllPlayers();
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Force Crewmate Victory"))
			{
				// Just incase the user has this enabled
				Host.DisableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
				Hydra.notifications.Send("Game Finished", "You ended the game with a crewmate victory.", 5);
			}

			if(GUILayout.Button("Force Imposter Victory"))
			{
				// Just incase the user has this enabled
				Host.DisableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
				Hydra.notifications.Send("Game Finished", "You ended the game with an imposter victory.", 5);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("Map Spawner:");

			GUILayout.Label($"Selected map: {(MapNames)selectedMap}");
			selectedMap = (byte)GUILayout.HorizontalSlider(selectedMap, 0, 5);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Despawn Map"))
			{
				if(ShipStatus.Instance != null)
				{
					ShipStatus.Instance.Despawn();
					Hydra.notifications.Send("Game Map", "The current map has been despawned.", 5);
				}
				else
				{
					Hydra.notifications.Send("Game Map", "The game map has already been despawned.", 5);
				}
			}

			if(GUILayout.Button("Spawn Map"))
			{
				AmongUsClient.Instance.StartCoroutine(SpawnMap(selectedMap).WrapToIl2Cpp());
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Despawn Lobby"))
			{
				if(LobbyBehaviour.Instance != null)
				{
					LobbyBehaviour.Instance.Despawn();
					Hydra.notifications.Send("Lobby Map", "The lobby map has been despawned.", 5);
				}
				else
				{
					Hydra.notifications.Send("Lobby Map", "The lobby map has already been despawned.", 5);
				}
			}

			if(GUILayout.Button("Spawn Lobby"))
			{
				LobbyBehaviour.Instance = UnityEngine.Object.Instantiate<LobbyBehaviour>(GameStartManager.Instance.LobbyPrefab);
				AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, SpawnFlags.None);

				Hydra.notifications.Send("Lobby Map", "A new instance of the lobby map has been spawned", 5);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("Assign roles for next round:");
			Host.AlwaysImposter.Enabled = GUILayout.Toggle(Host.AlwaysImposter.Enabled, "Enabled");
			GUILayout.Label($"Role to assign: {Host.AlwaysImposter.assignedRole}");
			Host.AlwaysImposter.assignedRole = Controls.HorizontalRoleSlider(Host.AlwaysImposter.assignedRole);

			GUILayout.Space(5);
			GUILayout.Label("Meeting Controls:");
			Host.DisableMeetings.Enabled = GUILayout.Toggle(Host.DisableMeetings.Enabled, "Disable Meetings");
			Hydra.routines.reportBodySpam.Enabled = GUILayout.Toggle(Hydra.routines.reportBodySpam.Enabled, "Spam Report Bodies");

			if(GUILayout.Button("Close Meeting"))
			{
				if(MeetingHud.Instance == null)
				{
					Hydra.notifications.Send("Skip Meeting", "This option can only be used in a meeting.");
				}
				else
				{
					MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();

					Network.BatchedMessage batch = new Network.BatchedMessage();
					batch.QueueVotingComplete(votes, null, false);
					batch.QueueCloseMeeting();
					batch.FinishBatch();
				}
			}

			GUILayout.Space(5);
			GUILayout.Label("Shapeshift Controls:");
			if(GUILayout.Button("Shapeshift Everyone Into Me"))
			{
				AmongUsClient.Instance.StartCoroutine(ShapeshiftAll(PlayerControl.LocalPlayer).WrapToIl2Cpp());
			}

			if(GUILayout.Button("Shapeshift Everyone Into Random"))
			{
				PlayerControl target = Utilities.GetRandomPlayer(false, false, false, false);
				AmongUsClient.Instance.StartCoroutine(ShapeshiftAll(target).WrapToIl2Cpp());
			}

			if(GUILayout.Button("Revert All Shapeshifts"))
			{
				AmongUsClient.Instance.StartCoroutine(RevertAllShapeshift().WrapToIl2Cpp());
			}

			GUILayout.Space(5);
			GUILayout.Label("Disco Party:");
			Hydra.routines.discoHost.Enabled = Controls.GlobalPlayerSpecificToggle("Enabled", ref Hydra.routines.discoHost.targets);

			GUILayout.Label($"Color randomization delay: {Hydra.routines.discoHost.randomizationDelay:F2}s");
			Hydra.routines.discoHost.randomizationDelay = GUILayout.HorizontalSlider(Hydra.routines.discoHost.randomizationDelay, 0.1f, 2.0f);
		}

		private static void KillAllPlayers()
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("Murder Player", "This feature can only be used once the game has started.");
				return;
			}

			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Murder Player", "This feature can only be used if you are the host of the lobby.");
				return;
			}

			Network.BatchedMessage batch = new Network.BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				batch.QueueMurderPlayer(PlayerControl.LocalPlayer, player, MurderResultFlags.Succeeded);
			}

			batch.FinishBatch();
		}

		private static IEnumerator SpawnMap(byte mapId)
		{
			Hydra.Log.LogInfo($"Attempting to spawn in map id {mapId}");

			// The Utilities::IsAnticheatPresent function does not work perfectly here
			// We are able to spawn in any maps we want without host in local, or even skeld.net lobbies
			// however +25 modded protocol lobbies, while having much of their anticheat checks disabled, still has checks against non-hosts sending spawn messages
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Map Spawner", "This feature can only be used if you are the host of the lobby.");
				yield break;
			}

			AsyncOperationHandle<GameObject> asyncHandle = AmongUsClient.Instance.ShipPrefabs[mapId].InstantiateAsync(null, false);
			yield return asyncHandle;

			ShipStatus ship = asyncHandle.Result.GetComponent<ShipStatus>();

			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.QueueSpawn(ship, -2, SpawnFlags.None);
			batch.FinishBatch();

			Hydra.notifications.Send("Map Spawner", $"{(MapNames)mapId} has been spawned.", 5);
		}

		private static IEnumerator ShapeshiftAll(PlayerControl target)
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Shapeshift Player", "You need to be the host of the lobby in order to use this feature.");
				yield break;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == target || player.shapeshiftTargetPlayerId == target.PlayerId) continue;

				Utilities.ShapeshiftPlayer(player, target);

				// This function can send up to 42 reliable messages at once, so we need to implement a delay to avoid getting disconnected
				yield return Effects.Wait(0.05f);
			}
		}

		private static IEnumerator RevertAllShapeshift()
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Shapeshift Player", "You need to be the host of the lobby in order to use this feature.");
				yield break;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player.shapeshiftTargetPlayerId == -1) continue;

				Utilities.ShapeshiftPlayer(player, player);

				yield return Effects.Wait(0.05f);
			}
		}
	}
}