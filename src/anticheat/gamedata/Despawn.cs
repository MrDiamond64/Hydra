using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using HydraMenu.network;
using InnerNet;

namespace HydraMenu.anticheat.gamedata
{
	internal class Despawn : GameDataCheck
	{
		public override bool Validate(MessageReader reader)
		{
			uint netId = reader.ReadPackedUInt32();
			InnerNetObject netObject = AmongUsClient.Instance.FindObjectByNetId<InnerNetObject>(netId);
			if(netObject == null) return true;

			// Only the host should be despawning net objects
			if(AmongUsClient.Instance.AmHost)
			{
				Anticheat.Flag($"Received despawn for net object {(SpawnType)netObject.SpawnId} (net id: {netObject.NetId}) while host.");
				return false;
			}

			// Nobody should be despawning net objects we own (our PlayerControl, PlayerPhysics, or CustomNetworkTransform)
			bool isModded = Utilities.IsAnticheatPresent();
			if(!isModded && netObject.AmOwner)
			{
				Anticheat.Flag($"Received despawn for net object {(SpawnType)netObject.SpawnId} (net id: {netObject.NetId}) which we own.");
				return false;
			}

			// The only time a net object is despawned in vanilla lobbies is when the host despawns the LobbyBehavior when the game starts
			if(!isModded && netObject.SpawnId != (uint)SpawnType.LobbyBehavior)
			{
				Anticheat.Flag($"Received despawn for unexpected net object {(SpawnType)netObject.SpawnId} (net id: {netObject.NetId}).");
				return false;
			}

			return true;
		}

		public override GameDataTypes GetId()
		{
			return GameDataTypes.DespawnFlag;
		}
	}
}