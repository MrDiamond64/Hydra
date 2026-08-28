using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text.Json;
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
		public Dictionary<string, Dictionary<string, JsonElement>> GetConfigData()
		{
			Dictionary<string, Dictionary<string, JsonElement>> routineConfig = new Dictionary<string, Dictionary<string, JsonElement>>();

			foreach(Routine routine in routineList)
			{
				routineConfig.Add(routine.name, routine.GetConfigData());
			}

			return routineConfig;
		}

		public void LoadConfigData(Dictionary<string, Dictionary<string, JsonElement>> routineConfig)
		{
			foreach((string routineName, Dictionary<string, JsonElement> configData) in routineConfig)
			{
				int routineIndex = Array.FindIndex(routineList, r => r.name == routineName);
				if(routineIndex == -1)
				{
					Hydra.Log.LogWarning($"Config has entry for routine {routineName} when there is no such routine");
					continue;
				}

				Routine routine = routineList[routineIndex];
				routine.LoadConfigData(configData);
			}
		}
	}
}