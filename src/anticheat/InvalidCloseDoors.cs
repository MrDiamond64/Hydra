using Hazel;

namespace HydraMenu.anticheat
{
	internal class InvalidCloseDoors : ICheck
	{
		public static void OnDoorClose(MessageReader reader, ref bool blockRpc)
		{
			if(!Anticheat.Enabled || !Anticheat.CheckInvalidCloseDoors) return;

			// It would be nice if we could also add additional checks such as someone closing doors without being imposter
			// however we are not able to determine who send the CloseDoorsOfType RPC
			if(GameManager.Instance.IsHideAndSeek())
			{
				Hydra.notifications.Send("Anticheat", "Someone attempted to close doors while in Hide and Seek", Anticheat.NotificationDuration);
				blockRpc = true;
			}
		}
	}
}