using HarmonyLib;
using UnityEngine;

namespace HydraMenu.routines
{
	internal class RoutineManager : MonoBehaviour
	{
		public AutoTriggerSporesRoutine autoTriggerSpores = new AutoTriggerSporesRoutine();
		public DiscoHostRoutine discoHost = new DiscoHostRoutine();
		public DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();
		public JailPlayerRoutine jailPlayer = new JailPlayerRoutine();
		public PlayerFollowerRoutine playerFollower = new PlayerFollowerRoutine();
		public ReportBodySpam reportBodySpam = new ReportBodySpam();
		public TeleportSpammer teleportSpammer = new TeleportSpammer();

		public readonly IRoutine[] routineList;

		public RoutineManager()
		{
			routineList = [ autoTriggerSpores, discoHost, doorTroller, jailPlayer, playerFollower, reportBodySpam, teleportSpammer ];
		}

		public void Update()
		{
			foreach(IRoutine routine in routineList)
			{
				if(!routine.Enabled) continue;

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
					if(!routine.Enabled) continue;

					routine.OnDisconnect();
				}
			}
		}
	}
}