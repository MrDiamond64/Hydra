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

			public void QueueDataFlag(InnerNetObject netObj)
			{
				writer.StartMessage((byte)GameDataTypes.DataFlag);
				writer.WritePacked(netObj.NetId);
				netObj.Serialize(writer, false);
				writer.EndMessage();
			}

			public void QueueDataFlag(uint netId, MessageWriter msg)
			{
				writer.StartMessage((byte)GameDataTypes.DataFlag);
				writer.WritePacked(netId);
				writer.Write(msg, false);
				writer.EndMessage();
			}

			public void QueueDespawn(uint netId)
			{
				writer.StartMessage((byte)GameDataTypes.DespawnFlag);
				writer.WritePacked(netId);
				writer.EndMessage();
			}

			public void QueueSceneChange(int clientId, string scene)
			{
				writer.StartMessage((byte)GameDataTypes.SceneChangeFlag);
				writer.WritePacked(clientId);
				writer.Write(scene);
				writer.EndMessage();
			}

			public void QueueCompleteTask(PlayerControl source, byte taskId)
			{
				source.CompleteTask(taskId);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.CompleteTask);
				writer.Write(taskId);
				writer.EndMessage();
			}

			public void QueueCheckName(PlayerControl source, string name)
			{
				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.CheckName);
				writer.Write(name);
				writer.EndMessage();
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

			public void QueueSendChat(PlayerControl source, string text)
			{
				if(HudManager.Instance != null)
				{
					HudManager.Instance.Chat.AddChat(source, text, true);
				}

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SendChat);
				writer.Write(text);
				writer.EndMessage();
			}

			public void QueueSetScanner(PlayerControl source, bool scanning, byte seq)
			{
				source.SetScanner(scanning, seq);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetScanner);
				writer.Write(scanning);
				writer.Write(seq);
				writer.EndMessage();
			}

			public void QueueSetStartCounter(PlayerControl source, sbyte counter, int seq)
			{
				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetStartCounter);
				writer.WritePacked(seq);
				writer.Write(counter);
				writer.EndMessage();
			}

			public void QueueSnapTo(PlayerControl source, ushort seq, Vector2 position)
			{
				source.NetTransform.SnapTo(position, seq);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetTransform.NetId);
				writer.Write((byte)RpcCalls.SnapTo);
				NetHelpers.WriteVector2(position, writer);
				writer.Write(seq);
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

			public void QueueSetLevel(PlayerControl source, uint level)
			{
				source.SetLevel(level);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.SetLevel);
				writer.WritePacked(level);
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

			public void QueueCheckMurder(PlayerControl source, PlayerControl target)
			{
				if(AmongUsClient.Instance.AmHost)
				{
					source.CheckMurder(target);
				}

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(source.NetId);
				writer.Write((byte)RpcCalls.CheckMurder);
				writer.WriteNetObject(target);
				writer.EndMessage();
			}

			public void QueueLobbyTimeExpiring(int timer)
			{
				LobbyBehaviour.Instance.HandleLobbyTimerExtensionRequest(69420, false, 255, 0, 0);

				writer.StartMessage((byte)GameDataTypes.RpcFlag);
				writer.WritePacked(LobbyBehaviour.Instance.NetId);
				writer.Write((byte)RpcCalls.LobbyTimeExpiring);
				writer.WritePacked(timer);
				writer.Write(false);
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