using AmongUs.GameOptions;
using HydraMenu.features;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class RolesSection : ISection
	{
		public RolesSection() : base("Roles") 
		{
			AddFeature("Vent As Crewmate", () => {
				Roles.AllowVentingForCrewmates = GUILayout.Toggle(Roles.AllowVentingForCrewmates, "Vent As Crewmate");
			});
			AddFeature("Move In Vents", () => {
				Roles.MoveModifier.MoveInVents = GUILayout.Toggle(Roles.MoveModifier.MoveInVents, "Move In Vents");
			});
			AddFeature("Sabotage As Crewmate", () => {
				Roles.SkipSabotageChecks.SabotageAsCrewmate = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageAsCrewmate, "Sabotage As Crewmate");
			});
			AddFeature("Allow Sabotaging In Vents As Imposter", () => {
				Roles.SkipSabotageChecks.SabotageInVents = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageInVents, "Allow Sabotaging In Vents As Imposter");
			});
			AddFeature("Disable Shapeshift Animation", () => {
				Roles.DisableShapeshiftAnimation = GUILayout.Toggle(Roles.DisableShapeshiftAnimation, "Disable Shapeshift Animation");
			});
			AddFeature("No Kill Checks", () => {
				Roles.NoKillChecks = GUILayout.Toggle(Roles.NoKillChecks, "No Kill Checks");
			});
			AddFeature("Show Kill Cooldown", () => {
				Visuals.ShowKillCooldown = GUILayout.Toggle(Visuals.ShowKillCooldown, "Show Kill Cooldown");
			});
			AddFeature("Show Imposter", () => {
				Visuals.ShowImpostors = GUILayout.Toggle(Visuals.ShowImpostors, "Show Imposter");
			});
			AddFeature("Reveal All Roles", () => {
				Visuals.RevealRoles = GUILayout.Toggle(Visuals.RevealRoles, "Reveal All Roles");
			});
		}

		private RoleTypes selectedRole = RoleTypes.Crewmate;

		public override void Render()
		{
			foreach (var feature in Features)
			{
				feature.RenderAction();
			}

			GUILayout.Space(10);
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