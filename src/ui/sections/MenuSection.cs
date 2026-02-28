using System;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class MenuSection : ISection
	{
		public MenuSection()
		{
			name = "Menu";
		}

		public override void Render()
		{
			GUILayout.Label($"Primary Color: {Styles.primaryColor}");
			Styles.primaryColor = (Styles.UIColors)GUILayout.HorizontalSlider((float)Styles.primaryColor, 0, Styles.ColorValues.Count - 1);

			GUILayout.Label($"Menu Opacity: {Styles.menuOpacity * 100:F0}%");
			Styles.menuOpacity = (float)Math.Round(GUILayout.HorizontalSlider(Styles.menuOpacity, 0, 1), 4);
		}
	}
}