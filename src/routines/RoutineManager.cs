using UnityEngine;

namespace HydraMenu.routines
{
	public class RoutineManager : MonoBehaviour
	{
		public AutoTriggerSporesRoutine autoTriggerSpores = new AutoTriggerSporesRoutine();
		public DiscoHostRoutine discoHost = new DiscoHostRoutine();
		public DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();
		public PlayerFollowerRoutine playerFollower = new PlayerFollowerRoutine();

		public void Update()
		{
			if(autoTriggerSpores.Enabled) autoTriggerSpores.Run();
			if(discoHost.Enabled) discoHost.Run();
			if(doorTroller.Enabled) doorTroller.Run();
			if(playerFollower._enabled) playerFollower.Run();
		}
	}
}