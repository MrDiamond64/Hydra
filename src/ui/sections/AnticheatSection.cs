using HydraMenu.anticheat;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class AnticheatSection : ISection
	{
		public AnticheatSection() : base("Anticheat") 
		{
			AddFeature("Enable Hydra Anticheat", () => { });
			AddFeature("Flag Spoofed Platform Data", () => { });
			AddFeature("RPC Checks", () => { });
			AddFeature("Notification Settings", () => { });
			AddFeature("Discard RPC", () => { });
			AddFeature("Punishment Settings", () => { });
		}

		public override void Render()
		{
			Anticheat.Enabled = GUILayout.Toggle(Anticheat.Enabled, "Enable Hydra Anticheat");

			Anticheat.CheckSpoofedPlatforms = GUILayout.Toggle(Anticheat.CheckSpoofedPlatforms, "Flag Spoofed Platform Data");

			GUILayout.Space(5);
			GUILayout.Label("RPCs that should be checked by the anticheat:");
			foreach(var (rpcCall, handler) in Anticheat.RpcHandlers)
			{
				handler.Enabled = GUILayout.Toggle(handler.Enabled, $"{rpcCall}");
			}

			GUILayout.Space(5);
			GUILayout.Label("When a cheater is detected:");
			Anticheat.sendNotification = GUILayout.Toggle(Anticheat.sendNotification, "Send notification");
			Anticheat.discardRpc = GUILayout.Toggle(Anticheat.discardRpc, "Discard RPC");

			GUILayout.BeginHorizontal();
			GUILayout.Label($"Punish the player with: {Anticheat.punishment}");
			Anticheat.punishment = (Anticheat.Punishments)GUILayout.HorizontalSlider((float)Anticheat.punishment, 0, 3);
			GUILayout.EndHorizontal();
		}
	}
}