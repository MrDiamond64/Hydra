using HarmonyLib;
using HydraMenu.ui;
using UnityEngine;

namespace HydraMenu.anticheat
{
	internal static class ClickBlockPatch
	{
		private static bool MenuInTheWay()
		{
			MainUI ui = Hydra.mainUI;
			if(ui == null || !ui.visible || !ui.blockClickThrough) return false;
			Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			return MainUI.IsInBox(mousePos);
		}

		[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
		class BlockClickDown
		{
			static bool Prefix() { return !ClickBlockPatch.MenuInTheWay(); }
		}

		[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveRepeatDown))]
		class BlockRepeatDown
		{
			static bool Prefix() { return !ClickBlockPatch.MenuInTheWay(); }
		}

		[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickUp))]
		class BlockClickUp
		{
			static bool Prefix() { return !ClickBlockPatch.MenuInTheWay(); }
		}

		[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.SetPassiveButtonHoverStateActive))]
		class BlockHoverStateActive
		{
			static bool Prefix() { return !ClickBlockPatch.MenuInTheWay(); }
		}

		// DO NOT PATCH PassiveButton here. SlideBar is also not patched as its HandleDrag is false.
		[HarmonyPatch(typeof(Scrollbar), nameof(Scrollbar.ReceiveClickDrag))]
		class BlockScrollbarDrag
		{
			static bool Prefix(Vector2 dragDelta) { return !ClickBlockPatch.MenuInTheWay(); }
		}

		// PassiveButtonManager only has 1 button hovered at a time keeping it in currentOver field.
		[HarmonyPatch(typeof(PassiveButtonManager), nameof(PassiveButtonManager.Update))]
		class ClearHover
		{
			static void Postfix()
			{
				if(!ClickBlockPatch.MenuInTheWay()) return;

				PassiveButtonManager manager = PassiveButtonManager.Instance;
				if(manager == null || manager.currentOver == null) return;

				manager.currentOver.ReceiveMouseOut();
			}
		}
	}
}