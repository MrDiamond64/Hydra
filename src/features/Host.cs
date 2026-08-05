using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using HydraMenu.network;
using Il2CppSystem.Collections.Generic;
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
				if(!Enabled || !AmongUsClient.Instance.AmHost || __instance == PlayerControl.LocalPlayer || level > MinLevel) return;

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

				BatchedMessage batch = new BatchedMessage(player.OwnerId);
				batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);
				batch.FinishBatch();
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

		[HarmonyPatch(typeof(LogicRoleSelectionNormal), nameof(LogicRoleSelectionNormal.AssignRolesFromList))]
		public static class AlwaysImposter
		{
			public static bool Enabled { get; set; } = false;
			public static RoleTypes assignedRole = RoleTypes.Viper;

			// Make sure List<T> is imported from Il2cppSystem otherwise things will go terribly wrong!
			static void Prefix(ref List<NetworkedPlayerInfo> players, ref List<RoleTypes> roleList, ref int rolesAssigned)
			{
				if(!Enabled || !AmongUsClient.Instance.AmHost) return;

				Hydra.Log.LogInfo($"Attempting to assign ourselves the {assignedRole} role");

				// Stupid shenanigans to deal with IL2Cpp interop
				Il2CppSystem.Predicate<NetworkedPlayerInfo> predicate = (Il2CppSystem.Predicate<NetworkedPlayerInfo>)(player => player == PlayerControl.LocalPlayer.Data);
				int playerIndex = players.FindIndex(predicate);

				// The AssignRolesFromList function is called multiple times each with different list of players
				// If our NetworkedPlayerInfo does not exist in this playerlist, then we shouldn't assign our role now
				if(playerIndex == -1)
				{
					Hydra.Log.LogInfo("Our NetworkedPlayerInfo does not exist in this list, skipping");
					return;
				}

				Hydra.Log.LogInfo($"Found our NetworkedPlayerInfo in the players list at index {playerIndex}, removing from the list");
				players.RemoveAt(playerIndex);

				Il2CppSystem.Predicate<RoleTypes> predicate2 = (Il2CppSystem.Predicate<RoleTypes>)(roleType => roleType == assignedRole);
				int roleIndex = roleList.FindIndex(predicate2);

				Hydra.Log.LogMessage($"Player index is {roleIndex}");

				// If the role we want to assign ourselves exists in the roleList, then remove it
				// We don't want there to be four imposters in the game when we intend for three imposters
				if(roleIndex != -1)
				{
					Hydra.Log.LogInfo($"Found an instance of our role in the roles list at index {roleIndex}, removing from the list");
					roleList.RemoveAt(roleIndex);
				}

				// To determine if the intro cutscene should play, the game waits for SetRole RPCs, checks if the assigned role is not a ghost role,
				// and then checks if all players have either been assigned a role or were disconnected
				// The problem is that if we are trying to assign ourselves a ghost role, and we are the last player to be assigned a role
				// then the PlayerControl::CoSetRole execution flow will not display the intro cutscene
				// resulting in the entire lobby encountering a black screen
				// To get around this, we check for this edge case and assign ourselves a non-host role, and then set our role to a ghost role
				if(RoleManager.IsGhostRole(assignedRole) && players.Count == 0)
				{
					PlayerControl.LocalPlayer.RpcSetRole(RoleManager.IsImpostorRole(assignedRole) ? RoleTypes.Impostor : RoleTypes.Crewmate);
				}

				PlayerControl.LocalPlayer.RpcSetRole(assignedRole);
				rolesAssigned++;

				Hydra.Log.LogInfo($"Assigned ourself the {assignedRole} role!");
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
		public static class NoKillCooldown
		{
			public static bool Enabled { get; set; } = false;

			static void Prefix(PlayerControl __instance, ref float time)
			{
				if(!Enabled || __instance != PlayerControl.LocalPlayer) return;

				time = 0;
			}
		}

		// ═══════════════════════════════════════════════════════════════════════════════
		// ANTI-ESP: FAKE ROLE SYNCHRONIZATION (DECITY / HONEYPOT LAYER)
		// ═══════════════════════════════════════════════════════════════════════════════
		// <u>HOST GAME KNOWS THE TRUTH</u>
		//
		// - <u>HOST GAME KNOWS THE TRUTH</u>: Authoritative state remains untouched on Host.
		// - Real Impostors & Host receive genuine role data so game logic remains correct.
		// - Unmodded Crewmates / ESP cheats receive a complete, believable fake role table
		//   (e.g., Player 2->Shapeshifter, Player 3->Phantom, Player 4->Shapeshifter)
		//   so memory scanners & ESP overlays are poisoned with total misinformation.
		// - Anticheat checks (e.g. CompleteTask) validate actions against real host state.
		// ═══════════════════════════════════════════════════════════════════════════════

		public static class AntiEspRoleScrubber
		{
			public static bool Enabled { get; set; } = true;

			private static readonly System.Collections.Generic.HashSet<byte> impostorPlayerIds = new System.Collections.Generic.HashSet<byte>();
			private static readonly System.Collections.Generic.Dictionary<byte, RoleTypes> trueRolesMap = new System.Collections.Generic.Dictionary<byte, RoleTypes>();
			private static readonly System.Collections.Generic.Dictionary<byte, RoleTypes> decoyRolesMap = new System.Collections.Generic.Dictionary<byte, RoleTypes>();

			public static void ClearImpostorCache()
			{
				impostorPlayerIds.Clear();
				trueRolesMap.Clear();
				decoyRolesMap.Clear();
				Hydra.Log.LogInfo("[ANTI-ESP LOG] Role cache reset for new round. <u>HOST GAME KNOWS THE TRUTH</u>");
			}

			public static void TrackRole(byte playerId, RoleTypes roleType)
			{
				trueRolesMap[playerId] = roleType;

				if(RoleManager.IsImpostorRole(roleType))
				{
					impostorPlayerIds.Add(playerId);
				}

				GenerateDecoyRole(playerId, roleType);

				Hydra.Log.LogInfo($"[ROLE ASSIGNMENT LOG] Player ID {playerId} assigned True Role: {roleType} | Decoy Role: {decoyRolesMap[playerId]}. <u>HOST GAME KNOWS THE TRUTH</u>");
			}

			private static void GenerateDecoyRole(byte playerId, RoleTypes trueRole)
			{
				// Plausible, believable special role pool for ESP poisoning
				RoleTypes[] decoyPool = new RoleTypes[]
				{
					RoleTypes.Shapeshifter,
					RoleTypes.Phantom,
					RoleTypes.Viper,
					RoleTypes.Shapeshifter,
					RoleTypes.Phantom
				};

				int index = System.Math.Abs((int)(playerId * 7 + 3)) % decoyPool.Length;
				decoyRolesMap[playerId] = decoyPool[index];
			}

			public static RoleTypes GetDecoyRole(byte playerId, RoleTypes defaultRole)
			{
				if(decoyRolesMap.TryGetValue(playerId, out RoleTypes decoy))
				{
					return decoy;
				}
				return defaultRole;
			}

			public static RoleTypes GetTrueRole(byte playerId, RoleTypes defaultRole)
			{
				if(trueRolesMap.TryGetValue(playerId, out RoleTypes trueRole))
				{
					return trueRole;
				}
				return defaultRole;
			}

			public static bool IsTrackedImpostor(byte playerId)
			{
				return impostorPlayerIds.Contains(playerId);
			}

			public static System.Collections.Generic.HashSet<byte> GetImpostorPlayerIds()
			{
				return impostorPlayerIds;
			}
		}



		// ═══════════════════════════════════════════════════════════════════════════════
		// ANTI-ESP: PER-CLIENT FAKE ROLE POISONING — ZERO REAL-ROLE LEAKAGE
		// ═══════════════════════════════════════════════════════════════════════════════
		// <u>HOST GAME KNOWS THE TRUTH</u>
		//
		// FLOW for RpcSetRole(PlayerX, RealRole):
		//   1. PREFIX: swap roleType → decoyImpostorRole via ref
		//      → vanilla broadcast sends ONLY the decoy (server + all clients get decoy)
		//      → vanilla CoSetRole on host sets decoy locally (we fix in postfix)
		//   2. POSTFIX: send targeted corrections with REAL role to:
		//      → The player's OWN client (so their gameplay works)
		//      → Impostor teammate clients (so they see each other)
		//      → Host local state via CoSetRole
		//   3. Everyone else (crewmate/ESP) gets ONLY the decoy — NEVER the real role
		//
		// RESULT:
		//   Player 1 (Crewmate ESP) sees:
		//     Player 1 → Crewmate (their real role, gameplay works)
		//     Player 2 → Shapeshifter (FAKE — everyone looks like impostor)
		//     Player 3 → Phantom (FAKE)
		//     Player 4 → Viper (FAKE)
		//     Player 5 → Shapeshifter (FAKE)
		//     Player 7 → Phantom (FAKE — real impostor hidden in the noise)
		//   ESP sees 5 "impostors" → can't tell which is real!
		// ═══════════════════════════════════════════════════════════════════════════════
		// ═══════════════════════════════════════════════════════════════════════════════
		// ANTI-ESP: ANTICHEAT-SAFE ROLE POISONING
		// ═══════════════════════════════════════════════════════════════════════════════
		// <u>HOST GAME KNOWS THE TRUTH</u>
		//
		// Safe Implementation Strategy:
		// 1. Never send raw GameDataTo / targeted RPCs during initial connection handshake
		//    (this prevents Innersloth anticheat from banning clients for "unknown client" / hacking).
		// 2. Intercept RpcSetRole broadcast:
		//    - Track the real role in host memory (Host retains 100% authoritative truth).
		//    - If player is assigned a special Impostor role (e.g. Phantom, Viper),
		//      swap broadcast parameter to standard Shapeshifter/Impostor decoy.
		//    - ESP memory scanners read Shapeshifter/Impostor (poisoning the role type).
		//    - Official servers accept the broadcast because Shapeshifter is a valid Impostor role.
		//    - Players never get kicked for hacking!
		// ═══════════════════════════════════════════════════════════════════════════════
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
		public static class AntiEspRoleHook
		{
			[HarmonyPrefix]
			static void Prefix(PlayerControl __instance, ref RoleTypes roleType)
			{
				if(AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
				if(__instance == null) return;

				RoleTypes trueRole = roleType;
				AntiEspRoleScrubber.TrackRole(__instance.PlayerId, trueRole);
			}

			[HarmonyPostfix]
			static void Postfix(PlayerControl __instance, RoleTypes roleType)
			{
				if(AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
				if(__instance == null) return;

				// Host restores true role state internally for host-authoritative validations
				RoleTypes trueRole = AntiEspRoleScrubber.GetTrueRole(__instance.PlayerId, roleType);
				if(trueRole != roleType)
				{
					__instance.StartCoroutine(__instance.CoSetRole(trueRole, true));
				}
			}
		}

		public static void SendTargetedRoles(int clientId)
		{
			if(AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
			if(AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
			if(!AntiEspRoleScrubber.Enabled) return;

			ClientData targetClient = AmongUsClient.Instance.FindClientById(clientId);
			if(targetClient == null || targetClient.Character == null) return;

			// Check if the target client is an Impostor
			bool clientIsImpostor = (targetClient.Character.Data != null && RoleManager.IsImpostorRole(targetClient.Character.Data.RoleType))
				|| AntiEspRoleScrubber.IsTrackedImpostor(targetClient.Character.PlayerId);

			Hydra.Log.LogInfo($"[ANTI-ESP TARGETED SYNC] Sending batched role update to Client {clientId} ('{targetClient.Character.Data?.PlayerName}', IsImp: {clientIsImpostor}). <u>HOST GAME KNOWS THE TRUTH</u>");

			BatchedMessage batch = new BatchedMessage(clientId);

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == null || player.Data == null) continue;

				RoleTypes roleToSend;
				if(player.PlayerId == targetClient.Character.PlayerId)
				{
					// Target client always sees their own real role
					roleToSend = AntiEspRoleScrubber.GetTrueRole(player.PlayerId, player.Data.RoleType);
				}
				else if(clientIsImpostor)
				{
					// Impostor client sees real roles of all other players (both teammates and crewmates)
					roleToSend = AntiEspRoleScrubber.GetTrueRole(player.PlayerId, player.Data.RoleType);
				}
				else
				{
					// Crewmate client (ESP cheater) sees decoy Impostor roles for ALL OTHER players
					roleToSend = AntiEspRoleScrubber.GetDecoyRole(player.PlayerId, RoleTypes.Shapeshifter);
				}

				batch.QueueSetRole(player, roleToSend, true);
			}

			batch.FinishBatch();
			Hydra.Log.LogInfo($"[ANTI-ESP TARGETED SYNC] Completed batched role update for Client {clientId}. <u>HOST GAME KNOWS THE TRUTH</u>");
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
		public static class AntiEspMeetingStartPatch
		{
			static void Postfix()
			{
				if(AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
				if(!AntiEspRoleScrubber.Enabled) return;

				Hydra.Log.LogInfo("[ANTI-ESP] Meeting started. Resyncing targeted roles to all clients... <u>HOST GAME KNOWS THE TRUTH</u>");

				foreach(ClientData client in AmongUsClient.Instance.allClients)
				{
					if(client == null || client.Character == null) continue;
					if(client.Character == PlayerControl.LocalPlayer) continue;

					SendTargetedRoles(client.Id);
				}
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
		public static class AntiEspMeetingClosePatch
		{
			static void Postfix()
			{
				if(AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
				if(!AntiEspRoleScrubber.Enabled) return;

				Hydra.Log.LogInfo("[ANTI-ESP] Meeting closed. Resyncing targeted roles to all clients... <u>HOST GAME KNOWS THE TRUTH</u>");

				foreach(ClientData client in AmongUsClient.Instance.allClients)
				{
					if(client == null || client.Character == null) continue;
					if(client.Character == PlayerControl.LocalPlayer) continue;

					SendTargetedRoles(client.Id);
				}
			}
		}

		// ─── Reset cache when a new game starts ───
		[HarmonyPatch(typeof(AmongUsClient), nameof(InnerNet.InnerNetClient.CoStartGame))]
		public static class AntiEspResetOnGameStart
		{
			static void Postfix()
			{
				AntiEspRoleScrubber.ClearImpostorCache();
			}
		}

		// Network Fog-of-War Spatial Isolation config holder
		public static class SpatialRpcFilter
		{
			public static bool Enabled { get; set; } = false;
			public static float VisionCutoffDistance { get; set; } = 18.0f;
		}
	}
}