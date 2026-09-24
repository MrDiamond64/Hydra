using AmongUs.InnerNet.GameDataMessages;
using HydraMenu.anticheat;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class AnticheatSection : Section
	{
		public AnticheatSection() : base("Anticheat") { }

		public override void Render()
		{
			Anticheat.Enabled = GUILayout.Toggle(Anticheat.Enabled, "Enable Hydra Anticheat");

			// The rest of the section is only meaningful when the anticheat is enabled.
			// Grey it out visually by disabling the controls so it is obvious that nothing below will run.
			GUI.enabled = Anticheat.Enabled;

			Anticheat.CheckSpoofedPlatforms = GUILayout.Toggle(Anticheat.CheckSpoofedPlatforms, "Flag Spoofed Platform Data");

			RenderDetectionResponse();
			RenderCheckList();

			GUI.enabled = true;
		}

		// Controls for what the anticheat does once a cheater has been detected.
		private void RenderDetectionResponse()
		{
			GUILayout.Space(8);
			GUILayout.Label("When a cheater is detected:");

			Anticheat.sendNotification = GUILayout.Toggle(Anticheat.sendNotification, "Send notification");
			Anticheat.discardRpc = GUILayout.Toggle(Anticheat.discardRpc, "Discard the invalid packet");

			GUILayout.BeginHorizontal();
			GUILayout.Label($"Notification duration: {Anticheat.NotificationDuration:0.0}s");
			Anticheat.NotificationDuration = Mathf.Round(GUILayout.HorizontalSlider(Anticheat.NotificationDuration, 1.0f, 30.0f) * 2f) / 2f;
			GUILayout.EndHorizontal();

			// Strike threshold: how many times a player must be flagged before they are punished.
			// This is the main knob for trading off between catching cheaters and never punishing an innocent player.
			GUILayout.BeginHorizontal();
			string strikeLabel = Anticheat.strikesBeforePunishment == 1
				? "Strikes before punishment: 1 (punish immediately)"
				: $"Strikes before punishment: {Anticheat.strikesBeforePunishment}";
			GUILayout.Label(strikeLabel);
			Anticheat.strikesBeforePunishment = Mathf.RoundToInt(GUILayout.HorizontalSlider(Anticheat.strikesBeforePunishment, 1, Anticheat.MaxStrikeThreshold));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label($"Punish with: {PunishmentLabel(Anticheat.punishment)}");
			Anticheat.punishment = (Anticheat.Punishments)Mathf.RoundToInt(GUILayout.HorizontalSlider((float)Anticheat.punishment, 0, 3));
			GUILayout.EndHorizontal();

			// A cosmetic orbital strike that plays on a cheater right before a ban lands.
			if(Anticheat.punishment == Anticheat.Punishments.Ban)
			{
				Anticheat.orbitalStrikeOnBan = GUILayout.Toggle(Anticheat.orbitalStrikeOnBan, "Orbital strike cheaters before banning (visual)");
			}

			// Punishments only take effect for the host; make that obvious so non-host users are not confused.
			if(Anticheat.punishment != Anticheat.Punishments.None)
			{
				GUILayout.Label("Punishments only apply when you are the host. As a non-host you are still notified.");
			}
		}

		// The list of individual checks, with bulk enable/disable controls and a live enabled count.
		private void RenderCheckList()
		{
			GUILayout.Space(8);

			CountChecks(out int enabled, out int total);

			GUILayout.BeginHorizontal();
			GUILayout.Label($"Checks ({enabled}/{total} enabled)");
			if(GUILayout.Button("Enable all"))
			{
				SetAllChecks(true);
			}
			if(GUILayout.Button("Disable all"))
			{
				SetAllChecks(false);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(3);
			GUILayout.Label("RPC checks:");
			foreach((RpcCalls rpcCall, RpcCheck handler) in Anticheat.RpcHandlers)
			{
				handler.Enabled = GUILayout.Toggle(handler.Enabled, $"{rpcCall}{(handler.IsHostOnly() ? " (host-only RPC)" : "")}");
			}

			GUILayout.Space(3);
			GUILayout.Label("Game data checks:");
			foreach((GameDataTypes type, GameDataCheck handler) in Anticheat.GameDataHandlers)
			{
				handler.Enabled = GUILayout.Toggle(handler.Enabled, $"{type}");
			}
		}

		private static void CountChecks(out int enabled, out int total)
		{
			enabled = 0;
			total = 0;

			foreach(RpcCheck handler in Anticheat.RpcHandlers.Values)
			{
				total++;
				if(handler.Enabled) enabled++;
			}

			foreach(GameDataCheck handler in Anticheat.GameDataHandlers.Values)
			{
				total++;
				if(handler.Enabled) enabled++;
			}
		}

		private static void SetAllChecks(bool value)
		{
			foreach(RpcCheck handler in Anticheat.RpcHandlers.Values)
			{
				handler.Enabled = value;
			}

			foreach(GameDataCheck handler in Anticheat.GameDataHandlers.Values)
			{
				handler.Enabled = value;
			}
		}

		private static string PunishmentLabel(Anticheat.Punishments punishment)
		{
			return punishment switch
			{
				Anticheat.Punishments.None => "Notify only (no punishment)",
				Anticheat.Punishments.Kick => "Kick",
				Anticheat.Punishments.ErrorKick => "Silent kick (fake timeout)",
				Anticheat.Punishments.Ban => "Ban from lobby",
				_ => punishment.ToString()
			};
		}
	}
}
