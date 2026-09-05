using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LunarMenu.ui
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
			RoleTypes.Scientist,
			RoleTypes.Engineer,
			RoleTypes.Noisemaker,
			RoleTypes.Tracker,
			RoleTypes.Detective,
			RoleTypes.Judge,
			RoleTypes.Impostor,
			RoleTypes.Shapeshifter,
			RoleTypes.Phantom,
			RoleTypes.Viper,
			RoleTypes.CrewmateGhost,
			RoleTypes.GuardianAngel,
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

		public static int HorizontalVentSlider(SortedDictionary<int, string> vents, int currentVent)
		{
			// Reset current vent if it is out ouf bounds
			int firstKey = vents.Keys.First();
			if(currentVent < firstKey || currentVent >= vents.Count) currentVent = firstKey;

			return (int)GUILayout.HorizontalSlider(currentVent, firstKey, vents.Count - 1);
		}

		public static bool PlayerSpecificToggle(string label, PlayerControl selectedPlayer, ref PlayerControl currentPlayer)
		{
			GUIStyle toggle = new GUIStyle(GUI.skin.toggle);
			// We do not want the toggle to appear as enabled if selectedPlayer and currentPlayer are both null
			bool isCurrentSelection = selectedPlayer != null && selectedPlayer == currentPlayer;

			if(isCurrentSelection)
			{
				toggle.normal = toggle.onNormal;
				toggle.active = toggle.onActive;
				toggle.hover = toggle.onHover;
			}

			// The GUILayout::Toggle function always returns the current state of the toggle
			// It is possible to determine when the toggle is changed, however it requires messy hacks involving getters and setters
			// Using a GUILayout.Button disguised as a toggle that triggers only when the button is pressed is more practical here
			if(GUILayout.Button(label, toggle))
			{
				currentPlayer = isCurrentSelection ? null : selectedPlayer;
			}

			// If current player is not null, then the module (or routine) should be enabled
			return currentPlayer != null;
		}

		public static bool PlayerSpecificToggle(string label, PlayerControl selectedPlayer, HashSet<int> currentPlayers)
		{
			GUIStyle toggle = new GUIStyle(GUI.skin.toggle);
			bool isSelected = selectedPlayer != null && currentPlayers.Contains(selectedPlayer.GetHashCode());

			if(isSelected)
			{
				toggle.normal = toggle.onNormal;
				toggle.active = toggle.onActive;
				toggle.hover = toggle.onHover;
			}

			// The GUILayout::Toggle function always returns the current state of the toggle
			// It is possible to determine when the toggle is changed, however it requires messy hacks involving getters and setters
			// Using a GUILayout.Button disguised as a toggle that triggers only when the button is pressed is more practical here
			if(GUILayout.Button(label, toggle) && selectedPlayer != null)
			{
				if(!isSelected)
				{
					currentPlayers.Add(selectedPlayer.GetHashCode());
				}
				else
				{
					currentPlayers.Remove(selectedPlayer.GetHashCode());
				}
			}

			return currentPlayers.Count != 0;
		}

		public static bool GlobalPlayerSpecificToggle(string label, HashSet<int> currentPlayers)
		{
			GUIStyle toggle = new GUIStyle(GUI.skin.toggle);
			bool enabled = currentPlayers.Count != 0;

			if(enabled)
			{
				toggle.normal = toggle.onNormal;
				toggle.active = toggle.onActive;
				toggle.hover = toggle.onHover;
			}

			if(GUILayout.Button(label, toggle))
			{
				if(!enabled)
				{
					currentPlayers.Add(int.MaxValue);
				}
				else
				{
					currentPlayers.Clear();
				}
			}

			return currentPlayers.Count != 0;
		}

		public static void DrawCrewmateColorBox(Rect rect, NetworkedPlayerInfo player)
		{
			string colorName = Utilities.GetPlayerColorString(player);
			GUI.Box(rect, "", Styles.CreateCrewmateColorBox(colorName, colorName != "Fortegreen" ? player.Color : Color.black));
		}

		public static void DrawButtonCell<TKey, TValue>(Dictionary<TKey, TValue> buttons, Action<TValue> action, int columnsPerRow)
		{
			int currentColumn = 0;

			foreach(var (key, value) in buttons)
			{
				if(currentColumn == 0)
				{
					GUILayout.BeginHorizontal();
				}

				if(GUILayout.Button(key.ToString()))
				{
					action(value);
				}

				currentColumn++;
				if(currentColumn == columnsPerRow)
				{
					GUILayout.EndHorizontal();
					currentColumn = 0;
				}
			}

			// If the amount of buttons does not divide into the amount of columns per row then we won't be ending the horizontal layout
			// so we check if we need to end it here
			if(currentColumn != 0)
			{
				GUILayout.EndHorizontal();
			}
		}
	}
}