using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
    internal class SabotageSection : ISection
    {
        public SabotageSection()
        {
			name = "Sabotage";
        }

        public override void Render()
        {
			if(ShipStatus.Instance == null)
			{
				GUILayout.Label("You are not currently in a game, or the game has not started yet. These options will not work.");
			}

            Sabotage.UpdateSystemsDirectly = GUILayout.Toggle(Sabotage.UpdateSystemsDirectly, "Update Sabotage Systems Directly");

            Dictionary<string, SystemTypes> sabotages = Sabotage.GetSabotages();
			Dictionary<string, SystemTypes> doors = Sabotage.GetDoors();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Sabotage All"))
			{
				Sabotage.SabotageAll();
				Hydra.notifications.Send("Sabotage", "All sabotages have been enabled.", 5);
			}

			if(GUILayout.Button("Close All Doors"))
			{
				Sabotage.LockAll();
				Hydra.notifications.Send("Sabotage", "All doors have been closed.", 5);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Fix All Sabotages"))
			{
				Sabotage.FixAllSabotages();
				Hydra.notifications.Send("Sabotage", "All sabotages have been repaired.", 5);
			}

			if(GUILayout.Button("Unlock All Doors"))
			{
				if(Sabotage.CanUnlockDoors())
				{
					Sabotage.UnlockAll();
					Hydra.notifications.Send("Sabotage", "All doors have been unlocked.", 5);
				} else {
					Hydra.notifications.Send("Sabotage", "The map you are currently on does not support unlocking doors.", 10);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("Sabotages:");
			foreach(var (key, value) in sabotages)
            {
                if(GUILayout.Button(key))
                {
                    Sabotage.SabotageSystem(value);
                    Hydra.notifications.Send("Sabotage", $"{key} has been sabotaged.", 5);
                }
            }

			GUILayout.Label("Close Doors:");
			if(doors.Count == 0)
			{
				GUILayout.Label("This map has no doors that can be closed.");
				return;
			}

			byte i = 0;
			foreach(var (key, value) in doors)
			{
				if(i % 2 == 0)
				{
					GUILayout.BeginHorizontal();
				}

				if(GUILayout.Button(key))
				{
					Sabotage.LockDoor(value);
				}

				if(i % 2 != 0)
				{
					GUILayout.EndHorizontal();
				}

				i++;
			}

			// If the amount of door sabotages is an odd number then we won't be ending the horizontal layout, so we check if we need to end it here
			if(i % 2 != 0)
			{
				GUILayout.EndHorizontal();
			}
		}
    }
}