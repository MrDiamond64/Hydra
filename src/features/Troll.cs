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

		/*
		This feature currently does not work. When a player enters a vent, the game runs the PlayerPhysics::CoEnterVent function which is an IEnumerator
		This IEnumerator function makes the player play the vent walk animation, enter vent animation, and update VentilationSystem
		which is used to let the host of the lobby know that someone is in a vent for the vent cleaning feature
		
		We have to wait for the end of the IEnumerator state to make sure the player who vented sends the VentilationSystem update, so we can clean the vent and get them booted from the vent
		Only problem is that Harmony's postfix function runs after the function call finishes, not after the IEnumerator completes.
		This means the player has not yet notificed the host of the ventilationsystem update and so cleaning the vent wont kick them out

		I'm not knowlegable on IEnumerators, so I am unable to get this feature to work. The closest I've gotten is to patch the Vent::EnterVent
		function which is called by PlayerPhysics::CoEnterVent, but that is still too early

		[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
		public static class BlockVenting
		{
			public static bool Enabled { get; set; } = false;

			static void Postfix(Vent __instance, PlayerControl pc)
			{
				if(!Enabled) return;

				Hydra.Log.LogInfo($"Detected someone venting in vent {__instance.Id}");

				if(AmongUsClient.Instance.AmHost)
				{
					Hydra.Log.LogInfo("We're the host, we can just use the BootFromVent RPC.");
					// VentilationSystem::BootImpostersFromVent sends the BootFromVent RPC using the player who should be booted's instance of PlayerPhysics
					// so we replicate that here
					pc.MyPhysics.RpcBootFromVent(__instance.Id);
				} else
				{
					Hydra.Log.LogInfo("We're not host, so we are using vent clean method.");
					VentilationSystem.Update(VentilationSystem.Operation.StartCleaning, __instance.Id);
				}
			}
		}
		*/
	}
}