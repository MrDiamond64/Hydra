using HarmonyLib;
using HydraMenu.network;

namespace HydraMenu.modules.roles
{
	internal class FakeShapeshiftBubble : Module
	{
		public FakeShapeshiftBubble() : base("FakeShapeshiftBubble", [ typeof(NoShapeshiftAnimation) ]) { }

		private bool awaitingResponse = false;
		private PlayerControl _shapeshiftTarget;
		private byte _originalColor = 0;

		private static FakeShapeshiftBubble Instance
		{
			get { return ModuleManager.fakeShapeshiftBubble; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
		class OnShapeshift
		{
			static bool Prefix(PlayerControl __instance, PlayerControl target, bool shouldAnimate)
			{
				if(!Instance.Enabled || !shouldAnimate || __instance != PlayerControl.LocalPlayer) return true;

				SendSpoofedShapeshift(__instance, target);
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
		class OnRevertShapeshift
		{
			static bool Prefix(PlayerControl __instance, bool shouldAnimate)
			{
				if(!Instance.Enabled || !shouldAnimate || __instance != PlayerControl.LocalPlayer) return true;

				SendSpoofedShapeshift(__instance, __instance);
				return false;
			}
		}

		private static void SendSpoofedShapeshift(PlayerControl shapeshifter, PlayerControl target)
		{
			int originalColor = shapeshifter.Data.DefaultOutfit.ColorId;
			int newColor = Utilities.GetRandomUnusedColor();

			if(!Utilities.IsAnticheatPresent() || AmongUsClient.Instance.AmHost)
			{
				Instance.awaitingResponse = false;

				BatchedMessage batch = new BatchedMessage();
				batch.QueueSetColor(shapeshifter, (byte)newColor);
				batch.QueueShapeshift(shapeshifter, target, true);
				batch.QueueSetColor(shapeshifter, (byte)originalColor);
				batch.FinishBatch();
			}
			else
			{
				Instance.awaitingResponse = true;
				Instance._shapeshiftTarget = target;
				Instance._originalColor = (byte)originalColor;

				PlayerControl.LocalPlayer.CmdCheckColor((byte)newColor);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetColor))]
		class PlayerUpdateColor
		{
			static void Postfix(PlayerControl __instance)
			{
				if(!Instance.awaitingResponse || __instance != PlayerControl.LocalPlayer) return;
				Instance.awaitingResponse = false;

				PlayerControl.LocalPlayer.RpcShapeshift(Instance._shapeshiftTarget, true);
				PlayerControl.LocalPlayer.CmdCheckColor(Instance._originalColor);
			}
		}
	}
}