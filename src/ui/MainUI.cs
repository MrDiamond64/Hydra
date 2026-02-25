using HydraMenu.ui.sections;
using System;
using UnityEngine;

namespace HydraMenu.ui
{
    public class MainUI : MonoBehaviour
    {
        // Current window
        public bool visible = false;
        public static Vector2 windowPosition = new Vector2(250, 100);
        public static Vector2 windowSize = new Vector2(500, 475);

        private bool isDragging = false;
        private Vector2 mouseDelta = new Vector2();

        // UI Header
        public static Vector2 HeaderSize
        {
            get { return new Vector2(windowSize.x, 25); }
        }

        public static Vector2 HeaderPosition
        {
            get { return new Vector2(windowPosition.x, windowPosition.y); }
        }

        // UI Section Pane
        private readonly ISection[] sections = { new GeneralSection(), new SelfSection(), new TrollSection(), new SabotageSection(), new HostSection(), new RolesSection(), new PlayersSection(), new MovementSection(), new VisualSection(), new ProtectionsSection(), new AnticheatSection(), new SpooferSection() };
        public byte activeTab = 0;

        public static Vector2 SectionListSize
        {
            get { return new Vector2(110, windowSize.y - HeaderSize.y); }
        }

        public static Vector2 SectionListPosition
        {
            get { return new Vector2(windowPosition.x, windowPosition.y + HeaderSize.y ); }
        }

        public static Vector2 SectionButtonSizes
        {
            get { return new Vector2(SectionListSize.x, 30); }
        }

        // Feature Pane
        public static Vector2 FeaturePaneSize
        {
            get { return new Vector2(windowSize.x - SectionListSize.x, windowSize.y - HeaderSize.y); }
        }

        public static Vector2 FeaturePanePosition
        {
            get { return new Vector2(SectionListPosition.x + SectionListSize.x, HeaderPosition.y + HeaderSize.y); }
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.Insert)) visible = !visible;

            // Tool to test the notifications system
            if(Input.GetKeyDown(KeyCode.F6))
            {
                System.Random random = new System.Random();
                Hydra.notifications.Send("Test", $"The quick brown fox jumped over the lazy dog. The quick brown fox jumped over the lazy dog. The quick brown fox jumped over the lazy dog. {random.Next(0, 100)}");
            }

            if(!visible) return;

			// Allow changing the selected section by using the up and down arrow keys
			if(Input.GetKeyDown(KeyCode.UpArrow))
			{
				activeTab = (byte)Math.Max(activeTab - 1, 0);
			}
			else if(Input.GetKeyDown(KeyCode.DownArrow))
			{
				activeTab = (byte)Math.Min(activeTab + 1, sections.Length - 1);
			}

			HandleBoxMovement();
        }

        public void OnGUI()
        {
            // https://docs.unity3d.com/6000.3/Documentation/Manual/GUIScriptingGuide.html
            if(!visible) return;

            // Render UI header
            GUI.Box(new Rect(windowPosition.x, windowPosition.y, HeaderSize.x, HeaderSize.y), $"{MyPluginInfo.PLUGIN_NAME} - {MyPluginInfo.PLUGIN_VERSION}");

            // Render Features Pane
            GUI.Box(new Rect(FeaturePanePosition.x, FeaturePanePosition.y, FeaturePaneSize.x, FeaturePaneSize.y), "");

            // Render Section List
            GUI.Box(new Rect(SectionListPosition.x, SectionListPosition.y, SectionListSize.x, SectionListSize.y), "");

            for(byte i = 0; i < sections.Length; i++)
            {
                ISection section = sections[i];

                // If we are currently on this section then render it
                if(i == activeTab)
                {
                    GUILayout.BeginArea(new Rect(FeaturePanePosition.x, FeaturePanePosition.y, FeaturePaneSize.x, FeaturePaneSize.y));

                    // Add a little buffer between the end of the section pane and the start of the feature pane to prevent text from overlapping
                    GUILayout.BeginVertical();
                    GUILayout.Space(5);
                    GUILayout.EndVertical();

                    section.Render();
                    GUILayout.EndArea();
                }

                // Add the tab to the left-pane
                RenderTabs(i, section);
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
                mousePos.x <= (windowPosition.x + windowSize.x) &&
                mousePos.y >= windowPosition.y &&
                mousePos.y <= (windowPosition.y + windowSize.y);
        }

        private void RenderTabs(byte position, ISection section)
        {
            Rect rect = new Rect(
                SectionListPosition.x,
                SectionListPosition.y + (position * SectionButtonSizes.y),
                SectionButtonSizes.x,
                SectionButtonSizes.y
            );

            if(GUI.Button(rect, section.name)) {
                activeTab = position;
            }
        }
    }
}