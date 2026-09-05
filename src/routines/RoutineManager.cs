using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace LunarMenu.routines
{
	internal class RoutineManager : MonoBehaviour
	{
		public readonly AutoTriggerSporesRoutine autoTriggerSpores = new AutoTriggerSporesRoutine();
		public readonly DiscoHostRoutine discoHost = new DiscoHostRoutine();
		public readonly DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();
		public readonly GlitterBomb glitterBomb = new GlitterBomb();
		public readonly JailPlayerRoutine jailPlayer = new JailPlayerRoutine();
		public readonly PetPlayerRoutine petPlayer = new PetPlayerRoutine();
		public readonly PlayerFollowerRoutine playerFollower = new PlayerFollowerRoutine();
		public readonly ReportBodySpam reportBodySpam = new ReportBodySpam();
		public readonly TeleportSpammer teleportSpammer = new TeleportSpammer();
		public readonly VoteSpammer voteSpammer = new VoteSpammer();
		public readonly ZiplineSpammer ziplineSpammer = new ZiplineSpammer();

		public readonly Routine[] routineList;

		public RoutineManager()
		{
			routineList = [ autoTriggerSpores, discoHost, doorTroller, glitterBomb, jailPlayer, petPlayer, playerFollower, reportBodySpam, teleportSpammer, voteSpammer, ziplineSpammer ];
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
					Lunar.Log.LogWarning($"Config has entry for routine {routineName} when there is no such routine");
					continue;
				}

				Routine routine = routineList[routineIndex];
				routine.LoadConfigData(configData);
			}
		}
	}
}