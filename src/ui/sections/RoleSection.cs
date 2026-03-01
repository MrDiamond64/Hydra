using AmongUs.GameOptions;
using HydraMenu.features;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class RolesSection : ISection
	{
		public RolesSection()
		{
			name = "Roles";
		}

		private byte selectedRole = 0;

		// The RoleTypes enum has some weird gaps, like everything from Crewmate (0) to Tracker (10) is normal, but then Detective is 12 and Viper is 18
		// https://www.innersloth.com/2026-roadmap-part-1/
		// The Among Us 2026 roadmap does state that there are currently 15 prototype roles in the works,
		// could these gaps be attributed to roles that have not been added to the retail version of the game?
		public readonly Dictionary<byte, RoleTypes> roles = new Dictionary<byte, RoleTypes>()
		{
			{ 0, RoleTypes.Crewmate },
			{ 1, RoleTypes.Impostor },
			{ 2, RoleTypes.Scientist},
			{ 3, RoleTypes.Engineer},
			{ 4, RoleTypes.GuardianAngel },
			{ 5, RoleTypes.Shapeshifter },
			{ 6, RoleTypes.Noisemaker },
			{ 7, RoleTypes.Phantom },
			{ 8, RoleTypes.Tracker },
			{ 9, RoleTypes.Detective },
			{ 10, RoleTypes.Viper },
			{ 11, RoleTypes.CrewmateGhost },
			{ 12, RoleTypes.ImpostorGhost }
		};

		public override void Render()
		{
			Roles.AllowVentingForCrewmates = GUILayout.Toggle(Roles.AllowVentingForCrewmates, "Vent As Crewmate");
			Roles.MoveModifier.MoveInVents = GUILayout.Toggle(Roles.MoveModifier.MoveInVents, "Move In Vents");

			Roles.SkipSabotageChecks.SabotageAsCrewmate = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageAsCrewmate, "Sabotage As Crewmate");
			Roles.SkipSabotageChecks.SabotageInVents = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageInVents, "Allow Sabotaging In Vents As Imposter");

			Roles.NoKillCooldown.Enabled = GUILayout.Toggle(Roles.NoKillCooldown.Enabled, "No Kill Cooldown");
			Roles.DisableShapeshiftAnimation = GUILayout.Toggle(Roles.DisableShapeshiftAnimation, "Disable Shapeshift Animation");
			// Roles.DisablePhantomEndAnimation = GUILayout.Toggle(Roles.DisablePhantomEndAnimation, "Disable Phantom End Animation");

			RoleTypes role = roles[selectedRole];
			GUILayout.Label($"Change role to: {role}");

			GUILayout.BeginHorizontal();
			selectedRole = (byte)GUILayout.HorizontalSlider((float)selectedRole, 0, roles.Count - 1);

			if(GUILayout.Button("Apply Role" + (AmongUsClient.Instance.AmHost ? "" : " (Local)")) && PlayerControl.LocalPlayer)
			{
				Hydra.Log.LogInfo($"Updating role to {role}");
				Roles.UpdateRole(role);

				if(AmongUsClient.Instance.AmHost)
				{
					Hydra.Log.LogInfo("Since we are host, we can send the SetRole RPC to sync the new role to the server");
					PlayerControl.LocalPlayer.RpcSetRole(role, true);
				}

				Hydra.notifications.Send("Update Role", $"Your role has been updated to {role}.");
			}
			GUILayout.EndHorizontal();
		}
	}
}