using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;

namespace HydraMenu.modules.host
{
	internal class AssignRoles : Module
	{
		public AssignRoles() : base("AssignRoles") { }

		private static AssignRoles Instance
		{
			get { return ModuleManager.assignRoles; }
		}

		public RoleTypes AssignedRole { get; set; } = RoleTypes.Viper;
		public System.Collections.Generic.Dictionary<byte, RoleTypes> CustomPlayerRoles { get; } = new System.Collections.Generic.Dictionary<byte, RoleTypes>();

		public void SetPlayerRole(byte playerId, RoleTypes role)
		{
			CustomPlayerRoles[playerId] = role;
		}

		public void RemovePlayerRole(byte playerId)
		{
			CustomPlayerRoles.Remove(playerId);
		}

		public void ClearCustomRoles()
		{
			CustomPlayerRoles.Clear();
		}

		public static void ApplyRole(PlayerControl target, RoleTypes role)
		{
			if(target == null) return;
			try
			{
				if(RoleManager.Instance != null)
				{
					RoleManager.Instance.SetRole(target, role);
				}

				if(AmongUsClient.Instance.AmHost)
				{
					target.RpcSetRole(role, true);
				}

				if(target == PlayerControl.LocalPlayer && HudManager.Instance != null && HudManager.Instance.ReportButton != null)
				{
					HudManager.Instance.ReportButton.gameObject.SetActive(!RoleManager.IsGhostRole(role));
				}

				Hydra.Log.LogInfo($"[AssignRoles] Applied {role} to {target.Data?.PlayerName}");
			}
			catch(System.Exception ex)
			{
				Hydra.Log.LogError($"[AssignRoles] Error applying role {role}: {ex.Message}");
			}
		}

		[HarmonyPatch(typeof(LogicRoleSelectionNormal), nameof(LogicRoleSelectionNormal.AssignRolesFromList))]
		class AlwaysImposter
		{
			// Make sure List<T> is imported from Il2CppSystem otherwise things will go terribly wrong!
			static void Prefix(ref List<NetworkedPlayerInfo> players, ref List<RoleTypes> roleList, ref int rolesAssigned)
			{
				if(!Instance.Enabled || !AmongUsClient.Instance.AmHost || players == null || roleList == null || roleList.Count == 0) return;

				bool isImpostorPass = RoleManager.IsImpostorRole(roleList[0]);

				for(int i = players.Count - 1; i >= 0; i--)
				{
					NetworkedPlayerInfo playerInfo = players[i];
					if(playerInfo == null) continue;

					RoleTypes targetRole = RoleTypes.Crewmate;
					bool hasCustomRole = false;

					if(Instance.CustomPlayerRoles.TryGetValue(playerInfo.PlayerId, out RoleTypes customRole))
					{
						targetRole = customRole;
						hasCustomRole = true;
					}
					else if(PlayerControl.LocalPlayer != null && playerInfo == PlayerControl.LocalPlayer.Data)
					{
						targetRole = Instance.AssignedRole;
						hasCustomRole = true;
					}

					if(!hasCustomRole) continue;

					// Only assign during the matching pass (impostor vs crewmate/special) to avoid breaking game balance
					bool isTargetImpostor = RoleManager.IsImpostorRole(targetRole);
					if(isTargetImpostor != isImpostorPass) continue;

					Hydra.Log.LogInfo($"Found player {playerInfo.PlayerName} in players list, assigning {targetRole} role");
					players.RemoveAt(i);

					Il2CppSystem.Predicate<RoleTypes> rolePred = (Il2CppSystem.Predicate<RoleTypes>)(roleType => roleType == targetRole);
					int roleIndex = roleList.FindIndex(rolePred);
					if(roleIndex != -1)
					{
						Hydra.Log.LogInfo($"Found an instance of {targetRole} in the roles list at index {roleIndex}, removing from list");
						roleList.RemoveAt(roleIndex);
					}

					PlayerControl targetControl = null;
					foreach(PlayerControl pc in PlayerControl.AllPlayerControls)
					{
						if(pc != null && pc.PlayerId == playerInfo.PlayerId)
						{
							targetControl = pc;
							break;
						}
					}

					if(targetControl != null)
					{
						// To avoid blackscreen bug if assigning ghost role to the last player
						if(RoleManager.IsGhostRole(targetRole) && players.Count == 0)
						{
							targetControl.RpcSetRole(RoleManager.IsImpostorRole(targetRole) ? RoleTypes.Impostor : RoleTypes.Crewmate);
						}

						targetControl.RpcSetRole(targetRole, true);
						rolesAssigned++;
						Hydra.Log.LogInfo($"Assigned {playerInfo.PlayerName} the {targetRole} role!");
					}
				}
			}
		}

		private void OnGameStart()
		{
			if(AmongUsClient.Instance.AmHost || Utilities.IsAnticheatPresent()) return;
			Hydra.Log.LogMessage($"We are in a host-authoritative lobby, we can hijack the assigned roles");

			// If we are in Hide and Seek, we can assign everyone the Engineer role so the host doesn't assign a second impostor
			if(GameManager.Instance.IsHideAndSeek() && RoleManager.IsImpostorRole(AssignedRole))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player == PlayerControl.LocalPlayer) continue;

					player.RpcSetRole(RoleTypes.Engineer, false);
				}
			}

			PlayerControl.LocalPlayer.RpcSetRole(AssignedRole, false);
		}

		private void OnGameLoad()
		{
			if(!Enabled || !AmongUsClient.Instance.AmHost) return;

			foreach(var kvp in CustomPlayerRoles)
			{
				foreach(PlayerControl pc in PlayerControl.AllPlayerControls)
				{
					if(pc != null && pc.PlayerId == kvp.Key)
					{
						ApplyRole(pc, kvp.Value);
						break;
					}
				}
			}
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameStart += OnGameStart;
			EventCoordinator.OnGameLoad += OnGameLoad;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
			EventCoordinator.OnGameLoad -= OnGameLoad;
		}
	}
}