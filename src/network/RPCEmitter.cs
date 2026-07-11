using Hazel;

namespace HydraMenu.network
{
	// Shorthand, and more convenient way, of sending RPCs without having to create a new batch through BatchedMessage
	internal class RPCEmitter
	{
		// The PlayerControl::RpcSetScanner function does not send the RPC if visual tasks are off
		// If we want the scan animation to show up even if visual tasks are enabled, then we will need to reimplement it
		public static void SendSetScanner(bool scanning)
		{
			byte scanCount = ++PlayerControl.LocalPlayer.scannerCount;

			// Render the medbay animation for ourselves
			PlayerControl.LocalPlayer.SetScanner(scanning, scanCount);

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.NetId,
				(byte)RpcCalls.SetScanner,
				SendOption.Reliable,
				-1
			);

			writer.Write(scanning);
			writer.Write(scanCount);

			AmongUsClient.Instance.FinishRpcImmediately(writer);
		}

		// The PlayerControl::RpcPlayAnimation function does not send the RPC if visual tasks are off
		// If we want the task animation to show up even if visual tasks are enabled, then we will need to reimplement it
		public static void SendPlayAnimation(byte animation)
		{
			// Render the task animation for ourselves
			PlayerControl.LocalPlayer.PlayAnimation(animation);

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.NetId,
				(byte)RpcCalls.PlayAnimation,
				SendOption.None,
				-1
			);

			writer.Write(animation);

			AmongUsClient.Instance.FinishRpcImmediately(writer);
		}
	}
}