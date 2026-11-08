using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using HydraMenu.network;
using System.Collections;
using UnityEngine;

namespace HydraMenu.modules.roles
{
	internal class NoPhantomAnimation : Module
	{
		public NoPhantomAnimation() : base("NoPhantomAnimation") { }

		private static NoPhantomAnimation Instance
		{
			get { return ModuleManager.noPhantomAnimation; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
		class CheckVanish
		{
			static void Prefix(PlayerControl __instance)
			{
				if(!Instance.Enabled || __instance != PlayerControl.LocalPlayer) return;

				PlayerControl.LocalPlayer.StartCoroutine(HideVanishAnimation().WrapToIl2Cpp());
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
		class CheckAppear
		{
			static void Prefix(PlayerControl __instance, ref bool shouldAnimate)
			{
				if(!Instance.Enabled || __instance != PlayerControl.LocalPlayer) return;

				shouldAnimate = false;

				// If fading is true then UI buttons such as the Kill or Phantom buttons will be greyed out
				// If shouldAnimate is false then fading will be set to true, and remain true, meaning that none of the UI buttons will work
				// so we will have to manually set it to false
				PhantomRole role = PlayerControl.LocalPlayer.Data.Role.TryCast<PhantomRole>();
				if(role != null)
				{
					role.fading = false;
				}
			}
		}

		// Unlike the CheckAppear/Appear RPC, there is no field to toggle the Vanish animation
		// so instead we teleport our player to the void for the duration of the animation so that nobody can see the animation
		public static IEnumerator HideVanishAnimation()
		{
			PhantomRole role = PlayerControl.LocalPlayer.Data.Role.TryCast<PhantomRole>();
			if(role == null)
			{
				Hydra.Log.LogWarning("Our player started the phantom animation when our role is not Phantom");
				yield break;
			}

			// We need to pause our NetTransform or our player will end up back to our old position once we move
			Hydra.Log.LogInfo($"Phantom vanish animation started, teleporting ourself to the void");
			PlayerControl.LocalPlayer.NetTransform.SetPaused(true);
			PlayerControl.LocalPlayer.NetTransform.sendQueue.Clear();
			PlayerControl.LocalPlayer.NetTransform.ClearDirtyBits();

			BatchedMessage batch = new BatchedMessage();
			batch.skipLocalHandling = true;

			batch.QueueSnapTo(PlayerControl.LocalPlayer, new Vector2(999, 999));
			batch.FinishBatch();

			// Wait for the phantom vanish animation to end
			// We have to use a hardcoded time here to account for ping differences, and for the little animation at the end of the vanish poof
			yield return Effects.Wait(0.75f);

			Hydra.Log.LogInfo("Phantom vanish animation ended, unpausing our CustomNetworkTransform");
			PlayerControl.LocalPlayer.NetTransform.SetPaused(false);
		}
	}
}