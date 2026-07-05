using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;
using UnityEngine;

namespace HydraMenu
{
	internal class Network
	{
		// The PlayerControl::RpcSetScanner function does not send the RPC if visual tasks are off
		// If we want the scan animation to show up even if visual tasks are enabled, then we will need to reimplement it
		public static void SendSetScanner(bool scanning)
		{
			byte scanCount = ++PlayerControl.LocalPlayer.scannerCount;

			// Render the medbay animation for ourselves
			PlayerControl.LocalPlayer.SetScanner(scanning, scanCount);

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.NetId,
				(byte)RpcCalls.SetScanner,
				SendOption.Reliable,
				-1
			);

			writer.Write(scanning);
			writer.Write(scanCount);

			AmongUsClient.Instance.FinishRpcImmediately(writer);
		}

		// The PlayerControl::RpcPlayAnimation function does not send the RPC if visual tasks are off
		// If we want the task animation to show up even if visual tasks are enabled, then we will need to reimplement it
		public static void SendPlayAnimation(byte animation)
		{
			// Render the task animation for ourselves
			PlayerControl.LocalPlayer.PlayAnimation(animation);

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.NetId,
				(byte)RpcCalls.PlayAnimation,
				SendOption.None,
				-1
			);

			writer.Write(animation);

			AmongUsClient.Instance.FinishRpcImmediately(writer);
		}

		public static void SendDataFlag(uint netId, MessageWriter msg, int targetClientId = -1)
		{
			BatchedMessage batch = new BatchedMessage(targetClientId);
			batch.QueueDataFlag(netId, msg);
			batch.FinishBatch();
		}

		public class BatchedMessage
		{
			public MessageWriter writer;

			public BatchedMessage(int targetClientId = -1)
			{
				writer = MessageWriter.Get(SendOption.Reliable);

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
				source.SetName(name);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetName);
				writer.Write(source.NetId);
				writer.Write(name);
				writer.EndMessage();
			}

			public void QueueSetColor(PlayerControl source, byte color)
			{
				source.SetColor(color);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetColor);
				writer.Write(source.Data.NetId);
				writer.Write(color);
				writer.EndMessage();
			}

			public void QueueReportDeadBody(PlayerControl source, NetworkedPlayerInfo target)
			{
				if(AmongUsClient.Instance.AmHost)
				{
					source.ReportDeadBody(target);
					return;
				}

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.ReportDeadBody);
				writer.Write(target != null ? target.PlayerId : 255);
				writer.EndMessage();
			}

			public void QueueMurderPlayer(PlayerControl source, PlayerControl target, MurderResultFlags result)
			{
				source.MurderPlayer(target, result);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.MurderPlayer);
				writer.WritePacked(target.NetId);
				writer.Write((int)result);
				writer.EndMessage();
			}

			public void QueueSnapTo(PlayerControl source, Vector2 position)
			{
				source.NetTransform.SnapTo(position, (ushort)(source.NetTransform.lastSequenceId + 1));

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
				MeetingHud.Instance.Close();

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(MeetingHud.Instance.NetId);
				writer.Write((byte)RpcCalls.CloseMeeting);
				writer.EndMessage();
			}

			public void QueueVotingComplete(MeetingHud.VoterState[] voteStates, NetworkedPlayerInfo ejectedPlayer, bool isTie)
			{
				MeetingHud.Instance.VotingComplete(voteStates, ejectedPlayer, isTie);

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
				VoteBanSystem.Instance.AddVote(sourceId, targetId);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(VoteBanSystem.Instance.NetId);
				writer.Write((byte)RpcCalls.AddVote);
				writer.Write(sourceId);
				writer.Write(targetId);
				writer.EndMessage();
			}

			public void QueueSetTasks(NetworkedPlayerInfo player, byte[] tasks)
			{
				player.SetTasks(tasks);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(player.NetId);
				writer.Write((byte)RpcCalls.SetTasks);
				writer.WriteBytesAndSize(tasks);
				writer.EndMessage();
			}

			public void QueueSetHatStr(PlayerControl source, string hat, byte seqid)
			{
				source.SetHat(hat, source.Data.DefaultOutfit.ColorId);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetHatStr);
				writer.Write(hat);
				writer.Write(seqid);
				writer.EndMessage();
			}

			public void QueueSetSkinStr(PlayerControl source, string skin, byte seqid)
			{
				source.SetSkin(skin, source.Data.DefaultOutfit.ColorId);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetSkinStr);
				writer.Write(skin);
				writer.Write(seqid);
				writer.EndMessage();
			}

			public void QueueSetPetStr(PlayerControl source, string pet, byte seqid)
			{
				source.SetPet(pet, source.Data.DefaultOutfit.ColorId);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetPetStr);
				writer.Write(pet);
				writer.Write(seqid);
				writer.EndMessage();
			}

			public void QueueSetVisorStr(PlayerControl source, string visor, byte seqid)
			{
				source.SetVisor(visor, source.Data.DefaultOutfit.ColorId);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetVisorStr);
				writer.Write(visor);
				writer.Write(seqid);
				writer.EndMessage();
			}

			public void QueueSetNameplateStr(PlayerControl source, string nameplate, byte seqid)
			{
				source.SetVisor(nameplate, source.Data.DefaultOutfit.ColorId);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetNamePlateStr);
				writer.Write(nameplate);
				writer.Write(seqid);
				writer.EndMessage();
			}

			public void QueueSetRole(PlayerControl source, RoleTypes role, bool canOverride = false)
			{
				source.StartCoroutine(source.CoSetRole(role, canOverride));

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetRole);
				writer.Write((ushort)role);
				writer.Write(canOverride);
				writer.EndMessage();
			}

			public void QueueShapeshift(PlayerControl source, PlayerControl target, bool shouldAnimate)
			{
				source.Shapeshift(target, shouldAnimate);

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
}