using LunarMenu.modules;
using UnityEngine;

namespace LunarMenu.ui.sections
{
	internal class ProtectionsSection : Section
	{
		public ProtectionsSection() : base("Protections") { }

		public override void Render()
		{
			// Network
			ModuleManager.forceDtls.Enabled = GUILayout.Toggle(ModuleManager.forceDtls.Enabled, "Force enable DTLS to encrypt network data");

			ModuleManager.bypassDisconnectPenalty.Enabled = GUILayout.Toggle(ModuleManager.bypassDisconnectPenalty.Enabled, "Bypass disconnection penalties");
			ModuleManager.blockServerTeleports.Enabled = GUILayout.Toggle(ModuleManager.blockServerTeleports.Enabled, "Block position updates from server");
			ModuleManager.blockUnauthorizedUpdates.Enabled = GUILayout.Toggle(ModuleManager.blockUnauthorizedUpdates.Enabled, "Block unauthorized system updates");
			ModuleManager.bypassShapeshiftRatelimits.Enabled = GUILayout.Toggle(ModuleManager.bypassShapeshiftRatelimits.Enabled, "Bypass ratelimits for Shapeshift RPC");

			ModuleManager.antiCrash.Enabled = GUILayout.Toggle(ModuleManager.antiCrash.Enabled, "Protect against ReportDeadBody lobby crash exploit");

			// Overloads
			GUILayout.Space(5);
			GUILayout.Label("Anti Overload:");
			ModuleManager.antiOverload.Enabled = GUILayout.Toggle(ModuleManager.antiOverload.Enabled, "Enabled");
			ModuleManager.antiOverload.BlockLargeGameMessages = GUILayout.Toggle(ModuleManager.antiOverload.BlockLargeGameMessages, "Block large game messages");
			ModuleManager.antiOverload.BlockInvalidGameDataMessages = GUILayout.Toggle(ModuleManager.antiOverload.BlockInvalidGameDataMessages, "Block invalid game data message types");
			ModuleManager.antiOverload.HardenedPacketUIntDeserializer = GUILayout.Toggle(ModuleManager.antiOverload.HardenedPacketUIntDeserializer, "Use hardened packed int deserializer");
			ModuleManager.antiOverload.BlockVotingCompleteOverload = GUILayout.Toggle(ModuleManager.antiOverload.BlockVotingCompleteOverload, "Protect against VotingComplete overloads");

			// Kick Exploits
			GUILayout.Space(5);
			GUILayout.Label("Anti Kick:");
			ModuleManager.antiKick.Enabled = GUILayout.Toggle(ModuleManager.antiKick.Enabled, "Enabled");
			ModuleManager.antiKick.BlockVotekicks = GUILayout.Toggle(ModuleManager.antiKick.BlockVotekicks, "Protect against votekicks as host");
			ModuleManager.antiKick.BlockHostOnlyRpcExploit = GUILayout.Toggle(ModuleManager.antiKick.BlockHostOnlyRpcExploit, "Protect against host-only RPC kick exploit");
		}
	}
}