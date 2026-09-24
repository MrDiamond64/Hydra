using HydraMenu.ui.sections;
using System;
using UnityEngine;

namespace HydraMenu.ui
{
	public class MainUI : MonoBehaviour
	{
		// Current window
		public KeyCode menuKey = KeyCode.Insert;
		public bool visible = false;
		public static float scale = 1.0f;

		// When true, the next key the user presses is captured and set as the new menu key (see Update).
		public bool isRebinding = false;

		// Optional self-only FPS counter drawn in the top-left corner regardless of whether the menu is open.
		public static bool showFps = false;
		private float smoothedFps = 0.0f;

		private bool isDragging = false;
		private Vector2 mouseDelta = new Vector2();

		public static readonly Vector2 DefaultWindowPosition = new Vector2(250, 100);
		public static Vector2 windowPosition = DefaultWindowPosition;
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
		private readonly Section[] sections = { new GeneralSection(), new SelfSection(), new TrollSection(), new SabotageSection(), new HostSection(), new RolesSection(), new PlayersSection(), new MovementSection(), new VisualSection(), new ProtectionsSection(), new AnticheatSection(), new SpooferSection(), new MenuSection() };
		public byte activeTab = 0;

		public static Vector2 SectionListSize
		{
			get { return new Vector2(100 * scale, WindowSize.y - HeaderSize.y); }
		}

		public static Vector2 SectionListPosition
		{
			get { return new Vector2(windowPosition.x, windowPosition.y + HeaderSize.y); }
		}

		public static Vector2 SectionButtonSize
		{
			get { return new Vector2(SectionListSize.x, 25 * scale); }
		}

		// Feature Pane
		public static Vector2 FeaturePaneSize
		{
			get { return new Vector2(WindowSize.x - SectionListSize.x, WindowSize.y - HeaderSize.y); }
		}

		public static Vector2 FeaturePanePosition
		{
			get { return new Vector2(SectionListPosition.x + SectionListSize.x, HeaderPosition.y + HeaderSize.y); }
		}

		public void Update()
		{
			// Smooth the framerate reading so the counter does not jump around every frame.
			if(Time.deltaTime > 0.0f)
			{
				smoothedFps = Mathf.Lerp(smoothedFps, 1.0f / Time.deltaTime, 0.1f);
			}

			Event currentEvent = Event.current;
			if(currentEvent == null) return;

			// While rebinding, swallow the next key press and use it as the new menu key instead of toggling the menu.
			if(isRebinding)
			{
				if(currentEvent.type == EventType.KeyDown && currentEvent.keyCode != KeyCode.None)
				{
					// Escape cancels the rebind and leaves the current key untouched.
					if(currentEvent.keyCode != KeyCode.Escape)
					{
						menuKey = currentEvent.keyCode;
						Hydra.notifications.Send("Menu", $"Menu key rebound to {menuKey}.");
					}

					isRebinding = false;
				}

				return;
			}

			// Input::GetKeyDown(KeyCodes.Insert) returns true if you press the dedicated Insert key, but not the numpad Insert key
			// so we have to rely on Event.current here
			if(currentEvent.type == EventType.KeyDown && currentEvent.keyCode == menuKey)
			{
				visible = !visible;
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

			HandleBoxMovement();
		}

		public void OnGUI()
		{
			// https://docs.unity3d.com/6000.3/Documentation/Manual/GUIScriptingGuide.html

			// The FPS counter is drawn independently of the menu so it can be left on as a HUD.
			if(showFps)
			{
				DrawFpsCounter();
			}

			if(!visible) return;

			GUI.skin.label.fontSize = (int)(13 * scale);

			// Render UI box. The title includes the toggle key so users always know how to reopen the menu.
			GUI.Box(new Rect(windowPosition.x, windowPosition.y, WindowSize.x, WindowSize.y), $"{MyPluginInfo.PLUGIN_NAME} - {MyPluginInfo.PLUGIN_VERSION}  [{menuKey}]", Styles.MainBox);

			for(byte i = 0; i < sections.Length; i++)
			{
				Section section = sections[i];

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

		private void DrawFpsCounter()
		{
			Rect rect = new Rect(10, 10, 120 * scale, HeaderSize.y);
			GUI.Box(rect, $"FPS: {Mathf.RoundToInt(smoothedFps)}", Styles.MainBox);
		}

		private void RenderTab(byte position, Section section)
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
			}
		}

		public class MainUIConfig
		{
			public KeyCode MenuKey { get; set; }
			public Styles.UIColors PrimaryColor { get; set; }
			public float MenuOpacity { get; set; }
			public float UiScale { get; set; }
			public bool DisableNotifications { get; set; }
			public bool ShowFps { get; set; }
		}

		public MainUIConfig GetConfigData()
		{
			return new MainUIConfig
			{
				MenuKey = menuKey,
				PrimaryColor = Styles.primaryColor,
				MenuOpacity = Styles.menuOpacity,
				UiScale = scale,
				DisableNotifications = Hydra.notifications.disableNotifications,
				ShowFps = showFps
			};
		}

		public void LoadConfigData(MainUIConfig configData)
		{
			if(configData == null) return;

			if(configData.MenuKey != KeyCode.None)
			{
				Hydra.mainUI.menuKey = configData.MenuKey;
			}

			Styles.primaryColor = (Styles.UIColors)Math.Clamp((int)configData.PrimaryColor, 0, Styles.ColorValues.Count - 1);
			Styles.menuOpacity = Mathf.Clamp(configData.MenuOpacity, 0.0f, 1.0f);
			scale = Mathf.Clamp(configData.UiScale, 0.5f, 2.0f);
			Hydra.notifications.disableNotifications = configData.DisableNotifications;
			showFps = configData.ShowFps;
		}
	}
}