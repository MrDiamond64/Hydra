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
				style.alignment = TextAnchor.UpperCenter;
				style.padding.top = 5;
				// The product of the font size and the UI scale will result in a float value with decimal values
				// which would get truncated if we cast this into an int
				// however this is rather insignificant as the font size would be at most one unit off
				style.fontSize = (int)(13 * MainUI.scale);

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
				style.padding.left = (int)(8 * MainUI.scale);
				style.fontSize = (int)(14 * MainUI.scale);

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
				style.padding.left = (int)(13 * MainUI.scale);
				style.fontSize = (int)(MainUI.scale * 14);

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
				style.clipping = TextClipping.Clip;
				style.padding.left = (int)(10 * MainUI.scale);
				style.richText = true;
				style.fontSize = (int)(13 * MainUI.scale);

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
				style.clipping = TextClipping.Clip;
				style.padding.left = (int)(10 * MainUI.scale);
				style.richText = true;
				style.fontSize = (int)(13 * MainUI.scale);

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
			background.SetPixel(0, 0, color.SetAlpha(opacity));
			background.Apply();

			CachedTextures[textureName] = background;
			return background;
		}

		public static Texture2D GetJoystickBaseTexture(int size = 128)
		{
			string key = $"JoystickBase_{primaryColor}_{size}";
			if(CachedTextures.TryGetValue(key, out Texture2D tex) && tex != null) return tex;

			tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
			float center = size / 2f;
			float outerRadius = center - 2f;
			float ringWidth = 3f;
			Color baseColor = new Color(0.08f, 0.08f, 0.12f, 0.70f);
			Color ringColor = ColorValues.ContainsKey(primaryColor) ? ColorValues[primaryColor] : new Color(0f, 0.5f, 1f);
			ringColor.a = 0.85f;

			for(int y = 0; y < size; y++)
			{
				for(int x = 0; x < size; x++)
				{
					float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
					if(dist > outerRadius)
					{
						tex.SetPixel(x, y, Color.clear);
					}
					else if(dist >= outerRadius - ringWidth)
					{
						float edgeAlpha = Mathf.Clamp01(outerRadius - dist);
						tex.SetPixel(x, y, Color.Lerp(baseColor, ringColor, edgeAlpha));
					}
					else if(dist >= outerRadius - ringWidth - 1.5f)
					{
						tex.SetPixel(x, y, ringColor * 0.9f);
					}
					else
					{
						float fillFactor = 1f - (dist / outerRadius);
						Color fill = Color.Lerp(baseColor, baseColor * 1.3f, fillFactor);
						tex.SetPixel(x, y, fill);
					}
				}
			}
			tex.Apply();
			CachedTextures[key] = tex;
			return tex;
		}

		public static Texture2D GetJoystickKnobTexture(int size = 64)
		{
			string key = $"JoystickKnob_{primaryColor}_{size}";
			if(CachedTextures.TryGetValue(key, out Texture2D tex) && tex != null) return tex;

			tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
			float center = size / 2f;
			float outerRadius = center - 2f;
			Color themeColor = ColorValues.ContainsKey(primaryColor) ? ColorValues[primaryColor] : new Color(0f, 0.5f, 1f);
			Color coreColor = Color.Lerp(themeColor, Color.white, 0.35f);
			Color shadowColor = themeColor * 0.5f;
			shadowColor.a = 0.95f;

			for(int y = 0; y < size; y++)
			{
				for(int x = 0; x < size; x++)
				{
					Vector2 pos = new Vector2(x, y);
					float dist = Vector2.Distance(pos, new Vector2(center, center));
					if(dist > outerRadius)
					{
						tex.SetPixel(x, y, Color.clear);
					}
					else if(dist >= outerRadius - 2f)
					{
						tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.85f));
					}
					else
					{
						Vector2 offset = new Vector2(x - (center - 4f), y - (center + 4f));
						float lightDist = offset.magnitude / outerRadius;
						Color knobColor = Color.Lerp(coreColor, shadowColor, Mathf.Clamp01(lightDist));
						knobColor.a = 0.92f;
						tex.SetPixel(x, y, knobColor);
					}
				}
			}
			tex.Apply();
			CachedTextures[key] = tex;
			return tex;
		}

		public static GUIStyle JoystickBaseStyle
		{
			get
			{
				GUIStyle style = new GUIStyle();
				style.normal.background = GetJoystickBaseTexture(128);
				return style;
			}
		}

		public static GUIStyle JoystickKnobStyle
		{
			get
			{
				GUIStyle style = new GUIStyle();
				style.normal.background = GetJoystickKnobTexture(64);
				return style;
			}
		}

		public static void ClearCache()
		{
			foreach(Texture2D texture in CachedTextures.Values)
			{
				Object.Destroy(texture);
			}
			CachedTextures.Clear();
		}
	}
}