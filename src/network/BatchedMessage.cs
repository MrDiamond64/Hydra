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
		private int msgCount = 0;

		public BatchedMessage(int targetClientId = (int)Constants.OwnerIds.Everyone)
		{
			writer = MessageWriter.Get(SendOption.Reliable);

			this.targetClientId = targetClientId;
			if(targetClientId == (int)Constants.OwnerIds.Everyone)
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

		private bool IsGlobal
		{
			get { return targetClientId == (int)Constants.OwnerIds.Everyone; }
		}

		private bool AmTarget
		{
			get { return targetClientId == AmongUsClient.Instance.ClientId; }
		}

		public void QueueDataFlag(uint netId, MessageWriter msg)
		{
			msgCount++;

			writer.StartMessage((byte)GameDataTypes.DataFlag);
			writer.WritePacked(netId);
			writer.Write(msg, false);
			writer.EndMessage();
		}

		public void QueueSpawn(InnerNetObject netObject, int ownerId = -2, SpawnFlags flags = SpawnFlags.None)
		{
			msgCount++;

			SpawnGameDataMessage spawn = AmongUsClient.Instance.CreateSpawnMessage(netObject, ownerId, flags);
			spawn.Serialize(writer);
		}

		public void QueueCompleteTask(PlayerControl source, uint taskIndex)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.CompleteTask(taskIndex);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.CompleteTask);
			writer.WritePacked(taskIndex);
			writer.EndMessage();
		}

		public void QueueSetName(PlayerControl source, string name)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.SetName(name);
				if(AmTarget) return;
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
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.SetColor(color);
				if(AmTarget) return;
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
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.MurderPlayer(target, result);
				if(AmTarget) return;
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
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.NetTransform.SnapTo(position, (ushort)(source.NetTransform.lastSequenceId + 1));
				if(AmTarget) return;
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
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				MeetingHud.Instance.Close();
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(MeetingHud.Instance.NetId);
			writer.Write((byte)RpcCalls.CloseMeeting);
			writer.EndMessage();
		}

		public void QueueVotingComplete(MeetingHud.VoterState[] voteStates, NetworkedPlayerInfo ejectedPlayer, bool isTie, bool wasOverruled, ushort overruleNonce)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				MeetingHud.Instance.VotingComplete(voteStates, ejectedPlayer, isTie, wasOverruled, overruleNonce);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(MeetingHud.Instance.NetId);
			writer.Write((byte)RpcCalls.VotingComplete);

			writer.WritePacked(voteStates.Length);

			foreach(MeetingHud.VoterState state in voteStates)
			{
				state.Serialize(writer);
			}

			writer.Write(ejectedPlayer != null ? ejectedPlayer.PlayerId : byte.MaxValue);
			writer.Write(isTie);
			writer.Write(wasOverruled);
			writer.Write(overruleNonce);

			writer.EndMessage();
		}

		public void QueueAddVote(int sourceId, int targetId)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				VoteBanSystem.Instance.AddVote(sourceId, targetId);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(VoteBanSystem.Instance.NetId);
			writer.Write((byte)RpcCalls.AddVote);
			writer.Write(sourceId);
			writer.Write(targetId);
			writer.EndMessage();
		}

		public void QueueCloseDoors(SystemTypes door)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				ShipStatus.Instance.CloseDoorsOfType(door);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(ShipStatus.Instance.NetId);
			writer.Write((byte)RpcCalls.CloseDoorsOfType);
			writer.Write((byte)door);
			writer.EndMessage();
		}

		public void QueueUpdateSystem(PlayerControl source, SystemTypes system, MessageWriter msg)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				MessageReader reader = MessageReader.Get(msg.ToByteArray(false));
				ShipStatus.Instance.UpdateSystem(system, source, reader);

				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(ShipStatus.Instance.NetId);
			writer.Write((byte)RpcCalls.UpdateSystem);
			writer.Write((byte)system);
			writer.WriteNetObject(source);
			writer.Write(msg, false);
			writer.EndMessage();
		}

		public void QueueSetHatStr(PlayerControl source, string hat, byte seqId)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.SetHat(hat, source.Data.DefaultOutfit.ColorId);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetHatStr);
			writer.Write(hat);
			writer.Write(seqId);
			writer.EndMessage();
		}

		public void QueueSetSkinStr(PlayerControl source, string skin, byte seqId)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.SetSkin(skin, source.Data.DefaultOutfit.ColorId);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetSkinStr);
			writer.Write(skin);
			writer.Write(seqId);
			writer.EndMessage();
		}

		public void QueueSetPetStr(PlayerControl source, string pet, byte seqId)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.SetPet(pet, source.Data.DefaultOutfit.ColorId);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetPetStr);
			writer.Write(pet);
			writer.Write(seqId);
			writer.EndMessage();
		}

		public void QueueSetVisorStr(PlayerControl source, string visor, byte seqId)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.SetVisor(visor, source.Data.DefaultOutfit.ColorId);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetVisorStr);
			writer.Write(visor);
			writer.Write(seqId);
			writer.EndMessage();
		}

		public void QueueSetNameplateStr(PlayerControl source, string nameplate, byte seqId)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.SetNamePlate(nameplate);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SetNamePlateStr);
			writer.Write(nameplate);
			writer.Write(seqId);
			writer.EndMessage();
		}

		public void QueueSetRole(PlayerControl source, RoleTypes role, bool canOverride = false)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.StartCoroutine(source.CoSetRole(role, canOverride));
				if(AmTarget) return;
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
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				source.Shapeshift(target, shouldAnimate);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.Shapeshift);
			writer.WriteNetObject(target);
			writer.Write(shouldAnimate);
			writer.EndMessage();
		}

		public void QueueTriggerSpore(PlayerControl source, Mushroom mushroom)
		{
			msgCount++;

			if(IsGlobal || AmTarget)
			{
				mushroom.TriggerSpores();
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.TriggerSpores);
			writer.Write(mushroom.Id);
			writer.EndMessage();
		}

		public void FinishBatch()
		{
			writer.EndMessage();
			if(msgCount > 0)
			{
				AmongUsClient.Instance.SendOrDisconnect(writer);
			}
			writer.Recycle();
		}
	}
}