using HydraMenu.modules;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class MovementSection : Section
	{
		public MovementSection() : base("Movement") { }

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");

				// Having all possible options shown at once (even if the player isn't in a game) is nice user experience
				GUILayout.Toggle(false, "Noclip");
			}
			else
			{
				// We don't want the position that includes the player's collider from PlayerControl::GetTruePosition()
				Vector2 position = PlayerControl.LocalPlayer.transform.position;

				GUILayout.Label($"Current Map: {Utilities.GetCurrentMap()}\nCurrent Position:\nX: {position.x:F2}\nY: {position.y:F2}");

				PlayerControl.LocalPlayer.Collider.enabled = !GUILayout.Toggle(!PlayerControl.LocalPlayer.Collider.enabled, "Noclip");
			}

			GUILayout.Label($"Speed Modifier: {ModuleManager.speedModifier.Multiplier:F2}x");
			ModuleManager.speedModifier.Multiplier = GUILayout.HorizontalSlider(ModuleManager.speedModifier.Multiplier, 0f, 5f);

			Teleporter.UseSnapToRPC = GUILayout.Toggle(Teleporter.UseSnapToRPC, "Use SnapTo RPC For Teleports");
			GUILayout.Label("Teleport To Location:");

			Dictionary<string, Vector2> teleportLocations = Teleporter.GetTeleportLocations();
			Controls.DrawButtonCell(teleportLocations, HandleTeleport, 2);
		}

		private void HandleTeleport(Vector2 location)
		{
			Teleporter.TeleportTo(location);
		}
	}
}