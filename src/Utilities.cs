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
                Network.SendCheckColor((byte)rnd.Next(0, 15));

                // string randomName = AccountManager.Instance.GetRandomName();
                // PlayerControl.LocalPlayer.CmdCheckName(randomName);

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

		public static PlayerControl GetRandomPlayer(bool excludeHost = false, bool excludeDead = false, bool excludeImposters = false)
		{
			Il2CppSystem.Collections.Generic.List<PlayerControl> allPlayers = PlayerControl.AllPlayerControls;
			List<PlayerControl> validPlayers = [];

			foreach(PlayerControl player in allPlayers)
			{
				if(
					(player.PlayerId == PlayerControl.LocalPlayer.PlayerId) ||
					(excludeHost && AmongUsClient.Instance.HostId == player.OwnerId) ||
					(excludeDead && player.Data.IsDead) ||
					(excludeImposters && player.Data.Role.CanUseKillButton)
				) continue;

				validPlayers.Add(player);
			}

			System.Random rnd = new System.Random();
			return validPlayers[rnd.Next(validPlayers.Count)];
		}

		public static void CopyPlayer(PlayerControl player)
        {
            NetworkedPlayerInfo.PlayerOutfit outfit = player.CurrentOutfit;

            if(AmongUsClient.Instance.AmHost)
            {
                PlayerControl.LocalPlayer.RpcSetName(outfit.PlayerName);
                PlayerControl.LocalPlayer.RpcSetColor((byte)outfit.ColorId);
            } else
            {
                // Changing player names mid-game through the CheckName RPC was patched in December 2025
                Network.SendCheckColor((byte)outfit.ColorId);
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

        public static MapNames GetCurrentMap()
        {
            if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            {
                return (MapNames)AmongUsClient.Instance.TutorialMapId;
            } else {
                return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
            }
        }
    }
}