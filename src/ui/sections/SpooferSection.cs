using HydraMenu.features;
using HydraMenu.modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SpooferSection : Section
	{
		public SpooferSection() : base("Spoofer") { }

		public readonly Dictionary<string, int> versions = new Dictionary<string, int>()
		{
			// Current version at runtime
			// VersionShower::Start uses ReferenceDataManager.Refdata.userFacingVersion to get version strings such as "17.1"
			// however that doesn't seem to before the game fully loads, so we have to use Constants::AddressablesVersion to get a less human-understandable version string
			{ $"{Constants.AddressablesVersion} (Current)", Constants.GetBroadcastVersion() },
			{ "16.1.0", 50632950 },
			{ "17.1", 50643450 },
			{ "17.1.2", 50647000 },
			{ "17.2", 50645050 },
			{ "17.2.1", 50652900 },
			{ "17.2.2", 50653700 },
			{ "17.3", 50652400 },
			{ "17.4", 50656300 },
			{ "18.0", 50663350 }
		};

		private int versionSelection = 0;

		public override void Render()
		{
			GUILayout.Label("Version Spoofer:");
			ModuleManager.spoofVersion.Enabled = GUILayout.Toggle(ModuleManager.spoofVersion.Enabled, "Enable Version Spoofing");

			GUILayout.Label($"Spoofed Version: {versions.ElementAt(versionSelection).Key} ({ModuleManager.spoofVersion.spoofedVersion})");
			versionSelection = (int)GUILayout.HorizontalSlider(versionSelection, 0, versions.Count - 1);
			ModuleManager.spoofVersion.spoofedVersion = versions.ElementAt(versionSelection).Value;

			ModuleManager.spoofVersion.useModdedProtocol = GUILayout.Toggle(ModuleManager.spoofVersion.useModdedProtocol, "Use Modded Protocol");

			GUILayout.Space(5);
			GUILayout.Label("Level Spoofer:");

			ModuleManager.spoofLevel.Enabled = GUILayout.Toggle(ModuleManager.spoofLevel.Enabled, "Enabled");
			GUILayout.Label($"Spoofed Level: {ModuleManager.spoofLevel.spoofedLevel}");
			ModuleManager.spoofLevel.spoofedLevel = (uint)GUILayout.HorizontalSlider(ModuleManager.spoofLevel.spoofedLevel, 1, 200);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("-100"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.spoofedLevel - 100);
			}

			if(GUILayout.Button("-10"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.spoofedLevel - 10);
			}

			if(GUILayout.Button("+10"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.spoofedLevel + 10);
			}

			if(GUILayout.Button("+100"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.spoofedLevel + 100);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Send Level Update"))
			{
				PlayerControl.LocalPlayer.RpcSetLevel(ModuleManager.spoofLevel.spoofedLevel - 1);
				Hydra.notifications.Send("Level Updater", $"Your level has been changed to {ModuleManager.spoofLevel.spoofedLevel}", 5);
			}

			GUILayout.Space(5);
			GUILayout.Label("Platform Spoofer:");

			GUILayout.Label($"Spoofed Platform: {ModuleManager.spoofDevice.spoofedPlatform}");
			ModuleManager.spoofDevice.spoofedPlatform = (Platforms)GUILayout.HorizontalSlider((float)ModuleManager.spoofDevice.spoofedPlatform, 0, 10);
		}

		private void ClampSelectedLevel(uint newLevel)
		{
			// Do we really need to have an upper bounds on the level value?
			// I doubt anyone will press the +100 that much anyway
			uint maxLevel = Utilities.IsAnticheatPresent() ? 100001 : uint.MaxValue - 1;

			ModuleManager.spoofLevel.spoofedLevel = Math.Clamp(newLevel, 0, maxLevel);
		}
	}
}