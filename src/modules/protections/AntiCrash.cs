using HarmonyLib;

namespace LunarMenu.modules.protections
{
	internal class AntiCrash : Module
	{
		// A way to crash Among Us lobbies is to send a ReportDeadBody RPC during the game loading screen
		// The RPC handler for the ReportDeadBody RPC has a check to see if all the tasks in the game has completed
		// Since no players have been assigned tasks yet, this will return true and result in the game ending
		// The host tells the AU server that the game has ended by sending an EndGame root message (the function is called an RPC, but it is not an RPC)
		// This in theory should mean that the game will forcibly be finished and everyone will be returned to the lobby, but that is actually not the case
		// When the lobby host sends the EndGame message during the loading screen, the Among Us matchmaking servers will delete the lobby instead of returning everyone back
		public AntiCrash() : base("AntiCrash")
		{
			base.Enabled = true;
		}

		private static AntiCrash Instance
		{
			get { return ModuleManager.antiCrash; }
		}

		private bool gameFullyLoaded = false;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
		class OnCallMeeting
		{
			static bool Prefix(PlayerControl __instance)
			{
				if(!Instance.Enabled || Instance.gameFullyLoaded) return true;

				Lunar.notifications.Send("Protections Alert", $"{__instance.Data.PlayerName} attempted to use a lobby crash exploit!");
				return false;
			}
		}

		private void OnGameStart()
		{
			gameFullyLoaded = false;
		}

		private void OnGameLoad()
		{
			gameFullyLoaded = true;
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameStart += OnGameStart;
			EventCoordinator.OnGameLoad += OnGameLoad;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
			EventCoordinator.OnGameLoad -= OnGameLoad;
		}
	}
}