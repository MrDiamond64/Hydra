using HydraMenu.ui.sections;
using System;
using UnityEngine;
using HydraMenu.features;

namespace HydraMenu.ui
{
	public class MainUI : MonoBehaviour
	{
		// Current window
		public bool visible = false;
		public static float scale = 1.0f;

		private bool isDragging = false;
		private Vector2 mouseDelta = new Vector2();

		public static Vector2 windowPosition = new Vector2(250, 100);
		public static Vector2 WindowSize
		{
			get { return new Vector2(500, 470) * scale; }
		}

		// UI Header
		public static Vector2 HeaderSize
		{
			get { return new Vector2(WindowSize.x, 20 * scale); }
		}

		public static Vector2 HeaderPosition
		{
			get { return new Vector2(windowPosition.x, windowPosition.y); }
		}

		// UI Section Pane
		private readonly ISection[] sections = { new GeneralSection(), new SelfSection(), new TrollSection(), new SabotageSection(), new HostSection(), new RolesSection(), new PlayersSection(), new MovementSection(), new VisualSection(), new ProtectionsSection(), new AnticheatSection(), new SpooferSection(), new KeybindsSection(), new MenuSection() };
		private SearchSection searchSection = new SearchSection();
		private bool searchFocused = false;
		public byte activeTab = 0;

		private bool _initialProtection = true;
		private float _openProtectionTimer = 0f;

		public static Vector2 SectionListSize
		{
			get { return new Vector2(100 * scale, WindowSize.y - HeaderSize.y - (25 * scale)); }
		}

		public static Vector2 SectionListPosition
		{
			get { return new Vector2(windowPosition.x, windowPosition.y + HeaderSize.y + (25 * scale)); }
		}

		public static Vector2 SectionButtonSize
		{
			get { return new Vector2(SectionListSize.x, 25 * scale); }
		}

		// Feature Pane
		public static Vector2 FeaturePaneSize
		{
			get { return new Vector2(WindowSize.x - SectionListSize.x, WindowSize.y - HeaderSize.y - (25 * scale)); }
		}

		public static Vector2 FeaturePanePosition
		{
			get { return new Vector2(SectionListPosition.x + SectionListSize.x, HeaderPosition.y + HeaderSize.y + (25 * scale)); }
		}

		public void Update()
		{
			if(Input.GetKeyDown(KeyCode.Insert) || Input.GetKeyDown(Settings.Config.Binds.ToggleMenu) || (Settings.Config.General.AltF9ToggleEnabled && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F9))) visible = !visible;

			if (visible && _openProtectionTimer <= 0 && _initialProtection)
			{
				_openProtectionTimer = 30f;
				_initialProtection = false;
			}

			if (_openProtectionTimer > 0)
			{
				_openProtectionTimer -= Time.deltaTime;
			}

			if ((_initialProtection || _openProtectionTimer > 0) && !Styles.IsCacheValid())
			{
				Styles.ClearCache();
			}

			// Tool to test the notifications system
			if(Input.GetKeyDown(KeyCode.F6))
			{
				System.Random random = new System.Random();
				Hydra.notifications.Send("Test", $"The quick brown fox jumps over the lazy dog. {random.Next(0, 100)}");
			}

			if(!visible) return;

			// Handle changing the current section through arrow keys
			if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
			{
				int offset = Input.GetKeyDown(KeyCode.UpArrow) ? -1 : 1;

				activeTab = (byte)Math.Clamp(activeTab + offset, 0, sections.Length - 1);
			}

			if(Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown))
			{
				int offset = Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1;

				sections[activeTab].HandleSubsectionMove(offset);
			}

			// Keybind Triggers
			if(Settings.Config.Binds.NoClip != KeyCode.None && Input.GetKeyDown(Settings.Config.Binds.NoClip))
			{
				PlayerControl.LocalPlayer.Collider.enabled = !PlayerControl.LocalPlayer.Collider.enabled;
				Hydra.notifications.Send("NoClip", $"NoClip is now {(PlayerControl.LocalPlayer.Collider.enabled ? "Disabled" : "Enabled")}");
			}

			if(Settings.Config.Binds.KillAll != KeyCode.None && Input.GetKeyDown(Settings.Config.Binds.KillAll))
			{
				// This is a host feature
				// We can't call the private method directly, but can use reflection or move the logic to a feature class.
				// For now trigger the logic if the user is host.
				if(AmongUsClient.Instance.AmHost)
				{
					// Since KillAllPlayers is private, use reflection to call it.
					var method = typeof(HostSection).GetMethod("KillAllPlayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
					method?.Invoke(null, null);
					Hydra.notifications.Send("Kill All", "Murdered all players!");
				}
				else
				{
					Hydra.notifications.Send("Kill All", "You must be host to use this feature.");
				}
			}

			if(Settings.Config.Binds.CloseAllDoors != KeyCode.None && Input.GetKeyDown(Settings.Config.Binds.CloseAllDoors))
			{
				if(AmongUsClient.Instance.AmHost)
				{
					foreach(var door in Sabotage.GetDoors().Values)
					{
						Sabotage.LockDoor(door);
					}
					Hydra.notifications.Send("Close Doors", "Closed all doors!");
				}
				else
				{
					Hydra.notifications.Send("Close Doors", "You must be host to close all doors.");
				}
			}

			HandleBoxMovement();
		}

		public void OnGUI()
		{
			// https://docs.unity3d.com/6000.3/Documentation/Manual/GUIScriptingGuide.html
			if(!visible) return;

			try
			{
				GUI.skin.label.fontSize = (int)(13 * scale);

				// Render UI box
				GUI.Box(new Rect(windowPosition.x, windowPosition.y, WindowSize.x, WindowSize.y), $"{MyPluginInfo.PLUGIN_NAME} - {MyPluginInfo.PLUGIN_VERSION}", Styles.MainBox);

				// Search Bar
				Rect searchRect = new Rect(windowPosition.x + 10, windowPosition.y + 25, 140 * scale, 20 * scale);
				Event e = Event.current;

				if (e.type == EventType.MouseDown && searchRect.Contains(e.mousePosition))
				{
					searchFocused = true;
				}
				else if (e.type == EventType.MouseDown)
				{
					searchFocused = false;
				}

				if (searchFocused && e.type == EventType.KeyDown)
				{
					if (e.keyCode == KeyCode.Backspace && searchSection.SearchQuery.Length > 0)
					{
						searchSection.SearchQuery = searchSection.SearchQuery.Substring(0, searchSection.SearchQuery.Length - 1);
						e.Use();
					}
					else if (e.character != 0)
					{
						searchSection.SearchQuery += (char)e.character;
						e.Use();
					}
				}

				string searchDisplay = "";
				if (!searchFocused && string.IsNullOrEmpty(searchSection.SearchQuery))
				{
					searchDisplay = "Search...";
				}
				else
				{
					searchDisplay = searchSection.SearchQuery;
				}

				if (searchFocused && (Time.frameCount % 60 < 30))
				{
					searchDisplay += "|";
				}

				GUI.Box(searchRect, searchDisplay, searchFocused ? Styles.SearchBoxActive : Styles.SectionBox);

				if (!string.IsNullOrEmpty(searchSection.SearchQuery))
				{
					searchSection.UpdateResults(sections, activeTab, (tab) => { activeTab = tab; });
				}

				if (!string.IsNullOrEmpty(searchSection.SearchQuery))
				{
					activeTab = 255; // Special index for search results
					GUILayout.BeginArea(new Rect(FeaturePanePosition.x, FeaturePanePosition.y, FeaturePaneSize.x, FeaturePaneSize.y));
					searchSection.scrollVector = GUILayout.BeginScrollView(searchSection.scrollVector);
					searchSection.Render();
					GUILayout.EndScrollView();
					GUILayout.EndArea();
				}

				for(byte i = 0; i < sections.Length; i++)
				{
					ISection section = sections[i];

					// Add the tab to the left-pane
					RenderTab(i, section);

					if(i == activeTab)
					{
						GUILayout.BeginArea(new Rect(FeaturePanePosition.x, FeaturePanePosition.y, FeaturePaneSize.x, FeaturePaneSize.y));
						section.scrollVector = GUILayout.BeginScrollView(section.scrollVector);

						section.Render();

						GUILayout.EndScrollView();
						GUILayout.EndArea();
					}
				}

				// Render Search Results if active
				if (!string.IsNullOrEmpty(searchSection.SearchQuery))
				{
					activeTab = 255; // Special index for search results
					GUILayout.BeginArea(new Rect(FeaturePanePosition.x, FeaturePanePosition.y, FeaturePaneSize.x, FeaturePaneSize.y));
					searchSection.scrollVector = GUILayout.BeginScrollView(searchSection.scrollVector);
					searchSection.Render();
					GUILayout.EndScrollView();
					GUILayout.EndArea();
				}
			}
			finally
			{
				// This ensures that if an ExitGUIException occurs it don't leave the GUI state unbalanced
			}
		}

		private void HandleBoxMovement()
		{
			// https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Event.html
			Event currentEvent = Event.current;
			Vector2 mousePos = currentEvent.mousePosition;

			switch(currentEvent.type)
			{
				// I tried using currentEvent.delta to get the delta between the last mouse position and the current one,
				// however I noticed it would 'skip' quite frequently resulting in the window box not properly lining up where it should actually be dragged
				case EventType.MouseDown:
					if(!IsInBox(mousePos)) break;

					isDragging = true;
					mouseDelta = currentEvent.mousePosition - windowPosition;
					break;

				case EventType.MouseDrag:
					if(!isDragging) break;

					windowPosition.x = mousePos.x - mouseDelta.x;
					windowPosition.y = mousePos.y - mouseDelta.y;
					break;

				case EventType.MouseUp:
					isDragging = false;
					break;
			}
		}

		private bool IsInBox(Vector2 mousePos)
		{
			return
				mousePos.x >= windowPosition.x &&
				mousePos.x <= (windowPosition.x + WindowSize.x) &&
				mousePos.y >= windowPosition.y &&
				mousePos.y <= (windowPosition.y + WindowSize.y);
		}

		private void RenderTab(byte position, ISection section)
		{
			Rect rect = new Rect(
				SectionListPosition.x,
				SectionListPosition.y + (position * SectionButtonSize.y),
				SectionButtonSize.x,
				SectionButtonSize.y
			);

			GUIStyle style = activeTab == position ? Styles.SectionBoxActive : Styles.SectionBox;
			if(GUI.Button(rect, section.name, style))
			{
				activeTab = position;
				searchSection.SearchQuery = string.Empty;
			}
		}
	}
}