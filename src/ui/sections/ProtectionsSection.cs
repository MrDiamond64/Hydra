using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class ProtectionsSection : ISection
	{
		public ProtectionsSection()
		{
			name = "Protections";
		}

		public override void Render()
		{
			// Network
			Protections.ForceDTLS.Enabled = GUILayout.Toggle(Protections.ForceDTLS.Enabled, "Force enable DTLS to encrypt network data");

			Protections.BlockServerTeleports.Enabled = GUILayout.Toggle(Protections.BlockServerTeleports.Enabled, "Block position updates from server");

			// Overloads
			Protections.HardenedReadPackedUInt.Enabled = GUILayout.Toggle(Protections.HardenedReadPackedUInt.Enabled, "Use hardened packet int deserializer");
			Protections.BlockInvalidLadderOverload = GUILayout.Toggle(Protections.BlockInvalidLadderOverload, "Protect against invalid ladder overload");

			Protections.Votekicks.Enabled = GUILayout.Toggle(Protections.Votekicks.Enabled, "Prevent being votekicked as host");
		}
	}
}