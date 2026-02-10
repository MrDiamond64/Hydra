using HydraMenu.features;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
    internal class MovementSection : ISection
    {
        public MovementSection()
        {
            sectionName = "Movement";
        }

        public override void Render()
        {
            if(PlayerControl.LocalPlayer == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.");

                // Having all possible options shown at once (even if the player isn't in a game) is nice user experience
                GUILayout.Toggle(false, "Noclip");
            } else
            {
                // We don't want the position that includes the player's collider from PlayerControl::GetTruePosition()
                Vector2 position = PlayerControl.LocalPlayer.transform.position;

                GUILayout.Label($"Current Map: {Utilities.GetCurrentMap()}\nCurrent Position:\nX: {position.x:F2}\nY: {position.y:F2}");

				PlayerControl.LocalPlayer.Collider.enabled = !GUILayout.Toggle(!PlayerControl.LocalPlayer.Collider.enabled, "Noclip");
			}

            GUILayout.Label($"Speed Modifier: {Self.PlayerSpeedModifier.Multiplier:F2}x");
            Self.PlayerSpeedModifier.Multiplier = GUILayout.HorizontalSlider(Self.PlayerSpeedModifier.Multiplier, 0f, 5f);

			Teleporter.UseSnapToRPC = GUILayout.Toggle(Teleporter.UseSnapToRPC, "Use SnapTo RPC For Teleports");
			GUILayout.Label("Teleport To Location:");

			Dictionary<string, Vector2> teleportLocations = Teleporter.GetTeleportLocations();

            byte i = 0;
            foreach(var (key, value) in teleportLocations)
            {
                if(i % 2 == 0)
                {
                    GUILayout.BeginHorizontal();
                }

                if(GUILayout.Button(key))
                {
                    Teleporter.TeleportTo(value);
                }

                if(i % 2 != 0)
                {
                    GUILayout.EndHorizontal();
                }

                i++;
            }

            // If the amount of teleport locations is an odd number then we won't be ending the horizontal layout, so we check if we need to end it here
            if(i % 2 != 0)
            {
                GUILayout.EndHorizontal();
            }
        }
    }
}