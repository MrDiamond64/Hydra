using HarmonyLib;
using UnityEngine;

namespace LunarMenu.modules.visuals
{
	internal class AlwaysVisibleChat : Module
	{
		public AlwaysVisibleChat() : base("AlwaysVisibleChat")
		{
			base.Enabled = true;
		}

		private static AlwaysVisibleChat Instance
		{
			get { return ModuleManager.alwaysVisibleChat; }
		}

		public bool chatActiveOriginalState { get; set; } = false;

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.SetVisible))]
		class SetChatVisibility
		{
			static void Prefix(ref bool visible)
			{
				if (Instance.Enabled)
					visible = true;
				else
					Instance.chatActiveOriginalState = visible;
			}
		}

		[HarmonyPatch(typeof(MatchInfoHudButton), nameof(MatchInfoHudButton.Update))]
		class MatchInfoFix
		{
			static void Prefix(MatchInfoHudButton __instance)
			{
				if (!HudManager.InstanceExists) return;

				var chat = HudManager.Instance.Chat;
				var chatGameObject = chat.gameObject;
				if (chat == null || chatGameObject == null) return;

				var distanceFromEdge = chatGameObject.active ? new Vector3(2.75f, 0.505f, -400f) : new Vector3(2.15f, 0.505f, -400f);
				__instance.aspectPosition.DistanceFromEdge = distanceFromEdge;
			}
		}
	}
}