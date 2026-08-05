using UnityEngine;
using HydraMenu.features;

namespace HydraMenu.ui.sections
{
	internal class LogsSection : ISection
	{
		public LogsSection() : base("Logs") { }

		private Vector2 scrollPosition;
		private Vector2 playerScrollPosition;

		public override void Render()
		{
			GUILayout.BeginHorizontal();
			
			// Columns for checkboxes
			GUILayout.BeginVertical(GUILayout.Width(120));
			GameLogger.ShowRpc = GUILayout.Toggle(GameLogger.ShowRpc, "Show RPCs");
			GameLogger.ShowWarning = GUILayout.Toggle(GameLogger.ShowWarning, "Show Warnings");
			GUILayout.EndVertical();

			GUILayout.BeginVertical(GUILayout.Width(120));
			GameLogger.ShowSystem = GUILayout.Toggle(GameLogger.ShowSystem, "Show System");
			GameLogger.ShowChat = GUILayout.Toggle(GameLogger.ShowChat, "Show Chat");
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			if(GUILayout.Button("Clear Logs"))
			{
				GameLogger.Clear();
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			GUILayout.Space(5);

			// Player Filters (Scrollable horizontally)
			GUILayout.Label("Filter by Player:");
			GUILayout.BeginHorizontal();
			
			// "All" button
			bool isAllSelected = string.IsNullOrEmpty(GameLogger.PlayerFilter);
			if(GUILayout.Toggle(isAllSelected, "All Players", "Button"))
			{
				GameLogger.PlayerFilter = "";
			}

			// Scrollable list of players
			playerScrollPosition = GUILayout.BeginScrollView(playerScrollPosition, GUILayout.Height(35));
			GUILayout.BeginHorizontal();
			if(PlayerControl.AllPlayerControls != null)
			{
				foreach(PlayerControl p in PlayerControl.AllPlayerControls)
				{
					if(p == null || p.Data == null) continue;
					string name = p.Data.PlayerName;
					bool isSelected = GameLogger.PlayerFilter == name;
					if(GUILayout.Toggle(isSelected, name, "Button"))
					{
						GameLogger.PlayerFilter = name;
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndScrollView();
			GUILayout.EndHorizontal();

			GUILayout.Space(5);

			// Scrollable logs viewport
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(280));
			
			var logs = GameLogger.GetFilteredEntries();
			if(logs.Count == 0)
			{
				GUILayout.Label("No logs found.");
			}
			else
			{
				// Show oldest first (chronological order)
				for(int i = 0; i < logs.Count; i++)
				{
					GUILayout.Label(logs[i].GetFormattedString());
				}
			}

			GUILayout.EndScrollView();
		}
	}
}
