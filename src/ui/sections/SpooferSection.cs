using HydraMenu.features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SpooferSection : ISection
	{
		public SpooferSection()
		{
			sectionName = "Spoofer";
		}

		public readonly Dictionary<string, int> versions = new Dictionary<string, int>()
		{
			// Current version at runtime
			// VersionShower::Start uses ReferenceDataManager.Refdata.userFacingVersion to get version strings such as "17.1" however that doesn't seem to work at runtime, so we have to use Constants::AddressablesVersion to get a less human-understandable version string
			{ $"{Constants.AddressablesVersion} (Current)", Constants.GetBroadcastVersion() },
			{ "16.1.0", 50632950 },
			{ "17.1", 50643450 },
			{ "17.1.2", 50647000 },
			{ "17.2", 50645050 },
			{ "17.2.1", 50652900 }
		};

		private int versionSelection = 0;

		public override void Render()
		{
			Spoofer.shouldSpoofVersion = GUILayout.Toggle(Spoofer.shouldSpoofVersion, "Enable Version Spoofing");

			GUILayout.Label($"Spoofed Version: {versions.ElementAt(versionSelection).Key} ({Spoofer.spoofedVersion})");
			versionSelection = (int)GUILayout.HorizontalSlider(versionSelection, 0, versions.Count - 1);
			Spoofer.spoofedVersion = versions.ElementAt(versionSelection).Value;

			Spoofer.useModdedProtocol = GUILayout.Toggle(Spoofer.useModdedProtocol, "Use Modded Protocol");

			GUILayout.Label($"Spoofed Platform: {Spoofer.spoofedPlatform}");
			Spoofer.spoofedPlatform = (Platforms)GUILayout.HorizontalSlider((float)Spoofer.spoofedPlatform, 0, 10);
		}
	}
}