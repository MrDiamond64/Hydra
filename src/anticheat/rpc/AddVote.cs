using Hazel;
using InnerNet;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class AddVote : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			int source = reader.ReadInt32();
			int target = reader.ReadInt32();

			ClientData client = AmongUsClient.Instance.FindClientById(source);
			if(client == null || client.Character == null)
			{
				Hydra.Log.LogInfo($"An unknown client id ({source}) attempted to votekick {target}");
				return false;
			}

			player = client.Character;

			if(player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to votekick a player while dead.");
				return false;
			}

			if(MeetingHud.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to votekick a player outside of a meeting.");
				return false;
			}

			return true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.AddVote;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(VoteBanSystem);
		}
	}
}