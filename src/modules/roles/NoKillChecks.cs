using HarmonyLib;
using LunarMenu.network;

namespace LunarMenu.modules.roles
{
	internal class NoKillChecks : Module
	{
		public NoKillChecks() : base("NoKillChecks") { }

		private static NoKillChecks Instance
		{
			get { return ModuleManager.noKillChecks; }
		}

		public bool KillOtherImpostors { get; set; } = false;
		public bool KillAsPhantom { get; set; } = false;
		public bool NoKillCooldown { get; set; } = false;
		public bool KillGhosts { get; set; } = false;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
		class KillTimer
		{
			static void Prefix(PlayerControl __instance, ref float time)
			{
				if(!Instance.Enabled || !Instance.NoKillCooldown || __instance != PlayerControl.LocalPlayer) return;

				time = 0.0f;
			}
		}

		[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
		class NoImpKillChecks
		{
			static bool Prefix(NetworkedPlayerInfo target, ref bool __result)
			{
				if(!Instance.Enabled) return true;

				__result = IsValidTarget(target);
				return false;
			}
		}

		[HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.IsValidTarget))]
		class NoPhantomKillChecks
		{
			static bool Prefix(PhantomRole __instance, NetworkedPlayerInfo target, ref bool __result)
			{
				if(!Instance.Enabled) return true;

				__result = IsValidTarget(target) && (!__instance.isInvisible || Instance.KillAsPhantom);
				return false;
			}
		}

		private static bool IsValidTarget(NetworkedPlayerInfo target)
		{
			return target != null &&
			       target != PlayerControl.LocalPlayer.Data &&
			       !target.Disconnected &&
			       (!target.IsDead || Instance.KillGhosts) &&
			       (!RoleManager.IsImpostorRole(target.RoleType) || Instance.KillOtherImpostors);
		}

		// The CheckMurder RPC handler has checks against killing ghosts
		// so we need to directly send the MurderPlayer RPC to get around it
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
		class KillBypass
		{
			static bool Prefix(PlayerControl __instance, PlayerControl target)
			{
				if(!Instance.Enabled || (!AmongUsClient.Instance.AmHost && Utilities.IsAnticheatPresent())) return true;

				__instance.RpcMurderPlayer(target, true);
				return false;
			}
		}

		// The backend AU servers rely on the CheckVanish and CheckAppear RPCs to know if a player has vanished
		// This information is then used by the server's CheckMurder RPC handler to know if a kill should be authorized
		// No idea why, but Innersloth has secured the CheckVanish and CheckAppear RPCs to hell the Vanish and Appear RPCs have almost-zero protection
		// Sending CheckVanish in the lobby, as non-phantom, or while already phantomed, in cooldown, or with the maxDuration field mismatched to the game settings phantom duration will result in a ban
		// Sending CheckAppear in the lobby, as non-phantom, or while non-vanished, will also result in a kick from the lobby
		// But sending Vanish or Appear in any of those conditions will result in the RPC being relayed to other players
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
		class VanishBypass
		{
			static bool Prefix(PlayerControl __instance)
			{
				if(!Instance.Enabled || !Instance.KillAsPhantom) return true;

				BatchedMessage batch = new BatchedMessage();
				batch.QueueVanish(__instance);
				batch.FinishBatch();
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
		class AppearBypass
		{
			static bool Prefix(PlayerControl __instance, bool shouldAnimate)
			{
				if(!Instance.Enabled || !Instance.KillAsPhantom) return true;

				BatchedMessage batch = new BatchedMessage();
				batch.QueueAppear(__instance,  shouldAnimate);
				batch.FinishBatch();
				return false;
			}
		}
	}
}