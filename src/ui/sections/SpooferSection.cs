using HydraMenu.features;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SpooferSection : ISection
	{
		public SpooferSection() : base("Spoofer") 
		{
			AddFeature("Version Spoofing", () => { });
			AddFeature("Level Spoofing", () => { });
			AddFeature("Modded Protocol", () => { });
			AddFeature("Platform Spoofing", () => { });
			AddFeature("Avoid Penalties", () => {
				Spoofer.avoidPenalties = GUILayout.Toggle(Spoofer.avoidPenalties, "Avoid Penalties");
			});
		}

		public readonly Dictionary<string, int> versions = new Dictionary<string, int>()
		{
			// Current version at runtime
			// VersionShower::Start uses ReferenceDataManager.Refdata.userFacingVersion to get version strings such as "17.1" however that doesn't seem to before the game fully loads, so we have to use Constants::AddressablesVersion to get a less human-understandable version string
			{ $"{Constants.AddressablesVersion} (Current)", Constants.GetBroadcastVersion() },
			{ "16.1.0", 50632950 },
			{ "17.1", 50643450 },
			{ "17.1.2", 50647000 },
			{ "17.2", 50645050 },
			{ "17.2.1", 50652900 },
			{ "17.2.2", 50653700 },
			{ "17.3", 50652400 },
			{ "17.4", 50656300 }
		};

		private int versionSelection = 0;

		public override void Render()
		{
			GUILayout.Label("Version Spoofer:");
			Spoofer.shouldSpoofVersion = GUILayout.Toggle(Spoofer.shouldSpoofVersion, "Enable Version Spoofing");

			GUILayout.Label($"Spoofed Version: {versions.ElementAt(versionSelection).Key} ({Spoofer.spoofedVersion})");
			versionSelection = (int)GUILayout.HorizontalSlider(versionSelection, 0, versions.Count - 1);
			Spoofer.spoofedVersion = versions.ElementAt(versionSelection).Value;

			Spoofer.useModdedProtocol = GUILayout.Toggle(Spoofer.useModdedProtocol, "Use Modded Protocol");

			GUILayout.Space(5);
			GUILayout.Label("Level Spoofer:");

			Spoofer.SpoofLevel.Enabled = GUILayout.Toggle(Spoofer.SpoofLevel.Enabled, "Enabled");
			GUILayout.Label($"Spoofed Level: {Spoofer.SpoofLevel.newLevel}");
			Spoofer.SpoofLevel.newLevel = (uint)GUILayout.HorizontalSlider(Spoofer.SpoofLevel.newLevel, 1, 200);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("-100"))
			{
				ClampSelectedLevel(Spoofer.SpoofLevel.newLevel - 100);
			}

			if(GUILayout.Button("-10"))
			{
				ClampSelectedLevel(Spoofer.SpoofLevel.newLevel - 10);
			}

			if(GUILayout.Button("+10"))
			{
				ClampSelectedLevel(Spoofer.SpoofLevel.newLevel + 10);
			}

			if(GUILayout.Button("+100"))
			{
				ClampSelectedLevel(Spoofer.SpoofLevel.newLevel + 100);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Send Level Update"))
			{
				PlayerControl.LocalPlayer.RpcSetLevel(Spoofer.SpoofLevel.newLevel - 1);
				Hydra.notifications.Send("Level Updater", $"Your level has been changed to {Spoofer.SpoofLevel.newLevel}", 5);
			}

			GUILayout.Space(5);
			GUILayout.Label("Platform Spoofer:");

			GUILayout.Label($"Spoofed Platform: {Spoofer.spoofedPlatform}");
			Spoofer.spoofedPlatform = (Platforms)GUILayout.HorizontalSlider((float)Spoofer.spoofedPlatform, 0, 10);

			GUILayout.Space(5);
			GUILayout.Label("Penalty Bypass:");
			Spoofer.avoidPenalties = GUILayout.Toggle(Spoofer.avoidPenalties, "Avoid Disconnect Penalties");
		}

		private void ClampSelectedLevel(uint newLevel)
		{
			// Do we really need to have an upper bounds on the level value?
			// I doubt anyone will press the +100 that much anyway
			uint maxLevel = Utilities.IsAnticheatPresent() ? 100001 : uint.MaxValue - 1;

			Spoofer.SpoofLevel.newLevel = Math.Clamp(newLevel, 0, maxLevel);
		}
	}
}