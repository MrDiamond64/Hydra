using HarmonyLib;
using HydraMenu.ui;
using HydraMenu.ui.sections;
using UnityEngine;

namespace HydraMenu.features
{
	static class ClickBlock
	{
		public static bool ShouldBlock()
		{
			if(!MenuSection.BlockClickThrough) return true;
			MainUI ui = MainUI.Instance;
			if(ui == null || !ui.visible) return true;

			if(GUIUtility.hotControl != 0) return true;

			Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			bool inMenu = mousePos.x >= MainUI.windowPosition.x && mousePos.x <= (MainUI.windowPosition.x + MainUI.WindowSize.x) && mousePos.y >= MainUI.windowPosition.y && mousePos.y <= (MainUI.windowPosition.y + MainUI.WindowSize.y);
			return !inMenu;
		}
	}

	[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
	static class BlockPassiveButtonClickDown { static bool Prefix() => ClickBlock.ShouldBlock(); }

	[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveRepeatDown))]
	static class BlockPassiveButtonRepeatDown { static bool Prefix() => ClickBlock.ShouldBlock(); }

	[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickUp))]
	static class BlockPassiveButtonClickUp { static bool Prefix() => ClickBlock.ShouldBlock(); }

	[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.SetPassiveButtonHoverStateActive))]
	static class BlockHoverActive { static bool Prefix() => ClickBlock.ShouldBlock(); }
}
