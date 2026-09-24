using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;
using HydraMenu.anticheat.gamedata;
using HydraMenu.anticheat.rpc;
using HydraMenu.modules;
using System;
using System.Collections.Generic;

namespace HydraMenu.anticheat
{
	/// <summary>
	/// The core of Hydra Anticheat. Intercepts incoming RPCs and game data messages, hands them to the
	/// relevant <see cref="RpcCheck"/> / <see cref="GameDataCheck"/> for validation, and decides what to
	/// do when a check flags a player.
	///
	/// Detection is decoupled from enforcement: any check can call <see cref="Flag(PlayerControl, string, bool)"/>,
	/// but a player is only ever punished when we are the host. To avoid penalising innocent players for a
	/// single ambiguous detection, punishment is gated behind a configurable strike threshold
	/// (<see cref="strikesBeforePunishment"/>) that accumulates over the course of a game.
	/// </summary>
	internal class Anticheat
	{
		/// <summary>Master switch for the whole anticheat. When false, no checks run and no RPCs are discarded.</summary>
		public static bool Enabled { get; set; } = true;

		public static readonly Dictionary<GameDataTypes, GameDataCheck> GameDataHandlers = new Dictionary<GameDataTypes, GameDataCheck>()
		{
			{ GameDataTypes.SceneChangeFlag, new SceneChange() },
			{ GameDataTypes.ReadyFlag, new ClientReady() }
		};

		public static readonly Dictionary<RpcCalls, RpcCheck> RpcHandlers = new Dictionary<RpcCalls, RpcCheck>()
		{
			// RPC handlers in this dictionary should be sorted by their RPC ID
			{ RpcCalls.PlayAnimation, new PlayAnimation() },
			{ RpcCalls.CompleteTask, new CompleteTask() },
			{ RpcCalls.Exiled, new Exiled() },
			{ RpcCalls.CheckName, new CheckName() },
			{ RpcCalls.SetName, new SetName() },
			{ RpcCalls.SetColor, new SetColor() },
			{ RpcCalls.ReportDeadBody, new ReportDeadBody() },
			{ RpcCalls.SetScanner, new SetScanner() },
			{ RpcCalls.SetStartCounter, new SetStartCounter() },
			{ RpcCalls.EnterVent, new EnterVent() },
			{ RpcCalls.ExitVent, new ExitVent() },
			{ RpcCalls.SnapTo, new SnapTo() },
			{ RpcCalls.AddVote, new AddVote() },
			{ RpcCalls.CloseDoorsOfType, new CloseDoorsOfType() },
			{ RpcCalls.ClimbLadder, new ClimbLadder() },
			{ RpcCalls.UsePlatform, new UsePlatform() },
			{ RpcCalls.UpdateSystem, new UpdateSystem() },
			{ RpcCalls.SetLevel, new SetLevel() }
		};

		public static bool CheckSpoofedPlatforms { get; set; } = true;

		public enum Punishments
		{
			None,
			Kick,
			ErrorKick,
			Ban
		}

		public static float NotificationDuration = 10.0f;

		public static Punishments punishment = Punishments.None;
		public static bool sendNotification = true;
		public static bool discardRpc = true;

		// The number of times a single player must be flagged before a punishment is applied to them.
		// A value of 1 preserves the original behaviour of punishing on the very first flag.
		// Higher values make the anticheat more forgiving: a player has to trip multiple checks (or the
		// same check repeatedly) before being kicked or banned, which greatly reduces the chance of
		// punishing an innocent player over one ambiguous detection while still catching real cheaters
		// who tend to trip many checks in quick succession.
		public static int strikesBeforePunishment = 1;

		// The maximum value the strike threshold can be set to from the UI or a config file.
		public const int MaxStrikeThreshold = 10;

		// Tracks how many times each player has been flagged during the current game, keyed by OwnerId.
		// Strikes are reset at the start of every game and whenever we leave a lobby so detections from
		// one round never carry over into the next (see ResetStrikes / Initialize).
		private static readonly Dictionary<int, int> playerStrikes = new Dictionary<int, int>();

		/// <summary>
		/// Subscribes the anticheat to the lifecycle events it needs. Call this once during plugin load.
		/// </summary>
		public static void Initialize()
		{
			EventCoordinator.OnGameStart += ResetStrikes;
			EventCoordinator.OnDisconnect += ResetStrikes;
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
		class OnPlayerControlRPC
		{
			static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(PlayerControl), __instance, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
		class OnPlayerPhysicsRPC
		{
			static bool Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(PlayerPhysics), __instance.myPlayer, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		class OnNetTransformRPC
		{
			static bool Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(CustomNetworkTransform), __instance.myPlayer, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(ShipStatus), null, (RpcCalls)callId, reader);
			}
		}

		private static bool HandleRpc(Type sourceNetObj, PlayerControl player, RpcCalls rpc, MessageReader reader)
		{
			RpcHandlers.TryGetValue(rpc, out RpcCheck rpcCheck);
			if(!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

			if(sourceNetObj != rpcCheck.GetExpectedNetObject())
			{
				// Received an RPC that should've been sent for a different net object, some sort of exploit attempt?
				return false;
			}

			// Only we, the host, should be sending host-only RPCs
			if(player != null && AmongUsClient.Instance.AmHost && rpcCheck.IsHostOnly())
			{
				Flag(player, $"{player.Data.PlayerName} sent the {rpc} RPC while non-host.");
				return false;
			}

			int oldReadPosition = reader.Position;

			bool isValid = rpcCheck.Validate(player, reader);

			// Put the read position back to its previous spot to not mess up the HandleRpc function or other patches
			reader.Position = oldReadPosition;

			return isValid || !discardRpc;
		}

		public static bool HandleGameData(GameDataTypes type, MessageReader reader)
		{
			GameDataHandlers.TryGetValue(type, out GameDataCheck gameDataCheck);
			if(!Enabled || gameDataCheck == null || !gameDataCheck.Enabled) return true;

			int oldReadPosition = reader.Position;

			bool isValid = gameDataCheck.Validate(reader);

			// Put the read position back to its previous spot
			reader.Position = oldReadPosition;

			return isValid || !discardRpc;
		}

		/// <summary>
		/// Records a detection against a specific player and, when we are the host, punishes them once they
		/// reach the configured strike threshold.
		/// </summary>
		/// <param name="player">The player that tripped the check.</param>
		/// <param name="reason">A human-readable description of what was detected, shown in the notification.</param>
		/// <param name="shouldPunish">
		/// When false the detection is only ever reported and never counts towards a strike or a punishment.
		/// Use this for low-confidence checks that should inform the host without ever acting on their own.
		/// </param>
		public static void Flag(PlayerControl player, string reason, bool shouldPunish = true)
		{
			// Sanity check, make sure that we are not flagging ourselves
			// On servers without net object impersonation checks, it may be possible to send an invalid RPC on the behalf of the host
			// which would result in Hydra Anticheat flagging ourselves and banning us from our own lobby
			if(player == PlayerControl.LocalPlayer) return;

			// Only the host can actually enforce a punishment, so strikes are only meaningful for the host.
			bool canPunish = AmongUsClient.Instance.AmHost && shouldPunish;
			int strikes = 0;

			if(canPunish)
			{
				strikes = RegisterStrike(player);
			}

			if(sendNotification)
			{
				string message = reason;

				// Let the host see how close a player is to being punished when using a strike threshold,
				// so an escalating cheater is obvious before the punishment actually lands.
				if(canPunish && strikesBeforePunishment > 1)
				{
					message += $" (strike {strikes}/{strikesBeforePunishment})";
				}

				Hydra.notifications.Send("Anticheat", message, NotificationDuration);
			}

			if(canPunish && strikes >= strikesBeforePunishment)
			{
				Punish(player);
			}
		}

		// If we do not know which player caused the violation
		public static void Flag(string reason)
		{
			if(sendNotification)
			{
				Hydra.notifications.Send("Anticheat", reason, NotificationDuration);
			}
		}

		/// <summary>
		/// Increments and returns the strike count for a player for the current game.
		/// </summary>
		private static int RegisterStrike(PlayerControl player)
		{
			int ownerId = player.OwnerId;

			playerStrikes.TryGetValue(ownerId, out int strikes);
			strikes++;
			playerStrikes[ownerId] = strikes;

			return strikes;
		}

		/// <summary>
		/// Clears every player's accumulated strikes. Called automatically when a new game starts or when we
		/// leave a lobby so that detections do not carry over between rounds.
		/// </summary>
		public static void ResetStrikes()
		{
			playerStrikes.Clear();
		}

		private static void Punish(PlayerControl player)
		{
			switch(punishment)
			{
				case Punishments.None:
					break;

				case Punishments.Kick:
				case Punishments.ErrorKick:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} was kicked by Hydra Anticheat for hacking");

					// The vanilla anticheat prevents using the ErrorKick method if the game has not started yet
					if(punishment == Punishments.Kick || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
					{
						AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
					}
					else
					{
						// When a game starts, the host waits around ten seconds to wait for all clients to send the ClientReady game message
						// If the ten-second timer is reached without a ClientReady game message being received by the host, the host will kick the player due to timeout
						// The kick message shown to the player will explain that the player has a poor internet connection or that their device is too old
						// and in-game, players will be shown that the player left due to an error instead of being kicked
						// Any other disconnection messages other than ClientTimeout will result in the vanilla anticheat kicking us from the lobby
						AmongUsClient.Instance.SendLateRejection(player.OwnerId, DisconnectReasons.ClientTimeout);
					}
					break;

				case Punishments.Ban:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} was automatically banned by Hydra Anticheat for hacking");
					AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
					break;
			}
		}

		public class AnticheatConfigData
		{
			public bool AcEnabled { get; set; }
			public bool SendNotification { get; set; }
			public bool DiscardRpc { get; set; }
			public Punishments Punishment { get; set; }
			public int StrikesBeforePunishment { get; set; } = 1;
		}

		public static AnticheatConfigData GetConfigData()
		{
			return new AnticheatConfigData
			{
				AcEnabled = Enabled,
				SendNotification = sendNotification,
				DiscardRpc = discardRpc,
				Punishment = punishment,
				StrikesBeforePunishment = strikesBeforePunishment,
			};
		}

		public static void LoadConfigData(AnticheatConfigData configData)
		{
			if(configData == null) return;

			Enabled = configData.AcEnabled;
			sendNotification = configData.SendNotification;
			discardRpc = configData.DiscardRpc;
			punishment = configData.Punishment;
			// Clamp on load so a hand-edited config can never disable punishment entirely (0) or set an absurd threshold.
			strikesBeforePunishment = Math.Clamp(configData.StrikesBeforePunishment, 1, MaxStrikeThreshold);
		}
	}
}