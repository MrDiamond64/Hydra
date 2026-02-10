using UnityEngine;

namespace HydraMenu.routines
{
    public class RoutineManager : MonoBehaviour
    {
        public DiscoHostRoutine discoHost = new DiscoHostRoutine();
		public DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();

		public void Update()
        {
            if(discoHost.Enabled) discoHost.Run();
			if(doorTroller.Enabled) doorTroller.Run();
		}
    }
}