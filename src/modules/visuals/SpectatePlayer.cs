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
			// In case this module was enabled as part of the config
			if(target == null)
			{
				_enabled = false;
				return;
			}

			FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
			camera.SetTarget(target);

			wereShadowsEnabled = HudManager.Instance.ShadowQuad.gameObject.active;
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