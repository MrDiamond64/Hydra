using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine.AddressableAssets;

namespace HydraMenu.features
{
	internal class Host
	{
		private static bool isSkeldFlipped = false;
		public static bool FlippedSkeld
		{
			get { return isSkeldFlipped; }
			set
			{
				if(AmongUsClient.Instance == null || isSkeldFlipped == value) return;

				// ShipPrefabs is a list corresponding map IDs to their map
				// ID 0 is Skeld, 1 is Mira, 2 is Polus, and 3 is Dleks
				// If we want to be able to spawn in Dleks (as this is normally inaccessible) we can swap the two elements
				// so that 0 is Dleks and 3 is Skeld, spawning in Dleks instead of Skeld
				AssetReference temp = AmongUsClient.Instance.ShipPrefabs[3];
				AmongUsClient.Instance.ShipPrefabs[3] = AmongUsClient.Instance.ShipPrefabs[0];
				AmongUsClient.Instance.ShipPrefabs[0] = temp;

				isSkeldFlipped = value;
			}
		}

		// When a player reports a body, their client sends a ReportDeadBody RPC to the host. The host then should validate the RPC and start a meeting
		// To block meetings, we can simply ignore any received ReportDeadBody RPCs
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
		public static class DisableMeetings
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix()
			{
				return !Enabled;
			}
		}

		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
		public static class DisableSabotages
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix()
			{
				return !Enabled;
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
		public static class DisableCloseDoors
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix()
			{
				return !Enabled;
			}
		}

		/*
		[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldFlipSkeld))]
		public static class FlippedSkeld
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix(ref bool __result)
			{
				__result = Enabled;
				return false;
			}
		}
		*/

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetLevel))]
		public static class BlockLowLevels
		{
			public static bool Enabled { get; set; } = false;
			public static uint MinLevel { get; set; } = 20;

			static void Prefix(PlayerControl __instance, uint level)
			{
				if(!Enabled || !AmongUsClient.Instance.AmHost || __instance.PlayerId == PlayerControl.LocalPlayer.PlayerId|| level > MinLevel) return;

				Hydra.notifications.Send("Block Low Levels", $"{__instance.Data.PlayerName} is level {level}, which is below the level threshold. They will be kicked from the game.");
				AmongUsClient.Instance.KickPlayer(__instance.OwnerId, false);
			}
		}

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanBan))]
		public static class BanMidGame
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(InnerNetClient __instance, ref bool __result)
			{
				if(!Enabled) return true;

				__result = __instance.AmHost;
				return false;
			}
		}

		// It is not possible to watch security cameras when the comms sabotage is active. We can abuse this to disable security cameras
		// When a player starts to watch security cameras, sabotage comms for that player, when the player stops watching cameras, fix comms sabotage for that player
		[HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.UpdateSystem))]
		public static class DisableCameras
		{
			public static bool Enabled { get; set; } = false;

			static void Postfix(PlayerControl player, MessageReader msgReader)
			{
				if(!Enabled || !AmongUsClient.Instance.AmHost || player.OwnerId == AmongUsClient.Instance.HostId) return;

				// Prevent an exploit where if the comms sabotage is active, someone could enter and leave the security cameras to remove the comms effect from themselves
				if(Sabotage.IsSabotageActive(SystemTypes.Comms))
				{
					// There is an edge case where if someone is on the security cameras panel when comms are actively sabotaged, and the sabotage is fixed,
					// then the player will be able to watch the security cameras
					// I don't think it is worthwhile to fix this edge case considering this feature is unlikely to even be used by anyone
					Hydra.Log.LogMessage($"{player.Data.name} updated security cameras, we do not need to do anything as the Comms sabotage is already active");
					return;
				}

				Hydra.Log.LogMessage($"{player.Data.PlayerName} updated security cameras, sending Comms system update");

				msgReader.Position--;
				// 1 = Player started to watch cameras, 2 (and every other value) = Player stopped watching cameras
				byte operation = msgReader.ReadByte();

				MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
				systemUpdate.StartMessage((byte)SystemTypes.Comms);
				// 1 = Comms sabotage is active, 0 = Comms sabotage is inactive
				systemUpdate.Write(operation == 1);
				systemUpdate.EndMessage();

				Network.SendDataFlag(ShipStatus.Instance.NetId, systemUpdate, player.OwnerId);
			}
		}

		[HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
		public static class DisableGameEnd
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix()
			{
				return !Enabled;
			}
		}
	}
}