using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
    internal class ProtectionsSection : ISection
    {
        public ProtectionsSection()
        {
            sectionName = "Protections";
        }

        public override void Render()
        {
			Protections.ForceDTLS.Enabled = GUILayout.Toggle(Protections.ForceDTLS.Enabled, "Force enable DTLS to encrypt network data");

			Protections.BlockServerTeleports.Enabled = GUILayout.Toggle(Protections.BlockServerTeleports.Enabled, "Block position updates from server");

			Protections.Votekicks.Enabled = GUILayout.Toggle(Protections.Votekicks.Enabled, "Prevent being votekicked as host");
        }
    }
}