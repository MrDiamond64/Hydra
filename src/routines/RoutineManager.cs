using UnityEngine;

namespace HydraMenu.routines
{
	public class RoutineManager : MonoBehaviour
	{
		public DiscoHostRoutine discoHost = new DiscoHostRoutine();
		public DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();
		public PlayerFollowerRoutine playerFollower = new PlayerFollowerRoutine();

		public void Update()
		{
			if(discoHost.Enabled) discoHost.Run();
			if(doorTroller.Enabled) doorTroller.Run();
			if(playerFollower._enabled) playerFollower.Run();
		}
	}
}