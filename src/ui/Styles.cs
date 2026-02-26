using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui
{
	internal class Styles
	{
		public enum UIColors
		{
			Azure,
			Carbon,
			Jasper,
			Pesto,
			White,
			Violet
		}

		public static Dictionary<UIColors, Color> ColorValues = new Dictionary<UIColors, Color>()
		{
			{ UIColors.Azure, new Color(0.0f, 0.50f, 1f) }, // ##007FFF
			{ UIColors.Carbon, new Color(0.07f, 0.07f, 0.07f) }, // #222222
			{ UIColors.Pesto, new Color(0.05f, 0.5f, 0.13f) }, // #119922
			{ UIColors.Jasper, new Color(0.89f, 0.64f, 0.42f) }, // #E0934C
			{ UIColors.White, new Color(0.95f, 0.95f, 0.97f) }, // #F0EFDF
			{ UIColors.Violet, new Color(0.5f, 0f, 1f) } // #7F00FF
		};

		public static float menuOpacity = 0.85f;
		public static UIColors primaryColor = UIColors.Azure;

		public static GUIStyle MainBox
		{
			get
			{
				GUIStyle style = new GUIStyle();

				Texture2D background = CreateColoredTexture(UIColors.Carbon, menuOpacity);
				style.normal.background = background;

				style.normal.textColor = Color.white;
				style.alignment = TextAnchor.MiddleCenter;
				style.padding.top = 2;

				return style;
			}
		}

		public static GUIStyle SectionBox
		{
			get
			{
				GUIStyle style = new GUIStyle();

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.padding.bottom = 1;
				style.padding.left = 8;
				style.fontSize = 14;

				return style;
			}
		}

		public static GUIStyle SectionBoxActive
		{
			get
			{
				GUIStyle style = new GUIStyle();

				Texture2D background = CreateColoredTexture(primaryColor);
				style.normal.background = background;

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.padding.bottom = 1;
				style.padding.left = 13;
				style.fontSize = 14;

				return style;
			}
		}

		private static Texture2D CreateColoredTexture(UIColors color, float opacity = 1.0f)
		{
			Texture2D background = new Texture2D(1, 1);
			background.SetPixel(0, 0, ColorValues[color].SetAlpha(opacity));
			background.Apply();

			return background;
		}
	}
}