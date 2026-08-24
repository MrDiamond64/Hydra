using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.routines
{
	internal class RoutineManager : MonoBehaviour
	{
		public AutoTriggerSporesRoutine autoTriggerSpores = new AutoTriggerSporesRoutine();
		public DiscoHostRoutine discoHost = new DiscoHostRoutine();
		public DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();
		public JailPlayerRoutine jailPlayer = new JailPlayerRoutine();
		public PetPlayerRoutine petPlayer = new PetPlayerRoutine();
		public PlayerFollowerRoutine playerFollower = new PlayerFollowerRoutine();
		public ReportBodySpam reportBodySpam = new ReportBodySpam();
		public TeleportSpammer teleportSpammer = new TeleportSpammer();

		public readonly Routine[] routineList;

		public RoutineManager()
		{
			routineList = [ autoTriggerSpores, discoHost, doorTroller, jailPlayer, petPlayer, playerFollower, reportBodySpam, teleportSpammer ];
		}

		public void Update()
		{
			foreach(Routine routine in routineList)
			{
				if(!routine.Enabled) continue;

				routine.Run();
			}
		}

		// Return a dictionary of each routine with its name, and another dictionary with names and values of each property
		public Dictionary<string, Dictionary<string, object>> GetConfigData()
		{
			Dictionary<string, Dictionary<string, object>> routineConfig = new Dictionary<string, Dictionary<string, object>>();

			foreach(Routine routine in routineList)
			{
				routineConfig.Add(routine.name, routine.GetConfigData());
			}

			return routineConfig;
		}

		[HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
		class DisconnectHandler
		{
			static void Prefix()
			{
				Hydra.Log.LogInfo("Player disconnected from the lobby, disabling relevant routines");

				foreach(Routine routine in Hydra.routines.routineList)
				{
					if(!routine.Enabled) continue;

					routine.OnDisconnect();
				}
			}
		}
	}
}