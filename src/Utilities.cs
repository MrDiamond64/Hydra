using AmongUs.GameOptions;
using System.Collections.Generic;

namespace HydraMenu
{
	internal class Utilities
	{
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SkinData> allSkins = HatManager.Instance.allSkins;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<HatData> allHats = HatManager.Instance.allHats;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<VisorData> allVisors = HatManager.Instance.allVisors;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PetData> allPets = HatManager.Instance.allPets;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NamePlateData> allNameplates = HatManager.Instance.allNamePlates;

		public static void RandomizePlayer(bool ingame = false)
		{
			System.Random rnd = new System.Random();

			if(ingame)
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)rnd.Next(0, 17));

				PlayerControl.LocalPlayer.RpcSetHat(allHats[rnd.Next(0, allHats.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetVisor(allVisors[rnd.Next(0, allVisors.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetSkin(allSkins[rnd.Next(0, allSkins.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetPet(allPets[rnd.Next(0, allPets.Length)].ProductId);
			}
			else
			{
				PlayerCustomization.EquipSkin(allSkins[rnd.Next(0, allSkins.Length)]);
				PlayerCustomization.EquipHat(allHats[rnd.Next(0, allHats.Length)]);
				PlayerCustomization.EquipVisor(allVisors[rnd.Next(0, allVisors.Length)]);
				PlayerCustomization.EquipPet(allPets[rnd.Next(0, allPets.Length)]);
				PlayerCustomization.EquipNameplate(allNameplates[rnd.Next(0, allNameplates.Length)]);

				AccountManager.Instance.RandomizeName();
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

			// We cannot change the name of our player in server-authoritative lobbies, even as the host
			if(!hasAnticheat)
			{
				PlayerControl.LocalPlayer.RpcSetName(outfit.PlayerName);
			}

			if(!hasAnticheat || AmongUsClient.Instance.AmHost)
			{
				PlayerControl.LocalPlayer.RpcSetColor((byte)outfit.ColorId);
			}

			PlayerControl.LocalPlayer.RpcSetNamePlate(outfit.NamePlateId);
			PlayerControl.LocalPlayer.RpcSetHat(outfit.HatId);
			PlayerControl.LocalPlayer.RpcSetVisor(outfit.VisorId);
			PlayerControl.LocalPlayer.RpcSetSkin(outfit.SkinId);
			PlayerControl.LocalPlayer.RpcSetPet(outfit.PetId);
		}

		public static void OpenMeeting(PlayerControl reporter, NetworkedPlayerInfo target)
		{
			MeetingRoomManager.Instance.AssignSelf(reporter, target);
			reporter.RpcStartMeeting(target);
			HudManager.Instance.OpenMeetingRoom(reporter);
		}

		public static void ShapeshiftPlayer(PlayerControl victim, PlayerControl target)
		{
			bool hasAnticheat = IsAnticheatPresent();

			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Shapeshift Player", "You must be the host of the lobby in order to use this feature.");
				return;
			}

			if(hasAnticheat && ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Shapeshift Player", "The game must have started for this option to work.");
				return;
			}

			Network.BatchedMessage batch = new Network.BatchedMessage();

			// The vanilla anticheat will ban the host if they attempt to send the Shapeshift RPC for a player whose role is not Shapeshifter
			// To get around this, we temporarily change the player's role to Shapeshifter, make them shapeshift, and revert them back to their previous role
			if(hasAnticheat && victim.Data.RoleType != RoleTypes.Shapeshifter)
			{
				RoleTypes currentRole = victim.Data.RoleType;

				// The client that we're attempting to frame shouldn't notice anything as during role selection the SetRole RPC is sent with the canOverrideRole option set to false
				// meaning any future SetRole RPCs will be ignored unless the new role is a ghost role
				// Just in case this ever gets changed in the future, we could broadcast the SetRole RPC to a junk client ID instead of everyone to avoid the client knowing they became a Shapeshifter
				batch.QueueSetRole(victim, RoleTypes.Shapeshifter, true);
				batch.QueueShapeshift(victim, target, true);
				batch.QueueSetRole(victim, currentRole, true);
			}
			else
			{
				batch.QueueShapeshift(victim, target, true);
			}

			batch.FinishBatch();
		}

		public static MapNames GetCurrentMap()
		{
			if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
			{
				return (MapNames)AmongUsClient.Instance.TutorialMapId;
			} else {
				return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
			}
		}

		public static bool IsAnticheatPresent()
		{
			if(Constants.IsVersionModded() || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return false;

			// On freeplay, local, and modded lobbies, NetworkedPlayerInfo net objects are owned by the host (-2)
			// On vanilla lobbies, NetworkedPlayerInfo net objects are owned by the backend among us servers (-4)
			// If our NetworkedPlayerInfo net object is owned by the host, we can assume that the lobby has a lax anticheat without server authority
			// which does not require us to use any sort of bypasses
			return PlayerControl.LocalPlayer.Data.OwnerId == -4;
		}

		public static string GetPlayerColor(NetworkedPlayerInfo player)
		{
			int colorId = player.DefaultOutfit.ColorId;

			if(colorId < 0 || colorId >= Palette.ColorNames.Length)
			{
				return "Fortegreen";
			}

			return player.GetPlayerColorString();
		}
	}
}