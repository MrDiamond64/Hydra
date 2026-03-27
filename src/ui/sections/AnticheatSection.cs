using HydraMenu.anticheat;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class AnticheatSection : ISection
	{
		public AnticheatSection()
		{
			name = "Anticheat";
		}

		public override void Render()
		{
			Anticheat.Enabled = GUILayout.Toggle(Anticheat.Enabled, "Enable Hydra Anticheat");

			Anticheat.CheckSpoofedPlatforms = GUILayout.Toggle(Anticheat.CheckSpoofedPlatforms, "Flag Spoofed Platform Data");

			Anticheat.CheckInvalidCloseDoors = GUILayout.Toggle(Anticheat.CheckInvalidCloseDoors, "Block Invalid CloseDoors RPCs");
			Anticheat.CheckInvalidCompleteTask = GUILayout.Toggle(Anticheat.CheckInvalidCompleteTask, "Flag Invalid CompleteTask RPCs");
			Anticheat.CheckInvalidPlayAnimation = GUILayout.Toggle(Anticheat.CheckInvalidPlayAnimation, "Flag Invalid PlayAnimation RPCs");
			Anticheat.CheckSpoofedLevels = GUILayout.Toggle(Anticheat.CheckSpoofedLevels, "Flag Invalid SetLevel RPCs");
			Anticheat.CheckInvalidVent = GUILayout.Toggle(Anticheat.CheckInvalidVent, "Flag Invalid EnterVent and ExitVent RPCs");
			Anticheat.CheckInvalidScan = GUILayout.Toggle(Anticheat.CheckInvalidScan, "Flag Invalid SetScanner RPCs");
			Anticheat.CheckInvalidSnapTo = GUILayout.Toggle(Anticheat.CheckInvalidSnapTo, "Flag Invalid SnapTo RPCs");
			Anticheat.CheckInvalidStartCounter = GUILayout.Toggle(Anticheat.CheckInvalidStartCounter, "Flag Invalid SetStartCounter RPCs");
			Anticheat.CheckInvalidSystemUpdates = GUILayout.Toggle(Anticheat.CheckInvalidSystemUpdates, "Flag Invalid UpdateSystem RPCs");

			GUILayout.Space(5);
			GUILayout.Label("When a cheating is detected:");
			Anticheat.DiscardRPC = GUILayout.Toggle(Anticheat.DiscardRPC, "Discard RPC");

			GUILayout.BeginHorizontal();
			GUILayout.Label($"Punish the player with: {Anticheat.punishment}");
			Anticheat.punishment = (Anticheat.Punishments)GUILayout.HorizontalSlider((float)Anticheat.punishment, 0, 3);
			GUILayout.EndHorizontal();
		}
	}
}