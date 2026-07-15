using HarmonyLib;
using UnityEngine;

namespace HydraMenu.features
{
    internal class Visuals
    {
        public static bool ShowKillCooldown { get => HydraMenu.ui.Settings.Config.General.ShowKillCooldown; set => HydraMenu.ui.Settings.Config.General.ShowKillCooldown = value; }
        public static bool ShowImpostors { get => HydraMenu.ui.Settings.Config.General.ShowImpostors; set => HydraMenu.ui.Settings.Config.General.ShowImpostors = value; }
        public static bool RevealRoles { get => HydraMenu.ui.Settings.Config.General.RevealRoles; set => HydraMenu.ui.Settings.Config.General.RevealRoles = value; }

        public static void MeetingNametags(MeetingHud meetingHud)
        {
            try
            {
                foreach (var playerState in meetingHud.playerStates)
                {
                    var data = GameData.Instance.GetPlayerById(playerState.TargetPlayerId);
                    if (data == null || data.Disconnected || data.Outfits[PlayerOutfitType.Default] == null) continue;

                    string nameText = data.DefaultOutfit.PlayerName;
                    string color = "white";
                    bool isImpostor = data.Role != null && data.Role.CanUseKillButton;

                    if (RevealRoles)
                    {
                        nameText += $" <size=60%>[{data.RoleType}]</size>";
                    }

                    if (ShowImpostors && isImpostor)
                    {
                        color = "red";
                    }

                    playerState.NameText.text = $"<color=\"{color}\">{nameText}</color>";

                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            } catch { }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class MeetingNametagsPatch
        {
            static void Postfix(MeetingHud __instance)
            {
                MeetingNametags(__instance);
            }
        }

        // Is there a better way of implementing fullbright?
        // This current method does not allow you to see through walls due to shadows
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static class Fullbright
        {
            public static bool Enabled { get => HydraMenu.ui.Settings.Config.General.FullbrightEnabled; set => HydraMenu.ui.Settings.Config.General.FullbrightEnabled = value; }

            static bool Prefix(ref float __result)
            {
                if(!Enabled) return true;

                __result = 1000f;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
        public static class ShowProtections
        {
            public static bool Enabled { get => HydraMenu.ui.Settings.Config.General.ShowProtectionsEnabled; set => HydraMenu.ui.Settings.Config.General.ShowProtectionsEnabled = value; }

            static void Prefix(ref bool visible)
            {
                if(Enabled) visible = true;
            }
        }

        // The GameData::ShowNotification function by default only handles disconnect reasons of ExitGame, Kicked, or Banned
        // Any other disconnection reasons automatically default to the error disconnection message
		[HarmonyPatch(typeof(GameData), nameof(GameData.ShowNotification))]
		public static class AccurateDisconnectReasons
		{
			public static bool Enabled { get => HydraMenu.ui.Settings.Config.General.AccurateDisconnectReasonsEnabled; set => HydraMenu.ui.Settings.Config.General.AccurateDisconnectReasonsEnabled = value; }

			static bool Prefix(string playerName, DisconnectReasons reason)
			{
                if(!Enabled) return true;

				Hydra.Log.LogInfo($"[Disconnect Logger] {playerName} was disconnected with reason {reason}");

				switch(reason) {
                    // GameData::ShowNotification already handles these disconnect messages
                    case DisconnectReasons.ExitGame:
                    case DisconnectReasons.Kicked:
                    case DisconnectReasons.Banned:
                    case DisconnectReasons.Error:
                        return true;

                    case DisconnectReasons.Hacking:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} was banned by the Among Us anticheat for hacking.");
						return false;

                    case DisconnectReasons.DuplicateConnectionDetected:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} was kicked due to duplicate login.");
						return false;

                    // This disconnect reason happens when a player does not send the ClientReady message after the game starts in time
                    case DisconnectReasons.ClientTimeout:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} was kicked due to timeout.");
                        return false;

					default:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} was disconnected due to {reason}.");
						return false;
                }
			}
		}

		[HarmonyPatch(typeof(ShhhBehaviour), nameof(ShhhBehaviour.PlayAnimation))]
		public static class SkipShhhAnimation
		{
			public static bool Enabled { get => HydraMenu.ui.Settings.Config.General.SkipShhhAnimationEnabled; set => HydraMenu.ui.Settings.Config.General.SkipShhhAnimationEnabled = value; }

			static bool Prefix()
			{
				if(Enabled)
				{
					HudManager.Instance.shhhEmblem.gameObject.SetActive(false);
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
		public static class KillCooldownUpdate
		{
			public static bool Enabled { get; set; } = false;

			static void Postfix(PlayerControl __instance)
			{
				// Logic moved to VisualsRenderer.Update to prevent main thread hangs
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetName))]
		public static class KillCooldownNametag
		{
			public static bool Enabled { get; set; } = true;

			static void Postfix(PlayerControl __instance)
			{
				// This method is called by the game to set the player's name.
				// We let it happen, and our VisualsRenderer.Update will handle the cooldown overlay.
			}
		}

		// PlayerControl::FixedUpdate sets PlayerControl::set_Visible to false if the player is dead, or true if the player is alive
		// The set_Visible function runs CosmeticsLayer::set_Visible in order to hide or show the player's cosmetics
		// If we want to show ghosts even if we are alive, then we can reimplement PlayerControl::set_Visible and make it so player cosmetics are always visible
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Visible), MethodType.Setter)]
		public static class ShowGhosts
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(PlayerControl __instance)
			{
				if(Enabled && __instance.Data.IsDead)
				{
					__instance.cosmetics.Visible = true;
					return false;
				}
				else
				{
					return true;
				}
			}
		}
	}
}