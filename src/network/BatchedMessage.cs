using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;
using UnityEngine;

namespace LunarMenu.network
{
	internal class BatchedMessage
	{
		public readonly MessageWriter writer;
		public readonly int targetClientId;
		public int msgCount = 0;

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
			writer.StartMessage((byte)GameDataTypes.DataFlag);
			writer.WritePacked(netId);
			writer.Write(msg, false);
			writer.EndMessage();

			msgCount++;
		}

		public void QueueSpawn(InnerNetObject netObject, int ownerId = (int)Constants.OwnerIds.Host, SpawnFlags flags = SpawnFlags.None)
		{
			SpawnGameDataMessage spawn = AmongUsClient.Instance.CreateSpawnMessage(netObject, ownerId, flags);
			spawn.Serialize(writer);

			msgCount++;
		}

		public void QueueCompleteTask(PlayerControl source, uint taskIndex)
		{
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

			msgCount++;
		}

		public void QueueSetName(PlayerControl source, string name)
		{
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

			msgCount++;
		}

		public void QueueSetColor(PlayerControl source, byte color)
		{
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

			msgCount++;
		}

		public void QueueMurderPlayer(PlayerControl source, PlayerControl target, MurderResultFlags result)
		{
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

			msgCount++;
		}

		public void QueueSendChatNote(PlayerControl source, byte playerId, ChatNoteTypes chatNote)
		{
			if(IsGlobal || AmTarget)
			{
				NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(playerId);
				HudManager.Instance.Chat.AddChatNote(player, chatNote);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.SendChatNote);
			writer.Write(playerId);
			writer.Write((byte)chatNote);
			writer.EndMessage();

			msgCount++;
		}

		public void QueueSnapTo(PlayerControl source, Vector2 position)
		{
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

			msgCount++;
		}

		public void QueueCloseMeeting()
		{
			if(IsGlobal || AmTarget)
			{
				MeetingHud.Instance.Close();
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(MeetingHud.Instance.NetId);
			writer.Write((byte)RpcCalls.CloseMeeting);
			writer.EndMessage();

			msgCount++;
		}

		public void QueueVotingComplete(MeetingHud.VoterState[] voteStates, NetworkedPlayerInfo ejectedPlayer, bool isTie, bool wasOverruled, ushort overruleNonce)
		{
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

			msgCount++;
		}

		public void QueueAddVote(int sourceId, int targetId)
		{
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

			msgCount++;
		}

		public void QueueCloseDoors(SystemTypes door)
		{
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

			msgCount++;
		}

		public void QueueUpdateSystem(PlayerControl source, SystemTypes system, byte value)
		{
			if(IsGlobal || AmTarget)
			{
				ShipStatus.Instance.UpdateSystem(system, source, value);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(ShipStatus.Instance.NetId);
			writer.Write((byte)RpcCalls.UpdateSystem);
			writer.Write((byte)system);
			writer.WriteNetObject(source);
			writer.Write(value);
			writer.EndMessage();

			msgCount++;
		}

		public void QueueUpdateSystem(PlayerControl source, SystemTypes system, MessageWriter msg)
		{
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

			msgCount++;
		}

		public void QueueSetHatStr(PlayerControl source, string hat, byte seqId)
		{
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

			msgCount++;
		}

		public void QueueSetSkinStr(PlayerControl source, string skin, byte seqId)
		{
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

			msgCount++;
		}

		public void QueueSetPetStr(PlayerControl source, string pet, byte seqId)
		{
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

			msgCount++;
		}

		public void QueueSetVisorStr(PlayerControl source, string visor, byte seqId)
		{
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

			msgCount++;
		}

		public void QueueSetNameplateStr(PlayerControl source, string nameplate, byte seqId)
		{
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

			msgCount++;
		}

		public void QueueSetRole(PlayerControl source, RoleTypes role, bool canOverride = false)
		{
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

			msgCount++;
		}

		public void QueueShapeshift(PlayerControl source, PlayerControl target, bool shouldAnimate)
		{
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

			msgCount++;
		}

		public void QueueUseZipline(PlayerControl source, ZiplineBehaviour zipline, bool fromTop)
		{
			if(IsGlobal || AmTarget)
			{
				zipline.Use(source, fromTop);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.UseZipline);
			writer.Write(fromTop);
			writer.EndMessage();

			msgCount++;
		}

		public void QueueTriggerSpore(PlayerControl source, Mushroom mushroom)
		{
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

			msgCount++;
		}

		public void QueueVanish(PlayerControl source)
		{
			if(IsGlobal || AmTarget)
			{
				source.SetRoleInvisibility(true, true, false);
				source.HandleServerVanish();
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.StartVanish);
			writer.EndMessage();

			msgCount++;
		}

		public void QueueAppear(PlayerControl source, bool shouldAnimate = true)
		{
			if(IsGlobal || AmTarget)
			{
				source.HandleServerAppear(shouldAnimate);
				if(AmTarget) return;
			}

			writer.StartMessage((byte)GameDataTypes.RpcFlag);
			writer.WritePacked(source.NetId);
			writer.Write((byte)RpcCalls.StartAppear);
			writer.Write(shouldAnimate);
			writer.EndMessage();

			msgCount++;
		}

		public void FinishBatch()
		{
			writer.EndMessage();

			int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();
			if(msgCount > packingLimit)
			{
				Lunar.Log.LogWarning($"BatchedMessage has {msgCount} packed messages, which exceeds the packed message limit of {packingLimit}. This may result in anticheat disconnections");
			}

			if(writer.Length > 1201)
			{
				Lunar.Log.LogWarning($"BatchedMessage has a size of {writer.Length} bytes, which exceeds the vanilla limit of 1201 bytes. This may result in anticheat disconnections");
			}

			if(msgCount > 0)
			{
				AmongUsClient.Instance.SendOrDisconnect(writer);
			}
			writer.Recycle();
		}
	}
}