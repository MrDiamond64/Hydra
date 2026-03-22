using HarmonyLib;

namespace HydraMenu.features
{
	internal class Troll
	{
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		public static class AutoReportBodies
		{
			public static bool Enabled { get; set; } = false;

			static void Prefix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
			{
				if(!Enabled || PlayerControl.LocalPlayer.Data.IsDead) return;

				Hydra.Log.LogInfo($"Recieved MurderPlayer for {target.Data.PlayerName} with result flags {resultFlags}");

				if(!resultFlags.HasFlag(MurderResultFlags.Succeeded)) return;

				// NetworkedPlayerInfo::ColorName automatically appends parentheses at the start and end of the color's name, so we don't need to add them ourselves in the notification
				Hydra.notifications.Send("Auto Report Bodies", $"{target.Data.PlayerName} was just killed by {__instance.Data.PlayerName} {__instance.Data.ColorName}, their body has been automatically reported.");
				PlayerControl.LocalPlayer.CmdReportDeadBody(target.Data);
			}
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deserialize))]
		public static class BlockVenting
		{
			public static bool Enabled { get; set; } = false;

			static void Postfix(VentilationSystem __instance)
			{
				if(!Enabled) return;

				Hydra.Log.LogInfo($"Received updated for VentilationSystem, going to kick out all players who are inside a vent");

				if(__instance.PlayersInsideVents.Count >= PlayerControl.AllPlayerControls.Count)
				{
					Hydra.Log.LogInfo($"Apparently there are more people inside of vents than people inside the game, the host may be trying to overload our game! Players in vents: {__instance.PlayersInsideVents.Count}, total players: {PlayerControl.AllPlayerControls.Count}"); return;
				}

				foreach(byte ventId in __instance.PlayersInsideVents.Values)
				{
					if(ventId >= ShipStatus.Instance.AllVents.Count) continue;

					Hydra.Log.LogInfo($"Kicked someone out of vent {ventId}");
					VentilationSystem.Update(VentilationSystem.Operation.StartCleaning, ventId);
				}
			}
		}
	}
}