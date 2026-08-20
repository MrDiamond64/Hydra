using HarmonyLib;
using System.Collections.Generic;

namespace HydraMenu.features
{
	internal class Troll
	{
		public static Dictionary<PlayerControl, ushort> VentSeqIds = new Dictionary<PlayerControl, ushort>();

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deserialize))]
		public static class BlockVenting
		{
			public static bool Enabled { get; set; } = false;

			static void Postfix(VentilationSystem __instance)
			{
				if(!Enabled) return;

				Hydra.Log.LogInfo($"Received update for VentilationSystem, going to kick out all players who are inside a vent");

				if(__instance.PlayersInsideVents.Count >= PlayerControl.AllPlayerControls.Count)
				{
					Hydra.Log.LogInfo($"Apparently there are more people inside of vents than people inside the game, the host may be trying to overload our game! Players in vents: {__instance.PlayersInsideVents.Count}, total players: {PlayerControl.AllPlayerControls.Count}");
					return;
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