using UnityEngine;

namespace HydraMenu.modules.visuals
{
	internal class SpectatePlayer : Module
	{
		public SpectatePlayer() : base("SpectatePlayer") { }

		public PlayerControl target;
		private bool wereShadowsEnabled = false;

		protected override void OnEnable()
		{
			FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
			camera.SetTarget(target);

			wereShadowsEnabled = HudManager._instance.ShadowQuad.gameObject.active;
			HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
		}

		protected override void OnDisable()
		{
			FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
			camera.SetTarget(PlayerControl.LocalPlayer);

			if(wereShadowsEnabled) HudManager.Instance.ShadowQuad.gameObject.SetActive(true);
		}
	}
}