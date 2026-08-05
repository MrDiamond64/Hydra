using Hazel;
using UnityEngine;

namespace HydraMenu.routines
{

	public class PetPlayerRoutine : IRoutine
	{
		public PetPlayerRoutine() : base("PetPlayer") { }

		public readonly float TARGET_PET_DELAY = 0.60f;
		public readonly float MANUAL_RPC_DELAY = 0.10f;

		public PlayerControl target;
		private float timeElapsed = 0.0f;

		public bool manualControl = false;
		public Vector2 handOffset = Vector2.zero;
		public Vector2 joystickVector = Vector2.zero;
		public float speed = 5.0f;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null) return;

			Vector2 petPosition;
			if(manualControl)
			{
				if(PlayerControl.LocalPlayer != null)
				{
					PlayerControl.LocalPlayer.moveable = true;
				}

				// Smooth 60FPS continuous hand movement driven by on-screen joystick
				handOffset += joystickVector * speed * Time.deltaTime;
				petPosition = (Vector2)PlayerControl.LocalPlayer.transform.position + handOffset;
			}
			else
			{
				if(PlayerControl.LocalPlayer != null)
				{
					PlayerControl.LocalPlayer.moveable = false;
					if(PlayerControl.LocalPlayer.NetTransform != null && PlayerControl.LocalPlayer.NetTransform.body != null)
					{
						PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;
					}
				}

				if(target == null) return;
				petPosition = target.transform.position;
				petPosition.y -= PlayerControl.LocalPlayer.cosmetics.currentPet.yOffset * 2;
			}

			// Update local visual position every frame for ultra smooth 60+ FPS motion
			if(PlayerControl.LocalPlayer.cosmetics != null && PlayerControl.LocalPlayer.cosmetics.CurrentPet != null)
			{
				PlayerControl.LocalPlayer.cosmetics.CurrentPet.SetGettingPet(true, petPosition);
			}

			// Decouple RPC network send rate and animation trigger so the hand animation isn't restarted every frame
			timeElapsed += Time.deltaTime;
			float rpcDelay = manualControl ? MANUAL_RPC_DELAY : TARGET_PET_DELAY;
			if(timeElapsed < rpcDelay) return;
			timeElapsed = 0.0f;

			if(PlayerControl.LocalPlayer.cosmetics != null && PlayerControl.LocalPlayer.cosmetics.PettingHand != null)
			{
				PlayerControl.LocalPlayer.cosmetics.PettingHand.StartPet(PlayerControl.LocalPlayer.cosmetics.currentPet);
			}

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.MyPhysics.NetId,
				(byte)RpcCalls.Pet,
				SendOption.Reliable,
				-1
			);

			NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), writer);
			NetHelpers.WriteVector2(petPosition, writer);

			AmongUsClient.Instance.FinishRpcImmediately(writer);
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer != null)
			{
				if(!manualControl)
				{
					// Targeted Petting: Freeze player movement so petting target works cleanly
					PlayerControl.LocalPlayer.moveable = false;
					if(PlayerControl.LocalPlayer.NetTransform != null && PlayerControl.LocalPlayer.NetTransform.body != null)
					{
						PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;
					}
				}
				else
				{
					// Manual Hand Control: Allow normal player walking
					PlayerControl.LocalPlayer.moveable = true;
					handOffset = Vector2.zero;
				}
			}
		}

		protected override void OnDisable()
		{
			target = null;
			manualControl = false;
			handOffset = Vector2.zero;

			if(PlayerControl.LocalPlayer != null)
			{
				PlayerControl.LocalPlayer.moveable = true;
				if(PlayerControl.LocalPlayer.cosmetics != null)
				{
					if(PlayerControl.LocalPlayer.cosmetics.PettingHand != null)
					{
						PlayerControl.LocalPlayer.cosmetics.PettingHand.StopPetting();
					}
					if(PlayerControl.LocalPlayer.cosmetics.CurrentPet != null)
					{
						PlayerControl.LocalPlayer.cosmetics.CurrentPet.SetGettingPet(false, Vector2.zero);
					}
				}
			}
		}

		public override void OnDisconnect()
		{
			Hydra.notifications.Send("Pet Player", "Pet Player was disabled as you left the game.", 10);
			Enabled = false;
		}
	}
}