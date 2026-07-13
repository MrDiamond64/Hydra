using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class ProtectionsSection : ISection
	{
		public ProtectionsSection() : base("Protections") { }
		public override void Render()
		{
			// Network
			Protections.ForceDTLS.Enabled = GUILayout.Toggle(Protections.ForceDTLS.Enabled, "Force enable DTLS to encrypt network data");

			Protections.BlockServerTeleports.Enabled = GUILayout.Toggle(Protections.BlockServerTeleports.Enabled, "Block position updates from server");
			Protections.BlockUnauthorizedSystemUpdates = GUILayout.Toggle(Protections.BlockUnauthorizedSystemUpdates, "Block unauthorized system updates");

			// Overloads
			Protections.BlockLargeGameMessages = GUILayout.Toggle(Protections.BlockLargeGameMessages, "Block large game messages");
			Protections.BlockInvalidGameDataMessages = GUILayout.Toggle(Protections.BlockInvalidGameDataMessages, "Block invalid game data message types");
			Protections.HardenedReadPackedUInt.Enabled = GUILayout.Toggle(Protections.HardenedReadPackedUInt.Enabled, "Use hardened packed int deserializer");
			Protections.MemoryAllocationOverload.Enabled = GUILayout.Toggle(Protections.MemoryAllocationOverload.Enabled, "Protect against VotingComplete overloads");

			Protections.BypassShapeshiftRatelimits.Enabled = GUILayout.Toggle(Protections.BypassShapeshiftRatelimits.Enabled, "Bypass ratelimits for Shapeshift RPC");
			Protections.Votekicks.Enabled = GUILayout.Toggle(Protections.Votekicks.Enabled, "Prevent being votekicked as host");
			Protections.ProtectAgainstNonHostKickExploit = GUILayout.Toggle(Protections.ProtectAgainstNonHostKickExploit, "Protect against non-host kick exploit");
		}
	}
}