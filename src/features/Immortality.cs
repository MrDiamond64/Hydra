using HarmonyLib;
using InnerNet;

namespace HydraMenu.features
{
	internal class Immortality
	{
		// The PlayerControl::CheckMurder function is the handler for CheckMurder RPCs. When the host of the lobby receives this RPC, it first checks
		// to make sure that the player who attempted to kill is an imposter and is alive, and then checks if the player who should be killed is alive, is not inside a vent, and is not on a ladder or platform
		// If everything goes smoothly, a MurderPlayer RPC with flag Succeeded is sent to all online players and the player killed
		// If one of the above checks fails, then a MurderPlayer RPC with flag FailedError is sent and the player is not killed
		// We can potentially use this as an immortality exploit by making the host think we are inside a vent
		// In theory, we should be able to send a GameDataTo message to the host with an EnterVent RPC which will make the host think we are inside a vent
		// but to every other player in the game we will still appear to be moving around
		// The biggest problem with this is that the CheckMurder RPC is server-authoritative, not host-authoritative, meaning that the checks we see in the PlayerControl::CheckMurder function may not actually be the case when the backend Among Us servers handle the CheckMurder RPC
		// The backend Among Us servers do check if the player who should be killed is inside a vent, but not through the EnterVent or ExitVent RPCs!
		// Instead they check if a player is inside a vent through the VentilationSystem system in the ShipStatus net object
		// When your Among Us client enters a vent, you first send an EnterVent RPC which makes your player walk towards a vent and then go inside a vent, and then your client sends an UpdateSystem RPC
		// for the ventilation system with an operation of Enter, which tells players that you are inside of a vent
		// This feature is used for the vent-cleaning feature to determine which players should be kicked out of a vent
		// but it also used by the backend Among Us servers to determine if a player is inside a vent when handling CheckMurder RPCs
		// So when the backend Among Us servers received a CheckMurder RPC, it goes through a list of all net objects that exist for the given lobby, finds ShipStatus, gets the data for the VentilationSystem, and determines if a player is inside of a vent through there
		// Server authority here is actually helpful for us as our previous theory for immortality would make us immortal in the eyes of the host, meanwhile this will make us visible for all online players
		private static readonly int CUSTOM_VENT_ID = 50;

		private static bool _enabled = false;

		public static bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if(value == _enabled) return;

				if(PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.inVent)
				{
					if(value)
					{
						Hydra.Log.LogInfo("Immortality was enabled, sending a VentilationSystem update with operation Enter");
						VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
					}
					else
					{
						Hydra.Log.LogInfo("Immortality was disabled, sending a VentilationSystem update with operation Exit");
						VentilationSystem.Update(VentilationSystem.Operation.Exit, CUSTOM_VENT_ID);
					}
				}

				_enabled = value;
			}
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Update))]
		class BlockSendingUpdates
		{
			static bool Prefix(VentilationSystem.Operation op, int ventId)
			{
				if(ventId != CUSTOM_VENT_ID && Enabled && (op == VentilationSystem.Operation.Enter || op == VentilationSystem.Operation.Exit || op == VentilationSystem.Operation.Move))
				{
					// Hydra.Log.LogInfo($"Our client send VentilationSystem operation {op} for vent {ventId}. Resending Immortality RPC");
					// VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);

					Hydra.Log.LogInfo($"Our client sent VentilationSystem operation {op} for vent {ventId}, cancelling..");
					return false;
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
		class OnGameStart
		{
			static void Postfix()
			{
				if(!Enabled) return;

				Hydra.Log.LogMessage($"A new instance of ShipStatus has spawned, sending the immortality RPC");
				VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		class OnMurder
		{
			static void Postfix(PlayerControl __instance, PlayerControl target)
			{
				if(Enabled && target == PlayerControl.LocalPlayer)
				{
					Hydra.notifications.Send("Immortality", $"{__instance.Data.PlayerName} attempted to kill you!", 5);
				}
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
		class OnMeetingEnd
		{
			static void Postfix()
			{
				if(!Enabled || PlayerControl.LocalPlayer.Data.IsDead) return;

				Hydra.Log.LogInfo("Meeting has ended, resending Immortality RPC to retain immortal status");
				VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
			}
		}
	}
}