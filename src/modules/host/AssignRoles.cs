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

		[HarmonyPatch(typeof(LogicRoleSelectionNormal), nameof(LogicRoleSelectionNormal.AssignRolesFromList))]
		class AlwaysImposter
		{
			// Make sure List<T> is imported from Il2CppSystem otherwise things will go terribly wrong!
			static void Prefix(ref List<NetworkedPlayerInfo> players, ref List<RoleTypes> roleList, ref int rolesAssigned)
			{
				if(!Instance.Enabled || !AmongUsClient.Instance.AmHost) return;

				RoleTypes assignedRole = Instance.AssignedRole;
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
	}
}