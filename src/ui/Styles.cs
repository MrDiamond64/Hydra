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

		public static float menuOpacity { get => HydraMenu.ui.Settings.Config.General.MenuOpacity; set => HydraMenu.ui.Settings.Config.General.MenuOpacity = value; }
		public static UIColors primaryColor { get => (UIColors)HydraMenu.ui.Settings.Config.General.PrimaryColorIndex; set => HydraMenu.ui.Settings.Config.General.PrimaryColorIndex = (int)value; }

		private static Dictionary<string, Texture2D> CachedTextures = new Dictionary<string, Texture2D>();
		private static Dictionary<string, GUIStyle> CachedStyles = new Dictionary<string, GUIStyle>();

		public static void ClearCache()
		{
			if (UnityEngine.Resources.UnloadUnusedAssets == null) return;

			foreach(Texture2D texture in CachedTextures.Values)
			{
				if (texture != null)
				{
					UnityEngine.Object.Destroy(texture);
				}
			}
			CachedTextures.Clear();
			CachedStyles.Clear();
		}

		public static bool IsCacheValid()
		{
			foreach(var texture in CachedTextures.Values)
			{
				if(texture == null) return false;
			}
			return true;
		}

		public static GUIStyle MainBox
		{
			get
			{
				if (CachedStyles.TryGetValue("MainBox", out var style)) return style;

				style = new GUIStyle();

				Texture2D background = CreateColoredTexture("MainBox", ColorValues[UIColors.Carbon], menuOpacity);
				style.normal.background = background;

				style.normal.textColor = Color.white;
				style.alignment = TextAnchor.UpperCenter;
				style.padding.top = 5;
				// The product of the font size and the UI scale will result in a float value with decimal values
				// which would get truncated if we cast this into an int
				// however this is rather insignificant as the font size would be at most one unit off
				style.fontSize = (int)(13 * MainUI.scale);

				CachedStyles["MainBox"] = style;
				return style;
			}
		}

		public static GUIStyle SectionBox
		{
			get
			{
				if (CachedStyles.TryGetValue("SectionBox", out var style)) return style;

				style = new GUIStyle();

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.padding.bottom = 1;
				style.padding.left = (int)(8 * MainUI.scale);
				style.fontSize = (int)(14 * MainUI.scale);

				CachedStyles["SectionBox"] = style;
				return style;
			}
		}

		public static GUIStyle SectionBoxActive
		{
			get
			{
				if (CachedStyles.TryGetValue("SectionBoxActive", out var style)) return style;

				style = new GUIStyle();

				Texture2D background = CreateColoredTexture("SectionBoxActive", ColorValues[primaryColor]);
				style.normal.background = background;

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.padding.bottom = 1;
				style.padding.left = (int)(13 * MainUI.scale);
				style.fontSize = (int)(MainUI.scale * 14);

				CachedStyles["SectionBoxActive"] = style;
				return style;
			}
		}

		public static GUIStyle SearchBoxActive
		{
			get
			{
				if (CachedStyles.TryGetValue("SearchBoxActive", out var style)) return style;

				style = new GUIStyle();

				Texture2D background = CreateColoredTexture("SearchBoxActive", ColorValues[primaryColor]);
				style.normal.background = background;

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.padding.bottom = 1;
				style.padding.left = (int)(8 * MainUI.scale);
				style.fontSize = (int)(14 * MainUI.scale);

				CachedStyles["SearchBoxActive"] = style;
				return style;
			}
		}

		public static GUIStyle PlayerBox
		{
			get
			{
				if (CachedStyles.TryGetValue("PlayerBox", out var style)) return style;

				style = new GUIStyle();

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.clipping = TextClipping.Clip;
				style.padding.left = (int)(10 * MainUI.scale);
				style.richText = true;
				style.fontSize = (int)(13 * MainUI.scale);

				CachedStyles["PlayerBox"] = style;
				return style;
			}
		}

		public static GUIStyle PlayerBoxActive
		{
			get
			{
				if (CachedStyles.TryGetValue("PlayerBoxActive", out var style)) return style;

				style = new GUIStyle();

				Texture2D background = CreateColoredTexture("SectionBoxActive", ColorValues[primaryColor]);
				style.normal.background = background;

				style.normal.textColor = ColorValues[UIColors.White];
				style.alignment = TextAnchor.MiddleLeft;
				style.clipping = TextClipping.Clip;
				style.padding.left = (int)(10 * MainUI.scale);
				style.richText = true;
				style.fontSize = (int)(13 * MainUI.scale);

				CachedStyles["PlayerBoxActive"] = style;
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
			CachedTextures.TryGetValue(textureName, out Texture2D background);
			if(background != null) return background;

			Hydra.Log.LogInfo($"Cache lookup for texture {textureName} returned a miss, creating the required texture...");

			background = new Texture2D(1, 1);
			Color finalColor = color;
			finalColor.a = Mathf.Clamp01(opacity);
			background.SetPixel(0, 0, finalColor);
			background.Apply();

			CachedTextures[textureName] = background;
			return background;
		}
	}

	[HarmonyLib.HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.UnloadUnusedAssets))]
	public static class Resources_UnloadUnusedAssets_Patch
	{
		public static void Postfix()
		{
			Styles.ClearCache();
		}
	}
}