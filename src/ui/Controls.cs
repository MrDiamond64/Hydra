using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui
{
	internal class Controls
	{
		// The RoleTypes enum has some weird gaps, everything from Crewmate (0) to Tracker (10) is normal, but then Detective is 12 and Viper is 18
		// https://www.innersloth.com/2026-roadmap-part-1/
		// The Among Us 2026 roadmap does state that there are currently 15 prototype roles in the works,
		// could these gaps be attributed to roles that have not been added to the retail version of the game?
		public static readonly List<RoleTypes> RolesList = new List<RoleTypes>()
		{
			RoleTypes.Crewmate,
			RoleTypes.Impostor,
			RoleTypes.Scientist,
			RoleTypes.Engineer,
			RoleTypes.GuardianAngel,
			RoleTypes.Shapeshifter,
			RoleTypes.Noisemaker,
			RoleTypes.Phantom,
			RoleTypes.Tracker,
			RoleTypes.Detective,
			RoleTypes.Viper,
			RoleTypes.CrewmateGhost,
			RoleTypes.ImpostorGhost
		};

		public enum PlayerColors
		{
			Red,
			Blue,
			Green,
			Pink,
			Orange,
			Yellow,
			Black,
			White,
			Purple,
			Brown,
			Cyan,
			Lime,
			Maroon,
			Rose,
			Banana,
			Gray,
			Tan,
			Coral,
			Fortegreen
		}


		public static RoleTypes HorizontalRoleSlider(RoleTypes currentRole)
		{
			int currentValue = RolesList.IndexOf(currentRole);

			byte newValue = (byte)GUILayout.HorizontalSlider(currentValue, 0, RolesList.Count - 1);

			return RolesList[newValue];
		}

		public static PlayerColors HorizontalColorSlider(PlayerColors currentColor)
		{
			return (PlayerColors)GUILayout.HorizontalSlider((int)currentColor, 0, Palette.ColorNames.Length);
		}


		public static bool PlayerSpecificToggle(string label, PlayerControl selectedPlayer, ref PlayerControl currentPlayer)
		{
			GUIStyle toggle = new GUIStyle(GUI.skin.toggle);
			// We do not want the toggle to appear as enabled if selectedPlayer and currentPlayer are both null
			bool isCurrentSelection = selectedPlayer != null && selectedPlayer == currentPlayer;

			if (isCurrentSelection)
			{
				toggle.normal = toggle.onNormal;
				toggle.active = toggle.onActive;
				toggle.hover = toggle.onHover;
			}

			// The GUILayout::Toggle function always returns the current state of the toggle
			// It is possible to determine when the toggle is changed, however it requires messy hacks involving getters and setters
			// Using a GUILayout.Button disguised as a toggle that triggers only when the button is pressed is more pratical here
			if (GUILayout.Button(label, toggle))
			{
				currentPlayer = isCurrentSelection ? null : selectedPlayer;
			}

			// If current player is not null, then the module (or routine) should be enabled
			return currentPlayer != null;
		}

		// It may seem like it would make more sense for the currentPlayers hashset to be a set of PlayerControls and not uint
		// My original implementation did exactly that, however I noticed that after some time has elapsed, the UI would show that the player is no longer jailed, but yet the jail routine would still teleport them
		// I did some debugging and noticed that when this happens, the hashset does not loses any items, and it is possible for the set to include mulitple copies of a player's PlayerControl
		// A hashset, by its nature, should be a deduplicated, but yet the same PlayerControl could appear multiple times in the set
		// HashSet::Contains would return false until the player was added to the set again
		// It seems incredibily peculiar, but it would appear that a player's reference to PlayerControl can change throughout time, and stale references of a player's previous PlayerControls will still exist in memory
		// until cleared out by the garbage collector
		// What is even more weirder is that storing a player's reference to PlayerControl in a variable does not exhibit this behavior,
		// comparing a stored reference of a player's PlayerControl and comparing it with a player's PlayerControl will return true
		// Instead of storing references to PlayerControl in the hashset, we can just store the player's net id
		// We also cannot use the player's owner ID, as on Freeplay all PlayerControls are owned by -2
		public static bool PlayerSpecificToggle(string label, PlayerControl selectedPlayer, ref HashSet<uint> currentPlayers)
		{
			GUIStyle toggle = new GUIStyle(GUI.skin.toggle);
			bool isSelected = selectedPlayer != null && currentPlayers.Contains(selectedPlayer.NetId);

			if (isSelected)
			{
				toggle.normal = toggle.onNormal;
				toggle.active = toggle.onActive;
				toggle.hover = toggle.onHover;
			}

			// The GUILayout::Toggle function always returns the current state of the toggle
			// It is possible to determine when the toggle is changed, however it requires messy hacks involving getters and setters
			// Using a GUILayout.Button disguised as a toggle that triggers only when the button is pressed is more pratical here
			if (GUILayout.Button(label, toggle))
			{
				if (!isSelected)
				{
					currentPlayers.Add(selectedPlayer.NetId);
				}
				else
				{
					currentPlayers.Remove(selectedPlayer.NetId);
				}
			}

			return currentPlayers.Count != 0;
		}

		public static void DrawCrewmateColorBox(Rect rect, NetworkedPlayerInfo player)
		{
			string colorName = Utilities.GetPlayerColor(player);
			GUI.Box(rect, "", Styles.CreateCrewmateColorBox(colorName, colorName != "Fortegreen" ? player.Color : Color.black));
		}

		public class TextInput(string label, string defVal = "")
		{
			private readonly string labelText = label;
			private bool isEditing = false;
			public string contents = defVal;

			public void Render()
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(labelText);
				string buttonText = isEditing ? (contents + "|") : (string.IsNullOrEmpty(contents) ? "[Click here to type]" : contents);
				if (GUILayout.Button(buttonText))
				{
					isEditing = !isEditing;
				}
				GUILayout.EndHorizontal();
				if (isEditing)
				{
					HandleInput();
					GUILayout.Label("<i><color=yellow>Typing active, press [Esc] or click the field again to confirm.</color></i>");
				}
			}

			private void HandleInput()
			{
				Event e = Event.current;
				if (e != null && e.isKey)
				{
					if (e.type == EventType.KeyDown)
					{
						if (e.keyCode == KeyCode.V && e.control)
						{
							contents += GUIUtility.systemCopyBuffer;
						}
						else if (e.keyCode == KeyCode.Escape)
						{
							isEditing = false;
						}
						else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
						{
							contents += "\n";
						}
						else if (e.keyCode == KeyCode.Backspace)
						{
							if (contents.Length > 0)
							{
								contents = contents.Substring(0, contents.Length - 1);
							}
						}
						else if (e.character != '\0' && !char.IsControl(e.character))
						{
							contents += e.character;
						}
					}

					e.Use();
				}
			}
		}

		public static void DrawButtonCell<TKey, TValue>(Dictionary<TKey, TValue> buttons, Action<TValue> action, int columnsPerRow)
		{
			int currentColumn = 0;

			foreach (var (key, value) in buttons)
			{
				if (currentColumn == 0)
				{
					GUILayout.BeginHorizontal();
				}

				if (GUILayout.Button(key.ToString()))
				{
					action(value);
				}

				currentColumn++;
				if (currentColumn == columnsPerRow)
				{
					GUILayout.EndHorizontal();
					currentColumn = 0;
				}
			}

			// If the amount of buttons does not divide into the amount of colums per row then we won't be ending the horizontal layout
			// so we check if we need to end it here
			if (currentColumn != 0)
			{
				GUILayout.EndHorizontal();
			}
		}
	}
}