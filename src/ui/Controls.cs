using AmongUs.GameOptions;
using HydraMenu.assets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui
{
	internal class Controls
	{
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

		public static T HorizontalArraySlider<T>(T[] array, T currentValue)
		{
			int index = Array.IndexOf(array, currentValue);

			int newValue = (int)GUILayout.HorizontalSlider(index, 0, array.Length - 1);

			return array[newValue];
		}

		public static T HorizontalListSlider<T>(List<T> list, T currentValue)
		{
			int index = list.IndexOf(currentValue);

			int newValue = (int)GUILayout.HorizontalSlider(index, 0, list.Count - 1);

			return list[newValue];
		}

		public static TKey HorizontalListSlider<TKey, TValue>(SortedList<TKey, TValue> list, TKey currentValue)
		{
			int index = list.IndexOfKey(currentValue);

			int newIndex = (int)GUILayout.HorizontalSlider(index, 0, list.Count - 1);

			return list.Keys[newIndex];
		}

		public static RoleTypes HorizontalRoleSlider(RoleTypes currentRole)
		{
			return HorizontalListSlider(GameAssets.RolesList, currentRole);
		}

		public static PlayerColors HorizontalColorSlider(PlayerColors currentColor)
		{
			return (PlayerColors)GUILayout.HorizontalSlider((int)currentColor, 0, Palette.ColorNames.Length);
		}

		public static int HorizontalVentSlider(SortedList<int, string> vents, int currentVent)
		{
			// Reset current vent if it is out ouf bounds
			// SortedList::Contains is a O(log n) operation
			// Since all vent IDs are (currently) contiguous, we can just check the id of the first key and the total number of vents to check if an item exists with O(1) time complexity
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
			string colorName = Utilities.GetPlayerColor(player);
			GUI.Box(rect, "", Styles.CreateCrewmateColorBox(colorName, colorName != "Fortegreen" ? player.Color : Color.black));
		}

		public static void DrawButtonCell<TKey, TValue>(Dictionary<TKey, TValue> buttons, Action<TValue> action, int columnsPerRow)
		{
			int currentColumn = 0;

			foreach((TKey key, TValue value) in buttons)
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