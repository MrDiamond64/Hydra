using Hazel;
using UnityEngine;

namespace HydraMenu
{
    internal class Network
    {
        // PlayerControl.LocalPlayer.CmdCheckSporeTrigger expects an instance of Mushroom to be passed
        // It isn't very easy to get an instance of Mushroom so we just reimplement that function and accept a mushroom ID
        public static void SendCheckSporeTrigger(int mushroomId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RpcCalls.CheckSpore,
                SendOption.Reliable,
                AmongUsClient.Instance.HostId
            );

            writer.WritePacked(mushroomId);

            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

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
    }
}