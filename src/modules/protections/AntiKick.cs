using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;

namespace HydraMenu.modules.protections
{
	internal class AntiKick : Module
	{
		public AntiKick() : base("AntiKick")
		{
			base.Enabled = true;
		}

		private static AntiKick Instance
		{
			get { return ModuleManager.antiKick; }
		}

		public bool BlockVotekicks { get; set; } = true;
		public bool BlockHostOnlyRpcExploit { get; set; } = true;

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				if(!Instance.Enabled || !Instance.BlockHostOnlyRpcExploit || callId != (byte)RpcCalls.UpdateSystem) return true;
				int oldReadPosition = reader.Position;

				SystemTypes system = (SystemTypes)reader.ReadByte();
				PlayerControl player = reader.ReadNetObject<PlayerControl>();

				if(system == SystemTypes.Ventilation && !AmongUsClient.Instance.AmHost)
				{
					Hydra.notifications.Send("Protections Alert", $"{player.Data.PlayerName} attempted to use the VentilationSystem kick exploit on you!");
					return false;
				}

				reader.Position = oldReadPosition;
				return true;
			}
		}

		private void OnPlayerVotekick(ClientData source, ClientData target)
		{
			if(!BlockVotekicks || !AmongUsClient.Instance.AmHost || source.Id != AmongUsClient.Instance.ClientId) return;

			// Remove our votes, as if nothing ever happened
			VoteBanSystem.Instance.Votes[AmongUsClient.Instance.ClientId] = new Il2CppStructArray<int>(0);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerVotekick += OnPlayerVotekick;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerVotekick -= OnPlayerVotekick;
		}
	}
}