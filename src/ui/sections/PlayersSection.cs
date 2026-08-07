using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.features;
using HydraMenu.network;
using InnerNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class PlayersSection : ISection
	{
		public PlayersSection() : base("Players") { }

		public static Vector2 PlayerPaneSize
		{
			get { return new Vector2(100 * MainUI.scale, MainUI.WindowSize.y - MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerPanePosition
		{
			get { return new Vector2(MainUI.SectionListPosition.x + MainUI.SectionListSize.x, MainUI.HeaderSize.y + MainUI.HeaderPosition.y); }
		}

		public static Vector2 PlayerButtonSize
		{
			get { return new Vector2(PlayerPaneSize.x, 30 * MainUI.scale); }
		}

		public static Vector2 PlayerOptionsSize
		{
			get { return new Vector2(MainUI.WindowSize.x - MainUI.SectionListSize.x - PlayerPaneSize.x, MainUI.WindowSize.y - MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerOptionsPosition
		{
			get { return new Vector2(PlayerPanePosition.x + PlayerPaneSize.x, MainUI.HeaderPosition.y + MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerColorBoxSize
		{
			get { return new Vector2(5 * MainUI.scale, PlayerButtonSize.y); }
		}

		public static PlayerControl selectedPlayer;
		private Vector2 subsectionScrollVector;

		private Controls.PlayerColors selectedColor = Controls.PlayerColors.Red;
		private int selectedVent = 0;

		private static Dictionary<int, int> _voteCounts = new Dictionary<int, int>();
		private static Dictionary<int, float> _voteTimers = new Dictionary<int, float>();

		public override void HandleSubsectionMove(int offset)
		{
			if(PlayerControl.AllPlayerControls.Count == 0) return;

			int currentPlayer = PlayerControl.AllPlayerControls.IndexOf(selectedPlayer);
			int newPosition = Math.Clamp(currentPlayer + offset, 0, PlayerControl.AllPlayerControls.Count - 1);

			selectedPlayer = PlayerControl.AllPlayerControls[newPosition];
		}

		public override void Render()
		{
			if(PlayerControl.AllPlayerControls.Count == 0)
			{
				GUILayout.Label("There are currently no online players.");
				return;
			}

			GUI.Box(new Rect(0, 0, PlayerPaneSize.x, PlayerPaneSize.y), "", Styles.MainBox);

			for(byte i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
			{
				PlayerControl player = PlayerControl.AllPlayerControls[i];
				// Wait for player data to fully load
				if(player.Data == null) continue;

				RenderPlayerSelection(i, player);

				if(player == selectedPlayer)
				{
					GUILayout.BeginArea(new Rect(PlayerPaneSize.x, 0, PlayerOptionsSize.x, PlayerOptionsSize.y));
					subsectionScrollVector = GUILayout.BeginScrollView(subsectionScrollVector);

					RenderPlayerControls(player);

					GUILayout.EndScrollView();
					GUILayout.EndArea();
				}
			}
		}

		private void RenderPlayerSelection(byte position, PlayerControl player)
		{
			Rect playerInfo = new Rect(0, position * PlayerButtonSize.y, PlayerButtonSize.x, PlayerButtonSize.y);

			string playerName = player.Data.PlayerName;
			playerName += $"\n<color=\"{GetRoleColor(player.Data.RoleType)}\">{player.Data.RoleType}</color>";

			GUIStyle style = player == selectedPlayer ? Styles.PlayerBoxActive : Styles.PlayerBox;

			if(player.OwnerId == AmongUsClient.Instance.HostId)
			{
				style.normal.textColor = new Color(1.0f, 0.84f, 0.0f); // #FFD700
			}

			if(GUI.Button(playerInfo, playerName, style))
			{
				selectedPlayer = player;
			}

			Rect playerColor = new Rect(0, position * PlayerButtonSize.y, PlayerColorBoxSize.x, PlayerColorBoxSize.y);
			Controls.DrawCrewmateColorBox(playerColor, player.Data);
		}

		private string GetRoleColor(RoleTypes role)
		{
			return RoleManager.IsImpostorRole(role) ? "red" : "#8afcfc";
		}

		public static void VotekickTarget(PlayerControl target)
		{
			if (target == null || target.Data == null)
			{
				Hydra.notifications.Send("Votekick", "No player selected.");
				return;
			}
			if (VoteBanSystem.Instance == null)
			{
				Hydra.notifications.Send("Votekick", "VoteBanSystem isn't available.");
				return;
			}
			int targetClientId = target.OwnerId;
			if (targetClientId == AmongUsClient.Instance.ClientId)
			{
				Hydra.notifications.Send("Votekick", "You cannot votekick yourself.");
				return;
			}
			VoteBanSystem.Instance.CmdAddVote(targetClientId);
			if (!_voteCounts.ContainsKey(targetClientId))
			{
				_voteCounts[targetClientId] = 0;
			}
			_voteCounts[targetClientId]++;
			_voteTimers[targetClientId] = Time.time + 10f;
			int currentVotes = _voteCounts[targetClientId];
			Hydra.notifications.Send("Votekick", $"Vote sent to {target.Data.PlayerName} ({currentVotes}/{3} votes)");
			if (currentVotes >= 3)
			{
				Hydra.notifications.Send("Votekick", "Player " + target.Data.PlayerName + " has been kicked!");
				_voteCounts.Remove(targetClientId);
				_voteTimers.Remove(targetClientId);
			}
			Hydra.Log.LogInfo($"[Votekick] Voted to kick {target.Data.PlayerName} - Votes: {currentVotes}/{3}");
		}

		private void RenderPlayerControls(PlayerControl target)
		{
			if(target == null || target.Data == null)
			{
				GUILayout.Label("Specified target is not valid.");
				return;
			}

			bool hasAnticheat = Utilities.IsAnticheatPresent();

			// If we want to get a player's name, we have to use NetworkedPlayerInfo::PlayerName instead of PlayerControl::name to avoid
			// getting the incorrect name if the player is shapeshifted to another player
			string playerInfo =
				$"Name: {target.Data.PlayerName} ({Utilities.GetPlayerColor(target.Data)})" +
				$"\nRole: {target.Data.RoleType}" +
				$"\nState: " + (target.Data.IsDead ? "Dead" : "Alive");

			ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(target);
			if(clientData != null)
			{
				PlatformSpecificData platform = clientData.PlatformData;

				bool streamerMode = DataManager.Settings.Gameplay.StreamerMode;

				playerInfo +=
					$"\nFriendcode: " + (streamerMode ? "REDACTED" : target.Data.FriendCode) +
					$"\nPUID: " + (streamerMode ? "REDACTED" : target.Data.Puid) +
					$"\nLevel: {target.Data.PlayerLevel + 1}" +
					$"\nDevice: {platform.Platform}" +
					(target.OwnerId == AmongUsClient.Instance.HostId ? "\nHost: true" : "");
			}

			GUILayout.Label(playerInfo);

			Visuals.SpectatePlayer.Enabled = Controls.PlayerSpecificToggle("Spectate", target, ref Visuals.SpectatePlayer.target);
			Hydra.routines.petPlayer.Enabled = Controls.PlayerSpecificToggle("Pet Player", target, ref Hydra.routines.petPlayer.target);
			Hydra.routines.playerFollower.Enabled = Controls.PlayerSpecificToggle("Follow", target, ref Hydra.routines.playerFollower.following);
			Hydra.routines.jailPlayer.Enabled = Controls.PlayerSpecificToggle("Place in Jail", target, ref Hydra.routines.jailPlayer.targets);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Teleport"))
			{
				// We do not want to use PlayerControl::GetTruePosition() here as it would teleport us to the player's feet
				Teleporter.TeleportTo(target.transform.position);
			}

			if(!hasAnticheat && GUILayout.Button("Teleport to Me"))
			{
				Teleporter.TeleportPlayerTo(target, PlayerControl.LocalPlayer.transform.position);
			}
			GUILayout.EndHorizontal();

			if(!hasAnticheat && GUILayout.Button("Teleport All To"))
			{
				Teleporter.TeleportAllTo(target.transform.position);
			}

			if(GUILayout.Button("Murder"))
			{
				AttemptMurder(target);
			}

			if(GUILayout.Button("Copy Avatar"))
			{
				Utilities.CopyPlayer(target);
			}

			if(GUILayout.Button("Report Body"))
			{
				Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, target.Data);
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Kick Player"))
			{
				Utilities.KickPlayer(target);
			}

			if(GUILayout.Button("Votekick"))
			{
				VotekickTarget(target);
			}
			GUILayout.EndHorizontal();

			GUILayout.Label($"Teleport player to vent: {selectedVent}");
			selectedVent = (int)GUILayout.HorizontalSlider(selectedVent, 0, ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 10);
			if(GUILayout.Button("Teleport"))
			{
				Teleporter.TeleportToVent(target, selectedVent);
			}

			GUILayout.Space(5);
			GUILayout.Label("Host Only Features:" + (AmongUsClient.Instance.AmHost ? "" : "\n(Using these will get you kicked!)"));

			Troll.AutoReportBodies.Enabled = Controls.PlayerSpecificToggle("Auto Report Bodies As", target, ref Troll.AutoReportBodies.source);
			Hydra.routines.discoHost.Enabled = Controls.PlayerSpecificToggle("Disco Mode", target, ref Hydra.routines.discoHost.targets);

			if(GUILayout.Button("Force Meeting As"))
			{
				Utilities.AttemptStartMeeting(target, null);
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Force All Votes To"))
			{
				if(MeetingHud.Instance == null)
				{
					Hydra.notifications.Send("Vote Forcer", "This option can only be used when there is an active meeting.");
				}
				else
				{
					MeetingHud.VoterState[] array = new MeetingHud.VoterState[PlayerControl.AllPlayerControls.Count];

					for(int i = 0; i < array.Length; i++)
					{
						MeetingHud.VoterState state = array[i];

						state.VoterId = (byte)i;
						state.VotedForId = target.PlayerId;

						array[i] = state;
					}

					BatchedMessage batch = new BatchedMessage();
					batch.QueueVotingComplete(array, target.Data, false);
					batch.FinishBatch();
				}
			}

			if(GUILayout.Button("Eject"))
			{
				BatchedMessage batch = new BatchedMessage();

				if(MeetingHud.Instance == null)
				{
					MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(HudManager.Instance.MeetingPrefab);
					batch.QueueSpawn(MeetingHud.Instance, -2, SpawnFlags.None);
				}

				MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();

				batch.QueueVotingComplete(votes, target.Data, false);
				// If we created a MeetingHud object then it will be destroyed by the RpcClose function
				batch.QueueCloseMeeting();
				batch.FinishBatch();
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Frame Shapeshift"))
			{
				PlayerControl randomPl = Utilities.GetRandomPlayer(false, false, false, false);
				Utilities.ShapeshiftPlayer(target, randomPl);
			}

			if(GUILayout.Button("Frame for Killing All"))
			{
				target.StartCoroutine(AttemptFrameForKillingAll(target).WrapToIl2Cpp());
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Flood Player with Tasks"))
			{
				byte[] taskIds = new byte[255];

				for(byte i = 0; i < 255; i++)
				{
					taskIds[i] = i;
				}

				target.Data.RpcSetTasks(taskIds);
			}

			if(GUILayout.Button("Clear Tasks"))
			{
				target.Data.RpcSetTasks(Array.Empty<byte>());
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("Game Options Modifier:");

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Blind"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.CrewLightMod, -1.0f);
				gameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, -1.0f);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			if(GUILayout.Button("Fullbright"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.CrewLightMod, 1000f);
				gameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, 1000f);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Slow Speed"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 0.1f);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			if(GUILayout.Button("Super Speed"))
			{
				// The vanilla anticheat prevents us from being able to exceed speeds greater than 3.0f
				float maxSpeed = Utilities.IsAnticheatPresent() ? 3.0f : 5.0f;

				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, maxSpeed);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();

			/*
			// The problem with changing the TaskBarMode is that if we remove the task bar, we are not able to bring it back
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Hide Task Bar"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			if(GUILayout.Button("Show Task Bar"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Normal);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();
			*/

			if(GUILayout.Button("Reset to Defaults"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			GUILayout.Space(5);
			GUILayout.Label($"Change color to: {selectedColor}");
			selectedColor = Controls.HorizontalColorSlider(selectedColor);

			if(GUILayout.Button("Set Color"))
			{
				target.RpcSetColor((byte)selectedColor);
			}
		}

		private static void AttemptMurder(PlayerControl target)
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("Murder Player", $"You can only kill players once the game has started.");
				return;
			}

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo($"Attempting to murder {target.Data.PlayerName}, we are the host so we can use the MurderPlayer RPC");
				PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
				Hydra.notifications.Send("Murder Player", $"Killed {target.Data.PlayerName}.", 5);
				return;
			}

			if(!hasAnticheat)
			{
				Hydra.Log.LogInfo($"Attempting to murder {target.Data.PlayerName}, we are are in a host-authoritative lobby so we can use the MurderPlayer RPC");
				PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
				Hydra.notifications.Send("Murder Player", $"Killed {target.Data.PlayerName}.", 5);
				return;
			}

			Hydra.Log.LogInfo($"Attempting to kill {target.Data.PlayerName}, we are not the host so we have to use the CheckMurder RPC");

			// The CheckMurder RPC handler will not authorize kills if you are not the imposter or you are inside a meeting
			// There are more checks, but I do not think it is worth adding them all here
			if(!RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType))
			{
				Hydra.notifications.Send("Murder Player", "You can only murder players when you are an Impostor, unless you are the host of the lobby.");
				return;
			}

			if(MeetingHud.Instance != null)
			{
				Hydra.notifications.Send("Murder Player", "You can only murder players outside of meetings, unless you are the host of the lobby.");
				return;
			}

			Hydra.notifications.Send("Murder Player", $"Attempted to kill {target.Data.PlayerName}.", 5);
			PlayerControl.LocalPlayer.CmdCheckMurder(target);
		}

		private static IEnumerator AttemptFrameForKillingAll(PlayerControl target)
		{
			Hydra.Log.LogInfo($"Attempting to frame {target.Data.PlayerName} for killing all players...");

			bool hasAnticheat = Utilities.IsAnticheatPresent();
			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Framer", "You must be the host of the lobby in order to use this option.");
				yield break;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("Framer", "The game must have started for this option for this option to work.");
				yield break;
			}

			Host.DisableGameEnd.Enabled = true;

			if(target != PlayerControl.LocalPlayer)
			{
				// On official servers, we are not able to send MurderPlayer RPCs with other player net IDs
				// so we need to shapeshift into our desired player and kill everyone ourselves
				Utilities.ShapeshiftPlayer(PlayerControl.LocalPlayer, target, false);
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == target) continue;

				PlayerControl.LocalPlayer.RpcMurderPlayer(player, true);
			}

			// Wait three seconds so all players can see which player we are framing
			yield return Effects.Wait(3.0f);

			Host.DisableGameEnd.Enabled = false;
			Hydra.notifications.Send("Framer", $"Framed {target.Data.PlayerName} for killing all players!");
		}
	}
}
