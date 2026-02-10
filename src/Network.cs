using Hazel;
using UnityEngine;

namespace HydraMenu
{
    internal class Network
    {
		public static void SendCheckColor(byte color)
        {
            if(AmongUsClient.Instance.AmHost)
            {
                PlayerControl.LocalPlayer.RpcSetColor(color);
                return;
            }

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RpcCalls.CheckColor,
                SendOption.None,
                AmongUsClient.Instance.HostId
            );

            writer.Write(color);

            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        // PlayerControl.LocalPlayer.CmdCheckSporeTrigger expects an instance of Mushroom to be passed
        // It isn't very easy to get an instance of Mushroom so we just reimplement that function and accept a mushroom ID
        public static void SendCheckSporeTrigger(int mushroomId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RpcCalls.CheckSpore,
                SendOption.None,
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
                SendOption.None,
                -1
            );

            byte scanCount = ++PlayerControl.LocalPlayer.scannerCount;

            writer.Write(scanning);
            writer.Write(scanCount);

            AmongUsClient.Instance.FinishRpcImmediately(writer);

            // Render the medbay animation for ourselves
            PlayerControl.LocalPlayer.SetScanner(scanning, scanCount);
        }

        public static void SendSnapTo(CustomNetworkTransform transform, Vector2 position)
        {
			transform.SnapTo(position);

			ushort seqNum = (ushort)(transform.lastSequenceId + 2);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                transform.NetId,
                (byte)RpcCalls.SnapTo,
                SendOption.None,
                -1
            );

            NetHelpers.WriteVector2(position, writer);
            writer.Write(seqNum);

            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}