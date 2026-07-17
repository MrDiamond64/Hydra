using HydraMenu.ui;
using UnityEngine;
using HydraMenu.features;

namespace HydraMenu.ui.sections
{
    internal class KeybindsSection : ISection
    {
public KeybindsSection() : base("Keybinds") 
		{
			AddFeature("Toggle Menu", () => { });
			AddFeature("NoClip", () => { });
			AddFeature("Kill All", () => { });
			AddFeature("Close All Doors", () => { });
		}

        private KeyCode? listeningKey = null;
        private string listeningAction = null;

        public override void Render()
        {
            if (listeningKey != null)
            {
                GUILayout.Label($"Listening for {listeningAction}... Press any key.", Styles.MainBox);
                
                // Check for any key press
                foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(k))
                    {
                        UpdateKeybind(k);
                        break;
                    }
                }
                return;
            }

            RenderBind("Toggle Menu", Settings.Config.Binds.ToggleMenu);
            RenderBind("NoClip", Settings.Config.Binds.NoClip);
            RenderBind("Kill All", Settings.Config.Binds.KillAll);
            RenderBind("Close All Doors", Settings.Config.Binds.CloseAllDoors);
        }

        private void RenderBind(string label, KeyCode bind)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            string bindText = bind == KeyCode.None ? "None" : bind.ToString();
            if (GUILayout.Button(bindText, GUILayout.Width(100)))
            {
                listeningKey = KeyCode.None;
                listeningAction = label;
            }
            GUILayout.EndHorizontal();
        }

        private void UpdateKeybind(KeyCode k)
        {
            switch (listeningAction)
            {
                case "Toggle Menu": Settings.Config.Binds.ToggleMenu = k; break;
                case "NoClip": Settings.Config.Binds.NoClip = k; break;
                case "Kill All": Settings.Config.Binds.KillAll = k; break;
                case "Close All Doors": Settings.Config.Binds.CloseAllDoors = k; break;
            }
            listeningKey = null;
            listeningAction = null;
            Settings.Save();
        }

        public override void HandleSubsectionMove(int offset) { }
    }
}
