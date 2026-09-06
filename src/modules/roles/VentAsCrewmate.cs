using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace LunarMenu.modules.roles
{
	internal class VentAsCrewmate : Module
	{
		public VentAsCrewmate() : base("VentAsCrewmate")
		{
			base.Enabled = true;
		}

		private static VentAsCrewmate Instance
		{
			get { return ModuleManager.ventAsCrewmate; }
		}

		// Similar to being able to use the sabotage button while crewmate, the vent button also has checks to make sure the current player can actually vent, so we have to reimplement the Vent::CanUse function
		// The normal function also has checks to make sure the vent isn't being cleaned, however that isn't important so we don't reimplement those checks
		[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
		class SkipVentChecks
		{
			static bool Prefix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
			{
				if (Instance.Enabled || PlayerControl.LocalPlayer.inVent)
				{
					var pObject = pc.Object;
					if (!pObject) return true;

					var ventVector = __instance.transform.position;

					float ventDistance = Vector2.Distance(pObject.GetTruePosition(), new Vector2(ventVector.x, ventVector.y));
					if (pc.IsDead)
					{
						canUse = false;
						couldUse = false;
					}
					else
					{
						canUse = (ventDistance < __instance.UsableDistance);
						couldUse = true;
					}

					__result = ventDistance;
					return false;
				}
				return true;
			}
		}

		private static bool bChatAlwaysActivePrevious = false;

		[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
		class VentButtonPatch
		{
			static void Prefix(HudManager __instance)
			{
				try
				{
					if (bChatAlwaysActivePrevious != ModuleManager.alwaysVisibleChat.Enabled)
					{
						if (ModuleManager.alwaysVisibleChat.Enabled)
							__instance.Chat.SetVisible(true);
						else if (!GameState.InMeeting && !Utilities.InLobby())
							__instance.Chat.SetVisible(ModuleManager.alwaysVisibleChat.chatActiveOriginalState);
						bChatAlwaysActivePrevious = ModuleManager.alwaysVisibleChat.Enabled;
					}

					if (Utilities.InGame() || Utilities.InLobby())
					{
						var localData = PlayerControl.LocalPlayer?.Data;
						if (!localData) return;

						if (!GameState.InMeeting)
						{
							var kbjPlayer = KeyboardJoystick.player;

							RoleBehaviour playerRole = localData.Role;
							RoleTypes role = playerRole != null ? playerRole.Role : RoleTypes.Crewmate;
							GameObject ImpostorVentButton = __instance.ImpostorVentButton.gameObject;

							if (ImpostorVentButton != null)
							{
								bool forceShowVentButton = Instance.Enabled || (PlayerControl.LocalPlayer.inVent && role != RoleTypes.Engineer);

                                var gameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                                if ((role != RoleTypes.Engineer || !Instance.Enabled) && (localData.IsDead || Utilities.InLobby()))
									ImpostorVentButton.SetActive(false);
								else
									ImpostorVentButton.SetActive(forceShowVentButton || (Utilities.IsImpostor(localData) && gameOptions.GameMode == GameModes.Normal));

								if (kbjPlayer != null && forceShowVentButton && !(Utilities.IsImpostor(localData) && gameOptions.GameMode == GameModes.Normal) &&
									kbjPlayer.GetButton(50) && (PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.inVent))
									__instance.ImpostorVentButton.DoClick();
							}
						}
					}
				} catch { }
			}
		}
    }
}