using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;
using UnityEngine;

namespace HydraMenu.network
{
	internal class BatchedMessage
	{
		public readonly MessageWriter writer;
		public readonly int targetClientId;

		public BatchedMessage(int targetClientId = -1)
		{
			writer = MessageWriter.Get(SendOption.Reliable);

			this.targetClientId = targetClientId;
			if(targetClientId == -1)
			{
				writer.StartMessage(InnerNet.Tags.GameData);
				writer.Write(AmongUsClient.Instance.GameId);
			}
			else
			{
				writer.StartMessage(InnerNet.Tags.GameDataTo);
				writer.Write(AmongUsClient.Instance.GameId);
				writer.WritePacked(targetClientId);
			}
		}

		private bool AmTarget
		{
			get { return targetClientId == -1 || targetClientId == AmongUsClient.Instance.ClientId; }
		}

		public void QueueDataFlag(uint netId, MessageWriter msg)
		{
			writer.StartMessage((byte)GameDataTypes.DataFlag);
			writer.WritePacked(netId);
			writer.Write(msg, false);
			writer.EndMessage();
		}

		public void QueueSpawn(InnerNetObject netObject, int ownerId = -2, SpawnFlags flags = SpawnFlags.None)
		{
			SpawnGameDataMessage spawn = AmongUsClient.Instance.CreateSpawnMessage(netObject, ownerId, flags);
			spawn.Serialize(writer);
		}

		public void QueueSetName(PlayerControl source, string name)
		{
			if(AmTarget)
			{
				source.SetName(name);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetName);
			writer.Write(source.NetId);
			writer.Write(name);
			writer.EndMessage();
		}

		public void QueueSetColor(PlayerControl source, byte color)
		{
			if(AmTarget)
			{
				source.SetColor(color);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetColor);
			writer.Write(source.Data.NetId);
			writer.Write(color);
			writer.EndMessage();
		}

		public void QueueMurderPlayer(PlayerControl source, PlayerControl target, MurderResultFlags result)
		{
			if(AmTarget)
			{
				source.MurderPlayer(target, result);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.MurderPlayer);
			writer.WritePacked(target.NetId);
			writer.Write((int)result);
			writer.EndMessage();
		}

		public void QueueSnapTo(PlayerControl source, Vector2 position)
		{
			if(AmTarget)
			{
				source.NetTransform.SnapTo(position, (ushort)(source.NetTransform.lastSequenceId + 1));
			}

			ushort seqId = (ushort)(source.NetTransform.lastSequenceId + 2);

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetTransform.NetId);
			writer.Write((byte)RpcCalls.SnapTo);
			NetHelpers.WriteVector2(position, writer);
			writer.Write(seqId);
			writer.EndMessage();
		}

		public void QueueCloseMeeting()
		{
			if(AmTarget)
			{
				MeetingHud.Instance.Close();
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(MeetingHud.Instance.NetId);
			writer.Write((byte)RpcCalls.CloseMeeting);
			writer.EndMessage();
		}

		public void QueueVotingComplete(MeetingHud.VoterState[] voteStates, NetworkedPlayerInfo ejectedPlayer, bool isTie)
		{
			if(AmTarget)
			{
				MeetingHud.Instance.VotingComplete(voteStates, ejectedPlayer, isTie);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(MeetingHud.Instance.NetId);
			writer.Write((byte)RpcCalls.VotingComplete);

			writer.WritePacked(voteStates.Length);

			foreach(MeetingHud.VoterState state in voteStates)
			{
				state.Serialize(writer);
			}

			writer.Write(ejectedPlayer.PlayerId);
			writer.Write(isTie);

			writer.EndMessage();
		}

		public void QueueAddVote(int sourceId, int targetId)
		{
			if(AmTarget)
			{
				VoteBanSystem.Instance.AddVote(sourceId, targetId);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(VoteBanSystem.Instance.NetId);
			writer.Write((byte)RpcCalls.AddVote);
			writer.Write(sourceId);
			writer.Write(targetId);
			writer.EndMessage();
		}

		public void QueueUpdateSystem(PlayerControl source, SystemTypes system, MessageWriter msg)
		{
			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(ShipStatus.Instance.NetId);
			writer.Write((byte)RpcCalls.UpdateSystem);
			writer.Write((byte)system);
			writer.WriteNetObject(source);
			writer.Write(msg, false);
			writer.EndMessage();
		}

		public void QueueSetHatStr(PlayerControl source, string hat, byte seqid)
		{
			if(AmTarget)
			{
				source.SetHat(hat, source.Data.DefaultOutfit.ColorId);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetHatStr);
			writer.Write(hat);
			writer.Write(seqid);
			writer.EndMessage();
		}

		public void QueueSetSkinStr(PlayerControl source, string skin, byte seqid)
		{
			if(AmTarget)
			{
				source.SetSkin(skin, source.Data.DefaultOutfit.ColorId);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetSkinStr);
			writer.Write(skin);
			writer.Write(seqid);
			writer.EndMessage();
		}

		public void QueueSetPetStr(PlayerControl source, string pet, byte seqid)
		{
			if(AmTarget)
			{
				source.SetPet(pet, source.Data.DefaultOutfit.ColorId);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetPetStr);
			writer.Write(pet);
			writer.Write(seqid);
			writer.EndMessage();
		}

		public void QueueSetVisorStr(PlayerControl source, string visor, byte seqid)
		{
			if(AmTarget)
			{
				source.SetVisor(visor, source.Data.DefaultOutfit.ColorId);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetVisorStr);
			writer.Write(visor);
			writer.Write(seqid);
			writer.EndMessage();
		}

		public void QueueSetNameplateStr(PlayerControl source, string nameplate, byte seqid)
		{
			if(AmTarget)
			{
				source.SetNamePlate(nameplate);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetNamePlateStr);
			writer.Write(nameplate);
			writer.Write(seqid);
			writer.EndMessage();
		}

		public void QueueSetRole(PlayerControl source, RoleTypes role, bool canOverride = false)
		{
			if(AmTarget)
			{
				source.StartCoroutine(source.CoSetRole(role, canOverride));
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetRole);
			writer.Write((ushort)role);
			writer.Write(canOverride);
			writer.EndMessage();
		}

		public void QueueShapeshift(PlayerControl source, PlayerControl target, bool shouldAnimate)
		{
			if(AmTarget)
			{
				source.Shapeshift(target, shouldAnimate);
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.Shapeshift);
			writer.WriteNetObject(target);
			writer.Write(shouldAnimate);
			writer.EndMessage();
		}

		public void FinishBatch()
		{
			writer.EndMessage();
			AmongUsClient.Instance.SendOrDisconnect(writer);
			writer.Recycle();
		}
	}
}