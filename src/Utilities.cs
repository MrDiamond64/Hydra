using AmongUs.GameOptions;
using HydraMenu.features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu
{
	internal class Utilities
	{
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SkinData> allSkins = HatManager.Instance.allSkins;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<HatData> allHats = HatManager.Instance.allHats;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<VisorData> allVisors = HatManager.Instance.allVisors;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PetData> allPets = HatManager.Instance.allPets;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NamePlateData> allNameplates = HatManager.Instance.allNamePlates;

		public static int GetRandomUnusedColor()
		{
			List<int> colors = Enumerable.Range(0, 18).ToList();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				colors.Remove(player.Data.DefaultOutfit.ColorId);
			}

			System.Random rnd = new System.Random();

			// Some modded lobbies may have more than 18 players, which means there wont be enough unique colors for everyone
			// so we should take that edge case into account
			if(colors.Count == 0)
			{
				return rnd.Next(0, 18);
			}

			return colors[rnd.Next(0, colors.Count)];
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

			Network.BatchedMessage batch = new Network.BatchedMessage();

			if(Self.UseBypassRpc)
			{
				batch.UseAnticheatBypass();
			}

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
			Hydra.Log.LogInfo($"Attempting to start a meeting for {reporter.Data.PlayerName}");

			bool hasAnticheat = IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("Start Meeting", "The game must have started in order for this feature to work.");
				return;
			}

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo($"We are the host so we can directly use the StartMeeting RPC");

				if(ShipStatus.Instance == null)
				{
					Hydra.notifications.Send("Start Meeting", "There must be a valid instance of ShipStatus for this feature to work.");
				}
				else
				{
					OpenMeeting(reporter, target);
				}

				return;
			}

			Hydra.Log.LogInfo("We are not the host so we have to use the ReportDeadBody RPC");

			if(hasAnticheat && reporter != PlayerControl.LocalPlayer)
			{
				Hydra.notifications.Send("Start Meeting", "You must be the host of the lobby to make another player start a meeting.");
				return;
			}

			if(reporter.Data.IsDead)
			{
				Hydra.notifications.Send("Start Meeting", "You can only call meetings or report bodies if you are alive.");
				return;
			}

			if(hasAnticheat && target != null)
			{
				if(!target.IsDead)
				{
					Hydra.notifications.Send("Start Meeting", "You can only report bodies of players who have died in this round.");
					return;
				}

				if(!DoesDeadBodyExist(target.PlayerId))
				{
					Hydra.notifications.Send("Start Meeting", "Unable to find a dead body for this player, you can only report a player's body if they have died this round and their body has not dissolved.");
					return;
				}
			}

			Network.BatchedMessage batch = new Network.BatchedMessage();

			if(Self.UseBypassRpc)
			{
				batch.UseAnticheatBypass();
			}

			batch.QueueReportDeadBody(reporter, target);

			batch.FinishBatch();
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
				Hydra.notifications.Send("Shapeshift Player", "You must be the host of the lobby in order to use this feature.");
				return;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("Shapeshift Player", "The game must have started for this option to work.");
				return;
			}

			Network.BatchedMessage batch = new Network.BatchedMessage();

			if(Self.UseBypassRpc)
			{
				batch.UseAnticheatBypass();
			}

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
			if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
			{
				return (MapNames)AmongUsClient.Instance.TutorialMapId;
			}
			else
			{
				return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
			}
		}

		public static bool IsAnticheatPresent()
		{
			if(Constants.IsVersionModded() || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || Self.UseBypassRpc) return false;

			// On freeplay, local, and modded lobbies, NetworkedPlayerInfo net objects are owned by the host (-2)
			// On vanilla lobbies, NetworkedPlayerInfo net objects are owned by the backend among us servers (-4)
			// If our NetworkedPlayerInfo net object is owned by the host, we can assume that the lobby has a lax anticheat without server authority
			// which does not require us to use any sort of bypasses
			return PlayerControl.LocalPlayer.Data.OwnerId != -2;
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