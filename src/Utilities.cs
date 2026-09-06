using AmongUs.GameOptions;
using Hazel;
using LunarMenu.network;
using InnerNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LunarMenu.network.Constants;
using System.Text.RegularExpressions;

namespace LunarMenu
{
	internal class Utilities
	{
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SkinData> allSkins = HatManager.Instance.allSkins;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<HatData> allHats = HatManager.Instance.allHats;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<VisorData> allVisors = HatManager.Instance.allVisors;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PetData> allPets = HatManager.Instance.allPets;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NamePlateData> allNameplates = HatManager.Instance.allNamePlates;

		public static bool inLocalGame => ((AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) && (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started));
        public static bool inLocalLobby => ((AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) && (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined));
        public static bool inOnlineGame => ((AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame) && (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started));
        public static bool inOnlineLobby => ((AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame) && (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined));
		public static bool tutorialScene => GameState.CurrentScene == "TutorialScene";

		public static bool InLobby()
		{
			if (AmongUsClient.Instance == null) return false;
			if (!GameManager.Instance) return false;
			return (inLocalLobby || inOnlineLobby) && PlayerControl.LocalPlayer != null;
		}

		public static bool InGame()
		{
			if (AmongUsClient.Instance == null) return false;
			if (!GameManager.Instance) return false;
			return (inLocalGame || inOnlineGame || tutorialScene) && (ShipStatus.Instance != null || GameState.GameLoaded) && PlayerControl.LocalPlayer != null;
		}

		public static string GetHostUsername(bool colored = false)
		{
			if (InGame() || InLobby() && !colored)
				return AmongUsClient.Instance.GetHost().PlayerName;
			if (InGame() || InLobby())
			{
				Color color = GetPlayerColor(GetPlayerOutfit(AmongUsClient.Instance.GetHost().Character.Data).ColorId);
				string username = AmongUsClient.Instance.GetHost().PlayerName;
				return $"<#{ColorUtility.ToHtmlStringRGB(color)}>{RemoveHTMLTags(username)}</color>";
			}
			return "";
		}

		public static Color GetRoleColor(RoleBehaviour roleBehaviour)
		{
			if (roleBehaviour == null) return Palette.White;

			return roleBehaviour.TeamColor;
		}

		public static bool IsImpostor(NetworkedPlayerInfo player)
		{
			if (player.Role == null) return false;

			var role = player.RoleType;
			return role == RoleTypes.ImpostorGhost || RoleManager.IsImpostorRole(role);
		}

		public static PlayerControl GetPlayerControlById(byte id)
		{
			foreach (var player in PlayerControl.AllPlayerControls)
			{
				if (player.PlayerId == id) return player;
			}

			return null;
		}

		public static List<NormalPlayerTask> GetNormalPlayerTasks(PlayerControl player)
		{
			try
			{
				var playerTasks = player.myTasks;
				if (playerTasks == null) return [];

				List<NormalPlayerTask> normalPlayerTasks = [];

				foreach (var playerTask in playerTasks)
				{
					var normalTask = playerTask.TryCast<NormalPlayerTask>();
					if (normalTask != null) normalPlayerTasks.Add(normalTask);
				}

				return normalPlayerTasks;
			}
			catch
			{
				return [];
			}
		}

		public static string GetRoleName(RoleBehaviour roleBehaviour)
		{
			if (roleBehaviour == null) return "Unknown";

			const ushort trackerRoleId = 55050;

			if ((roleBehaviour.Role == RoleTypes.Tracker || roleBehaviour.Role == (RoleTypes)trackerRoleId) && roleBehaviour.StringName != (StringNames)1681)
				roleBehaviour.StringName = (StringNames)1681;

			return roleBehaviour.Role switch
			{
				RoleTypes.Crewmate => "Crewmate",
                RoleTypes.CrewmateGhost => "Crewmate Ghost",
                RoleTypes.Detective => "Detective",
                RoleTypes.Engineer => "Engineer",
                RoleTypes.GuardianAngel => "Guardian Angel",
                RoleTypes.Impostor => "Impostor",
                RoleTypes.ImpostorGhost => "Impostor Ghost",
                RoleTypes.Judge => "Judge",
                RoleTypes.Noisemaker => "Noisemaker",
                RoleTypes.Phantom => "Phantom",
                RoleTypes.Scientist => "Scientist",
                RoleTypes.Shapeshifter => "Shapeshifter",
                RoleTypes.Tracker => "Tracker",
				var r when r == (RoleTypes)trackerRoleId => "Tracker",
                RoleTypes.Viper => "Viper",
				_ => "Unknown"
            };
		}

        public static string GetPlatformString(Platforms platform)
        {
            return platform switch
            {
                Platforms.Android => "Android",
                Platforms.IPhone => "iOS",
                Platforms.Playstation => "PlayStation",
                Platforms.StandaloneEpicPC => "Epic Games",
                Platforms.StandaloneItch => "Itch.io",
                Platforms.StandaloneMac => "MacOS",
                Platforms.StandaloneSteamPC => "Steam",
                Platforms.StandaloneWin10 => "Microsoft Store",
                Platforms.Switch => "Nintendo Switch",
                Platforms.Xbox => "Xbox",
				_ => "Unknown",
            };
        }

        public static string RemoveHTMLTags(string html_str)
		{
			return Regex.Replace(html_str, "<[^>]*>", "");
		}

		public static Color GetPlayerColor(int colorId)
		{
			var colorArray = Palette.PlayerColors;
			if (colorId < 0 || colorId >= colorArray.Length)
			{
				return new Color32(38, 166, 98, 255);
			}
			return colorArray[colorId];
		}

		public static NetworkedPlayerInfo.PlayerOutfit GetPlayerOutfit(NetworkedPlayerInfo player, bool includeShapeshifted = false)
		{
			if (!player) return null;
			var dic = player.Outfits;
			if (includeShapeshifted && dic.ContainsKey(PlayerOutfitType.Shapeshifted))
			{
				var playerOutfit = dic[PlayerOutfitType.Shapeshifted];
				if (playerOutfit != null && !string.IsNullOrEmpty(player.PlayerName))
				{
					return playerOutfit;
				}
			}
			return dic[PlayerOutfitType.Default];
		}

        public static int GetRandomUnusedColor()
		{
			List<int> colors = Enumerable.Range(0, 18).ToList();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				colors.Remove(player.Data.DefaultOutfit.ColorId);
			}

			System.Random rnd = new System.Random();

			// Some modded lobbies may have more than 18 players, which means there will not be enough unique colors for everyone
			// so we should take that edge case into account
			return colors.Count != 0 ? colors[rnd.Next(0, colors.Count)] : rnd.Next(0, 18);
		}

		public static void RandomizePlayer(bool ingame = false)
		{
			System.Random rnd = new System.Random();

			if(ingame)
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)GetRandomUnusedColor());

				PlayerControl.LocalPlayer.RpcSetHat(allHats[rnd.Next(0, allHats.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetVisor(allVisors[rnd.Next(0, allVisors.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetSkin(allSkins[rnd.Next(0, allSkins.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetPet(allPets[rnd.Next(0, allPets.Length)].ProductId);
			}
			else
			{
				AccountManager.Instance.RandomizeName();

				PlayerCustomization.EquipHat(allHats[rnd.Next(0, allHats.Length)]);
				PlayerCustomization.EquipVisor(allVisors[rnd.Next(0, allVisors.Length)]);
				PlayerCustomization.EquipSkin(allSkins[rnd.Next(0, allSkins.Length)]);
				PlayerCustomization.EquipPet(allPets[rnd.Next(0, allPets.Length)]);
				PlayerCustomization.EquipNameplate(allNameplates[rnd.Next(0, allNameplates.Length)]);
			}
		}

		public static PlayerControl GetRandomPlayer(bool excludeHost = false, bool excludeDead = false, bool excludeImposters = false, bool excludeSelf = true)
		{
			Il2CppSystem.Collections.Generic.List<PlayerControl> allPlayers = PlayerControl.AllPlayerControls;
			List<PlayerControl> validPlayers = new List<PlayerControl>();

			foreach(PlayerControl player in allPlayers)
			{
				if(
					(excludeSelf && AmongUsClient.Instance.ClientId == player.OwnerId) ||
					(excludeHost && AmongUsClient.Instance.HostId == player.OwnerId) ||
					(excludeDead && player.Data.IsDead) ||
					(excludeImposters && player.Data.Role.CanUseKillButton)
				) continue;

				validPlayers.Add(player);
			}

			if(validPlayers.Count == 0) return null;

			System.Random rnd = new System.Random();
			return validPlayers[rnd.Next(validPlayers.Count)];
		}

		public static void CopyPlayer(PlayerControl player)
		{
			NetworkedPlayerInfo.PlayerOutfit outfit = player.CurrentOutfit;

			bool hasAnticheat = IsAnticheatPresent();

			BatchedMessage batch = new BatchedMessage();

			// We cannot change the name of our player in server-authoritative lobbies, even as the host
			if(!hasAnticheat)
			{
				batch.QueueSetName(PlayerControl.LocalPlayer, outfit.PlayerName);
			}

			if(!hasAnticheat || AmongUsClient.Instance.AmHost)
			{
				batch.QueueSetColor(PlayerControl.LocalPlayer, (byte)outfit.ColorId);
			}

			batch.QueueSetNameplateStr(PlayerControl.LocalPlayer, outfit.NamePlateId, ++outfit.NamePlateSequenceId);
			batch.QueueSetHatStr(PlayerControl.LocalPlayer, outfit.HatId, ++outfit.HatSequenceId);
			batch.QueueSetVisorStr(PlayerControl.LocalPlayer, outfit.VisorId, ++outfit.VisorSequenceId);
			batch.QueueSetSkinStr(PlayerControl.LocalPlayer, outfit.SkinId, ++outfit.SkinSequenceId);
			batch.QueueSetPetStr(PlayerControl.LocalPlayer, outfit.PetId, ++outfit.PetSequenceId);

			batch.FinishBatch();
		}

		public static void AttemptStartMeeting(PlayerControl reporter, NetworkedPlayerInfo target)
		{
			Lunar.Log.LogInfo($"Attempting to start a meeting for {reporter.Data.PlayerName}");

			bool hasAnticheat = IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Lunar.notifications.Send("Start Meeting", "The game must have started in order for this feature to work.");
				return;
			}

			if(AmongUsClient.Instance.AmHost)
			{
				Lunar.Log.LogInfo($"We are the host so we can directly use the StartMeeting RPC");

				if(ShipStatus.Instance == null)
				{
					Lunar.notifications.Send("Start Meeting", "There must be a valid instance of ShipStatus for this feature to work.");
				}
				else
				{
					OpenMeeting(reporter, target);
				}

				return;
			}

			Lunar.Log.LogInfo("We are not the host so we have to use the ReportDeadBody RPC");

			if(hasAnticheat && reporter != PlayerControl.LocalPlayer)
			{
				Lunar.notifications.Send("Start Meeting", "You must be the host of the lobby to make another player start a meeting.");
				return;
			}

			if(reporter.Data.IsDead)
			{
				Lunar.notifications.Send("Start Meeting", "You can only call meetings or report bodies if you are alive.");
				return;
			}

			if(hasAnticheat && target != null)
			{
				if(!target.IsDead)
				{
					Lunar.notifications.Send("Start Meeting", "You can only report bodies of players who have died in this round.");
					return;
				}

				if(!DoesDeadBodyExist(target.PlayerId))
				{
					Lunar.notifications.Send("Start Meeting", "Unable to find a dead body for this player, you can only report a player's body if they have died this round and their body has not dissolved.");
					return;
				}
			}

			reporter.CmdReportDeadBody(target);
		}

		public static void OpenMeeting(PlayerControl reporter, NetworkedPlayerInfo target)
		{
			MeetingRoomManager.Instance.AssignSelf(reporter, target);
			reporter.RpcStartMeeting(target);
			HudManager.Instance.OpenMeetingRoom(reporter);
		}

		public static bool DoesDeadBodyExist(byte playerId)
		{
			foreach(Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(0, 0), 99999f, Constants.PlayersOnlyMask))
			{
				if(collider.tag != "DeadBody") continue;

				DeadBody bodyComponent = collider.GetComponent<DeadBody>();
				if(bodyComponent && bodyComponent.ParentId == playerId)
				{
					return true;
				}
			}

			return false;
		}

		public static void ShapeshiftPlayer(PlayerControl victim, PlayerControl target, bool shouldAnimate = true)
		{
			bool hasAnticheat = IsAnticheatPresent();

			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Lunar.notifications.Send("Shapeshift Player", "You must be the host of the lobby in order to use this feature.");
				return;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Lunar.notifications.Send("Shapeshift Player", "The game must have started for this option to work.");
				return;
			}

			BatchedMessage batch = new BatchedMessage();

			// The vanilla anticheat will ban the host if they attempt to send the Shapeshift RPC for a player whose role is not Shapeshifter
			// To get around this, we temporarily change the player's role to Shapeshifter, make them shapeshift, and revert them back to their previous role
			if(hasAnticheat && victim.Data.RoleType != RoleTypes.Shapeshifter)
			{
				RoleTypes currentRole = victim.Data.RoleType;

				// The client that we're attempting to frame shouldn't notice anything as during role selection the SetRole RPC is sent with the canOverrideRole option set to false
				// meaning any future SetRole RPCs will be ignored unless the new role is a ghost role
				// Just in case this ever gets changed in the future, we could broadcast the SetRole RPC to a junk client ID instead of everyone to avoid the client knowing they became a Shapeshifter
				batch.QueueSetRole(victim, RoleTypes.Shapeshifter, true);
				batch.QueueShapeshift(victim, target, shouldAnimate);
				batch.QueueSetRole(victim, currentRole, true);
			}
			else
			{
				batch.QueueShapeshift(victim, target, shouldAnimate);
			}

			batch.FinishBatch();
		}

		public static MapNames GetCurrentMap()
		{
			// If Among Us has not fully loaded in yet then default to The Skeld
			if(AmongUsClient.Instance == null) return MapNames.Skeld;

			// Fall back to current map according to game options if ShipStatus does not exist
			if(ShipStatus.Instance == null)
			{
				if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
				{
					return (MapNames)AmongUsClient.Instance.TutorialMapId;
				}
				else
				{
					return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
				}
			}

			return (SpawnType)ShipStatus.Instance.SpawnId switch
			{
				SpawnType.SkeldShipStatus => MapNames.Skeld,
				SpawnType.DleksShipStatus => MapNames.Dleks,
				SpawnType.MiraShipStatus => MapNames.MiraHQ,
				SpawnType.PolusShipStatus => MapNames.Polus,
				SpawnType.AirshipShipStatus => MapNames.Airship,
				SpawnType.FungleShipStatus => MapNames.Fungle,
				_ => MapNames.Skeld
			};
		}

		public static bool IsAnticheatPresent()
		{
			if(Constants.IsVersionModded() || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return false;

			// On Freeplay, local, and modded lobbies, NetworkedPlayerInfo net objects are owned by the host (-2)
			// On vanilla lobbies, NetworkedPlayerInfo net objects are owned by the backend among us servers (-4)
			// If our NetworkedPlayerInfo net object is owned by the host, we can assume that the lobby has a lax anticheat without server authority
			// which does not require us to use any sort of bypasses
			return PlayerControl.LocalPlayer.Data.OwnerId != (int)OwnerIds.Host;
		}

		public static string GetPlayerColorString(NetworkedPlayerInfo player)
		{
			int colorId = player.DefaultOutfit.ColorId;

			if(colorId < 0 || colorId >= Palette.ColorNames.Length)
			{
				return "Fortegreen";
			}

			return player.GetPlayerColorString();
		}

		// This kick method allows a player who is not the host of the lobby to kick someone out of the lobby by making them trigger the Among Us Anticheat
		// There are various RPCs that can only be sent by the host of the lobby, such as MurderPlayer, Shapeshift, ProtectPlayer, etc
		// These RPCs are sent by the host in response to their client-authoritative equivalent, such as CheckMurder, CheckShapeshift, CheckProtect, etc
		// If we are able to make a player send a host-only RPC without being the host of the lobby, we can make the anticheat kick them out of the lobby
		// Most RPC handlers have checks to ensure that the client is the host of the lobby to avoid exactly this exploit
		// however one exception is UpdateSystem RPC for Ventilation System
		// Sending a ventilation system update with an operation of StartCleaning or BootFromVent will result in the host sending a BootFromVent RPC, which is one of these host-only RPCs
		// however nowhere in the callstack from ShipStatus::UpdateSystem to PlayerPhysics::RpcBootFromVent is there a check to ensure that the current client is the host of the lobby
		// If you were to send this system update to someone other than the host, they will send the BootFromVent RPC and get kicked by the Among Us anticheat
		// In my experience this has been incredibly useful to kick out players who are blatantly hacking, calling useless meetings, or causing other mischief even if I am not the host if the lobby
		public static void KickPlayer(PlayerControl player, bool skipFirstStage = false)
		{
			if(AmongUsClient.Instance.AmHost)
			{
				AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
				Lunar.notifications.Send("Kick Player", $"{player.Data.PlayerName} has been kicked from the game.", 5);
				return;
			}

			if(player.OwnerId == AmongUsClient.Instance.HostId)
			{
				Lunar.notifications.Send("Kick Player", "You are not able to kick out the host of the lobby.");
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Lunar.notifications.Send("Kick Player", "The game must have started in order for this feature to work.");
				return;
			}

			if(!IsAnticheatPresent())
			{
				Lunar.notifications.Send("Kick Player", "This feature only works in server-authoritative lobbies.");
				return;
			}

			BatchedMessage batch = new BatchedMessage(player.OwnerId);

			if(!skipFirstStage)
			{
				Lunar.Log.LogInfo($"Sending Enter ventilation system update to {player.OwnerId}");

				MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
				writer.Write((ushort)0);
				writer.Write((byte)VentilationSystem.Operation.Enter);
				writer.Write((byte)0);

				batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer);
				writer.Recycle();
			}

			Lunar.Log.LogInfo($"Sending BootImposters ventilation system update to {player.OwnerId}");

			MessageWriter writer2 = MessageWriter.Get(SendOption.Reliable);
			writer2.Write((ushort)1);
			writer2.Write((byte)VentilationSystem.Operation.BootImpostors);
			writer2.Write((byte)0);

			batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer2);
			writer2.Recycle();

			batch.FinishBatch();

			Lunar.notifications.Send("Kick Player", $"{player.Data.PlayerName} has been kicked from the game.", 5);
		}
	}
}