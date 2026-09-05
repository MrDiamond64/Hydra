using HarmonyLib;
using Hazel;
using LunarMenu.network;

namespace LunarMenu.modules.troll
{
	internal class DisableCloseDoors : Module
	{
		public DisableCloseDoors() : base("DisableCloseDoors") { }

		private static DisableCloseDoors Instance
		{
			get { return ModuleManager.disableCloseDoors; }
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
		class OnCloseDoor
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}

		// While we cant block doors from actually being closed as non-host, we can just fix the doors as soon as they get locked
		[HarmonyPatch(typeof(DoorsSystemType), nameof(DoorsSystemType.Deserialize))]
		class DoorsDeserialize
		{
			static void Prefix(MessageReader reader)
			{
				if(!Instance.Enabled || AmongUsClient.Instance.AmHost) return;

				int oldReadPosition = reader.Position;

				int systems = (int)reader.ReadByte();
				if(systems > ShipStatus.Instance.Systems.Count || systems > reader.BytesRemaining) goto end;

				// One byte identifier for the systemtype, and a float (four bytes) for system cooldown
				reader.Position += systems * (1 + 4);

				int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();
				BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);

				for(int i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
				{
					bool isOpen = reader.ReadBoolean();
					if(isOpen) continue;

					if(batch.msgCount >= packingLimit)
					{
						batch.FinishBatch();
						batch = new BatchedMessage(AmongUsClient.Instance.HostId);
					}

					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Doors, (byte)(i | 64));
				}

				batch.FinishBatch();

				end:
				reader.Position = oldReadPosition;
			}
		}

		protected override void OnEnable()
		{
			if(!Sabotage.CanUnlockDoors())
			{
				Lunar.notifications.Send("Disable Close Doors", "Disable Close Doors only works if you are the host of the lobby, or you are playing on Polus, Airship, or The Fungle.");
			}
		}
	}
}