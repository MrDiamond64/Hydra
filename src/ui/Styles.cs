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
			Cardinal,
			Pesto,
			Pumpkin,
			White,
			Violet
		}

		public static Dictionary<UIColors, Color> ColorValues = new Dictionary<UIColors, Color>()
		{
			{ UIColors.Azure, new Color(0.0f, 0.50f, 1f) }, // #007FFF
			{ UIColors.Carbon, new Color(0.07f, 0.07f, 0.07f) }, // #222222
			{ UIColors.Cardinal, new Color(0.77f, 0.12f, 0.23f) }, // #C41E3A
			{ UIColors.Pesto, new Color(0.05f, 0.5f, 0.13f) }, // #119922
			{ UIColors.Pumpkin, new Color(1.0f, 0.18f, 0.04f) }, // #FF7518
			{ UIColors.White, new Color(0.95f, 0.95f, 0.97f) }, // #F0EFDF
			{ UIColors.Violet, new Color(0.5f, 0f, 1f) } // #7F00FF
		};

		public static float menuOpacity = 0.85f;
		public static UIColors primaryColor = UIColors.Azure;

		private static Dictionary<string, Texture2D> CachedTextures = new Dictionary<string, Texture2D>();

		public static GUIStyle MainBox
		{
			get
			{
				GUIStyle style = new GUIStyle();

				Texture2D background = CreateColoredTexture("MainBox", ColorValues[UIColors.Carbon], menuOpacity);
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

				Texture2D background = CreateColoredTexture("SectionBoxActive", ColorValues[primaryColor]);
				style.normal.background = background;

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.padding.bottom = 1;
				style.padding.left = 13;
				style.fontSize = 14;

				return style;
			}
		}

		public static GUIStyle PlayerBox
		{
			get
			{
				GUIStyle style = new GUIStyle();

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.padding.left = 10;
				style.richText = true;

				return style;
			}
		}

		public static GUIStyle PlayerBoxActive
		{
			get
			{
				GUIStyle style = new GUIStyle();

				Texture2D background = CreateColoredTexture("SectionBoxActive", ColorValues[primaryColor]);
				style.normal.background = background;

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.padding.left = 10;
				style.richText = true;

				return style;
			}
		}

		public static GUIStyle CreateCrewmateColorBox(string colorName, Color color)
		{
			GUIStyle style = new GUIStyle();

			Texture2D background = CreateColoredTexture(colorName, color);
			style.normal.background = background;

			return style;
		}

		private static Texture2D CreateColoredTexture(string textureName, Color color, float opacity = 1.0f)
		{
			Texture2D background;
			// Check if the cached background we have is null, as the garbage collector could delete our cached Texture2D and we would be returning a null Texture
			if(CachedTextures.TryGetValue(textureName, out background) && background != null)
			{
				return background;
			}

			Hydra.Log.LogInfo($"Cache lookup for texture {textureName} returned a miss, creating the required texture...");

			background = new Texture2D(1, 1);
			background.SetPixel(0, 0, color.SetAlpha(opacity));
			background.Apply();

			CachedTextures[textureName] = background;
			return background;
		}

		public static void ClearCache()
		{
			foreach(Texture2D texture in CachedTextures.Values)
			{
				Texture2D.Destroy(texture);
			}
			CachedTextures.Clear();
		}
	}
}