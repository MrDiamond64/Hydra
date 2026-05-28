using HarmonyLib;
using UnityEngine;

namespace HydraMenu.routines
{
	public class RoutineManager : MonoBehaviour
	{
		public AutoTriggerSporesRoutine autoTriggerSpores = new AutoTriggerSporesRoutine();
		public DiscoHostRoutine discoHost = new DiscoHostRoutine();
		public DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();
		public PlayerFollowerRoutine playerFollower = new PlayerFollowerRoutine();
		public ReportBodySpam reportBodySpam = new ReportBodySpam();

		public IRoutine[] routineList = [];

		public RoutineManager()
		{
			routineList = [ autoTriggerSpores, discoHost, doorTroller, playerFollower, reportBodySpam ];
		}

		public void Update()
		{
			foreach(IRoutine routine in routineList)
			{
				if(!routine._enabled) continue;

				routine.Run();
			}
		}

		[HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
		class DisconnectHandler
		{
			static void Prefix()
			{
				Hydra.Log.LogInfo("Player disconnected from the lobby, disabling relevant routines");

				foreach(IRoutine routine in Hydra.routines.routineList)
				{
					if(!routine._enabled) continue;

					routine.OnDisconnect();
				}
			}
		}
	}
}