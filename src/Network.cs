using Hazel;

namespace HydraMenu
{
	internal class Network
	{
		// The PlayerControl::RpcSetScanner function does not send the RPC if visual tasks are off
		// If we want the scan animation to show up even if visual tasks are enabled, then we will need to reimplement it
		public static void SendSetScanner(bool scanning)
		{
			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.NetId,
				(byte)RpcCalls.SetScanner,
				SendOption.Reliable,
				-1
			);

			byte scanCount = ++PlayerControl.LocalPlayer.scannerCount;

			writer.Write(scanning);
			writer.Write(scanCount);

			AmongUsClient.Instance.FinishRpcImmediately(writer);

			// Render the medbay animation for ourselves
			PlayerControl.LocalPlayer.SetScanner(scanning, scanCount);
		}

		// The PlayerControl::RpcPlayAnimation function does not send the RPC if visual tasks are off
		// If we want the task animation to show up even if visual tasks are enabled, then we will need to reimplement it
		public static void SendPlayAnimation(byte animation)
		{
			if(ShipStatus.Instance == null) return;

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.NetId,
				(byte)RpcCalls.PlayAnimation,
				SendOption.None,
				-1
			);

			writer.Write(animation);

			AmongUsClient.Instance.FinishRpcImmediately(writer);

			// Render the task animation for ourselves
			PlayerControl.LocalPlayer.PlayAnimation(animation);
		}
	}
}