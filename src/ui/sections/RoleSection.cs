using AmongUs.GameOptions;
using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class RolesSection : ISection
	{
		public RolesSection() : base("Roles") { }

		private RoleTypes selectedRole = RoleTypes.Crewmate;
		private Vector2 scrollPosition;

		public override void Render()
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			GUILayout.Label("General Role Cheats:");
			Roles.AllowVentingForCrewmates = GUILayout.Toggle(Roles.AllowVentingForCrewmates, "Vent As Crewmate");
			Roles.MoveModifier.MoveInVents = GUILayout.Toggle(Roles.MoveModifier.MoveInVents, "Move In Vents");
			Roles.SkipSabotageChecks.SabotageAsCrewmate = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageAsCrewmate, "Sabotage As Crewmate");
			Roles.SkipSabotageChecks.SabotageInVents = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageInVents, "Allow Sabotaging In Vents As Imposter");
			Roles.NoKillChecks = GUILayout.Toggle(Roles.NoKillChecks, "No Kill Checks");

			GUILayout.Space(10);
			GUILayout.Label("Impostor & Shapeshifter:");
			Roles.KillReach = GUILayout.Toggle(Roles.KillReach, "Infinite Kill Reach");
			Roles.DisableShapeshiftAnimation = GUILayout.Toggle(Roles.DisableShapeshiftAnimation, "Disable Shapeshift Animation");
			Roles.EndlessShapeshiftDuration = GUILayout.Toggle(Roles.EndlessShapeshiftDuration, "Endless Shapeshift Duration");

			GUILayout.Space(10);
			GUILayout.Label("Engineer:");
			Roles.EndlessVentTime = GUILayout.Toggle(Roles.EndlessVentTime, "Endless Vent Time");
			Roles.NoVentCooldown = GUILayout.Toggle(Roles.NoVentCooldown, "No Vent Cooldown");

			GUILayout.Space(10);
			GUILayout.Label("Scientist:");
			Roles.EndlessBattery = GUILayout.Toggle(Roles.EndlessBattery, "Endless Vitals Battery");
			Roles.NoVitalsCooldown = GUILayout.Toggle(Roles.NoVitalsCooldown, "No Vitals Cooldown");

			GUILayout.Space(10);
			GUILayout.Label("Tracker & Detective:");
			Roles.EndlessTracking = GUILayout.Toggle(Roles.EndlessTracking, "Endless Tracking Duration");
			Roles.NoTrackingDelay = GUILayout.Toggle(Roles.NoTrackingDelay, "No Track Delay");
			Roles.NoTrackingCooldown = GUILayout.Toggle(Roles.NoTrackingCooldown, "No Track Cooldown");
			Roles.TrackReach = GUILayout.Toggle(Roles.TrackReach, "Infinite Track Reach");
			Roles.InterrogateReach = GUILayout.Toggle(Roles.InterrogateReach, "Infinite Interrogate Reach");

			GUILayout.Space(10);
			GUILayout.Label("Role Changer:");
			GUILayout.Label($"Change role to: {selectedRole}");
			GUILayout.BeginHorizontal();
			selectedRole = Controls.HorizontalRoleSlider(selectedRole);

			if(GUILayout.Button("Apply Role" + (AmongUsClient.Instance.AmHost ? "" : " (Local)")))
			{
				UpdateRole(selectedRole);
			}
			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();
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