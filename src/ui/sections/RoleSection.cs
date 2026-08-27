using AmongUs.GameOptions;
using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class RolesSection : Section
	{
		public RolesSection() : base("Roles") { }

		private RoleTypes selectedRole = RoleTypes.Crewmate;

		public override void Render()
		{
			ModuleManager.ventAsCrewmate.Enabled = GUILayout.Toggle(ModuleManager.ventAsCrewmate.Enabled, "Vent As Crewmate");
			ModuleManager.moveInVents.Enabled = GUILayout.Toggle(ModuleManager.moveInVents.Enabled, "Move In Vents");

			ModuleManager.unlockSabotageButton.SabotageAsCrewmate = GUILayout.Toggle(ModuleManager.unlockSabotageButton.SabotageAsCrewmate, "Sabotage As Crewmate");
			ModuleManager.unlockSabotageButton.SabotageInVents = GUILayout.Toggle(ModuleManager.unlockSabotageButton.SabotageInVents, "Allow Sabotaging In Vents As Imposter");

			ModuleManager.noShapeshiftAnimation.Enabled = GUILayout.Toggle(ModuleManager.noShapeshiftAnimation.Enabled, "Disable Shapeshift Animation");
			// Roles.DisablePhantomEndAnimation = GUILayout.Toggle(Roles.DisablePhantomEndAnimation, "Disable Phantom End Animation");

			ModuleManager.noKillChecks.Enabled = GUILayout.Toggle(ModuleManager.noKillChecks.Enabled, "No Kill Checks");

			GUILayout.Label($"Change role to: {selectedRole}");
			GUILayout.BeginHorizontal();
			selectedRole = Controls.HorizontalRoleSlider(selectedRole);

			if(GUILayout.Button("Apply Role" + (AmongUsClient.Instance.AmHost ? "" : " (Local)")))
			{
				UpdateRole(selectedRole);
			}

			GUILayout.EndHorizontal();
		}

		public static void UpdateRole(RoleTypes role)
		{
			Hydra.Log.LogInfo($"Updating role to {role}");

			bool isGhost = RoleManager.IsGhostRole(role);

			// When a player turns into the ghost, the PlayerControl::CoSetRole function hides the report button. This function then calls the RoleManager::SetRole function we call here
			// This means when we are changing between normal or ghost roles, the report button will not properly be added/removed, so we have to reimplement it here
			// We also cannot use PlayerControl::CoSetRole directly as it prevents in-game roles being overriden by non-ghosts ones (we could just patch it and disable overriding, however a blackout occurs when the game starts)
			HudManager.Instance.ReportButton.gameObject.SetActive(!isGhost);

			RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, role);

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo("Since we are host, we can send the SetRole RPC to sync the new role to the server");
				PlayerControl.LocalPlayer.RpcSetRole(role, true);
			}

			Hydra.notifications.Send("Update Role", $"Your role has been updated to {role}.");
		}
	}
}