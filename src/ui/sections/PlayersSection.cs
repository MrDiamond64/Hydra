using AmongUs.Data;
using AmongUs.GameOptions;
using InnerNet;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class PlayersSection : ISection
	{
		public PlayersSection()
		{
			name = "Players";
		}

		public static Vector2 PlayerPaneSize
		{
			get { return new Vector2(100, MainUI.windowSize.y - MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerPanePosition
		{
			get { return new Vector2(MainUI.SectionListPosition.x + MainUI.SectionListSize.x, MainUI.HeaderSize.y + MainUI.HeaderPosition.y); }
		}

		public static Vector2 PlayerButtonSize
		{
			get { return new Vector2(PlayerPaneSize.x, 30); }
		}

		public static Vector2 PlayerOptionsSize
		{
			get { return new Vector2(MainUI.windowSize.x - MainUI.SectionListSize.x - PlayerPaneSize.x, MainUI.windowSize.y - MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerOptionsPosition
		{
			get { return new Vector2(PlayerPanePosition.x + PlayerPaneSize.x, MainUI.HeaderPosition.y + MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerColorBoxSize
		{
			get { return new Vector2(5, PlayerButtonSize.y); }
		}

		public static PlayerControl selectedPlayer;
		private Vector2 subsectionScrollVector;

		private static CrewmateColor selectedColor = CrewmateColor.Red;

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
			GUI.Box(playerColor, "", Styles.CreateCrewmateColorBox(player.Data.ColorName, player.Data.Color));
		}

		private string GetRoleColor(RoleTypes role)
		{
			return RoleManager.IsImpostorRole(role) ? "red" : "#8afcfc";
		}

		private static void RenderPlayerControls(PlayerControl target)
		{
			if(target == null)
			{
				GUILayout.Label("Specified target is not valid.");
				return;
			}

			ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(target);
			if(clientData != null)
			{
				PlatformSpecificData platform = clientData.PlatformData;

				bool streamerMode = DataManager.Settings.Gameplay.StreamerMode;

				GUILayout.Label(
					// If we want to get a player's name, we have to use NetworkedPlayerInfo::PlayerName instead of PlayerControl::name to avoid
					// getting the incorrect name if the player is currently shapeshifted to another player
					$"Name: {target.Data.PlayerName} {target.Data.ColorName}" +
					$"\nRole: {target.Data.RoleType}" +
					$"\nState: " + (target.Data.IsDead ? "Dead" : "Alive") +
					$"\nFriendcode: " + (streamerMode ? "REDACTED" : target.Data.FriendCode) +
					$"\nPUID: " + (streamerMode ? "REDACTED" : target.Data.Puid) +
					$"\nLevel: {target.Data.PlayerLevel + 1}" +
					$"\nDevice: {platform.Platform}" +
					(target.OwnerId == AmongUsClient.Instance.HostId ? "\nHost: true" : "")
				);
			} else
			{
				GUILayout.Label(
					$"Name: {target.Data.PlayerName} {target.Data.ColorName}" +
					$"\nRole: {target.Data.RoleType}" +
					$"\nState: " + (target.Data.IsDead ? "Dead" : "Alive") +
					$"\nIs Dummy: true"
				);
			}

			Hydra.routines.playerFollower.Enabled = GUILayout.Toggle(Hydra.routines.playerFollower.Enabled, "Follow");

			if(GUILayout.Button("Teleport"))
			{
				// We do not want to use PlayerControl::GetTruePosition() here as it would teleport us to the player's feet
				Teleporter.TeleportTo(target.transform.position);
			}

			if(GUILayout.Button("Murder"))
			{
				if(AmongUsClient.Instance.AmHost)
				{
					Hydra.Log.LogInfo($"Attempting to kill {target.Data.PlayerName}, we are host so we are using the MurderPlayer RPC");
					PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
				}
				else
				{
					Hydra.Log.LogInfo($"Attempting to kill {target.Data.PlayerName}, we are not the host so we have to use the CheckMurder RPC");
					PlayerControl.LocalPlayer.CmdCheckMurder(target);
				}
			}

			if(GUILayout.Button("Copy Avatar"))
			{
				Utilities.CopyPlayer(target);
			}

			// Can result in bans if you report someone's body after the round they were killed in
			if(GUILayout.Button("Report Body") && (AmongUsClient.Instance.AmHost || target.Data.IsDead))
			{
				if(AmongUsClient.Instance.AmHost)
				{
					Hydra.Log.LogInfo($"Attempting to report {target.Data.PlayerName}'s body, we are the host so we directly use the StartMeeting RPC");
					Utilities.OpenMeeting(PlayerControl.LocalPlayer, target.Data);
				}
				else
				{
					Hydra.Log.LogInfo($"Attempting to report {target.Data.PlayerName}'s body, we are not the host so we have to use the ReportDeadBody RPC");
					PlayerControl.LocalPlayer.CmdReportDeadBody(target.Data);
				}
			}

			GUILayout.Space(5);
			GUILayout.Label("Host Only Features:" + (AmongUsClient.Instance.AmHost ? "" : "\n(Using these will get you banned!)"));

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Force Meeting As"))
			{
				Utilities.OpenMeeting(target, null);
			}

			if(GUILayout.Button("Force All Votes To"))
			{
				if(MeetingHud.Instance == null)
				{
					Hydra.notifications.Send("Vote Forcer", "This option can only be used when there is an active meeting.");
				}
				else
				{
					foreach(PlayerControl player in PlayerControl.AllPlayerControls)
					{
						PlayerVoteArea votingArea = MeetingHud.Instance.playerStates[player.PlayerId];

						votingArea.SetVote(target.PlayerId);
					}

					MeetingHud.Instance.SetDirtyBit(1);
					MeetingHud.Instance.CheckForEndVoting();
				}
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Frame Shapeshift"))
			{
				AttemptShapeshiftFrame(target);
			}

			/*
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Frame Vanish"))
			{
				if(target.Data.RoleType != RoleTypes.Phantom) target.RpcSetRole(RoleTypes.Phantom, true);
				target.CheckVanish();
			}

			if(GUILayout.Button("Frame Appear"))
			{
				if(target.Data.RoleType != RoleTypes.Phantom) target.RpcSetRole(RoleTypes.Phantom, true);
				target.CheckAppear(true);
			}

			GUILayout.EndHorizontal();

			if(GUILayout.Button("Start Scan"))
			{
				target.RpcSetScanner(true);
			}
			*/

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Flood Player with Tasks"))
			{
				byte[] taskIds = new byte[255];

				for(byte i = 0; i < 255; i++)
				{
					// Is it maybe possible to get a random task from the entire task pool for the map instead of a random task whose ID ranges from 0 to 20?
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
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				// We can't exceed 3.0x speed on Vanilla servers without getting banned
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 3.0f);

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
			selectedColor = (CrewmateColor)GUILayout.HorizontalSlider((float)selectedColor, 0, 17);

			if(GUILayout.Button("Set Color"))
			{
				target.RpcSetColor((byte)selectedColor);
			}
		}

		private static async void AttemptShapeshiftFrame(PlayerControl target)
		{
			bool hasAnticheat = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && !Constants.IsVersionModded();

			if(ShipStatus.Instance == null && hasAnticheat)
			{
				Hydra.notifications.Send("Framer", "The game must have started for this option to work.");
				return;
			}

			// Setting our role to Shapeshifter and then setting it back causes our game to crash for some odd reason
			if(PlayerControl.LocalPlayer == target && hasAnticheat)
			{
				Hydra.notifications.Send("Framer", "This option would've resulted in your game crashing.");
				return;
			}

			PlayerControl randomPl = Utilities.GetRandomPlayer(false, false, false, false);

			// The vanilla anticheat will ban the host if they attempt to send the Shapeshift RPC for a player whose role is not Shapeshifter
			// To get around this, we temporarily change the player's role to Shapeshifter, make them shapeshift, and revert them back to their previous role
			if(target.Data.RoleType != RoleTypes.Shapeshifter && hasAnticheat)
			{
				RoleTypes currentRole = target.Data.RoleType;

				// The client that we're attempting to frame shouldn't notice anything as during role selection the SetRole RPC is sent with the canOverrideRole option set to false
				// meaning any future SetRole RPCs will be ignored (unless the new role is a ghost role)
				// Just in case this ever gets changed in the future, we could broadcast the SetRole RPC to a junk client ID instead of everyone to avoid the client knowing they became a Shapeshifter
				target.RpcSetRole(RoleTypes.Shapeshifter, true);
				// Wait 500ms to make sure the server received the role update request
				await Task.Delay(500);
				target.RpcShapeshift(randomPl, true);
				target.RpcSetRole(currentRole, true);
			}
			else
			{
				target.RpcShapeshift(randomPl, true);
			}
		}
	}
}