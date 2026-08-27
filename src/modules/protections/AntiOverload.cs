using HarmonyLib;
using Hazel;

namespace HydraMenu.modules.protections
{
	internal class AntiOverload : Module
	{
		public AntiOverload() : base("AntiOverload")
		{
			base.Enabled = true;
		}

		private static AntiOverload Instance
		{
			get { return ModuleManager.antiOverload; }
		}

		public bool BlockLargeGameMessages { get; set; } = true;
		public bool BlockInvalidGameDataMessages { get; set; } = true;
		public bool HardenedPacketUIntDeserializer { get; set; } = true;
		public bool BlockVotingCompleteOverload { get; set; } = true;

		[HarmonyPatch(typeof(MessageReader), nameof(MessageReader.ReadPackedUInt32))]
		class HardenedReadPackedUInt
		{
			static bool Prefix(MessageReader __instance, ref uint __result)
			{
				if(!Instance.Enabled || !Instance.HardenedPacketUIntDeserializer) return true;

				bool readMore = true;
				int shift = 0;
				uint output = 0;

				while(readMore)
				{
					if(__instance.BytesRemaining < 1) break;

					byte b = __instance.ReadByte();
					if(b >= 0x80)
					{
						readMore = true;
						b ^= 0x80;
					}
					else
					{
						readMore = false;
					}

					output |= (uint)(b << shift);
					shift += 7;
				}

				__result = output;
				return false;
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleRpc))]
		class MemoryAllocationOverload
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				if(!Instance.Enabled || !Instance.BlockVotingCompleteOverload || callId != (byte)RpcCalls.VotingComplete) return true;

				int oldReadPosition = reader.Position;

				// The game creates an array with the size of the following value
				// If this value is very large, then the client will attempt to allocate several gigabytes of memory
				int arrayLength = reader.ReadPackedInt32();

				if(arrayLength > 1024 || arrayLength > reader.BytesRemaining)
				{
					return false;
				}

				reader.Position = oldReadPosition;
				return true;
			}
		}
	}
}