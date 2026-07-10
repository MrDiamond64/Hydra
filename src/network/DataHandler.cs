using HarmonyLib;
using Hazel;
using HydraMenu.features;
using InnerNet;

namespace HydraMenu.network
{
	internal class DataHandler
	{
		// On Vanilla servers, messages are limited to at most 1201 bytes
		// On Modded Nikocat servers, messages are limited to at most 1400 bytes
		// This value should be 1500 bytes at most, as at that point messages will likely not be recieved by some clients due to exceeding MTU size
		// 1400 bytes seems to be the sweet spot, as it is enough to support modded servers, and also enough to prevent attacks with large messages
		public static int MAX_MESSAGE_LENGTH = 1400;

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleGameData))]
		class HandleGameData
		{
			static bool Prefix(InnerNetClient __instance, MessageReader parentReader)
			{
				if(Protections.BlockLargeGameMessages && parentReader.Length > MAX_MESSAGE_LENGTH)
				{
					parentReader.Recycle();
					return false;
				}

				try
				{
					while(parentReader.BytesRemaining > 0)
					{
						MessageReader reader = parentReader.ReadMessageAsNewBuffer();
						HandleGameDataInner(__instance, reader, ++__instance.msgNum);
					}
				}
				finally
				{
					parentReader.Recycle();
				}

				return false;
			}
		}

		public static void HandleGameDataInner(InnerNetClient innerNetClient, MessageReader reader, int msgNum)
		{
			if(Protections.BlockInvalidGameDataMessages && (reader.Tag == 3 || reader.Tag > 7))
			{
				reader.Recycle();
				return;
			}

			innerNetClient.StartCoroutine(innerNetClient.HandleGameDataInner(reader, msgNum));
		}
	}
}
