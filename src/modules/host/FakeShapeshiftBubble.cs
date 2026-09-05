using HarmonyLib;
using LunarMenu.network;

namespace LunarMenu.modules.host
{
	internal class FakeShapeshiftBubble : Module
	{
		public FakeShapeshiftBubble() : base("FakeShapeshiftBubble") { }

		private static FakeShapeshiftBubble Instance
		{
			get { return ModuleManager.fakeShapeshiftBubble; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
		class OnShapeshift
		{
			static bool Prefix(PlayerControl __instance, PlayerControl target, bool shouldAnimate)
			{
				if(!Instance.Enabled || !shouldAnimate || __instance != PlayerControl.LocalPlayer || (!AmongUsClient.Instance.AmHost && Utilities.IsAnticheatPresent())) return true;

				SendSpoofedShapeshift(__instance, target);
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
		class OnRevertShapeshift
		{
			static bool Prefix(PlayerControl __instance, bool shouldAnimate)
			{
				if(!Instance.Enabled || !shouldAnimate || __instance != PlayerControl.LocalPlayer || (!AmongUsClient.Instance.AmHost && Utilities.IsAnticheatPresent())) return true;

				SendSpoofedShapeshift(__instance, __instance);
				return false;
			}
		}

		private static void SendSpoofedShapeshift(PlayerControl shapeshifter, PlayerControl target)
		{
			int originalColor = shapeshifter.Data.DefaultOutfit.ColorId;
			int newColor = Utilities.GetRandomUnusedColor();

			BatchedMessage batch = new BatchedMessage();
			batch.QueueSetColor(shapeshifter, (byte)newColor);
			batch.QueueShapeshift(shapeshifter, target, true);
			batch.QueueSetColor(shapeshifter, (byte)originalColor);
			batch.FinishBatch();
		}
	}
}